using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private Shader grayscaleShader;

    private CameraController controller;

    private Vector4 activationColor = Vector3.zero;
    private Material grayscaleMaterial;

    private void Awake()
    {
        controller = GetComponent<CameraController>();
    }
    private void Start()
    {
        activationColor.w = 1;
        grayscaleMaterial = new Material(grayscaleShader);
        grayscaleMaterial.SetVector("_Color", activationColor);
    }

    public void ShakeCamera(float shakeTime)
    {
        controller.ShakeCamera(shakeTime);
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
