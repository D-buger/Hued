using Newtonsoft.Json.Linq;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Serialization;
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
    private FlyAntMonsterStat stat;
    [SerializeField]
    private JObject jsonObject;

    private Vector2 monsterStartPos;
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
    [SerializeField]
    private EMonsterAttackState currentState = EMonsterAttackState.isFirstAttack;
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
        monsterStartPos = new Vector2(transform.position.x, transform.position.y);
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
        StartCoroutine(CheckPlayer(startFlyAntPosition));
        if (IsStateActive(EMonsterState.isBattle))
        {
            Attack();
        }
        if (currentState.HasFlag(EMonsterAttackState.isBadyAttack) && !currentState.HasFlag(EMonsterAttackState.isReturnEnemy))
        {
            StartCoroutine(Rush());
        }
        if (currentState.HasFlag(EMonsterAttackState.isReturnEnemy) && !isReturnStop)
        {
            ReturnMonster();
        }
    }
    private void CheckIsHeavy(eActivableColor color)
    {
        if (color == stat.enemyColor)
        {
            isHeavy = false;
        }
        flyAntColorEvent?.Invoke(color);
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
    public override void Attack()
    {
        if (isDead)
        {
            return;
        }

        if (currentState.HasFlag(EMonsterAttackState.isFirstAttack))
        {
            battlePos = new(transform.position.x, transform.position.y);
            SetAttackState(EMonsterAttackState.isFirstAttack, false);
        }

        if (canAttack && !currentState.HasFlag(EMonsterAttackState.IsAttack))
        {
            StartCoroutine(AttackSequence(PlayerPos));
        }
    }
    private IEnumerator AttackSequence(Vector2 attackAngle)
    {
        SetAttackState(EMonsterAttackState.IsAttack, true);
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
        int checkRandomAttackType = 40;//UnityEngine.Random.Range(1, 100);
        if (checkRandomAttackType < 50)
        {
            StartCoroutine(BadyAttack()); // 돌진 공격
            Debug.Log("돌진 공격");
        }
        else
        {
            StabThrowAttack(); // 창 던지기 공격
            Debug.Log("창 던지기");
        }

        yield return Yields.WaitSeconds(stat.attackCooldown);
        SetAttackState(EMonsterAttackState.IsAttack, false);
        meleeAttack?.AttackDisable();
    }

    private IEnumerator BadyAttack()
    {
        targetPos = new(PlayerPos.x, PlayerPos.y);
        int checkRandomAttackType = 60;//UnityEngine.Random.Range(1, 100);
        int dmagepool = stat.contactDamage;
        yield return Yields.WaitSeconds(stat.flyAntAttackDelay);
        stat.contactDamage = stat.badyAttackDamage;
        if (checkRandomAttackType > stat.doubleBadyAttackPer)
        {
            SetAttackState(EMonsterAttackState.isBadyAttack, true);
            isDoubleBadyAttack = true;
            isReturnStop = true;
            Debug.Log("연속 돌진");
        }
        else
        {
            SetAttackState(EMonsterAttackState.isBadyAttack, true);
        }
        stat.contactDamage = dmagepool;
    }
    private IEnumerator Rush()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPos, stat.badyAttackSpeed * Time.deltaTime);
        Debug.Log("러시");
        if (Vector2.Distance(transform.position, targetPos) <= stat.returnPosValue)
        {
            if (isDoubleBadyAttack)
            {
                targetPos = new(PlayerPos.x, PlayerPos.y);
                SetAttackState(EMonsterAttackState.isBadyAttack, true);
                yield return Yields.WaitSeconds(1.0f); //FIX 매직넘버
                isDoubleBadyAttack= false;
            }
            else
            {
                Debug.Log("일반 돌진이 선택됨");
                SetAttackState(EMonsterAttackState.isBadyAttack, false);
                SetAttackState(EMonsterAttackState.isReturnEnemy, true);
                isReturnStop = false;
            }
        }
    }
    private void ReturnMonster()
    {
        Debug.Log("리턴");
        transform.position = Vector2.MoveTowards(transform.position, battlePos, stat.badyAttackSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, battlePos) <= stat.returnPosValue)
        {
            SetAttackState(EMonsterAttackState.isReturnEnemy, false);
            canAttack = true;
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
                    stat.stabThrowAttackRange, stat.stabThrowSpeed, stat.stabThrowDamage, ZAngle, eActivableColor.RED);
                projectileObj.transform.position = transform.position;

                PlayManager.Instance.UpdateColorthing();
                projectile.ReturnStartRoutine(stat.stabThrowAttackRange);
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
    private void SetAttackState(EMonsterAttackState eState, bool value)
    {
        if (value)
        {
            currentState |= eState;
        }
        else
        {
            currentState &= ~eState;
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
