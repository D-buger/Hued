using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;
using Spine.Unity;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class SpyderEnemy : Monster, IAttack, IParry
{
    [Header("Components")]              // 바로 밑에 직렬화 시키는 변수가 없는데 헤더 선언은 불필요합니다
    private Rigidbody2D rigid;          
    private GameObject attackPoint;     
    private Attack meleeAttack;         
    [SerializeField]
    private SpyderMonsterStats stat;
    [SerializeField, Space]
    private Projectile rangedAttack;

    [Header("Animation")]
    [SerializeField]
    private SkeletonAnimation skeletonAnimation;
    [SerializeField]
    private AnimationReferenceAsset[] aniClip;

    public enum AnimaState      // enum을 쓸 때는 앞에 e를 붙입니다 (ex. eAnimaState)
    {
        Idle, Walk, Charge, Ground, Spit, Detection, Dead   // 보통은 enum을 쓸 때 한줄에 하나의 요소씩 줄바꿈을 합니다
    }
    private AnimaState animState;

    private string currentAnimation;

    private float elapsedTime = 0;
    private float arrivalThreshold = 1f;
    private float distanceToPlayer = 0;
    private float angleThreshold = 48f;

    [SerializeField, Tooltip("몬스터 기준 이동 범위")]
    private float runPosition;                              //권장사항으론 위의 직렬화시키는 변수들과 같이 선언하는게 보기 편할 것 같습니다

    public ParticleSystem[] earthObjectGroup;

    public UnityEvent<eActivableColor> spyderColorEvent;

    public GameObject[] earthObjectGroup1;                  // 세줄을 하나의 변수로 묶을 수 있습니다 (2, 3 배열)
    public GameObject[] earthObjectGroup2;                  // 그리고 왜 퍼블릭으로 선언했는지 싶습니다 그냥 직렬화해도 괜찮았을 것 같아요
    public GameObject[] earthObjectGroup3;

    private Vector2 startPosition;                          // 변수 이름에 직관성이 너무 떨어집니다.
    private Vector2 targetPosition;                         
    private Vector2 thisPosition;                           // 마찬가지로 변수 이름이 좀 더 길어지더라도 목적을 명확하게 해주면 더 좋을것같아요
    private Vector3 startPos;
    private Vector3 endPos;

    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;
    private LayerMask originLayer;
    private LayerMask colorVisibleLayer;

    private bool isBettle = false;                         // 스펠링 체크는 생활화합시다
    private bool canAttack = true;
    private bool isAttack = false;
    private bool isWait = true;
    private bool isfirstAttack = false;                     // 코드 컨벤션 오류입니다 f가 대문자가 되어야합니다. 놓치기 쉬우니 잘 확인해주세요
    private bool playerBetweenPositions = false;
    private bool isEarthAttack = false;
    private bool isHeavy = false;
    private bool gameStart = false;                         //이건 나중에 말하겠습니다

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        attackPoint = transform.GetChild(0).gameObject;
        meleeAttack = attackPoint.GetComponentInChildren<Attack>();
    }
    private void Start()
    {
        startPos = new Vector3(transform.position.x + runPosition, transform.position.y);
        endPos = new Vector3(transform.position.x - runPosition, transform.position.y);
        startPosition.y = transform.position.y;
        targetPosition.y = transform.position.y;
        startPosition.x += transform.position.x + runPosition;
        targetPosition.x += transform.position.x - runPosition;

        currentHP = stat.MonsterHP;
        thisPosition = targetPosition;
        originLayer = gameObject.layer;
        colorVisibleLayer = LayerMask.NameToLayer("ColorEnemy");
        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, stat.enemyColor, null);
        MonsterManager.Instance?.getColorEvent.AddListener(CheckIsHeavy);
        gameStart = true;

        PlayManager.Instance.FilterColorAttackEvent.AddListener(IsActiveColor);
        PlayManager.Instance.UpdateColorthing();
    }

    private void CheckIsHeavy(eActivableColor color)
    {
        if (color == stat.enemyColor)
        {
            isHeavy = false;
        }
        spyderColorEvent?.Invoke(color);
    }

    private void Update()
    {
        if (isDead)                     // Dead 상태 체크같이 단발성으로 이루어지는 것들은 해당 조건이 불리는 타이밍에 체크해주는게 좋습니다
        {                               
            StartCoroutine(Dead());     // Dead로 예시를 들면, 피가 닳았을 때 해당을 체크해주고 함수를 실행해주는게 훨씬 효율이 좋겠죠
        }
        else
        {
            CheckDead();                // 위와 마찬가지입니다
        }
        CheckPlayer();
        if (isWait)
        {
            WaitSituation();
            animState = AnimaState.Walk;
        }
        else if (canAttack && isBettle)
        {
            Attack(PlayerPos);
        }

        if (!playerBetweenPositions)
        {
            CheckWaitTime();
        }
        SetCurrentAniamtion(animState);
        if (!gameStart)
        {
            gameStart = true;               // gameStart는 어떠한 상황에서도 false가 될 수 없습니다. 애초에 false시키는 코드도 없고요. 비효율적입니다
        }
    }
    private void AsncAnimation(AnimationReferenceAsset animClip, bool loop, float timeScale)
    {
        if (animClip.name.Equals(currentAnimation))
            return;                                 // 이건 우리 팀의 컨벤션이긴한데 한블럭의 코드도 중괄호로 묶어주세요

        skeletonAnimation.state.SetAnimation(0, animClip, loop).TimeScale = timeScale;
        skeletonAnimation.loop = loop;
        skeletonAnimation.timeScale = timeScale;
        currentAnimation = animClip.name;
    }
    private void SetCurrentAniamtion(AnimaState _state)
    {
        switch (_state)
        {
            case AnimaState.Idle:
                AsncAnimation(aniClip[(int)AnimaState.Idle], true, 1f); // 항상 매직넘버는 지양하는 것이 좋습니다. 이런 단순한 숫자라도 위에 변수를 선언해주면 나중에 수정해야할 때 유연한 대처가 가능합니다
                break;
            case AnimaState.Walk:
                AsncAnimation(aniClip[(int)AnimaState.Walk], true, 1f); // 물론 각각 다 따로 만들란 얘기는 아니고 timeScale이란 변수 하나만 만들어 놓고 모두에게 적용시키면 된다는 뜻입니다
                break;
            case AnimaState.Charge:
                AsncAnimation(aniClip[(int)AnimaState.Charge], false, 1f);
                break;
            case AnimaState.Ground:
                AsncAnimation(aniClip[(int)AnimaState.Ground], false, 1f);
                break;
            case AnimaState.Spit:
                AsncAnimation(aniClip[(int)AnimaState.Spit], false, 1f);
                break;
            case AnimaState.Detection:
                AsncAnimation(aniClip[(int)AnimaState.Detection], false, 1f);
                break;
            case AnimaState.Dead:
                AsncAnimation(aniClip[(int)AnimaState.Dead], false, 1f);
                break;
        }
    }

    private void CheckPlayer()
    {
        if (IsBetween(PlayerPos.x, startPosition.x, targetPosition.x))  // 구현쪽 얘기인데 굳이 start와 end로 구분짓지 않고, 자신의 위치와 distance 길이값을 이용해서 원 형식으로 플레이어를 탐지하는걸 추천합니다
        {
            playerBetweenPositions = true;
            isBettle = true;
            isWait = false;
            elapsedTime = 0f;
        }
        else
        {
            playerBetweenPositions = false;
        }

        distanceToPlayer = Vector2.Distance(transform.position, PlayerPos);
    }
    private bool IsBetween(float value, float start, float end)     // 여러번 사용되지 않는 함수는 굳이 따로 선언하지 않아도 됩니다
    {
        return value >= Mathf.Min(start, end) && value <= Mathf.Max(start, end);
    }

    private void WaitSituation()
    {
        currentHP = stat.MonsterHP;
        isfirstAttack = true;
        isBettle = false;
        transform.position = Vector2.MoveTowards(transform.position, thisPosition, stat.moveSpeed * Time.deltaTime);

        if (HasArrived((Vector2)transform.position, targetPosition))
        {
            transform.localScale = new Vector3(-1, 1, 1);
            thisPosition = startPosition;
        }
        if (HasArrived((Vector2)transform.position, startPosition))
        {
            transform.localScale = new Vector3(1, 1, 1);
            thisPosition = targetPosition;
        }
    }
    public void CheckWaitTime()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= stat.usualTime)
        {
            elapsedTime = 0f;
            isWait = true;
        }
    }
    private bool HasArrived(Vector2 currentPosition, Vector2 targetPosition)    // 이런식으로 여러번 사용된 함수는 잘 빼셨습니다
    {
        return Vector2.Distance(currentPosition, targetPosition) <= arrivalThreshold;
    }
    private void IsActiveColor(eActivableColor color)
    {
        if (color != stat.enemyColor)
        {
            gameObject.layer = originLayer;
        }
        else
        {
            gameObject.layer = colorVisibleLayer;
        }
    }


    public void Attack(Vector2 vec)
    {
        StartCoroutine(MoveToPlayer());
    }

    IEnumerator AttackSequence(Vector2 attackAngle) // 문제의 Attack관련 함수입니다. 함수가 너무 비대합니다. 나눌 필요성이 있어요
    {
        isAttack = true;
        canAttack = false;
        float horizontalValue = attackAngle.x - transform.position.x;
        float verticalValue = attackAngle.y - transform.position.y;
        Vector2 value = new Vector2(horizontalValue, verticalValue);
        Vector2 check = new Vector2(1.0f, 0);
        float angleToPlayer = Mathf.Atan2(attackAngle.y, transform.position.y) * Mathf.Rad2Deg;
        bool facingPlayer = Mathf.Abs(angleToPlayer - transform.eulerAngles.z) < angleThreshold;
        double yAngle = Math.Atan2(horizontalValue, verticalValue);         // 일관성있게 Math 말고 Mathf를 씁시다

        if (value.x <= 0)
        {
            transform.localScale = new Vector2(1, 1);
            check = new Vector2(-0.998f, 0);            // 1도 아니고 이상한 매직넘버 쓰지 마세요
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
            check = new Vector2(0.998f, 0);
        }

        animState = AnimaState.Detection;
        SetCurrentAniamtion(animState);
        yield return Yields.WaitSeconds(1.34f);         // 마찬가지입니다. 이상한 매직넘버 쓰지 마세요 => animation에 맞출거면 spine에서 제공 함수나 변수 찾아보고 공식을 세워서 자동으로 맞춰지게 하세요. 일일이 맞추지 말고
        if (isfirstAttack)
        {
            yield return Yields.WaitSeconds(0.10f);     // 이런것도 매직넘버 말고 변수에 담아서 사용하세요
            animState = AnimaState.Spit;
            SetCurrentAniamtion(animState);
            Projectile attack = Instantiate(rangedAttack.gameObject).GetComponent<Projectile>();    // 이후에 ObjectPooling으로 변경하세요
            attack.transform.rotation = Quaternion.Euler(0, 0, (float)yAngle);      // float 캐스팅을 할거면 처음부터 float으로 선언하시고, 왜 yAngle인데 z축을 바꾸고 있습니까 네이민 신경써주세요
            attack.Shot(gameObject, transform.position, new Vector2(horizontalValue, verticalValue).normalized,
                    stat.rangedAttackRange, stat.rangedAttackSpeed, stat.rangedAttackDamege, isHeavy, eActivableColor.RED);
            spyderColorEvent.AddListener(attack.CheckIsHeavyAttack);
            PlayManager.Instance.UpdateColorthing();
            isfirstAttack = false;
        }
        else if (distanceToPlayer < stat.meleeAttackRange)
        {
            float randomChance = UnityEngine.Random.value;  
            if (facingPlayer && randomChance <= stat.specialAttackPercent/100)  // 왜 굳이 float으로 랜덤값을 받아서 나눗셈 연산을 합니까. 정수형으로 받아서 비교하면 더 좋습니다
            {
                animState = AnimaState.Charge;
                SetCurrentAniamtion(animState);
                yield return Yields.WaitSeconds(1.3f);      // 매직넘버
                facingPlayer = false;                       // 왜 false로 바꿔주는지 모르겠습니다

                meleeAttack?.AttackAble(-value, stat.attackDamage);
                rigid.AddForce(check * stat.specialAttackRound, ForceMode2D.Impulse);
            }
            else
            {
                animState = AnimaState.Ground;
                SetCurrentAniamtion(animState);
                yield return Yields.WaitSeconds(1.8f);      // 매직넘버
                rigid.velocity = Vector2.up * stat.earthAttackJump;
                isEarthAttack = true;

                StartCoroutine(SpawnObjects());
            }
        }
        else if (distanceToPlayer > stat.meleeAttackRange && distanceToPlayer < stat.rangedAttackRange)
        {
            animState = AnimaState.Spit;
            SetCurrentAniamtion(animState);
            yield return Yields.WaitSeconds(0.14f);         // 매직넘버
            Projectile attack = Instantiate(rangedAttack.gameObject).GetComponent<Projectile>();
            attack.transform.rotation = Quaternion.Euler(horizontalValue, verticalValue, 0);
            attack.Shot(gameObject, transform.position, new Vector2(horizontalValue, verticalValue).normalized,
                    stat.rangedAttackRange, stat.rangedAttackSpeed, stat.rangedAttackDamege, isHeavy, eActivableColor.RED);
            spyderColorEvent.AddListener(attack.CheckIsHeavyAttack);

        }
        yield return Yields.WaitSeconds(stat.attackTime);
        isAttack = false;
        meleeAttack?.AttackDisable();
        yield return Yields.WaitSeconds(stat.attackCooldown);
        canAttack = true;
    }

    private IEnumerator MoveToPlayer()
    {
        while (!isAttack && !isWait)
        {
            yield return new WaitForSeconds(stat.attackCooldown);
            float horizontalValue = PlayerPos.x - transform.position.x;
            float verticalValue = PlayerPos.y - transform.position.y;   // 불필요한 변수선언

            if (horizontalValue > 0)
            {
                skeletonAnimation.initialFlipX = false;             // 삼항연산자로 간단하게 줄일 수 있습니다
            }
            else
            {
                skeletonAnimation.initialFlipX = true;
            }

            if (PlayerPos == null)                                  // 말했듯 우리팀 코딩 컨벤션에서는 중괄호로 감싸줘야합니다
                yield break;

            if (distanceToPlayer <= stat.rangedAttackRange && canAttack)
            {
                StartCoroutine(AttackSequence(PlayerPos));
                yield break;
            }
            else if (distanceToPlayer > stat.rangedAttackRange)
            {
                // 주석을 쓸 때는 "FIX : X 좌표로만 이동하게 변경" 이런식으로 써야 직관성이 높습니다
                //FIX X좌표로만 이동하게 변경
                transform.position = Vector2.MoveTowards(transform.position, PlayerPos, stat.moveSpeed * Time.deltaTime);
            }
        }
    }
    private IEnumerator SpawnObjects()
    {
        while (isEarthAttack)
        {
            yield return new WaitForSeconds(0.6f);      // 매직넘버

            ActivateParticle(earthObjectGroup);

            isEarthAttack = false;

            ActivateObjects(earthObjectGroup1);         // 위에서 말했듯이 하나의 배열안에 담았으면 for문으로 간단히 할 수 있습니다
            yield return new WaitForSeconds(0.1f);      // 매직넘버들
            DeactivateObjects(earthObjectGroup1);
            yield return new WaitForSeconds(0.3f);

            ActivateObjects(earthObjectGroup2);
            yield return new WaitForSeconds(0.1f);
            DeactivateObjects(earthObjectGroup2);
            yield return new WaitForSeconds(0.3f);

            ActivateObjects(earthObjectGroup3);
            yield return new WaitForSeconds(0.1f);
            DeactivateObjects(earthObjectGroup3);
            yield return new WaitForSeconds(0.3f);
        }
    }
    private void ActivateObjects(GameObject[] objects)  // 같은 기능인데 bool값 하나만 다르다면, 차라리 매개변수를 하나 추가해서 SetActive 안에 넣으십쇼
    {
        foreach (GameObject obj in objects)
        {
            obj.SetActive(true);
        }
    }

    private void DeactivateObjects(GameObject[] objects)
    {
        foreach (GameObject obj in objects)
        {
            obj.SetActive(false);
        }
    }

        private void ActivateParticle(ParticleSystem[] objects)
    {
         StartCoroutine(PlayObjects(objects));
    }
    private IEnumerator PlayObjects(ParticleSystem[] objects)
    {
        int objectsPerBatch = 2;
        int batches = Mathf.CeilToInt((float)objects.Length / objectsPerBatch); // 무슨 연산인지 모르겠습니다. 

        for (int i = 0; i < batches; i++)
        {
            for (int j = 0; j < objectsPerBatch; j++)
            {
                int index = i * objectsPerBatch + j;
                if (index < objects.Length && objects[index] != null)   
                {
                    objects[index].Play();          // 그냥 파티클시스템 for문 하나로만 쭉 플레이 할 수 있지 않나요. 굳이 이중for문에 위의 연산까지 해야하는 이유를 모르겠습니다  
                }
            }
            yield return new WaitForSeconds(stat.earthAttackTime);
        }
    }

    public override void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage = 0)
    {
        if (!isHeavyAttack)
        {
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                currentHP -= criticalDamage;
                rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
            }
            else
            {
                currentHP -= damage;
                rigid.AddForce(attackDir * stat.hitReboundPower, ForceMode2D.Impulse);
            }
        }
        else
        {
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                currentHP -= damage;
                rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
            }
        }
    }

    private IEnumerator Dead()
    {
        isDead = false;
        skeletonAnimation.state.GetCurrent(0).TimeScale = 0;
        skeletonAnimation.state.GetCurrent(0).TimeScale = 1;
        animState = AnimaState.Dead;
        SetCurrentAniamtion(animState);
        yield return new WaitForSeconds(1.5f);      // 매직넘버
        gameObject.SetActive(false);
        yield return null;                  // 불필요한 줄
    }

    private void OnDrawGizmos()      // 항상 기즈모 그리는건 클래스의 최하단부에 위치해야합니다
    {
        if (null != stat)
        {
            if (stat.meleeAttackRange >= distanceToPlayer)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawWireSphere(transform.position + transform.forward, stat.senseCircle);
        }
        if (!gameStart)         // 시스템상에서 이미 게임이 시작했는지를 아는 변수가 있는걸로 압니다. 불필요한 변수에요
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x + runPosition, transform.position.y), 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x - runPosition, transform.position.y), 0.5f);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPos, 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPos, 0.5f);
        }

    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            collision.gameObject.GetComponent<Player>().Hit(stat.contactDamage,
                    transform.position - collision.transform.position, false, stat.contactDamage);
        }
    }

    public void Parried()   // 구현하지 않는 인터페이스는 상속받지 말도록 합시다
    {
        throw new NotImplementedException();
    }
}
