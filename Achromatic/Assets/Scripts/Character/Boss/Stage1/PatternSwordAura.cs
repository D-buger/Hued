using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternSwordAura : MonoBehaviour, IParryConditionCheck
{
    private eActivableColor patternColor;
    private Vector2 swordAuraMoveDirection;
    private float swordAuraMoveSpeed;
    private int swordAuraDamage;

    private bool isShot = false;

    private void Update()
    {
        if (!isShot)
        {
            return;
        }

        transform.position += (Vector3)swordAuraMoveDirection * swordAuraMoveSpeed;
    }

    public void Shot(eActivableColor color, Vector2 direction, float speed, int damage)
    {
        patternColor = color;
        swordAuraMoveDirection = direction;
        swordAuraMoveSpeed = speed;
        swordAuraDamage = damage;
    }

    public bool CanParryAttack()
    {
        return PlayManager.Instance.ContainsActivationColors(patternColor);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(PlayManager.PLAYER_TAG))
        {

        }
    }
}
