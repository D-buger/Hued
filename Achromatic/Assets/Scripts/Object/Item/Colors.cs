using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Colors : MonoBehaviour
{
    [SerializeField]
    eActivableColor color;

    protected void OnTriggerEnter2D()
    {
        PlayManager.Instance.ActivationColors = color;
    }
}
