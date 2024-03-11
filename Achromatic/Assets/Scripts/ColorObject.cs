using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorObject : MonoBehaviour
{
    private Effector2D effector;

    [SerializeField]
    private eActivableColor objectColor;

    public eActivableColor ObjectColor 
    { 
        get { return objectColor; } 
    }
    private void Awake()
    {
        effector = GetComponent<PlatformEffector2D>();
    }

    private void Start()
    {

    }

    public void DisableObject()
    {
        effector.colliderMask &= ~(1 << LayerMask.NameToLayer(PlayManager.PLAYER_TAG));
    }

    public void EnableObject(eActivableColor color)
    {
        if(color != objectColor)
        {
            return;
        }
        effector.colliderMask |= (1 << LayerMask.NameToLayer(PlayManager.PLAYER_TAG));
    }
}
