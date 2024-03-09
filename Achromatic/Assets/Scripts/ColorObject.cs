using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorObject : MonoBehaviour
{
    private Collider2D col;

    [SerializeField]
    private eActivableColor objectColor;

    public eActivableColor ObjectColor 
    { 
        get { return objectColor; } 
    }
    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {

    }

    public void DisableObject()
    {
        col.excludeLayers |= (1 << LayerMask.NameToLayer(PlayManager.PLAYER_TAG));
    }

    public void EnableObject(eActivableColor color)
    {
        if(color != objectColor)
        {
            return;
        }
        col.excludeLayers &= ~(1 << LayerMask.NameToLayer(PlayManager.PLAYER_TAG));
    }
}
