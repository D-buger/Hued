using UnityEngine;
using UnityEngine.UI;
public class HUDPresenter : MonoBehaviour
{
    private UIElement<HealthBar> HealthBar;
    private UIElement<Slider> BossHpSlider;

    private void Awake()
    {
        HealthBar = new UIElement<HealthBar>("HealthBar", this.gameObject);
        //BossHpSlider = new UIElement<Slider>("BossHP", this.gameObject);

        PlayManager.Instance.GetPlayer.PlayerCurrentHPEvent += HealthBar.Component.SetInsideGraphic;
        PlayManager.Instance.GetPlayer.PlayerMaxHPEvent += HealthBar.Component.SizeChange;
    }
}