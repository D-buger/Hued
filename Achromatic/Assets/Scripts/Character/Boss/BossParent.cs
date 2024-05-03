using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossParent : MonoBehaviour
{
    public Rigidbody2D RigidbodyComp { get; private set; }
    public Collider2D ColliderComp { get; private set; }

    [SerializeField]
    private BossStatus bossStatus;
    public BossStatus GetBossStatus => bossStatus;

    protected BossPattern previousPattern = null;
    protected BossPattern currentPattern = null;
    protected List<BossPattern> patternPool;

    protected bool isPatternEnd = false;
    protected bool isPlayerInRoom = false;
    protected bool isChangePhase = false;
    protected bool isDead = false;

    private float patternDelayTimer = 0;
    private void Awake()
    {
        RigidbodyComp = GetComponent<Rigidbody2D>();
        ColliderComp = GetComponent<Collider2D>();
        patternPool.AddRange(GetBossStatus.patterns);
        OnAwake();
    }

    protected abstract void OnAwake();
    private void Update()
    {
        patternDelayTimer += Time.deltaTime;
        if (isPatternEnd)
        {
            isPatternEnd = false;
            patternDelayTimer = 0;
        }

        if (patternDelayTimer > GetBossStatus.patternDelayTime &&
            ReferenceEquals(currentPattern, null))
        {
            ChoosePattern();
        }

        if(!ReferenceEquals(currentPattern, null))
        {
            currentPattern.Update();   
        }

        OnUpdate();
    }
    protected abstract void OnUpdate();

    private void ChoosePattern()
    {
        if(!ReferenceEquals(currentPattern, null))
        {
            return;
        }

        currentPattern = patternPool[Random.Range(0, patternPool.Count)];
        patternPool.Remove(currentPattern);
        currentPattern.Start();
    }

    public void CurrentPatternEnd()
    {
        previousPattern = currentPattern;
        currentPattern = null;
        patternPool.Add(previousPattern);
        isPatternEnd = true;
    }

    private void OnDrawGizmos()
    {
        if(!ReferenceEquals(currentPattern, null))
        {
            currentPattern.OnDrawGizmos();
        }
    }
}