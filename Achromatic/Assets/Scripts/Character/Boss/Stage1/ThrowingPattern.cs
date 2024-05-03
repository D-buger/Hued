using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class ThrowingPattern : BossPattern
{
    [SerializeField]
    private GameObject crystalPrefab;
    [SerializeField]
    private Rect initializeCrystalArea;
    [SerializeField]
    private float crystalSpeed = 10f;
    [SerializeField]
    private int initialCrystalNum = 3;
    [SerializeField]
    private float shotPostDelay = 0.5f;
    [SerializeField]
    private float postExplosionDelay = 3;
    [SerializeField]
    private float explosionRadius = 0.5f;
    [SerializeField]
    private float explosionTime = 1f;
    [SerializeField]
    private int crystalDamage = 5;
    [SerializeField]
    private int explosionDamage = 5;
    [SerializeField]
    private float directionMinAngle = 0;
    [SerializeField]
    private float directionMaxAngle = 90;

    private Vector2 originBossPosition;
    private Vector2[] postCrystalPositions;
    private Vector2[] crystalDirections;
    private PatternCrystal[] crystalObjects;

    private float elapsedTime;

    private bool isShotPostBehaviour;
    private bool alreadyShot;
    public override void OnStart()
    {
        if(boss.transform.childCount > 0)
        {
            crystalObjects = boss.transform.GetComponentsInChildren<PatternCrystal>();
        }
        else 
        {
            crystalObjects = new PatternCrystal[initialCrystalNum];
            for(int i = 0; i < postCrystalPositions.Length; i++)
            {
                crystalObjects[i] = Instantiate(crystalPrefab, boss.transform).GetComponent<PatternCrystal>().SettingFirst(
                    crystalDamage, explosionDamage, crystalSpeed, explosionRadius, postExplosionDelay, explosionTime);
                crystalObjects[i].gameObject.SetActive(false);
            }
        }

        originBossPosition = boss.transform.position;
        postCrystalPositions = new Vector2[initialCrystalNum];
        crystalDirections = new Vector2[initialCrystalNum];

        for (int i = 0; i < initialCrystalNum; i++)
        {
            postCrystalPositions[i] = originBossPosition + new Vector2(
                Random.Range(initializeCrystalArea.xMin, initializeCrystalArea.xMax),
                Random.Range(initializeCrystalArea.yMin, initializeCrystalArea.yMax));

            float randomPickedDirection = Random.Range(directionMinAngle, directionMaxAngle);
            float directionX = Mathf.Sin(randomPickedDirection * Mathf.Deg2Rad);
            float directionY = Mathf.Cos(randomPickedDirection * Mathf.Deg2Rad);
            crystalDirections[i] = new Vector2(directionX, directionY).normalized;

            crystalObjects[i].transform.position = postCrystalPositions[i];
        }

        elapsedTime = 0;
        isShotPostBehaviour = true;
        alreadyShot = false;
    }

    public override void OnUpdate()
    {
        elapsedTime += Time.deltaTime;

        if (isShotPostBehaviour)
        {
            for (int i = 0; i < crystalObjects.Length; i++)
            {
                crystalObjects[i].gameObject.SetActive(true);
                crystalObjects[i].transform.position = Vector2.Lerp(boss.transform.position, postCrystalPositions[i], elapsedTime);
            }

            if (elapsedTime > 1)
            {
                isShotPostBehaviour = false;
                elapsedTime = 0;
            }
        }
        else if (!alreadyShot && elapsedTime > shotPostDelay)
        {
            alreadyShot = true;
            for(int i = 0; i < crystalObjects.Length; i++)
            {
                crystalObjects[i].Shot(crystalDirections[i]);
            }
        }
    } 

    private IEnumerator CrystalAfterShotRoutine()
    {

        for(int i = 0; i < crystalObjects.Length; i++)
        {

        }
        yield return null;
    } 

    public override bool CanParryAttack()
    {
        return false;
    }

    public override void DrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(originBossPosition + initializeCrystalArea.center, initializeCrystalArea.size);
    }
}
