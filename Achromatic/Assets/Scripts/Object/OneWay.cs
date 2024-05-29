using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWay : MonoBehaviour
{
    private Collider2D coll;
    private bool isPlayerOn = false;

    private float disableTime = 0.5f;
    private Coroutine colliderCoroutine;

    private void Awake()
    {
        coll = GetComponent<Collider2D>();

        InputManager.Instance.DownJumpEvent.AddListener(DisableCollider);
    }

    private void DisableCollider()
    {
        if (isPlayerOn && colliderCoroutine == null)
        {
            colliderCoroutine = StartCoroutine(ColliderSet());
        }
    }

    private IEnumerator ColliderSet()
    {
        coll.enabled = false;
        yield return Yields.WaitSeconds(disableTime);
        coll.enabled = true;
        colliderCoroutine = null;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            isPlayerOn = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            isPlayerOn = false;
        }
    }
}