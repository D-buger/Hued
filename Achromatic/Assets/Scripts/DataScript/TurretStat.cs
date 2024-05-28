using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TurretStats", menuName = "Scriptable Object/TurretStats")]
public class TurretStat : MonsterStat
{
    [Tooltip("�ͷ� ���� ����ü�� ���ӵǴ� �ð�")]
    public float turretAttackDuration = 2.0f;
    [Tooltip("�ͷ� ���� ����ü �ӵ�")]
    public float turretAttackSpeed = 300f;
    [Tooltip("�ͷ� ���� ����ü ������")]
    public int turretAttackDamage = 2;
    [Tooltip("�ͷ� ���� ������")]
    public float turretAttackDelay = 2.0f;
}
