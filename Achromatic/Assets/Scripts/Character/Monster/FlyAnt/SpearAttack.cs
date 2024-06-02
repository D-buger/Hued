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
    private GameObject flyAntMonster;
    private Rigidbody2D rigid2D;

    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;

    private void OnEnable()
    {
        isReturn = false;
    }

    private void Start()
    {
        rigid2D = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        if (isReturn)
        {
            ReturnObject(flyAntMonster);
        }
    }

    public void ReturnObject(GameObject obj)
    {
        float shotDir = (Mathf.Atan2(flyAntMonster.transform.position.y - transform.position.y, flyAntMonster.transform.position.x - transform.position.x) * Mathf.Rad2Deg) - 180;
        Vector2 originalPos = (Vector2)obj.transform.position - (Vector2)transform.position;

        rigid2D.Sleep();
        rigid2D.AddForce(originalPos * stat.spearThrowReturnSpeed);

        transform.rotation = Quaternion.Euler(1, 1, shotDir);
    }
}
