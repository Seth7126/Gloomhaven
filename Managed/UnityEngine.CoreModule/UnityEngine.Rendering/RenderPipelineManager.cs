using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering;

public static class RenderPipelineManager
{
	internal static RenderPipelineAsset s_CurrentPipelineAsset;

	private static List<Camera> s_Cameras = new List<Camera>();

	private static string s_currentPipelineType;

	private static string s_builtinPipelineName = "Built-in Pipeline";

	private static RenderPipeline s_currentPipeline = null;

	public static RenderPipeline currentPipeline
	{
		get
		{
			return s_currentPipeline;
		}
		private set
		{
			s_currentPipelineType = ((value != null) ? value.GetType().ToString() : s_builtinPipelineName);
			s_currentPipeline = value;
		}
	}

	public static event Action<ScriptableRenderContext, List<Camera>> beginContextRendering;

	public static event Action<ScriptableRenderContext, List<Camera>> endContextRendering;

	public static event Action<ScriptableRenderContext, Camera[]> beginFrameRendering;

	public static event Action<ScriptableRenderContext, Camera> beginCameraRendering;

	public static event Action<ScriptableRenderContext, Camera[]> endFrameRendering;

	public static event Action<ScriptableRenderContext, Camera> endCameraRendering;

	public static event Action activeRenderPipelineTypeChanged;

	internal static void BeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
	{
		RenderPipelineManager.beginFrameRendering?.Invoke(context, cameras.ToArray());
		RenderPipelineManager.beginContextRendering?.Invoke(context, cameras);
	}

	internal static void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		RenderPipelineManager.beginCameraRendering?.Invoke(context, camera);
	}

	internal static void EndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
	{
		RenderPipelineManager.endFrameRendering?.Invoke(context, cameras.ToArray());
		RenderPipelineManager.endContextRendering?.Invoke(context, cameras);
	}

	internal static void EndCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		RenderPipelineManager.endCameraRendering?.Invoke(context, camera);
	}

	[RequiredByNativeCode]
	internal static void OnActiveRenderPipelineTypeChanged()
	{
		RenderPipelineManager.activeRenderPipelineTypeChanged?.Invoke();
	}

	[RequiredByNativeCode]
	internal static void HandleRenderPipelineChange(RenderPipelineAsset pipelineAsset)
	{
		if ((object)s_CurrentPipelineAsset != pipelineAsset)
		{
			CleanupRenderPipeline();
			s_CurrentPipelineAsset = pipelineAsset;
		}
	}

	[RequiredByNativeCode]
	internal static void CleanupRenderPipeline()
	{
		if (currentPipeline != null && !currentPipeline.disposed)
		{
			currentPipeline.Dispose();
			s_CurrentPipelineAsset = null;
			currentPipeline = null;
			SupportedRenderingFeatures.active = new SupportedRenderingFeatures();
		}
	}

	[RequiredByNativeCode]
	private static string GetCurrentPipelineAssetType()
	{
		return s_currentPipelineType;
	}

	[RequiredByNativeCode]
	private static void DoRenderLoop_Internal(RenderPipelineAsset pipe, IntPtr loopPtr, List<Camera.RenderRequest> renderRequests)
	{
		PrepareRenderPipeline(pipe);
		if (currentPipeline != null)
		{
			ScriptableRenderContext context = new ScriptableRenderContext(loopPtr);
			s_Cameras.Clear();
			context.GetCameras(s_Cameras);
			if (renderRequests == null)
			{
				currentPipeline.InternalRender(context, s_Cameras);
			}
			else
			{
				currentPipeline.InternalRenderWithRequests(context, s_Cameras, renderRequests);
			}
			s_Cameras.Clear();
		}
	}

	internal static void PrepareRenderPipeline(RenderPipelineAsset pipelineAsset)
	{
		HandleRenderPipelineChange(pipelineAsset);
		if (s_CurrentPipelineAsset != null && (currentPipeline == null || currentPipeline.disposed))
		{
			currentPipeline = s_CurrentPipelineAsset.InternalCreatePipeline();
		}
	}
}
