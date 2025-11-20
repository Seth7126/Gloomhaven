using UnityEngine;
using VolumetricFogAndMist;

namespace LowGraphicEffects;

public class GraphicLevelController : MonoBehaviour
{
	public const int HightResolutionFog = 2048;

	public const int LowResolutionFog = 1024;

	[SerializeField]
	private MonoBehaviour[] _hightEffects;

	[SerializeField]
	private MonoBehaviour[] _lowEffects;

	[SerializeField]
	private MonoBehaviour[] _hightAntialiasEffects;

	[SerializeField]
	private GameObject[] _hightGraphicDetails;

	[SerializeField]
	private VolumetricFog _volumetricFog;

	private void Awake()
	{
		if (PlatformLayer.Setting.UseLowGraphicEffects)
		{
			DestroyEffects(_hightEffects);
			EnableEffects(_lowEffects);
			for (int i = 0; i < _hightGraphicDetails.Length; i++)
			{
				_hightGraphicDetails[i].SetActive(value: false);
			}
		}
		else
		{
			DestroyEffects(_lowEffects);
			EnableEffects(_hightEffects);
		}
		if (PlatformLayer.Setting.UseSimpleAntialiasTechnique && _hightAntialiasEffects != null)
		{
			DestroyEffects(_hightAntialiasEffects);
		}
		if (_volumetricFog != null)
		{
			_volumetricFog.fogOfWarTextureSize = Mathf.Clamp(_volumetricFog.fogOfWarTextureSize, 0, PlatformLayer.Setting.UseLowGraphicEffects ? 1024 : 2048);
		}
	}

	private void DestroyEffects(MonoBehaviour[] components)
	{
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i] != null)
			{
				Object.Destroy(components[i]);
			}
		}
	}

	private void EnableEffects(MonoBehaviour[] components)
	{
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i] != null)
			{
				components[i].enabled = true;
			}
		}
	}
}
