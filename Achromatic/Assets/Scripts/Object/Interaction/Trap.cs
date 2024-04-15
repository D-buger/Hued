using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Trap : MonoBehaviour
{
    [SerializeField]
    private int damage = 1;
    [SerializeField]
    private float cameraFadeTime = 0.5f;

    private int positionArrSize = 0;
    [Space(10)]
    public Vector2[] goBackPosition = new Vector2[0];

    private void OnValidate()
    {
        if(positionArrSize != goBackPosition.Length)
        {
            if(positionArrSize < goBackPosition.Length)
            {
                if (goBackPosition[positionArrSize].x == 0 && goBackPosition[positionArrSize].y == 0)
                    goBackPosition[positionArrSize] = transform.position;
                positionArrSize++;
            }
            else if(positionArrSize > goBackPosition.Length)
            {
                positionArrSize--;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            if (goBackPosition.Length > 0)
            {
                float distance = 0;
                float smallDistance = -1;
                int smallDistanceIndex = -1;
                for (int i = 0; i < goBackPosition.Length; i++)
                {
                    distance = Vector2.Distance(collision.transform.position, goBackPosition[i]);
                    if (smallDistance == -1 || smallDistance > distance)
                    {
                        smallDistance = distance;
                        smallDistanceIndex = i;
                    }
                }
                CameraManager.Instance.CameraFade(cameraFadeTime, 0,
                    () =>
                    {
                        collision.gameObject.transform.position = goBackPosition[smallDistanceIndex];
                    });
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
             Vector2 attackDir = collision.transform.position - collision.transform.position;
                collision.gameObject.GetComponent<IAttack>().Hit(damage, attackDir.normalized, true);
            
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Trap))]
public class TrapInspector : Editor
{
    private bool isChangeSelf = true;
    private void OnSceneGUI()
    {
        //Tools.current = Tool.None;
        var component = target as Trap;

        if (!isChangeSelf)
        {
            for (int i = 0; i < component.goBackPosition.Length; i++)
            {
                component.goBackPosition[i] = Handles.PositionHandle(component.goBackPosition[i], Quaternion.identity);
            }
        }
        else
        {
            component.transform.position = Handles.PositionHandle( component.transform.position, component.transform.rotation);
        }

        Handles.BeginGUI();

        if (isChangeSelf)
        {
            if(GUILayout.Button("옵션 Transform 변경", GUILayout.Width(200)))
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


#endif