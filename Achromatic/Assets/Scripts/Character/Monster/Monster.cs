using UnityEngine;

public abstract class Monster : MonoBehaviour, IAttack
{
    [SerializeField]
    private MonsterStat baseStat;
    public int currentHP;

    [HideInInspector]
    public Vector2 monsterRunleftPosition;
    [HideInInspector]
    public Vector2 monsterRunRightPosition;
    [HideInInspector]
    public Vector2 MonsterPosition;
    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;

    public float runPosition;

    private float elapsedTime = 0;
    private float arrivalThreshold = 1f;
    [HideInInspector]
    public float distanceToPlayer = 0;
    [HideInInspector]
    public bool isDead = false;

    [System.Flags]
    public enum EMonsterState
    {
        isWait = 1 << 0,
        isPlayerBetween = 1 << 1,
        isBattle = 1 << 2
    }
    [HideInInspector]
    public EMonsterState state = 0;
    [HideInInspector]
    public bool isWait = true;
    [HideInInspector]
    public bool isBattle = false;
    [HideInInspector]
    public bool isPlayerBetween = false;
    [HideInInspector]
    public bool canAttack = true;
    [HideInInspector]
    public bool isChase = false;

    public virtual void CheckPlayer(Vector2 startMonsterPos)
    {
        distanceToPlayer = Vector2.Distance(startMonsterPos, PlayerPos);
        if (distanceToPlayer <= runPosition && !isBattle && canAttack)
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
    public virtual void WaitSituation()
    {
        currentHP = baseStat.MonsterHP;
        isBattle = false;
        transform.position = Vector2.MoveTowards(transform.position, MonsterPosition, baseStat.moveSpeed * Time.deltaTime);
        if (MonsterPosition == monsterRunleftPosition)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (MonsterPosition == monsterRunRightPosition)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        if (HasArrived((Vector2)transform.position, monsterRunRightPosition))
        {
            MonsterPosition = monsterRunleftPosition;
        }
        if (HasArrived((Vector2)transform.position, monsterRunleftPosition))
        {
            MonsterPosition = monsterRunRightPosition;
        }
    }
    private bool HasArrived(Vector2 currentPosition, Vector2 targetPosition)
    {
        return Vector2.Distance(currentPosition, targetPosition) <= arrivalThreshold;
    }

    public virtual void CheckWaitTime()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= baseStat.timeToWait && !isWait && !isBattle && canAttack)
        {
            elapsedTime = 0f;
            isWait = true;
            isPlayerBetween = false;
            isBattle = false;
            CheckStateChange();
        }
    }
    public virtual void MoveToPlayer()
    {
        if (PlayerPos == null)
        {
            return;
        }
        float horizontalValue = PlayerPos.x - transform.position.x;

        transform.localScale = (horizontalValue >= 0) ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);

        if (distanceToPlayer <= baseStat.senseCircle && !isBattle && !isChase)
        {
            isBattle = true;
            isWait = false;
            isPlayerBetween = false;
            CheckStateChange();
        }
        else if (distanceToPlayer > baseStat.senseCircle && isPlayerBetween && canAttack)
        {
            transform.position = Vector2.MoveTowards(transform.position, PlayerPos, baseStat.moveSpeed * Time.deltaTime);
        }
    }
    public abstract void Attack();
    public abstract void CheckStateChange();
    public virtual void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage = 0)
    {

    }
    public virtual void HPDown(int hitDamage)
    {
        currentHP -= hitDamage;
    }
    public virtual void CheckDead()
    {
        if (currentHP <= 0)
        {
            isDead = true;
            Dead();
        }
    }
    public abstract void Dead();
    void IAttack.AfterAttack(Vector2 attackDir)
    {
    }
}
