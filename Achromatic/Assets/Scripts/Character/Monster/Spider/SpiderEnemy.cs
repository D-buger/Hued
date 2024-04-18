using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Spine.Unity;
using Newtonsoft.Json.Linq;
using TextAsset = UnityEngine.TextAsset;
using static UnityEngine.Rendering.DebugUI;
using System.Collections.Generic;

public class SpiderEnemy : Monster, IAttack
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
    private EanimState animState;

    private string currentAnimation;

    [SerializeField, Tooltip("몬스터 기준 이동 범위")]
    private float runPosition;
    private float elapsedTime = 0;
    private float arrivalThreshold = 1f;
    private float distanceToPlayer = 0;
    private float angleThreshold = 52f;


    public ParticleSystem[] earthParticleGroup;

    public UnityEvent<eActivableColor> spyderColorEvent;

    [SerializeField]
    private GameObject[] earthObjects;


    private Vector2 leftPosition;
    private Vector2 rightPosition;
    private Vector2 thisPosition;
    private Vector2 startSpiderPosition;

    private Vector3 gizmoLeftPos;
    private Vector3 gizmoRightPos;

    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;
    private LayerMask originLayer;
    private LayerMask colorVisibleLayer;

    private JObject jsonObject;

    private bool isBattle = false;
    private bool canAttack = true;
    private bool isAttack = false;
    private bool isFirstAttack = true;
    private bool isPlayerBetween = false;
    private bool isEarthAttack = false;
    private bool isHeavy = false;
    private bool isGameStart = true;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        attackPoint = transform.GetChild(0).gameObject;
        meleeAttack = attackPoint.GetComponentInChildren<Attack>();
    }
    private void Start()
    {
        startSpiderPosition = new Vector3(transform.position.x, transform.position.y, 0);
        gizmoLeftPos = new Vector3(transform.position.x + runPosition, transform.position.y);
        gizmoRightPos = new Vector3(transform.position.x - runPosition, transform.position.y);

        leftPosition.y = transform.position.y;
        rightPosition.y = transform.position.y;
        leftPosition.x += transform.position.x + runPosition;
        rightPosition.x += transform.position.x - runPosition;

        currentHP = stat.MonsterHP;
        thisPosition = rightPosition;
        originLayer = gameObject.layer;
        colorVisibleLayer = LayerMask.NameToLayer("ColorEnemy");
        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, stat.enemyColor, null);
        MonsterManager.Instance?.getColorEvent.AddListener(CheckIsHeavy);

        PlayManager.Instance.FilterColorAttackEvent.AddListener(IsActiveColor);
        PlayManager.Instance.UpdateColorthing();

        angleThreshold += transform.position.y;
        if (animationJson != null)
        {
            jsonObject = JObject.Parse(animationJson.text);
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

    private void Update()
    {
        CheckPlayer();
        if (isWait)
        {
            WaitSituation();
            animState = EanimState.Walk;
        }
        else if (canAttack && isBattle)
        {
            Attack(PlayerPos);
        }

        if (!isPlayerBetween)
        {
            CheckWaitTime();
        }
        SetCurrentAniamtion(animState);
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

    private void CheckPlayer()
    {
        distanceToPlayer = Vector2.Distance(transform.position, PlayerPos);
        float distanceToMonster = Vector2.Distance(startSpiderPosition, PlayerPos);
        if (distanceToMonster <= runPosition)
        {
            isPlayerBetween = true;
            isBattle = true;
            isWait = false;
            elapsedTime = 0f;
        }
        else
        {
            isPlayerBetween = false;
        }
    }

    private void WaitSituation()
    {
        currentHP = stat.MonsterHP;
        isBattle = false;
        animState = EanimState.Walk;
        SetCurrentAniamtion(animState);
        transform.position = Vector2.MoveTowards(transform.position, thisPosition, stat.moveSpeed * Time.deltaTime);
        if (thisPosition == leftPosition)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (thisPosition == rightPosition)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        if (HasArrived((Vector2)transform.position, rightPosition))
        {
            thisPosition = leftPosition;
        }
        if (HasArrived((Vector2)transform.position, leftPosition))
        {
            thisPosition = rightPosition;
        }
    }
    public void CheckWaitTime()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= stat.usualTime)
        {
            elapsedTime = 0f;
            isWait = true;
        }
    }
    private bool HasArrived(Vector2 currentPosition, Vector2 targetPosition)
    {
        return Vector2.Distance(currentPosition, targetPosition) <= arrivalThreshold;
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


    public void Attack(Vector2 vec)
    {
        StartCoroutine(MoveToPlayer());
    }

    IEnumerator AttackSequence(Vector2 attackAngle)
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
        meleeAttack?.AttackDisable();
        yield return Yields.WaitSeconds(stat.attackCooldown);
        isAttack = false;
        canAttack = true;
    }

    private IEnumerator MoveToPlayer()
    {
        while (!isAttack && !isWait)
        {
            yield return new WaitForSeconds(stat.attackCooldown);
            float horizontalValue = PlayerPos.x - transform.position.x;

            if (PlayerPos == null)
            {
                yield break;
            }

            if (horizontalValue >= 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }

            if (distanceToPlayer <= stat.rangedAttackRange && canAttack)
            {
                StartCoroutine(AttackSequence(PlayerPos));
                yield break;
            }
            else if (distanceToPlayer > stat.rangedAttackRange)
            {
                if (!isAttack && canAttack)
                {
                    animState = EanimState.Walk;
                    SetCurrentAniamtion(animState);
                    transform.position = Vector2.MoveTowards(transform.position, PlayerPos, stat.moveSpeed * Time.deltaTime);
                    StartCoroutine(MoveToPlayer());
                }
            }
        }
    }
    public IEnumerator Spit(float horizontalValue, float verticalValue, float ZAngle)
    {
        if (isWait)
        {
            yield break;
        }
        float spitWaitTime = 0.12f;
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
                projectile.Shot(gameObject, transform.position, new Vector2(horizontalValue, verticalValue).normalized,
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

        meleeAttack?.AttackAble(-value, stat.attackDamage);
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
        yield return new WaitForSeconds(stat.earthAttackTime);
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

    public override void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage = 0)
    {
        if (!isHeavyAttack)
        {
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                currentHP -= criticalDamage;
                rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
            }
            else
            {
                currentHP -= damage;
                rigid.AddForce(attackDir * stat.hitReboundPower, ForceMode2D.Impulse);
            }
        }
        else
        {
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                currentHP -= damage;
                rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
            }
        }
        if (!isDead)
        {
            CheckDead();
        }
        if (isDead)
        {
            StartCoroutine(Dead());
        }
    }

    private IEnumerator Dead()
    {
        isDead = false;
        float deathAnimTime = 1.5f;
        skeletonAnimation.state.GetCurrent(0).TimeScale = 0;
        skeletonAnimation.state.GetCurrent(0).TimeScale = 1;
        animState = EanimState.Dead;
        SetCurrentAniamtion(animState);
        yield return new WaitForSeconds(deathAnimTime);
        gameObject.SetActive(false);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            collision.gameObject.GetComponent<Player>().Hit(stat.contactDamage,
                    transform.position - collision.transform.position, false, stat.contactDamage);
        }
    }
    private void OnDrawGizmos()
    {
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
        if (!isGameStart)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x + runPosition, transform.position.y), 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x - runPosition, transform.position.y), 0.5f);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(gizmoLeftPos, 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(gizmoRightPos, 0.5f);
        }
    }
}