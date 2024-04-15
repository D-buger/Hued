using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;
using Spine.Unity;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Spine.Unity.Examples;

public class SpyderEnemy : MonoBehaviour, IAttack, IParry
{
    [Header("Components")]
    private Rigidbody2D rigid;
    private GameObject attackPoint;
    private Attack meleeAttack;
    [SerializeField]
    private SpyderMonsterStats stat;
    [SerializeField, Space]
    private Projectile rangedAttack;
    [SerializeField]
    private Projectile earthAttack;



    [Header("Animation")]
    [SerializeField]
    private SkeletonAnimation skeletonAnimation;
    [SerializeField]
    private AnimationReferenceAsset[] aniClip;

    public enum AnimaState
    {
        Idle, Walk, Charge, Ground, Spit, Detection, Dead
    }
    private AnimaState animState;

    private string currentAnimation;

    private float elapsedTime = 0;
    private float arrivalThreshold = 1f;
    private float distanceToPlayer = 0;
    private float angleThreshold = 48f;

    [SerializeField, Tooltip("몬스터 기준 이동 범위")]
    private float runPosition;

    public GameObject[] earthObjectGroup1;
    public GameObject[] earthObjectGroup2;
    public GameObject[] earthObjectGroup3;
    public UnityEvent<eActivableColor> spyderColorEvent;


    private Vector2 startPosition;
    private Vector2 targetPosition;
    private Vector2 thisPosition;
    private Vector3 startPos;
    private Vector3 endPos;

    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;

    public int currentHP;

    public bool isDead = false;
    private bool isBettle = false;
    private bool canAttack = true;
    private bool isAttack = false;
    private bool isWait = true;
    private bool isfirstAttack = false;
    private bool playerBetweenPositions = false;
    private bool isEarthAttack = false;
    private bool isHeavy = false;
    private bool gameStart = false;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        attackPoint = transform.GetChild(0).gameObject;
        meleeAttack = attackPoint.GetComponentInChildren<Attack>();
    }
    private void Start()
    {
        startPos = new Vector3(transform.position.x + runPosition, transform.position.y);
        endPos = new Vector3(transform.position.x - runPosition, transform.position.y);
        startPosition.y = transform.position.y;
        targetPosition.y = transform.position.y;
        startPosition.x += transform.position.x + runPosition;
        targetPosition.x += transform.position.x - runPosition;

        currentHP = stat.MonsterHP;
        thisPosition = targetPosition;
        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, null);
        //MonsterManager.Instance.getColorEvent.AddListener(CheckIsHeavy);
        gameStart = true;
    }

    private void CheckIsHeavy(eActivableColor playerColor)
    {
        if (playerColor == stat.enemyColor)
        {
            isHeavy = false;
        }
        spyderColorEvent?.Invoke(playerColor);
    }

    private void Update()
    {
        if (isDead)
        {
            StartCoroutine(Dead());
        }
        CheckPlayer();
        if (isWait)
        {
            WaitSituation();
            animState = AnimaState.Walk;
        }
        else if (canAttack && isBettle)
        {
            Attack(PlayerPos);
        }

        if (!playerBetweenPositions)
        {
            CheckWaitTime();
        }
        SetCurrentAniamtion(animState);
        if (!gameStart)
        {
            gameStart = true;
        }
    }
    private void AsncAnimation(AnimationReferenceAsset animClip, bool loop, float timeScale)
    {
        if (animClip.name.Equals(currentAnimation))
            return;

        skeletonAnimation.state.SetAnimation(0, animClip, loop).TimeScale = timeScale;
        skeletonAnimation.loop = loop;
        skeletonAnimation.timeScale = timeScale;
        currentAnimation = animClip.name;
    }
    private void SetCurrentAniamtion(AnimaState _state)
    {
        switch (_state)
        {
            case AnimaState.Idle:
                AsncAnimation(aniClip[(int)AnimaState.Idle], true, 1f);
                break;
            case AnimaState.Walk:
                AsncAnimation(aniClip[(int)AnimaState.Walk], true, 1f);
                break;
            case AnimaState.Charge:
                AsncAnimation(aniClip[(int)AnimaState.Charge], false, 1f);
                break;
            case AnimaState.Ground:
                AsncAnimation(aniClip[(int)AnimaState.Ground], false, 1f);
                break;
            case AnimaState.Spit:
                AsncAnimation(aniClip[(int)AnimaState.Spit], false, 1f);
                break;
            case AnimaState.Detection:
                AsncAnimation(aniClip[(int)AnimaState.Detection], false, 1f);
                break;
            case AnimaState.Dead:
                AsncAnimation(aniClip[(int)AnimaState.Dead], false, 1f);
                break;
        }
    }

    private void CheckPlayer()
    {
        if (IsBetween(PlayerPos.x, startPosition.x, targetPosition.x))
        {
            playerBetweenPositions = true;
            isBettle = true;
            isWait = false;
            elapsedTime = 0f;
        }
        else
        {
            playerBetweenPositions = false;
        }

        distanceToPlayer = Vector2.Distance(transform.position, PlayerPos);
    }
    private bool IsBetween(float value, float start, float end)
    {
        return value >= Mathf.Min(start, end) && value <= Mathf.Max(start, end);
    }
    private void CheckWaitTime()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= stat.usualTime)
        {
            elapsedTime = 0f;
            isWait = true;
        }
    }

    private void WaitSituation()
    {
        currentHP = stat.MonsterHP;
        isfirstAttack = true;
        isBettle = false;
        transform.position = Vector2.MoveTowards(transform.position, thisPosition, stat.moveSpeed * Time.deltaTime);

        if (HasArrived((Vector2)transform.position, targetPosition))
        {
            thisPosition = startPosition;
        }
        if (HasArrived((Vector2)transform.position, startPosition))
        {
            thisPosition = targetPosition;
        }
    }
    private bool HasArrived(Vector2 currentPosition, Vector2 targetPosition)
    {
        return Vector2.Distance(currentPosition, targetPosition) <= arrivalThreshold;
    }


    public void Attack(Vector2 vec)
    {
        StartCoroutine(MoveToPlayer());
    }

    IEnumerator AttackSequence(Vector2 attackAngle)
    {
        isAttack = true;
        canAttack = false;
        float horizontalValue = attackAngle.x - transform.position.x;
        float verticalValue = attackAngle.y - transform.position.y;
        Vector2 value = new Vector2(horizontalValue, verticalValue);
        Vector2 check = new Vector2(1.0f, 0);
        float angleToPlayer = Mathf.Atan2(attackAngle.y, transform.position.y) * Mathf.Rad2Deg;
        Debug.Log(angleToPlayer);
        bool facingPlayer = Mathf.Abs(angleToPlayer - transform.eulerAngles.z) < angleThreshold;

        if (value.x <= 0)
        {
            transform.localScale = new Vector2(1, 1);
            check = new Vector2(-0.998f, 0);
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
            check = new Vector2(0.998f, 0);
        }

        animState = AnimaState.Detection;
        SetCurrentAniamtion(animState);
        yield return Yields.WaitSeconds(1.34f);
        if (isfirstAttack)
        {
            yield return Yields.WaitSeconds(0.10f);
            animState = AnimaState.Spit;
            SetCurrentAniamtion(animState);
            Projectile attack = Instantiate(rangedAttack.gameObject).GetComponent<Projectile>();
            isfirstAttack = false;
            attack.Shot(gameObject, transform.position, new Vector2(horizontalValue, verticalValue).normalized,
                    stat.rangedAttackRange, stat.rangedAttackSpeed, stat.rangedAttackDamege, isHeavy, eActivableColor.RED);
        }
        else if (distanceToPlayer < stat.meleeAttackRange)
        {
            float randomChance = UnityEngine.Random.value;
            if (facingPlayer && randomChance <= stat.specialAttackPercent/100)
            {
                animState = AnimaState.Charge;
                SetCurrentAniamtion(animState);
                yield return Yields.WaitSeconds(1.3f);
                facingPlayer = false;

                meleeAttack?.AttackAble(-value, stat.attackDamage, isHeavy);
                rigid.AddForce(check * stat.specialAttackRound, ForceMode2D.Impulse);
            }
            else
            {
                animState = AnimaState.Ground;
                SetCurrentAniamtion(animState);
                yield return Yields.WaitSeconds(1.8f);
                Projectile earthProjectileLeft = Instantiate(earthAttack.gameObject).GetComponent<Projectile>();
                Projectile earthProjectileRight = Instantiate(earthAttack.gameObject).GetComponent<Projectile>();
                rigid.velocity = Vector2.up * stat.earthAttackJump;
                isEarthAttack = true;

                StartCoroutine(SpawnObjects());
            }
        }
        else if (distanceToPlayer > stat.meleeAttackRange && distanceToPlayer < stat.rangedAttackRange)
        {
            animState = AnimaState.Spit;
            SetCurrentAniamtion(animState);
            yield return Yields.WaitSeconds(0.14f);
            Projectile attack = Instantiate(rangedAttack.gameObject).GetComponent<Projectile>();
            isfirstAttack = false;
            attack.Shot(gameObject, transform.position, new Vector2(horizontalValue, verticalValue).normalized,
                    stat.rangedAttackRange, stat.rangedAttackSpeed, stat.rangedAttackDamege, isHeavy, eActivableColor.RED);

        }
        yield return Yields.WaitSeconds(stat.attackTime);
        isAttack = false;
        meleeAttack?.AttackDisable();
        yield return Yields.WaitSeconds(stat.attackCooldown);
        canAttack = true;
    }

    private IEnumerator MoveToPlayer()
    {
        while (!isAttack && !isWait)
        {
            yield return new WaitForSeconds(stat.attackCooldown);
            float horizontalValue = PlayerPos.x - transform.position.x;
            float verticalValue = PlayerPos.y - transform.position.y;

            if (horizontalValue > 0)
            {
                skeletonAnimation.initialFlipX = false;
            }
            else
            {
                skeletonAnimation.initialFlipX = true;
            }

            if (PlayerPos == null)
                yield break;

            if (distanceToPlayer <= stat.rangedAttackRange && canAttack)
            {
                StartCoroutine(AttackSequence(PlayerPos));
                yield break;
            }
            else if (distanceToPlayer > stat.rangedAttackRange)
            {
                //FIX X좌표로만 이동하게 변경
                transform.position = Vector2.MoveTowards(transform.position, PlayerPos, stat.moveSpeed * Time.deltaTime);
            }
        }
    }
    private IEnumerator SpawnObjects()
    {
        while (isEarthAttack)
        {
            yield return new WaitForSeconds(0.6f);

            ActivateObjects(earthObjectGroup1);
            yield return new WaitForSeconds(stat.earthAttackTime);
            DeactivateObjects(earthObjectGroup1);
            yield return new WaitForSeconds(stat.earthAttackDalay);

            ActivateObjects(earthObjectGroup2);
            yield return new WaitForSeconds(stat.earthAttackTime);
            DeactivateObjects(earthObjectGroup2);
            yield return new WaitForSeconds(stat.earthAttackDalay);

            ActivateObjects(earthObjectGroup3);
            yield return new WaitForSeconds(stat.earthAttackTime);
            DeactivateObjects(earthObjectGroup3);
            yield return new WaitForSeconds(stat.earthAttackDalay);
            isEarthAttack = false;
        }
    }

    private void ActivateObjects(GameObject[] objects)
    {
        foreach (GameObject obj in objects)
        {
            obj.SetActive(true);
        }
    }

    private void DeactivateObjects(GameObject[] objects)
    {
        foreach (GameObject obj in objects)
        {
            obj.SetActive(false);
        }
    }
    public void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage = 0)
    {
        if (!isHeavyAttack)
        {
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                currentHP -= criticalDamage;
                rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
            }
            else
            {
                currentHP -= damage;
                rigid.AddForce(attackDir * stat.hitReboundPower, ForceMode2D.Impulse);
            }
        }
        else
        {
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                currentHP -= damage;
                rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
            }
        }
        CheckDead();
    }

    private void CheckDead()
    {
        if (currentHP <= 0 && !isDead)
        {
            isDead = true;
;        }
    }
    private IEnumerator Dead()
    {
        skeletonAnimation.state.GetCurrent(0).TimeScale = 0;
        skeletonAnimation.state.GetCurrent(0).TimeScale = 1;
        animState = AnimaState.Dead;
        SetCurrentAniamtion(animState);
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
        yield return null;
    }

    private void OnDrawGizmos()
    {
        if (null != stat)
        {
            if (stat.meleeAttackRange >= distanceToPlayer)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawWireSphere(transform.position + transform.forward, stat.senseCircle);
        }
        if (!gameStart)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x + runPosition, transform.position.y), 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x - runPosition, transform.position.y), 0.5f);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPos, 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPos, 0.5f);
        }

    }

    public void AfterAttack(Vector2 attackDir)
    {
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            collision.gameObject.GetComponent<Player>().Hit(stat.contactDamage,
                    transform.position - collision.transform.position, false, stat.contactDamage);
        }
    }

    public void Parried()
    {
        throw new NotImplementedException();
    }
}
