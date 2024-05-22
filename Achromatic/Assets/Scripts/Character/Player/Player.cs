using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public enum ePlayerState : int
{
    IDLE,
    RUN,
    JUMP,
    DASH,
    GROGGY,
    HIT ,
    ATTACK,
    ATTACK_REBOUND,
    DEAD
}

public class Player : MonoBehaviour, IAttack
{
    public Rigidbody2D RigidbodyComp { get; private set; }
    public BoxCollider2D ColliderComp { get; private set; }
    public SpriteRenderer RendererComp { get; private set; }
    public Animator AnimatorComp { get; private set; }

    private List<ParticleSystem> effectList = new List<ParticleSystem>();

    private ParticleSystem parryEffect;
    private ParticleSystem dashEffect;
    private ParticleSystem runningEffect;
    private ParticleSystem attackHitEffect;
    private ParticleSystem hitEffect;

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
            Debug.Log("max hp : " + stat.maxHP);
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
            Debug.Log("current hp : " + stat.currentHP);
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
    [HideInInspector]
    public UnityEvent<int, int> PlayerCurrentHPEvent;
    [HideInInspector]
    public UnityEvent<int, int> PlayerMaxHPEvent;

    private Dictionary<ePlayerState, PlayerBaseState> playerStates;
    private PlayerFSM playerFSM;

    public bool CanChangeState { get; set; } = true;
    public bool IsDash { get; set; } = false;
    public bool IsParryDash { get; set; } = false;

    public bool IsInvincibility { get; set; } = false;

    public bool ParryCondition { get; set; } = false;
    public bool IsCriticalAttack{ get; set; } = false;
    public bool OnGround { get; private set; }
    public float footOffGroundTime { get; set; } = 0f;
    public bool PlayerFaceRight { get; set; } = true;

    public Collision2D ParryDashCollision { get; set; }
    public LayerMask GroundLayer { get; private set; }
    private float bottomOffset = 0.2f;
    private float fallSpeedYDampingChangeThreshold;
    
    private void Awake()
    {
        playerStates = new Dictionary<ePlayerState, PlayerBaseState>();

        RigidbodyComp = GetComponent<Rigidbody2D>();
        ColliderComp = GetComponent<BoxCollider2D>();
        RendererComp = GetComponent<SpriteRenderer>();
        AnimatorComp = GetComponent<Animator>();
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
        PlayerDeadState dead = new PlayerDeadState(this);

        playerStates.Add(ePlayerState.IDLE, idle);
        playerStates.Add(ePlayerState.RUN, run);
        playerStates.Add(ePlayerState.ATTACK, attack);
        playerStates.Add(ePlayerState.ATTACK_REBOUND, afterAttack);
        playerStates.Add(ePlayerState.JUMP, jump);
        playerStates.Add(ePlayerState.DASH, dash);
        playerStates.Add(ePlayerState.GROGGY, groggy);
        playerStates.Add(ePlayerState.HIT, hit);
        playerStates.Add(ePlayerState.DEAD, dead);

        playerFSM = new PlayerFSM(playerStates[ePlayerState.IDLE]);
        MaxHP = stat.playerHP;
        CurrentHP = stat.playerHP;

        GroundLayer = (1 << LayerMask.NameToLayer("Platform")) | (1 << LayerMask.NameToLayer("ColorObject"));

        fallSpeedYDampingChangeThreshold = CameraManager.Instance.fallSpeedYDampingChangeThreshold;

        UISystem.Instance?.hpSliderEvent?.Invoke(CurrentHP);
    }

    private void Update()
    {
        playerFSM.UpdateState();

        Turn();

        RaycastHit2D raycastHit = Physics2D.BoxCast(ColliderComp.bounds.center, ColliderComp.bounds.size, 0f, Vector2.down, bottomOffset, GroundLayer);
        OnGround = ReferenceEquals(raycastHit.collider, null) ? false : true;
        AnimatorComp.SetBool("onGround", OnGround);
        footOffGroundTime = OnGround ? 0 : footOffGroundTime + Time.deltaTime;

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
        ChangeState(playerFSM.GetPrevState() == playerStates[ePlayerState.RUN] ? ePlayerState.RUN : ePlayerState.IDLE);
    }
    public void ChangeState(ePlayerState state)
    {
        playerFSM.ChangeState(playerStates[state]);
    }
    public void ControlParticles(ePlayerState state, bool onoff, int index = 0)
    {
        switch (state)
        {
            case ePlayerState.ATTACK_REBOUND:
                EffectPlayOrStop(attackHitEffect, onoff);
                break;
            case ePlayerState.DASH:
                if(index == 0)
                {
                    EffectPlayOrStop(dashEffect ,onoff);
                }
                else
                {
                    EffectPlayOrStop(parryEffect, onoff);
                }
                break;
            case ePlayerState.RUN:
                EffectPlayOrStop(runningEffect, onoff);
                break;
            case ePlayerState.HIT:
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
        PlayerAttackReboundState afterAttackState = (PlayerAttackReboundState)playerStates[ePlayerState.ATTACK_REBOUND];
        ChangeState(ePlayerState.ATTACK_REBOUND);
        afterAttackState.OnPostAttack(attackDir);
    }


    public void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage)
    {
        if(IsDash || IsParryDash)
        {
            return;
        }
        PlayerHitState hitState = (PlayerHitState)playerStates[ePlayerState.HIT];
        ChangeState(ePlayerState.HIT);
        hitState.Hit(damage, attackDir.normalized);
    }

    private void Dead()
    {
        Debug.Log("Player Dead");
        SceneManager.LoadScene(0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.ENEMY_TAG))
        {
            if (IsDash || IsParryDash)
            {
                int damage = IsParryDash ? stat.parryDashDamage : stat.dashDamage;
                collision.gameObject.GetComponent<Monster>()?.Hit(damage, 
                    collision.transform.position - transform.position, false, damage);
            }
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
            Attack attack = collision.GetComponent<Attack>();
            Projectile projectile = collision.GetComponent<Projectile>();
            if (attack != null && attack.isCanParryAttack(PlayManager.PLAYER_TAG))
            {
                ParryCondition = true;
            }
            else if(projectile != null && projectile.IsParryAllow)
            {
                ParryCondition = true;
            }

        }

    }
}
