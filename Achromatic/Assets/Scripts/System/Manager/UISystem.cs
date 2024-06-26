using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UISystem : SingletonBehavior<UISystem>
{
    [SerializeField]
    private Image filterGauge;
    [SerializeField]
    private Image dashCooldown;
    [SerializeField]
    private Texture2D cursorTexture;

    public UnityAction<int> hpSliderEvent;
    public UnityAction<float> filterSliderEvent;
    public UnityAction<float> dashCooldownEvent;

    private Vector2 cursorHotspot;

    protected override void OnAwake()
    {
        filterSliderEvent += filterUI;
        dashCooldownEvent += dashCooldownUI;

        dashCooldown?.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (cursorTexture != null)
        {
            cursorHotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
            Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
        }
    }

    private void filterUI(float filter)
    {
        if (ReferenceEquals(filterGauge, null))
        {
            return;
        }

        if (filter >= 1 || filter < 0)
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
        if (ReferenceEquals(dashCooldown, null))
        {
            return;
        }

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
