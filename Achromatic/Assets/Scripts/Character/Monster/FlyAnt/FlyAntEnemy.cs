using Newtonsoft.Json.Linq;
using Spine.Unity;
using System.Collections;
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
    private GameObject flyAntAttackTransform;
    [SerializeField]
    private GameObject rushAttackObject;
    [SerializeField]
    private FlyAntMonsterStat stat;
    [SerializeField]
    private JObject jsonObject;

    private Vector2 targetPos;
    [SerializeField]
    private Vector2 battlePos;
    private Vector2 startFlyAntPosition;
    private Vector2 targetMonsterPosition;

    private Vector2 flyAntRunLeftPosition;
    private Vector2 flyAntRunRightPosition;

    public UnityEvent<eActivableColor> flyAntColorEvent;

    private LayerMask originLayer;
    private LayerMask colorVisibleLayer;

    private bool isHeavy = false;
    private bool isDoubleBodyAttack = false;
    private bool isThrow = false;
    private bool stopDoubleAttack = false;

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
    [SerializeField]
    private SkeletonAnimation skeletonAnimation;
    [SerializeField]
    private AnimationReferenceAsset[] aniClip;
    [SerializeField]
    private TextAsset animationJson;
    private string currentAnimation;

    private enum EMonsterAttackState
    {
        NONE = 0,
        ISBODYATTACK = 1 << 0,
        ISRETURNENEMY = 2 << 0,
        ISFIRSTATTACK = 3 << 0,
        ISATTACK = 4 << 0,
    }
    [SerializeField]
    private EMonsterAttackState attackState = EMonsterAttackState.NONE;
    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;

    private void Awake()
    {
        fsm = GetComponent<MonsterFSM>();
        rigid = GetComponent<Rigidbody2D>();
        attackPoint = transform.GetChild(0).gameObject;
        meleeAttack = attackPoint.GetComponentInChildren<Attack>();
    }

    private void Start()
    {
        currentHP = stat.MonsterHP;
        SetAttackState(EMonsterAttackState.ISATTACK, false);
        SetAttackState(EMonsterAttackState.ISBODYATTACK, false);
        isDead = false;
        rigid.gravityScale = 0;
        rushAttackObject.SetActive(false);

        transform.localScale = new Vector3(-1, 1, 1);
        battlePos = transform.position;
        startFlyAntPosition = transform.position;
        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, stat.enemyColor);

        flyAntRunLeftPosition.y = transform.position.y;
        flyAntRunRightPosition.y = transform.position.y;
        flyAntRunLeftPosition.x += transform.position.x + stat.enemyRoamingRange;
        flyAntRunRightPosition.x += transform.position.x - stat.enemyRoamingRange;

        PlayManager.Instance.FilterColorAttackEvent.AddListener(IsActiveColor);
        targetMonsterPosition = monsterRunRightPosition;

        originLayer = LayerMask.GetMask("Enemy");
        colorVisibleLayer = LayerMask.GetMask("ColorEnemy");
        if (animationJson is not null)
        {
            jsonObject = JObject.Parse(animationJson.text);
        }
        SetState(EMonsterState.isWait, true);
        SetState(EMonsterState.isBattle, false);
        CheckStateChange();
    }
    private void Update()
    {
        if (!isDead)
        {
            distanceToMonsterStartPos = Vector2.Distance(transform.position, startFlyAntPosition);
            StartCoroutine(CheckPlayer(startFlyAntPosition));
            if (attackState.HasFlag(EMonsterAttackState.ISRETURNENEMY) && attackState.HasFlag(EMonsterAttackState.ISATTACK) && state.HasFlag(EMonsterState.isBattle))
            {
                ReturnMonster();
            }
            if (attackState.HasFlag(EMonsterAttackState.ISBODYATTACK) && !attackState.HasFlag(EMonsterAttackState.ISRETURNENEMY))
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
        distanceToStartPos = Vector2.Distance(startMonsterPos, PlayerPos);

        if (distanceToStartPos <= stat.enemyRoamingRange && !IsStateActive(EMonsterState.isBattle) && canAttack)
        {
            SetState(EMonsterState.isWait, false);
            SetState(EMonsterState.isBattle, true);
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
        SetState(EMonsterState.isBattle, false);
        transform.position = Vector2.MoveTowards(transform.position, targetMonsterPosition, stat.moveSpeed * Time.deltaTime);
        if (targetMonsterPosition == flyAntRunLeftPosition)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (targetMonsterPosition == flyAntRunRightPosition)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        if (HasArrived((Vector2)transform.position, flyAntRunRightPosition))
        {
            targetMonsterPosition = flyAntRunLeftPosition;
        }
        else if (HasArrived((Vector2)transform.position, flyAntRunLeftPosition))
        {
            targetMonsterPosition = flyAntRunRightPosition;
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

        if (attackState.HasFlag(EMonsterAttackState.ISFIRSTATTACK))
        {
            SetAttackState(EMonsterAttackState.ISFIRSTATTACK, false);
        }

        if (canAttack && !attackState.HasFlag(EMonsterAttackState.ISATTACK) && distanceToPlayer <= stat.enemyRoamingRange)
        {
            StartCoroutine(AttackSequence(PlayerPos));
            battlePos = transform.position;
        }
        else if (canAttack && !attackState.HasFlag(EMonsterAttackState.ISATTACK) && distanceToPlayer >= stat.enemyRoamingRange)
        {
            SetState(EMonsterState.isBattle, false);
            SetState(EMonsterState.isWait, true);
            CheckStateChange();
        }
    }
    private IEnumerator AttackSequence(Vector2 attackAngle)
    {
        SetAttackState(EMonsterAttackState.ISATTACK, true);
        canAttack = false;
        GetPlayerPositionFromMonster();
        animState = EAnimState.DETECTION;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds(1.33f);
        if (!isDead)
        {
            int checkRandomAttackType = UnityEngine.Random.Range(1, 101);
            if (checkRandomAttackType < 50)
            {
                StartCoroutine(RushAttack()); // 돌진 공격
            }
            else
            {
                StartCoroutine(SpearThrowAttack()); // 창 던지기 공격
            }
        }
        yield break;
    }

    private IEnumerator RushAttack()
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
            if (checkRandomAttackType > stat.doubleBodyAttackPercent)
            {
                SetAttackState(EMonsterAttackState.ISBODYATTACK, true);
                isDoubleBodyAttack = true;
                stopDoubleAttack = true;
                CheckAttackStateChange();
            }
            else
            {
                SetAttackState(EMonsterAttackState.ISBODYATTACK, true);
                CheckAttackStateChange();
            }
            rushAttackObject.SetActive(true);
            yield break;
        }
    }
    private IEnumerator Rush()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPos, stat.rushAttackSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, targetPos) <= stat.returnPosValue)
        {
            if (isDoubleBodyAttack && stopDoubleAttack)
            {
                stopDoubleAttack = false;
                animState = EAnimState.CHARGEFINISH;
                SetCurrentAnimation(animState);
                transform.rotation = Quaternion.Euler(1, 1, 1);
                SetAttackState(EMonsterAttackState.ISBODYATTACK, false);
                yield return Yields.WaitSeconds(1.0f); //FIX 매직넘버
                if (!isDead)
                {
                    SetAttackState(EMonsterAttackState.ISRETURNENEMY, true);
                    animState = EAnimState.IDLE;
                    SetCurrentAnimation(animState);
                    yield return Yields.WaitSeconds(0.5f); //FIX 매직넘버
                }
                if (isDead)
                {
                    yield break;
                }
                targetPos = new(PlayerPos.x, PlayerPos.y);
                SetAttackState(EMonsterAttackState.ISRETURNENEMY, false);
                isDoubleBodyAttack = false;
                SetAttackState(EMonsterAttackState.ISBODYATTACK, true);
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
                isDoubleBodyAttack = false;
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
                SetAttackState(EMonsterAttackState.ISBODYATTACK, false);
                animState = EAnimState.CHARGEFINISH;
                SetCurrentAnimation(animState);
                transform.rotation = Quaternion.Euler(1, 1, 1);
                yield return Yields.WaitSeconds(1.0f); //FIX 매직넘버
                if (isDead)
                {
                    yield break;
                }
                SetAttackState(EMonsterAttackState.ISRETURNENEMY, true);
                rushAttackObject.SetActive(false);
                canAttack = true;
                yield break;
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
        if (Vector2.Distance(transform.position, battlePos) <= stat.returnPosValue && !isDoubleBodyAttack)
        {
            SetAttackState(EMonsterAttackState.ISRETURNENEMY, false);
            SetAttackState(EMonsterAttackState.ISATTACK, false);
            canAttack = true;
        }
    }
    private IEnumerator SpearThrowAttack()
    {

        animState = EAnimState.THROWREADY;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds(3); //FIX 매직넘버
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
                projectile.Shot(gameObject, flyAntAttackTransform.transform.position, shotDir.normalized,
                    stat.spearThrowAttackRange, stat.spearThrowSpeed, stat.spearThrowDamage, ZAngle, eActivableColor.RED);
                projectileObj.transform.position = flyAntAttackTransform.transform.position;

                PlayManager.Instance.UpdateColorthing();
                projectile.ReturnStartRoutine(stat.spearThrowAttackRange);
            }
        }

        if (isDead)
        {
            yield break;
        }
        yield return Yields.WaitSeconds(0.1f); //FIX 매직넘버
        isThrow = true;
        animState = EAnimState.THROWCALLBACK;
        SetCurrentAnimation(animState);
        yield break;
    }

    public override void Dead()
    {
        StartCoroutine(DeadSequence());
    }
    private IEnumerator DeadSequence()
    {
        rushAttackObject.SetActive(false);
        SetAttackState(EMonsterAttackState.ISRETURNENEMY, false);
        SetAttackState(EMonsterAttackState.ISBODYATTACK, false);
        isDoubleBodyAttack = false;
        SetState(EMonsterState.isBattle, false);
        SetState(EMonsterState.isWait, false);
        transform.localScale = new Vector3(1, 1, 1);
        animState = EAnimState.DEAD;
        SetCurrentAnimation(animState);
        rigid.gravityScale = 2.0f;
        yield return new WaitForSeconds(0.333f); //FIX 매직넘버 수정
        animState = EAnimState.DEADFALL;
        SetCurrentAnimation(animState);
        yield return new WaitForSeconds(stat.deadDelay);
        gameObject.SetActive(false);
        isDead = true;
        yield break;
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
                SetAttackState(EMonsterAttackState.ISFIRSTATTACK, true);
                break;
            default:
                break;
        }
    }
    private void CheckAttackStateChange()
    {
        switch (attackState)
        {
            case EMonsterAttackState.ISBODYATTACK:
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

    public override void Hit(int damage, int colorDamage, Vector2 attackDir, IParryConditionCheck parryCheck = null, bool finish = false)
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
            SetAttackState(EMonsterAttackState.ISATTACK, false);
            canAttack = true;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
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
