using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public enum eActivableColor
{
    NONE,
    RED,
    GREEN,
    BLUE,
    MAX_COLOR
}

public class PlayManager : SingletonBehavior<PlayManager>
{
    public static readonly string PLAYER_TAG = "Player";
    public static readonly string ATTACK_TAG = "Attack";
    public static readonly string ENEMY_TAG = "Enemy";
    public static readonly string COLOR_OBJECT_PARENT_TAG = "ColorObjects";

    private const int FILTER_MAX_GAUGE = 100;

    public LayerMask EnemyMask => (1 << LayerMask.NameToLayer(ENEMY_TAG)) | (1 << LayerMask.NameToLayer("ColorEnemy"));

    [HideInInspector]
    public CameraManager cameraManager;

    [SerializeField]
    private float filterInputCooldown = 1f;
    [SerializeField]
    private float filterPercentPerSec = 10.0f;
    [SerializeField]
    private float filterRecoveryPersec = 15.0f;
    [SerializeField]
    private float filterCoolRecoveryPerSec = 10.0f;

    private Grayscale volumeProfile;
    private Color activateColor = Color.black;
    private eActivableColor haveColor = eActivableColor.NONE;
    private Vector4 playerFilterPosition = Vector4.zero;

    private ColorObjectManager colorObjectManager;
    private List<eActivableColor> activationColors = new List<eActivableColor>();

    public UnityEvent<eActivableColor> FilterColorAttackEvent;
    public UnityEvent<eActivableColor> ActivationColorEvent;

    public bool ContainsActivationColors(eActivableColor color) => activationColors.Contains(color) || (haveColor == color && isFilterOn);

    private Player player;
    public Player GetPlayer => player;

    private float filterGauge = 100;
    private float filterCooldown = 0;

    private bool canFilterOn = false;
    private bool isFilterOn = false;
    private bool isFilterInputCooldown = false;
    private bool activeOnce = false;
    
    public eActivableColor ActivationColors
    {
        set
        {
            if (!activationColors.Contains(value) && haveColor != value)
            {
                haveColor = value;
                volumeProfile.FilterColor.Override(ActivableColor2Color(haveColor));
            }
            else if (!activationColors.Contains(value) && haveColor == value)
            {
                haveColor = eActivableColor.NONE;
                volumeProfile.FilterColor.Override(ActivableColor2Color(haveColor));
                activationColors.Add(value);
                SetActivateColor(value);
                ActivationColorEvent?.Invoke(value);
            }
        }
    }

    protected override void OnAwake()
    {
        cameraManager = Camera.main.GetComponentInChildren<CameraManager>();
        Camera.main.GetComponentInChildren<Volume>().profile.TryGet(out volumeProfile);

        colorObjectManager = GameObject.FindGameObjectWithTag(COLOR_OBJECT_PARENT_TAG).GetComponent<ColorObjectManager>();
        
        player = GameObject.FindGameObjectWithTag(PLAYER_TAG).GetComponent<Player>();

        volumeProfile.activationColor.Override(activateColor);
        volumeProfile.FilterColor.Override(activateColor);
        volumeProfile.playerPosition.Override(playerFilterPosition);
    }

    public void UpdateColorthing()
    {
        FilterColorAttackEvent?.Invoke(isFilterOn ? haveColor : eActivableColor.NONE);
    }
    private void Start()
    {
        InputManager.Instance.FilterEvent.AddListener(ActiveFilter);

        filterGauge = FILTER_MAX_GAUGE;
    }
    private void Update()
    {
        if (!activeOnce)
        {
            activeOnce = true;

            FilterColorAttackEvent?.Invoke(haveColor);
        }


        if (isFilterInputCooldown)
        {
            filterCooldown += Time.deltaTime;

            if (filterCooldown > filterInputCooldown)
            {
                isFilterInputCooldown = false;
                filterCooldown = 0;
            }
        }

        if (haveColor != eActivableColor.NONE)
        {
            if (isFilterOn){
            Vector2 playerPositionInClipSpace = Camera.main.WorldToScreenPoint(player.transform.position);
            playerFilterPosition = new Vector4(playerPositionInClipSpace.x, playerPositionInClipSpace.y, 0, 1);

                colorObjectManager.EnableColors(haveColor);
                FilterColorAttackEvent?.Invoke(haveColor);
                filterGauge -= filterPercentPerSec * Time.deltaTime;
            }
            else
            {
                colorObjectManager.DisableColors(haveColor);
                FilterColorAttackEvent?.Invoke(eActivableColor.NONE);
            playerFilterPosition = Vector4.zero;

            colorObjectManager.DisableColors(haveColor);
                if (canFilterOn)
                {
                    filterGauge += filterRecoveryPersec * Time.deltaTime;
                }
                else
                {
                    filterGauge += filterCoolRecoveryPerSec * Time.deltaTime;
                }
            }
            UISystem.Instance?.filterSliderEvent.Invoke(filterGauge / FILTER_MAX_GAUGE);
        }
        else
        {
            UISystem.Instance?.filterSliderEvent.Invoke(-1);
        }
        volumeProfile.playerPosition.Override(playerFilterPosition);
        FilterCheck();
    }
    private void FilterCheck()
    {
        if(filterGauge < 0)
        {
            isFilterOn = false;
            canFilterOn = false;
            filterGauge = 0;
        }

        if(filterGauge > FILTER_MAX_GAUGE)
        {
            canFilterOn = true;
            filterGauge = FILTER_MAX_GAUGE;
        }
    }

    private void ActiveFilter()
    {
        if (!isFilterInputCooldown)
        {
            if (!isFilterOn && canFilterOn)
            {
                isFilterOn = true;
                isFilterInputCooldown = true;
            }
            else if (isFilterOn)
            {
                isFilterOn = false;
                isFilterInputCooldown = true;
            }
        }
    }
    Color ActivableColor2Color(eActivableColor color)
    {
        Color result = new Color(0, 0, 0, 0);
        switch (color)
        {
            case eActivableColor.RED:
                result.r = 1;
                break;
            case eActivableColor.BLUE:
                result.b = 1;
                break;
            case eActivableColor.GREEN:
                result.g = 1;
                break;
            default:
                return result;
        }
        return result;
    }

    private void SetActivateColor(eActivableColor color)
    {
        activateColor += ActivableColor2Color(color);
        volumeProfile.activationColor.Override(activateColor);
    }
}
