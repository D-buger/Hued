using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalRainPattern : ThrowingPattern
{
    [Space(10)]
    [SerializeField]
    private Rect secondInitializeCrystalArea;
    [SerializeField]
    private int secondInitialCrystalNum = 6;
    [SerializeField]
    private float secondCrystalSpeed = 0.2f;

    private Vector2[] secondPostCrystalPositions;
    private Vector2[] secondCrystalDirections;
    private Vector2[] crystalInitialPositions;
    private Vector2[] secondCrystalInitialPositions;
    private PatternCrystal[] secondCrystalObjects;
    private float[] secondCrystalAngles;

    private bool shootAlreadyFirstshot;

    public override void OnStart()
    {
        base.OnStart();

        secondPostCrystalPositions = new Vector2[secondInitialCrystalNum];
        secondCrystalDirections = new Vector2[secondInitialCrystalNum];
        crystalInitialPositions = new Vector2[initialCrystalNum];
        secondCrystalInitialPositions = new Vector2[secondInitialCrystalNum];
        secondCrystalAngles = new float[secondInitialCrystalNum];
        secondCrystalObjects = new PatternCrystal[secondInitialCrystalNum];

        shootAlreadyFirstshot = false;

        if (boss.transform.childCount >= initialCrystalNum + secondInitialCrystalNum)
        {
            for (int i = 0; i < secondInitialCrystalNum; i++)
            {
                secondCrystalObjects[i] = boss.transform.GetChild(initialCrystalNum + i).GetComponent<PatternCrystal>();
                secondCrystalObjects[i].transform.position = originBossPosition;
                secondCrystalObjects[i].transform.rotation = Quaternion.Euler(0, 0, 0);
                secondCrystalObjects[i].gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < secondInitialCrystalNum; i++)
            {
                secondCrystalObjects[i] = Instantiate(crystalPrefab, boss.transform).GetComponent<PatternCrystal>().SettingFirst(
                    boss.GetBossStatus.bossColor, crystalDamage, explosionDamage, crystalSpeed, explosionRadius, postExplosionDelay, explosionTime, crystalShakeDuration);
                secondCrystalObjects[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < initialCrystalNum; i++)
        {
            crystalInitialPositions[i] = originBossPosition + new Vector2(
                Random.Range(secondInitializeCrystalArea.xMin, secondInitializeCrystalArea.xMax),
                Random.Range(secondInitializeCrystalArea.yMin, secondInitializeCrystalArea.yMax));
        }

        for (int i = 0; i < secondInitialCrystalNum; i++)
        {
            secondPostCrystalPositions[i] = originBossPosition + new Vector2(
                Random.Range(initializeCrystalArea.xMin, initializeCrystalArea.xMax),
                Random.Range(initializeCrystalArea.yMin, initializeCrystalArea.yMax));

            secondCrystalInitialPositions[i] = originBossPosition + new Vector2(
                Random.Range(secondInitializeCrystalArea.xMin, secondInitializeCrystalArea.xMax),
                Random.Range(secondInitializeCrystalArea.yMin, secondInitializeCrystalArea.yMax));

            secondCrystalAngles[i] = Random.Range(directionMinAngle, directionMaxAngle);
            float directionX = Mathf.Sin(secondCrystalAngles[i] * Mathf.Deg2Rad);
            float directionY = Mathf.Cos(secondCrystalAngles[i] * Mathf.Deg2Rad);
            secondCrystalDirections[i] = new Vector2(directionX, directionY).normalized;
            
            secondCrystalObjects[i].afterExplosionEvent.AddListener(() => disabledCrystalNum++);
        }
    }
    public override void OnUpdate()
    {
        elapsedTime += Time.deltaTime;

        if (!shootAlreadyFirstshot)
        {
            ShotSequence(crystalObjects, initialCrystalNum + secondInitialCrystalNum, crystalInitialPositions);
        }
        else
        {
            ShotSequence(secondCrystalObjects, initialCrystalNum + secondInitialCrystalNum, secondCrystalInitialPositions);
        }
    }

    protected override void ShotBehaviour(PatternCrystal[] crystals)
    {
        base.ShotBehaviour(crystals);
        if (!shootAlreadyFirstshot)
        {
            shootAlreadyFirstshot = true;
            isShotPostBehaviour = true;
            alreadyShot = false;
            elapsedTime = 0;
        }
    }

    protected override void AfterShotBehaviour()
    {
        if (shootAlreadyFirstshot)
        {
            base.AfterShotBehaviour();
        }
    }

    public override bool CanParryAttack()
    {
        return false;
    }
    public override void DrawGizmos()
    {
        base.DrawGizmos();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(originBossPosition + secondInitializeCrystalArea.center, secondInitializeCrystalArea.size);
    }
}
