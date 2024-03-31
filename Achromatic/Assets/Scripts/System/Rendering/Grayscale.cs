using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom Post-processing/Grayscale", typeof(UniversalRenderPipeline))]
public class Grayscale : VolumeComponent, IPostProcessComponent
{
    private const string SHADER_NAME = "Scripts/Shaders/Grayscale";


    private Material material;

    public BoolParameter IsEnable = new BoolParameter(false);
    

    public bool IsActive()
    {
        return default;
    }

    public bool IsTileCompatible()
    {
        return default;
    }
}
