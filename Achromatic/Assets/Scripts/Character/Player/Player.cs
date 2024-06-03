using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Spine;
using Spine.Unity;

public enum EPlayerState : int
{
    IDLE,
    RUN,
    JUMP,
    DASH,
    GROGGY,
    HIT ,
    ATTACK,
    ATTACK_REBOUND,
    REBOUND,
    DEAD
}

public class Player : MonoBehaviour, IAttack
{
    public Rigidbody2D RigidbodyComp { get; private set; }
    public BoxCollider2D ColliderComp { get; private set; }
    public MeshRenderer RendererComp { get; private set; }
    public SkeletonAnimation AnimationComp { get; private set; }
    public CameraFollowObject CameraObject { get; set; }

    [SerializeField]
    private PlayerStatus stat;
    public PlayerStatus GetPlayerStat => stat;
    public int MaxHP
    {
        get
        {
            return stat.maxHP;
        }
        set
        {
            stat.maxHP = value % 2 == 0 ? value : ++value;
            PlayerMaxHPEvent?.Invoke(stat.maxHP, stat.currentHP);
        }
    }
    public int CurrentHP
    {
        get
        {
            return stat.currentHP;
        }
        set
        {
            stat.currentHP = Mathf.Min(value, stat.maxHP);
            if (stat.currentHP <= 0)
            {
                Dead();
            }
            else
            {
                PlayerCurrentHPEvent?.Invoke(stat.maxHP, stat.currentHP);
            }
        }
    }

    private List<ParticleSystem> effectList = new List<ParticleSystem>();

    private ParticleSystem parryEffect;
    private ParticleSystem dashEffect;
    private ParticleSystem runningEffect;
    private ParticleSystem attackHitEffect;
    private ParticleSystem hitEffect;

    private Dictionary<EPlayerState, PlayerBaseState> playerStates;
    private PlayerFSM playerFSM;

    [HideInInspector]
    public UnityEvent<int, int> PlayerCurrentHPEvent;
    [HideInInspector]
    public UnityEvent<int, int> PlayerMaxHPEvent;

    public bool CanChangeState { get; set; } = true;
    public bool IsDash { get; set; } = false;
    public bool IsParryDash { get; set; } = false;
    public bool StopDash { get; set; } = false;
    public bool IsInvincibility { get; set; } = false;
    public bool ParryCondition { get; set; } = false;
    public bool IsCriticalAttack{ get; set; } = false;
    public bool OnGround { get; private set; }
    public bool PlayerFaceRight { get; set; } = true;
    public float FootOffGroundTime { get; set; } = 0f;
    public Vector2 PrevDashPosition { get; set; } = Vector2.zero;

    public Collision2D ParryDashCollision { get; set; }
    public LayerMask GroundLayer { get; private set; }

    private bool randTrigger = false;
    private float bottomOffset = 0.2f;
    private float fallSpeedYDampingChangeThreshold;

    private void Awake()
    {
        playerStates = new Dictionary<EPlayerState, PlayerBaseState>();

        RigidbodyComp = GetComponent<Rigidbody2D>();
        ColliderComp = GetComponent<BoxCollider2D>();
        RendererComp = GetComponent<MeshRenderer>();
        AnimationComp = GetComponentInChildren<SkeletonAnimation>();
        CameraObject = FindObjectOfType<CameraFollowObject>();

        GameObject effects = transform.GetChild(1).gameObject;
        parryEffect = effects.transform.GetChild(0).GetComponent<ParticleSystem>();
        dashEffect = effects.transform.GetChild(1).GetComponent<ParticleSystem>();
        runningEffect = effects.transform.GetChild(2).GetComponent<ParticleSystem>();
        attackHitEffect = transform.GetChild(0).GetComponentInChildren<ParticleSystem>();
        hitEffect = effects.transform.GetChild(3).GetComponent<ParticleSystem>();

        effectList.Add(parryEffect);
        effectList.Add(dashEffect);
        effectList.Add(runningEffect);
        effectList.Add(attackHitEffect);
        effectList.Add(hitEffect);

        for (int i = 0; i < effects.transform.childCount; i++)
        {
            effectList[i].Stop();
        }
    }

    void Start()
    {
        PlayerIdleState idle = new PlayerIdleState(this);
        PlayerRunState run = new PlayerRunState(this);
        PlayerAttackState attack = new PlayerAttackState(this, transform.GetChild(0).gameObject);
        PlayerAttackReboundState afterAttack = new PlayerAttackReboundState(this);
        PlayerJumpState jump = new PlayerJumpState(this);
        PlayerDashState dash = new PlayerDashState(this);
        PlayerGroggyState groggy = new PlayerGroggyState(this);
        PlayerHitState hit = new PlayerHitState(this);
        PlayerReboundState rebound = new PlayerReboundState(this);
        PlayerDeadState dead = new PlayerDeadState(this);

        playerStates.Add(EPlayerState.IDLE, idle);
        playerStates.Add(EPlayerState.RUN, run);
        playerStates.Add(EPlayerState.ATTACK, attack);
        playerStates.Add(EPlayerState.ATTACK_REBOUND, afterAttack);
        playerStates.Add(EPlayerState.JUMP, jump);
        playerStates.Add(EPlayerState.DASH, dash);
        playerStates.Add(EPlayerState.GROGGY, groggy);
        playerStates.Add(EPlayerState.HIT, hit);
        playerStates.Add(EPlayerState.REBOUND, rebound);
        playerStates.Add(EPlayerState.DEAD, dead);

        playerFSM = new PlayerFSM(playerStates[EPlayerState.IDLE]);
        MaxHP = stat.playerHP;
        CurrentHP = stat.playerHP;

        GroundLayer = LayerMask.GetMask("Platform") | LayerMask.GetMask("Object") | LayerMask.GetMask("ColorObject");

        fallSpeedYDampingChangeThreshold = CameraManager.Instance.fallSpeedYDampingChangeThreshold;

        UISystem.Instance?.hpSliderEvent?.Invoke(CurrentHP);

        AnimationComp.AnimationState.SetAnimation(5, PlayerAnimationNameCaching.SWORD_ONOFF_ANIMATION[0], true);
    }

    private void Update()
    {
        playerFSM.UpdateState();

        Turn();

        RaycastHit2D raycastHit = Physics2D.BoxCast(ColliderComp.bounds.center, ColliderComp.bounds.size, 0f, Vector2.down, bottomOffset, GroundLayer);
        OnGround = ReferenceEquals(raycastHit.collider, null) ? false : true;
        FootOffGroundTime = OnGround ? 0 : FootOffGroundTime + Time.deltaTime;
        randTrigger = OnGround ? randTrigger : true;
        if (!OnGround && !string.Equals(AnimationComp.AnimationName, PlayerAnimationNameCaching.JUMP_ANIMATION[1]))
        {
            AnimationComp.AnimationState.SetAnimation(0, PlayerAnimationNameCaching.JUMP_ANIMATION[1], true);
        }

        if (randTrigger && OnGround)
        {
            randTrigger = false;
            AnimationComp.AnimationState.SetAnimation(0, PlayerAnimationNameCaching.RANDING_ANIMATION, false);
        }

        if (RigidbodyComp.velocity.y < fallSpeedYDampingChangeThreshold
            && !CameraManager.Instance.IsLerpingYDamping
            && !CameraManager.Instance.LerpedFromPlayerFalling)
        {
            CameraManager.Instance.LerpedFromPlayerFalling = true;
            CameraManager.Instance.LerpYDamping(true);
        }
        if(RigidbodyComp.velocity.y >= 0f && 
            !CameraManager.Instance.IsLerpingYDamping
            && CameraManager.Instance.LerpedFromPlayerFalling)
        {
            CameraManager.Instance.LerpedFromPlayerFalling = false;
            CameraManager.Instance.LerpYDamping(false);
        }

    }

    private void FixedUpdate()
    {
        playerFSM.FixedUpdateState();
    }

    private void Turn()
    {
        if (PlayerFaceRight && transform.rotation.y == 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            CameraObject.CallTurn();
        }
        else if (!PlayerFaceRight && (transform.rotation.y == -1))
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            CameraObject.CallTurn();
        }
    }
    public void ChangePrevState()
    {
        ChangeState(playerFSM.GetPrevState() == playerStates[EPlayerState.RUN] ? EPlayerState.RUN : EPlayerState.IDLE);
    }
    public bool ChangeState(EPlayerState state)
    {
        if (CanChangeState)
        {
            playerFSM.ChangeState(playerStates[state]);
            return true;
        }
        return false;
    }
    public void ControlParticles(EPlayerState state, bool onoff, int index = 0)
    {
        switch (state)
        {
            case EPlayerState.ATTACK_REBOUND:
                EffectPlayOrStop(attackHitEffect, onoff);
                break;
            case EPlayerState.DASH:
                if(index == 0)
                {
                    EffectPlayOrStop(dashEffect ,onoff);
                }
                else
                {
                    EffectPlayOrStop(parryEffect, onoff);
                }
                break;
            case EPlayerState.RUN:
                EffectPlayOrStop(runningEffect, onoff);
                break;
            case EPlayerState.HIT:
                EffectPlayOrStop(hitEffect, onoff);
                break;
            default:
                break;
        }
        
    }

    private void EffectPlayOrStop(ParticleSystem particle, bool onoff)
    {
        if (particle != null)
        {
            if (onoff && !particle.isPlaying)
            {
                particle.Play();
            }
            else if (!onoff)
            {
                particle.Stop();
            }
        }
    }

    public void OnPostAttack(Vector2 attackDir)
    {
        if (ChangeState(EPlayerState.ATTACK_REBOUND))
        {
            PlayerAttackReboundState afterAttackState = (PlayerAttackReboundState)playerStates[EPlayerState.ATTACK_REBOUND];
            afterAttackState.OnPostAttack(attackDir);
        }
    }

    public void Hit(int damage, int colorDamage, Vector2 attackDir, IParryConditionCheck parryCheck = null, bool isInfinityRebound = false)
    {
        if(IsDash || IsParryDash)
        {
            if (!ReferenceEquals(parryCheck, null))
            {
                ParryCondition = parryCheck.CanParryAttack() ? true : ParryCondition;
            }
            return;
        }

        if (!IsInvincibility)
        {
            if (ChangeState(EPlayerState.HIT))
            {
                PlayerHitState hitState = (PlayerHitState)playerStates[EPlayerState.HIT];
                hitState.Hit(damage, attackDir.normalized);
            }
        }
        else if (isInfinityRebound)
        {
            Rebound(attackDir);
        }
    }

    public void Rebound(Vector2 dir)
    {
        if (ChangeState(EPlayerState.REBOUND))
        {
            PlayerReboundState reboundState = (PlayerReboundState)playerStates[EPlayerState.REBOUND];
            reboundState.Rebound(dir, stat.hitReboundPower, stat.hitReboundTime);
        }
    }

    private void Dead()
    {
        Debug.Log("Player Dead");
        SceneManager.LoadScene(0);
    }

    public Vector2 GetPlayerMoveDirection()
    {
        return RigidbodyComp.velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.ENEMY_TAG) && (IsDash || IsParryDash))
        {
            int damage = IsParryDash ? stat.parryDashDamage : stat.dashDamage;
            collision.gameObject.GetComponent<Monster>()?.Hit(damage, damage,
                collision.transform.position - transform.position);
            
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (IsParryDash && collision.gameObject.CompareTag(PlayManager.ENEMY_TAG))
        {
            ParryDashCollision = collision;
        }

    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsParryDash)
        {
            ParryDashCollision = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsDash && collision.CompareTag(PlayManager.ATTACK_TAG))
        {
            IParryConditionCheck checkParry = collision.GetComponent<IParryConditionCheck>();
            if (checkParry != null && checkParry.CanParryAttack())
            {
                ParryCondition = true;
            }
        }
    }
}
