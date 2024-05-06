using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossParent : MonoBehaviour, IAttack
{
    public Rigidbody2D RigidbodyComp { get; private set; }
    public Collider2D ColliderComp { get; private set; }

    [SerializeField]
    private BossStatus bossStatus;
    public BossStatus GetBossStatus => bossStatus;

    private int currentHp;
    public int CurrentHp
    {
        get
        {
            return currentHp;
        }
        set
        {
            currentHp = value;
            if(currentHp <= bossStatus.maxHp * 0.5f)
            {
                intendedPattern = isEndPhase? intendedPattern : bossStatus.phaseChangePattern;
            }
        }
    }

    protected BossPattern previousPattern = null;
    protected BossPattern currentPattern = null;
    protected BossPattern intendedPattern = null;
    protected List<BossPattern> startPhasePatternPool = new List<BossPattern>();
    protected List<BossPattern> endPhasePatternPool = new List<BossPattern>();

    protected bool isPatternEnd = false;
    protected bool isPlayerInRoom = false;
    protected bool isChangePhase = false;
    protected bool isEndPhase = false;
    protected bool isDead = false;

    private float patternDelayTimer = 0;
    private void Awake()
    {
        RigidbodyComp = GetComponent<Rigidbody2D>();
        ColliderComp = GetComponent<Collider2D>();
        for(int i = 0; i < GetBossStatus.startPhasePatterns.Length; i++)
        {
            startPhasePatternPool.Add(GetBossStatus.startPhasePatterns[i].SetBossPattern(this));
        }
        for (int i = 0; i < GetBossStatus.endPhasePatterns.Length; i++)
        {
            endPhasePatternPool.Add(GetBossStatus.endPhasePatterns[i].SetBossPattern(this));
        }
        currentHp = bossStatus.maxHp;
        OnAwake();
    }

    protected abstract void OnAwake();
    private void Update()
    {
        PatternActions();

        OnUpdate();
    }

    protected abstract void OnUpdate();
    #region pattern action
    private void PatternActions()
    {
        patternDelayTimer += Time.deltaTime;
        if (isPatternEnd)
        {
            isPatternEnd = false;
            patternDelayTimer = 0;
        }
        if (ReferenceEquals(currentPattern, null))
        {
            if (!ReferenceEquals(intendedPattern, null))
            {
                currentPattern = intendedPattern;
                intendedPattern = null;
                currentPattern.OnStart();
                isEndPhase = true;
            }
            else if (patternDelayTimer > GetBossStatus.patternDelayTime)
            {
                //ChoosePattern(isEndPhase ? startPhasePatternPool : endPhasePatternPool);
                ChoosePattern(startPhasePatternPool);
            }
        }

        if(!ReferenceEquals(currentPattern, null))
        {
            currentPattern.OnUpdate();   
        }
    }

    private void ChoosePattern(List<BossPattern> patternPool)
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
    #endregion
    public void OnPostAttack(Vector2 attackDir)
    {

    }

    public void Hit(int damage, int colorDamage, Vector2 attackDir, IParryConditionCheck parryCheck = null)
    {
        CurrentHp -= PlayManager.Instance.ContainsActivationColors(bossStatus.bossColor) ? colorDamage : damage;
    }

    private void OnDrawGizmos()
    {
        if(!ReferenceEquals(currentPattern, null))
        {
            currentPattern.DrawGizmos();
        }
    }

}
