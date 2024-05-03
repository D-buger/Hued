using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternCrystal : MonoBehaviour, IParryConditionCheck
{
    private eActivableColor patternColor;

    private int crystalDamage;
    private int explosionDamage;
    private float moveSpeed;
    private float elapsedTime;

    private float explosionRadius;
    private float postExplosionDelay;
    private float explosionTime;

    private Vector2 moveDirection;

    private bool isShot = false;
    private bool isTouchFloor = false;
    private bool isExplosion = false;

    private void Update()
    {
        if (isShot && !isTouchFloor)
        {
            transform.Translate(moveDirection * moveSpeed);
        }
        else if(!isExplosion)
        {
            elapsedTime += Time.deltaTime;

            if(elapsedTime > postExplosionDelay)
            {
                isExplosion = true;
                elapsedTime = 0;
            }
        }
        else
        {
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, explosionRadius, Vector2.zero, 0, LayerMask.GetMask(PlayManager.PLAYER_TAG));
            CheckPlayer(hit);

            if (elapsedTime > explosionTime)
            {
                DisableCrystal();
            }
        }
    }
    private void CheckPlayer(RaycastHit2D hit)
    {
        if (!ReferenceEquals(hit.collider, null) && hit.collider.CompareTag(PlayManager.PLAYER_TAG))
        {
            hit.collider.gameObject.GetComponent<IAttack>().Hit(explosionDamage, explosionDamage
                , transform.position - hit.transform.position, this);

        }
    }

    public PatternCrystal SettingFirst(int cryDmg, int expDmg, float speed, float radius, float delay, float time)
    {
        crystalDamage = cryDmg;
        explosionDamage = expDmg;
        moveSpeed = speed;
        explosionRadius = radius;
        postExplosionDelay = delay;
        explosionTime = time;
        return this;
    }

    public void Shot(Vector2 dir)
    {
        moveDirection = dir;
        isShot = true;
        isTouchFloor = false;
        isExplosion = false;
        elapsedTime = 0f;
    }

    private void DisableCrystal()
    {
        gameObject.SetActive(false);
    }
    public bool CanParryAttack()
    {
        return PlayManager.Instance.ContainsActivationColors(patternColor) && isShot;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayManager.FLOOR_TAG))
        {
            isShot = false;
            isTouchFloor = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (isExplosion)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
