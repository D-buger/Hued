using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    protected abstract void TriggerEnterBehaviour();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayManager.PLAYER_TAG))
        {
            TriggerEnterBehaviour();
            gameObject.SetActive(false);
            //Destroy(gameObject);
        }
    }

}
