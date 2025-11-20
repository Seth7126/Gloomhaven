#define ENABLE_LOGS
using System.Text;
using BeautifyEffect;
using LowGraphicEffects;
using UnityEngine;
using UnityEngine.Rendering;
using UnityStandardAssets.Utility.Ambience;

[ExecuteInEditMode]
public class StaticAmbience : MonoBehaviour
{
	[Header("Environment")]
	[Tooltip("Scene skybox material")]
	public Material skyboxMaterial;

	[Tooltip("Use the skybox for ambient lighting colours?")]
	public bool skyboxAmbientLight;

	[Header("Post Processing")]
	[Tooltip("Post processing effects profile to apply")]
	public BeautifyProfile postProcessingProfile;

	[Tooltip("Post processing effects profile to apply")]
	public AmbienceConfig ambienceConfig;

	private void Start()
	{
		ProceduralScenario componentInParent = GetComponentInParent<ProceduralScenario>();
		if (componentInParent != null)
		{
			bool flag = true;
			if (componentInParent.Ambience != null && componentInParent.Ambience.gameObject.transform.parent.GetSiblingIndex() < base.transform.GetSiblingIndex())
			{
				flag = false;
			}
			if (flag)
			{
				componentInParent.Ambience = this;
			}
		}
	}

	public void Apply()
	{
		_ = base.gameObject.scene;
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		RenderSettings.skybox = skyboxMaterial;
		RenderSettings.ambientMode = ((!skyboxAmbientLight) ? AmbientMode.Flat : AmbientMode.Skybox);
		GameObject gameObject = FindCameraObject();
		if (PlatformLayer.Setting.UseLowGraphicEffects)
		{
			GraphicEffects component = gameObject.GetComponent<GraphicEffects>();
			if (component != null)
			{
				component.SetExposure(ambienceConfig);
			}
			return;
		}
		Beautify component2 = gameObject.GetComponent<Beautify>();
		if (component2 != null)
		{
			if (postProcessingProfile != null)
			{
				component2.profile = postProcessingProfile;
				component2.enabled = true;
			}
			else
			{
				Debug.LogWarning("No post processing profile set for static ambience '" + base.name + "'.");
				component2.enabled = false;
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (component2 != null)
		{
			stringBuilder.Append(" (");
			if (component2 != null)
			{
				stringBuilder.Append("beautify: ");
				stringBuilder.Append((postProcessingProfile != null) ? postProcessingProfile.name : "none");
			}
			stringBuilder.Append(")");
		}
	}

	private void OnValidate()
	{
		ProceduralScenario componentInParent = GetComponentInParent<ProceduralScenario>();
		if (componentInParent != null && componentInParent.Ambience == this)
		{
			Apply();
		}
	}

	public static GameObject FindCameraObject()
	{
		if (RoomVisibilityManager.s_Instance != null)
		{
			return RoomVisibilityManager.s_Instance.ScenarioCamera.gameObject;
		}
		return CameraController.s_CameraController.m_Camera.gameObject;
	}
}
