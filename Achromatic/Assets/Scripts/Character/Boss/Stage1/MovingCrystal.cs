using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MovingCrystal : MonoBehaviour, IParryConditionCheck
{
    [SerializeField]
    private float moveSpeed = 0.7f;
    [SerializeField]
    private float changedSpeedByBossHp = 1f;
    [SerializeField]
    private eTwoDirection moveDirection;
    [SerializeField]
    private float eachCrystalDistance = 5;
    [SerializeField]
    private float maxMoveDistance = 5;
    [SerializeField]
    private float chargingTime = 0.2f;
    [SerializeField]
    private float laserRemainTime = 0.5f;
    [SerializeField]
    private float laserShotDuration = 8;
    [SerializeField]
    private int crystalDamage = 5;
    [SerializeField]
    private int laserDamage = 10;

    private float currentSpeed;
    private float elapsedTime = 0;
    private float upSideDownAngle = 180;
    private int direction = 1;
    private Vector2 anotherCrystalPosition = Vector2.zero;
    private Vector2 originalPosition = Vector2.zero;
    private Vector2 moveEndPosition = Vector2.zero;

    private bool isMove = true;
    private bool isShotLaser = false;

    private void Awake()
    {
        originalPosition = transform.position;
        moveEndPosition = transform.position;
        moveEndPosition += moveDirection == eTwoDirection.HORIZONTAL ?
            new Vector2(maxMoveDistance, 0) : new Vector2(0, maxMoveDistance);
        anotherCrystalPosition = transform.position;
        anotherCrystalPosition += moveDirection == eTwoDirection.HORIZONTAL ?
            new Vector2(0, eachCrystalDistance) : new Vector2(eachCrystalDistance, 0);

        GameObject anotherCrystal = new GameObject("AnotherCrystal");
        anotherCrystal.AddComponent<SpriteRenderer>().sprite = GetComponent<SpriteRenderer>().sprite;
        anotherCrystal.AddComponent<BoxCollider2D>().isTrigger = true;
        anotherCrystal.transform.position = anotherCrystalPosition;
        anotherCrystal.transform.parent = transform;
        anotherCrystal.transform.rotation = Quaternion.Euler(0, 0,upSideDownAngle);
    }

    private void OnEnable()
    {
        currentSpeed = moveSpeed;
        isShotLaser = false;
        isMove = true;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        MoveCrystal();
        LaserBehaviour();
    }

    private void MoveCrystal()
    {
        if ((moveDirection == eTwoDirection.HORIZONTAL &&
            (transform.position.x > moveEndPosition.x || transform.position.x < originalPosition.x))
            || (moveDirection == eTwoDirection.VERTICAL &&
            (transform.position.y > moveEndPosition.y || transform.position.y < originalPosition.y)))
        {
            direction *= -1;
        }

        if (isMove)
        {
            transform.position += (moveDirection == eTwoDirection.HORIZONTAL ?
                new Vector3(Time.deltaTime, 0) : new Vector3(0, Time.deltaTime)) * direction * currentSpeed;
        }
    }

    private void LaserBehaviour()
    {
        if(elapsedTime > laserShotDuration)
        {
            isShotLaser = elapsedTime >= laserShotDuration + chargingTime ? true : false;

            if(elapsedTime < laserShotDuration + chargingTime)
            {
                // ย๗ยก
            }
            else if(isShotLaser && elapsedTime < laserShotDuration + laserRemainTime)
            {
                RaycastHit2D laserHit = Physics2D.Linecast(transform.position, transform.GetChild(0).position);

                CheckPlayer(laserHit);
            }
            else
            {
                isShotLaser = false;
                elapsedTime = 0;
            }
        }
    }
    private bool CheckPlayer(RaycastHit2D hit)
    {
        if (!ReferenceEquals(hit.collider, null) && hit.collider.CompareTag(PlayManager.PLAYER_TAG))
        {
            hit.collider.gameObject.GetComponent<IAttack>().Hit(laserDamage, laserDamage
                , Vector2.zero, this);

            return true;
        }
        return false;
    }

    public void ChangeSpeed()
    {
        currentSpeed = changedSpeedByBossHp;
    }
    public bool CanParryAttack()
    {
        return false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        collision.GetComponent<IAttack>().Hit(crystalDamage, crystalDamage, Vector2.zero, this);
    }

    private void OnDrawGizmos()
    {
        Vector2 gizmoStartPosition, gizmoEndPosition;
        gizmoStartPosition = Application.isPlaying ? originalPosition : transform.position;
        gizmoEndPosition = gizmoStartPosition;

        if (moveDirection == eTwoDirection.HORIZONTAL)
        {
            gizmoEndPosition.x += maxMoveDistance;
        }
        else
        {
            gizmoEndPosition.y += maxMoveDistance;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(gizmoStartPosition, gizmoEndPosition);

        if (isShotLaser)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.GetChild(0).position);
        }
    }

}
