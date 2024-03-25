using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Backgrounds : MonoBehaviour
{
    [SerializeField]
    GameObject[] backgrounds;

    [SerializeField]
    private float offset;

    [SerializeField]
    private float eachMoveAmount = 0.5f;

    private Vector3[] origin;

    private Vector3 targetOriginPos;
    private Transform targetPos;

    private void Start()
    {
        origin = new Vector3[backgrounds.Length];
        for (int i = 0; i < backgrounds.Length; i++)
        {
            origin[i] = backgrounds[i].transform.position;
        }
        targetPos = GameObject.FindGameObjectWithTag(PlayManager.PLAYER_TAG).transform;
        targetOriginPos = targetPos.position;
    }
    private void Update()
    {
        float newOffset = offset;
        Vector3 target = targetOriginPos - targetPos.position;

        for (int i = 0; i < backgrounds.Length; i++)
        {
            backgrounds[i].transform.position
                = new Vector3(origin[i].x + (newOffset * target.x), origin[i].y + (newOffset * target.y / 2), origin[i].z);
            newOffset *= eachMoveAmount;
        }
    }
}
