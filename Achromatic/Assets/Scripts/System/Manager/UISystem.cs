using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UISystem : SingletonBehavior<UISystem>
{
    [SerializeField]
    private Slider hpSlider;
    [SerializeField]
    private Image filterGauge;
    [SerializeField]
    private Image dashCooldown;

    public UnityAction<int> hpSliderEvent;
    public UnityAction<float> filterSliderEvent;
    public UnityAction<float> dashCooldownEvent;

    private bool isSetHP = false;

    protected override void OnAwake()
    {
        hpSliderEvent += hpUI;
        filterSliderEvent += filterUI;
        dashCooldownEvent += dashCooldownUI;

        dashCooldown.gameObject.SetActive(false);
    }

    private void hpUI(int hp)
    {
        if (!isSetHP)
        {
            isSetHP = true;
            hpSlider.maxValue = hp;
        }

        hpSlider.value = hp;
    }

    private void filterUI(float filter)
    {
        if(filter >= 1 || filter < 0)
        {
            filterGauge.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            filterGauge.transform.parent.gameObject.SetActive(true);
        }

        filterGauge.fillAmount = filter;
    }

    private void dashCooldownUI(float cooldown)
    {
        if (cooldown >= 1 || cooldown <= 0)
        {
            dashCooldown.gameObject.SetActive(false);
        }
        else
        {
            dashCooldown.gameObject.SetActive(true);
            dashCooldown.fillAmount = cooldown;
        }
    }
}
