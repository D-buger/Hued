using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FlyAntEnemy : Monster
{
    private MonsterFSM fsm;

    private Rigidbody2D rigid;
    private GameObject attackPoint;
    private Attack meleeAttack;
    [SerializeField]
    private GameObject attackTransform;
    [SerializeField]
    private FlyAntMonsterStat stat;
    private Projectile projectile;
    private Vector2 attackDir;
    private Vector2 startPos;
    private float deadDelayTime = 1.3f;

    public UnityEvent<eActivableColor> flyAntColorEvent;

    private bool isHeavy = false;
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

    private void CheckIsHeavy(eActivableColor color)
    {
        if (color == stat.enemyColor)
        {
            isHeavy = false;
        }
        flyAntColorEvent?.Invoke(color);
    }

    private void Start()
    {
        attackDir = (PlayerPos - (Vector2)transform.position).normalized;
        startPos = new Vector2(transform.position.x, transform.position.y);
        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, stat.enemyColor);
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
    public override void Attack()
    {
        if (canAttack && !currentState.HasFlag(EMonsterAttackState.IsAttack))
        {
            StartCoroutine(AttackSequence(PlayerPos));
        }
        else
        {
            Debug.Log("ErrorAttack");
            return;
        }
    }
    private IEnumerator AttackSequence(Vector2 attackAngle)
    {
        currentState |= EMonsterAttackState.IsAttack;
        canAttack = false;
        float ZAngle = (Mathf.Atan2(attackAngle.x - transform.position.x, attackAngle.y - transform.position.y) * Mathf.Rad2Deg);

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

        if (currentState.HasFlag(EMonsterAttackState.isFirstAttack))
        {
            int checkRandomAttackType = UnityEngine.Random.Range(1, 100);
            if (checkRandomAttackType < 50)
            {
                StartCoroutine(BadyAttack(attackDir));
            }
            else
            {
                StabThrowAttack();
            }
        }

        yield return Yields.WaitSeconds(stat.attackCooldown);
        currentState &= ~EMonsterAttackState.IsAttack;
        meleeAttack?.AttackDisable();
    }

    private IEnumerator BadyAttack(Vector2 direction)
    {
        int checkRandomAttackType = UnityEngine.Random.Range(1, 100);
        yield return Yields.WaitSeconds(stat.flyAntAttackDelay);
        meleeAttack.AttackAble(direction, stat.badyAttackDamage, stat.badyAttackDamage);
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
            yield return Yields.WaitSeconds(0.1f); // FIX stat¿¡ Ãß°¡
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
}
