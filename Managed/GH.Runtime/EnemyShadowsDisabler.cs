using UnityEngine;
using UnityEngine.Rendering;

public sealed class EnemyShadowsDisabler : CharacterShadowsDisabler
{
	protected override void HandleCharacterSkinMeshRendererDisabling(SkinnedMeshRenderer skinnedMeshRenderer)
	{
		if (!(skinnedMeshRenderer == null) && skinnedMeshRenderer.shadowCastingMode != ShadowCastingMode.Off)
		{
			ShadowCastingMode shadowCastingMode = (PlatformLayer.Setting.GraphicPlatformSettings._castEnemyShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
			skinnedMeshRenderer.shadowCastingMode = shadowCastingMode;
		}
	}

	protected override void HandleCharacterMeshRendererDisabling(MeshRenderer meshRenderer)
	{
		if (!(meshRenderer == null) && meshRenderer.shadowCastingMode != ShadowCastingMode.Off)
		{
			ShadowCastingMode shadowCastingMode = (PlatformLayer.Setting.GraphicPlatformSettings._castEnemyShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
			meshRenderer.shadowCastingMode = shadowCastingMode;
		}
	}
}
