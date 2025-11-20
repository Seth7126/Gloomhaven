#define ENABLE_LOGS
using System;
using GLOOM;
using Gloomhaven;
using Platforms;
using Platforms.Social;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoiceChat;

public class UserVoiceUiController : MonoBehaviour, IDisposable
{
	[SerializeField]
	private TMP_Text _playersName;

	[SerializeField]
	private Button _muteButton;

	[SerializeField]
	private Slider _volumeSlider;

	[SerializeField]
	private UISliderController _sliderController;

	[SerializeField]
	private CanvasGroup _mute;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Color _enableColor;

	[SerializeField]
	private Color _muteColor;

	[SerializeField]
	private float _displayModificator = 100f;

	private IUserVoice _voice;

	private VoiceChatUserBlockerController _voiceChatUserBlockerController;

	private const string COMMUNICATION_DISABLED_UNKNOWN_REASON = "Consoles/COMMUNICATION_DISABLED_UNKNOWN_REASON";

	private const string COMMUNICATION_DISABLED_NOT_ALLOWED_REASON = "Consoles/COMMUNICATION_DISABLED_NOT_ALLOWED_REASON";

	private const string COMMUNICATION_DISABLED_SETTINGS_RESTRICTS_REASON = "Consoles/COMMUNICATION_DISABLED_SETTINGS_RESTRICTS_REASON";

	private const string COMMUNICATION_DISABLED_USER_IN_BLOCK_LIST_REASON = "Consoles/COMMUNICATION_DISABLED_USER_IN_BLOCK_LIST_REASON";

	private const string COMMUNICATION_DISABLED_USER_IN_MUTE_LIST_REASON = "Consoles/COMMUNICATION_DISABLED_USER_IN_MUTE_LIST_REASON";

	private string _currentErrorSystemMessageLocKey = "Consoles/COMMUNICATION_DISABLED_UNKNOWN_REASON";

	public bool IsAbleToCommunicate { get; private set; } = true;

	public float DisplayVolume
	{
		get
		{
			return _volumeSlider.value / _displayModificator;
		}
		set
		{
			_sliderController.SetAmount(Mathf.RoundToInt(value * _displayModificator));
			_volumeSlider.SetValueWithoutNotify(value * _displayModificator);
		}
	}

	public IUserVoice Voice => _voice;

	public void Show(IUserVoice voice, VoiceChatUserBlockerController voiceChatUserBlockerController, bool showVolumeSlider = true)
	{
		_voice = voice;
		_voiceChatUserBlockerController = voiceChatUserBlockerController;
		if (voice.Name != null)
		{
			_playersName.text = PlatformTextSpriteProvider.FormatUserNameWithPlatformIcon(voice.PlatformName, voice.Name);
		}
		Subscribe();
		DisplayVolume = Voice.Volume;
		DisplayCurrentMute();
		base.gameObject.SetActive(value: true);
		_voice.EventMuteChange += UpdateBlockedState;
		UpdateBlockedState();
		if (!showVolumeSlider)
		{
			_sliderController.gameObject.SetActive(value: false);
		}
	}

	public void Hide()
	{
		if (_voice != null)
		{
			_voice.EventMuteChange -= UpdateBlockedState;
		}
		IsAbleToCommunicate = true;
		_currentErrorSystemMessageLocKey = "Consoles/COMMUNICATION_DISABLED_UNKNOWN_REASON";
		Reset();
		base.gameObject.SetActive(value: false);
	}

	private void UpdateBlockedState()
	{
		if (_voiceChatUserBlockerController.IsUserVoiceUnBlocked(_voice, out var result, out var mutedByPlayer))
		{
			ViewVoiceAsUnblocked();
		}
		else
		{
			ViewVoiceAsBlocked(result, mutedByPlayer);
		}
		DisplayCurrentMute();
	}

	public void Dispose()
	{
		Reset();
		_voice = null;
	}

	private void Subscribe()
	{
		_muteButton.onClick.AddListener(OnMuteButtonClick);
		_volumeSlider.onValueChanged.AddListener(OnVolumeSliderChange);
	}

	private void UnSubscribe()
	{
		_muteButton.onClick.RemoveListener(OnMuteButtonClick);
		_volumeSlider.onValueChanged.RemoveListener(OnVolumeSliderChange);
	}

	private void Reset()
	{
		if (Voice != null)
		{
			UnSubscribe();
			_voice = null;
		}
	}

	public void DisplayMute(bool mute)
	{
		Debug.Log("DisplayMute(Voice.IsMuted: " + mute + ") was called...");
		_mute.alpha = (mute ? 1 : 0);
		_icon.color = (mute ? _muteColor : _enableColor);
	}

	private void DisplayCurrentMute()
	{
		DisplayMute(Voice.IsMuted || !Voice.HasRecordDevice);
	}

	private void OnVolumeSliderChange(float newValue)
	{
		Voice.Volume = DisplayVolume;
	}

	private void OnMuteButtonClick()
	{
		if (Voice.IsMuted)
		{
			if (!IsAbleToCommunicate)
			{
				PlatformLayer.Message.ShowSystemMessage(IPlatformMessage.MessageType.Ok, LocalizationManager.GetTranslation(_currentErrorSystemMessageLocKey), null);
			}
			else
			{
				_voiceChatUserBlockerController.UnMuteByPlayer(Voice.PlatformAccountID);
			}
		}
		else
		{
			_voiceChatUserBlockerController.MuteByPlayer(Voice.PlatformAccountID);
		}
	}

	public void ViewVoiceAsBlocked(PermissionOperationResult result, bool mutedByPlayer)
	{
		IsAbleToCommunicate = mutedByPlayer;
		switch (result)
		{
		case PermissionOperationResult.PrivilegeSettingsRestricts:
			_currentErrorSystemMessageLocKey = "Consoles/COMMUNICATION_DISABLED_SETTINGS_RESTRICTS_REASON";
			break;
		case PermissionOperationResult.UserInMuteList:
			_currentErrorSystemMessageLocKey = "Consoles/COMMUNICATION_DISABLED_USER_IN_MUTE_LIST_REASON";
			break;
		case PermissionOperationResult.UserInBlockList:
			_currentErrorSystemMessageLocKey = "Consoles/COMMUNICATION_DISABLED_USER_IN_BLOCK_LIST_REASON";
			break;
		case PermissionOperationResult.NotAllowed:
			_currentErrorSystemMessageLocKey = "Consoles/COMMUNICATION_DISABLED_NOT_ALLOWED_REASON";
			break;
		case PermissionOperationResult.Unknown:
			_currentErrorSystemMessageLocKey = "Consoles/COMMUNICATION_DISABLED_UNKNOWN_REASON";
			break;
		}
		_volumeSlider.interactable = false;
		Debug.Log("Voice " + Voice.Name + " with PlatformAccountID " + Voice.PlatformAccountID + " is blocked, user muted");
	}

	public void ViewVoiceAsUnblocked()
	{
		IsAbleToCommunicate = true;
		_currentErrorSystemMessageLocKey = "Consoles/COMMUNICATION_DISABLED_UNKNOWN_REASON";
		_volumeSlider.interactable = true;
		Debug.Log("Voice " + Voice.Name + " with PlatformAccountID " + Voice.PlatformAccountID + " is unblocked");
	}

	public void OnMove(BaseEventData eventData)
	{
		if (eventData is AxisEventData eventData2)
		{
			_volumeSlider.OnMove(eventData2);
		}
	}
}
