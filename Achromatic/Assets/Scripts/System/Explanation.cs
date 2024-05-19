using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Explanation : MonoBehaviour
{
    private Image itemImage;
    private TextMeshPro itemNameText;
    private TextMeshPro itemExplanationText;
    private Button itemEquipButton;
    private TextMeshPro itemEquipButtonText;

    private void Awake()
    {
        itemImage = transform.GetComponentInChildren<Image>();
        itemNameText = transform.GetChild(1).GetComponent<TextMeshPro>();
        itemNameText = transform.GetChild(2).GetComponent<TextMeshPro>();
        itemEquipButton = transform.GetChild(3).GetComponent<Button>();
        itemEquipButtonText = transform.GetChild(3).GetComponentInChildren<TextMeshPro>();
    }

    public void SetExplanation(EquippableItem equipment)
    {

    }

    public void SetExplanation(ExpendableItem expendablement)
    {

    }

    public void Clear()
    {

    }
}
