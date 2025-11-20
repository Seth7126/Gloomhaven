using System;
using Code.State;
using FFSNet;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.MainMenuStates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiplayerConfirmationBox : MonoBehaviour
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private ExtendedButton confirmButton;

	[SerializeField]
	private ExtendedButton alternativeConfirmButton;

	[SerializeField]
	private ExtendedButton cancelButton;

	[SerializeField]
	private TMP_Text informationText;

	[SerializeField]
	private ControllerInputAreaLocal _controllerArea;

	[SerializeField]
	private Image _playerPlatformImage;

	[SerializeField]
	private ExtendedButton _pcConfirmButton;

	[SerializeField]
	private ExtendedButton _pcAlternativeConfirmButton;

	[SerializeField]
	private ExtendedButton _pcCancelButton;

	[SerializeField]
	private GameObject _pcButtons;

	[SerializeField]
	private GameObject _gamepadButtons;

	private IStateFilter _stateFilter = new StateFilterByType(typeof(MultiplayerOnlineContainerWithSelectedState)).InverseFilter();

	private UIWindow confirmationBox;

	private Action _onConfirmClickedCallback;

	private Action _onAlternativeConfirmClickedCallback;

	private Action _onCancelClickedCallback;

	private NetworkPlayer _networkPlayer;

	private KeyActionHandler _cancelKeyActionHandler;

	private KeyActionHandler _reportKeyActionHandler;

	private KeyActionHandler _kickKeyActionHandler;

	private void Awake()
	{
		confirmationBox = GetComponent<UIWindow>();
		_controllerArea.OnFocusedArea.AddListener(HandleFocus);
		_controllerArea.OnUnfocusedArea.AddListener(HandleUnfocus);
		ResetConfirmationBox();
		InitGamepadInput();
	}

	private void HandleUnfocus()
	{
		Debug.LogError("Multiplayer window has been unfocused!");
	}

	private void OnDestroy()
	{
		DeinitGamepadInput();
		confirmButton.onClick.RemoveAllListeners();
		alternativeConfirmButton?.onClick.RemoveAllListeners();
		cancelButton.onClick.RemoveAllListeners();
	}

	private void OnControllerCancelled()
	{
		OnCancel();
	}

	private void OnControllerSubmit()
	{
		OnConfirm();
		Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState();
	}

	private void OnControllerAlternativeSubmit()
	{
		OnSecondConfirm();
		Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState();
	}

	private void HandleFocus()
	{
		Singleton<UINavigation>.Instance.NavigationManager.DeselectAll();
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.MultiplayerConfirmationBox);
	}

	private void InitGamepadInput()
	{
		_kickKeyActionHandler = new KeyActionHandler(KeyAction.UI_SUBMIT, OnControllerSubmit);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(_kickKeyActionHandler.AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(_controllerArea)));
		_reportKeyActionHandler = new KeyActionHandler(KeyAction.UI_KICK_PLAYER, OnControllerAlternativeSubmit);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(_reportKeyActionHandler.AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(_controllerArea)));
		_cancelKeyActionHandler = new KeyActionHandler(KeyAction.UI_CANCEL, OnControllerCancelled);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(_cancelKeyActionHandler.AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(_controllerArea)));
	}

	private void DeinitGamepadInput()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnControllerSubmit);
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_KICK_PLAYER, OnControllerAlternativeSubmit);
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnControllerCancelled);
		}
	}

	public void ShowConfirmation(NetworkPlayer player, Action onActionConfirmed = null, Action onAlternativeActionConfirmed = null, Action onCanceled = null)
	{
		Singleton<UINavigation>.Instance.StateMachine.SetFilter(null);
		ResetConfirmationBox();
		if (_kickKeyActionHandler == null || !_kickKeyActionHandler.HasBlockers)
		{
			InitGamepadInput();
		}
		_networkPlayer = player;
		_onConfirmClickedCallback = onActionConfirmed;
		_onAlternativeConfirmClickedCallback = onAlternativeActionConfirmed;
		_onCancelClickedCallback = onCanceled;
		icon.sprite = player.Avatar;
		icon.color = UIInfoTools.GetPlaceholderPlayerColor(player);
		informationText.text = player.Username;
		_playerPlatformImage.sprite = PlatformLayer.Instance.PlayerPlatformImageController.GetPlayerPlatformImage(player.PlatformName);
		bool gamePadInUse = InputManager.GamePadInUse;
		_pcButtons.SetActive(!gamePadInUse);
		_gamepadButtons.SetActive(gamePadInUse);
		if (gamePadInUse)
		{
			confirmButton?.gameObject.SetActive(_onConfirmClickedCallback != null);
			alternativeConfirmButton?.gameObject.SetActive(_onAlternativeConfirmClickedCallback != null);
			confirmButton.onClick.AddListener(OnConfirm);
			alternativeConfirmButton?.onClick.AddListener(OnSecondConfirm);
			cancelButton.onClick.AddListener(OnCancel);
		}
		else
		{
			_pcConfirmButton?.gameObject.SetActive(_onConfirmClickedCallback != null);
			_pcAlternativeConfirmButton?.gameObject.SetActive(_onAlternativeConfirmClickedCallback != null);
			_pcConfirmButton.onClick.AddListener(OnConfirm);
			_pcAlternativeConfirmButton.onClick.AddListener(OnSecondConfirm);
			_pcCancelButton.onClick.AddListener(OnCancel);
		}
		confirmationBox.Show();
		_controllerArea.Enable();
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
	}

	private void OnPlayerLeft(NetworkPlayer player)
	{
		if (player == _networkPlayer)
		{
			OnCancel();
		}
	}

	private void OnConfirm()
	{
		if (_onConfirmClickedCallback == null)
		{
			return;
		}
		confirmationBox.onTransitionComplete.AddListener(delegate(UIWindow window, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Hidden)
			{
				_onConfirmClickedCallback();
			}
		});
		Hide();
	}

	private void OnSecondConfirm()
	{
		if (_onAlternativeConfirmClickedCallback == null)
		{
			return;
		}
		confirmationBox.onTransitionComplete.AddListener(delegate(UIWindow window, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Hidden)
			{
				_onAlternativeConfirmClickedCallback();
			}
		});
		Hide();
	}

	private void OnCancel()
	{
		Hide();
		_onCancelClickedCallback?.Invoke();
	}

	public void Hide(bool isToPreviousStateRequired = true)
	{
		confirmationBox.Hide();
		_controllerArea.Destroy();
		_networkPlayer = null;
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		if (isToPreviousStateRequired)
		{
			Singleton<UINavigation>.Instance.StateMachine.SetFilter(null);
			Singleton<UINavigation>.Instance.StateMachine.ToPreviousState();
		}
	}

	private void ResetConfirmationBox()
	{
		confirmButton.onClick.RemoveAllListeners();
		alternativeConfirmButton?.onClick.RemoveAllListeners();
		cancelButton.onClick.RemoveAllListeners();
		_pcConfirmButton.onClick.RemoveAllListeners();
		_pcAlternativeConfirmButton?.onClick.RemoveAllListeners();
		_pcCancelButton.onClick.RemoveAllListeners();
		confirmationBox.onHidden.RemoveAllListeners();
		confirmationBox.onTransitionComplete.RemoveAllListeners();
	}
}
