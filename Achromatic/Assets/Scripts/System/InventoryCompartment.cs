using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
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

    public void SetItem(Item item, Color color)
    {
        this.item = item;
        imageComponent.sprite = item.itemSprite;
        imageComponent.color = color;
    }
    public Item GetItem() => item;
    public bool HasItem() => item is not null;
    public bool CompareItem(Item item) => this.item == item;

    public void Clear()
    {
        if (HasItem())
        {
            item.isEquipped = false;
        }

        item = null;
        imageComponent.sprite = default;
        imageComponent.color = new Color(0, 0, 0, 0);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(item is not null)
        {
            Inventory.Explanation.SetExplanation(item);
        }
        else
        {
            Inventory.Explanation.Clear();
        }
    }
}
