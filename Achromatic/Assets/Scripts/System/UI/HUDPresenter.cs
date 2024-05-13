using UnityEngine;
using UnityEngine.UI;
public class HUDPresenter : MonoBehaviour
{
    private UIElement<HealthBar> HealthBar;
    private UIElement<Image> BossHpSlider;

    private void Awake()
    {
        HealthBar = new UIElement<HealthBar>("HealthBar", this.gameObject);
        BossHpSlider = new UIElement<Image>("BossHP", this.gameObject);

        PlayManager.Instance.GetPlayer.PlayerCurrentHPEvent += HealthBar.Component.SetInsideGraphic;
        PlayManager.Instance.GetPlayer.PlayerMaxHPEvent += HealthBar.Component.SizeChange;


    }
    public void BossHpSliderSetValue(int maxValue, int value)
    {
        BossHpSlider.Component.fillAmount = Mathf.Clamp(value, 0, 100);
    }
}