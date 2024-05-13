using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager instance;

    [SerializeField]
    private GameObject[] projectilePrefabs;

    [SerializeField]
    private int poolSize = 10;

    private Queue<GameObject>[] inactivePools;

    void Awake()
    {
        instance = this;
        InitializePools();
    }

    void InitializePools()
    {
        inactivePools = new Queue<GameObject>[projectilePrefabs.Length];
        for (int i = 0; i < projectilePrefabs.Length; i++)
        {
            inactivePools[i] = new Queue<GameObject>();
            for (int j = 0; j < poolSize; j++)
            {
                GameObject projectile = Instantiate(projectilePrefabs[i]);
                projectile.SetActive(false);
                inactivePools[i].Enqueue(projectile);
            }
        }
    }

    public GameObject GetProjectileFromPool(int index)
    {
        if (index < 0 || index >= projectilePrefabs.Length)
        {
            return null;
        }

        if (inactivePools[index].Count > 0)
        {
            GameObject projectile = inactivePools[index].Dequeue();
            projectile.SetActive(true);
            return projectile;
        }
        else
        {
            GameObject newProjectile = Instantiate(projectilePrefabs[index]);
            return newProjectile;
        }
    }

    public void ReturnProjectileToPool(GameObject projectile, int index)
    {
        if (index < 0 || index >= projectilePrefabs.Length)
        {
            return;
        }

        projectile.SetActive(false);
        inactivePools[index].Enqueue(projectile);
    }
}