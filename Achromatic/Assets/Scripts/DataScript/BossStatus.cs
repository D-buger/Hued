using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossStatus", menuName = "Scriptable Object/BossStatus")]
public class BossStatus : ScriptableObject
{
    [Space(10), Header("Pattern")]
    public float patternDelayTime = 0.7f;
    public BossPattern[] patterns;

    [Space(10), Header("Status")]
    public int maxHp = 100;
    public float moveSpeed = 0.7f;
    public eActivableColor bossColor = eActivableColor.NONE;
}
