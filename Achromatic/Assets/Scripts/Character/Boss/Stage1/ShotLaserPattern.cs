using UnityEngine;

public class ShotLaserPattern : BossPattern
{
    [SerializeField]
    private float aimingTime = 2f;
    [SerializeField]
    private float postShotTime = 0.7f;
    [SerializeField]
    private float laserShotTime = 1f;
    [SerializeField]
    private float shotTwiceBetweenDelay = 1f;
    [SerializeField]
    private Vector2 laserShotStartPos;
    [SerializeField]
    private int damage = 5;
    [SerializeField]
    private float patternAfterDelay = 0.5f;

    private LayerMask playerAndGroundMask;
    private Vector2 aimingVector;
    private float elapsedTime = 0;

    private bool isAiming;
    private bool isShooting;
    private bool isShotOnce;
    private bool isPatternEnd;

    public override void OnStart()
    {
        if(PlayManager.Instance.GetPlayer.transform.position.x > boss.transform.position.x)
        {
            laserShotStartPos.x *= -1;
        }

        playerAndGroundMask = LayerMask.GetMask(PlayManager.PLAYER_TAG) | LayerMask.GetMask("Platform");

        isShotOnce = Random.Range(0, 2) == 0 ? true : false;
        isAiming = true;
        isShooting = false;
        isPatternEnd = false;
        elapsedTime = 0;
       
    }

    public override void OnUpdate() 
    {
        elapsedTime += Time.deltaTime;

        if(isAiming)
        {
            aimingVector = ((Vector2)PlayManager.Instance.GetPlayer.transform.position - ((Vector2)boss.transform.position + laserShotStartPos)).normalized;
            if(elapsedTime > aimingTime)
            {
                isAiming = false;
                isShooting = true;
                elapsedTime = 0;
            }
        }
        else if (!isPatternEnd && isShooting && elapsedTime > postShotTime)
        {
            ShotLaser();

            if(elapsedTime - postShotTime > laserShotTime)
            {
                elapsedTime = 0;
                if (isShotOnce)
                {
                    isPatternEnd = true;
                }
                else
                {
                    isShooting = false;
                }
            }
        }
        else if (!isPatternEnd && !isShotOnce && elapsedTime > shotTwiceBetweenDelay)
        {
            ShotLaser();

            if (elapsedTime - postShotTime > laserShotTime)
            {
                elapsedTime = 0;
                isPatternEnd = true;
            }
        }
        else if (isPatternEnd && elapsedTime > patternAfterDelay)
        {
            PatternEnd();
        }

    }

    private void ShotLaser()
    {
        RaycastHit2D hit = Physics2D.Raycast((Vector2)boss.transform.position + laserShotStartPos, aimingVector, Mathf.Infinity, playerAndGroundMask);
        Debug.DrawRay((Vector2)boss.transform.position + laserShotStartPos, aimingVector * Vector2.Distance(hit.point, (Vector2)boss.transform.position + laserShotStartPos), Color.red);

        CheckPlayer(hit);
    }

    private bool CheckPlayer(RaycastHit2D hit)
    {
        if (!ReferenceEquals(hit.collider, null) && hit.collider.CompareTag(PlayManager.PLAYER_TAG))
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
    }
}
