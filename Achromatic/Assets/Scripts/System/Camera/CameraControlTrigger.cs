using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Cinemachine;

public enum EPanDirection
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public enum ETwoDirection
{
    HORIZONTAL,
    VERTICAL
}
[RequireComponent(typeof(Collider2D))]
public class CameraControlTrigger : MonoBehaviour
{
    public CustomInspectorCameraObjects customInspectorObjects;

    private Collider2D coll;
    private void OnValidate()
    {
        if (customInspectorObjects.playerMoveEndPos == null)
        {
            customInspectorObjects.playerMoveEndPos = new Vector2[2] {transform.position, transform.position};
        }
    }
    private void Awake()
    {
        coll = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayManager.PLAYER_TAG))
        {
            Vector2 exitDirection = (coll.bounds.center - collision.transform.position).normalized;

            if (customInspectorObjects.swapBounds && customInspectorObjects.boundLineLD != null && customInspectorObjects.boundLineRU != null)
            {
                CameraManager.Instance.SwitchBoundLine(customInspectorObjects.boundLineLD, customInspectorObjects.boundLineRU, 
                    customInspectorObjects.playerMoveEndPos, customInspectorObjects.boundMoveCurve, exitDirection, customInspectorObjects.boundDirection);
            }

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

            if (customInspectorObjects.swapBounds && customInspectorObjects.boundLineLD != null && customInspectorObjects.boundLineRU != null)
            {
                CameraManager.Instance.SwitchBoundLine(customInspectorObjects.boundLineLD, customInspectorObjects.boundLineRU, 
                    customInspectorObjects.playerMoveEndPos, customInspectorObjects.boundMoveCurve, exitDirection, customInspectorObjects.boundDirection);
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

    [HideInInspector] public Collider2D boundLineLD;
    [HideInInspector] public Collider2D boundLineRU;
    [HideInInspector] public ETwoDirection boundDirection;
    [HideInInspector] public AnimationCurve boundMoveCurve = new AnimationCurve();
    [HideInInspector] public Vector2[] playerMoveEndPos;

    [HideInInspector] public EPanDirection panDirection;
    [HideInInspector] public float panDistance = 3f;
    [HideInInspector] public float panTime = 0.35f;

    [HideInInspector] public ETwoDirection lockPosition;
}

#if UNITY_EDITOR
[CustomEditor(typeof(CameraControlTrigger))]
public class MyScriptEditor : Editor
{
    CameraControlTrigger cameraControlTrigger;
    private bool isChangeSelf = true;
    private void OnEnable()
    {
        cameraControlTrigger = (CameraControlTrigger)target;
    }
    private void OnSceneGUI()
    {
        //Tools.current = Tool.None;
        var component = target as CameraControlTrigger;

        if (component.customInspectorObjects.swapBounds)
        {
            if (!isChangeSelf)
            {
                component.customInspectorObjects.playerMoveEndPos[0] = Handles.PositionHandle(component.customInspectorObjects.playerMoveEndPos[0], Quaternion.identity);
                component.customInspectorObjects.playerMoveEndPos[1] = Handles.PositionHandle(component.customInspectorObjects.playerMoveEndPos[1], Quaternion.identity);
            }
            else
            {
                component.transform.position = Handles.PositionHandle(component.transform.position, component.transform.rotation);
            }

            Handles.BeginGUI();

            if (isChangeSelf)
            {
                if (GUILayout.Button("옵션 Transform 변경", GUILayout.Width(200)))
                {
                    isChangeSelf = !isChangeSelf;
                }
            }
            else
            {
                isChangeSelf = GUILayout.Button("Transform 변경", GUILayout.Width(200));
            }

            Handles.EndGUI();
        }
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (cameraControlTrigger.customInspectorObjects.swapBounds)
        {
            cameraControlTrigger.customInspectorObjects.boundLineLD = EditorGUILayout.ObjectField("Left/Down BoundLine(Trigger)", cameraControlTrigger.customInspectorObjects.boundLineLD,
                typeof(Collider2D), true) as Collider2D;

            cameraControlTrigger.customInspectorObjects.boundLineRU = EditorGUILayout.ObjectField("Right/Up BoundLine(Trigger)", cameraControlTrigger.customInspectorObjects.boundLineRU,
                typeof(Collider2D), true) as Collider2D;

            cameraControlTrigger.customInspectorObjects.boundDirection = (ETwoDirection)EditorGUILayout.EnumPopup("Move Direction",
                cameraControlTrigger.customInspectorObjects.boundDirection);

            cameraControlTrigger.customInspectorObjects.boundMoveCurve = EditorGUILayout.CurveField("Move Style", cameraControlTrigger.customInspectorObjects.boundMoveCurve);

        }

        if (cameraControlTrigger.customInspectorObjects.panCameraOnContact)
        {
            cameraControlTrigger.customInspectorObjects.panDirection = (EPanDirection)EditorGUILayout.EnumPopup("Camera Pan Direction",
                cameraControlTrigger.customInspectorObjects.panDirection);

            cameraControlTrigger.customInspectorObjects.panDistance = EditorGUILayout.FloatField("Pan Distance", cameraControlTrigger.customInspectorObjects.panDistance);
            cameraControlTrigger.customInspectorObjects.panTime = EditorGUILayout.FloatField("Pan Time", cameraControlTrigger.customInspectorObjects.panTime);
        }

        if (cameraControlTrigger.customInspectorObjects.lockXorY)
        {
            cameraControlTrigger.customInspectorObjects.lockPosition = (ETwoDirection)EditorGUILayout.EnumPopup("Lock Position",
                cameraControlTrigger.customInspectorObjects.lockPosition);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(cameraControlTrigger);
        }
    }
}

#endif