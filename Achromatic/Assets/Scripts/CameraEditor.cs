using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

[CustomEditor(typeof(CameraController))]
public class CameraEditor : Editor
{

    private CameraController handleComponent;
    private void OnEnable()
    {
        Tools.current = Tool.None;
        handleComponent = target as CameraController;
    }

    private void OnSceneGUI()
    {
        if (null != handleComponent)
        {
            Vector3[] vertex = handleComponent.GetVertex;

            for (int i = 0; i < vertex.Length; i++)
            {
                vertex[i] = Handles.PositionHandle(vertex[i], Quaternion.identity);
            }

            Vector3[] drawHandle = vertex;
            Array.Resize(ref drawHandle, drawHandle.Length + 1);
            drawHandle[drawHandle.Length - 1] = drawHandle[0];

            Handles.DrawAAPolyLine(drawHandle);
        }
    }
}
