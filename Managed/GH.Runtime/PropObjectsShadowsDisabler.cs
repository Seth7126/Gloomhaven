#define ENABLE_LOGS
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[RequireComponent(typeof(DetailsDisabler))]
public class PropObjectsShadowsDisabler : MonoDisableProviderBase
{
	[FormerlySerializedAs("_meshRenderers")]
	[SerializeField]
	private List<Renderer> _renderers;

	public override void StartDisable()
	{
		if (PlatformLayer.Setting == null)
		{
			Debug.LogWarning("Platform layer is null! It shouldn't be null!");
			return;
		}
		foreach (Renderer renderer in _renderers)
		{
			HandleRendererDisabling(renderer);
		}
	}

	protected virtual void HandleRendererDisabling(Renderer rendererComponent)
	{
		if (!(rendererComponent == null) && rendererComponent.shadowCastingMode != ShadowCastingMode.Off)
		{
			ShadowCastingMode shadowCastingMode = (PlatformLayer.Setting.GraphicPlatformSettings._castPropObjectsShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
			rendererComponent.shadowCastingMode = shadowCastingMode;
		}
	}

	[ContextMenu("Get all renderers")]
	private void GetAllRenderers()
	{
		GetRendererComponentsInternal(ref _renderers);
	}

	private void GetRendererComponentsInternal(ref List<Renderer> list)
	{
		if (list == null)
		{
			list = new List<Renderer>();
		}
		else
		{
			list.Clear();
		}
		List<MeshRenderer> list2 = new List<MeshRenderer>();
		GetComponentsInChildren(includeInactive: true, list2);
		list.AddRange(list2);
		List<SkinnedMeshRenderer> list3 = new List<SkinnedMeshRenderer>();
		GetComponentsInChildren(includeInactive: true, list3);
		list.AddRange(list3);
	}
}
