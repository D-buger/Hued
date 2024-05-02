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
    private float attackAfterDelay = 1f;

    private Vector2 originBossPos;
    private Vector2 slamPosition;
    private float slamSizeOffset = 2f;
    private float floorCheckOffset = 1f;
    private float elapsedTime;

    private bool isChosenSlamPosition;
    private bool isSlamPostBehaviour;
    private bool isPatternEnd;

    public override void OnStart()
    {
        originBossPos = boss.transform.position;
        elapsedTime = 0;
        isSlamPostBehaviour = true;
        isChosenSlamPosition = false;
        isPatternEnd = false;
    }
    public override void OnUpdate()
    {
        elapsedTime += Time.deltaTime;

        if(!isChosenSlamPosition && elapsedTime > prepareTime)
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
        else if(isChosenSlamPosition && !isPatternEnd && elapsedTime > slamPostWaitTime)
        {
            boss.RigidbodyComp.velocity = Vector2.down * slamSpeed;

            RaycastHit2D hit = Physics2D.BoxCast(boss.transform.position, new Vector2(boss.ColliderComp.bounds.size.x + slamSizeOffset, boss.ColliderComp.bounds.size.y + slamSizeOffset)
                , 0, Vector2.down, Mathf.Infinity, LayerMask.GetMask(PlayManager.PLAYER_TAG));
            CheckPlayer(hit);

            Vector2 floorCheckVector = boss.transform.position;
            floorCheckVector.y -= floorCheckOffset;
            RaycastHit2D floorHit = Physics2D.BoxCast(floorCheckVector, boss.ColliderComp.bounds.size, 0, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Platform"));
            if (!ReferenceEquals(floorHit.collider, null) && floorHit.collider.CompareTag(PlayManager.FLOOR_TAG))
            {
                boss.RigidbodyComp.velocity = Vector2.zero;
                isPatternEnd = true;
                elapsedTime = 0;
            }
        }
        else if (isChosenSlamPosition && elapsedTime > attackAfterDelay)
        {
            PatternEnd();
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
