using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Monster;
using static UnityEngine.Rendering.DebugUI;

public class Turret : MonoBehaviour
{
    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;
    private Projectile rangedAttack;
    [SerializeField]
    private TurretStat stat;

    private bool canAttack = true;

    float distanceToPlayer;
    void Update()
    {
        distanceToPlayer = Vector2.Distance(transform.position, PlayerPos);
        if (distanceToPlayer <= stat.senseCircle && canAttack)
        {
            StartCoroutine(TurretAttack());
        }
        Debug.Log(distanceToPlayer);
    }

    private IEnumerator TurretAttack()
    {
        canAttack = false;
        GameObject projectileObj = ObjectPoolManager.instance.GetProjectileFromPool(0);
        Vector2 value = PlayerPos - (Vector2)transform.position;
        float zAngle = (Mathf.Atan2(PlayerPos.y - transform.position.y, PlayerPos.x - transform.position.x) * Mathf.Rad2Deg) + stat.projectileZAngleByHeight;
        if (projectileObj is not null)
        {
            projectileObj.SetActive(true);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile is not null)
            {
                projectile.Shot(gameObject, gameObject.transform.position, value.normalized,
                    stat.turretAttackDuration, stat.turretAttackSpeed, stat.turretAttackDamage, zAngle, eActivableColor.RED);
                projectileObj.transform.position = transform.transform.position;

                PlayManager.Instance.UpdateColorthing();
                projectile.ReturnStartRoutine(stat.turretAttackDuration);
            }
        }
        yield return Yields.WaitSeconds(stat.turretAttackDelay);
        canAttack = true;
    }
}
