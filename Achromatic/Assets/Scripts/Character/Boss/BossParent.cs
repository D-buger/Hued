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
    protected List<BossPattern> patternPool = new List<BossPattern>();

    protected bool isPatternEnd = false;
    protected bool isPlayerInRoom = false;
    protected bool isChangePhase = false;
    protected bool isDead = false;

    private float patternDelayTimer = 0;
    private void Awake()
    {
        RigidbodyComp = GetComponent<Rigidbody2D>();
        ColliderComp = GetComponent<Collider2D>();
        for(int i = 0; i < GetBossStatus.startPhasePatterns.Length; i++)
        {
            patternPool.Add(GetBossStatus.startPhasePatterns[i].SetBossPattern(this));
        }
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
            currentPattern.OnUpdate();   
        }

        OnUpdate();
    }

    protected abstract void OnUpdate();

    private void ChoosePattern()
    {
        currentPattern = patternPool.Count <= 0 ? previousPattern : patternPool[Random.Range(0, patternPool.Count)];

        if (!ReferenceEquals(previousPattern, null))
        {
            patternPool.Add(previousPattern);
        }
        patternPool.Remove(currentPattern);
        currentPattern.OnStart();
    }

    public void CurrentPatternEnd()
    {
        previousPattern = currentPattern;
        currentPattern = null;
        isPatternEnd = true;
    }

    private void OnDrawGizmos()
    {
        if(!ReferenceEquals(currentPattern, null))
        {
            currentPattern.DrawGizmos();
        }
    }
}
