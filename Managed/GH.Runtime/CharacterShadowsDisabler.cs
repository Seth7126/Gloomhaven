#define ENABLE_LOGS
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[RequireComponent(typeof(DetailsDisabler))]
public class CharacterShadowsDisabler : MonoDisableProviderBase
{
	[FormerlySerializedAs("_meshRenderers")]
	[SerializeField]
	private List<SkinnedMeshRenderer> _skinnedMeshRenderers;

	[SerializeField]
	private List<MeshRenderer> _otherRenderers;

	public override void StartDisable()
	{
		if (PlatformLayer.Setting == null)
		{
			Debug.LogWarning("Platform layer is null! It shouldn't be null!");
			return;
		}
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in _skinnedMeshRenderers)
		{
			HandleCharacterSkinMeshRendererDisabling(skinnedMeshRenderer);
		}
		foreach (MeshRenderer otherRenderer in _otherRenderers)
		{
			HandleCharacterMeshRendererDisabling(otherRenderer);
		}
	}

	protected virtual void HandleCharacterSkinMeshRendererDisabling(SkinnedMeshRenderer skinnedMeshRenderer)
	{
		if (!(skinnedMeshRenderer == null) && skinnedMeshRenderer.shadowCastingMode != ShadowCastingMode.Off)
		{
			ShadowCastingMode shadowCastingMode = (PlatformLayer.Setting.GraphicPlatformSettings._castPlayerCharactersShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
			skinnedMeshRenderer.shadowCastingMode = shadowCastingMode;
		}
	}

	protected virtual void HandleCharacterMeshRendererDisabling(MeshRenderer meshRenderer)
	{
		if (!(meshRenderer == null) && meshRenderer.shadowCastingMode != ShadowCastingMode.Off)
		{
			ShadowCastingMode shadowCastingMode = (PlatformLayer.Setting.GraphicPlatformSettings._castPlayerCharactersShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
			meshRenderer.shadowCastingMode = shadowCastingMode;
		}
	}

	[ContextMenu("Get all renderers")]
	private void GetAllRenderers()
	{
		GetComponentsInternal(ref _skinnedMeshRenderers);
		GetComponentsInternal(ref _otherRenderers);
	}

	private void GetComponentsInternal<T>(ref List<T> list)
	{
		if (list == null)
		{
			list = new List<T>();
		}
		else
		{
			list.Clear();
		}
		GetComponentsInChildren(includeInactive: true, list);
	}
}
