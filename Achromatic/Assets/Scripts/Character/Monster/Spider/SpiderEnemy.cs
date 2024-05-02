using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Spine.Unity;
using Newtonsoft.Json.Linq;
using TextAsset = UnityEngine.TextAsset;
using Unity.VisualScripting;
using System;
using System.Runtime.CompilerServices;

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
    [Flags]
    private enum EMonsterAttackState
    {
        None = 0,
        IsAttack = 1 << 0,
        IsFirstAttack = 1 << 1,
        IsEarthAttack = 1 << 2
    }
    private EMonsterAttackState currentState = EMonsterAttackState.IsFirstAttack;
    [SerializeField]
    private EanimState animState;

    private string currentAnimation;
    private float angleThreshold = 52f;
    private float spitTime = 2.0f;
    private float spitWaitTime = 0.2f;
    private float delayToAttack = 0.05f;
    private float delayToDestory = 0.05f;
    private float delayToEarthAttack = 0.6f;
    private float deadDelayTime = 1.3f;


    public ParticleSystem[] earthParticleGroup;

    public UnityEvent<eActivableColor> SpIderColorEvent;

    [SerializeField]
    private GameObject[] earthObjects;
    [SerializeField]
    private GameObject attackTransform;

    public Vector3 gizmoLeftPos;

    public Vector2 startSpiderPosition;
    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;
    private LayerMask originLayer;
    private LayerMask colorVisibleLayer;

    private JObject jsonObject;

    private bool isHeavy = false;

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

        monsterRunleftPosition.y = transform.position.y;
        monsterRunRightPosition.y = transform.position.y;
        monsterRunleftPosition.x += transform.position.x + runPosition;
        monsterRunRightPosition.x += transform.position.x - runPosition;
        startSpiderPosition = new Vector2(gizmoLeftPos.x - runPosition, transform.position.y);

        currentHP = stat.MonsterHP;
        monsterPosition = monsterRunRightPosition;
        originLayer = gameObject.layer;
        colorVisibleLayer = LayerMask.NameToLayer("ColorEnemy");
        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, stat.enemyColor);
        MonsterManager.Instance?.GetColorEvent.AddListener(CheckIsHeavy);

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
        if (canAttack && IsStateActive(EMonsterState.isBattle))
        {
            Attack();
        }
        ///<summary>  넣을지 말지 미확정 ///</summary>
        /*if (isChase)
        {
            MovePetton();
        }*/
        SetCurrentAnimation(animState);
    }

    public override void CheckStateChange()
    {
        switch (state)
        {
            case EMonsterState.isBattle:
                fsm.ChangeState("Attack");
                break;
            case EMonsterState.isPlayerBetween:
                fsm.ChangeState("Chase");
                animState = EanimState.Walk;
                SetCurrentAnimation(animState);
                break;
            case EMonsterState.isWait:
                fsm.ChangeState("Idle");
                animState = EanimState.Walk;
                SetCurrentAnimation(animState);
                break;
            default:
                break;
        }
    }

    private void CheckIsHeavy(eActivableColor color)
    {
        if (color == stat.enemyColor)
        {
            isHeavy = true;
        }
        SpIderColorEvent?.Invoke(color);
    }

    private void AsyncAnimation(AnimationReferenceAsset animClip, bool loop, float timeScale)
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
    private void SetCurrentAnimation(EanimState _state)
    {
        float timeScale = 1;
        switch (_state)
        {
            case EanimState.Idle:
                AsyncAnimation(aniClip[(int)EanimState.Idle], true, timeScale);
                break;
            case EanimState.Walk:
                AsyncAnimation(aniClip[(int)EanimState.Walk], true, timeScale);
                break;
            case EanimState.Charge:
                AsyncAnimation(aniClip[(int)EanimState.Charge], false, timeScale);
                break;
            case EanimState.Ground:
                AsyncAnimation(aniClip[(int)EanimState.Ground], false, timeScale);
                break;
            case EanimState.Spit:
                AsyncAnimation(aniClip[(int)EanimState.Spit], false, timeScale);
                break;
            case EanimState.Detection:
                AsyncAnimation(aniClip[(int)EanimState.Detection], false, timeScale);
                break;
            case EanimState.Dead:
                AsyncAnimation(aniClip[(int)EanimState.Dead], false, timeScale);
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
        gameObject.layer = (color != stat.enemyColor) ? originLayer : colorVisibleLayer;
    }


    public override void Attack()
    {
        if (Vector2.Distance(transform.position, PlayerPos) >= stat.senseCircle)
        {
            SetState(EMonsterState.isBattle, false);
            SetState(EMonsterState.isPlayerBetween, true);
            SetState(EMonsterState.isWait, false);
        }
        else if (canAttack && !currentState.HasFlag(EMonsterAttackState.IsAttack))
        {
            StartCoroutine(AttackSequence(PlayerPos));
        }
    }

    private IEnumerator AttackSequence(Vector2 attackAngle)
    {
        currentState |= EMonsterAttackState.IsAttack;
        canAttack = false;
        float angleToPlayer = Mathf.Atan2(attackAngle.y, transform.position.y) * Mathf.Rad2Deg;
        bool facingPlayer = Mathf.Abs(angleToPlayer - transform.eulerAngles.z) < angleThreshold;
        float ZAngle = (Mathf.Atan2(attackAngle.y - transform.position.y, attackAngle.x - transform.position.x) * Mathf.Rad2Deg);
        Vector2 value = new Vector2(attackAngle.x - transform.position.x, attackAngle.y - transform.position.y);
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
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds((float)jsonObject["animations"]["attack/charge_attack"]["events"][1]["time"]);
        if (currentState.HasFlag(EMonsterAttackState.IsFirstAttack))
        {
            StartCoroutine(Spit(value, ZAngle));
            currentState &= ~EMonsterAttackState.IsFirstAttack;
        }
        else if (distanceToPlayer < stat.meleeAttackRange)
        {
            int checkRandomAttackType = UnityEngine.Random.Range(1, 100);
            if (facingPlayer && checkRandomAttackType >= stat.specialAttackPercent)
            {
                StartCoroutine(ChargeAttack(value, check));
            }
            else
            {
                StartCoroutine(EarthAttack());
            }
        }
        else
        {
            int checkRandomAttackType = UnityEngine.Random.Range(1, 100);
            if (checkRandomAttackType <= stat.rangeAttackPercent)
            {
                StartCoroutine(Spit(value, ZAngle));
            }
            else
            {
                StartCoroutine(CompositeAttack(check));
                yield return Yields.WaitSeconds(4.0f);
            }

            ///<summary>  넣을지 말지 미확정 ///</summary>
            /*else
            {
                isChase = true;
                SetState(EMonsterState.isBattle, false);
                SetState(EMonsterState.isPlayerBetween, false);
                SetState(EMonsterState.isWait, false);
                animState = EanimState.Walk;
                SetCurrentAnimation(animState);
                yield return Yields.WaitSeconds(1.5f);
                if (IsStateActive(EMonsterState.isBattle))
                {
                    isChase = false;
                    int randomGage = UnityEngine.Random.Range(1, 100);
                    if (randomGage <= stat.specialAttackPercent)
                    {
                        StartCoroutine(EarthAttack());
                    }
                    else
                    {
                        StartCoroutine(ChargeAttack(value, check));
                    }
                }
            }*/

        }
        yield return Yields.WaitSeconds(stat.attackTime);
        currentState &= ~EMonsterAttackState.IsAttack;
        yield return Yields.WaitSeconds(stat.attackCooldown);
        canAttack = true;
    }

    /* private void MovePetton()
    {
        transform.position = Vector2.MoveTowards(transform.position, PlayerPos, stat.moveSpeed * Time.deltaTime);
        if (distanceToPlayer >= stat.meleeAttackRange)
        {
            SetState(EMonsterState.isBattle, true);
            isChase = false;
        }
        
        if (distanceToStartPos >= runPosition)
        {
            CheckWaitTime();
        }
    }*/
    public IEnumerator Spit(Vector2 value, float zAngle)
    {
        if (IsStateActive(EMonsterState.isWait))
        {
            yield break;
        }
        animState = EanimState.Spit;
        SetCurrentAnimation(animState);
        yield return new WaitForSeconds(spitWaitTime);
        GameObject projectileObj = ObjectPoolManager.instance.GetProjectileFromPool(0);
        if (projectileObj != null)
        {
            projectileObj.SetActive(true);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Shot(gameObject, attackTransform.transform.position, value.normalized,
                    stat.rangedAttackRange, stat.rangedAttackSpeed, stat.rangedAttackDamege, isHeavy, zAngle, eActivableColor.RED);
                projectileObj.transform.position = attackTransform.transform.position;

                PlayManager.Instance.UpdateColorthing();
                projectile.ReturnStartRoutine(spitTime);
            }
        }
    }

    private IEnumerator ChargeAttack(Vector2 value, Vector2 check)
    {
        if (IsStateActive(EMonsterState.isWait))
        {
            yield break;
        }
        animState = EanimState.Charge;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds((float)jsonObject["animations"]["attack/charge_attack"]["events"][1]["time"]);

        meleeAttack?.AttackAble(-value, stat.attackDamage);
        rigid.AddForce(check * stat.specialAttackRound, ForceMode2D.Impulse);
        meleeAttack?.AttackDisable();
    }
    private IEnumerator EarthAttack()
    {
        if (IsStateActive(EMonsterState.isWait))
        {
            yield break;
        }
        animState = EanimState.Ground;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds((float)jsonObject["animations"]["attack/ground_attack"]["events"][0]["time"]);
        rigid.velocity = Vector2.up * stat.earthAttackJump;
        currentState |= EMonsterAttackState.IsEarthAttack;

        StartCoroutine(SpawnObjects());
    }
    private IEnumerator CompositeAttack(Vector2 check)
    {
        rigid.AddForce(check * stat.compositeAttackRound, ForceMode2D.Impulse);
        animState = EanimState.Charge;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds((float)jsonObject["animations"]["attack/charge_attack"]["events"][1]["time"]); // FIX 애니메이션 속도 확인 이후 패치

        StartCoroutine(EarthAttack());
    }
    private IEnumerator SpawnObjects()
    {
        if (IsStateActive(EMonsterState.isWait))
        {
            yield break;
        }
        int objectCount = earthObjects.Length;
        while (currentState.HasFlag(EMonsterAttackState.IsEarthAttack))
        {
            yield return new WaitForSeconds(delayToEarthAttack);

            StartCoroutine(ActivateParticle(earthParticleGroup));

            currentState &= ~EMonsterAttackState.IsEarthAttack;

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
                HPDown(criticalDamage);
                rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
            }
            else
            {
                HPDown(damage);
                rigid.AddForce(attackDir * stat.hitReboundPower, ForceMode2D.Impulse);
            }
        }
        else
        {
            HPDown(damage);
            rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
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
        SetState(EMonsterState.isBattle, false);
        SetState(EMonsterState.isWait, false);
        SetState(EMonsterState.isPlayerBetween, false);
        isDead = false;
        StopCoroutine(AttackSequence(PlayerPos));
        skeletonAnimation.state.GetCurrent(0).TimeScale = 0;
        skeletonAnimation.state.GetCurrent(0).TimeScale = 1;
        ///<summary> 현재 실행중인 애니메이션 강제 종료, 따로 함수가 없음. ///</summary>
        animState = EanimState.Dead;
        SetCurrentAnimation(animState);
        yield return new WaitForSeconds(deadDelayTime);
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
            if (stat.meleeAttackRange >= distanceToStartPos)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawWireSphere(transform.position + transform.forward, stat.senseCircle);
            Gizmos.DrawWireSphere(transform.position + transform.forward, stat.meleeAttackRange);
            Gizmos.DrawWireSphere(transform.position + transform.forward, stat.rangedAttackRange);
        }
    }
}