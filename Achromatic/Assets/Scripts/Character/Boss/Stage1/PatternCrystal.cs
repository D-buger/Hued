using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PatternCrystal : MonoBehaviour, IParryConditionCheck
{
    private Coroutine shakeCoroutine = null;

    private eActivableColor patternColor;

    private int crystalDamage;
    private int explosionDamage;
    private float moveSpeed;
    private float elapsedTime;
    private float explosionRadius;
    private float postExplosionDelay;
    private float explosionTime;
    private float shakeTime;
    private float shakeAngle = 20;
    private float shakeOneSideDuration = 0.1f;

    private Vector2 moveDirection;

    private bool isEnable;
    private bool isShot;
    private bool isTouchFloor;
    private bool isExplosion;

    public UnityEvent afterExplosionEvent;

    private void OnEnable()
    {
        isShot = false;
        isTouchFloor = false;
        isExplosion = false;
    }
    private void Update()
    {
        if (!isEnable)
        {
            return;
        }

        if (isShot && !isTouchFloor)
        {
            transform.position += (Vector3)(moveDirection * moveSpeed);
        }
        else if (!isExplosion)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime > postExplosionDelay)
            {
                isExplosion = true;
                elapsedTime = 0;
            }
            else if(ReferenceEquals(shakeCoroutine, null) && elapsedTime > postExplosionDelay - shakeTime)
            {
                shakeCoroutine = StartCoroutine(ShakeCrystalRoutine(shakeTime));
            }
        }
        else
        {
            elapsedTime += Time.deltaTime;

            RaycastHit2D hit = Physics2D.CircleCast(transform.position, explosionRadius, Vector2.zero, 0, LayerMask.GetMask(PlayManager.PLAYER_TAG));
            CheckPlayer(hit);

            if (elapsedTime > explosionTime)
            {
                DisableCrystal();
            }
        }
        
    }
    private IEnumerator ShakeCrystalRoutine(float time)
    {
        float shakeTotalTime = 0;
        float shakeOneSideTime = 0;
        int shakeDirection = 1;

        Quaternion originRotation = transform.rotation;
        Vector3 originEulerAngle = transform.rotation.eulerAngles;
        Quaternion leftAngle = Quaternion.Euler(originEulerAngle.x, originEulerAngle.y, originEulerAngle.z - shakeAngle);
        Quaternion rightAngle = Quaternion.Euler(originEulerAngle.x, originEulerAngle.y, originEulerAngle.z + shakeAngle);
        while (true)
        {
            shakeTotalTime += Time.deltaTime;
            shakeOneSideTime += Time.deltaTime / shakeOneSideDuration * shakeDirection;
            transform.rotation = Quaternion.Lerp(leftAngle, rightAngle, shakeOneSideTime);
            if (shakeTotalTime > time)
            {
                transform.rotation = originRotation;
                break;
            }
            else if (shakeOneSideTime > 1 || shakeOneSideTime < 0)
            {
                shakeDirection *= -1;
            }
            yield return null;
        }
        shakeCoroutine = null;
    }
    private void CheckPlayer(RaycastHit2D hit)
    {
        if (!ReferenceEquals(hit.collider, null) && hit.collider.CompareTag(PlayManager.PLAYER_TAG))
        {
            hit.collider.gameObject.GetComponent<IAttack>().Hit(explosionDamage, explosionDamage
                , transform.position - hit.transform.position, this);
        }
    }

    public PatternCrystal SettingFirst(eActivableColor color, int cryDmg, int expDmg, float speed, float radius, float delay, float time, float shakeDuration)
    {
        crystalDamage = cryDmg;
        explosionDamage = expDmg;
        moveSpeed = speed;
        explosionRadius = radius;
        postExplosionDelay = delay;
        explosionTime = time;
        patternColor = color;
        shakeTime = shakeDuration;
        return this;
    }

    public void Shot(Vector2 dir)
    {
        moveDirection = dir;
        isShot = true;
        isEnable = true;
        elapsedTime = 0f;
    }

    private void DisableCrystal()
    {
        isEnable = false;
        afterExplosionEvent?.Invoke();
        afterExplosionEvent.RemoveAllListeners();
        gameObject.SetActive(false);
    }
    public bool CanParryAttack()
    {
        return PlayManager.Instance.ContainsActivationColors(patternColor) && isShot;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayManager.PLAYER_TAG))
        {
            collision.gameObject.GetComponent<IAttack>().Hit(crystalDamage, crystalDamage
                , transform.position - collision.transform.position, this);
        }

        if (collision.CompareTag(PlayManager.FLOOR_TAG))
        {
            isShot = false;
            isTouchFloor = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (isExplosion)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
