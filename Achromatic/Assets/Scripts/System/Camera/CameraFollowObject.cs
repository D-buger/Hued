using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{
    private Player player;

    [SerializeField]
    private float flipYRotationTime = 0.1f;

    [SerializeField]
    private Vector2 offset = Vector2.zero;

    private Coroutine turnCoroutine;

    private bool isFacingRight;

    private void Awake()
    {
        
    }

    private void Start()
    {
        player = PlayManager.Instance.GetPlayer;
        player.CameraObject = this;
    }
    private void Update()
    {
        transform.position = (Vector2)player.transform.position + offset;
    }

    public void CallTurn()
    {
        turnCoroutine = StartCoroutine(FlipYLerp());
    }

    private IEnumerator FlipYLerp()
    {
        float startRotation = transform.localEulerAngles.y;
        float endRotationAmount = DetermineEndRotation();
        float yRotation = 0f;

        float elapsedTime = 0f;
        while (elapsedTime < flipYRotationTime)
        {
            elapsedTime += Time.deltaTime;

            yRotation = Mathf.Lerp(startRotation, endRotationAmount, (elapsedTime / flipYRotationTime));
            transform.rotation = Quaternion.Euler(0, yRotation, 0);

            yield return null;
        }
    }

    private float DetermineEndRotation()
    {
        isFacingRight = !isFacingRight;

        if (isFacingRight)
        {
            return 180f;
        }
        else
        {
            return 0f;
        }
    }
}
