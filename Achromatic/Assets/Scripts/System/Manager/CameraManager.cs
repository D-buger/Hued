using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private float shakeAmplitude = 1.2f;
    [SerializeField]
    private float shakeFrequency = 2.0f;

    private Vector3 originPos;
    private bool isShake = false;
    private bool isChangeFOV = false;

    private GameObject parent;

    private CinemachineVirtualCamera cinemachine;
    private CinemachineBasicMultiChannelPerlin cinemachineNoise;

    private void Awake()
    {
        parent = transform.parent.gameObject;
        cinemachine = parent.GetComponentInChildren<CinemachineVirtualCamera>();
        if(null != cinemachine)
        {
            cinemachineNoise = cinemachine.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
        }
    }
    private void Start()
    {
        originPos = transform.position;
    }

    public void ChangeFOV(float amount, float time)
    {
        if (!isChangeFOV)
        {
            StartCoroutine(FOVSequence(amount, time));
        }
    }

    IEnumerator FOVSequence(float amount, float time)
    {
        isChangeFOV = true;
        yield return Yields.WaitSeconds(time);
        isChangeFOV = false;
    }

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
}
