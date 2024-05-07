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
    private GameObject[] stabAttackOBJ;

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
        Idle,
        Walk,
        Stab,
        Sword,
        Counter,
        Detection,
        Dead
    }
    private EanimState animState;

    public Vector2 startSpiderPosition;

    public Vector3 gizmoLeftPos;

    public UnityEvent<eActivableColor> antColorEvent;

    private string currentAnimation;
    private float angleThreshold = 52f;

    private enum EMonsterAttackState
    {
        None = 0,
        IsAttack = 1 << 0,
        isStabAttack = 1 << 1,
        isCounter = 1 << 2
    }
    private EMonsterAttackState currentState = EMonsterAttackState.None;

    private bool isHeavy = false;

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
        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, stat.enemyColor);
        monsterRunleftPosition.y = transform.position.y;
        monsterRunRightPosition.y = transform.position.y;
        monsterRunleftPosition.x += transform.position.x + runPosition;
        monsterRunRightPosition.x += transform.position.x - runPosition;
        currentHP = stat.MonsterHP;
        monsterPosition = monsterRunRightPosition;
        startSpiderPosition = new Vector2(gizmoLeftPos.x - runPosition, transform.position.y);

        originLayer = gameObject.layer;
        colorVisibleLayer = LayerMask.NameToLayer("ColorEnemy");

        MonsterManager.Instance?.GetColorEvent.AddListener(CheckIsHeavy);
        PlayManager.Instance.FilterColorAttackEvent.AddListener(IsActiveColor);
        PlayManager.Instance.UpdateColorthing();

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
        SetCurrentAnimation(animState);
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
            case EanimState.Stab:
                AsyncAnimation(aniClip[(int)EanimState.Stab], false, timeScale);
                break;
            case EanimState.Sword:
                AsyncAnimation(aniClip[(int)EanimState.Sword], false, timeScale);
                break;
            case EanimState.Counter:
                AsyncAnimation(aniClip[(int)EanimState.Counter], false, timeScale);
                break;
            case EanimState.Detection:
                AsyncAnimation(aniClip[(int)EanimState.Detection], false, timeScale);
                break;
            case EanimState.Dead:
                AsyncAnimation(aniClip[(int)EanimState.Dead], false, timeScale);
                break;
        }
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
        float horizontalValue = attackAngle.x - transform.position.x;
        float verticalValue = attackAngle.y - transform.position.y;
        float ZAngle = (Mathf.Atan2(verticalValue, horizontalValue) * Mathf.Rad2Deg);
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
        // TODO 공격 전조 애니메이션
        yield return Yields.WaitSeconds(0.0f); // TODO 공격 실행 이전 대기 시간

        // TODO 공격 패턴 구현

        int checkRandomAttackType = UnityEngine.Random.Range(1, 100);
        Debug.Log(checkRandomAttackType);
        if (checkRandomAttackType <= stat.swordAttackPercent)
        {
            StartCoroutine(SwordAttack(value, check, ZAngle));
            Debug.Log("스워드 공격");
        }
        else if (checkRandomAttackType >= stat.stabAttackPercent && stat.stabAttackPercent + stat.swordAttackPercent >= checkRandomAttackType)
        {
            StartCoroutine(StabAttack(check));
            Debug.Log("창 공격");
        }
        else
        {
            StartCoroutine(CounterAttackStart());
            Debug.Log("카운터 어택");
        }

        yield return Yields.WaitSeconds(stat.attackTime);
        currentState &= ~EMonsterAttackState.IsAttack;
        meleeAttack?.AttackDisable();
        yield return Yields.WaitSeconds(stat.attackCooldown);
        canAttack = true;
    }

    private IEnumerator SwordAttack(Vector2 dir, Vector2 check, float ZAngle)
    {
        if (IsStateActive(EMonsterState.isWait))
        {
            yield return null;
        }
        animState = EanimState.Sword;
        SetCurrentAnimation(animState);
        attackPoint.transform.rotation = Quaternion.Euler(0, 0, ZAngle);
        meleeAttack?.AttackEnable(-dir, stat.attackDamage, stat.attackDamage);
        rigid.AddForce(check * stat.swordAttackRebound, ForceMode2D.Impulse);
        yield return Yields.WaitSeconds(0.0f); // FIX 근접 공격 애니메이션 JSON 파싱
        GameObject projectileObj = ObjectPoolManager.instance.GetProjectileFromPool(1);
        if (projectileObj != null)
        {
            projectileObj.SetActive(true);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Shot(gameObject, attackTransform.transform.position, dir.normalized,
                    stat.swordAttackRange, stat.swordAttackSpeed, stat.swordAttackDamage, isHeavy, ZAngle, eActivableColor.RED);
                projectileObj.transform.position = transform.position;

                PlayManager.Instance.UpdateColorthing();
                projectile.ReturnStartRoutine(stat.swordAttackRange);
            }
        }
        Debug.Log(dir);
    }

    private IEnumerator StabAttack(Vector2 check)
    {
        if (IsStateActive(EMonsterState.isWait))
        {
            yield break;
        }
        currentState |= EMonsterAttackState.isStabAttack;
        float delayToAttack = 0.2f;
        float delayToDestory = 0.05f;
        int objectCount = stabAttackOBJ.Length / 2;

        while (currentState.HasFlag(EMonsterAttackState.isStabAttack))
        {
            currentState &= ~EMonsterAttackState.isStabAttack;
            int satbValue = (check.x > 0) ? satbValue = 3 : satbValue = 0;
            Debug.Log(satbValue);
            objectCount += satbValue;
            for (int i = satbValue; i < objectCount; i += 1)
            {
                if (i % 3 == 0)
                {
                    StartCoroutine(ActivateObjects(stabAttackOBJ, i, i + 1, true, true));
                    yield return new WaitForSeconds(delayToAttack);
                    StartCoroutine(ActivateObjects(stabAttackOBJ, i, i + 1, false, true));
                    yield return new WaitForSeconds(delayToDestory);
                }
                else
                {
                    StartCoroutine(ActivateObjects(stabAttackOBJ, i, i + 1, true, false));
                    yield return new WaitForSeconds(delayToAttack);
                    StartCoroutine(ActivateObjects(stabAttackOBJ, i, i + 1, false, false));
                    yield return new WaitForSeconds(delayToDestory);
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
                    Debug.Log("1,2타 제대로 들어옴");
                    yield return Yields.WaitSeconds((float)jsonObject["animations"]["ground_ant/ground_ant_battle__stabbing/ground_ant_battle_stabbing_full/ground_ant_battle_stabbing_full_2"]["events"][0]["time"]);
                    objects[i].SetActive(isSet);
                }
                else
                {
                    Debug.Log("막타도 제대로 들어옴");
                    yield return Yields.WaitSeconds((float)jsonObject["animations"]["ground_ant/ground_ant_battle__stabbing/ground_ant_battle_stabbing_full/ground_ant_battle_stabbing_full_2"]["events"][4]["time"] - (float)jsonObject["animations"]["ground_ant/ground_ant_battle__stabbing/ground_ant_battle_stabbing_full/ground_ant_battle_stabbing_full_2"]["events"][0]["time"]);
                    objects[i].SetActive(isSet);
                }
            }
        }
    }
    private IEnumerator CounterAttackStart()
    {
        currentState |= EMonsterAttackState.isCounter;
        yield return Yields.WaitSeconds(stat.counterAttackTime);
        currentState &= ~EMonsterAttackState.isCounter;
    }
    private void CounterAttackPlay(Vector2 dir, float ZAngle)
    {
        GameObject projectileObj = ObjectPoolManager.instance.GetProjectileFromPool(0);
        if (projectileObj != null)
        {
            projectileObj.SetActive(true);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Shot(gameObject, attackTransform.transform.position, new Vector2(dir.x, dir.y).normalized,
                    stat.counterAttackRange, stat.counterAttackSpeed, stat.counterAttackDamage, isHeavy, ZAngle, eActivableColor.RED);
                projectileObj.transform.position = transform.position;

                PlayManager.Instance.UpdateColorthing();
                projectile.ReturnStartRoutine(stat.counterAttackRange);
            }
        }
    }
    public override void Dead()
    {
        StartCoroutine(DeadSequence());
    }
    private IEnumerator DeadSequence()
    {
        float deadDelayTime = 1.3f;
        SetState(EMonsterState.isBattle, false);
        SetState(EMonsterState.isWait, false);
        SetState(EMonsterState.isPlayerBetween, false);
        isDead = false;
        StopCoroutine(AttackSequence(PlayerPos));
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
    public override void Hit(int damage, int colorDamage, Vector2 attackDir, IParryConditionCheck parryCheck = null)
    {
        if (currentState.HasFlag(EMonsterAttackState.isCounter))
        {
            CounterAttackPlay(new Vector2(PlayerPos.x - transform.position.x, PlayerPos.y - transform.position.y), Mathf.Atan2(PlayerPos.x - transform.position.x, PlayerPos.y - transform.position.y) * Mathf.Rad2Deg);
            currentState &= ~EMonsterAttackState.IsAttack;
        }

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
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            collision.gameObject.GetComponent<Player>().Hit(stat.contactDamage, stat.contactDamage,
                    transform.position - collision.transform.position, this);
        }
    }
    public bool CanParryAttack()
    {
        return PlayManager.Instance.ContainsActivationColors(stat.enemyColor);
    }
}
