using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpiderMonsterStats", menuName = "Scriptable Object/SpiderMonsterStat")]
public class SpiderMonsterStats : MonsterStat
{
    [Tooltip("���� ���� ��Ÿ�")]
    public float meleeAttackRange = 1.0f;
    [Tooltip("���� ���� ����")]
    public float attackRange = 0.8f;
    [Tooltip("���Ÿ� ���� ������")]
    public int rangedAttackDamage = 1;
    [Tooltip("���Ÿ� ���� �ӵ�")]
    public float rangedAttackSpeed = 1.0f;
    [Tooltip("���Ÿ� ���� �����Ÿ�")]
    public float rangedAttackRange = 2.0f;
    [Tooltip("��ġ�� ���� �ݵ� �Ŀ�")]
    public float specialAttackReboundPower = 1.0f;
    [Tooltip("��ġ�� ���� �ӵ�")]
    public float specialAttackSpeed = 1.0f;
    [Tooltip("����� �ĵ� �ӵ�")]
    public float earthAttackSpeed = 1.0f;
    [Tooltip("����� �ĵ� ������")]
    public int earthAttackDamage = 1;
    [Tooltip("����� ���� ���� �ð�")]
    public float earthAttackDuration = 0.1f;
    [Tooltip("����� ���� ����")]
    public float earthAttackJump = 10.0f;
    [Tooltip("����� �ĵ� ������")]
    public float earthAttackDelay = 0.3f;
    [Tooltip("��ġ�� ���� Ȯ��")]
    public float specialAttackPercent = 60.0f;
    [Tooltip("���Ÿ� ���� Ȯ��")]
    public int rangedAttackPercent = 50;
    [Tooltip("���� ����� ��Ÿ�")]
    public float rushGroundAttackRange = 1.0f;
}
