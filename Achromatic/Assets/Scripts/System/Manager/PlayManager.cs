using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum eActivableColor
{
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

    public CameraManager cameraManager;

    private Grayscale volumeProfile;
    private Color activateColor = Color.black;

    private ColorObjectManager colorObjectManager;
    private List<eActivableColor> activationColors = new List<eActivableColor>();
    public bool ContainsActivationColors(eActivableColor color) => activationColors.Contains(color);

    private Player player;
    public Player GetPlayer => player;

    public eActivableColor ActivationColors
    {
        set
        {
            if (!activationColors.Contains(value))
            {
                activationColors.Add(value);
                colorObjectManager.EnableColors(value);
                SetColor(value);
            }
        }
    }


    protected override void OnAwake()
    {
        cameraManager = Camera.main.GetComponentInChildren<CameraManager>();
        Camera.main.GetComponentInChildren<Volume>().profile.TryGet(out volumeProfile);

        colorObjectManager = GameObject.FindGameObjectWithTag(COLOR_OBJECT_PARENT_TAG).GetComponent<ColorObjectManager>();

    }
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag(PLAYER_TAG).GetComponent<Player>();
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
