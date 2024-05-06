using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoutingPattern : BossPattern
{
    [SerializeField]
    private float confusionDebuffDuration = 3f;
    [SerializeField]
    private float postHitDownDelay = 2f;
    [SerializeField]
    private float afterHitDownDelay = 1f;
    [SerializeField]
    private int patternDamage = 10;

    private float elapsedTime;

    private bool hasPlayerConfusionDebuff;
    private bool isPostHitDownBehaviour;
    private bool isPatternEnd;

    public override void OnStart()
    {
        elapsedTime = 0;
        hasPlayerConfusionDebuff = true;
        isPostHitDownBehaviour = false;
        isPatternEnd = false;
        InputManager.Instance.CanInput = false;
        PlayManager.Instance.GetPlayer.ChangeState(ePlayerState.IDLE);
        PlayManager.Instance.cameraManager.ShakeCamera(confusionDebuffDuration, true);
    }
    public override void OnUpdate()
    {
        elapsedTime += Time.deltaTime;

        Debug.Log(elapsedTime);
        if(hasPlayerConfusionDebuff && elapsedTime > confusionDebuffDuration)
        {
            hasPlayerConfusionDebuff = false;
            isPostHitDownBehaviour = true;
            InputManager.Instance.CanInput = true;
            elapsedTime = 0;
        }
        else if (isPostHitDownBehaviour && elapsedTime > postHitDownDelay)
        {
            isPostHitDownBehaviour = false;
            isPatternEnd = true;
            PlayManager.Instance.GetPlayer.Hit(patternDamage, patternDamage, 
                Vector2.zero, this);
            elapsedTime = 0;
        }
        else if(isPatternEnd && elapsedTime > afterHitDownDelay)
        {
            PatternEnd();
        }

    }
    public override bool CanParryAttack()
    {
        return base.CanParryAttack();
    }
    public override void DrawGizmos()
    {

    }
}
