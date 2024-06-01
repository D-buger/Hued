using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class ItemTestObject : MonoBehaviour
{
    [SerializeField]
    private ExpendableItem expend;
    [SerializeField]
    private EquippableItem equip;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayManager.PLAYER_TAG))
        {
            if(expend is not null)
            {
                PlayManager.Instance.GetInventory.GetItem(expend);
            }
            else
            {
                PlayManager.Instance.GetInventory.GetItem(equip);
            }
            gameObject.SetActive(false);
        }
    }
}
