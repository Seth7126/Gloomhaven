#define ENABLE_LOGS
using System.Text;
using DynamicFogAndMist;
using UnityEngine;

[ExecuteInEditMode]
public class DynamicAmbience : MonoBehaviour
{
	[Header("Ambient Skylight")]
	[Range(0f, 8f)]
	[Min(0f)]
	[Tooltip("Intensity of skybox as ambient light source")]
	public float intensityMultiplier = 1f;

	[Header("Ambient Skylight For LowGraphicEffects ")]
	[Range(0f, 2f)]
	[Min(0f)]
	[Tooltip("Intensity of skybox as ambient light source")]
	public float intensityMultiplierForLowGraphicEffects;

	[Header("Ambient Colour")]
	[Tooltip("Colour of ambient light (if skybox isn't used for lighting)")]
	public Color ambientColour = Color.white;

	[Header("Dynamic Fog and Mist")]
	[Tooltip("Dynamic fog profile to apply")]
	public DynamicFogProfile fogProfile;

	private DynamicAmbienceState prevAmbience;

	private Light[] lightTemplates;

	private Light[] lightInstances;

	public bool IsReady { get; set; }

	private void Start()
	{
		if (Mathf.Approximately(intensityMultiplierForLowGraphicEffects, 0f))
		{
			intensityMultiplierForLowGraphicEffects = intensityMultiplier;
		}
		ProceduralMapTile componentInParent = GetComponentInParent<ProceduralMapTile>();
		if (componentInParent != null)
		{
			componentInParent.Ambience = this;
		}
		InitLighting();
		IsReady = true;
	}

	private void InitLighting()
	{
		if (lightTemplates == null)
		{
			SetLightLevel(0f);
		}
	}

	public void BeginBlendIn()
	{
		Capture(ref prevAmbience);
		DynamicFog dynamicFog = StaticAmbience.FindCameraObject()?.GetComponent<DynamicFog>();
		if (dynamicFog != null)
		{
			if (fogProfile != null)
			{
				dynamicFog.useFogVolumes = true;
				if (Application.isPlaying)
				{
					dynamicFog.SetTargetProfile(fogProfile, ProceduralScenario.dynamicFogTransitionTime);
				}
				else
				{
					dynamicFog.profile = fogProfile;
				}
			}
			else
			{
				Debug.LogWarning("No volumetric fog profile set for dynamic ambience '" + base.name + "' so will cut to disabled.");
				dynamicFog.useFogVolumes = false;
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (dynamicFog != null)
		{
			stringBuilder.Append(" (");
			if (dynamicFog != null)
			{
				stringBuilder.Append("fog: ");
				stringBuilder.Append((fogProfile != null) ? fogProfile.name : "none");
			}
			stringBuilder.Append(")");
		}
	}

	public void ApplyBlendIn(float blend)
	{
		_ = base.gameObject.scene;
		if (base.gameObject.activeInHierarchy)
		{
			float num = (PlatformLayer.Setting.UseLowGraphicEffects ? intensityMultiplierForLowGraphicEffects : intensityMultiplier);
			float ambientIntensity = num;
			Color ambientSkyColor = ambientColour;
			if (prevAmbience.isAvailable)
			{
				ambientIntensity = Mathf.Lerp(prevAmbience.intensityMultiplier, num, blend);
				ambientSkyColor = Color.Lerp(prevAmbience.ambientColour, ambientColour, blend);
			}
			RenderSettings.ambientIntensity = ambientIntensity;
			RenderSettings.ambientSkyColor = ambientSkyColor;
			SetLightLevel(blend);
		}
	}

	public void EndBlendIn()
	{
		prevAmbience.isAvailable = false;
	}

	public void BeginBlendOut()
	{
	}

	public void ApplyBlendOut(float blend)
	{
		SetLightLevel(blend);
	}

	public void EndBlendOut()
	{
	}

	private void Capture(ref DynamicAmbienceState state)
	{
		state.isAvailable = true;
		state.intensityMultiplier = RenderSettings.ambientIntensity;
		state.ambientColour = RenderSettings.ambientSkyColor;
	}

	private void OnValidate()
	{
		ProceduralScenario componentInParent = GetComponentInParent<ProceduralScenario>();
		if (componentInParent != null && componentInParent.CurrentDynamicAmbience == this)
		{
			ApplyBlendIn(1f);
		}
	}

	private void SetLightLevel(float level)
	{
		if (lightTemplates == null)
		{
			lightTemplates = GetComponentsInChildren<Light>();
			lightInstances = new Light[lightTemplates.Length];
			for (int i = 0; i < lightTemplates.Length; i++)
			{
				GameObject gameObject = Object.Instantiate(lightTemplates[i].gameObject);
				lightInstances[i] = gameObject.GetComponent<Light>();
				gameObject.hideFlags |= HideFlags.DontSave;
				gameObject.transform.SetParent(base.transform);
				lightTemplates[i].gameObject.SetActive(value: false);
			}
		}
		for (int j = 0; j < lightTemplates.Length; j++)
		{
			Light light = lightTemplates[j];
			Light obj = lightInstances[j];
			obj.intensity = light.intensity * level;
			obj.gameObject.SetActive(level > 0f);
		}
	}
}
