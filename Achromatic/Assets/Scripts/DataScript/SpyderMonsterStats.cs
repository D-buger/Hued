using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpyderMonsterStats", menuName = "Scriptable Object/SpyderMonsterStat")]
public class SpyderMonsterStats : MonsterStat
{
    [Tooltip("���� ���� ��Ÿ�")]
    public float meleeAttackRange = 1.0f;
    [Tooltip("���� ���� ����")]
    public float AttackRange = 0.8f;
    [Tooltip("��� ���±��� �ɸ��� �ð�")]
    public float usualTime = 1.0f;
    [Tooltip("���Ÿ� ���� ������")]
    public int rangedAttackDamege = 1;
    [Tooltip("���Ÿ� ���� �ӵ�")]
    public float rangedAttackSpeed = 1.0f;
    [Tooltip("���Ÿ� ���� �����Ÿ�")]
    public float rangedAttackRange = 1.0f;
    [Tooltip("��ġ�� ���� ������")]
    public int specialAttackDamege = 1;
    [Tooltip("��ġ�� ���� �̵� �Ÿ�")]
    public float specialAttackRange = 1.0f;
    [Tooltip("��ġ�� ���� �ӵ�")]
    public float specialAttackSpeed = 1.0f;
    [Tooltip("����� �ĵ� �ӵ�")]
    public float earthAttackSpeed = 1.0f;
    [Tooltip("����� �ĵ� ������")]
    public int earthAttackDamege = 1;
    [Tooltip("����� �ĵ� �̵� �Ÿ�")]
    public float earthAttackRange = 1.0f;
    [Tooltip("��ġ�� ���� Ȯ��")]
    public int specialAttackPercent = 50;
    [Tooltip("����� ���� Ȯ��")]
    public int earthAttackPercent = 50;
}
