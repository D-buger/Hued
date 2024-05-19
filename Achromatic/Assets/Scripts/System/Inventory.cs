using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    private Item[] expendableItems;
    [SerializeField]
    private Item[] equippableItems;
    [SerializeField]
    private GameObject expendableItemEquipCompartmentParent;
    [SerializeField]
    private GameObject equippableItemEquipCompartmentParent;
    [SerializeField]
    private GameObject expendableItemCompartmentParent;
    [SerializeField]
    private GameObject equippableItemCompartmentParent;

    private InventoryCompartment[] expendableItemEquipCompartments;
    private InventoryCompartment[] equippableItemEquipCompartments;
    private InventoryCompartment[] expendableItemCompartments;
    private InventoryCompartment[] equippableItemCompartments;

    private void Awake()
    {
        expendableItemEquipCompartments = expendableItemEquipCompartmentParent.GetComponentsInChildren<InventoryCompartment>(true);
        equippableItemEquipCompartments = equippableItemEquipCompartmentParent.GetComponentsInChildren<InventoryCompartment>(true);
        expendableItemCompartments = expendableItemCompartmentParent.GetComponentsInChildren<InventoryCompartment>(true);
        equippableItemCompartments = equippableItemCompartmentParent.GetComponentsInChildren<InventoryCompartment>(true);

        for (int i = 0; i < expendableItemCompartments.Length; i++)
        {
            expendableItemCompartments[i].SetItem(expendableItems[i]);
        }
    }

    private void Start()
    {
        InputManager.Instance.InventoryEvent?.AddListener(() => SetActiveInventory(true));
    }

    public void SetActiveInventory(bool active)
    {
        gameObject.SetActive(active);
        Time.timeScale = active ? 0.0f : 1.0f;
        InputManager.Instance.CanInput = !active;
    }

    public void SetExplanationTap(Item item)
    {
        Debug.Log(item);
    }
}
