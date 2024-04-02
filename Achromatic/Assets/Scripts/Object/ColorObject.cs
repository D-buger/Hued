using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorObject : MonoBehaviour
{
    private Collider2D coll;
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
        coll = GetComponent<Collider2D>();
    }

    private void Start()
    {
        DisableObject();
    }

    public void DisableObject()
    {
        effector.colliderMask &= ~(1 << LayerMask.NameToLayer(PlayManager.PLAYER_TAG));

        coll.forceReceiveLayers &= ~(1 << LayerMask.NameToLayer(PlayManager.PLAYER_TAG));
        coll.forceSendLayers &= ~(1 << LayerMask.NameToLayer(PlayManager.PLAYER_TAG));
    }

    public void EnableObject(eActivableColor color)
    {
        if(color != objectColor)
        {
            return;
        }
        effector.colliderMask |= (1 << LayerMask.NameToLayer(PlayManager.PLAYER_TAG));

        coll.forceReceiveLayers |= (1 << LayerMask.NameToLayer(PlayManager.PLAYER_TAG));
        coll.forceSendLayers |= (1 << LayerMask.NameToLayer(PlayManager.PLAYER_TAG));
    }
}
