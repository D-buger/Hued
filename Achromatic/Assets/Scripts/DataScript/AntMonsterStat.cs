using UnityEngine;
[CreateAssetMenu(fileName = "AntMonsterStats", menuName = "Scriptable Object/AntMonsterStat")]
public class AntMonsterStat : MonsterStat
{
    [Tooltip("���� ���� ������")]
    public int cuttingAttackDamage = 1;
    [Tooltip("���� ���� ���� �̵��Ÿ�")]
    public float cuttingAttackRebound = 100.0f;
    [Tooltip("�˱� ���� ������")]
    public int swordAttackDamage = 1;
    [Tooltip("�˱� �ӵ�")]
    public float swordAttackSpeed = 2.0f;
    [Tooltip("�˱� �Ÿ�")]
    public float swordAttackRange = 2.0f;
    [Tooltip("�˱� �ݵ�")]
    public float swordAttackRebound = 2.0f;
    [Tooltip("��� ���� ������(���ϴ�)")]
    public int stabAttackDamage = 1;
    [Tooltip("������ ��� ���� ������")]
    public int lastStabAttackDamage = 2;
    [Tooltip("��� �ӵ�")]
    public float stabAttackSpeed = 2.0f;
    [Tooltip("��� ������ Ÿ�� �ӵ�")]
    public float lastStabSpeed = 4.0f;
    [Tooltip("��� ���ϴ� ������")]
    public float stabAttackDelay = 0.5f;
    [Tooltip("��� �ߴ� ������")]
    public float middleStabAttackDelay = 1.0f;
    [Tooltip("�ݰ� ���� �ð�")]
    public float counterAttackTime = 2.0f;
    [Tooltip("�ݰ� ���� ��Ÿ�")]
    public float counterAttackRange = 1.0f;
    [Tooltip("�ݰ� ���� ������")]
    public int counterAttackDamage = 2;
    [Tooltip("�ݰ� ���� �ӵ�")]
    public int counterAttackSpeed = 6;
    [Tooltip("��� ���� Ȯ��")]
    public int stabAttackPercent = 33;
    [Tooltip("�˱� ���� Ȯ��")]
    public int swordAttackPercent = 33;
}
