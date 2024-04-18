using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// isRunning
/// isGroggy
/// onGround
/// 
/// dashTrigger
/// attackTrigger
/// parryTrigger
/// hitTrigger
/// jumpTrigger
/// 
/// </summary>


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

    public CameraFollowObject CameraObject { get; set; }

    [SerializeField]
    private PlayerStatus stat;
    public PlayerStatus GetPlayerStat => stat;

    public int currentHP
    {
        get
        {
            return stat.currentHP;
        }
        set
        {
            stat.currentHP = value;
            if (stat.currentHP < 0)
            {
                Dead();
            }
            
        }
    }

    private Dictionary<ePlayerState, PlayerBaseState> playerStates;
    private PlayerFSM playerFSM;

    public bool IsDash { get; set; } = false;
    public bool IsParryDash { get; set; } = false;

    public bool IsInvincibility { get; set; } = false;

    public bool ParryCondition { get; set; } = false;
    public bool IsCriticalAttack{ get; set; } = false;
    public bool OnGround { get; private set; }
    public bool PlayerFaceRight { get; set; } = true;

    public LayerMask GroundLayer { get; private set; }
    private float bottomOffset = 0.2f;
    private float fallSpeedYDampingChangeThreshold;

    public Collision2D ParryDashCollision { get; set;}
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

        effectList.Add(parryEffect);
        effectList.Add(dashEffect);
        effectList.Add(runningEffect);
        effectList.Add(attackHitEffect);

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
        stat.currentHP = stat.playerHP;

        GroundLayer = (1 << LayerMask.NameToLayer("Platform")) | (1 << LayerMask.NameToLayer("Object")) | (1 << LayerMask.NameToLayer("ColorObject"));

        fallSpeedYDampingChangeThreshold = CameraManager.Instance.fallSpeedYDampingChangeThreshold;
    }

    private void Update()
    {
        playerFSM.UpdateState();

        Turn();

        RaycastHit2D raycastHit = Physics2D.BoxCast(ColliderComp.bounds.center, ColliderComp.bounds.size, 0f, Vector2.down, bottomOffset, GroundLayer);
        if (raycastHit.collider != null)
        {
            OnGround = true;
            AnimatorComp.SetBool("onGround", true);
        }
        else
        {
            OnGround = false;
            AnimatorComp.SetBool("onGround", false);
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
    public void ChangePrevState()
    {
        playerFSM.ChangePrevState();
    }
    public void ChangeState(ePlayerState state)
    {
        playerFSM.ChangeState(playerStates[state]);
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
            default:
                break;
        }
        
    }

    private void EffectPlayOrStop(ParticleSystem particle, bool onoff)
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

    public void AfterAttack(Vector2 attackDir)
    {
        PlayerAttackReboundState afterAttackState = (PlayerAttackReboundState)playerStates[ePlayerState.ATTACK_REBOUND];
        afterAttackState.AfterAttack(attackDir);
        ChangeState(ePlayerState.ATTACK_REBOUND);
    }


    public void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage)
    {
        if(IsDash || IsParryDash)
        {
            return;
        }

        PlayerHitState hitState = (PlayerHitState)playerStates[ePlayerState.HIT];
        hitState.Hit(damage, attackDir);
        ChangeState(ePlayerState.HIT);
    }

    private void Dead()
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.ENEMY_TAG))
        {
            if (IsDash || IsParryDash)
            {
                int damage = IsParryDash ? stat.parryDashDamage : stat.dashDamage;
                collision.gameObject.GetComponent<TestEnemy>().Hit(damage, 
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
            else if(projectile != null)
            {
                ParryCondition = true;
            }

        }

    }
}
