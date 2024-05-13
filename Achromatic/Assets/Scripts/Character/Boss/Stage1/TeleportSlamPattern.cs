using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TeleportSlamPattern : BossPattern
{
    [SerializeField]
    private Vector2 teleportPositionOffset;
    [SerializeField]
    private float afterTeleportDelay = 1f;
    [SerializeField]
    private float slamSpeed = 2f;
    [SerializeField]
    private int slamDamage = 10;
    [SerializeField]
    private float afterSlamDelay = 1f;
    [SerializeField]
    private Vector2[] afterSlamLandingPositions;
    [SerializeField]
    private float floorCheckOffset = 1f;
    [SerializeField]
    private float slamSizeOffset = 0.3f;

    private Vector2 originBossPosition;
    private float originGravityScale;
    private float elapsedTime = 0;

    private bool isSlamEnd;
    private bool isAlreadySlam;
    private bool isPatternEnd;

    public override void OnStart()
    {
        originBossPosition = transform.position;
        originGravityScale = boss.RigidbodyComp.gravityScale;
        boss.RigidbodyComp.gravityScale = 0;
        boss.transform.position = PlayManager.Instance.GetPlayer.transform.position + (Vector3)teleportPositionOffset;
        elapsedTime = 0;
        isSlamEnd = false;
        isAlreadySlam = false;
        isPatternEnd = false;
    }

    public override void OnUpdate()
    {
        elapsedTime += Time.deltaTime;

        RaycastHit2D hit = Physics2D.BoxCast(boss.transform.position, new Vector2(boss.ColliderComp.bounds.size.x + slamSizeOffset, boss.ColliderComp.bounds.size.y + slamSizeOffset)
            , 0, Vector2.down, 0, LayerMask.GetMask(PlayManager.PLAYER_TAG));

        if (!isSlamEnd && elapsedTime > afterTeleportDelay)
        {
            boss.RigidbodyComp.gravityScale = originGravityScale;
            boss.RigidbodyComp.velocity = Vector2.down * slamSpeed;

            Vector2 floorCheckVector = boss.transform.position;
            floorCheckVector.y -= floorCheckOffset;
            RaycastHit2D floorHit = Physics2D.BoxCast(floorCheckVector, boss.ColliderComp.bounds.size, 0, Vector2.zero, Mathf.Infinity, PlayManager.Instance.PlatformMask);

            bool isPlayerDetected = CheckPlayer(hit);

            if ((!ReferenceEquals(floorHit.collider, null) && floorHit.collider.CompareTag(PlayManager.FLOOR_TAG)) || isPlayerDetected)
            {
                if(!isAlreadySlam && Random.Range(0, 2) == 0)
                {
                    boss.transform.position = PlayManager.Instance.GetPlayer.transform.position + (Vector3)teleportPositionOffset;
                    boss.RigidbodyComp.velocity = Vector2.zero;
                    boss.RigidbodyComp.gravityScale = 0;
                    isAlreadySlam = true;
                    elapsedTime = 0;
                }
                else
                {
                    boss.RigidbodyComp.gravityScale = originGravityScale;
                    isSlamEnd = true;
                    elapsedTime = isPlayerDetected == true ? afterSlamDelay : 0;
                }
            }
        }
        else if (!isPatternEnd && isSlamEnd && elapsedTime >= afterSlamDelay)
        {
            float nowDistance;
            float fartestDistance = Vector2.Distance(afterSlamLandingPositions[0] + originBossPosition, PlayManager.Instance.GetPlayer.transform.position);
            int fartestIndex = 0;
            for(int i = 1; i < afterSlamLandingPositions.Length; i++)
            {
                nowDistance = Vector2.Distance(afterSlamLandingPositions[i] + originBossPosition, PlayManager.Instance.GetPlayer.transform.position);
                if (fartestDistance < nowDistance)
                {
                    fartestDistance = nowDistance;
                    fartestIndex = i;
                }
            }
            boss.transform.position = afterSlamLandingPositions[fartestIndex] + originBossPosition;
            isPatternEnd = true;
        }
        else if (isPatternEnd)
        {
            PatternEnd();
        }
    }

    private bool CheckPlayer(RaycastHit2D hit)
    {
        if (!ReferenceEquals(hit.collider, null) && hit.collider.CompareTag(PlayManager.PLAYER_TAG))
        {
            hit.collider.gameObject.GetComponent<IAttack>().Hit(slamDamage, slamDamage
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
        Gizmos.color = Color.blue;
        if (Application.isPlaying)
        {
            for (int i = 0; i < afterSlamLandingPositions.Length; i++)
            { 
                Gizmos.DrawWireSphere(afterSlamLandingPositions[i] + originBossPosition, 1);
            }
        }

        if (!isSlamEnd)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boss.transform.position, new Vector2(boss.ColliderComp.bounds.size.x + slamSizeOffset, boss.ColliderComp.bounds.size.y + slamSizeOffset));
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(new Vector2(boss.transform.position.x, boss.transform.position.y - floorCheckOffset), boss.ColliderComp.bounds.size);
        }
    }
}
