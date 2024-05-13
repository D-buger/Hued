using UnityEngine;

public class SwingPattern : BossPattern
{
    [SerializeField]
    private float swingRange = 2.5f;

    [SerializeField]
    private float postSwingSpeed = 0.1f;
    [SerializeField]
    private float swingFirstHandSpeed = 1.2f;
    [SerializeField]
    private float swingSecondHandSpeed = 0.7f;
    [SerializeField]
    private Vector2 swingPostPosition = Vector2.zero;

    [SerializeField]
    private float swingDelay = 0.5f;
    [SerializeField]
    private float patternAfterDelay = 1f;

    [SerializeField]
    private int damage = 5;

    private Vector2 initPosition;
    private Vector2 curHandPosition;

    private bool isFirstAttack;
    private bool isSwingPostBehaviour;
    private bool isPatternEnd;

    private float elapsedTime = 0f;

    public override void OnStart()
    {
        if(PlayManager.Instance.GetPlayer.transform.position.x > boss.transform.position.x)
        {
            swingPostPosition.x *= swingPostPosition.x < 0 ? -1 : 1;
        }
        else
        {
            swingPostPosition.x *= swingPostPosition.x > 0 ? -1 : 1;
        }

        curHandPosition = boss.transform.position;
        initPosition = curHandPosition;
        elapsedTime = 0f;
        isFirstAttack = true;
        isSwingPostBehaviour = true;
        isPatternEnd = false;
    }

    public override void OnUpdate()
    {
        elapsedTime += Time.deltaTime;

        RaycastHit2D hit = Physics2D.BoxCast(curHandPosition, new Vector2(swingRange, swingRange), 0, Vector2.zero);

        if (isFirstAttack && !isPatternEnd)
        {
            if (isSwingPostBehaviour) {
                SwingPostBehaviour();
            }
            else
            {
                if (!ReferenceEquals(hit.collider, null) && hit.collider.CompareTag(PlayManager.FLOOR_TAG))
                {
                    elapsedTime = 0;
                    curHandPosition = initPosition;
                    isSwingPostBehaviour = true;
                    isFirstAttack = false;
                }
                else
                {
                    curHandPosition += Vector2.down * swingFirstHandSpeed;
                }
            }
        }
        else if(!isPatternEnd && elapsedTime > swingDelay)
        {
            if (isSwingPostBehaviour)
            {
                SwingPostBehaviour();
            }
            else
            {
                if (!ReferenceEquals(hit.collider, null) && hit.collider.CompareTag(PlayManager.FLOOR_TAG))
                {
                    isPatternEnd = true;
                    elapsedTime = 0;
                }
                else
                {
                    curHandPosition += Vector2.down * swingSecondHandSpeed;
                }
            }
        }
        else if(isPatternEnd && elapsedTime > patternAfterDelay)
        {
            PatternEnd();
        }

        CheckPlayer(hit);
    }

    private void SwingPostBehaviour()
    {
        curHandPosition = Vector2.Lerp(initPosition, initPosition + swingPostPosition, elapsedTime * postSwingSpeed);

        if (elapsedTime * postSwingSpeed > 1)
        {
            isSwingPostBehaviour = false;
            elapsedTime = 0f;
        }
    }

    private void CheckPlayer(RaycastHit2D hit)
    {
        if (!ReferenceEquals(hit.collider, null) && hit.collider.CompareTag(PlayManager.PLAYER_TAG))
        {
            hit.collider.gameObject.GetComponent<IAttack>().Hit(damage, damage
                ,boss.transform.position - hit.transform.position, this);
        }
    }

    public override bool CanParryAttack()
    {
        return base.CanParryAttack();
    }
    
    public override void DrawGizmos() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(curHandPosition, new Vector2(swingRange, swingRange));
    }
}
