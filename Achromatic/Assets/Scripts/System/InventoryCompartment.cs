using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryCompartment : MonoBehaviour, IPointerClickHandler
{
    private Image imageComponent;
    private Inventory Inventory => PlayManager.Instance.GetInventory;
    private Item item = null;

    private void Awake()
    {
        imageComponent = GetComponent<Image>();
    }

    public void SetItem(Item item, Color color = default)
    {
        this.item = item;
        imageComponent.sprite = item.itemSprite;
        imageComponent.color = color;
    }

    public void Clear()
    {
        item = null;
        imageComponent.sprite = default;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(gameObject.name);
        Inventory.SetExplanationTap(item);
    }
}
