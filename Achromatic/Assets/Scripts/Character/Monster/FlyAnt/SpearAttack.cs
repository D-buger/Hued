using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpearAttack : Projectile
{
    [SerializeField]
    private FlyAntMonsterStat stat;

    [SerializeField]
    private string targetObjectName = "FlyAntMonster 1";
    private Transform targetPos;

    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;

    private void OnEnable()
    {
        isReturn = false;
    }

    private void Start()
    {
        GameObject foundObject = GameObject.Find(targetObjectName);
        if (foundObject is not null)
        {
            targetPos = foundObject.transform;
        }
        else
        {
            Debug.LogWarning("오류 발생");
        }
    }
    private void Update()
    {
        if (isReturn)
        {
            ReturnObject(targetPos);
        }
    }

    public void ReturnObject(Transform obj)
    {
        float shotDir = (Mathf.Atan2(obj.position.y - transform.position.y, obj.position.x - transform.position.x) * Mathf.Rad2Deg) - 180;
        Vector2 originalPos = (Vector2)obj.position - (Vector2)transform.position;

        rigid.Sleep();
        rigid.AddForce(originalPos * stat.spearThrowReturnSpeed);

        transform.rotation = Quaternion.Euler(1, 1, shotDir);
    }
}
