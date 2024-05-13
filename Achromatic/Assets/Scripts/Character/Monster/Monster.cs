using UnityEngine;

public abstract class Monster : MonoBehaviour, IAttack
{
    [SerializeField]
    private MonsterStat baseStat;
    public int currentHP;

    [HideInInspector]
    public Vector2 leftPosition;
    [HideInInspector]
    public Vector2 rightPosition;
    [HideInInspector]
    public Vector2 thisPosition;
    [Tooltip("몬스터 기준 이동 범위")]
    public float runPosition;
    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;

    private float elapsedTime = 0;
    private float arrivalThreshold = 1f;
    [HideInInspector]
    public float distanceToPlayer = 0;
    [HideInInspector]
    public bool isDead = false;
    [HideInInspector]
    public bool isWait = true;
    [HideInInspector]
    public bool isBattle = false;
    [HideInInspector]
    public bool isPlayerBetween = false;
    [HideInInspector]
    public bool canAttack = true;

    public void CheckPlayer(Vector2 startSpriderPos)
    {
        distanceToPlayer = Vector2.Distance(transform.position, PlayerPos);
        float distanceToMonster = Vector2.Distance(startSpriderPos, PlayerPos);
        if (distanceToMonster <= runPosition && !isBattle && canAttack)
        {
            isPlayerBetween = true;
            isWait = false;
            elapsedTime = 0f;
            CheckStateChange();
        }
        else
        {
            CheckWaitTime();
        }
    }
    public void WaitSituation()
    {
        currentHP = baseStat.MonsterHP;
        isBattle = false;
        transform.position = Vector2.MoveTowards(transform.position, thisPosition, baseStat.moveSpeed * Time.deltaTime);
        if (thisPosition == leftPosition)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (thisPosition == rightPosition)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        if (HasArrived((Vector2)transform.position, rightPosition))
        {
            thisPosition = leftPosition;
        }
        if (HasArrived((Vector2)transform.position, leftPosition))
        {
            thisPosition = rightPosition;
        }
    }
    private bool HasArrived(Vector2 currentPosition, Vector2 targetPosition)
    {
        return Vector2.Distance(currentPosition, targetPosition) <= arrivalThreshold;
    }

    public void CheckWaitTime()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= baseStat.usualTime && !isWait && !isBattle && canAttack)
        {
            elapsedTime = 0f;
            isWait = true;
            isPlayerBetween = false;
            isBattle = false;
            CheckStateChange();
        }
    }
    public void MoveToPlayer()
    {
        float horizontalValue = PlayerPos.x - transform.position.x;

        if (PlayerPos == null)
        {
            return;
        }

        if (horizontalValue >= 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        if (distanceToPlayer <= baseStat.senseCircle && !isBattle)
        {
            isBattle = true;
            isWait = false;
            isPlayerBetween = false;
            CheckStateChange();
        }
        else if (distanceToPlayer > baseStat.senseCircle)
        {
            if (isPlayerBetween && canAttack)
            {
                transform.position = Vector2.MoveTowards(transform.position, PlayerPos, baseStat.moveSpeed * Time.deltaTime);
            }
        }
    }
    public abstract void Attack();
    public abstract void CheckStateChange();
    public virtual void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage = 0)
    {

    }
    public void HPDown(int HitDamage)
    {
        currentHP -= HitDamage;
    }
    public void CheckDead()
    {
        if (currentHP <= 0)
        {
            isDead = true;
        }
        if (isDead)
        {
            Dead();
        }
    }
    public abstract void Dead();
    void IAttack.OnPostAttack(Vector2 attackDir)
    {
    }
}
