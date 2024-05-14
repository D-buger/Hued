using System.Collections;
using UnityEngine;

public abstract class Monster : MonoBehaviour, IAttack
{
    public MonsterStat baseStat;
    public int currentHP;

    [HideInInspector]
    public Vector2 monsterRunleftPosition;
    [HideInInspector]
    public Vector2 monsterRunRightPosition;
    [HideInInspector]
    public Vector2 monsterPosition;
    [HideInInspector]
    public float distanceToPlayer;
    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;

    public float runPosition;

    [HideInInspector]
    public float elapsedTime = 0;
    private float arrivalThreshold = 1f;
    [HideInInspector]
    public float distanceToStartPos = 0;
    [HideInInspector]
    public bool isDead = false;
    [SerializeField]
    private bool flyMonster = false;

    [System.Flags]
    public enum EMonsterState
    {
        isWait = 1 << 0,
        isPlayerBetween = 1 << 1,
        isBattle = 1 << 2
    }
    public EMonsterState state = EMonsterState.isWait;
    [HideInInspector]
    public bool canAttack = true;
    public bool isRespawnMonster = true;

    public virtual IEnumerator CheckPlayer(Vector2 startMonsterPos)
    {
        distanceToPlayer = Vector2.Distance(transform.position, PlayerPos);
        distanceToStartPos = Vector2.Distance(startMonsterPos, PlayerPos);
        if (flyMonster)
        {
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
        }
        else
        {
            if (distanceToStartPos <= runPosition && !IsStateActive(EMonsterState.isBattle) && canAttack)
            {
                SetState(EMonsterState.isPlayerBetween, true);
                SetState(EMonsterState.isWait, false);
                elapsedTime = 0f;
                CheckStateChange();
            }
            else
            {
                CheckWaitTime();
            }
        }
        yield break;
    }
    public virtual void WaitSituation()
    {
        currentHP = baseStat.MonsterHP;
        SetState(EMonsterState.isBattle, false);
        transform.position = Vector2.MoveTowards(transform.position, monsterPosition, baseStat.moveSpeed * Time.deltaTime);
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
    public bool HasArrived(Vector2 currentPosition, Vector2 targetPosition)
    {
        return Vector2.Distance(currentPosition, targetPosition) <= arrivalThreshold;
    }

    public virtual IEnumerator CheckWaitTime()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= baseStat.timeToWait && !IsStateActive(EMonsterState.isWait) && !IsStateActive(EMonsterState.isBattle) && canAttack)
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
        if (PlayerPos == null)
        {
            return;
        }
        float horizontalValue = PlayerPos.x - transform.position.x;
        transform.localScale = (horizontalValue >= 0) ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
        if (!flyMonster)
        {
            if (distanceToPlayer <= baseStat.senseCircle && !IsStateActive(EMonsterState.isBattle))
            {
                SetState(EMonsterState.isBattle, true);
                SetState(EMonsterState.isWait, false);
                SetState(EMonsterState.isPlayerBetween, false);
                CheckStateChange();
            }
            else if (distanceToPlayer > baseStat.senseCircle && !IsStateActive(EMonsterState.isBattle) && canAttack)
            {
                transform.position = Vector2.MoveTowards(transform.position, PlayerPos, baseStat.runSpeed * Time.deltaTime);
            }
        }
        else
        {
            // FIX 기획서 나오면 날아다니는 몬스터에 맞게 판정 변경
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
    }
    public abstract void Attack();
    public abstract void CheckStateChange();
    public virtual void Hit(int damage, int colorDamage, Vector2 attackDir, IParryConditionCheck parryCheck = null)
    {

    }
    public virtual void HPDown(int hitDamage)
    {
        currentHP -= hitDamage;
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

    public bool IsStateActive(EMonsterState eState)
    {
        return (state & eState) != 0;
    }
    public void OnPostAttack(Vector2 vec)
    {

    }

    //public abstract void Respawn(GameObject monsterPos, bool isRespawnMonster);
}
