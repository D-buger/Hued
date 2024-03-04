using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private Shader grayscaleShader;
    [SerializeField]
    private float shakeAmount = 0.1f;

    private Vector4 activationColor = Vector3.zero;
    private Material grayscaleMaterial;
    private Vector3 originPos;
    private bool isShake = false;
    private void Awake()
    {
        originPos = transform.position;    
    }
    private void Start()
    {
        activationColor.w = 1;
        grayscaleMaterial = new Material(grayscaleShader);
        grayscaleMaterial.SetVector("_Color", activationColor);
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

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, grayscaleMaterial);
    }
}
