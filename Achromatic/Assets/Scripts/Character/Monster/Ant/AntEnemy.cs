using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
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

    private LayerMask originLayer;
    private LayerMask colorVisibleLayer;
    [SerializeField]
    private JObject jsonObject;

    [Header("Animation")]
    [SerializeField]
    private SkeletonAnimation skeletonAnimation;
    [SerializeField]
    private AnimationReferenceAsset[] aniClip;
    [SerializeField]
    private TextAsset animationJson;
    public enum EanimState
    {
        detection,
        Idle,
        Walk,
        Stab,
        Sword,
        Counter,
        CounterAttack,
        Dead,
        Break,
        Filp,
        Over,
        Off,
        Trigger,
        EnemyDisco
    }
    private EanimState animState;

    public Vector2 startAntPosition;

    public UnityEvent<eActivableColor> antColorEvent;

    private string currentAnimation;
    private float stabDelayToAttack = 0.2f;
    private float stabDelayToDestory = 0.05f;
    private enum EMonsterAttackState
    {
        None = 0,
        IsAttack = 1 << 0,
        isStabAttack = 1 << 1,
        isCounter = 1 << 2,
        isCounterAttack = 1 << 3
    }
    private EMonsterAttackState currentState = EMonsterAttackState.None;

    private bool isHeavy = false;
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
    }

    private void Start()
    {
        monsterRunleftPosition.y = transform.position.y;
        monsterRunRightPosition.y = transform.position.y;
        monsterRunleftPosition.x += transform.position.x + runPosition;
        monsterRunRightPosition.x += transform.position.x - runPosition;

        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, stat.enemyColor);
        MonsterManager.Instance?.GetColorEvent.AddListener(CheckIsHeavy);
        PlayManager.Instance.FilterColorAttackEvent.AddListener(IsActiveColor);
        PlayManager.Instance.UpdateColorthing();
        monsterPosition = monsterRunRightPosition;
        startAntPosition = new Vector2(transform.position.x, transform.position.y);

        originLayer = gameObject.layer;
        colorVisibleLayer = LayerMask.NameToLayer("ColorEnemy");

        if (animationJson != null)
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
        SetCurrentAnimation(animState);
    }
    private void SetCurrentAnimation(EanimState _state)
    {
        float timeScale = 1;
        switch (_state)
        {
            case EanimState.detection:
                AsyncAnimation(aniClip[(int)EanimState.detection], true, timeScale);
                break;
            case EanimState.Idle:
                AsyncAnimation(aniClip[(int)EanimState.Idle], true, timeScale);
                break;
            case EanimState.Walk:
                AsyncAnimation(aniClip[(int)EanimState.Walk], true, timeScale);
                break;
            case EanimState.Stab:
                AsyncAnimation(aniClip[(int)EanimState.Stab], false, timeScale);
                break;
            case EanimState.Sword:
                AsyncAnimation(aniClip[(int)EanimState.Sword], false, timeScale);
                break;
            case EanimState.Counter:
                AsyncAnimation(aniClip[(int)EanimState.Counter], false, timeScale);
                break;
            case EanimState.CounterAttack:
                AsyncAnimation(aniClip[(int)EanimState.CounterAttack], false, timeScale);
                break;
            case EanimState.Dead:
                AsyncAnimation(aniClip[(int)EanimState.Dead], false, timeScale);
                break;
            case EanimState.Break:
                AsyncAnimation(aniClip[(int)EanimState.Break], false, timeScale);
                break;
            case EanimState.Filp:
                AsyncAnimation(aniClip[(int)EanimState.Filp], false, timeScale);
                break;
            case EanimState.Over:
                AsyncAnimation(aniClip[(int)EanimState.Over], false, timeScale);
                break;
            case EanimState.Off:
                AsyncAnimation(aniClip[(int)EanimState.Off], false, timeScale);
                break;
            case EanimState.Trigger:
                AsyncAnimation(aniClip[(int)EanimState.Trigger], false, timeScale);
                break;
            case EanimState.EnemyDisco:
                AsyncAnimation(aniClip[(int)EanimState.EnemyDisco], false, timeScale);
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
    private void CheckIsHeavy(eActivableColor color)
    {
        if (color == stat.enemyColor)
        {
            isHeavy = false;
        }
        antColorEvent?.Invoke(color);
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

    public override IEnumerator CheckPlayer(Vector2 startMonsterPos)
    {
        distanceToPlayer = Vector2.Distance(transform.position, PlayerPos);
        distanceToStartPos = Vector2.Distance(startMonsterPos, PlayerPos);
        if (distanceToStartPos <= runPosition && !IsStateActive(EMonsterState.isBattle) && canAttack)
        {
            if (isFirstAnimCheckIdle)
            {
                animState = EanimState.EnemyDisco;
                SetCurrentAnimation(animState);
                SetState(EMonsterState.isWait, false);
                SetState(EMonsterState.isBattle, false);
                stat.moveSpeed = 0; // FIX 아래 매직넘버들 죄다 수정 예정
                stat.runSpeed = 0;
                CheckStateChange();
                yield return Yields.WaitSeconds(1f);
                SetState(EMonsterState.isPlayerBetween, true);
                stat.moveSpeed = 1; 
                stat.runSpeed = 2;
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
        if (elapsedTime >= baseStat.timeToWait && !IsStateActive(EMonsterState.isWait) && !IsStateActive(EMonsterState.isBattle) && canAttack)
        {
            elapsedTime = 0f;
            SetState(EMonsterState.isWait, true);
            SetState(EMonsterState.isPlayerBetween, false);
            SetState(EMonsterState.isBattle, false);
            isFirstAnimCheckIdle = true;
            animState = EanimState.Over;
            SetCurrentAnimation(animState);
            CheckStateChange();
            yield return Yields.WaitSeconds(1.0f);
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
            animState = EanimState.Filp;
            SetCurrentAnimation(animState);
        }
        else if (monsterPosition == monsterRunRightPosition)
        {
            transform.localScale = new Vector3(1, 1, 1);
            animState = EanimState.Filp;
            SetCurrentAnimation(animState);
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
    public override void Attack()
    {
        if (isDead)
        {
            return;
        }
        if (Vector2.Distance(transform.position, PlayerPos) >= stat.senseCircle && canAttack)
        {
            SetState(EMonsterState.isBattle, false);
            SetState(EMonsterState.isPlayerBetween, true);
            SetState(EMonsterState.isWait, false);
        }
        else if (!currentState.HasFlag(EMonsterAttackState.IsAttack) && canAttack)
        {
            StartCoroutine(AttackSequence(PlayerPos));
        }
    }

    private IEnumerator AttackSequence(Vector2 attackAngle)
    {
        currentState |= EMonsterAttackState.IsAttack;
        canAttack = false;
        Vector2 value = new Vector2(attackAngle.x - transform.position.x, attackAngle.y - transform.position.y);
        Vector2 check;
        if (value.x <= 0)
        {
            transform.localScale = new Vector2(1, 1);
            check = new Vector2(-1f, 0);
            animState = EanimState.Filp;
            SetCurrentAnimation(animState);
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
            check = new Vector2(1f, 0);
            animState = EanimState.Filp;
            SetCurrentAnimation(animState);
        }
        animState = EanimState.detection;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds(stat.AttackDelay);
        value = new Vector2(attackAngle.x - transform.position.x, attackAngle.y - transform.position.y);
        float ZAngle = (Mathf.Atan2(attackAngle.y - transform.position.y, attackAngle.x - transform.position.x) * Mathf.Rad2Deg);
        if (isDead)
        {
            yield return null;
        }
        else
        {
            int checkRandomAttackType = UnityEngine.Random.Range(1, 100);
            if (checkRandomAttackType <= stat.swordAttackPercent)
            {
                StartCoroutine(SwordAttack(new Vector2(value.x, -value.y - 0.22f), check, ZAngle));
            }
            else if (checkRandomAttackType >= stat.stabAttackPercent && stat.stabAttackPercent + stat.swordAttackPercent >= checkRandomAttackType)
            {
                animState = EanimState.Stab;
                SetCurrentAnimation(animState);
                StartCoroutine(StabAttack());
            }
            else
            {
                StartCoroutine(CounterAttackStart());
            }

            yield return Yields.WaitSeconds(stat.attackTime);
            currentState &= ~EMonsterAttackState.IsAttack;
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
        animState = EanimState.Sword;
        SetCurrentAnimation(animState);

        rigid.AddForce(check * stat.cuttingAttackRebound, ForceMode2D.Impulse);

        yield return Yields.WaitSeconds(stat.cuttingAttackDelay);
        swordAttackObject.SetActive(true);
        yield return Yields.WaitSeconds(stat.cuttingAttackTime);
        swordAttackObject.SetActive(false);

        GameObject projectileObj = ObjectPoolManager.instance.GetProjectileFromPool(1);
        if (projectileObj != null)
        {
            projectileObj.SetActive(true);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Shot(gameObject, attackTransform.transform.position, dir.normalized,
                    stat.swordAttackRangeTime, stat.swordAttackSpeed, stat.swordAttackDamage, isHeavy, -ZAngle, eActivableColor.RED);
                projectileObj.transform.position = attackTransform.transform.position;
                PlayManager.Instance.UpdateColorthing();
                projectile.ReturnStartRoutine(stat.swordAttackRangeTime);
            }
        }
        yield return Yields.WaitSeconds(stat.cuttingAttackDelay);
    }

    private IEnumerator StabAttack()
    {
        if (IsStateActive(EMonsterState.isWait))
        {
            yield break;
        }
        if (!isDead)
        {
            currentState |= EMonsterAttackState.isStabAttack;
            while (currentState.HasFlag(EMonsterAttackState.isStabAttack))
            {
                currentState &= ~EMonsterAttackState.isStabAttack;
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
    }
    private IEnumerator ActivateObjects(GameObject[] objects, int startIndex, int endIndex, bool isSet, bool lastAttack)
    {
        for (int i = startIndex; i < endIndex; i++)
        {
            if (objects[i] != null)
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
            animState = EanimState.Counter;
            SetCurrentAnimation(animState);
            currentState |= EMonsterAttackState.isCounter;
            yield return Yields.WaitSeconds(stat.counterAttackTime);
            if (currentState.HasFlag(EMonsterAttackState.isCounter))
            {
                animState = EanimState.Off;
                SetCurrentAnimation(animState);
                currentState &= ~EMonsterAttackState.isCounter;
                yield return Yields.WaitSeconds(0.2f);
            }
        }
    }
    private IEnumerator CounterAttackPlay(Vector2 dir, float ZAngle)
    {
        if (!isDead)
        {
            yield return Yields.WaitSeconds((float)jsonObject["animations"]["ground_ant/ground_ant_battle/ground_ant_battle_parrying/ground_ant_battle_parrying"]["events"][0]["time"] + 0.14f);
            animState = EanimState.CounterAttack;
            SetCurrentAnimation(animState);
            currentState |= EMonsterAttackState.isCounterAttack;
            yield return Yields.WaitSeconds((float)jsonObject["animations"]["ground_ant/ground_ant_battle/ground_ant_battle_parrying/ground_ant_battle_parrying"]["events"][0]["time"]);
            counterAttackObject.SetActive(true);
            yield return Yields.WaitSeconds(stat.counterAttackPlayTime);
            counterAttackObject.SetActive(false);
            currentState &= ~EMonsterAttackState.isCounterAttack;
            currentState &= ~EMonsterAttackState.isCounter;
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
        animState = EanimState.Dead;
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
        else
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
                    animState = EanimState.Idle;
                    SetCurrentAnimation(animState);
                    break;
                default:
                    break;
            }
        }
    }
    public override void Hit(int damage, int colorDamage, Vector2 attackDir, IParryConditionCheck parryCheck = null)
    {
        if (isDead)
        {
            return;
        }
        if (currentState.HasFlag(EMonsterAttackState.isCounter))
        {
            animState = EanimState.Trigger;
            SetCurrentAnimation(animState);
            StartCoroutine(CounterAttackPlay(new Vector2(PlayerPos.x - transform.position.x, PlayerPos.y - transform.position.y), Mathf.Atan2(PlayerPos.x - transform.position.x, PlayerPos.y - transform.position.y) * Mathf.Rad2Deg));
            currentState &= ~EMonsterAttackState.IsAttack;
        }

        else if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
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
    private void OnDrawmos()
    {
        if (null != stat)
        {
            if (stat.senseCircle >= distanceToStartPos)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawWireSphere(transform.position + transform.forward, stat.senseCircle);
        }
    }

    //public override void Respawn(GameObject monsterPos, bool isRespawnMonster)
    //{
    //    throw new NotImplementedException();
    //}
}
