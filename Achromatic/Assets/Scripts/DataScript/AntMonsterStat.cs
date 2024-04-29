using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AntMonsterStats", menuName = "Scriptable Object/AntMonsterStat")]
public class AntMonsterStat : MonsterStat
{
    [Tooltip("���� ���� ������")]
    public int cuttingAttackDamage = 1;
    [Tooltip("���� ���� ���� �̵��Ÿ�")]
    public float cuttingAttackRebound = 100.0f;
    [Tooltip("�˱� ���� ������")]
    public float swordAttackDamage = 1.0f;
    [Tooltip("�˱� �ӵ�")]
    public float swordAttackSpeed = 2.0f;
    [Tooltip("��� �ӵ�")]
    public float stabAttackSpeed = 2.0f;
    [Tooltip("��� ������ Ÿ�� �ӵ�")]
    public float lastStabSpeed = 4.0f;
    [Tooltip("��� ���ϴ� ������")]
    public float stabAttackDelay = 1.0f;
    [Tooltip("��� �ߴ� ������")]
    public float middleStabAttackDelay = 2.0f;
    [Tooltip("�ݰ� ���� �ð�")]
    public float counterAttackTime = 2.0f;
    [Tooltip("��� ���� Ȯ��")]
    public int stabAttackPercent = 33;
    [Tooltip("�˱� ���� Ȯ��")]
    public int swordAttackPercent = 33;
}
