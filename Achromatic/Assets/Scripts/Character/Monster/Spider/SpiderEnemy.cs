using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Spine.Unity;
using Newtonsoft.Json.Linq;
using TextAsset = UnityEngine.TextAsset;
using Unity.VisualScripting;
using System;

public class SpiderEnemy : Monster
{
    private MonsterFSM fsm;

    private Rigidbody2D rigid;
    private GameObject attackPoint;
    private Attack meleeAttack;
    [SerializeField]
    private SpiderMonsterStats stat;
    [SerializeField, Space]
    private Projectile rangedAttack;

    [Header("Animation")]
    [SerializeField]
    private SkeletonAnimation skeletonAnimation;
    [SerializeField]
    private AnimationReferenceAsset[] aniClip;
    [SerializeField]
    private TextAsset animationJson;
    public enum EanimState
    {
        Idle,
        Walk,
        Charge,
        Ground,
        Spit,
        Detection,
        Dead
    }
    [SerializeField]
    private EanimState animState;

    private string currentAnimation;
    private float angleThreshold = 52f;


    public ParticleSystem[] earthParticleGroup;

    public UnityEvent<eActivableColor> spyderColorEvent;

    [SerializeField]
    private GameObject[] earthObjects;
    [SerializeField]
    private GameObject attackTransform;

    public Vector3 gizmoLeftPos;
    public Vector3 gizmoRightPos;

    public Vector2 startSpiderPosition;
    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;
    private LayerMask originLayer;
    private LayerMask colorVisibleLayer;

    private JObject jsonObject;

    private bool isAttack = false;
    private bool isFirstAttack = true;
    private bool isEarthAttack = false;
    private bool isHeavy = false;
    private bool isGameStart = true;

    private void Awake()
    {
        fsm = GetComponent<MonsterFSM>();
        rigid = GetComponent<Rigidbody2D>();
        attackPoint = transform.GetChild(0).gameObject;
        meleeAttack = attackPoint.GetComponentInChildren<Attack>();
    }
    private void Start()
    {
        gizmoLeftPos = new Vector3(transform.position.x + runPosition, transform.position.y);
        gizmoRightPos = new Vector3(transform.position.x - runPosition, transform.position.y);

        leftPosition.y = transform.position.y;
        rightPosition.y = transform.position.y;
        leftPosition.x += transform.position.x + runPosition;
        rightPosition.x += transform.position.x - runPosition;
        startSpiderPosition = new Vector2(gizmoLeftPos.x - runPosition, transform.position.y);

        currentHP = stat.MonsterHP;
        thisPosition = rightPosition;
        originLayer = gameObject.layer;
        colorVisibleLayer = LayerMask.NameToLayer("ColorEnemy");
        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, stat.enemyColor);
        MonsterManager.Instance?.getColorEvent.AddListener(CheckIsHeavy);

        PlayManager.Instance.FilterColorAttackEvent.AddListener(IsActiveColor);
        PlayManager.Instance.UpdateColorthing();

        angleThreshold += transform.position.y;
        if (animationJson != null)
        {
            jsonObject = JObject.Parse(animationJson.text);
        }
        CheckStateChange();
    }
    private void Update()
    {
        CheckPlayer(startSpiderPosition);
        if (canAttack && isBattle)
        {
            Attack();
        }
        SetCurrentAniamtion(animState);
    }

    public override void CheckStateChange()
    {
        if (isWait)
        {
            fsm.ChangeState("Idle");
            animState = EanimState.Walk;
            SetCurrentAniamtion(animState);
        }
        if (isPlayerBetween)
        {
            fsm.ChangeState("Chase");
            animState = EanimState.Walk;
            SetCurrentAniamtion(animState);
        }
        if (isBattle)
        {
            fsm.ChangeState("Attack");
        }
    }

    private void CheckIsHeavy(eActivableColor color)
    {
        if (color == stat.enemyColor)
        {
            isHeavy = false;
        }
        spyderColorEvent?.Invoke(color);
    }

    private void AsncAnimation(AnimationReferenceAsset animClip, bool loop, float timeScale)
    {
        if (animClip.name.Equals(currentAnimation))
        {
            return;
        }

        skeletonAnimation.state.SetAnimation(0, animClip, loop).TimeScale = timeScale;
        skeletonAnimation.loop = loop;
        skeletonAnimation.timeScale = timeScale;
        currentAnimation = animClip.name;
    }
    private void SetCurrentAniamtion(EanimState _state)
    {
        float timeScale = 1;
        switch (_state)
        {
            case EanimState.Idle:
                AsncAnimation(aniClip[(int)EanimState.Idle], true, timeScale);
                break;
            case EanimState.Walk:
                AsncAnimation(aniClip[(int)EanimState.Walk], true, timeScale);
                break;
            case EanimState.Charge:
                AsncAnimation(aniClip[(int)EanimState.Charge], false, timeScale);
                break;
            case EanimState.Ground:
                AsncAnimation(aniClip[(int)EanimState.Ground], false, timeScale);
                break;
            case EanimState.Spit:
                AsncAnimation(aniClip[(int)EanimState.Spit], false, timeScale);
                break;
            case EanimState.Detection:
                AsncAnimation(aniClip[(int)EanimState.Detection], false, timeScale);
                break;
            case EanimState.Dead:
                AsncAnimation(aniClip[(int)EanimState.Dead], false, timeScale);
                break;
        }
    }
    private void IsActiveColor(eActivableColor color)
    {
        if (color != stat.enemyColor)
        {
            gameObject.layer = originLayer;
        }
        else
        {
            gameObject.layer = colorVisibleLayer;
        }
    }


    public override void Attack()
    {
        if (canAttack && !isAttack)
        {
            StartCoroutine(AttackSequence(PlayerPos));
        }
        if (Vector2.Distance(transform.position, PlayerPos) >= stat.senseCircle)
        {
            isBattle = false;
            isPlayerBetween = true;
            isWait = false;
        }
    }

    private IEnumerator AttackSequence(Vector2 attackAngle)
    {
        isAttack = true;
        canAttack = false;
        float horizontalValue = attackAngle.x - transform.position.x;
        float verticalValue = attackAngle.y - transform.position.y;
        float angleToPlayer = Mathf.Atan2(attackAngle.y, transform.position.y) * Mathf.Rad2Deg;
        bool facingPlayer = Mathf.Abs(angleToPlayer - transform.eulerAngles.z) < angleThreshold;
        float ZAngle = (Mathf.Atan2(verticalValue, horizontalValue) * Mathf.Rad2Deg);
        Vector2 value = new Vector2(horizontalValue, verticalValue);
        Vector2 check;
        if (value.x <= 0)
        {
            transform.localScale = new Vector2(1, 1);
            check = new Vector2(-1f, 0);
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
            check = new Vector2(1f, 0);
        }
        animState = EanimState.Detection;
        SetCurrentAniamtion(animState);
        yield return Yields.WaitSeconds((float)jsonObject["animations"]["attack/charge_attack"]["events"][1]["time"]);
        if (isFirstAttack)
        {
            StartCoroutine(Spit(horizontalValue, verticalValue, ZAngle));
            isFirstAttack = false;
        }
        else if (distanceToPlayer < stat.meleeAttackRange)
        {
            int randomChance = UnityEngine.Random.Range(1, 100);
            if (facingPlayer && randomChance <= stat.specialAttackPercent)
            {
                StartCoroutine(ChargeAttack(facingPlayer, value, check));
            }
            else
            {
                StartCoroutine(GroundAtack());
            }
        }
        else
        {
            StartCoroutine(Spit(horizontalValue, verticalValue, ZAngle));
        }
        yield return Yields.WaitSeconds(stat.attackTime);
        yield return Yields.WaitSeconds(stat.attackCooldown);
        isAttack = false;
        canAttack = true;
    }

    public IEnumerator Spit(float horizontalValue, float verticalValue, float ZAngle)
    {
        if (isWait)
        {
            yield break;
        }
        float spitWaitTime = 0.2f;
        yield return new WaitForSeconds(spitWaitTime);
        animState = EanimState.Spit;
        SetCurrentAniamtion(animState);
        GameObject projectileObj = ObjectPoolManager.Instance.GetProjectileFromPool();
        if (projectileObj != null)
        {
            projectileObj.SetActive(true);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Shot(gameObject, attackTransform.transform.position, new Vector2(horizontalValue, verticalValue).normalized,
                    stat.rangedAttackRange, stat.rangedAttackSpeed, stat.rangedAttackDamege, isHeavy, ZAngle, eActivableColor.RED);
                projectileObj.transform.position = transform.position;

                PlayManager.Instance.UpdateColorthing();
                projectile.ReturnStart();
                isFirstAttack = false;
            }
        }
    }

    private IEnumerator ChargeAttack(bool facingPlayer, Vector2 value, Vector2 check)
    {
        if (isWait)
        {
            yield break;
        }
        animState = EanimState.Charge;
        SetCurrentAniamtion(animState);
        yield return Yields.WaitSeconds((float)jsonObject["animations"]["attack/charge_attack"]["events"][1]["time"]);

        meleeAttack?.AttackEnable(-value, stat.attackDamage, stat.attackDamage);
        rigid.AddForce(check * stat.specialAttackRound, ForceMode2D.Impulse);
    }
    private IEnumerator GroundAtack()
    {
        if (isWait)
        {
            yield break;
        }
        animState = EanimState.Ground;
        SetCurrentAniamtion(animState);
        yield return Yields.WaitSeconds((float)jsonObject["animations"]["attack/ground_attack"]["events"][0]["time"]);
        rigid.velocity = Vector2.up * stat.earthAttackJump;
        isEarthAttack = true;

        StartCoroutine(SpawnObjects());
    }
    private IEnumerator SpawnObjects()
    {
        if (isWait)
        {
            yield break;
        }
        float delayToAttack = 0.05f;
        float delayToDestory = 0.05f;
        float delayToEarthAttack = 0.6f;
        int objectCount = earthObjects.Length;
        while (isEarthAttack)
        {
            yield return new WaitForSeconds(delayToEarthAttack);

            StartCoroutine(ActivateParticle(earthParticleGroup));

            isEarthAttack = false;

            for (int i = 0; i < objectCount; i += 2)
            {
                ActivateObjects(earthObjects, i, i + 1, true);
                yield return new WaitForSeconds(delayToAttack);
                ActivateObjects(earthObjects, i, i + 1, false);
                yield return new WaitForSeconds(delayToDestory);

            }
        }
    }
    private IEnumerator ActivateParticle(ParticleSystem[] particles)
    {
        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i] != null)
            {
                particles[i].Play();
            }

            if (i % 2 == 1)
            {
                yield return new WaitForSeconds(stat.earthAttackTime);
            }
        }
    }
    private void ActivateObjects(GameObject[] objects, int startIndex, int endIndex, bool isSet)
    {
        for (int i = startIndex; i < endIndex; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(isSet);
            }
        }
    }

    public override void Hit(int damage, int colorDamage, Vector2 attackDir, IParryConditionCheck parryCheck)
    {
        if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
        {
            HPDown(colorDamage);
            rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
        }
        else
        {
            HPDown(damage);
            rigid.AddForce(attackDir * stat.hitReboundPower, ForceMode2D.Impulse);
        }

        if (!isDead)
        {
            CheckDead();
        }
    }

    public override void Dead()
    {
        StartCoroutine(DeadSequence());
    }

    private IEnumerator DeadSequence()
    {
        float deadDelayTime = 1.3f;
        isBattle = false;
        isWait = false;
        isPlayerBetween = false;
        isDead = false;
        StopCoroutine(AttackSequence(PlayerPos));
        skeletonAnimation.state.GetCurrent(0).TimeScale = 0;
        skeletonAnimation.state.GetCurrent(0).TimeScale = 1;
        animState = EanimState.Dead;
        SetCurrentAniamtion(animState);
        yield return new WaitForSeconds(deadDelayTime);
        gameObject.SetActive(false);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            collision.gameObject.GetComponent<Player>().Hit(stat.contactDamage, stat.contactDamage,
                    transform.position - collision.transform.position);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gizmoLeftPos, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gizmoRightPos, 0.5f);
        if (null != stat)
        {
            if (stat.meleeAttackRange >= distanceToPlayer)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawWireSphere(transform.position + transform.forward, stat.senseCircle);
            Gizmos.DrawWireSphere(transform.position + transform.forward, stat.rangedAttackRange);
        }
    }
}