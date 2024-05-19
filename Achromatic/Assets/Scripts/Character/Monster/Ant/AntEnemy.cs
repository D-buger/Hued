using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static SpiderEnemy;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using static UnityEngine.Rendering.DebugUI;

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
    private GameObject swordAttackObject;
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
        SWORD,
        COUNTER,
        COUNTERATTACK,
        DEAD,
        MOVETOBREAK,
        FLIP,
        SPEAROVER,
        COUNTEROFF,
        COUNTERTRIGGER,
        ENEMYDISCOVERY
    }
    [SerializeField]
    private EAnimState animState;

    public Vector2 startAntPosition;

    public UnityEvent<eActivableColor> antColorEvent;

    private string currentAnimation;
    private float stabDelayToAttack = 0.2f;
    private float stabDelayToDestory = 0.05f;
    private float originalMoveSpeed = 1;
    private float originalRunSpeed = 2;
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
        originalMoveSpeed = stat.moveSpeed;
        originalRunSpeed = stat.runSpeed;

        monsterRunleftPosition.y = transform.position.y;
        monsterRunRightPosition.y = transform.position.y;
        monsterRunleftPosition.x += transform.position.x + runPosition;
        monsterRunRightPosition.x += transform.position.x - runPosition;

        runPosition = stat.enemyRoamingRange;

        originalLayer = LayerMask.GetMask("Enemy");
        colorVisibleLayer = LayerMask.GetMask("ColorEnemy");

        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, stat.enemyColor);
        PlayManager.Instance.FilterColorAttackEvent.AddListener(IsActiveColor);
        PlayManager.Instance.UpdateColorthing();
        monsterPosition = monsterRunRightPosition;
        startAntPosition = new Vector2(transform.position.x, transform.position.y);

        if (animationJson is not null)
        {
            jsonObject = JObject.Parse(animationJson.text);
        }
    }
    private void Update()
    {
        StartCoroutine(CheckPlayer(startAntPosition));
        if (canAttack && IsStateActive(EMonsterState.isBattle))
        {
            Attack();
        }
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
            case EAnimState.SWORD:
                AsyncAnimation(aniClip[(int)EAnimState.SWORD], false, timeScale);
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
                ///<summary>  FSM�� ��� ���¸� �Ͻ������� �����Ű�� �ִϸ��̼� ��� �ð� ���� �ٽ� FSM�� True�� ��Ű�� ����Դϴ�. ///</summary>
                SetState(EMonsterState.isWait, false);
                SetState(EMonsterState.isBattle, false);
                stat.moveSpeed = moveSpeedDown;
                stat.runSpeed = moveSpeedDown;
                CheckStateChange();
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
            isFirstAnimCheckIdle = true;
            animState = EAnimState.SPEAROVER;
            SetCurrentAnimation(animState);
            CheckStateChange();
            yield return Yields.WaitSeconds(1.0f);
        }
    }
    public override void WaitSituation()
    {
        currentHP = stat.MonsterHP;
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
        float horizontalValue = PlayerPos.x - transform.position.x;
        transform.localScale = (horizontalValue >= 0) ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
        if (distanceToPlayer <= stat.senseCircle && !IsStateActive(EMonsterState.isBattle))
        {
            SetState(EMonsterState.isBattle, true);
            CheckStateChange();
        }
        else if (distanceToPlayer > stat.senseCircle && !IsStateActive(EMonsterState.isBattle) && canAttack)
        {
            transform.position = Vector2.MoveTowards(transform.position, PlayerPos, stat.moveSpeed * Time.deltaTime);
        }
    }
    public override void Attack()
    {
        if (isDead)
        {
            return;
        }
        if (Vector2.Distance(transform.position, PlayerPos) >= stat.senseCircle && canAttack)
        {
            SetState(EMonsterState.isPlayerBetween, true);
        }
        else if (!currentState.HasFlag(EMonsterAttackState.ISATTACK) && canAttack)
        {
            StartCoroutine(AttackSequence(PlayerPos));
        }
    }

    private IEnumerator AttackSequence(Vector2 attackAngle)
    {
        currentState |= EMonsterAttackState.ISATTACK;
        canAttack = false;
        Vector2 value = new Vector2(attackAngle.x - transform.position.x, attackAngle.y - transform.position.y);
        Vector2 reboundDirCheck;
        if (value.x <= 0)
        {
            transform.localScale = new Vector2(1, 1);
            reboundDirCheck = new Vector2(-1f, 0);
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
            reboundDirCheck = new Vector2(1f, 0);
        }
        animState = EAnimState.DETECTION;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds(stat.attackDelay);
        value = new Vector2(attackAngle.x - transform.position.x, attackAngle.y - transform.position.y);
        float ZAngle = (Mathf.Atan2(attackAngle.y - transform.position.y, attackAngle.x - transform.position.x) * Mathf.Rad2Deg);
        if (!isDead)
        {
            int checkRandomAttackType = UnityEngine.Random.Range(1, 101);
            if (checkRandomAttackType <= stat.swordAttackPercent)
            {
                StartCoroutine(SwordAttack(new Vector2(value.x, -value.y - stat.projectileAnglebyHeight), reboundDirCheck, ZAngle));
            }
            else if (checkRandomAttackType >= stat.stabAttackPercent && stat.stabAttackPercent + stat.swordAttackPercent >= checkRandomAttackType)
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
            canAttack = true;
        }
    }

    private IEnumerator SwordAttack(Vector2 dir, Vector2 check, float ZAngle)
    {
        if (IsStateActive(EMonsterState.isWait))
        {
            yield return null;
        }
        animState = EAnimState.SWORD;
        SetCurrentAnimation(animState);

        rigid.AddForce(check * stat.slashAttackRebound, ForceMode2D.Impulse);

        yield return Yields.WaitSeconds(stat.slashAttackDelay);
        swordAttackObject.SetActive(true);
        yield return Yields.WaitSeconds(stat.slashAttackTime);
        swordAttackObject.SetActive(false);

        GameObject projectileObj = ObjectPoolManager.instance.GetProjectileFromPool(1);
        if (projectileObj is not null)
        {
            projectileObj.SetActive(true);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile is not null)
            {
                projectile.Shot(gameObject, attackTransform.transform.position, dir.normalized,
                    stat.swordAuraRangePerTime, stat.swordAttackSpeed, stat.swordAttackDamage, -ZAngle, eActivableColor.RED);
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
        if (!isDead)
        {
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

        yield return new WaitForSeconds(stat.deadDelay);
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
            StartCoroutine(CounterAttackPlay(new Vector2(PlayerPos.x - transform.position.x, PlayerPos.y - transform.position.y), Mathf.Atan2(PlayerPos.x - transform.position.x, PlayerPos.y - transform.position.y) * Mathf.Rad2Deg));
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