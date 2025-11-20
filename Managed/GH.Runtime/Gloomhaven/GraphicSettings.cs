#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using SM.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace Gloomhaven;

public class GraphicSettings : LocalizedListener
{
	[Serializable]
	private class AntialiasingGraphicProfile
	{
		public string graphicProfile;

		public AntialiasingRender antialiasing;

		public AntialiasingGraphicProfile(string graphicProfile, AntialiasingRender antialiasing)
		{
			this.graphicProfile = graphicProfile;
			this.antialiasing = antialiasing;
		}
	}

	private class GraphicSelector<T> : SelectorWrapper<T> where T : Enum
	{
		public GraphicSelector(TMP_Dropdown dropdown)
			: base(dropdown, (from T it in Enum.GetValues(typeof(T))
				select new SelectorOptData<T>(it, () => LocalizationManager.GetTranslation(string.Format("{0}_{1}", typeof(T).Name.SplitCamelCase().Replace(" ", "_"), it)))).ToList())
		{
		}

		public GraphicSelector(TMP_Dropdown dropdown, List<SelectorOptData<T>> selectorOptDatas)
			: base(dropdown, selectorOptDatas)
		{
		}
	}

	private const string CUSTOM_PROFILE = "Custom";

	[SerializeField]
	private ButtonSwitch advancedOptionsToggle;

	[SerializeField]
	private GameObject advancedOptionsContainer;

	[SerializeField]
	private ExtendedDropdown levelSelector;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private ExtendedScrollRect scrollRect;

	[Header("Fields")]
	[SerializeField]
	private ExtendedDropdown anisotropicSelector;

	[SerializeField]
	private ExtendedDropdown antialiasingSelector;

	[SerializeField]
	private ExtendedDropdown antialiasingSelectorConsoles;

	[SerializeField]
	private ExtendedDropdown pixelLightSelector;

	[SerializeField]
	private ButtonSwitch reflectionProbesCheckbox;

	[SerializeField]
	private ExtendedDropdown shadowResolutionSelector;

	[SerializeField]
	private ExtendedDropdown shadowSelector;

	[SerializeField]
	private ExtendedDropdown skinWeightsSelector;

	[SerializeField]
	private ButtonSwitch softParticlesCheckbox;

	[SerializeField]
	private ExtendedDropdown textureQualitySelector;

	[SerializeField]
	private ExtendedDropdown vsyncSelector;

	[SerializeField]
	private ExtendedDropdown fpsSelector;

	[SerializeField]
	private GameObject fpsOption;

	[Header("Antialiasing")]
	[SerializeField]
	private List<AntialiasingGraphicProfile> defaultAntialiasingPerProfile;

	private string selectedLevel;

	private GraphicSelector<AnisotropicFiltering> anisotropicFilteringOpts;

	private GraphicSelector<ShadowQuality> shadowsQualityOpts;

	private GraphicSelector<ShadowResolution> shadowResolutionOpts;

	private GraphicSelector<SkinWeights> skinWeightsOpts;

	private GraphicSelector<TextureQualityRender> textureQualityOpts;

	private GraphicSelector<AntialiasingRender> antialisingOpts;

	private GraphicSelector<VSyncQuality> vSyncOps;

	private SelectorWrapper<string> levelOpts;

	private Dictionary<string, GraphicProfile> unityProfiles;

	public static AntialiasingRender s_Antialiasing;

	private void Awake()
	{
		unityProfiles = new Dictionary<string, GraphicProfile>();
		List<SelectorOptData<string>> list = QualitySettings.names.Select(Selector).ToList();
		list.Add(new SelectorOptData<string>("Custom", () => CustomStringTranslate("Custom")));
		levelOpts = new SelectorWrapper<string>(levelSelector, list);
		levelOpts.OnValuedChanged.AddListener(SetQualityLevel);
		InitializeDropDown(levelSelector);
		shadowsQualityOpts = new GraphicSelector<ShadowQuality>(shadowSelector);
		shadowsQualityOpts.OnValuedChanged.AddListener(SetShadows);
		InitializeDropDown(shadowSelector);
		anisotropicFilteringOpts = new GraphicSelector<AnisotropicFiltering>(anisotropicSelector);
		anisotropicFilteringOpts.OnValuedChanged.AddListener(SetAnisotropicTexture);
		InitializeDropDown(anisotropicSelector);
		shadowResolutionOpts = new GraphicSelector<ShadowResolution>(shadowResolutionSelector);
		shadowResolutionOpts.OnValuedChanged.AddListener(SetShadowResolution);
		InitializeDropDown(shadowResolutionSelector);
		skinWeightsOpts = new GraphicSelector<SkinWeights>(skinWeightsSelector);
		skinWeightsOpts.OnValuedChanged.AddListener(SetSkinWeights);
		InitializeDropDown(skinWeightsSelector);
		textureQualityOpts = new GraphicSelector<TextureQualityRender>(textureQualitySelector);
		textureQualityOpts.OnValuedChanged.AddListener(SetTextureQuality);
		InitializeDropDown(textureQualitySelector);
		antialiasingSelector = (PlatformLayer.Instance.IsConsole ? antialiasingSelectorConsoles : antialiasingSelector);
		List<SelectorOptData<AntialiasingRender>> list2 = (from AntialiasingRender it in Enum.GetValues(typeof(AntialiasingRender))
			select new SelectorOptData<AntialiasingRender>(it, () => LocalizationManager.GetTranslation(string.Format("{0}_{1}", typeof(AntialiasingRender).Name.SplitCamelCase().Replace(" ", "_"), it)))).ToList();
		if (Is8thConsoleGenerationExceptSwitch())
		{
			list2 = list2.Take(2).ToList();
		}
		else if (PlatformLayer.Instance.IsConsole)
		{
			DisableConsoleAntialiasingDropdown();
		}
		antialisingOpts = new GraphicSelector<AntialiasingRender>(antialiasingSelector, list2);
		antialisingOpts.OnValuedChanged.AddListener(SetAntiAliasing);
		InitializeDropDown(antialiasingSelector);
		vSyncOps = new GraphicSelector<VSyncQuality>(vsyncSelector);
		vSyncOps.OnValuedChanged.AddListener(SetVSync);
		InitializeDropDown(vsyncSelector);
		fpsSelector.onValueChanged.AddListener(delegate(int index)
		{
			SetFPS(Convert.ToInt32(fpsSelector.options[index].text));
		});
		InitializeDropDown(fpsSelector);
		pixelLightSelector.onValueChanged.AddListener(delegate(int index)
		{
			SetPixelLight(Convert.ToInt32(pixelLightSelector.options[index].text));
		});
		InitializeDropDown(pixelLightSelector);
		softParticlesCheckbox.OnValueChanged.AddListener(SetSoftParticles);
		softParticlesCheckbox.OnSelected.AddListener(delegate
		{
			OnSelected(softParticlesCheckbox);
		});
		reflectionProbesCheckbox.OnValueChanged.AddListener(SetRealtimeReflectionProbes);
		reflectionProbesCheckbox.OnSelected.AddListener(delegate
		{
			OnSelected(reflectionProbesCheckbox);
		});
		advancedOptionsToggle.OnValueChanged.AddListener(ToggleAdvancedOptions);
		advancedOptionsToggle.OnSelected.AddListener(delegate
		{
			OnSelected(advancedOptionsToggle);
		});
		controllerArea.OnFocusedArea.AddListener(EnableNavigation);
		controllerArea.OnUnfocusedArea.AddListener(DisableNavigation);
		if (controllerArea.IsFocused)
		{
			EnableNavigation();
		}
		void DisableConsoleAntialiasingDropdown()
		{
			antialiasingSelector.transform.parent.parent.gameObject.SetActive(value: false);
		}
		static bool Is8thConsoleGenerationExceptSwitch()
		{
			DeviceType currentPlatform = PlatformLayer.Instance.GetCurrentPlatform();
			return currentPlatform == DeviceType.PlayStation4 || currentPlatform == DeviceType.PlayStation4Pro || currentPlatform == DeviceType.XboxOne || currentPlatform == DeviceType.XboxOneX;
		}
	}

	protected void OnDestroy()
	{
		levelOpts.OnValuedChanged.RemoveAllListeners();
		shadowsQualityOpts.OnValuedChanged.RemoveAllListeners();
		anisotropicFilteringOpts.OnValuedChanged.RemoveAllListeners();
		shadowResolutionOpts.OnValuedChanged.RemoveAllListeners();
		skinWeightsOpts.OnValuedChanged.RemoveAllListeners();
		textureQualityOpts.OnValuedChanged.RemoveAllListeners();
		antialisingOpts.OnValuedChanged.RemoveAllListeners();
		vSyncOps.OnValuedChanged.RemoveAllListeners();
		fpsSelector.onValueChanged.RemoveAllListeners();
		pixelLightSelector.onValueChanged.RemoveAllListeners();
		pixelLightSelector.onValueChanged.RemoveAllListeners();
		softParticlesCheckbox.OnValueChanged.RemoveAllListeners();
	}

	private SelectorOptData<string> Selector(string it)
	{
		return new SelectorOptData<string>(it, () => CustomStringTranslate(it));
	}

	private string CustomStringTranslate(string stringToTranslate)
	{
		if (LocalizationManager.TryGetTranslation("GUI_OPT_GRAPHICS_" + stringToTranslate, out var Translation))
		{
			return Translation;
		}
		return stringToTranslate;
	}

	private void InitializeDropDown(ExtendedDropdown dropdown)
	{
		dropdown.OnSelected.AddListener(delegate
		{
			OnSelected(dropdown);
		});
		dropdown.OnClosed.AddListener(delegate
		{
			if (controllerArea.IsFocused)
			{
				dropdown.Select();
			}
		});
	}

	public void Initialize()
	{
		SetupQualityLevel();
		if (Application.platform != RuntimePlatform.Switch)
		{
			SetResolution();
		}
	}

	private void SetResolution()
	{
		if (SaveData.Instance.Global.LastLaunchedPlatform != PlatformLayer.Instance.GetCurrentPlatform().ToString())
		{
			SetDefaultResolution();
		}
		else
		{
			SetResolutionStatic();
		}
	}

	private void SetDefaultResolution()
	{
		SaveData.Instance.Global.TargetFrameRate = 60;
		SaveData.Instance.Global.TargetResolutionWidth = Screen.width;
		SaveData.Instance.Global.TargetResolutionHeight = Screen.height;
		SaveData.Instance.Global.TargetFullScreenMode = true;
		SetResolutionStatic();
		SaveData.Instance.Global.LastLaunchedPlatform = PlatformLayer.Instance.GetCurrentPlatform().ToString();
		SaveData.Instance.SaveGlobalData();
	}

	public static void SetResolutionStatic()
	{
		if (PlatformLayer.Instance.IsConsole)
		{
			QualitySettings.vSyncCount = 60 / SaveData.Instance.Global.TargetFrameRate;
		}
		LogUtils.Log($"[Settings] Set resolution, frameRate before {Application.targetFrameRate}");
		Screen.SetResolution(SaveData.Instance.Global.TargetResolutionWidth, SaveData.Instance.Global.TargetResolutionHeight, SaveData.Instance.Global.TargetFullScreenMode, SaveData.Instance.Global.TargetFrameRate);
		if (!PlatformLayer.Instance.IsConsole)
		{
			Application.targetFrameRate = SaveData.Instance.Global.TargetFrameRate;
		}
		LogUtils.Log($"[Settings] Set resolution, frameRate after {Application.targetFrameRate}");
	}

	private void SetupQualityLevel()
	{
		if (SaveData.Instance.Global.CustomQualityProfile != null)
		{
			StoreDefaultProfile();
			SaveData.Instance.Global.CustomQualityProfile.Setup();
			SetFPS(SaveData.Instance.Global.TargetFrameRate, needSave: false);
			SelectCustomProfile();
			UpdateFields();
		}
		else if (QualitySettings.names.Contains(SaveData.Instance.Global.QualityLevel))
		{
			SetQualityLevel(SaveData.Instance.Global.QualityLevel);
		}
		else
		{
			SetDefaultQualityLevel();
		}
	}

	private void ToggleAdvancedOptions(bool toggle)
	{
		advancedOptionsContainer.SetActive(toggle);
		if (controllerArea.IsFocused && toggle)
		{
			pixelLightSelector.Select();
		}
	}

	private void SelectCustomProfile()
	{
		if (!(selectedLevel == "Custom"))
		{
			selectedLevel = "Custom";
			levelOpts.AddOption(new SelectorOptData<string>("Custom", () => LocalizationManager.GetTranslation("GUI_OPT_GRAPHICS_Custom")));
			levelOpts.SetValueWithoutNotify("Custom");
		}
	}

	private void SetDefaultQualityLevel()
	{
		SetQualityLevel(QualitySettings.names[QualitySettings.GetQualityLevel()]);
	}

	private void SetQualityLevel(string level)
	{
		Debug.Log("[GraphicSettings.cs] SetQualityLeve(" + level + ") called");
		if (!(selectedLevel == level))
		{
			QualitySettings.SetQualityLevel(QualitySettings.names.IndexOf(level));
			if (selectedLevel == "Custom" && unityProfiles.ContainsKey(level))
			{
				unityProfiles[level].Setup();
			}
			if (RoomVisibilityManager.s_Instance != null)
			{
				RoomVisibilityManager.s_Instance.ReloadCamera();
			}
			selectedLevel = level;
			SaveData.Instance.Global.CustomQualityProfile = null;
			levelOpts.RemoveOption("Custom");
			levelOpts.SetValueWithoutNotify(level);
			SetFPS(SaveData.Instance.Global.TargetFrameRate, needSave: false);
			UpdateFields();
			Save();
		}
	}

	public void SetTextureQuality(TextureQualityRender textureQuality)
	{
		StoreDefaultProfile();
		GraphicProfile customProfile = GetCustomProfile();
		customProfile.TextureQuality = textureQuality;
		QualitySettings.masterTextureLimit = (int)customProfile.TextureQuality;
		SelectCustomProfile();
		Save();
	}

	private void StoreDefaultProfile()
	{
		string text = QualitySettings.names[QualitySettings.GetQualityLevel()];
		if (!(selectedLevel == "Custom") && !unityProfiles.ContainsKey(text))
		{
			unityProfiles[text] = new GraphicProfile();
			unityProfiles[text].LoadCurrentValues(GetAntialiasingValue(text));
		}
	}

	public void SetAntiAliasing(AntialiasingRender antialiasingRender)
	{
		StoreDefaultProfile();
		GraphicProfile customProfile = GetCustomProfile();
		customProfile.AntiAliasing = antialiasingRender;
		if (!PlatformLayer.Setting.UseSimpleAntialiasTechnique && CameraController.s_CameraController != null)
		{
			PostProcessLayer component = CameraController.s_CameraController.m_Camera.gameObject.GetComponent<PostProcessLayer>();
			if (component != null)
			{
				component.antialiasingMode = (PostProcessLayer.Antialiasing)customProfile.AntiAliasing;
			}
		}
		s_Antialiasing = customProfile.AntiAliasing;
		SelectCustomProfile();
		Save();
	}

	public void SetRealtimeReflectionProbes(bool reflectionProbes)
	{
		StoreDefaultProfile();
		GetCustomProfile().RealtimeReflectionProbes = reflectionProbes;
		QualitySettings.realtimeReflectionProbes = reflectionProbes;
		SelectCustomProfile();
		Save();
	}

	public void SetSoftParticles(bool softParticles)
	{
		StoreDefaultProfile();
		GetCustomProfile().SoftParticles = softParticles;
		QualitySettings.softParticles = softParticles;
		SelectCustomProfile();
		Save();
	}

	public void SetShadowResolution(ShadowResolution shadowResolution)
	{
		StoreDefaultProfile();
		GraphicProfile customProfile = GetCustomProfile();
		customProfile.ShadowResolution = shadowResolution;
		QualitySettings.shadowResolution = customProfile.ShadowResolution;
		SelectCustomProfile();
		Save();
	}

	public void SetShadows(ShadowQuality shadowQuality)
	{
		StoreDefaultProfile();
		GraphicProfile customProfile = GetCustomProfile();
		customProfile.Shadows = shadowQuality;
		QualitySettings.shadows = customProfile.Shadows;
		SelectCustomProfile();
		Save();
	}

	public void SetSkinWeights(SkinWeights skinWeights)
	{
		StoreDefaultProfile();
		GraphicProfile customProfile = GetCustomProfile();
		customProfile.SkinWeights = skinWeights;
		QualitySettings.skinWeights = customProfile.SkinWeights;
		SelectCustomProfile();
		Save();
	}

	public void SetVSync(VSyncQuality vsync)
	{
		StoreDefaultProfile();
		GraphicProfile customProfile = GetCustomProfile();
		customProfile.VSync = vsync;
		QualitySettings.vSyncCount = (int)customProfile.VSync;
		fpsSelector.interactable = vsync == VSyncQuality.DISABLED;
		int fps = ((vsync == VSyncQuality.DISABLED) ? SaveData.Instance.Global.TargetFrameRate : (-1));
		SetFPS(fps);
		SelectCustomProfile();
		Save();
	}

	public void SetFPS(int fps, bool needSave = true)
	{
		LogUtils.Log($"[Settings] Set FPS {fps}, previous is {Application.targetFrameRate}");
		Application.targetFrameRate = fps;
		if (needSave)
		{
			SaveData.Instance.Global.TargetFrameRate = fps;
			SaveData.Instance.SaveGlobalData();
		}
	}

	public void SetPixelLight(int pixelLight)
	{
		StoreDefaultProfile();
		GetCustomProfile().PixelLight = pixelLight;
		QualitySettings.pixelLightCount = pixelLight;
		SelectCustomProfile();
		Save();
	}

	public void SetAnisotropicTexture(AnisotropicFiltering anisotropicFiltering)
	{
		StoreDefaultProfile();
		GraphicProfile customProfile = GetCustomProfile();
		customProfile.AnisotropicTexture = anisotropicFiltering;
		QualitySettings.anisotropicFiltering = customProfile.AnisotropicTexture;
		SelectCustomProfile();
		Save();
	}

	private void UpdateFields()
	{
		GraphicProfile customProfile = SaveData.Instance.Global.CustomQualityProfile;
		anisotropicFilteringOpts.SetValueWithoutNotify(customProfile?.AnisotropicTexture ?? QualitySettings.anisotropicFiltering);
		shadowResolutionOpts.SetValueWithoutNotify(customProfile?.ShadowResolution ?? QualitySettings.shadowResolution);
		skinWeightsOpts.SetValueWithoutNotify(customProfile?.SkinWeights ?? QualitySettings.skinWeights);
		shadowsQualityOpts.SetValueWithoutNotify(customProfile?.Shadows ?? QualitySettings.shadows);
		pixelLightSelector.SetValueWithoutNotify(pixelLightSelector.options.FindIndex((TMP_Dropdown.OptionData it) => it.text == (customProfile?.PixelLight ?? QualitySettings.pixelLightCount).ToString()));
		UpdateAntialiasing();
		textureQualityOpts.SetValueWithoutNotify((TextureQualityRender)(((int?)customProfile?.TextureQuality) ?? QualitySettings.masterTextureLimit));
		VSyncQuality vSyncQuality = (VSyncQuality)(((int?)customProfile?.VSync) ?? QualitySettings.vSyncCount);
		vSyncOps.SetValueWithoutNotify(vSyncQuality);
		int num = ((vSyncQuality == VSyncQuality.DISABLED) ? SaveData.Instance.Global.TargetFrameRate : (-1));
		LogUtils.Log($"[Settings] Update fileds FPS {num}, previous is {Application.targetFrameRate}");
		Application.targetFrameRate = num;
		reflectionProbesCheckbox.SetValue(customProfile?.RealtimeReflectionProbes ?? QualitySettings.realtimeReflectionProbes);
		softParticlesCheckbox.SetValue(customProfile?.SoftParticles ?? QualitySettings.softParticles);
		fpsSelector.SetValueWithoutNotify(fpsSelector.options.FindIndex((TMP_Dropdown.OptionData it) => it.text == SaveData.Instance.Global.TargetFrameRate.ToString()));
		fpsSelector.interactable = vSyncQuality == VSyncQuality.DISABLED;
	}

	public void UpdateAntialiasing()
	{
		GraphicProfile customQualityProfile = SaveData.Instance.Global.CustomQualityProfile;
		s_Antialiasing = ((selectedLevel == "Custom") ? s_Antialiasing : GetAntialiasingValue(selectedLevel));
		Debug.Log($"UpdateAntialiasing to {s_Antialiasing}");
		antialisingOpts.SetValueWithoutNotify(customQualityProfile?.AntiAliasing ?? s_Antialiasing);
	}

	private static GraphicProfile GetCustomProfile()
	{
		if (SaveData.Instance.Global.CustomQualityProfile == null)
		{
			SaveData.Instance.Global.CustomQualityProfile = new GraphicProfile();
			SaveData.Instance.Global.CustomQualityProfile.LoadCurrentValues(s_Antialiasing);
		}
		return SaveData.Instance.Global.CustomQualityProfile;
	}

	private void Save()
	{
		if (!(SaveData.Instance.Global.QualityLevel != "Custom") || !(selectedLevel == SaveData.Instance.Global.QualityLevel) || SaveData.Instance.Global.CustomQualityProfile != null)
		{
			SaveData.Instance.Global.QualityLevel = selectedLevel;
			SaveData.Instance.SaveGlobalData();
		}
	}

	public void CloseSelectors()
	{
		anisotropicSelector.Hide();
		antialiasingSelector.Hide();
		shadowResolutionSelector.Hide();
		shadowSelector.Hide();
		skinWeightsSelector.Hide();
		textureQualitySelector.Hide();
		vsyncSelector.Hide();
	}

	private AntialiasingRender GetAntialiasingValue(string profileName)
	{
		if (Screen.width > 1920 && Screen.height > 1080)
		{
			return AntialiasingRender.DISABLED;
		}
		return defaultAntialiasingPerProfile.FirstOrDefault((AntialiasingGraphicProfile it) => it.graphicProfile == profileName)?.antialiasing ?? s_Antialiasing;
	}

	private void OnSelected(Component target)
	{
		if (InputManager.GamePadInUse)
		{
			if (target == levelSelector)
			{
				scrollRect.ScrollToTop();
			}
			else
			{
				scrollRect.ScrollToFit(target.transform as RectTransform);
			}
		}
	}

	private void EnableNavigation()
	{
		levelSelector.SetNavigation(Navigation.Mode.Vertical);
		anisotropicSelector.SetNavigation(Navigation.Mode.Vertical);
		antialiasingSelector.SetNavigation(Navigation.Mode.Vertical);
		shadowResolutionSelector.SetNavigation(Navigation.Mode.Vertical);
		shadowSelector.SetNavigation(Navigation.Mode.Vertical);
		skinWeightsSelector.SetNavigation(Navigation.Mode.Vertical);
		textureQualitySelector.SetNavigation(Navigation.Mode.Vertical);
		vsyncSelector.SetNavigation(Navigation.Mode.Vertical);
		fpsSelector.SetNavigation(Navigation.Mode.Vertical);
		pixelLightSelector.SetNavigation(Navigation.Mode.Vertical);
		softParticlesCheckbox.SetNavigation(Navigation.Mode.Vertical);
		reflectionProbesCheckbox.SetNavigation(Navigation.Mode.Vertical);
		advancedOptionsToggle.SetNavigation(Navigation.Mode.Vertical);
	}

	private void DisableNavigation()
	{
		levelSelector.DisableNavigation();
		anisotropicSelector.DisableNavigation();
		antialiasingSelector.DisableNavigation();
		shadowResolutionSelector.DisableNavigation();
		shadowSelector.DisableNavigation();
		skinWeightsSelector.DisableNavigation();
		textureQualitySelector.DisableNavigation();
		vsyncSelector.DisableNavigation();
		fpsSelector.DisableNavigation();
		pixelLightSelector.DisableNavigation();
		softParticlesCheckbox.DisableNavigation();
		reflectionProbesCheckbox.DisableNavigation();
		advancedOptionsToggle.DisableNavigation();
	}

	protected override void OnLanguageChanged()
	{
		anisotropicFilteringOpts.RefreshTexts();
		antialisingOpts.RefreshTexts();
		levelOpts.RefreshTexts();
		shadowResolutionOpts.RefreshTexts();
		shadowsQualityOpts.RefreshTexts();
		skinWeightsOpts.RefreshTexts();
		textureQualityOpts.RefreshTexts();
		vSyncOps.RefreshTexts();
	}
}
