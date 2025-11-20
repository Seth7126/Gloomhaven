using System;
using System.Collections.Generic;
using Platforms.Social;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;
using UnityEngine.UI;

namespace VoiceChat;

public class VoceChatOptions : MonoBehaviour
{
	[SerializeField]
	private UIWindow _window;

	[SerializeField]
	private UserVoiceUiController _userVoiceUiController;

	[SerializeField]
	private List<UserVoiceUiController> _userControllers;

	[SerializeField]
	private GameObject _voiceChatConnectedPanel;

	[SerializeField]
	private ExtendedButton _switchChatButton;

	[SerializeField]
	private string _onChatLocalizationKey;

	[SerializeField]
	private string _offChatLocalizationKey;

	private readonly UiNavigationBlocker _navigationBlocker = new UiNavigationBlocker("VoceChatOptions");

	private BoltVoiceChatService _service;

	private bool _wasConnected;

	private bool _isAcquiringRequiredPrivilegesFailed;

	private bool _wasStart;

	private bool _mushShow;

	private readonly Vector2 _voiceChatControllerOnHiddenPosition = new Vector2(-735f, -400f);

	private readonly Vector2 _voiceChatControllerOnShownPosition = new Vector2(-735f, -400f);

	public event Action OnHiddenCallback;

	public event Action OnDestroyCallback;

	public void Start()
	{
		_window.onShown.AddListener(OnShown);
		_window.onHidden.AddListener(OnHidden);
		_wasStart = true;
		if (_mushShow)
		{
			Show();
			_mushShow = false;
		}
	}

	private void OnDestroy()
	{
		this.OnDestroyCallback?.Invoke();
		_window.onShown.RemoveListener(OnShown);
		_window.onHidden.RemoveListener(OnHidden);
	}

	public void Show()
	{
		if (!_wasStart)
		{
			_mushShow = true;
		}
		else
		{
			_window.Show();
		}
	}

	public void Hide()
	{
		_window.Hide();
	}

	private void OnShown()
	{
		_wasConnected = false;
		_service = Singleton<BoltVoiceChatService>.Instance;
		InitVoiceChatUserBlockerController();
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.VoiceChatOptions);
		_service.EventJoinRoom += ServiceOnEventJoinRoom;
		_service.EventLeftRoom += ServiceOnEventLeftRoom;
		DisplayStateStatus(_service.IsVoiceChatConnected);
		_switchChatButton.onClick.AddListener(SwitchStatus);
		Singleton<ESCMenu>.Instance.VoiceChatController.ChangePosition(_voiceChatControllerOnShownPosition);
	}

	private void OnHidden()
	{
		this.OnHiddenCallback?.Invoke();
		_service.EventJoinRoom -= ServiceOnEventJoinRoom;
		_service.EventLeftRoom -= ServiceOnEventLeftRoom;
		if (_wasConnected)
		{
			Unbind();
		}
		_switchChatButton.onClick.RemoveListener(SwitchStatus);
		Singleton<UINavigation>.Instance.StateMachine.ToPreviousState();
		if (Singleton<ESCMenu>.Instance != null && Singleton<ESCMenu>.Instance.VoiceChatController != null)
		{
			Singleton<ESCMenu>.Instance.VoiceChatController.ChangePosition(_voiceChatControllerOnHiddenPosition);
		}
	}

	private void VoicePermissionValidationStarted()
	{
		InputManager.RequestDisableInput(this, EKeyActionTag.All);
		Singleton<UINavigation>.Instance.NavigationManager.BlockNavigation(_navigationBlocker);
	}

	private void VoicePermissionValidationCompleted(OperationResult operationResult)
	{
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
		Singleton<UINavigation>.Instance.NavigationManager.UnblockNavigation(_navigationBlocker);
	}

	private void ServiceOnEventJoinRoom()
	{
		InitVoiceChatUserBlockerController();
		DisplayStateStatus(connected: true);
	}

	private void ServiceOnEventLeftRoom()
	{
		DisplayStateStatus(connected: false);
	}

	private void VoiceBridgeOnEventRemoveIncomingVoiceUser(ConnectedUserVoice connectedUserVoice, int index)
	{
		Unbind();
		Bind();
	}

	private void VoiceBridgeOnEventAddIncomingVoiceUser(ConnectedUserVoice connectedUserVoice, int index)
	{
		InitVoiceChatUserBlockerController();
		_userControllers[index].Show(connectedUserVoice, _service.VoiceChatUserBlockerController);
	}

	private void InitVoiceChatUserBlockerController()
	{
		_service.InitVoiceChatUserBlockerController();
	}

	private void SwitchStatus()
	{
		if (!_service.IsVoiceChatConnected)
		{
			PlatformLayer.Networking.CheckForPrivilegeValidityAsync(Privilege.Communications, delegate(bool isCommunicationValid)
			{
				if (isCommunicationValid)
				{
					_service.SetupAndConnect();
				}
			}, PrivilegePlatform.All);
		}
		else
		{
			_service.ShutdownVoiceConnection();
		}
	}

	private void Bind()
	{
		InitVoiceChatUserBlockerController();
		_wasConnected = true;
		_service.EventNewUserConnected += VoiceBridgeOnEventAddIncomingVoiceUser;
		_service.EventUserDisconnected += VoiceBridgeOnEventRemoveIncomingVoiceUser;
		_service.EventValidatePermissionStarted += VoicePermissionValidationStarted;
		_service.EventValidatePermissionCompleted += VoicePermissionValidationCompleted;
		_userVoiceUiController.Show(_service.SelfUserVoice, _service.VoiceChatUserBlockerController);
		for (int i = 0; i < _userControllers.Count; i++)
		{
			UserVoiceUiController userVoiceUiController = _userControllers[i];
			if (i < _service.PlayerVoices.Count)
			{
				userVoiceUiController.Show(_service.PlayerVoices[i], _service.VoiceChatUserBlockerController);
			}
			else
			{
				userVoiceUiController.Hide();
			}
		}
	}

	private void Unbind()
	{
		_wasConnected = false;
		_service.EventNewUserConnected -= VoiceBridgeOnEventAddIncomingVoiceUser;
		_service.EventUserDisconnected -= VoiceBridgeOnEventRemoveIncomingVoiceUser;
		_service.EventValidatePermissionStarted -= VoicePermissionValidationStarted;
		_service.EventValidatePermissionCompleted -= VoicePermissionValidationCompleted;
		_userVoiceUiController.Hide();
		for (int i = 0; i < _userControllers.Count; i++)
		{
			_userControllers[i].Hide();
		}
	}

	private void DisplayStateStatus(bool connected)
	{
		_switchChatButton.TextLanguageKey = (connected ? _onChatLocalizationKey : _offChatLocalizationKey);
		_voiceChatConnectedPanel.SetActive(connected);
		if (connected && !_wasConnected)
		{
			Bind();
		}
		else if (_wasConnected && !connected)
		{
			Unbind();
		}
	}
}
