using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private Shader grayscaleShader;

    [SerializeField]
    private float shakeAmplitude = 1.2f;
    [SerializeField]
    private float shakeFrequency = 2.0f;

    private Vector3 originPos;
    private bool isShake = false;
    private bool isChangeFOV = false;

    private CinemachineVirtualCamera cinemachine;
    private CinemachineBasicMultiChannelPerlin cinemachineNoise;

    private Vector4 activationColor = Vector3.zero;
    private Material grayscaleMaterial;
    public Material GrayscaleMaterial 
    {
        get 
        {
            if (null == grayscaleMaterial)
            {
                grayscaleMaterial = new Material(grayscaleShader);
                grayscaleMaterial.SetVector("_Color", activationColor);
            }
            
            return grayscaleMaterial;
        }
    }

    private void Awake()
    {
        cinemachine = transform.GetComponentInChildren<CinemachineVirtualCamera>();
        if(null != cinemachine)
        {
            cinemachineNoise = cinemachine.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
        }
    }
    private void Start()
    {
        originPos = transform.position;
        activationColor.w = 1;
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

    public void SetColor(eActivableColor color)
    {
        switch(color) {
            case eActivableColor.RED:
                activationColor.x = 1;
                break;
            case eActivableColor.GREEN:
                activationColor.y = 1;
                break;
            case eActivableColor.BLUE:
                activationColor.z = 1;
                break;
            default:
                break;
        }
        grayscaleMaterial.SetVector("_Color", activationColor);
    }
}
