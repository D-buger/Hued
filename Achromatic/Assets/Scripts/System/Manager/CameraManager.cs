using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : SingletonBehavior<CameraManager>
{
    [SerializeField]
    private CinemachineVirtualCamera[] allVirtualCameras;

    [Header("Shake"), Space(10)]
    [SerializeField]
    private float shakeAmplitude = 1.2f;
    [SerializeField]
    private float shakeFrequency = 2.0f;

    [Header("Fall"), Space(10)]
    [SerializeField]
    private float fallPanAmount = 0.25f;
    [SerializeField]
    private float fallYPanTime = 0.35f;
    public float fallSpeedYDampingChangeThreshold = -5f;

    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private float normYPanAmount;
    private Coroutine lerpYPanCoroutine;

    private Vector3 originPos;
    private bool isShake = false;
    private bool isChangeFOV = false;

    private GameObject parent;

    private CinemachineVirtualCamera cinemachine;
    private CinemachineBasicMultiChannelPerlin cinemachineNoise;
    private CinemachineVirtualCamera currentCamera;
    private CinemachineFramingTransposer framingTransposer;

    protected override void OnAwake()
    {
        parent = transform.parent.gameObject;
        cinemachine = parent.GetComponentInChildren<CinemachineVirtualCamera>();
        if (null != cinemachine)
        {
            cinemachineNoise = cinemachine.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
        }

        for (int i = 0; i < allVirtualCameras.Length; i++)
        {
            if (allVirtualCameras[i].enabled)
            {
                currentCamera = allVirtualCameras[i];

                framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }

        normYPanAmount = framingTransposer.m_YDamping;
    }
    private void Start()
    {
        originPos = transform.position;
    }

    public void ChangeFOV(float amount, float enterTime, float durationTime, float exitTime)
    {
        if (!isChangeFOV)
        {
            StartCoroutine(FOVSequence(amount, enterTime, durationTime, exitTime));
        }
    }

    IEnumerator FOVSequence(float amount, float enterTime, float durationTime, float exitTime)
    {
        isChangeFOV = true;
        yield return Yields.WaitSeconds(durationTime);
        isChangeFOV = false;
    }

    #region Shake Camera
    public void ShakeCamera(float shakeTime)
    {
        if (!isShake)
        {
            StartCoroutine(ShakeSequence(shakeTime));
        }
    }
    IEnumerator ShakeSequence(float shakeTime)
    {
        isShake = true;
        while (shakeTime > 0)
        {
            cinemachineNoise.m_AmplitudeGain = shakeAmplitude;
            cinemachineNoise.m_FrequencyGain = shakeFrequency;

            shakeTime -= Time.deltaTime;
            yield return null;
        }
        cinemachineNoise.m_AmplitudeGain = 0f;
        cinemachineNoise.m_FrequencyGain = 0f;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        isShake = false;
    }
    #endregion

    #region Lerp Y Damping
    public void LerpYDamping(bool isPlayerFalling)
    {
        lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }

    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        IsLerpingYDamping = true;

        float startDampAmount = framingTransposer.m_YDamping;
        float endDampAmount = 0f;

        if (isPlayerFalling)
        {
            endDampAmount = fallPanAmount;
            LerpedFromPlayerFalling = true;
        }
        else
        {
            endDampAmount = normYPanAmount;
        }

        float elapsedTime = 0f;
        while(elapsedTime < fallYPanTime)
        {
            elapsedTime += Time.deltaTime;

            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, (elapsedTime / fallYPanTime));
            framingTransposer.m_YDamping = lerpedPanAmount;

            yield return null;
        }

        IsLerpingYDamping = false;
    }
    #endregion

    #region Swap Cameras

    public void SwapCamera(CinemachineVirtualCamera cameraFromLeft)
    {

    }

    #endregion
}
