using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
    private float fallYPanDuration = 0.35f;
    public float fallSpeedYDampingChangeThreshold = -3f;

    [Header("Change Room"), Space(10)]
    [SerializeField]
    private float changeFadeTime = 0.5f;
    [SerializeField]
    private float changeDelayTime = 0.5f;

    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private Coroutine lerpYPanCoroutine;
    private Coroutine panCameraCorountine;
    private Coroutine fadeCameraCoroutine;

    private float deadZoneWidth;
    private float deadZoneHeight;
    private float softZoneWidth;
    private float softZoneHeight;

    private float normYPanAmount;
    private Vector2 startingTrackedObjectOffset;

    private float deadZoneSoftZoneLimit = 2;

    private bool isShake = false;
    private bool isChangeFOV = false;

    private GameObject parent;

    private CinemachineVirtualCamera cinemachine; 
    private CinemachineImpulseSource cinemachineNoise;
    private CinemachineVirtualCamera currentCamera;
    private CinemachineFramingTransposer framingTransposer;
    private CinemachineConfiner2D confiner;
    private CinemachineStoryboard cameraFade;
    protected override void OnAwake()
    {
        parent = transform.parent.transform.parent.gameObject;
        cinemachine = parent.GetComponentInChildren<CinemachineVirtualCamera>();

        for (int i = 0; i < allVirtualCameras.Length; i++)
        {
            if (allVirtualCameras[i].enabled)
            {
                currentCamera = allVirtualCameras[i];

                framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                cinemachineNoise = currentCamera.GetComponent<CinemachineImpulseSource>();
                confiner = currentCamera.GetComponent<CinemachineConfiner2D>();
                cameraFade = currentCamera.GetComponent<CinemachineStoryboard>();
            }
        }

        normYPanAmount = framingTransposer.m_YDamping;

        startingTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;
    }
    private void Start()
    {

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
    public void ShakeCamera()
    {
        if (!isShake)
        {
            StartCoroutine(ShakeSequence());
        }
    }
    IEnumerator ShakeSequence()
    {
        isShake = true;
        cinemachineNoise.m_ImpulseDefinition.m_AmplitudeGain = shakeAmplitude;
        cinemachineNoise.m_ImpulseDefinition.m_FrequencyGain = shakeFrequency;
        cinemachineNoise.GenerateImpulse();
        isShake = false;
        yield return null;
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
        while(elapsedTime < fallYPanDuration)
        {
            elapsedTime += Time.deltaTime;

            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, (elapsedTime / fallYPanDuration));
            framingTransposer.m_YDamping = lerpedPanAmount;

            yield return null;
        }

        IsLerpingYDamping = false;
    }
    #endregion

    #region Swap Cameras

    public void SwapCamera(CinemachineVirtualCamera cameraFromLeft, CinemachineVirtualCamera cameraFromRight, Vector2 triggerExitDirection)
    {
        if(currentCamera == cameraFromLeft && triggerExitDirection.x > 0f)
        {
            cameraFromRight.enabled = true;
            cameraFromLeft.enabled = false;
            currentCamera = cameraFromRight;
            framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        else if(currentCamera == cameraFromRight && triggerExitDirection.x < 0f)
        {
            cameraFromLeft.enabled = true;
            cameraFromRight.enabled = false;
            currentCamera = cameraFromRight;
            framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
    }

    #endregion

    #region Pan Camera
    public void PanCameraOnContact(float panDistance, float panTime, ePanDirection panDirection, bool panToStartingPos)
    {
        panCameraCorountine = StartCoroutine(PanCamera(panDistance, panTime, panDirection, panToStartingPos));
    }

    private IEnumerator PanCamera(float panDistance, float panTime, ePanDirection panDirection, bool panToStartingPos)
    {
        Vector2 endPos = Vector2.zero;
        Vector2 startPos = Vector2.zero;

        if(!panToStartingPos)
        {
            switch (panDirection)
            {
                case ePanDirection.UP:
                    endPos = Vector2.up;    
                    break;
                case ePanDirection.DOWN:
                    endPos = Vector2.down;
                    break;
                case ePanDirection.LEFT:
                    endPos = Vector2.right;
                    break;
                case ePanDirection.RIGHT:
                    endPos = Vector2.left;
                    break;
                default:
                    break;
            }
            endPos *= panDistance;

            startPos = startingTrackedObjectOffset;

            endPos += startPos;
        }
        else
        {
            startPos = framingTransposer.m_TrackedObjectOffset;
            endPos = startingTrackedObjectOffset;
        }

        float elapsedTime = 0f;
        while(elapsedTime < panTime)
        {
            elapsedTime += Time.deltaTime;

            Vector3 panLerp = Vector3.Lerp(startPos, endPos, (elapsedTime / panTime));
            framingTransposer.m_TrackedObjectOffset = panLerp;
            yield return null;
        }
    }

    #endregion

    #region Fade

    public void SwitchBoundLine(Collider2D collLD, Collider2D collRU, Vector2 exitDirection, eTwoDirection dir)
    {
        bool isNewCollOnUp = false;
        Collider2D oldColl = default;
        Collider2D newColl = default;

        switch (dir)
        {
            case eTwoDirection.HORIZONTAL:
                if(exitDirection.x > 0)
                {
                    oldColl = collLD;
                    newColl = collRU;
                }
                else
                {
                    oldColl = collRU;
                    newColl = collLD;
                }
                break;
            case eTwoDirection.VERTICAL:
                if(exitDirection.y < 0)
                {
                    oldColl = collRU;
                    newColl = collLD;
                }
                else
                {
                    oldColl = collLD;
                    newColl = collRU;
                    isNewCollOnUp = true;
                }
                break;
            default:
                break;
        }
        fadeCameraCoroutine = StartCoroutine(FadeSequence(changeFadeTime, changeDelayTime, 
            () =>
            {
                confiner.m_BoundingShape2D = newColl;
                if (isNewCollOnUp)
                {
                    PlayManager.Instance.GetPlayer.RigidbodyComp.AddForce(new Vector2(1, 1) * 30, ForceMode2D.Impulse);
                }
            }));
    }

    public void CameraFade(float fadeTime, float fadeDelay, UnityAction action)
    {
        fadeCameraCoroutine = StartCoroutine(FadeSequence(fadeTime, fadeDelay, action));
    }

    IEnumerator FadeSequence(float fadeTime, float fadeDelay, UnityAction action)
    {
        InputManager.Instance.CanInput = false;

        float elapsedTime = 0;
        float lerp = 0;

        while (true)
        {
            elapsedTime += Time.deltaTime / fadeTime;
            cameraFade.m_Alpha = elapsedTime;
            if (elapsedTime > 1)
            {
                break;
            }

            yield return null;
        }
        elapsedTime = 0;
        action?.Invoke();
        yield return Yields.WaitSeconds(fadeDelay);
        InputManager.Instance.CanInput = true;
        while (true)
        {
            elapsedTime += Time.deltaTime / fadeTime;
            cameraFade.m_Alpha = lerp;
            if (elapsedTime > 1)
            {
                break;
            }

            yield return null;
        }

    }

    #endregion

    #region Lock Position
    public void LockPosition(eTwoDirection lockDir, bool isLock)
    {
        switch(lockDir)
        {
            case eTwoDirection.HORIZONTAL:
                if (isLock)
                {
                    deadZoneWidth = framingTransposer.m_DeadZoneWidth;
                    softZoneWidth = framingTransposer.m_SoftZoneWidth;
                    framingTransposer.m_DeadZoneWidth = deadZoneSoftZoneLimit;
                    framingTransposer.m_SoftZoneWidth = deadZoneSoftZoneLimit;
                }
                else
                {
                    framingTransposer.m_DeadZoneWidth = deadZoneWidth;
                    framingTransposer.m_SoftZoneWidth = softZoneWidth;
                }
                break;
            case eTwoDirection.VERTICAL:
                if (isLock)
                {
                    deadZoneHeight = framingTransposer.m_DeadZoneHeight;
                    softZoneHeight = framingTransposer.m_SoftZoneHeight;
                    framingTransposer.m_DeadZoneHeight = deadZoneSoftZoneLimit;
                    framingTransposer.m_SoftZoneHeight = deadZoneSoftZoneLimit;
                }
                else
                {
                    framingTransposer.m_DeadZoneHeight = deadZoneHeight;
                    framingTransposer.m_SoftZoneHeight = softZoneHeight;
                }
                break;
            default:
                break;
        }
    }

    #endregion
}
