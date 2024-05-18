using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Spine.Unity;
using Newtonsoft.Json.Linq;
using TextAsset = UnityEngine.TextAsset;
using Unity.VisualScripting;
using System;
using System.Runtime.CompilerServices;

public class SpiderEnemy : Monster, IAttack, IParryConditionCheck
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
        IDLE,
        WALK,
        CHARGE,
        GROUND,
        SPIT,
        DETECTION,
        DEAD
    }
    [Flags]
    private enum EMonsterAttackState
    {
        NONE = 0,
        ISATTACK = 1 << 0,
        ISFIRSTATTACK = 1 << 1,
        ISEARTHATTACK = 1 << 2
    }
    private EMonsterAttackState currentState = EMonsterAttackState.ISFIRSTATTACK;
    [SerializeField]
    private EanimState animState;

    private string currentAnimation;
    private float angleThreshold = 52f;
    private float spitTime = 2.0f;
    private float spitWaitTime = 0.4f;
    private float delayToAttack = 0.05f;
    private float delayToDestory = 0.05f;
    private float delayToEarthAttack = 0.6f;


    public ParticleSystem[] earthParticleGroup;

    public UnityEvent<eActivableColor> spriderColorEvent;

    [SerializeField]
    private GameObject[] earthObjects;
    [SerializeField]
    private GameObject attackTransform;

    public Vector2 startSpiderPosition;
    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;
    private LayerMask originLayer;
    private int colorVisibleLayer;

    private JObject jsonObject;

    private bool isHeavy = true;

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
        monsterRunleftPosition = transform.position;
        monsterRunRightPosition = transform.position;
        monsterRunleftPosition.x += runPosition;
        monsterRunRightPosition.x -= runPosition;

        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, stat.enemyColor);
        startSpiderPosition = new Vector2(transform.position.x, transform.position.y);
        monsterPosition = monsterRunRightPosition;

        originLayer = gameObject.layer;
        colorVisibleLayer = LayerMask.GetMask("ColorEnemy");

        MonsterManager.Instance?.GetColorEvent.AddListener(CheckIsHeavy);
        PlayManager.Instance.FilterColorAttackEvent.AddListener(IsActiveColor);
        PlayManager.Instance.UpdateColorthing();
        angleThreshold += transform.position.y;
        if (animationJson is not null)
        {
            jsonObject = JObject.Parse(animationJson.text);
        }
        CheckStateChange();
    }
    private void Update()
    {
        StartCoroutine(CheckPlayer(startSpiderPosition));
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
                animState = EanimState.WALK;
                SetCurrentAnimation(animState);
                break;
            case EMonsterState.isWait:
                fsm.ChangeState("Idle");
                animState = EanimState.WALK;
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
        spriderColorEvent?.Invoke(color);
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
            case EanimState.IDLE:
                AsyncAnimation(aniClip[(int)EanimState.IDLE], true, timeScale);
                break;
            case EanimState.WALK:
                AsyncAnimation(aniClip[(int)EanimState.WALK], true, timeScale);
                break;
            case EanimState.CHARGE:
                AsyncAnimation(aniClip[(int)EanimState.CHARGE], false, timeScale);
                break;
            case EanimState.GROUND:
                AsyncAnimation(aniClip[(int)EanimState.GROUND], false, timeScale);
                break;
            case EanimState.SPIT:
                AsyncAnimation(aniClip[(int)EanimState.SPIT], false, timeScale);
                break;
            case EanimState.DETECTION:
                AsyncAnimation(aniClip[(int)EanimState.DETECTION], false, timeScale);
                break;
            case EanimState.DEAD:
                AsyncAnimation(aniClip[(int)EanimState.DEAD], false, timeScale);
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
            SetState(EMonsterState.isWait, false);
            SetState(EMonsterState.isPlayerBetween, false);
            CheckStateChange();
        }
        else if (distanceToPlayer > stat.senseCircle && !IsStateActive(EMonsterState.isBattle) && canAttack)
        {
            transform.position = Vector2.MoveTowards(transform.position, PlayerPos, stat.moveSpeed * Time.deltaTime);
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

    public override void Attack()
    {
        if (Vector2.Distance(transform.position, PlayerPos) >= stat.senseCircle)
        {
            SetState(EMonsterState.isBattle, false);
            SetState(EMonsterState.isPlayerBetween, true);
            SetState(EMonsterState.isWait, false);
        }
        else if (canAttack && !currentState.HasFlag(EMonsterAttackState.ISATTACK))
        {
            StartCoroutine(AttackSequence(PlayerPos));
        }
    }

    private IEnumerator AttackSequence(Vector2 attackAngle)
    {
        currentState |= EMonsterAttackState.ISATTACK;
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
        animState = EanimState.DETECTION;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds((float)jsonObject["animations"]["attack/charge_attack"]["events"][1]["time"]);
        if (currentState.HasFlag(EMonsterAttackState.ISFIRSTATTACK))
        {
            StartCoroutine(Spit(value, ZAngle));
            currentState &= ~EMonsterAttackState.ISFIRSTATTACK;
        }
        else if (distanceToPlayer < stat.meleeAttackRange)
        {
            int checkRandomAttackType = UnityEngine.Random.Range(1, 101);
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
            int checkRandomAttackType = UnityEngine.Random.Range(1, 101);
            if (checkRandomAttackType <= stat.rangedAttackPercent)
            {
                StartCoroutine(Spit(value, ZAngle));
            }
            else
            {
                StartCoroutine(rushGroundAttack(check));
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
        currentState &= ~EMonsterAttackState.ISATTACK;
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
        animState = EanimState.SPIT;
        SetCurrentAnimation(animState);
        yield return new WaitForSeconds((float)jsonObject["animations"]["attack/spit_web"]["events"][0]["time"]);
        GameObject projectileObj = ObjectPoolManager.instance.GetProjectileFromPool(0);
        if (projectileObj is not null)
        {
            projectileObj.SetActive(true);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile is not null)
            {
                projectile.Shot(gameObject, attackTransform.transform.position, value.normalized,
                    stat.rangedAttackRange, stat.rangedAttackSpeed, stat.rangedAttackDamage, false, zAngle, eActivableColor.RED);
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
        animState = EanimState.CHARGE;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds((float)jsonObject["animations"]["attack/charge_attack"]["events"][1]["time"]);

        meleeAttack?.AttackEnable(-value, stat.attackDamage, stat.attackDamage);
        rigid.AddForce(check * stat.specialAttackReboundPower, ForceMode2D.Impulse);
        meleeAttack?.AttackDisable();
    }
    private IEnumerator EarthAttack()
    {
        if (IsStateActive(EMonsterState.isWait))
        {
            yield break;
        }
        animState = EanimState.GROUND;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds((float)jsonObject["animations"]["attack/ground_attack"]["events"][0]["time"]);
        rigid.velocity = Vector2.up * stat.earthAttackJump;
        currentState |= EMonsterAttackState.ISEARTHATTACK;

        StartCoroutine(SpawnObjects());
    }
    private IEnumerator rushGroundAttack(Vector2 check)
    {
        rigid.AddForce(check * stat.rushGroundAttackRange, ForceMode2D.Impulse);
        animState = EanimState.CHARGE;
        SetCurrentAnimation(animState);
        yield return Yields.WaitSeconds((float)jsonObject["animations"]["attack/charge_attack"]["events"][1]["time"]);

        StartCoroutine(EarthAttack());
    }
    private IEnumerator SpawnObjects()
    {
        if (IsStateActive(EMonsterState.isWait))
        {
            yield break;
        }
        int objectCount = earthObjects.Length;
        while (currentState.HasFlag(EMonsterAttackState.ISEARTHATTACK))
        {
            yield return new WaitForSeconds(delayToEarthAttack);

            StartCoroutine(ActivateParticle(earthParticleGroup));

            currentState &= ~EMonsterAttackState.ISEARTHATTACK;

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
            if (particles[i] is not null)
            {
                particles[i].Play();
            }

            if (i % 2 == 1)
            {
                yield return new WaitForSeconds(stat.earthAttackDuration);
            }
        }
    }
    private void ActivateObjects(GameObject[] objects, int startIndex, int endIndex, bool isSet)
    {
        for (int i = startIndex; i < endIndex; i++)
        {
            if (objects[i] is not null)
            {
                objects[i].SetActive(isSet);
            }
        }
    }

    public override void Hit(int damage, int colorDamage, Vector2 attackDir, IParryConditionCheck parryCheck = null)
    {
        if (!isDead)
        {
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
        StopCoroutine(AttackSequence(PlayerPos));
        animState = EanimState.DEAD;
        SetCurrentAnimation(animState);
        yield return new WaitForSeconds(stat.deadDelay);
        gameObject.SetActive(false);
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

    //public override void Respawn(GameObject monsterPos, bool isRespawnMonster)
    //{
    //    throw new NotImplementedException();
    //}
}