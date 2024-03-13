using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStat", menuName = "Scriptable Object/MonsterStat")]
public class MonsterStat : MonoBehaviour
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

    [SerializeField]
    private float sensePlayerCircle = 2f;

    [SerializeField]
    private float moveSpeed = 1f;
}
