using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
public class HUDPresenter : SingletonBehavior<HUDPresenter>
{
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Camera colorCamera;

    private Canvas canvas;

    private UIElement<HealthBar> healthBar;
    private UIElement<Image> bossHpSlider;
    private UIElement<Image> prismRed;
    private UIElement<Image> prismGreen;
    private UIElement<Image> prismBlue;

    public Action<int, int> BossHpAction;

    protected override void OnAwake()
    {
        canvas = GetComponent<Canvas>();
        PlayManager.Instance.FilterColorAttackEvent.AddListener(SetUIColor);

        healthBar = new UIElement<HealthBar>("HealthBar", this.gameObject);
        bossHpSlider = new UIElement<Image>("BossHP", this.gameObject);
        prismRed = new UIElement<Image>("PrismRed", this.gameObject);
        prismGreen = new UIElement<Image>("PrismGreen", this.gameObject);
        prismBlue = new UIElement<Image>("PrismBlue", this.gameObject);

        prismRed.Component.gameObject.SetActive(false);
        prismGreen.Component.gameObject.SetActive(false);
        prismBlue.Component.gameObject.SetActive(false);

        PlayManager.Instance.GetPlayer.PlayerCurrentHPEvent.AddListener(healthBar.Component.SetInsideGraphic);
        PlayManager.Instance.GetPlayer.PlayerMaxHPEvent.AddListener(healthBar.Component.SizeChange);

        BossHpAction +=
            (int maxValue, int currentValue) => bossHpSlider.Component.fillAmount = maxValue / currentValue;

        PlayManager.Instance.ActivationColorEvent.AddListener(MakeVisiblePrism);

    }
    private void SetUIColor(eActivableColor color)
    {
        canvas.Render
        if(color != eActivableColor.NONE || color != eActivableColor.MAX_COLOR)
        {

        }
        else
        {

        }
    }

    private void MakeVisiblePrism(eActivableColor color)
    {
        switch (color)
        {
            case eActivableColor.RED:
                prismRed.Component.gameObject.SetActive(true);
                break;
            case eActivableColor.GREEN:
                prismGreen.Component.gameObject.SetActive(true);
                break;
            case eActivableColor.BLUE:
                prismBlue.Component.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }

}