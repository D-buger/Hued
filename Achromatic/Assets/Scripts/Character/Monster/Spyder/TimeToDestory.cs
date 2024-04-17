using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeToDestory : MonoBehaviour
{
    public float timeCheck = 0;
    [SerializeField]
    private SpiderMonsterStats stat;
    void Update()
    {
        timeCheck += Time.deltaTime;
        OBJToDestroy();
    }
    private void OBJToDestroy()
    {
        if (timeCheck > 2.0f)
        {
            Destroy(gameObject);
        }
    }
}
