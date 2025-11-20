using UnityEngine;

namespace DynamicFogAndMist;

[ExecuteInEditMode]
[HelpURL("http://kronnect.com/taptapgo")]
public class DynamicFogManager : MonoBehaviour
{
	[Range(0f, 1f)]
	public float alpha = 1f;

	[Range(0f, 1f)]
	public float noiseStrength = 0.5f;

	[Range(0f, 0.999f)]
	public float distance = 0.2f;

	[Range(0f, 2f)]
	public float distanceFallOff = 1f;

	[Range(0f, 500f)]
	public float height = 1f;

	[Range(0f, 1f)]
	public float heightFallOff = 1f;

	public float baselineHeight;

	public Color color = new Color(0.89f, 0.89f, 0.89f, 1f);

	public GameObject sun;

	private Light sunLight;

	private Vector3 sunDirection = Vector3.zero;

	private Color sunColor = Color.white;

	private float sunIntensity = 1f;

	private void OnEnable()
	{
		UpdateMaterialProperties();
	}

	private void Reset()
	{
		UpdateMaterialProperties();
	}

	private void Update()
	{
		if (sun != null)
		{
			bool flag = false;
			if (sun.transform.forward != sunDirection)
			{
				flag = true;
			}
			if (sunLight != null && (sunLight.color != sunColor || sunLight.intensity != sunIntensity))
			{
				flag = true;
			}
			if (flag)
			{
				UpdateFogColor();
			}
		}
		UpdateFogData();
	}

	public void UpdateMaterialProperties()
	{
		UpdateFogData();
		UpdateFogColor();
	}

	private void UpdateFogData()
	{
		Vector4 value = new Vector4(height + 0.001f, baselineHeight, Camera.main.farClipPlane * distance, heightFallOff);
		Shader.SetGlobalVector("_FogData", value);
		Shader.SetGlobalFloat("_FogData2", distanceFallOff * value.z + 0.0001f);
	}

	private void UpdateFogColor()
	{
		if (sun != null)
		{
			if (sunLight == null)
			{
				sunLight = sun.GetComponent<Light>();
			}
			if (sunLight != null && sunLight.transform != sun.transform)
			{
				sunLight = sun.GetComponent<Light>();
			}
			sunDirection = sun.transform.forward;
			if (sunLight != null)
			{
				sunColor = sunLight.color;
				sunIntensity = sunLight.intensity;
			}
		}
		float num = sunIntensity * Mathf.Clamp01(1f - sunDirection.y);
		Color value = color * sunColor * num;
		value.a = alpha;
		Shader.SetGlobalColor("_FogColor", value);
	}
}
