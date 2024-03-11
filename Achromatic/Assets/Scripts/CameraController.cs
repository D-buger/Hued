using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera camera;

    [SerializeField]
    private float cameraMoveSpeed = 1f;
    [SerializeField]
    private Vector2 cameraOffset;

    [SerializeField]
    private float shakeAmount = 0.1f;

    [SerializeField, Space(10)]
    private Vector3[] cameraAreaVertex;
    public Vector3[] GetVertex => cameraAreaVertex;

    private Transform targetTransform;
    public void SetTargetTransform(Transform trans) => targetTransform = trans;

    private Vector3 firstPosition;
    private Vector3 originPos;
    private bool isShake = false;

    // LeftTop, LeftBottom, RightTop, RightBottom
    private Vector2[] vertices = new Vector2[4];
    private float halfX;
    private float halfY;
    private enum eLimitLine : byte
    {
        NONE = 0,
        LEFT = 1,
        RIGHT = LEFT << 1,
        TOP = LEFT << 2,
        BOTTOM = LEFT << 3
    }
    private byte checkLimit;

    private void Awake()
    {
        camera = Camera.main;
    }

    private void Start()
    {
        originPos = transform.position;
        firstPosition = transform.position;
        halfY = camera.orthographicSize;
        halfX = camera.aspect / 2 * halfY * 2;
        targetTransform = GameObject.FindGameObjectWithTag(PlayManager.PLAYER_TAG).transform;
    }

    private void LateUpdate()
    {
        CalculateCameraEachPos();
        CheckLimitLine();
        CameraMove();
    }
    private void CameraMove()
    {
        if (null != targetTransform && !isShake)
        {
            transform.position = Vector3.Lerp(transform.position,
                targetTransform.position + firstPosition + new Vector3(cameraOffset.x, cameraOffset.y, 0),
                Time.deltaTime * cameraMoveSpeed);
        }
    }

    private void CheckLimitLine()
    {

    }

    private void CalculateCameraEachPos()
    {
        vertices[0].x = transform.position.x - halfX;
        vertices[0].y = transform.position.y + halfY;

        vertices[1].x = transform.position.x - halfX;
        vertices[1].y = transform.position.y - halfY;

        vertices[2].x = transform.position.x + halfX;
        vertices[2].y = transform.position.y + halfY;

        vertices[3].x = transform.position.x + halfX;
        vertices[3].y = transform.position.y - halfY;
    }

    public void ShakeCamera(float shakeTime)
    {
        if (!isShake)
        {
            originPos = transform.position;
            StartCoroutine(ShakeSequence(shakeTime));
        }
    }


    IEnumerator ShakeSequence(float shakeTime)
    {
        isShake = true;
        while (shakeTime > 0)
        {
            transform.position = Random.insideUnitSphere * shakeAmount + originPos;

            shakeTime -= Time.deltaTime;
            yield return null;
        }
        transform.position = originPos;
        isShake = false;
    }
}
