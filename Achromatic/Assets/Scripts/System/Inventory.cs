using System;
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
    [Space(10), Header("equppable items compartments")]
    [SerializeField]
    private float xSwipeValue = 200;
    [SerializeField]
    private float swipeDurationTime = 0.2f;
    [Space(10), Header("expendable items compartments")]
    [SerializeField]
    private float sizeUpScaleValue = 1.2f;
    [SerializeField]
    private float sizeChangeDurationTime = 0.2f;

    private List<ExpendableItem> expendableItems = new List<ExpendableItem>();
    private List<EquippableItem> equippableItems = new List<EquippableItem>();

    private InventoryCompartment expendableItemEquipCompartment;
    private InventoryCompartment[] equippableItemEquipCompartments;
    private InventoryCompartment[] expendableItemCompartments;
    private InventoryCompartment[] equippableItemCompartments;

    private RectTransform equippableItemParentRect;
    private RectTransform[] expendableItemRects;

    private Explanation explanation;
    public Explanation Explanation => explanation;

    private Coroutine swipeCoroutine;
    private Coroutine[] sizeChangeCoroutines;

    private Vector3 equippableItemParentOriPosition;

    private int equippableItemIndex = 0;
    private int equippableItemRectIndex = 0;

    private void Awake()
    {
        expendableItemEquipCompartment = expendableItemEquipCompartmentParent.GetComponentInChildren<InventoryCompartment>(true);
        equippableItemEquipCompartments = equippableItemEquipCompartmentParent.GetComponentsInChildren<InventoryCompartment>(true);
        expendableItemCompartments = expendableItemCompartmentParent.GetComponentsInChildren<InventoryCompartment>(true);
        equippableItemCompartments = equippableItemCompartmentParent.GetComponentsInChildren<InventoryCompartment>(true);
        explanation = GetComponentInChildren<Explanation>(true);
        equippableItemParentRect = equippableItemCompartmentParent.GetComponent<RectTransform>();
        equippableItemParentOriPosition = equippableItemParentRect.position;
        expendableItemRects = new RectTransform[expendableItemCompartments.Length];
        for (int i = 0; i < expendableItemCompartments.Length; i++)
        {
            expendableItemRects[i] = expendableItemCompartments[i].transform.parent.GetComponent<RectTransform>();
        }
        sizeChangeCoroutines = new Coroutine[expendableItemRects.Length];
        Explanation.Clear();

        expendableItems.AddRange(expendables);
    }

    private void Start()
    {
        InputManager.Instance.InventoryEvent?.AddListener(() => SetActiveInventory(true));
        InputManager.Instance.ExitEvent?.AddListener(() => SetActiveInventory(false));
        InputManager.Instance.UseItemEvent?.AddListener(UseItem);

        expendableItemEquipCompartment.Clear();
        for(int i =0; i < equippableItemEquipCompartments.Length; i++)
        {
            equippableItemEquipCompartments[i].Clear();
        }

        SetExpendableItem();
        gameObject.SetActive(false);
    }

    public void SetActiveInventory(bool active)
    {
        Time.timeScale = active ? 0.0f : 1.0f;
        InputManager.Instance.CanInput = !active;
        explanation.gameObject.SetActive(false);
        gameObject.SetActive(active);
        equippableItemRectIndex = 0;
        equippableItemParentRect.position = equippableItemParentOriPosition;
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

    public void EquipItem(Item item, bool equip)
    {
        switch (item.ItemType())
        {
            case EItemType.EQUIPPABLE:
                if (equip)
                {
                    EquippableItem equipItem = item as EquippableItem;
                    equipItem.isEquipped = true;
                    equipItem.EquipItem();
                    (equippableItemEquipCompartments[equippableItemIndex].GetItem() as EquippableItem)?.DisarmItem();
                    equippableItemEquipCompartments[equippableItemIndex].Clear();
                    equippableItemEquipCompartments[equippableItemIndex].SetItem(equipItem, ACTIVE_COLOR);
                }
                else
                {
                    for(int i = 0; i < equippableItemEquipCompartments.Length; i++)
                    {
                        if (equippableItemEquipCompartments[i].CompareItem(item))
                        {
                            (equippableItemEquipCompartments[equippableItemIndex].GetItem() as EquippableItem)?.DisarmItem();
                            equippableItemEquipCompartments[i].Clear();
                        }
                    }
                }
                break;
            case EItemType.EXPENDABLE:
                ExpendableItem expendItem = item as ExpendableItem;
                if (expendItem.isDiscovered)
                {
                    expendableItemEquipCompartment.Clear();

                    if (equip)
                    {
                        expendItem.isEquipped = true;
                        expendableItemEquipCompartment.SetItem(expendItem, ACTIVE_COLOR);
                    }
                }
                break;
        }
        explanation.SetExplanation(item);
    }

    public void UseItem()
    {
        if (expendableItemEquipCompartment.HasItem())
        {
            ExpendableItem expendItem = expendableItemEquipCompartment.GetItem() as ExpendableItem;
            expendItem.UseItem();
        }
    }

    public void SwipeEquippableItemCompartments(int index)
    {
        if(swipeCoroutine is not null)
        {
            StopCoroutine(swipeCoroutine);
        }
        swipeCoroutine = StartCoroutine(SwipeCompartmentsRoutine(index));
        equippableItemRectIndex = index;
    }

    private IEnumerator SwipeCompartmentsRoutine(int index)
    {
        Vector3 startVector = equippableItemParentRect.position;
        Vector3 endVector = equippableItemParentOriPosition;
        endVector.x += xSwipeValue * (index * -1);
        float elapsedTime = 0;
        while (true)
        {
            elapsedTime += Time.unscaledDeltaTime / swipeDurationTime;

            equippableItemParentRect.position = Vector3.Lerp(startVector, endVector, elapsedTime);

            if (elapsedTime > 1)
            {
                break;
            }
            yield return null;
        }
        equippableItemParentRect.position = endVector;
        swipeCoroutine = null;
    }

    public void ChangeScaleExpendableItemCompartments(int index)
    {
        for (int i = 0; i < sizeChangeCoroutines.Length; i++)
        {
            if (sizeChangeCoroutines[i] is not null)
            {
                StopCoroutine(sizeChangeCoroutines[i]);
                sizeChangeCoroutines[i] = null;
            }

            sizeChangeCoroutines[i] = StartCoroutine(ChangeScaleCompartmentsRoutine(i, index == i));
        }
    }

    private IEnumerator ChangeScaleCompartmentsRoutine(int index, bool sizeUp)
    {
        Vector3 startSize = expendableItemRects[index].localScale;
        Vector3 endSize = sizeUp ? new Vector3(sizeUpScaleValue, sizeUpScaleValue, 1) : new Vector3(1, 1, 1);
        float elapsedTime = 0;
        while (true)
        {
            elapsedTime += Time.unscaledDeltaTime / sizeChangeDurationTime;

            expendableItemRects[index].localScale = Vector3.Lerp(startSize, endSize, elapsedTime);

            if (elapsedTime > 1)
            {
                break;
            }
            yield return null;
        }
        expendableItemRects[index].localScale = endSize;
        sizeChangeCoroutines[index] = null;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Explanation.Clear();
    }
}
