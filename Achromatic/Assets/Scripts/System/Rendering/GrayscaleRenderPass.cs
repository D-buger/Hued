using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrayscaleRenderPass : ScriptableRenderPass
{
    static readonly string RENDER_TAG = "grayscale";
    static readonly int TARGET_ID = Shader.PropertyToID("Grayscale");

    RenderTargetIdentifier currentTarget;
    Material blitRenderMaterial;

    public GrayscaleRenderPass(RenderPassEvent evnt, Material mat)
    {
        renderPassEvent = evnt;
        if (null == mat)
        {
            return;
        }

        blitRenderMaterial = mat;
    }

    public virtual void Setup(in RenderTargetIdentifier source)
    {
        currentTarget = source;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!renderingData.cameraData.postProcessEnabled)
        {
            return;
        }

        CommandBuffer commandBuffer = CommandBufferPool.Get();



        //임시 버퍼 생성
        //commandBuffer.Blit(currentTarget, );

        //Pass 렌더링


        context.ExecuteCommandBuffer(commandBuffer);
        CommandBufferPool.Release(commandBuffer);
    }

    private void Render(CommandBuffer cmd, ref RenderingData renderingData)
    {
        CameraData cameraData = renderingData.cameraData;
        RenderTargetIdentifier source = currentTarget;
        
    }
}
