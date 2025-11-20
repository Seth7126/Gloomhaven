using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Gloomhaven;

public class AudioSettings : MonoBehaviour
{
	[SerializeField]
	private UISliderController masterVolumeSlider;

	[SerializeField]
	private UISliderController musicVolumeSlider;

	[SerializeField]
	private UISliderController effectsVolumeSlider;

	[SerializeField]
	private UISliderController pingVolumeSlider;

	[SerializeField]
	private UISliderController storyVolumeSlider;

	[SerializeField]
	private UISliderController uiVolumeSlider;

	[SerializeField]
	private ButtonSwitch muteInBackgroundToggle;

	[SerializeField]
	private GameObject muteInBackgroundGO;

	[SerializeField]
	private GameObject hapticHeaderGO;

	[SerializeField]
	private GameObject hapticVolumeGO;

	[SerializeField]
	private GameObject hapticVibraGO;

	[SerializeField]
	private UISliderController hapticVolumeSlider;

	[SerializeField]
	private UISliderController hapticVibrationSlider;

	[SerializeField]
	private VideoPlayer _mainVideoPlayer;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	private void Awake()
	{
		muteInBackgroundToggle.OnValueChanged.AddListener(ToggleMuteAudioInBackground);
		masterVolumeSlider.SubscribeOnSliderValueChanged(AdjustMasterVolume);
		musicVolumeSlider.SubscribeOnSliderValueChanged(AdjustMusicVolume);
		effectsVolumeSlider.SubscribeOnSliderValueChanged(AdjustEffectsVolume);
		pingVolumeSlider.SubscribeOnSliderValueChanged(AdjustPingVolume);
		storyVolumeSlider.SubscribeOnSliderValueChanged(AdjustStoryVolume);
		uiVolumeSlider.SubscribeOnSliderValueChanged(AdjustUIVolume);
	}

	protected void OnDestroy()
	{
		muteInBackgroundToggle.OnValueChanged.RemoveAllListeners();
		masterVolumeSlider.ClearOnSliderValueChanged();
		musicVolumeSlider.ClearOnSliderValueChanged();
		effectsVolumeSlider.ClearOnSliderValueChanged();
		pingVolumeSlider.ClearOnSliderValueChanged();
		storyVolumeSlider.ClearOnSliderValueChanged();
		uiVolumeSlider.ClearOnSliderValueChanged();
	}

	private void ShowHapticControls()
	{
		hapticHeaderGO.SetActive(value: true);
		hapticVolumeGO.SetActive(value: true);
		hapticVibraGO.SetActive(value: true);
	}

	private void HideHapticControls()
	{
		hapticHeaderGO.SetActive(value: false);
		hapticVolumeGO.SetActive(value: false);
		hapticVibraGO.SetActive(value: false);
	}

	private void EnableNavigation()
	{
		masterVolumeSlider.EnableNavigation(select: true);
		musicVolumeSlider.EnableNavigation();
		effectsVolumeSlider.EnableNavigation();
		pingVolumeSlider.EnableNavigation();
		storyVolumeSlider.EnableNavigation();
		uiVolumeSlider.EnableNavigation();
		muteInBackgroundToggle.SetNavigation(Navigation.Mode.Vertical);
	}

	private void DisableNavigation()
	{
		masterVolumeSlider.DisableNavigation();
		musicVolumeSlider.DisableNavigation();
		effectsVolumeSlider.DisableNavigation();
		pingVolumeSlider.DisableNavigation();
		storyVolumeSlider.DisableNavigation();
		muteInBackgroundToggle.DisableNavigation();
		uiVolumeSlider.DisableNavigation();
	}

	private void OnEnable()
	{
		int masterVolume = SaveData.Instance.Global.MasterVolume;
		int musicVolume = SaveData.Instance.Global.MusicVolume;
		int sFXVolume = SaveData.Instance.Global.SFXVolume;
		int multiplayerPingVolume = SaveData.Instance.Global.MultiplayerPingVolume;
		int storyVolume = SaveData.Instance.Global.StoryVolume;
		int uIVolume = SaveData.Instance.Global.UIVolume;
		_ = SaveData.Instance.Global.HapticVolume;
		_ = SaveData.Instance.Global.HapticVibration;
		masterVolumeSlider.SetAmount(masterVolume);
		musicVolumeSlider.SetAmount(musicVolume);
		effectsVolumeSlider.SetAmount(sFXVolume);
		pingVolumeSlider.SetAmount(multiplayerPingVolume);
		storyVolumeSlider.SetAmount(storyVolume);
		uiVolumeSlider.SetAmount(uIVolume);
		muteInBackgroundToggle.SetValue(SaveData.Instance.Global.MuteAudioInBackground);
		muteInBackgroundGO.SetActive(!PlatformLayer.Instance.IsConsole);
		AdjustMasterVolume(masterVolume, saveChange: false);
		AdjustMusicVolume(musicVolume, saveChange: false);
		AdjustEffectsVolume(sFXVolume, saveChange: false);
		AdjustPingVolume(multiplayerPingVolume, saveChange: false);
		AdjustStoryVolume(storyVolume, saveChange: false);
		AdjustUIVolume(uIVolume, saveChange: false);
		HideHapticControls();
	}

	public void AdjustMasterVolume(float volume)
	{
		AdjustMasterVolume(volume, saveChange: true);
	}

	private void ToggleMuteAudioInBackground(bool active)
	{
		if (active != SaveData.Instance.Global.MuteAudioInBackground)
		{
			SaveData.Instance.Global.MuteAudioInBackground = active;
			SaveData.Instance.SaveGlobalData();
			if (PlatformLayer.Instance.IsDelayedInit)
			{
				SaveGlobalDataOnMachine();
			}
		}
	}

	private void AdjustMasterVolume(float volume, bool saveChange)
	{
		if (AudioControllerUtils.AdjustMasterVolume(_mainVideoPlayer, volume) && saveChange)
		{
			SaveData.Instance.Global.MasterVolume = (int)volume;
			SaveData.Instance.SaveGlobalData();
			if (PlatformLayer.Instance.IsDelayedInit)
			{
				SaveGlobalDataOnMachine();
			}
		}
	}

	public void AdjustMusicVolume(float volume)
	{
		AdjustMusicVolume(volume, saveChange: true);
	}

	private void AdjustMusicVolume(float volume, bool saveChange)
	{
		if (AudioControllerUtils.AdjustMusicVolume(volume) && saveChange)
		{
			SaveData.Instance.Global.MusicVolume = (int)volume;
			SaveData.Instance.SaveGlobalData();
			if (PlatformLayer.Instance.IsDelayedInit)
			{
				SaveGlobalDataOnMachine();
			}
		}
	}

	public void AdjustPingVolume(float volume)
	{
		AdjustPingVolume(volume, saveChange: true);
	}

	private void AdjustPingVolume(float volume, bool saveChange)
	{
		if (AudioControllerUtils.AdjustPingVolume(volume) && saveChange)
		{
			SaveData.Instance.Global.MultiplayerPingVolume = (int)volume;
			SaveData.Instance.SaveGlobalData();
			if (PlatformLayer.Instance.IsDelayedInit)
			{
				SaveGlobalDataOnMachine();
			}
		}
	}

	public void AdjustStoryVolume(float volume)
	{
		AdjustStoryVolume(volume, saveChange: true);
	}

	private void AdjustStoryVolume(float volume, bool saveChange)
	{
		if (AudioControllerUtils.AdjustStoryVolume(volume) && saveChange)
		{
			SaveData.Instance.Global.StoryVolume = (int)volume;
			SaveData.Instance.SaveGlobalData();
			if (PlatformLayer.Instance.IsDelayedInit)
			{
				SaveGlobalDataOnMachine();
			}
		}
	}

	public void AdjustUIVolume(float volume)
	{
		AdjustUIVolume(volume, saveChange: true);
	}

	private void AdjustUIVolume(float volume, bool saveChange)
	{
		if (AudioControllerUtils.AdjustUIVolume(volume) && saveChange)
		{
			SaveData.Instance.Global.UIVolume = (int)volume;
			SaveData.Instance.SaveGlobalData();
			if (PlatformLayer.Instance.IsDelayedInit)
			{
				SaveGlobalDataOnMachine();
			}
		}
	}

	public void AdjustEffectsVolume(float volume)
	{
		AdjustEffectsVolume(volume, saveChange: true);
	}

	private void AdjustEffectsVolume(float volume, bool saveChange)
	{
		if (AudioControllerUtils.AdjustEffectsVolume(volume) && saveChange)
		{
			SaveData.Instance.Global.SFXVolume = (int)volume;
			SaveData.Instance.SaveGlobalData();
			if (PlatformLayer.Instance.IsDelayedInit)
			{
				SaveGlobalDataOnMachine();
			}
		}
	}

	private void AdjustHapticVolume(float volume)
	{
		AdjustHapticVolume(volume, saveChanges: true);
	}

	private void AdjustHapticVolume(float volume, bool saveChanges)
	{
		if (AudioControllerUtils.AdjustHapticVolume(volume) && saveChanges)
		{
			SaveData.Instance.Global.HapticVolume = (int)volume;
			SaveData.Instance.SaveGlobalData();
		}
	}

	private void AdjustHapticVibration(float volume)
	{
		AdjustHapticVibration(volume, saveChanges: true);
	}

	private void AdjustHapticVibration(float volume, bool saveChanges)
	{
		if (AudioControllerUtils.AdjustHapticVibration(volume) && saveChanges)
		{
			SaveData.Instance.Global.HapticVibration = (int)volume;
			SaveData.Instance.SaveGlobalData();
		}
	}

	private void SaveGlobalDataOnMachine()
	{
		string value = JsonUtility.ToJson(SaveData.Instance.Global);
		PlayerPrefs.SetString("GlobalData.dat", value);
		PlayerPrefs.Save();
	}
}
