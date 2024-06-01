using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Colors : MonoBehaviour
{
    [SerializeField]
    eActivableColor color;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayManager.PLAYER_TAG))
        {
            PlayManager.Instance.ActivationColors = color;
            gameObject.SetActive(false);
        }
    }
}
