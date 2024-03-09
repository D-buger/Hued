using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public static readonly string COLOR_OBJECT_PARENT_TAG = "ColorObjects";

    public CameraManager cameraManager;

    private ColorObjectManager colorObjectManager;
    private List<eActivableColor> activationColors = new List<eActivableColor>();
    public bool ContainsActivationColors(eActivableColor color) => activationColors.Contains(color);
    public eActivableColor ActivationColors
    {
        set
        {
            if (!activationColors.Contains(value))
            {
                activationColors.Add(value);
                colorObjectManager.EnableColors(value);
                cameraManager.SetColor(value);
            }
        }
    }


    protected override void OnAwake()
    {
        cameraManager = Camera.main.GetComponent<CameraManager>();

        colorObjectManager = GameObject.FindGameObjectWithTag(COLOR_OBJECT_PARENT_TAG).GetComponent<ColorObjectManager>();

    }
}
