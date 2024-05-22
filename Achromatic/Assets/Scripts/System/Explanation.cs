using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEditor.Progress;

public class Explanation : MonoBehaviour
{
    private const string ITEM_EQUIP_BUTTON_TEXT = "장착";
    private const string ITEM_DISARM_BUTTON_TEXT = "해제";

    private const string ITEM_CONCEALED_NAME_TEXT = "???";
    private const string ITEM_CONCEALED_EXPLANATION_TEXT = "아직 발견되지 않은 듯 하다";
    private Inventory Inventory => PlayManager.Instance.GetInventory;

    private Image itemImage;
    private TMP_Text itemNameText;
    private TMP_Text itemExplanationText;
    private Button itemEquipButton;
    private TMP_Text itemEquipButtonText;

    private void Awake()
    {
        itemImage = transform.GetChild(0).GetComponent<Image>();
        itemNameText = transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>();
        itemExplanationText = transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>();
        itemEquipButton = transform.GetChild(2).GetComponent<Button>();
        itemEquipButtonText = transform.GetChild(2).GetComponentInChildren<TMP_Text>();
    }

    public void SetExplanation(Item item)
    {
        gameObject.SetActive(true);
        if (item.ItemType() == EItemType.EXPENDABLE 
            && !(item as ExpendableItem).isDiscovered)
        {
            itemImage.sprite = default;
            itemNameText.text = ITEM_CONCEALED_NAME_TEXT;
            itemExplanationText.text = ITEM_CONCEALED_EXPLANATION_TEXT;
        }
        else
        {
            itemImage.sprite = item.itemSprite;
            itemNameText.text = item.name;
            itemExplanationText.text = item.itemExplanation;
            itemEquipButton?.onClick.RemoveAllListeners();
            if (item.isEquipped)
            {
                itemEquipButtonText.text = ITEM_DISARM_BUTTON_TEXT;
                itemEquipButton.onClick.AddListener(() => Inventory.EquipItem(item, false));
            }
            else
            {
                itemEquipButtonText.text = ITEM_EQUIP_BUTTON_TEXT;
                itemEquipButton.onClick.AddListener(() => Inventory.EquipItem(item, true));
            }
        }
    }

    public void Clear()
    {
        itemEquipButton?.onClick.RemoveAllListeners();
        gameObject.SetActive(false);
    }
}
