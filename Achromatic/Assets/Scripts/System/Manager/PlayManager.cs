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

// 게임 플레이 담당 매니저
public class PlayManager : SingletonBehavior<PlayManager>
{
    public static readonly string PLAYER_TAG = "Player";
    public static readonly string ATTACK_TAG = "Attack";
    public static readonly string ENEMY_TAG = "Enemy";
    public static readonly string COLOR_OBJECT_PARENT_TAG = "ColorObjects";

    private const int FILTER_MAX_GAUGE = 100;

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

    private ColorObjectManager colorObjectManager;
    private List<eActivableColor> activationColors = new List<eActivableColor>();

    public UnityEvent<eActivableColor> ActivationColorEvent;
    public bool ContainsActivationColors(eActivableColor color) => activationColors.Contains(color) || (haveColor == color && isFilterOn);

    private Player player;
    public Player GetPlayer => player;

    private float filterGauge = 100;
    private float filterCooldown = 0;

    private bool canFilterOn = false;
    private bool isFilterOn = false;
    private bool isFilterInputCooldown = false;
    
    public eActivableColor ActivationColors
    {
        set
        {
            if (!activationColors.Contains(value) && haveColor != value)
            {
                haveColor = value;
            }
            else if (!activationColors.Contains(value) && haveColor == value)
            {
                haveColor = eActivableColor.NONE;
                activationColors.Add(value);
                SetColor(value);
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
    }
    private void Start()
    {
        InputManager.Instance.FilterEvent.AddListener(ActiveFilter);

        filterGauge = FILTER_MAX_GAUGE;
    }
    private void Update()
    {
        if (isFilterInputCooldown)
        {
            filterCooldown += Time.deltaTime;

            if (filterCooldown > filterInputCooldown)
            {
                isFilterInputCooldown = false;
                filterCooldown = 0;
            }
        }

        if (isFilterOn)
        {
            colorObjectManager.EnableColors(haveColor);
            filterGauge -= filterPercentPerSec * Time.deltaTime;
        }
        else
        {
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

    private void SetColor(eActivableColor color)
    {
        switch(color)
        {
            case eActivableColor.RED:
                activateColor.r = 1;
                break;
            case eActivableColor.BLUE:
                activateColor.g = 1;
                break;
            case eActivableColor.GREEN:
                activateColor.b = 1;
                break;
            default:
                return;
        }
        volumeProfile.activationColor.Override(activateColor);

    }
}
