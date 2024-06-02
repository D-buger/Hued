using Newtonsoft.Json.Linq;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
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

    private bool isHeavy = false;
    private bool isDoubleBadyAttack = false;
    private bool isReturnStop = false;
    private bool isThrow = false;

    public enum EAnimState
    {
        DETECTION,
        IDLE,
        WALK,
        CHARGEREADY,
        CHARGE,
        CHARGEIDLE,
        CHARGEFINISH,
        DEAD,
        DEADFALL,
        DEADLAND,
        THROWREADY,
        THROW,
        THROWCALLBACK,
        THROWCATCH
    }
    [Header("Animation")]
    [SerializeField]
    private EAnimState animState;

    private SkeletonAnimation skeletonAnimation;
    [SerializeField]
    private AnimationReferenceAsset[] aniClip;
    [SerializeField]
    private TextAsset animationJson;
    private string currentAnimation;

    private enum EMonsterAttackState
    {
        None = 0,
        isBadyAttack = 1 << 0,
        isReturnEnemy = 2 << 0,
        isFirstAttack = 3 << 0,
        isAttack = 4 << 0,
    }
    [SerializeField]
    private EMonsterAttackState attackState = EMonsterAttackState.isFirstAttack;
    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;

    private void Awake()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        fsm = GetComponent<MonsterFSM>();
        rigid = GetComponent<Rigidbody2D>();
        attackPoint = transform.GetChild(0).gameObject;
        meleeAttack = attackPoint.GetComponentInChildren<Attack>();
    }

    private void OnEnable()
    {
        currentHP = stat.MonsterHP;
        SetState(EMonsterState.isWait, true);
        SetAttackState(EMonsterAttackState.isAttack, false);
        isDead = false;
        rigid.gravityScale = 0;
        rushAttackObject.SetActive(false);
        CheckStateChange();
    }
    private void Start()
    {
        skeletonAnimation.state.SetAnimation(0, "FA/idle", true);
        monsterStartPos = transform.position;
        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, stat.enemyColor);

        runPosition = stat.enemyRoamingRange;

        monsterRunleftPosition.y = transform.position.y;
        monsterRunRightPosition.y = transform.position.y;
        monsterRunleftPosition.x += transform.position.x + runPosition;
        monsterRunRightPosition.x += transform.position.x - runPosition;

        PlayManager.Instance.FilterColorAttackEvent.AddListener(IsActiveColor);
        monsterPosition = monsterRunRightPosition;
        startFlyAntPosition = new Vector2(transform.position.x, transform.position.y);

        originLayer = LayerMask.GetMask("Enemy");
        colorVisibleLayer = LayerMask.GetMask("ColorEnemy");
        if (animationJson is not null)
        {
            jsonObject = JObject.Parse(animationJson.text);
        }
        SetState(EMonsterState.isWait, true);
    }
    private void Update()
    {
        if (!isDead)
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
            if (attackState.HasFlag(EMonsterAttackState.isBadyAttack) && !attackState.HasFlag(EMonsterAttackState.isReturnEnemy) && attackState.HasFlag(EMonsterAttackState.isAttack))
            {
                StartCoroutine(Rush());
            }
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
            case EAnimState.DEAD:
                AsyncAnimation(aniClip[(int)EAnimState.DEAD], false, timeScale);
                break;
            case EAnimState.DEADFALL:
                AsyncAnimation(aniClip[(int)EAnimState.DEADFALL], true, timeScale);
                break;
            case EAnimState.DEADLAND:
                AsyncAnimation(aniClip[(int)EAnimState.DEADLAND], false, timeScale);
                break;
            case EAnimState.THROWREADY:
                AsyncAnimation(aniClip[(int)EAnimState.THROWREADY], false, timeScale);
                break;
            case EAnimState.THROW:
                AsyncAnimation(aniClip[(int)EAnimState.THROW], false, timeScale);
                break;
            case EAnimState.THROWCALLBACK:
                AsyncAnimation(aniClip[(int)EAnimState.THROWCALLBACK], true, timeScale);
                break;
            case EAnimState.THROWCATCH:
                AsyncAnimation(aniClip[(int)EAnimState.THROWCATCH], false, timeScale);
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
        //SetAttackState(EMonsterAttackState.isFirstAttack, true);
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

        if (canAttack && !attackState.HasFlag(EMonsterAttackState.isAttack) && distanceToPlayer <= stat.enemyRoamingRange)
        {
            StartCoroutine(AttackSequence(PlayerPos));
        }
        else if (canAttack && !attackState.HasFlag(EMonsterAttackState.isAttack))
        {
            SetState(EMonsterState.isBattle, false);
            SetState(EMonsterState.isWait, true);
            CheckStateChange();
        }
    }
    private IEnumerator AttackSequence(Vector2 attackAngle)
    {
        SetAttackState(EMonsterAttackState.isAttack, true);
        canAttack = false;
        GetPlayerPositionFromMonster();
        animState = EAnimState.DETECTION;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds(1.33f);
        if (!isDead)
        {
            float zAngle = 0;
            int checkRandomAttackType = UnityEngine.Random.Range(1, 101);
            if (checkRandomAttackType < 50)
            {
                StartCoroutine(RushAttack(zAngle)); // 돌진 공격
            }
            else
            {
                StartCoroutine(SpearThrowAttack()); // 창 던지기 공격
            }
        }
    }

    private IEnumerator RushAttack(float zAngle)
    {
        if (!isDead)
        {
            animState = EAnimState.CHARGEREADY;
            SetCurrentAnimation(animState);
            yield return Yields.WaitSeconds(stat.rushReadyAnimaionDuration);
            if (isDead)
            {
                yield break;
            }
            int checkRandomAttackType = UnityEngine.Random.Range(1, 101);
            yield return Yields.WaitSeconds(stat.flyAntAttackDelay);
            if (isDead)
            {
                yield break;
            }
            animState = EAnimState.CHARGE;
            SetCurrentAnimation(animState);
            yield return Yields.WaitSeconds((float)jsonObject["animations"]["FA/chage/charge"]["events"][0]["time"]);
            if (isDead)
            {
                yield break;
            }
            RushAttackDirection();
            targetPos = new(PlayerPos.x, PlayerPos.y);
            if (checkRandomAttackType > stat.doubleBadyAttackPercent)
            {
                SetAttackState(EMonsterAttackState.isBadyAttack, true);
                isDoubleBadyAttack = true;
                isReturnStop = true;
                CheckAttackStateChange();
            }
            else
            {
                SetAttackState(EMonsterAttackState.isBadyAttack, true);
                CheckAttackStateChange();
            }
            rushAttackObject.SetActive(true);
        }
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
                if (!isDead)
                {
                    isReturnStop = false;
                    SetAttackState(EMonsterAttackState.isReturnEnemy, true);
                    animState = EAnimState.IDLE;
                    SetCurrentAnimation(animState);
                    yield return Yields.WaitSeconds(0.5f); //FIX 매직넘버
                }
                if (isDead)
                {
                    yield break;
                }
                targetPos = new(PlayerPos.x, PlayerPos.y);
                isReturnStop = true;
                SetAttackState(EMonsterAttackState.isReturnEnemy, false);
                isDoubleBadyAttack = false;
                SetAttackState(EMonsterAttackState.isBadyAttack, true);
                animState = EAnimState.CHARGEIDLE;
                SetCurrentAnimation(animState);
                RushAttackDirection();
                rigid.gravityScale = 1;
                rigid.gravityScale = 0;
                yield return Yields.WaitSeconds(1.0f); //FIX 매직넘버
                if (isDead)
                {
                    yield break;
                }
                animState = EAnimState.CHARGEFINISH;
                SetCurrentAnimation(animState);
                isDoubleBadyAttack = false;
                yield return Yields.WaitSeconds(1.167f); //FIX 매직넘버
                if (isDead)
                {
                    yield break;
                }
            }
            else
            {
                if (isDead)
                {
                    yield break;
                }
                SetAttackState(EMonsterAttackState.isBadyAttack, false);
                animState = EAnimState.CHARGEFINISH;
                SetCurrentAnimation(animState);
                transform.rotation = Quaternion.Euler(1, 1, 1);
                yield return Yields.WaitSeconds(1.0f); //FIX 매직넘버
                if (isDead)
                {
                    yield break;
                }
                SetAttackState(EMonsterAttackState.isReturnEnemy, true);
                isReturnStop = false;
                rushAttackObject.SetActive(false);
                canAttack = true;
            }
        }
    }
    private void ReturnMonster()
    {
        if (isDead)
        {
            return;
        }
        animState = EAnimState.IDLE;
        SetCurrentAnimation(animState);
        transform.position = Vector2.MoveTowards(transform.position, battlePos, stat.returnSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(1, 1, 1);
        if (Vector2.Distance(transform.position, battlePos) <= stat.returnPosValue && !isDoubleBadyAttack)
        {
            SetAttackState(EMonsterAttackState.isReturnEnemy, false);
            SetAttackState(EMonsterAttackState.isAttack, false);
            canAttack = true;
        }
    }
    private IEnumerator SpearThrowAttack()
    {

        animState = EAnimState.THROWREADY;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds(3); //매직넘버
        if (isDead)
        {
            yield break;
        }
        animState = EAnimState.THROW;
        SetCurrentAnimation(animState);
        GetPlayerPositionFromMonster();

        float ZAngle = (Mathf.Atan2(PlayerPos.x - transform.position.x, PlayerPos.y - transform.position.y) * Mathf.Rad2Deg) + stat.projectileZAngleByHeight;
        Vector2 shotDir = PlayerPos - (Vector2)transform.position;
        GameObject projectileObj = ObjectPoolManager.instance.GetProjectileFromPool(2);
        if (projectileObj is not null)
        {
            projectileObj.SetActive(true);

            SpearAttack projectile = projectileObj.GetComponent<SpearAttack>();
            if (projectile is not null)
            {
                projectile.Shot(gameObject, attackTransform.transform.position, shotDir.normalized,
                    stat.spearThrowAttackRange, stat.spearThrowSpeed, stat.spearThrowDamage, ZAngle, eActivableColor.RED);
                projectileObj.transform.position = attackTransform.transform.position;

                PlayManager.Instance.UpdateColorthing();
                projectile.ReturnStartRoutine(stat.spearThrowAttackRange);
            }
        }
        yield return Yields.WaitSeconds(0.1f); //매직넘버
        if (isDead)
        {
            yield break;
        }
        isThrow = true;
        animState = EAnimState.THROWCALLBACK;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds(3); //매직넘버
        if (isDead)
        {
            yield break;
        }

    }

    public override void Dead()
    {
        StartCoroutine(DeadSequence());
    }
    private IEnumerator DeadSequence()
    {
        rushAttackObject.SetActive(false);
        SetAttackState(EMonsterAttackState.isReturnEnemy, false);
        SetAttackState(EMonsterAttackState.isBadyAttack, false);
        isReturnStop = false;
        isDoubleBadyAttack = false;
        SetState(EMonsterState.isBattle, false);
        SetState(EMonsterState.isWait, false);
        transform.localScale = new Vector3(1, 1, 1);
        animState = EAnimState.DEAD;
        SetCurrentAnimation(animState);
        rigid.gravityScale = 2.0f;
        yield return new WaitForSeconds(0.333f); // 매직넘버 수정
        animState = EAnimState.DEADFALL;
        SetCurrentAnimation(animState);
        yield return new WaitForSeconds(stat.deadDelay);
        gameObject.SetActive(false);
        isDead = true;
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

    private void RushAttackDirection()
    {
        Vector2 attackDirection = PlayerPos - (Vector2)transform.position;
        if (attackDirection.x <= -5)
        {
            transform.localScale = new Vector2(1, 1);
            transform.rotation = Quaternion.Euler(1, 1, 45); // 몬스터 기준 왼쪽
        }
        else if (attackDirection.x < 0)
        {
            transform.localScale = new Vector2(1, 1);
            transform.rotation = Quaternion.Euler(1, 1, 55); // 몬스터 기준 왼쪽 아래
        }
        else if (attackDirection.x < 5)
        {
            transform.localScale = new Vector2(-1, 1);
            transform.rotation = Quaternion.Euler(1, 1, -55); // 몬스터 기준 오른쪽
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
            transform.rotation = Quaternion.Euler(1, 1, -45); // 몬스터 기준 오른쪽 아래
        }
    }
    private void GetPlayerPositionFromMonster()
    {
        Vector2 monsterMoveDirection = PlayerPos - (Vector2)transform.position;
        if (monsterMoveDirection.x < -5)
        {
            transform.localScale = new Vector2(1, 1);
            stat.projectileZAngleByHeight = -180; // 몬스터 기준 왼쪽
        }
        else if (monsterMoveDirection.x < 0)
        {
            transform.localScale = new Vector2(1, 1);
            stat.projectileZAngleByHeight = 210; // 몬스터 기준 왼쪽 아래
        }
        else if (monsterMoveDirection.x < 5)
        {
            transform.localScale = new Vector2(-1, 1);
            stat.projectileZAngleByHeight = -30; // 몬스터 기준 오른쪽 아래
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
            stat.projectileZAngleByHeight = 0; // 몬스터 기준 오른쪽
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
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Attack") && isThrow)
        {
            animState = EAnimState.THROWCATCH;
            SetCurrentAnimation(animState);
            isThrow = false;
            SetAttackState(EMonsterAttackState.isAttack, false);
            canAttack = true;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (attackState.HasFlag(EMonsterAttackState.isBadyAttack))
        {
            if (collision.collider.CompareTag("Floor"))
            {
                isDoubleBadyAttack = false;
                isReturnStop = false;
                SetAttackState(EMonsterAttackState.isBadyAttack, false);
                SetAttackState(EMonsterAttackState.isReturnEnemy, true);
            }
        }
        if (isDead)
        {
            if (collision.collider.CompareTag("Floor"))
            {
                animState = EAnimState.DEADLAND;
                SetCurrentAnimation(animState);
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
