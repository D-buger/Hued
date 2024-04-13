using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Cinemachine;

public enum ePanDirection
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public enum eTwoDirection
{
    HORIZONTAL,
    VERTICAL
}
[RequireComponent(typeof(Collider2D))]
public class CameraControlTrigger : MonoBehaviour
{
    public CustomInspectorCameraObjects customInspectorObjects;

    private Collider2D coll;

    private void Awake()
    {
        coll = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayManager.PLAYER_TAG))
        {
            if (customInspectorObjects.panCameraOnContact)
            {
                CameraManager.Instance.PanCameraOnContact(customInspectorObjects.panDistance, customInspectorObjects.panTime, customInspectorObjects.panDirection, false);
            }

            if (customInspectorObjects.lockXorY)
            {
                CameraManager.Instance.LockPosition(customInspectorObjects.lockPosition, true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayManager.PLAYER_TAG))
        {
            Vector2 exitDirection = (collision.transform.position - coll.bounds.center).normalized;

            if (customInspectorObjects.swapBounds && customInspectorObjects.BoundLineLD != null && customInspectorObjects.BoundLineRU != null)
            {
                CameraManager.Instance.SwitchBoundLine(customInspectorObjects.BoundLineLD, customInspectorObjects.BoundLineRU, exitDirection, customInspectorObjects.boundDirection);
            }

            if (customInspectorObjects.panCameraOnContact)
            {
                CameraManager.Instance.PanCameraOnContact(customInspectorObjects.panDistance, customInspectorObjects.panTime, customInspectorObjects.panDirection, true);
            }

            if (customInspectorObjects.lockXorY)
            {
                CameraManager.Instance.LockPosition(customInspectorObjects.lockPosition, false);
            }
        }
    }
}

[System.Serializable]
public class CustomInspectorCameraObjects
{
    public bool swapBounds = false;
    public bool panCameraOnContact = false;
    public bool lockXorY = false;

    [HideInInspector] public Collider2D BoundLineLD;
    [HideInInspector] public Collider2D BoundLineRU;
    [HideInInspector] public eTwoDirection boundDirection;

    [HideInInspector] public ePanDirection panDirection;
    [HideInInspector] public float panDistance = 3f;
    [HideInInspector] public float panTime = 0.35f;

    [HideInInspector] public eTwoDirection lockPosition;
}

#if UNITY_EDITOR
[CustomEditor(typeof(CameraControlTrigger))]
public class MyScriptEditor : Editor
{
    CameraControlTrigger cameraControlTrigger;
    private void OnEnable()
    {
        cameraControlTrigger = (CameraControlTrigger)target;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (cameraControlTrigger.customInspectorObjects.swapBounds)
        {
            cameraControlTrigger.customInspectorObjects.BoundLineLD = EditorGUILayout.ObjectField("Old BoundLine(Trigger)", cameraControlTrigger.customInspectorObjects.BoundLineLD,
                typeof(Collider2D), true) as Collider2D;

            cameraControlTrigger.customInspectorObjects.BoundLineRU = EditorGUILayout.ObjectField("New BoundLine(Trigger)", cameraControlTrigger.customInspectorObjects.BoundLineRU,
                typeof(Collider2D), true) as Collider2D;

            cameraControlTrigger.customInspectorObjects.boundDirection = (eTwoDirection)EditorGUILayout.EnumPopup("Move Direction",
                cameraControlTrigger.customInspectorObjects.boundDirection);
        }

        if (cameraControlTrigger.customInspectorObjects.panCameraOnContact)
        {
            cameraControlTrigger.customInspectorObjects.panDirection = (ePanDirection)EditorGUILayout.EnumPopup("Camera Pan Direction",
                cameraControlTrigger.customInspectorObjects.panDirection);

            cameraControlTrigger.customInspectorObjects.panDistance = EditorGUILayout.FloatField("Pan Distance", cameraControlTrigger.customInspectorObjects.panDistance);
            cameraControlTrigger.customInspectorObjects.panTime = EditorGUILayout.FloatField("Pan Time", cameraControlTrigger.customInspectorObjects.panTime);
        }

        if (cameraControlTrigger.customInspectorObjects.lockXorY)
        {
            cameraControlTrigger.customInspectorObjects.lockPosition = (eTwoDirection)EditorGUILayout.EnumPopup("Lock Position",
                cameraControlTrigger.customInspectorObjects.lockPosition);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(cameraControlTrigger);
        }
    }
}
#endif