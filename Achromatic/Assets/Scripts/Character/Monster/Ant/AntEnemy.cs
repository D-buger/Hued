using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AntEnemy : Monster, IAttack, IParryConditionCheck
{
    private MonsterFSM fsm;

    private Rigidbody2D rigid;
    private GameObject attackPoint;
    private Attack meleeAttack;
    [SerializeField]
    private GameObject attackTransform;
    [SerializeField]
    private AntMonsterStat stat;
    [SerializeField]
    private GameObject[] stabAttackObject;
    [SerializeField]
    private GameObject SlashAttackObject;
    [SerializeField]
    private GameObject counterAttackObject;
    [SerializeField]
    private JObject jsonObject;

    [Header("Animation")]
    [SerializeField]
    private SkeletonAnimation skeletonAnimation;
    [SerializeField]
    private AnimationReferenceAsset[] aniClip;
    [SerializeField]
    private TextAsset animationJson;
    public enum EAnimState
    {
        DETECTION,
        IDLE,
        WALK,
        STAB,
        SLASH,
        COUNTER,
        COUNTERATTACK,
        DEAD,
        MOVETOBREAK,
        FLIP,
        SPEAROVER,
        COUNTEROFF,
        COUNTERTRIGGER,
        ENEMYDISCOVERY,
        BATTLEIDLE
    }
    [SerializeField]
    private EAnimState animState;

    public Vector2 startAntPosition;

    public UnityEvent<eActivableColor> antColorEvent;

    private string currentAnimation;
    private float stabDelayToAttack = 0.2f;
    private float stabDelayToDestory = 0.05f;
    private float originalMoveSpeed = 1;
    private float originalRunSpeed = 4;
    private float rightZAngle = 220;
    private float leftZAngle = 160;
    private int moveSpeedDown = 0;

    private LayerMask colorVisibleLayer;
    private LayerMask originalLayer;
    private enum EMonsterAttackState
    {
        NONE = 0,
        ISATTACK = 1 << 0,
        ISSTABATTACK = 1 << 1,
        ISCOUNTER = 1 << 2,
        ISCOUNTERATTACK = 1 << 3
    }
    private EMonsterAttackState currentState = EMonsterAttackState.NONE;

    private bool isFirstAnimCheckIdle = true;
    private bool canAttackSlash = true;
    private bool canAttackSatb = true;
    private bool canAttackCounter = true;
    private bool canAnimMoveToBreak = true;
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
        stat.moveSpeed = originalMoveSpeed;
        stat.runSpeed = originalRunSpeed;
    }

    private void Start()
    {
        runPosition = stat.enemyRoamingRange;
        originalMoveSpeed = stat.moveSpeed;
        originalRunSpeed = stat.runSpeed;

        monsterRunleftPosition.y = transform.position.y;
        monsterRunRightPosition.y = transform.position.y;

        monsterRunleftPosition.x = transform.position.x;
        monsterRunRightPosition.x = transform.position.x;
        monsterRunleftPosition.x += runPosition;
        monsterRunRightPosition.x -= runPosition;

        originalLayer = LayerMask.GetMask("Enemy");
        colorVisibleLayer = LayerMask.GetMask("ColorEnemy");

        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, stat.enemyColor);
        PlayManager.Instance.FilterColorAttackEvent.AddListener(IsActiveColor);
        monsterPosition = monsterRunRightPosition;
        startAntPosition = ((Vector2)transform.position);

        CheckStateChange();

        if (animationJson is not null)
        {
            jsonObject = JObject.Parse(animationJson.text);
        }
    }
    private void Update()
    {
        if (canAttack && !IsStateActive(EMonsterState.isBattle))
        {
            StartCoroutine(CheckPlayer(startAntPosition));
        }
        if (canAttack && distanceToMonsterStartPos >= stat.enemyRoamingRange && !state.HasFlag(EMonsterState.isWait))
        {
            SetState(EMonsterState.isWait, true);
            SetState(EMonsterState.isBattle, false);
            SetState(EMonsterState.isPlayerBetween, false);
            CheckStateChange();
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
                AsyncAnimation(aniClip[(int)EAnimState.DETECTION], true, timeScale);
                break;
            case EAnimState.IDLE:
                AsyncAnimation(aniClip[(int)EAnimState.IDLE], true, timeScale);
                break;
            case EAnimState.WALK:
                AsyncAnimation(aniClip[(int)EAnimState.WALK], true, timeScale);
                break;
            case EAnimState.STAB:
                AsyncAnimation(aniClip[(int)EAnimState.STAB], false, timeScale);
                break;
            case EAnimState.SLASH:
                AsyncAnimation(aniClip[(int)EAnimState.SLASH], false, timeScale);
                break;
            case EAnimState.COUNTER:
                AsyncAnimation(aniClip[(int)EAnimState.COUNTER], false, timeScale);
                break;
            case EAnimState.COUNTERATTACK:
                AsyncAnimation(aniClip[(int)EAnimState.COUNTERATTACK], false, timeScale);
                break;
            case EAnimState.DEAD:
                AsyncAnimation(aniClip[(int)EAnimState.DEAD], false, timeScale);
                break;
            case EAnimState.MOVETOBREAK:
                AsyncAnimation(aniClip[(int)EAnimState.MOVETOBREAK], false, timeScale);
                break;
            case EAnimState.FLIP:
                AsyncAnimation(aniClip[(int)EAnimState.FLIP], false, timeScale);
                break;
            case EAnimState.SPEAROVER:
                AsyncAnimation(aniClip[(int)EAnimState.SPEAROVER], false, timeScale);
                break;
            case EAnimState.COUNTEROFF:
                AsyncAnimation(aniClip[(int)EAnimState.COUNTEROFF], false, timeScale);
                break;
            case EAnimState.COUNTERTRIGGER:
                AsyncAnimation(aniClip[(int)EAnimState.COUNTERTRIGGER], false, timeScale);
                break;
            case EAnimState.ENEMYDISCOVERY:
                AsyncAnimation(aniClip[(int)EAnimState.ENEMYDISCOVERY], false, timeScale);
                break;
            case EAnimState.BATTLEIDLE:
                AsyncAnimation(aniClip[(int)EAnimState.BATTLEIDLE], true, timeScale);
                break;
        }
    }

    private void IsActiveColor(eActivableColor color)
    {
        int newLayer = SOO.Util.LayerMaskToNumber((color == stat.enemyColor) ? colorVisibleLayer : originalLayer);
        newLayer -= 2;
        gameObject.layer = newLayer;
    }

    public override IEnumerator CheckPlayer(Vector2 startMonsterPos)
    {
        distanceToPlayer = Vector2.Distance(transform.position, PlayerPos);
        distanceToStartPos = Vector2.Distance(startMonsterPos, PlayerPos);
        if (distanceToStartPos <= runPosition && !IsStateActive(EMonsterState.isBattle) && canAttack)
        {
            if (isFirstAnimCheckIdle)
            {
                animState = EAnimState.ENEMYDISCOVERY;
                SetCurrentAnimation(animState);
                ///<summary>  FSM상 모든 상태를 일시적으로 종료시키고 애니메이션 대기 시간 이후 다시 FSM을 True를 시키는 방식입니다. ///</summary>
                SetState(EMonsterState.isWait, false);
                SetState(EMonsterState.isBattle, false);
                stat.moveSpeed = moveSpeedDown;
                stat.runSpeed = moveSpeedDown;
                elapsedTime = 0f;
                yield return Yields.WaitSeconds(stat.discoveryDuration);
                SetState(EMonsterState.isPlayerBetween, true);
                stat.moveSpeed = originalMoveSpeed;
                stat.runSpeed = originalRunSpeed;
                CheckStateChange();
                isFirstAnimCheckIdle = false;
            }
            elapsedTime = 0f;
            CheckStateChange();
        }
        else
        {
            StartCoroutine(CheckWaitTime());
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
            CheckStateChange();
            isFirstAnimCheckIdle = true;
            animState = EAnimState.SPEAROVER;
            SetCurrentAnimation(animState);
            CheckStateChange();
            yield break;
        }
    }
    public override void WaitSituation()
    {
        currentHP = stat.MonsterHP;
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
    public override void MoveToPlayer()
    {
        if (ReferenceEquals(PlayerPos, null))
        {
            return;
        }
        CheckStateChange();
        float horizontalValue = PlayerPos.x - transform.position.x;
        transform.localScale = (horizontalValue >= 0) ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
        if (distanceToPlayer <= stat.senseCircle && !IsStateActive(EMonsterState.isBattle))
        {
            SetState(EMonsterState.isBattle, true);
            SetState(EMonsterState.isPlayerBetween, false);
            CheckStateChange();
        }
        else if (distanceToPlayer > stat.senseCircle && !IsStateActive(EMonsterState.isBattle) && canAttack)
        {
            transform.position = Vector2.MoveTowards(transform.position, PlayerPos, stat.runSpeed * Time.deltaTime);
        }
    }
    public override void Attack()
    {
        if (isDead)
        {
            return;
        }
        CheckStateChange();
        if (Vector2.Distance(transform.position, PlayerPos) >= stat.senseCircle && canAttack)
        {
            SetState(EMonsterState.isPlayerBetween, true);
            SetState(EMonsterState.isBattle, false);
        }
        else if (!currentState.HasFlag(EMonsterAttackState.ISATTACK) && canAttack)
        {
            StartCoroutine(AttackSequence(PlayerPos));
            if (canAnimMoveToBreak)
            {
                animState = EAnimState.MOVETOBREAK;
                SetCurrentAnimation(animState);
            }
        }
    }

    private IEnumerator AttackSequence(Vector2 attackAngle)
    {
        currentState |= EMonsterAttackState.ISATTACK;
        canAttack = false;
        Vector2 playerLocationFromMonster = attackAngle - (Vector2)transform.position;
        Vector2 reboundDirCheck;
        if (playerLocationFromMonster.x <= 0)
        {
            transform.localScale = new Vector2(1, 1);
            reboundDirCheck = new Vector2(-1f, 0);
            stat.projectileZAngleByHeight = rightZAngle;
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
            reboundDirCheck = new Vector2(1f, 0);
            stat.projectileZAngleByHeight = leftZAngle;
        }
        animState = EAnimState.DETECTION;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds(stat.attackDelay);
        playerLocationFromMonster = attackAngle - (Vector2)transform.position;
        if (!isDead) // FIX 구조 개편 예정. 현재 똑같은 패턴 사용 불가능하게 하기 위해 임시로 처리해둠
        {
            int checkRandomAttackType = UnityEngine.Random.Range(1, 101);
            if (!canAttackSlash)
            {
                checkRandomAttackType = UnityEngine.Random.Range(stat.slashAttackPercent, 101);
            }
            else if (!canAttackCounter)
            {
                checkRandomAttackType = UnityEngine.Random.Range(1, 101 - stat.slashAttackPercent);
            }

            if (!canAttackSatb)
            {
                checkRandomAttackType = UnityEngine.Random.Range(1, 101);
                if (checkRandomAttackType >= 50)
                {
                    StartCoroutine(SlashAttack(playerLocationFromMonster, reboundDirCheck));
                }
                else
                {
                    StartCoroutine(CounterAttackStart());
                }
            }
            else if (checkRandomAttackType <= stat.slashAttackPercent)
            {
                StartCoroutine(SlashAttack(playerLocationFromMonster, reboundDirCheck));
            }
            else if (checkRandomAttackType >= stat.stabAttackPercent && stat.stabAttackPercent + stat.slashAttackPercent >= checkRandomAttackType)
            {
                animState = EAnimState.STAB;
                SetCurrentAnimation(animState);
                StartCoroutine(StabAttack());
            }
            else
            {
                StartCoroutine(CounterAttackStart());
            }

            yield return Yields.WaitSeconds(stat.attackTime);
            currentState &= ~EMonsterAttackState.ISATTACK;
            yield return Yields.WaitSeconds(stat.attackCooldown);
            animState = EAnimState.BATTLEIDLE;
            SetCurrentAnimation(animState);
            canAttack = true;
        }
    }

    private IEnumerator SlashAttack(Vector2 playerLocationFromMonster,Vector2 check)
    {
        float ZAngle;
        Vector2 attackAngle;
        if (playerLocationFromMonster.x <= 0)
        {
            attackAngle = new(-1, 0);
            ZAngle = 0;
        }
        else
        {
            attackAngle = new(1, 0);
            ZAngle = 180;
        }
        if (IsStateActive(EMonsterState.isWait))
        {
            yield return null;
        }
        canAttackSatb = true;
        canAttackSlash = false;
        canAttackCounter = true;

        animState = EAnimState.SLASH;
        SetCurrentAnimation(animState);

        rigid.AddForce(check * stat.slashAttackRebound, ForceMode2D.Impulse);
        yield return Yields.WaitSeconds(stat.slashAttackDelay);
        SlashAttackObject.SetActive(true);
        yield return Yields.WaitSeconds(stat.slashAttackTime);
        SlashAttackObject.SetActive(false);

        GameObject projectileObj = ObjectPoolManager.instance.GetProjectileFromPool(1);
        if (projectileObj is not null)
        {
            projectileObj.SetActive(true);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile is not null)
            {
                projectile.Shot(gameObject, attackTransform.transform.position, attackAngle,
                    stat.swordAuraRangePerTime, stat.swordAttackSpeed, stat.swordAttackDamage, ZAngle, eActivableColor.RED);
                projectileObj.transform.position = attackTransform.transform.position;
                PlayManager.Instance.UpdateColorthing();
                projectile.ReturnStartRoutine(stat.swordAuraRangePerTime);
            }
        }
        yield return Yields.WaitSeconds(stat.slashAttackDelay);
    }

    private IEnumerator StabAttack()
    {
        if (isDead)
        {
            yield break;
        }
        canAttackSatb = false;
        canAttackSlash = true;
        canAttackCounter = true;
        currentState |= EMonsterAttackState.ISSTABATTACK;
        while (currentState.HasFlag(EMonsterAttackState.ISSTABATTACK))
        {
            currentState &= ~EMonsterAttackState.ISSTABATTACK;
            for (int i = 0; i < 3; i += 1)
            {
                if (i == 2)
                {
                    StartCoroutine(ActivateObjects(stabAttackObject, i, i + 1, true, true));
                    yield return new WaitForSeconds(stabDelayToAttack);
                    StartCoroutine(ActivateObjects(stabAttackObject, i, i + 1, false, true));
                    yield return new WaitForSeconds(stabDelayToDestory);
                }
                else
                {
                    StartCoroutine(ActivateObjects(stabAttackObject, i, i + 1, true, false));
                    yield return new WaitForSeconds(stabDelayToAttack);
                    StartCoroutine(ActivateObjects(stabAttackObject, i, i + 1, false, false));
                    yield return new WaitForSeconds(stabDelayToDestory);
                }
            }
        }
    }
    private IEnumerator ActivateObjects(GameObject[] objects, int startIndex, int endIndex, bool isSet, bool lastAttack)
    {
        for (int i = startIndex; i < endIndex; i++)
        {
            if (objects[i] is not null)
            {
                if (!lastAttack)
                {
                    yield return Yields.WaitSeconds((float)jsonObject["animations"]["ground_ant/ground_ant_battle/ground_ant_battle_stabbing/ground_ant_battle_stabbing_full/ground_ant_battle_stabbing_full"]["events"][0]["time"]);
                    objects[i].SetActive(isSet);
                }
                else
                {
                    yield return Yields.WaitSeconds((float)jsonObject["animations"]["ground_ant/ground_ant_battle/ground_ant_battle_stabbing/ground_ant_battle_stabbing_full/ground_ant_battle_stabbing_full"]["events"][4]["time"] - (float)jsonObject["animations"]["ground_ant/ground_ant_battle/ground_ant_battle_stabbing/ground_ant_battle_stabbing_full/ground_ant_battle_stabbing_full"]["events"][0]["time"] - 0.1f);
                    objects[i].SetActive(isSet);
                    objects[i].SetActive(isSet);
                }
            }
        }
    }
    private IEnumerator CounterAttackStart()
    {
        if (isDead)
        {
            yield break;
        }
        canAttackSatb = true;
        canAttackSlash = true;
        canAttackCounter = false;
        animState = EAnimState.COUNTER;
        SetCurrentAnimation(animState);
        currentState |= EMonsterAttackState.ISCOUNTER;
        yield return Yields.WaitSeconds(stat.counterDurationTime);
        if (currentState.HasFlag(EMonsterAttackState.ISCOUNTER))
        {
            animState = EAnimState.COUNTEROFF;
            SetCurrentAnimation(animState);
            currentState &= ~EMonsterAttackState.ISCOUNTER;
        }
    }
    private IEnumerator CounterAttackPlay(Vector2 dir, float ZAngle)
    {
        if (!isDead)
        {
            yield return Yields.WaitSeconds((float)jsonObject["animations"]["ground_ant/ground_ant_battle/ground_ant_battle_parrying/ground_ant_battle_parrying"]["events"][0]["time"] + 0.14f);
            animState = EAnimState.COUNTERATTACK;
            SetCurrentAnimation(animState);
            currentState |= EMonsterAttackState.ISCOUNTERATTACK;
            yield return Yields.WaitSeconds((float)jsonObject["animations"]["ground_ant/ground_ant_battle/ground_ant_battle_parrying/ground_ant_battle_parrying"]["events"][0]["time"]);
            counterAttackObject.SetActive(true);
            yield return Yields.WaitSeconds(stat.counterAttackDurationTime);
            counterAttackObject.SetActive(false);
            currentState &= ~EMonsterAttackState.ISCOUNTERATTACK;
            currentState &= ~EMonsterAttackState.ISCOUNTER;
        }
    }
    public override void Hit(int damage, int colorDamage, Vector2 attackDir, IParryConditionCheck parryCheck = null)
    {
        if (isDead)
        {
            return;
        }
        if (currentState.HasFlag(EMonsterAttackState.ISCOUNTER))
        {
            animState = EAnimState.COUNTERTRIGGER;
            SetCurrentAnimation(animState);
            StartCoroutine(CounterAttackPlay(PlayerPos - (Vector2)transform.position, Mathf.Atan2(PlayerPos.x - transform.position.x, PlayerPos.y - transform.position.y) * Mathf.Rad2Deg));
            currentState &= ~EMonsterAttackState.ISATTACK;
        }

        else if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
        {
            currentHP -= colorDamage;
            rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
        }
        else
        {
            currentHP -= damage;
            rigid.AddForce(attackDir * stat.hitReboundPower, ForceMode2D.Impulse);
        }
        CheckDead();
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
        StopCoroutine(AttackSequence(PlayerPos));
        animState = EAnimState.DEAD;
        SetCurrentAnimation(animState);
        rigid.mass = DeadMass;

        yield return new WaitForSeconds(stat.deadDelay);
        rigid.mass = originalMass;
        gameObject.SetActive(false);
    }
    public override void CheckStateChange()
    {
        if (isDead)
        {
            return;
        }
        switch (state)
        {
            case EMonsterState.isBattle:
                fsm.ChangeState("Attack");
                break;
            case EMonsterState.isPlayerBetween:
                fsm.ChangeState("Chase");
                animState = EAnimState.WALK;
                SetCurrentAnimation(animState);
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
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead)
        {
            return;
        }
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            collision.gameObject.GetComponent<Player>().Hit(stat.contactDamage, stat.contactDamage,
                    transform.position - collision.transform.position, null);
        }
    }
    public bool CanParryAttack()
    {
        return PlayManager.Instance.ContainsActivationColors(stat.enemyColor);
    }
    private void OnDrawGizmos()
    {
        if (null != stat)
        {
            Gizmos.color = stat.senseCircle >= distanceToStartPos ? UnityEngine.Color.red : UnityEngine.Color.green;
            Gizmos.DrawWireSphere(transform.position + transform.forward, stat.senseCircle);
        }
    }

    //public override void Respawn(GameObject monsterPos, bool isRespawnMonster)
    //{
    //    throw new NotImplementedException();
    //}
}
