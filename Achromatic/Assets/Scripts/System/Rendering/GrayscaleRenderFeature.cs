using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GrayscaleRenderFeature : ScriptableRendererFeature
{
    private GrayscaleRenderPass renderPass = null;

    public override void Create()
    {
        renderPass = new GrayscaleRenderPass("GrayscaleRenderPass", RenderPassEvent.AfterRenderingPostProcessing);
    }
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        renderPass.Setup(renderer.cameraColorTarget);
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {;
        renderer.EnqueuePass(renderPass);
    }
}
