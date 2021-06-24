using Summer.Debugger.Window.Model;
using Spring.Util;
using UnityEngine;

namespace Summer.Debugger.Window
{
    public sealed class GraphicsInformationWindow : ScrollableDebuggerWindowBase
    {
        protected override void OnDrawScrollableWindow()
        {
            GUILayout.Label("<b>Graphics Information</b>");
            GUILayout.BeginVertical("box");
            {
                DrawItem("Device ID", SystemInfo.graphicsDeviceID.ToString());
                DrawItem("Device Name", SystemInfo.graphicsDeviceName);
                DrawItem("Device Vendor ID", SystemInfo.graphicsDeviceVendorID.ToString());
                DrawItem("Device Vendor", SystemInfo.graphicsDeviceVendor);
                DrawItem("Device Type", SystemInfo.graphicsDeviceType.ToString());
                DrawItem("Device Version", SystemInfo.graphicsDeviceVersion);
                DrawItem("Memory Size", StringUtils.Format("{} MB", SystemInfo.graphicsMemorySize.ToString()));
                DrawItem("Multi Threaded", SystemInfo.graphicsMultiThreaded.ToString());
                DrawItem("Shader Level", GetShaderLevelString(SystemInfo.graphicsShaderLevel));
                DrawItem("Global Maximum LOD", Shader.globalMaximumLOD.ToString());
                DrawItem("Global Render Pipeline", Shader.globalRenderPipeline);
                DrawItem("Active Tier", Graphics.activeTier.ToString());
                DrawItem("Active Color Gamut", Graphics.activeColorGamut.ToString());
                DrawItem("Preserve Frame Buffer Alpha", Graphics.preserveFramebufferAlpha.ToString());
                DrawItem("NPOT Support", SystemInfo.npotSupport.ToString());
                DrawItem("Max Texture Size", SystemInfo.maxTextureSize.ToString());
                DrawItem("Supported Render Target Count", SystemInfo.supportedRenderTargetCount.ToString());
                DrawItem("Copy Texture Support", SystemInfo.copyTextureSupport.ToString());
                DrawItem("Uses Reversed ZBuffer", SystemInfo.usesReversedZBuffer.ToString());
                DrawItem("Max Cubemap Size", SystemInfo.maxCubemapSize.ToString());
                DrawItem("Graphics UV Starts At Top", SystemInfo.graphicsUVStartsAtTop.ToString());
                DrawItem("Min Constant Buffer Offset Alignment", SystemInfo.minConstantBufferOffsetAlignment.ToString());
                DrawItem("Has Hidden Surface Removal On GPU", SystemInfo.hasHiddenSurfaceRemovalOnGPU.ToString());
                DrawItem("Has Dynamic Uniform Array Indexing In Fragment Shaders", SystemInfo.hasDynamicUniformArrayIndexingInFragmentShaders.ToString());
                DrawItem("Has Mip Max Level", SystemInfo.hasMipMaxLevel.ToString());
                DrawItem("Supports Sparse Textures", SystemInfo.supportsSparseTextures.ToString());
                DrawItem("Supports 3D Textures", SystemInfo.supports3DTextures.ToString());
                DrawItem("Supports Shadows", SystemInfo.supportsShadows.ToString());
                DrawItem("Supports Raw Shadow Depth Sampling", SystemInfo.supportsRawShadowDepthSampling.ToString());
                DrawItem("Supports Compute Shader", SystemInfo.supportsComputeShaders.ToString());
                DrawItem("Supports Instancing", SystemInfo.supportsInstancing.ToString());
                DrawItem("Supports 2D Array Textures", SystemInfo.supports2DArrayTextures.ToString());
                DrawItem("Supports Motion Vectors", SystemInfo.supportsMotionVectors.ToString());
                DrawItem("Supports Cubemap Array Textures", SystemInfo.supportsCubemapArrayTextures.ToString());
                DrawItem("Supports 3D Render Textures", SystemInfo.supports3DRenderTextures.ToString());
                DrawItem("Supports Texture Wrap Mirror Once", SystemInfo.supportsTextureWrapMirrorOnce.ToString());
                DrawItem("Supports Graphics Fence", SystemInfo.supportsGraphicsFence.ToString());
                DrawItem("Supports Async Compute", SystemInfo.supportsAsyncCompute.ToString());
                DrawItem("Supports Multisampled Textures", SystemInfo.supportsMultisampledTextures.ToString());
                DrawItem("Supports Async GPU Readback", SystemInfo.supportsAsyncGPUReadback.ToString());
                DrawItem("Supports 32bits Index Buffer", SystemInfo.supports32bitsIndexBuffer.ToString());
                DrawItem("Supports Hardware Quad Topology", SystemInfo.supportsHardwareQuadTopology.ToString());
                DrawItem("Supports Mip Streaming", SystemInfo.supportsMipStreaming.ToString());
                DrawItem("Supports Multisample Auto Resolve", SystemInfo.supportsMultisampleAutoResolve.ToString());
                DrawItem("Supports Separated Render Targets Blend", SystemInfo.supportsSeparatedRenderTargetsBlend.ToString());
                DrawItem("Supports Set Constant Buffer", SystemInfo.supportsSetConstantBuffer.ToString());
            }
            GUILayout.EndVertical();
        }

        private string GetShaderLevelString(int shaderLevel)
        {
            return StringUtils.Format("Shader Model {}.{}", (shaderLevel / 10).ToString(), (shaderLevel % 10).ToString());
        }
    }
}