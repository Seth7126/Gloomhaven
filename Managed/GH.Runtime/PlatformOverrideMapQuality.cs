using System;
using System.Collections.Generic;
using Script.Optimization;
using UnityEngine;

[CreateAssetMenu(fileName = "Platform override apparance quality levels", menuName = "ScriptableObjects/Platform override apparance quality levels")]
public class PlatformOverrideMapQuality : ScriptableObject
{
	public enum TargetFrameRate
	{
		FrameRate30,
		FrameRate60,
		Both
	}

	[Serializable]
	public class OverrideSettings
	{
		[SerializeField]
		private string _nameLevel;

		[SerializeField]
		private LightImportance _lightImportance;

		[SerializeField]
		private int _maxOpenedRoomsWithLight = 2;

		[SerializeField]
		private TargetFrameRate _targetFrameRate = TargetFrameRate.Both;

		[SerializeField]
		private ApparancePlatformSettingData _settings;

		public TargetFrameRate TargetFrameRate => _targetFrameRate;

		public string NameLevel => _nameLevel;

		public LightImportance LightImportance => _lightImportance;

		public int MaxOpenedRoomsWithLight => _maxOpenedRoomsWithLight;

		public ApparancePlatformSettingData Settings => _settings;
	}

	[SerializeField]
	private List<OverrideSettings> _overrideSettings = new List<OverrideSettings>();

	public OverrideSettings TryGetSettingForLevel(string settingsName)
	{
		TargetFrameRate targetFrameRate = ((SaveData.Instance.Global.TargetFrameRate == 60) ? TargetFrameRate.FrameRate60 : TargetFrameRate.FrameRate30);
		foreach (OverrideSettings overrideSetting in _overrideSettings)
		{
			if (overrideSetting.NameLevel == settingsName && (overrideSetting.TargetFrameRate == TargetFrameRate.Both || overrideSetting.TargetFrameRate == targetFrameRate))
			{
				UnityEngine.Debug.LogWarning("TEST... :" + overrideSetting.TargetFrameRate);
				return overrideSetting;
			}
		}
		return null;
	}
}
