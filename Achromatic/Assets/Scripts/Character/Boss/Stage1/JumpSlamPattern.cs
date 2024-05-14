using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpSlamPattern : BossPattern
{
    [SerializeField]
    private float prepareTime = 0.5f;
    [SerializeField]
    private float slamHeight = 5f;
    [SerializeField]
    private float slamSpeed = 1f;
    [SerializeField]
    private int damage = 7;
    [SerializeField]
    private float slamPostWaitTime = 0.5f;
    [SerializeField]
    private AnimationCurve slamPostBehaviourCurve;
    [SerializeField]
    private float slamAfterMiddlePositionY = 5f;
    [SerializeField]
    private float slamAfterDelay = 1f;
    [SerializeField]
    private float patternAfterDelay = 1f;

    [SerializeField]
    private float slamSizeOffset = 0.3f;
    [SerializeField]
    private float floorCheckOffset = 1f;
    [SerializeField]
    private float returnToOriginSpeed = 10;
    [SerializeField]
    private int bezierCurvePointNum = 20;

    Vector2[] bezierResults;
    private Vector2 originBossPos;
    private Vector2 slamPosition;
    private float elapsedTime;

    private bool isChosenSlamPosition;
    private bool isSlamPostBehaviour;
    private bool isSlamEnd;
    private bool isPatternEnd;

    public override void OnStart()
    {
        bezierResults = null;
        originBossPos = boss.transform.position;
        elapsedTime = 0;
        isSlamPostBehaviour = true;
        isChosenSlamPosition = false;
        isSlamEnd = false;
        isPatternEnd = false;
    }
    public override void OnUpdate()
    {
        elapsedTime += Time.deltaTime;

        RaycastHit2D hit = Physics2D.BoxCast(boss.transform.position, new Vector2(boss.ColliderComp.bounds.size.x + slamSizeOffset, boss.ColliderComp.bounds.size.y + slamSizeOffset)
            , 0, Vector2.down, 0, LayerMask.GetMask(PlayManager.PLAYER_TAG));

        if (!isChosenSlamPosition && elapsedTime > prepareTime)
        {
            isChosenSlamPosition = true;
            elapsedTime = 0;
            slamPosition.x = PlayManager.Instance.GetPlayer.transform.position.x;
            slamPosition.y = originBossPos.y + slamHeight;
        }
        else if (isChosenSlamPosition && isSlamPostBehaviour)
        {
            boss.transform.position = originBossPos + ((slamPosition - originBossPos) * slamPostBehaviourCurve.Evaluate(elapsedTime));
            if (elapsedTime > 1)
            {
                isSlamPostBehaviour = false;
                elapsedTime = 0;
            }
        }
        else if(isChosenSlamPosition && !isSlamEnd && elapsedTime > slamPostWaitTime)
        {
            boss.RigidbodyComp.velocity = Vector2.down * slamSpeed;

            Vector2 floorCheckVector = boss.transform.position;
            floorCheckVector.y -= floorCheckOffset;
            RaycastHit2D floorHit = Physics2D.BoxCast(floorCheckVector, boss.ColliderComp.bounds.size, 0, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Platform"));

            if (!ReferenceEquals(floorHit.collider, null) && floorHit.collider.CompareTag(PlayManager.FLOOR_TAG))
            {
                slamPosition = boss.transform.position;
                boss.RigidbodyComp.velocity = Vector2.zero;
                isSlamEnd = true;
                elapsedTime = 0;
            }
        }
        else if (isChosenSlamPosition && !isPatternEnd && elapsedTime > slamAfterDelay)
        {
            if (ReferenceEquals(bezierResults, null)) {
                Vector2 bezierLeftUpVector = new Vector2(slamPosition.x, slamPosition.y + slamAfterMiddlePositionY);
                Vector2 bezierRightUpVector = new Vector2(originBossPos.x, originBossPos.y + slamAfterMiddlePositionY);
                
                Vector2[] bezierCurveVectors = new Vector2[]
                {
                slamPosition,
                bezierLeftUpVector,
                bezierRightUpVector,
                originBossPos
                };
                bezierResults = SOO.Util.CurvePointsOfVectors(bezierCurvePointNum, bezierCurveVectors);
            }

            boss.transform.position = bezierResults[(int)((elapsedTime - slamAfterDelay)* returnToOriginSpeed)];

            if ((int)((elapsedTime - slamAfterDelay) * returnToOriginSpeed) >= bezierCurvePointNum)
            {
                isPatternEnd = true;
                elapsedTime = 0;
            }
        }
        else if (isChosenSlamPosition && isPatternEnd && elapsedTime > patternAfterDelay)
        {
            PatternEnd();
        }

        if(!isSlamPostBehaviour && !isSlamEnd && CheckPlayer(hit))
        {
            slamPosition = boss.transform.position;
            boss.RigidbodyComp.velocity = Vector2.zero;
            isSlamEnd = true;
            elapsedTime = 0;
        }
    }
    private bool CheckPlayer(RaycastHit2D hit)
    {
        if(!ReferenceEquals(hit.collider, null) && hit.collider.CompareTag(PlayManager.PLAYER_TAG))
        {
            hit.collider.gameObject.GetComponent<IAttack>().Hit(damage, damage
                , boss.transform.position - hit.transform.position, this);

            return true;
        }
        return false;
    }
    public override bool CanParryAttack()
    {
        return false;
    }
    public override void DrawGizmos()
    {
        if (!isSlamPostBehaviour)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boss.transform.position, new Vector2(boss.ColliderComp.bounds.size.x + slamSizeOffset, boss.ColliderComp.bounds.size.y + slamSizeOffset));
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(new Vector2(boss.transform.position.x, boss.transform.position.y - floorCheckOffset), boss.ColliderComp.bounds.size);
        }
    }
}
