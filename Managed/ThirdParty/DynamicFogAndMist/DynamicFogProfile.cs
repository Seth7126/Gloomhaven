using UnityEngine;

namespace DynamicFogAndMist;

[CreateAssetMenu(fileName = "DynamicFogProfile", menuName = "Dynamic Fog Profile", order = 100)]
public class DynamicFogProfile : ScriptableObject
{
	public FOG_TYPE effectType = FOG_TYPE.DesktopFogPlusWithSkyHaze;

	public bool enableDithering;

	[Range(0f, 0.2f)]
	public float ditherStrength = 0.03f;

	[Range(0f, 1f)]
	public float alpha = 1f;

	[Range(0f, 1f)]
	public float noiseStrength = 0.5f;

	[Range(0.01f, 1f)]
	public float noiseScale = 0.1f;

	[Range(0f, 0.999f)]
	public float distance = 0.1f;

	[Range(0.0001f, 2f)]
	public float distanceFallOff = 0.01f;

	[Range(0f, 1.2f)]
	public float maxDistance = 0.999f;

	[Range(0.0001f, 0.5f)]
	public float maxDistanceFallOff;

	[Range(0f, 500f)]
	public float height = 1f;

	[Range(0f, 500f)]
	public float maxHeight = 100f;

	[Range(0.0001f, 1f)]
	public float heightFallOff = 0.1f;

	public float baselineHeight;

	public bool clipUnderBaseline;

	[Range(0f, 15f)]
	public float turbulence = 0.1f;

	[Range(0f, 5f)]
	public float speed = 0.1f;

	public Vector3 windDirection = new Vector3(1f, 0f, 1f);

	public Color color = Color.white;

	public Color color2 = Color.gray;

	[Range(0f, 500f)]
	public float skyHaze = 50f;

	[Range(0f, 1f)]
	public float skySpeed = 0.3f;

	[Range(0f, 1f)]
	public float skyNoiseStrength = 0.1f;

	[Range(0f, 1f)]
	public float skyAlpha = 1f;

	public bool useXZDistance;

	[Range(0f, 1f)]
	public float scattering = 0.7f;

	public Color scatteringColor = new Color(1f, 1f, 0.8f);

	public void Load(DynamicFogBase fog)
	{
		fog.preset = FOG_PRESET.Custom;
		fog.effectType = effectType;
		fog.enableDithering = enableDithering;
		fog.ditherStrength = ditherStrength;
		fog.alpha = alpha;
		fog.noiseStrength = noiseStrength;
		fog.noiseScale = noiseScale;
		fog.distance = distance;
		fog.distanceFallOff = distanceFallOff;
		fog.maxDistance = maxDistance;
		fog.maxDistanceFallOff = maxDistanceFallOff;
		fog.height = height;
		fog.maxHeight = maxHeight;
		fog.heightFallOff = heightFallOff;
		fog.baselineHeight = baselineHeight;
		fog.clipUnderBaseline = clipUnderBaseline;
		fog.turbulence = turbulence;
		fog.speed = speed;
		fog.windDirection = windDirection;
		fog.color = color;
		fog.color2 = color2;
		fog.skyHaze = skyHaze;
		fog.skySpeed = skySpeed;
		fog.skyNoiseStrength = skyNoiseStrength;
		fog.skyAlpha = skyAlpha;
		fog.useXZDistance = useXZDistance;
		fog.scattering = scattering;
		fog.scatteringColor = scatteringColor;
	}

	public void Save(DynamicFogBase fog)
	{
		effectType = fog.effectType;
		enableDithering = fog.enableDithering;
		ditherStrength = fog.ditherStrength;
		alpha = fog.alpha;
		noiseStrength = fog.noiseStrength;
		noiseScale = fog.noiseScale;
		distance = fog.distance;
		distanceFallOff = fog.distanceFallOff;
		maxDistance = fog.maxDistance;
		maxDistanceFallOff = fog.maxDistanceFallOff;
		height = fog.height;
		maxHeight = fog.maxHeight;
		heightFallOff = fog.heightFallOff;
		baselineHeight = fog.baselineHeight;
		clipUnderBaseline = fog.clipUnderBaseline;
		turbulence = fog.turbulence;
		speed = fog.speed;
		windDirection = fog.windDirection;
		color = fog.color;
		color2 = fog.color2;
		skyHaze = fog.skyHaze;
		skySpeed = fog.skySpeed;
		skyNoiseStrength = fog.skyNoiseStrength;
		skyAlpha = fog.skyAlpha;
		useXZDistance = fog.useXZDistance;
		scattering = fog.scattering;
		scatteringColor = fog.scatteringColor;
	}

	public static void Lerp(DynamicFogProfile profile1, DynamicFogProfile profile2, float t, DynamicFogBase fog)
	{
		if (t < 0f)
		{
			t = 0f;
		}
		else if (t > 1f)
		{
			t = 1f;
		}
		fog.enableDithering = ((t < 0.5f) ? profile1.enableDithering : profile2.enableDithering);
		fog.ditherStrength = profile1.ditherStrength * (1f - t) + profile2.ditherStrength * t;
		fog.alpha = profile1.alpha * (1f - t) + profile2.alpha * t;
		fog.noiseStrength = profile1.noiseStrength * (1f - t) + profile2.noiseStrength * t;
		fog.noiseScale = profile1.noiseScale * (1f - t) + profile2.noiseScale * t;
		fog.distance = profile1.distance * (1f - t) + profile2.distance * t;
		fog.distanceFallOff = profile1.distanceFallOff * (1f - t) + profile2.distanceFallOff * t;
		fog.maxDistance = profile1.maxDistance * (1f - t) + profile2.maxDistance * t;
		fog.maxDistanceFallOff = profile1.maxDistanceFallOff * (1f - t) + profile2.maxDistanceFallOff * t;
		fog.height = profile1.height * (1f - t) + profile2.height * t;
		fog.maxHeight = profile1.maxHeight * (1f - t) + profile2.maxHeight * t;
		fog.heightFallOff = profile1.heightFallOff * (1f - t) + profile2.heightFallOff * t;
		fog.baselineHeight = profile1.baselineHeight * (1f - t) + profile2.baselineHeight * t;
		fog.clipUnderBaseline = ((t < 0.5f) ? profile1.clipUnderBaseline : profile2.clipUnderBaseline);
		fog.turbulence = profile1.turbulence * (1f - t) + profile2.turbulence * t;
		fog.speed = profile1.speed * (1f - t) + profile2.speed * t;
		fog.windDirection = profile1.windDirection * (1f - t) + profile2.windDirection * t;
		fog.color = profile1.color * (1f - t) + profile2.color * t;
		fog.color2 = profile1.color2 * (1f - t) + profile2.color2 * t;
		fog.skyHaze = profile1.skyHaze * (1f - t) + profile2.skyHaze * t;
		fog.skySpeed = profile1.skySpeed * (1f - t) + profile2.skySpeed * t;
		fog.skyNoiseStrength = profile1.skyNoiseStrength * (1f - t) + profile2.skyNoiseStrength * t;
		fog.skyAlpha = profile1.skyAlpha * (1f - t) + profile2.skyAlpha * t;
		fog.useXZDistance = ((t < 0.5f) ? profile1.useXZDistance : profile2.useXZDistance);
		fog.scattering = profile1.scattering * (1f - t) + profile2.scattering * t;
		fog.scatteringColor = profile1.scatteringColor * (1f - t) + profile2.scatteringColor * t;
	}
}
