using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [SerializeField]
    private Sprite breakSprite;

    private Sprite originSprite;
    private SpriteRenderer renderer;
    private Collider2D coll;
    private ParticleSystem particle;

    private bool isBreak = false;
    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
        particle = GetComponent<ParticleSystem>();
    }
    private void Start()
    {
        originSprite = renderer.sprite;
    }
    private void BreakAction()
    {
        isBreak = true;
        coll.enabled = false;
        renderer.sprite = breakSprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayManager.ATTACK_TAG) && string.Equals(collision.GetComponent<Attack>()?.AttackOwner, PlayManager.PLAYER_TAG))
        {
            BreakAction();
        }
    }
}
