using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GrayscaleRenderFeature : ScriptableRendererFeature
{
    private GrayscaleRenderFeature renderPass = null;

    public override void Create()
    {
        //renderPass = new GrayscaleRenderPass(RenderPassEvent.AfterRenderingPostProcessing, "Grayscale");
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //renderPass.SetupRenderPasses(renderer.cameraColorTarget);
        //renderer.EnqueuePass(renderPass);
    }
}
