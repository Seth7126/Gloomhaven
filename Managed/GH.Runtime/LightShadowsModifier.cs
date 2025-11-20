using JetBrains.Annotations;
using Script.Optimization;
using UnityEngine;

public class LightShadowsModifier : MonoBehaviour
{
	[UsedImplicitly]
	[SerializeField]
	private LightImportance _lightImportance;

	private Light _lightComponent;

	public LightImportance LightImportance => _lightImportance;

	private void Awake()
	{
		_lightComponent = GetComponent<Light>();
		ReinitShadows();
		if (Singleton<LightShadowsModifierController>.Instance != null)
		{
			Singleton<LightShadowsModifierController>.Instance.RegisterLightShadowModifier(this);
		}
	}

	public void DisableShadows()
	{
		_lightComponent.shadowBias = 0f;
		_lightComponent.shadows = LightShadows.None;
	}

	public void ReinitShadows()
	{
		_lightComponent.shadowBias = 0f;
		if (_lightComponent != null && _lightComponent.shadows != LightShadows.None && _lightImportance < PlatformLayer.Setting.GetLightImportance())
		{
			_lightComponent.shadows = LightShadows.None;
		}
	}

	private void OnDestroy()
	{
		if (Singleton<LightShadowsModifierController>.Instance != null)
		{
			Singleton<LightShadowsModifierController>.Instance.UnregisterLightShadowModifier(this);
		}
	}
}
