#define ENABLE_LOGS
using System;
using Script.Optimization;
using UnityEngine;

[CreateAssetMenu(fileName = "PlatformSetting", menuName = "ScriptableObjects/Platform setting")]
public class PlatformSetting : ScriptableObject
{
	[Serializable]
	public class LowParticlesUseData
	{
		[SerializeField]
		private bool _environmentLow;

		[SerializeField]
		private bool _hitLow;

		[SerializeField]
		private bool _otherLow;

		[SerializeField]
		private bool _propsLow;

		[SerializeField]
		private bool _weaponsLow;

		[SerializeField]
		private bool _guiLow;

		[SerializeField]
		private bool _charactersLow;

		[SerializeField]
		private bool _noCardsParticles;

		public bool EnvironmentLow => _environmentLow;

		public bool HitLow => _hitLow;

		public bool OtherLow => _otherLow;

		public bool PropsLow => _propsLow;

		public bool WeaponsLow => _weaponsLow;

		public bool GUILow => _guiLow;

		public bool CharactersLow => _charactersLow;

		public bool NoCardsParticles => _noCardsParticles;
	}

	[Serializable]
	public class SimplifiedUISettingsData
	{
		[SerializeField]
		private bool _lowGostEffectActorPanel;

		[SerializeField]
		private bool _miniCardEffectWithoutMaterials;

		[SerializeField]
		private bool _abilityCardEffectWithoutMaterials;

		[SerializeField]
		private bool _disableUIBlur;

		public bool LowGostEffectActorPanel => _lowGostEffectActorPanel;

		public bool MiniCardEffectWithoutMaterials => _miniCardEffectWithoutMaterials;

		public bool AbilityCardEffectWithoutMaterials => _abilityCardEffectWithoutMaterials;

		public bool DisableUIBlur => _disableUIBlur;
	}

	[SerializeField]
	private DeviceType _platform;

	[SerializeField]
	private bool _usePreloadAudioClips;

	[SerializeField]
	private LightImportance _lightShowLevel;

	[SerializeField]
	private int _defaultMaxOpenedRoomsWithLight = int.MaxValue;

	[SerializeField]
	private bool _useDecalOptimization;

	[SerializeField]
	private bool _useTouchScreen;

	[SerializeField]
	private bool _turnOffUICameraBloom;

	[SerializeField]
	private string _qualityLevel;

	[SerializeField]
	private bool _useLowGraphicEffects;

	[SerializeField]
	private bool _doNotLoadHighShaders;

	[SerializeField]
	private bool _useSimpleAntialiasTechnique;

	[SerializeField]
	private bool _forceFreeMemoryCharacter3DDisplay;

	[SerializeField]
	private bool _forceFreeMemoryParticleSystems;

	[SerializeField]
	private LowParticlesUseData _lowParticlesUse = new LowParticlesUseData();

	[SerializeField]
	private bool _prefabInstancing;

	[SerializeField]
	private bool _lessObjectPooling;

	[SerializeField]
	private bool _simplifyPhysics;

	[SerializeField]
	private bool _simplifiedUI;

	[SerializeField]
	private SimplifiedUISettingsData _simplifiedUISettings = new SimplifiedUISettingsData();

	[SerializeField]
	private bool _restrictCamera;

	[SerializeField]
	private bool _disableFogOfWar;

	[Header("Apparance Settings")]
	[SerializeField]
	private ApparancePlatformSettingData _apparancePlatformSettingData;

	[Header("Procedural Content")]
	[SerializeField]
	private ProceduralContentPlatformData _proceduralContentPlatformData;

	[Header("Graphic Platform Settings")]
	[SerializeField]
	private GraphicPlatformSettings _graphicPlatformSettings;

	[Header("Apparance override Quality levels")]
	[SerializeField]
	private PlatformOverrideMapQuality _mapQualityOverrides;

	public DeviceType Platform => _platform;

	public bool UsePreloadAudioClips => _usePreloadAudioClips;

	public bool UseTouchpad => _useTouchScreen;

	public bool UseDecalOptimization => _useDecalOptimization;

	public bool TurnOffUICameraBloom => _turnOffUICameraBloom;

	public bool UseLowGraphicEffects => _useLowGraphicEffects;

	public bool DoNotLoadHighShaders => _doNotLoadHighShaders;

	public bool UseSimpleAntialiasTechnique => _useSimpleAntialiasTechnique;

	public bool ForceFreeMemoryCharacter3DDisplay => _forceFreeMemoryCharacter3DDisplay;

	public bool ForceFreeMemoryParticleSystems => _forceFreeMemoryParticleSystems;

	public ApparancePlatformSettingData ApparanceSettingsData => _apparancePlatformSettingData;

	public PlatformOverrideMapQuality OverrideApparanceQualityLevels => _mapQualityOverrides;

	public ProceduralContentPlatformData ProceduralContentPlatformData => _proceduralContentPlatformData;

	public GraphicPlatformSettings GraphicPlatformSettings => _graphicPlatformSettings;

	public bool SimplifyPhysics => _simplifyPhysics;

	public bool PrefabInstancing => _prefabInstancing;

	public bool LessObjectPooling => _lessObjectPooling;

	public LowParticlesUseData LowParticlesUse => _lowParticlesUse;

	public bool SimplifiedUI => _simplifiedUI;

	public SimplifiedUISettingsData SimplifiedUISettings => _simplifiedUISettings;

	public bool RestrictCamera => _restrictCamera;

	public bool DisableFogOfWar => _disableFogOfWar;

	public ApparancePlatformSettingData GetApparenceSettingByCurrentLevel()
	{
		if (OverrideApparanceQualityLevels == null)
		{
			return ApparanceSettingsData;
		}
		string currentQuest = GetCurrentQuest();
		if (string.IsNullOrEmpty(currentQuest))
		{
			return ApparanceSettingsData;
		}
		PlatformOverrideMapQuality.OverrideSettings overrideSettings = OverrideApparanceQualityLevels.TryGetSettingForLevel(currentQuest);
		if (overrideSettings == null || overrideSettings.Settings == null)
		{
			return ApparanceSettingsData;
		}
		return overrideSettings.Settings;
	}

	public PlatformOverrideMapQuality.OverrideSettings GetOverrideSettingsForCurrentLevel()
	{
		string currentQuest = GetCurrentQuest();
		if (string.IsNullOrEmpty(currentQuest))
		{
			return null;
		}
		if (OverrideApparanceQualityLevels == null)
		{
			return null;
		}
		return OverrideApparanceQualityLevels.TryGetSettingForLevel(currentQuest);
	}

	public LightImportance GetLightImportance()
	{
		if (OverrideApparanceQualityLevels == null)
		{
			return _lightShowLevel;
		}
		string currentQuest = GetCurrentQuest();
		if (string.IsNullOrEmpty(currentQuest))
		{
			return _lightShowLevel;
		}
		PlatformOverrideMapQuality.OverrideSettings overrideSettings = OverrideApparanceQualityLevels.TryGetSettingForLevel(currentQuest);
		if (overrideSettings == null || overrideSettings.LightImportance == LightImportance.None)
		{
			return _lightShowLevel;
		}
		return overrideSettings.LightImportance;
	}

	public int GetMaxOpenedRoomsWithLight()
	{
		if (OverrideApparanceQualityLevels == null)
		{
			return _defaultMaxOpenedRoomsWithLight;
		}
		string currentQuest = GetCurrentQuest();
		if (string.IsNullOrEmpty(currentQuest))
		{
			return _defaultMaxOpenedRoomsWithLight;
		}
		return OverrideApparanceQualityLevels.TryGetSettingForLevel(currentQuest)?.MaxOpenedRoomsWithLight ?? _defaultMaxOpenedRoomsWithLight;
	}

	public string GetCurrentQuest()
	{
		if (SaveData.Instance.Global == null || SaveData.Instance.Global.CurrentAdventureData == null || SaveData.Instance.Global.CurrentAdventureData.AdventureMapState == null || SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState == null)
		{
			return null;
		}
		string text = null;
		if (SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState != null && SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.QuestName != null)
		{
			text = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.QuestName;
		}
		else if (SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentSingleScenario != null && SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentSingleScenario != null)
		{
			text = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentSingleScenario.Name;
		}
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return text;
	}

	public string GetQualitySettingsName()
	{
		string[] names = QualitySettings.names;
		string[] array = names;
		foreach (string text in array)
		{
			if (text == _qualityLevel)
			{
				return text;
			}
		}
		Debug.LogWarning("[PlatformSettings.cs] Was not able to match QualitySettings name. QualitySetting named " + _qualityLevel + " was not found in the build. Probably because console build is running in Editor. Setting named " + names[0] + " returned as default.");
		return names[0];
	}

	public int GetQualitySettingsIndex()
	{
		string[] names = QualitySettings.names;
		string qualitySettingsName = GetQualitySettingsName();
		return names.IndexOf(qualitySettingsName);
	}
}
