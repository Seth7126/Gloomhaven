using System;
using SRDebugger.Profiler;
using SRF.Service;
using UnityEngine.Rendering;

namespace SRDebugger.Services;

public static class ProfilerServiceSelector
{
	[ServiceSelector(typeof(IProfilerService))]
	public static Type GetProfilerServiceType()
	{
		if (GraphicsSettings.renderPipelineAsset != null)
		{
			return typeof(SRPProfilerService);
		}
		return typeof(ProfilerServiceImpl);
	}
}
