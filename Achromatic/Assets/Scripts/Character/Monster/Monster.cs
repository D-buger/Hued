using System.Collections;
using UnityEngine;

public abstract class Monster : MonoBehaviour, IAttack
{
    protected MonsterStat baseStat;
    protected int currentHP;

    protected Vector2 monsterRunleftPosition;
    protected Vector2 monsterRunRightPosition;
    protected Vector2 monsterPosition;
    protected float distanceToPlayer;
    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;

    public float runPosition;

    protected float elapsedTime = 0;
    private float arrivalThreshold = 1f;
    protected float distanceToStartPos = 0;
    protected int DeadMass = 100;
    protected int originalMass = 6;
    protected bool isDead = false;

    [System.Flags]
    public enum EMonsterState
    {
        isWait = 1 << 0,
        isPlayerBetween = 1 << 1,
        isBattle = 1 << 2
    }
    public EMonsterState state = EMonsterState.isWait;
    [HideInInspector]
    protected bool canAttack = true;
    protected bool isRespawnMonster = true;

    public abstract void CheckStateChange();
    public bool IsStateActive(EMonsterState eState)
    {
        return (state & eState) != 0;
    }
    public virtual IEnumerator CheckPlayer(Vector2 startMonsterPos)
    {
        distanceToPlayer = Vector2.Distance(transform.position, PlayerPos);
        distanceToStartPos = Vector2.Distance(startMonsterPos, PlayerPos);
        if (distanceToStartPos <= runPosition && !IsStateActive(EMonsterState.isBattle) && canAttack)
        {
            SetState(EMonsterState.isPlayerBetween, true);
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
    public abstract void WaitSituation();
    public bool HasArrived(Vector2 currentPosition, Vector2 targetPosition)
    {
        return Vector2.Distance(currentPosition, targetPosition) <= arrivalThreshold;
    }

    public virtual IEnumerator CheckWaitTime()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= baseStat.waitStateDelay && !IsStateActive(EMonsterState.isWait) && !IsStateActive(EMonsterState.isBattle) && canAttack)
        {
            elapsedTime = 0f;
            SetState(EMonsterState.isWait, true);
            SetState(EMonsterState.isPlayerBetween, false);
            SetState(EMonsterState.isBattle, false);
            CheckStateChange();
        }
        yield break;
    }
    public virtual void MoveToPlayer()
    {
        if (ReferenceEquals(PlayerPos, null))
        {
            return;
        }
        float horizontalValue = PlayerPos.x - transform.position.x;
        transform.localScale = (horizontalValue >= 0) ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
        if (distanceToPlayer <= baseStat.senseCircle && !IsStateActive(EMonsterState.isBattle))
        {
            SetState(EMonsterState.isBattle, true);
            SetState(EMonsterState.isWait, false);
            SetState(EMonsterState.isPlayerBetween, false);
            CheckStateChange();
        }
        else if (distanceToPlayer > baseStat.senseCircle && !IsStateActive(EMonsterState.isBattle) && canAttack)
        {
            transform.position = Vector2.MoveTowards(transform.position, PlayerPos, baseStat.moveSpeed * Time.deltaTime);
        }
    }
    public abstract void Attack();
    public virtual void Hit(int damage, int colorDamage, Vector2 attackDir, IParryConditionCheck parryCheck = null)
    {

    }
    public virtual void CheckDead()
    {
        if (currentHP <= 0 && !isDead)
        {
            isDead = true;
            Dead();
        }
    }
    public abstract void Dead();
    public void SetState(EMonsterState eState, bool value)
    {
        if (value)
        {
            state |= eState;
        }
        else
        {
            state &= ~eState;
        }
    }
    public void OnPostAttack(Vector2 vec)
    {

    }

    //public abstract void Respawn(GameObject monsterPos, bool isRespawnMonster);
}
