using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStat", menuName = "Scriptable Object/MonsterStat")]
public class MonsterStat : ScriptableObject
{
    [SerializeField]
    private int monsterHP = 5;
    public int MonsterHP
    {
        get
        {
            return monsterHP;
        }
        set
        {
            monsterHP = value;
        }
    }

    public eActivableColor enemyColor;

    [Tooltip("최대 사거리")]
    public float senseCircle = 5f;

    public float moveSpeed = 1f;
    public float runSpeed = 2f;

    public int attackDamage = 1;
    public float attackTime = 0.3f;
    public float attackCooldown = 1f;
    public int contactDamage = 1;

    public float hitReboundPower = 5f;
    public float heavyHitReboundPower = 10f;
    public float groggyTime = 0.5f;
    [Tooltip("대기 상태까지 걸리는 시간")]
    public float timeToWait = 1.0f;
    public float deadDelay = 1.33f;
    public float AttackDelay = 1.33f;
}
