using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Explanation : MonoBehaviour
{
    private const string ITEM_EQUIP_BUTTON_TEXT = "¿Â¬¯";
    private const string ITEM_DISARM_BUTTON_TEXT = "«ÿ¡¶";

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
        itemImage.sprite = item.itemSprite;
        itemNameText.text = item.name;
        itemExplanationText.text = item.itemExplanation;
        if (item.isEquipped)
        {
            itemEquipButtonText.text = ITEM_DISARM_BUTTON_TEXT;
            itemEquipButton.onClick.AddListener(DisarmItem);
        }
        else
        {
            itemEquipButtonText.text = ITEM_EQUIP_BUTTON_TEXT;
            itemEquipButton.onClick.AddListener(EquipItem);
        }
    }

    private void EquipItem()
    {

    }

    private void DisarmItem()
    {

    }


    public void Clear()
    {
        itemEquipButton.onClick.RemoveAllListeners();
        gameObject.SetActive(false);
    }
}
