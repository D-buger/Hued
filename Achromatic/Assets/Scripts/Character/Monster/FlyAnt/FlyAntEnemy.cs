using Newtonsoft.Json.Linq;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.Events;
using Color = UnityEngine.Color;

public class FlyAntEnemy : Monster
{
    private MonsterFSM fsm;
    private Rigidbody2D rigid;
    private GameObject attackPoint;
    private Projectile projectile;
    private Attack meleeAttack;

    [SerializeField]
    private GameObject attackTransform;
    [SerializeField]
    private FlyAntMonsterStat stat;
    [SerializeField]
    private JObject jsonObject;

    private Vector2 attackDir;
    private Vector2 startPos;
    public Vector2 startFlyAntPosition;

    public UnityEvent<eActivableColor> flyAntColorEvent;

    private LayerMask originLayer;
    private LayerMask colorVisibleLayer;

    private float deadDelayTime = 1.3f;

    private bool isHeavy = false;

    [Header("Animation")]
    [SerializeField]
    private SkeletonAnimation skeletonAnimation;
    [SerializeField]
    private AnimationReferenceAsset[] aniClip;
    [SerializeField]
    private TextAsset animationJson;

    private enum EMonsterAttackState
    {
        None = 0,
        IsAttack = 1 << 0,
        isBadyAttack = 2 << 0,
        isReturnEnemy = 3 << 0,
        isFirstAttack = 4 << 0,
    }
    private EMonsterAttackState currentState = EMonsterAttackState.None;
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
        attackDir = (PlayerPos - (Vector2)transform.position).normalized;
        startPos = new Vector2(transform.position.x, transform.position.y);
        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, stat.enemyColor);

        monsterRunleftPosition.y = transform.position.y;
        monsterRunRightPosition.y = transform.position.y;
        monsterRunleftPosition.x += transform.position.x + runPosition;
        monsterRunRightPosition.x += transform.position.x - runPosition;

        MonsterManager.Instance?.GetColorEvent.AddListener(CheckIsHeavy);
        PlayManager.Instance.FilterColorAttackEvent.AddListener(IsActiveColor);
        PlayManager.Instance.UpdateColorthing();
        monsterPosition = monsterRunRightPosition;
        startFlyAntPosition = new Vector2(transform.position.x, transform.position.y);

        originLayer = gameObject.layer;
        colorVisibleLayer = LayerMask.NameToLayer("ColorEnemy");

        if (animationJson != null)
        {
            jsonObject = JObject.Parse(animationJson.text);
        }
    }
    private void Update()
    {
        if (currentState.HasFlag(EMonsterAttackState.isBadyAttack))
        {
            Rush(attackDir);
        }
        if (currentState.HasFlag(EMonsterAttackState.isReturnEnemy))
        {
            ReturnMonster();
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
    private void CheckIsHeavy(eActivableColor color)
    {
        if (color == stat.enemyColor)
        {
            isHeavy = false;
        }
        flyAntColorEvent?.Invoke(color);
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
    public override IEnumerator CheckPlayer(Vector2 startMonsterPos)
    {
        distanceToPlayer = Vector2.Distance(transform.position, PlayerPos);
        distanceToStartPos = Vector2.Distance(startMonsterPos, PlayerPos);

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
        else if (canAttack && !currentState.HasFlag(EMonsterAttackState.IsAttack))
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
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
            check = new Vector2(1f, 0);
        }

        yield return Yields.WaitSeconds(stat.AttackDelay);
        float ZAngle = (Mathf.Atan2(attackAngle.x - transform.position.x, attackAngle.y - transform.position.y) * Mathf.Rad2Deg);
        if (currentState.HasFlag(EMonsterAttackState.isFirstAttack))
        {
            int checkRandomAttackType = UnityEngine.Random.Range(1, 100);
            if (checkRandomAttackType < 50)
            {
                StartCoroutine(BadyAttack(attackDir)); // 돌진 공격
            }
            else
            {
                StabThrowAttack(); // 창 던지기 공격
            }
        }

        yield return Yields.WaitSeconds(stat.attackCooldown);
        currentState &= ~EMonsterAttackState.IsAttack;
        meleeAttack?.AttackDisable();
        yield return Yields.WaitSeconds(stat.attackCooldown);
        canAttack = true;
    }

    private IEnumerator BadyAttack(Vector2 direction)
    {
        int checkRandomAttackType = UnityEngine.Random.Range(1, 100);
        yield return Yields.WaitSeconds(stat.flyAntAttackDelay);
        meleeAttack.AttackEnable(direction, stat.badyAttackDamage, stat.badyAttackDamage);
        if (checkRandomAttackType > stat.doubleBadyAttackPer)
        {
            currentState |= EMonsterAttackState.isBadyAttack;
            yield return Yields.WaitSeconds(stat.flyAntAttackDelay);
            currentState |= EMonsterAttackState.isBadyAttack;
        }
        else
        {
            currentState |= EMonsterAttackState.isBadyAttack;
        }
        while (canAttack)
        {
            yield return Yields.WaitSeconds(0.1f); // FIX stat에 추가
        }
    }
    private void Rush(Vector2 direction)
    {
        transform.Translate(direction * stat.badyAttackSpeed * Time.deltaTime);
        if (distanceToPlayer > stat.returnPosValue)
        {
            currentState &= ~EMonsterAttackState.isBadyAttack;
            currentState |= EMonsterAttackState.isReturnEnemy;
        }
    }
    private void StabThrowAttack()
    {
        Vector2 dir = new Vector2(PlayerPos.x - transform.position.x, PlayerPos.y - transform.position.y);
        float ZAngle = (Mathf.Atan2(PlayerPos.x - transform.position.x, PlayerPos.y - transform.position.y) * Mathf.Rad2Deg);
        GameObject projectileObj = ObjectPoolManager.instance.GetProjectileFromPool(2);
        if (projectileObj != null)
        {
            projectileObj.SetActive(true);

            SpearAttack projectile = projectileObj.GetComponent<SpearAttack>();
            if (projectile != null)
            {
                projectile.Shot(gameObject, attackTransform.transform.position, new Vector2(dir.x, dir.y).normalized,
                    stat.stabThrowAttackRange, stat.stabThrowSpeed, stat.stabThrowDamage, isHeavy, ZAngle, eActivableColor.RED);
                projectileObj.transform.position = transform.position;

                PlayManager.Instance.UpdateColorthing();
                projectile.ReturnStartRoutine(stat.stabThrowAttackRange);
            }
        }
    }
    private void ReturnMonster()
    {
        Vector2 returnDir = (startPos - (Vector2)transform.position).normalized;
        transform.Translate(returnDir * stat.badyAttackSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, startPos) >= stat.returnPosValue)
        {
            currentState &= ~EMonsterAttackState.isReturnEnemy;
            canAttack = true;
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
                break;
            default:
                break;
        }
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
            Gizmos.DrawCube(transform.position + transform.forward, stat.senseCube);
        }
    }
    //public override void Respawn(GameObject monsterPos, bool isRespawnMonster)
    //{
    //    throw new System.NotImplementedException();
    //}
}
