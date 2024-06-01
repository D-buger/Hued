using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private Vector2 laserShotVector;
    [SerializeField]
    private int laserDamage = 1;

    private LineRenderer lineRenderer;
    private ParticleSystem lineEndParticle;
    private EdgeCollider2D edgeCollider;

    private List<Vector2> edgePoints = new List<Vector2>();

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineEndParticle = transform.GetChild(0).GetComponent<ParticleSystem>();
        edgeCollider = GetComponent<EdgeCollider2D>();
    }

    private void Start()
    {
        laserShotVector = laserShotVector.normalized;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        edgePoints.Add(Vector2.zero);
        edgePoints.Add(Vector2.zero);
        edgeCollider.SetPoints(edgePoints);
    }

    private void Update()
    {
        RaycastHit2D ray = Physics2D.Raycast(transform.position, laserShotVector, float.PositiveInfinity, PlayManager.Instance.PlatformMask);
       
        if(ray.collider != null)
        {
            lineRenderer.SetPosition(1, ray.point);
            edgePoints[1] = lineRenderer.GetPosition(1) - transform.position;
            lineEndParticle.transform.position = ray.point;
            lineEndParticle.Play();
        }
        else 
        {
            lineRenderer.SetPosition(1, Vector3.positiveInfinity);
            edgePoints[1] = lineRenderer.GetPosition(1) - transform.position;
            lineEndParticle.Stop();
        }
        edgeCollider.SetPoints(edgePoints);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.CompareTag(PlayManager.PLAYER_TAG))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            Vector3 position = player.IsDash || player.IsParryDash ? player.PrevDashPosition : collision.transform.position;
            player.transform.position = position;
            player.StopDash = true;
            collision.gameObject.GetComponent<IAttack>().Hit(laserDamage, laserDamage, transform.position - position, null, true);
        }
    }
}
