using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    [SerializeField]
    private float detectPlayerDistance = 3.0f;

    private bool isNearPlayer = false;

    private void Start()
    {
        InputManager.Instance.InterectionEvent.AddListener(GetInteractionKeyDown);
    }

    private void Update()
    {
        CheckIsNearPlayer();
    }

    private void CheckIsNearPlayer()
    {
        isNearPlayer = Vector2.Distance(transform.position, PlayManager.Instance.GetPlayer.transform.position) <= detectPlayerDistance; 
    }
    private void GetInteractionKeyDown()
    {
        if (!isNearPlayer)
        {
            return;
        }

        Debug.Log("세이브 완료");
        PlayManager.Instance.GetPlayer.FillPlayerHPMax();
        PlayManager.Instance.FillFillterGaugeFull();
        //TODO : 세이브 추가
        //TODO : 아이템 쿨타임 초기화
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isNearPlayer ? Color.blue : Color.white;
        Gizmos.DrawWireSphere(transform.position, detectPlayerDistance);
    }
}
