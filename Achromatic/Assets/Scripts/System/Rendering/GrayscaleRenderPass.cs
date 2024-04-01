using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrayscaleRenderPass : ScriptableRenderPass
{
    protected const string TEMP_BUFFER_NAME = "_TempColorBuffer";

    protected Grayscale component;
    protected string RenderTag { get; }

    private RenderTargetIdentifier source;
    private RenderTargetHandle tempTexture;

    public GrayscaleRenderPass(string renderTag, RenderPassEvent passEvent)
    {
        renderPassEvent = passEvent;
        RenderTag = renderTag;
    }

    public virtual void Setup(in RenderTargetIdentifier source)
    {
        this.source = source;
        tempTexture.Init(TEMP_BUFFER_NAME);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!renderingData.cameraData.postProcessEnabled)
        {
            return;
        }

        VolumeStack volumeStack = VolumeManager.instance.stack;
        component = volumeStack.GetComponent<Grayscale>();
        if (component)
        {
            component.Setup();
        }
        if(!component || !component.IsActive())
        {
            return;
        }

        CommandBuffer commandBuffer = CommandBufferPool.Get(RenderTag);
        RenderTargetIdentifier destination = tempTexture.Identifier();

        CameraData cameraData = renderingData.cameraData;
        RenderTextureDescriptor descriptor = new RenderTextureDescriptor(cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight);
        descriptor.colorFormat = cameraData.isHdrEnabled ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
        commandBuffer.GetTemporaryRT(tempTexture.id, descriptor);

        commandBuffer.Blit(source, destination);

        component.Render(commandBuffer, ref renderingData, destination, source);
        commandBuffer.ReleaseTemporaryRT(tempTexture.id);

        context.ExecuteCommandBuffer(commandBuffer);
        CommandBufferPool.Release(commandBuffer);
    }
}
