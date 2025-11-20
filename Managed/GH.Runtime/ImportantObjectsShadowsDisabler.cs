using UnityEngine;
using UnityEngine.Rendering;

public sealed class ImportantObjectsShadowsDisabler : PropObjectsShadowsDisabler
{
	protected override void HandleRendererDisabling(Renderer rendererComponent)
	{
		if (!(rendererComponent == null) && rendererComponent.shadowCastingMode != ShadowCastingMode.Off)
		{
			ShadowCastingMode shadowCastingMode = (PlatformLayer.Setting.GraphicPlatformSettings._castImportantObjectsShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
			rendererComponent.shadowCastingMode = shadowCastingMode;
		}
	}
}
