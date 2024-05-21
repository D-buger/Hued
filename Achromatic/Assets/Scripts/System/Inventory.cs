using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour, IPointerClickHandler
{
    private readonly Color ACTIVE_COLOR = Color.white;
    private readonly Color INACTIVE_COLOR = new Color(0.1f, 0.1f, 0.1f, 1);

    [SerializeField]
    private GameObject expendableItemEquipCompartmentParent;
    [SerializeField]
    private GameObject equippableItemEquipCompartmentParent;
    [SerializeField]
    private GameObject expendableItemCompartmentParent;
    [SerializeField]
    private GameObject equippableItemCompartmentParent;

    [SerializeField]
    private ExpendableItem[] expendables;

    private List<ExpendableItem> expendableItems = new List<ExpendableItem>();
    private List<EquippableItem> equippableItems = new List<EquippableItem>();

    private InventoryCompartment[] expendableItemEquipCompartments;
    private InventoryCompartment[] equippableItemEquipCompartments;
    private InventoryCompartment[] expendableItemCompartments;
    private InventoryCompartment[] equippableItemCompartments;

    private Explanation explanation;
    public Explanation Explanation => explanation;

    private void Awake()
    {
        expendableItemEquipCompartments = expendableItemEquipCompartmentParent.GetComponentsInChildren<InventoryCompartment>(true);
        equippableItemEquipCompartments = equippableItemEquipCompartmentParent.GetComponentsInChildren<InventoryCompartment>(true);
        expendableItemCompartments = expendableItemCompartmentParent.GetComponentsInChildren<InventoryCompartment>(true);
        equippableItemCompartments = equippableItemCompartmentParent.GetComponentsInChildren<InventoryCompartment>(true);
        explanation = GetComponentInChildren<Explanation>(true);
        Explanation.Clear();

        expendableItems.AddRange(expendables);
    }

    private void Start()
    {
        InputManager.Instance.InventoryEvent?.AddListener(() => SetActiveInventory(true));
        InputManager.Instance.ExitEvent?.AddListener(() => SetActiveInventory(false));
        SetExpendableItem();
    }

    public void SetActiveInventory(bool active)
    {
        Time.timeScale = active ? 0.0f : 1.0f;
        InputManager.Instance.CanInput = !active;
        gameObject.SetActive(active);
    }

    public void GetItem(ExpendableItem item)
    {
        ExpendableItem searchedItem = expendableItems.Find(value => value == item);
        if (searchedItem is not null)
        {
            searchedItem.isDiscovered = true;
            SetExpendableItem();
        }
    }

    public void GetItem(EquippableItem item)
    {
        if (equippableItems.Contains(item)) 
        {
            return;
        }

        equippableItems.Add(item);
        SetEquippableItem();
    }

    private void SetExpendableItem()
    {
        for (int i = 0; i < expendableItems.Count; i++)
        {
            expendableItemCompartments[i].SetItem(expendableItems[i], expendableItems[i].isDiscovered ? ACTIVE_COLOR : INACTIVE_COLOR);
        }
    }

    private void SetEquippableItem()
    {
        for(int i = 0; i < equippableItems.Count; i++)
        {
            equippableItemCompartments[i].SetItem(equippableItems[i], ACTIVE_COLOR);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Explanation.Clear();
    }
}
