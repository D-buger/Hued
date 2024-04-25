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

        Debug.Log("���̺� �Ϸ�");
        PlayManager.Instance.GetPlayer.FillPlayerHPMax();
        PlayManager.Instance.FillFillterGaugeFull();
        //TODO : ���̺� �߰�
        //TODO : ������ ��Ÿ�� �ʱ�ȭ
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isNearPlayer ? Color.blue : Color.white;
        Gizmos.DrawWireSphere(transform.position, detectPlayerDistance);
    }
}
