using Newtonsoft.Json.Linq;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI;
using Color = UnityEngine.Color;

public class FlyAntEnemy : Monster, IAttack, IParryConditionCheck
{
    private MonsterFSM fsm;
    private Rigidbody2D rigid;
    private GameObject attackPoint;
    private Projectile projectile;
    private Attack meleeAttack;

    [SerializeField]
    private GameObject attackTransform;
    [SerializeField]
    private GameObject rushAttackObject;
    [SerializeField]
    private FlyAntMonsterStat stat;
    [SerializeField]
    private JObject jsonObject;
    private Vector2 targetPos;
    private Vector2 battlePos;
    public Vector2 startFlyAntPosition;

    public UnityEvent<eActivableColor> flyAntColorEvent;

    private LayerMask originLayer;
    private LayerMask colorVisibleLayer;

    private float deadDelayTime = 1.3f;

    private bool isHeavy = false;
    private bool isDoubleBadyAttack = false;
    private bool isReturnStop = false;

    public enum EAnimState
    {
        DETECTION,
        IDLE,
        WALK,
        CHARGEREADY,
        CHARGE,
        CHARGEIDLE,
        CHARGEFINISH
    }
    [Header("Animation")]
    [SerializeField]
    private EAnimState animState;
    [SerializeField]
    private SkeletonAnimation skeletonAnimation;
    [SerializeField]
    private AnimationReferenceAsset[] aniClip;
    [SerializeField]
    private TextAsset animationJson;
    private string currentAnimation;

    private enum EMonsterAttackState
    {
        None = 0,
        IsAttack = 1 << 0,
        isBadyAttack = 2 << 0,
        isReturnEnemy = 3 << 0,
        isFirstAttack = 4 << 0,
    }
    [SerializeField]
    private EMonsterAttackState attackState = EMonsterAttackState.isFirstAttack;
    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;

    private void Awake()
    {
        fsm = GetComponent<MonsterFSM>();
        rigid = GetComponent<Rigidbody2D>();
        attackPoint = transform.GetChild(0).gameObject;
        meleeAttack = attackPoint.GetComponentInChildren<Attack>();
    }

    private void OnEnable()
    {
        currentHP = stat.MonsterHP;
        SetState(EMonsterState.isWait, true);
        isDead = false;
        CheckStateChange();
    }
    private void Start()
    {
        monsterStartPos = transform.position;
        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, stat.enemyColor);

        runPosition = stat.enemyRoamingRange;

        monsterRunleftPosition.y = transform.position.y;
        monsterRunRightPosition.y = transform.position.y;
        monsterRunleftPosition.x += transform.position.x + runPosition;
        monsterRunRightPosition.x += transform.position.x - runPosition;

        PlayManager.Instance.FilterColorAttackEvent.AddListener(IsActiveColor);
        PlayManager.Instance.UpdateColorthing();
        monsterPosition = monsterRunRightPosition;
        startFlyAntPosition = new Vector2(transform.position.x, transform.position.y);

        originLayer = LayerMask.GetMask("Enemy");
        colorVisibleLayer = LayerMask.GetMask("ColorEnemy");

        if (animationJson is not null)
        {
            jsonObject = JObject.Parse(animationJson.text);
        }
    }
    private void Update()
    {
        distanceToMonsterStartPos = Vector2.Distance(transform.position, monsterStartPos);
        StartCoroutine(CheckPlayer(startFlyAntPosition));
        if (IsStateActive(EMonsterState.isBattle))
        {
            Attack();
        }
        if (attackState.HasFlag(EMonsterAttackState.isReturnEnemy) && !isReturnStop)
        {
            ReturnMonster();
        }
        if (attackState.HasFlag(EMonsterAttackState.isBadyAttack) && !attackState.HasFlag(EMonsterAttackState.isReturnEnemy))
        {
            StartCoroutine(Rush());
        }
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
    private void SetCurrentAnimation(EAnimState _state)
    {
        float timeScale = 1;
        switch (_state)
        {
            case EAnimState.DETECTION:
                AsyncAnimation(aniClip[(int)EAnimState.DETECTION], false, timeScale);
                break;
            case EAnimState.IDLE:
                AsyncAnimation(aniClip[(int)EAnimState.IDLE], true, timeScale);
                break;
            case EAnimState.WALK:
                AsyncAnimation(aniClip[(int)EAnimState.WALK], true, timeScale);
                break;
            case EAnimState.CHARGEREADY:
                AsyncAnimation(aniClip[(int)EAnimState.CHARGEREADY], false, timeScale);
                break;
            case EAnimState.CHARGE:
                AsyncAnimation(aniClip[(int)EAnimState.CHARGE], false, timeScale);
                break;
            case EAnimState.CHARGEIDLE:
                AsyncAnimation(aniClip[(int)EAnimState.CHARGEIDLE], true, timeScale);
                break;
            case EAnimState.CHARGEFINISH:
                AsyncAnimation(aniClip[(int)EAnimState.CHARGEFINISH], false, timeScale);
                break;
        }
    }
    private void IsActiveColor(eActivableColor color)
    {
        int newLayer = SOO.Util.LayerMaskToNumber((color == stat.enemyColor) ? colorVisibleLayer : originLayer);
        newLayer -= 2;
        gameObject.layer = newLayer;
    }
    public override IEnumerator CheckPlayer(Vector2 startMonsterPos)
    {
        distanceToPlayer = Vector2.Distance(transform.position, PlayerPos);
        distanceToStartPos = Vector2.Distance(new(startMonsterPos.x, startMonsterPos.y - 5), PlayerPos);

        if (distanceToStartPos <= runPosition && !IsStateActive(EMonsterState.isBattle) && canAttack)
        {
            SetState(EMonsterState.isBattle, true);
            SetState(EMonsterState.isWait, false);
            elapsedTime = 0f;
            CheckStateChange();
        }
        else
        {
            StartCoroutine(CheckWaitTime());
        }
        yield break;
    }
    public override void WaitSituation()
    {
        currentHP = stat.MonsterHP;
        SetAttackState(EMonsterAttackState.isFirstAttack, true);
        SetState(EMonsterState.isBattle, false);
        transform.position = Vector2.MoveTowards(transform.position, monsterPosition, stat.moveSpeed * Time.deltaTime);
        if (monsterPosition == monsterRunleftPosition)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (monsterPosition == monsterRunRightPosition)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        if (HasArrived((Vector2)transform.position, monsterRunRightPosition))
        {
            monsterPosition = monsterRunleftPosition;
        }
        else if (HasArrived((Vector2)transform.position, monsterRunleftPosition))
        {
            monsterPosition = monsterRunRightPosition;
        }
    }
    public override IEnumerator CheckWaitTime()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= stat.waitStateDelay && !IsStateActive(EMonsterState.isWait) && !IsStateActive(EMonsterState.isBattle) && canAttack)
        {
            elapsedTime = 0f;
            SetState(EMonsterState.isWait, true);
            SetState(EMonsterState.isPlayerBetween, false);
            SetState(EMonsterState.isBattle, false);
            CheckStateChange();
        }
        yield break;
    }
    public override void Attack()
    {
        if (isDead)
        {
            return;
        }

        if (attackState.HasFlag(EMonsterAttackState.isFirstAttack))
        {
            battlePos = new(transform.position.x, transform.position.y);
            SetAttackState(EMonsterAttackState.isFirstAttack, false);
        }

        if (canAttack && !attackState.HasFlag(EMonsterAttackState.IsAttack) && distanceToPlayer <= stat.enemyRoamingRange)
        {
            StartCoroutine(AttackSequence(PlayerPos));
        }
        else if (canAttack && !attackState.HasFlag(EMonsterAttackState.IsAttack))
        {
            SetState(EMonsterState.isBattle, false);
            SetState(EMonsterState.isWait, true);
            CheckStateChange();
        }
    }
    private IEnumerator AttackSequence(Vector2 attackAngle)
    {
        SetAttackState(EMonsterAttackState.IsAttack, true);
        canAttack = false;

        Vector2 monsterMoveDirection = attackAngle - (Vector2)transform.position;
        if (monsterMoveDirection.x <= 0)
        {
            transform.localScale = new Vector2(1, 1);
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
        }
        animState = EAnimState.DETECTION;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds(1.33f);
        float zAngle = 0;
        int checkRandomAttackType = 40;//UnityEngine.Random.Range(1, 101);
        if (checkRandomAttackType < 50)
        {
            StartCoroutine(RushAttack(zAngle)); // 돌진 공격
        }
        else
        {
            SpearThrowAttack(); // 창 던지기 공격
            Debug.Log("창 던지기");
        }

        yield return Yields.WaitSeconds(stat.attackCooldown);
        SetAttackState(EMonsterAttackState.IsAttack, false);
        meleeAttack?.AttackDisable();
    }

    private IEnumerator RushAttack(float zAngle)
    {
        Vector2 attackDirection = PlayerPos - (Vector2)transform.position;
        animState = EAnimState.CHARGEREADY;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds(stat.rushReadyAnimaionDuration);

        targetPos = new(PlayerPos.x, PlayerPos.y);
        int checkRandomAttackType = 60;//UnityEngine.Random.Range(1, 101);
        int dmagepool = stat.contactDamage;
        yield return Yields.WaitSeconds(stat.flyAntAttackDelay);

        stat.contactDamage = stat.rushAttackDamage;
        animState = EAnimState.CHARGE;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds((float)jsonObject["animations"]["FA/chage/charge"]["events"][0]["time"]);

        zAngle = (Mathf.Atan2(PlayerPos.x - transform.position.x, PlayerPos.y - transform.position.y) * Mathf.Rad2Deg);
        if (attackDirection.x <= 0)
        {
            transform.localScale = new Vector2(1, 1);
            transform.rotation = Quaternion.Euler(1, 1, 40);
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
            transform.rotation = Quaternion.Euler(1, 1, -40);
        }
        if (checkRandomAttackType > stat.doubleBadyAttackPercent)
        {
            SetAttackState(EMonsterAttackState.isBadyAttack, true);
            isDoubleBadyAttack = true;
            isReturnStop = true;
        }
        else
        {
            SetAttackState(EMonsterAttackState.isBadyAttack, true);
        }
        CheckAttackStateChange();
        stat.contactDamage = dmagepool;
    }
    private IEnumerator Rush()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPos, stat.rushAttackSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, targetPos) <= stat.returnPosValue)
        {
            Vector2 attackDirection = PlayerPos - (Vector2)transform.position;
            if (isDoubleBadyAttack)
            {
                animState = EAnimState.CHARGEFINISH;
                SetCurrentAnimation(animState);
                transform.rotation = Quaternion.Euler(1, 1, 1);
                SetAttackState(EMonsterAttackState.isBadyAttack, false);
                yield return Yields.WaitSeconds(1.0f); //FIX 매직넘버
                isReturnStop = false;
                SetAttackState(EMonsterAttackState.isReturnEnemy, true);
                animState = EAnimState.IDLE;
                SetCurrentAnimation(animState);
                yield return Yields.WaitSeconds(0.5f); //FIX 매직넘버
                targetPos = new(PlayerPos.x, PlayerPos.y);
                isReturnStop = true;
                SetAttackState(EMonsterAttackState.isReturnEnemy, false);
                isDoubleBadyAttack = false;
                SetAttackState(EMonsterAttackState.isBadyAttack, true);
                animState = EAnimState.CHARGEIDLE;
                SetCurrentAnimation(animState);
                if (attackDirection.x <= 0)
                {
                    transform.rotation = Quaternion.Euler(1, 1, 30);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(1, 1, -30);
                }
                rigid.gravityScale = 1;
                rigid.gravityScale = 0;
                yield return Yields.WaitSeconds(1.0f); //FIX 매직넘버
                animState = EAnimState.CHARGEFINISH;
                SetCurrentAnimation(animState);
                isDoubleBadyAttack = false;
                yield return Yields.WaitSeconds(1.167f); //FIX 매직넘버
            }
            else
            {
                SetAttackState(EMonsterAttackState.isBadyAttack, false);
                animState = EAnimState.CHARGEFINISH;
                SetCurrentAnimation(animState);
                transform.rotation = Quaternion.Euler(1, 1, 1);
                yield return Yields.WaitSeconds(1.0f); //FIX 매직넘버
                SetAttackState(EMonsterAttackState.isReturnEnemy, true);
                isReturnStop = false;
            }
        }
    }
    private void ReturnMonster()
    {
        animState = EAnimState.IDLE;
        SetCurrentAnimation(animState);
        transform.position = Vector2.MoveTowards(transform.position, battlePos, stat.returnSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(1, 1, 1);
        if (Vector2.Distance(transform.position, battlePos) <= stat.returnPosValue && !isDoubleBadyAttack)
        {
            SetAttackState(EMonsterAttackState.isReturnEnemy, false);
            canAttack = true;
        }
    }
    private void SpearThrowAttack()
    {
        Vector2 shotDir = PlayerPos - (Vector2)transform.position;
        float ZAngle = (Mathf.Atan2(PlayerPos.x - transform.position.x, PlayerPos.y - transform.position.y) * Mathf.Rad2Deg);
        GameObject projectileObj = ObjectPoolManager.instance.GetProjectileFromPool(2);
        if (projectileObj is not null)
        {
            projectileObj.SetActive(true);

            SpearAttack projectile = projectileObj.GetComponent<SpearAttack>();
            if (projectile is not null)
            {
                projectile.Shot(gameObject, attackTransform.transform.position, shotDir.normalized,
                    stat.spearThrowAttackRange, stat.spearThrowSpeed, stat.spearThrowDamage, ZAngle, eActivableColor.RED);
                projectileObj.transform.position = transform.position;

                PlayManager.Instance.UpdateColorthing();
                projectile.ReturnStartRoutine(stat.spearThrowAttackRange);
            }
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
        isDead = false;
        yield return new WaitForSeconds(deadDelayTime);
        gameObject.SetActive(false);
    }
    public override void CheckStateChange()
    {
        switch (state)
        {
            case EMonsterState.isBattle:
                fsm.ChangeState("Attack");
                break;
            case EMonsterState.isWait:
                fsm.ChangeState("Idle");
                animState = EAnimState.IDLE;
                SetCurrentAnimation(animState);
                break;
            default:
                break;
        }
    }
    private void CheckAttackStateChange()
    {
        switch (attackState)
        {
            case EMonsterAttackState.isBadyAttack:
                animState = EAnimState.CHARGEIDLE;
                SetCurrentAnimation(animState);
                break;
            default:
                break;
        }
    }

    public override void Hit(int damage, int colorDamage, Vector2 attackDir, IParryConditionCheck parryCheck = null)
    {
        if (isDead)
        {
            return;
        }
        if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
        {
            currentHP -= colorDamage;
            rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
        }
        else
        {
            currentHP -= damage;
            rigid.AddForce(attackDir * stat.hitReboundPower, ForceMode2D.Impulse);
        }

        if (!isDead)
        {
            CheckDead();
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!isDead)
        {
            if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
            {
                collision.gameObject.GetComponent<Player>().Hit(stat.contactDamage, stat.contactDamage,
                        transform.position - collision.transform.position, null);
            }
        }
    }
    public bool CanParryAttack()
    {
        return PlayManager.Instance.ContainsActivationColors(stat.enemyColor);
    }
    private void SetAttackState(EMonsterAttackState eState, bool value)
    {
        if (value)
        {
            attackState |= eState;
        }
        else
        {
            attackState &= ~eState;
        }
    }
    private void OnDrawmos()
    {
        if (stat is not null)
        {
            if (stat.senseCircle >= distanceToStartPos)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawCube(transform.position + transform.forward, stat.senseCube);
        }
    }
    //public override void Respawn(GameObject monsterPos, bool isRespawnMonster)
    //{
    //    throw new System.NotImplementedException();
    //}
}
