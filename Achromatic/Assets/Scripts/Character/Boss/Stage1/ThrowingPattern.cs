using System.Collections;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

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
    private float crystalShakeDuration = 1f;
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
    private float[] crystalAngles;

    private float elapsedTime;
    private int disabledCrystalNum;

    private bool isShotPostBehaviour;
    private bool alreadyShot;
    public override void OnStart()
    {
        originBossPosition = boss.transform.position;
        postCrystalPositions = new Vector2[initialCrystalNum];
        crystalDirections = new Vector2[initialCrystalNum];
        crystalAngles = new float[initialCrystalNum];
        crystalObjects = new PatternCrystal[initialCrystalNum];

        if (boss.transform.childCount > 0)
        {
            for (int i = 0; i < initialCrystalNum; i++)
            {
                crystalObjects[i] = boss.transform.GetChild(i).GetComponent<PatternCrystal>();
                crystalObjects[i].transform.position = originBossPosition;
                crystalObjects[i].transform.rotation = Quaternion.Euler(0, 0, 0);
                crystalObjects[i].gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < initialCrystalNum; i++)
            {
                crystalObjects[i] = Instantiate(crystalPrefab, boss.transform).GetComponent<PatternCrystal>().SettingFirst(
                    boss.GetBossStatus.bossColor, crystalDamage, explosionDamage, crystalSpeed, explosionRadius, postExplosionDelay, explosionTime, crystalShakeDuration);
                crystalObjects[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < initialCrystalNum; i++)
        {
            postCrystalPositions[i] = originBossPosition + new Vector2(
                Random.Range(initializeCrystalArea.xMin, initializeCrystalArea.xMax),
                Random.Range(initializeCrystalArea.yMin, initializeCrystalArea.yMax));

            crystalAngles[i] = Random.Range(directionMinAngle, directionMaxAngle);
            float directionX = Mathf.Sin(crystalAngles[i] * Mathf.Deg2Rad);
            float directionY = Mathf.Cos(crystalAngles[i] * Mathf.Deg2Rad);
            crystalDirections[i] = new Vector2(directionX, directionY).normalized;

            crystalObjects[i].transform.position = postCrystalPositions[i];
            crystalObjects[i].afterExplosionEvent.AddListener(() => disabledCrystalNum++);
        }

        elapsedTime = 0;
        disabledCrystalNum = 0;
        isShotPostBehaviour = true;
        alreadyShot = false;
    }

    public override void OnUpdate()
    {
        elapsedTime += Time.deltaTime;

        ShotBehaviour(boss.transform.position);
    } 

    protected void ShotBehaviour(Vector3 initialPos)
    {
        if (isShotPostBehaviour)
        {
            PostShotBehaviour(initialPos);

            if (elapsedTime > 1)
            {
                isShotPostBehaviour = false;
                elapsedTime = 0;
            }
        }
        else if (!alreadyShot && elapsedTime <= shotPostDelay)
        {
            for (int i = 0; i < crystalObjects.Length; i++)
            {
                crystalObjects[i].transform.rotation = Quaternion.Lerp(crystalObjects[i].transform.rotation, Quaternion.Euler(0, 0, -crystalAngles[i] - 90), elapsedTime / shotPostDelay);
            }
        }
        else if (!alreadyShot && elapsedTime > shotPostDelay)
        {
            alreadyShot = true;
            for (int i = 0; i < crystalObjects.Length; i++)
            {
                crystalObjects[i].Shot(crystalDirections[i]);
            }
            CoroutineHandler.StartCoroutine(CrystalAfterShotRoutine());
        }
    }

    protected virtual void PostShotBehaviour(Vector3 initialPos)
    {
        for (int i = 0; i < crystalObjects.Length; i++)
        {
            crystalObjects[i].gameObject.SetActive(true);
            crystalObjects[i].transform.position = Vector2.Lerp(initialPos, postCrystalPositions[i], elapsedTime);
        }
    }

    private IEnumerator CrystalAfterShotRoutine()
    {
        while (true)
        {
            if(disabledCrystalNum >= initialCrystalNum)
            {
                break;
            }
            yield return null;
        }
        AfterShotBehaviour();
    } 
    protected virtual void AfterShotBehaviour()
    {
        PatternEnd();
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
