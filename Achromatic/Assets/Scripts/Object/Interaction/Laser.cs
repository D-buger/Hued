using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private Vector2 laserShotVector;
    [SerializeField]
    private int laserDamage = 1;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        laserShotVector = laserShotVector.normalized;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
    }

    private void Update()
    {
        RaycastHit2D ray = Physics2D.Raycast(transform.position, laserShotVector, float.PositiveInfinity, PlayManager.Instance.PlatformMask | PlayManager.Instance.PlayerMask);
       
        if(ray.collider != null)
        {
            if (ray.collider.CompareTag(PlayManager.PLAYER_TAG))
            {
                ray.collider.gameObject.GetComponent<IAttack>().Hit(laserDamage, laserDamage, transform.position - ray.collider.transform.position, null, true);
            }
            else
            {
                lineRenderer.SetPosition(1, ray.point);
            }
        }
        else 
        {
            lineRenderer.SetPosition(1, Vector3.positiveInfinity);
        }
    }



}
