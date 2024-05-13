using System.Collections;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class ThrowingPattern : BossPattern
{
    [SerializeField]
    protected GameObject crystalPrefab;
    [SerializeField]
    protected Rect initializeCrystalArea;
    [SerializeField]
    protected float crystalSpeed = 10f;
    [SerializeField]
    protected int initialCrystalNum = 3;
    [SerializeField]
    protected float shotPostDelay = 0.5f;
    [SerializeField]
    protected float crystalShakeDuration = 1f;
    [SerializeField]
    protected float postExplosionDelay = 3;
    [SerializeField]
    protected float explosionRadius = 0.5f;
    [SerializeField]
    protected float explosionTime = 1f;
    [SerializeField]
    protected int crystalDamage = 5;
    [SerializeField]
    protected int explosionDamage = 5;
    [SerializeField]
    protected float directionMinAngle = 0;
    [SerializeField]
    protected float directionMaxAngle = 90;

    protected Coroutine patternEndCheckCoroutine;

    protected Vector2 originBossPosition;
    private Vector2[] postCrystalPositions;
    private Vector2[] crystalDirections;
    protected PatternCrystal[] crystalObjects;
    private float[] crystalAngles;

    protected int disabledCrystalNum;
    protected float elapsedTime;

    protected bool isShotPostBehaviour;
    protected bool alreadyShot;
    public override void OnStart()
    {
        originBossPosition = boss.transform.position;
        postCrystalPositions = new Vector2[initialCrystalNum];
        crystalDirections = new Vector2[initialCrystalNum];
        crystalAngles = new float[initialCrystalNum];
        crystalObjects = new PatternCrystal[initialCrystalNum];

        if (boss.transform.childCount >= initialCrystalNum)
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

        if(!ReferenceEquals(patternEndCheckCoroutine, null))
        {
            CoroutineHandler.StopCoroutine(patternEndCheckCoroutine);
            patternEndCheckCoroutine = null;
        }
    }

    public override void OnUpdate()
    {
        elapsedTime += Time.deltaTime;

        ShotSequence(crystalObjects, crystalObjects.Length);
    } 

    protected void ShotSequence(PatternCrystal[] crystals, int totalCrystalNum, Vector2[] initialPos = null)
    {
        if (isShotPostBehaviour)
        {
            if(ReferenceEquals(initialPos, null))
            {
                PostShotBehaviour(boss.transform.position, crystals);
            }
            else
            {
                PostShotBehaviour(initialPos, crystals);
            }

            if (elapsedTime > 1)
            {
                isShotPostBehaviour = false;
                elapsedTime = 0;
            }
        }
        else if (!alreadyShot && elapsedTime <= shotPostDelay)
        {
            for (int i = 0; i < crystals.Length; i++)
            {
                crystals[i].transform.rotation = Quaternion.Lerp(crystalObjects[i].transform.rotation, Quaternion.Euler(0, 0, -crystalAngles[i] - 90), elapsedTime / shotPostDelay);
            }
        }
        else if (!alreadyShot && elapsedTime > shotPostDelay)
        {
            alreadyShot = true;
            ShotBehaviour(crystals);
            if (ReferenceEquals(patternEndCheckCoroutine, null))
            {
                patternEndCheckCoroutine = CoroutineHandler.StartCoroutine(CrystalAfterShotRoutine(totalCrystalNum));
            }
        }
    }

    protected virtual void PostShotBehaviour(Vector2 initialPos, PatternCrystal[] crystals)
    {
        for (int i = 0; i < crystals.Length; i++)
        {
            crystals[i].gameObject.SetActive(true);
            crystals[i].transform.position = Vector2.Lerp(initialPos, postCrystalPositions[i], elapsedTime);
        }
    }
    protected virtual void PostShotBehaviour(Vector2[] initialPos, PatternCrystal[] crystals)
    {
        for (int i = 0; i < crystals.Length; i++)
        {
            crystals[i].gameObject.SetActive(true);
            crystals[i].transform.position = Vector2.Lerp(initialPos[i], postCrystalPositions[i], elapsedTime);
        }
    }

    protected virtual void ShotBehaviour(PatternCrystal[] crystals)
    {
        for (int i = 0; i < crystals.Length; i++)
        {
            crystals[i].Shot(crystalDirections[i]);
        }
    }

    private IEnumerator CrystalAfterShotRoutine(int totalNum)
    {
        while (true)
        {
            if(disabledCrystalNum >= totalNum)
            {
                break;
            }
            yield return null;
        }
        patternEndCheckCoroutine = null;
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
