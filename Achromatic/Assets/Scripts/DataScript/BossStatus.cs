using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossStatus", menuName = "Scriptable Object/BossStatus")]
public class BossStatus : ScriptableObject
{
    [Space(10), Header("Pattern")]
    public float patternDelayTime = 0.7f;
    public BossPattern[] startPhasePatterns;
    public BossPattern phaseChangePattern;
    public BossPattern[] endPhasePatterns;

    [Space(10), Header("Status")]
    public int maxHp = 100;
    public float moveSpeed = 0.7f;
    public eActivableColor bossColor = eActivableColor.NONE;
}
