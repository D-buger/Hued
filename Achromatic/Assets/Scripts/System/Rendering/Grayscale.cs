using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom Post-processing/Grayscale", typeof(UniversalRenderPipeline))]
public class Grayscale : VolumeComponent, IPostProcessComponent
{
    private const string SHADER_NAME = "Unlit/Grayscale";
    private const string PROPERTY_COLOR = "_Color";


    private Material material;

    public BoolParameter isEnable = new BoolParameter(false);
    public Vector4Parameter activationColor = new Vector4Parameter(new Vector4(0f, 0f, 0f, 1f));

    public bool IsActive()
    {
        if(isEnable.value == false || !active || !material)
        {
            return false;
        }
        return true;
    }

    public bool IsTileCompatible()
    {
        return false;
    }

    public void Setup()
    {
        if (!material)
        {
            Shader shader = Shader.Find(SHADER_NAME);
            material = CoreUtils.CreateEngineMaterial(shader);
        }
    }

    public void Destroy()
    {
        if (material)
        {
            CoreUtils.Destroy(material);
            material = null;
        }
    }

    public void Render(CommandBuffer commandBuffer, ref RenderingData renderingData, RenderTargetIdentifier source, RenderTargetIdentifier destination)
    {
        if (!material)
        {
            return;
        }

        material.SetColor(PROPERTY_COLOR, activationColor.value);

        commandBuffer.Blit(source, destination, material);

    }
}
