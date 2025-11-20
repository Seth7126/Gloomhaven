#define ENABLE_LOGS
using Chronos;
using UnityEngine;
using VolumetricFogAndMist;

public class VFX_UseWorldMask : MonoBehaviour
{
	private static readonly int _worldMask = Shader.PropertyToID("_WorldMask");

	private static readonly int _worldMaskScale = Shader.PropertyToID("_WorldMaskScale");

	public Texture2D worldMask;

	public Vector3 worldMaskScale;

	public VolumetricFog volumetricFogSource;

	public ParticleSystem[] maskingTargets;

	private ParticleSystemRenderer[] targetRenderers;

	private float updateTime;

	private void Start()
	{
		if ((bool)volumetricFogSource && maskingTargets[0] != null && (bool)maskingTargets[0])
		{
			worldMask = volumetricFogSource.fogOfWarTexture;
			worldMaskScale = volumetricFogSource.fogOfWarSize;
			targetRenderers = new ParticleSystemRenderer[maskingTargets.Length];
			Debug.Log(maskingTargets.Length + " " + targetRenderers.Length);
			for (int i = 0; i < maskingTargets.Length; i++)
			{
				if (maskingTargets[i] != null)
				{
					Debug.Log("ListenBearbeitung: " + i);
					targetRenderers[i] = maskingTargets[i].GetComponent<ParticleSystemRenderer>();
				}
			}
		}
		else
		{
			Debug.Log(base.gameObject?.ToString() + " is missing Volumetric Fog Source and/or Masking Targets");
		}
	}

	private void Update()
	{
		updateTime += Timekeeper.instance.m_GlobalClock.deltaTime;
		if (updateTime >= 1f)
		{
			updateTime = 0f;
			UpdateTargetRenderers();
		}
	}

	private void UpdateTargetRenderers()
	{
		if (targetRenderers == null)
		{
			return;
		}
		ParticleSystemRenderer[] array = targetRenderers;
		foreach (ParticleSystemRenderer particleSystemRenderer in array)
		{
			if (particleSystemRenderer != null)
			{
				particleSystemRenderer.material.SetTexture(_worldMask, worldMask);
				particleSystemRenderer.material.SetVector(_worldMaskScale, worldMaskScale);
			}
		}
	}
}
