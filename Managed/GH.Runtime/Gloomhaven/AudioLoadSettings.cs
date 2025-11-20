using UnityEngine;
using UnityEngine.Video;

namespace Gloomhaven;

public class AudioLoadSettings : MonoBehaviour
{
	[SerializeField]
	private VideoPlayer _mainVideoPlayer;

	public void Initialize()
	{
		int masterVolume = SaveData.Instance.Global.MasterVolume;
		int musicVolume = SaveData.Instance.Global.MusicVolume;
		int sFXVolume = SaveData.Instance.Global.SFXVolume;
		int multiplayerPingVolume = SaveData.Instance.Global.MultiplayerPingVolume;
		int storyVolume = SaveData.Instance.Global.StoryVolume;
		int uIVolume = SaveData.Instance.Global.UIVolume;
		int hapticVolume = SaveData.Instance.Global.HapticVolume;
		int hapticVibration = SaveData.Instance.Global.HapticVibration;
		AudioControllerUtils.AdjustMasterVolume(_mainVideoPlayer, masterVolume);
		AudioControllerUtils.AdjustMusicVolume(musicVolume);
		AudioControllerUtils.AdjustEffectsVolume(sFXVolume);
		AudioControllerUtils.AdjustPingVolume(multiplayerPingVolume);
		AudioControllerUtils.AdjustStoryVolume(storyVolume);
		AudioControllerUtils.AdjustUIVolume(uIVolume);
		AudioControllerUtils.AdjustHapticVolume(hapticVolume);
		AudioControllerUtils.AdjustHapticVibration(hapticVibration);
	}
}
