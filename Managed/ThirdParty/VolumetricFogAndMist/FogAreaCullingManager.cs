using UnityEngine;

namespace VolumetricFogAndMist;

[AddComponentMenu("")]
public class FogAreaCullingManager : MonoBehaviour
{
	public VolumetricFog fog;

	private void OnEnable()
	{
		if (fog == null)
		{
			fog = GetComponent<VolumetricFog>();
			if (fog == null)
			{
				fog = base.gameObject.AddComponent<VolumetricFog>();
			}
		}
	}

	private void OnBecameVisible()
	{
		if (fog != null)
		{
			fog.enabled = true;
		}
	}

	private void OnBecameInvisible()
	{
		if (fog != null)
		{
			fog.enabled = false;
		}
	}
}
