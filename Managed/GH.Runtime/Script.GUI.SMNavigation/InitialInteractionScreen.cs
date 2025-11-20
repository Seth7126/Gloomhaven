using System;
using System.Collections;
using JetBrains.Annotations;
using Platforms;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;
using UnityEngine.UI;
using VoiceChat;

namespace Script.GUI.SMNavigation;

[RequireComponent(typeof(UIWindow))]
public class InitialInteractionScreen : MonoBehaviour
{
	[SerializeField]
	private ExtendedButton _pressFireButton;

	[SerializeField]
	private MainStateTag _enteringTag;

	[SerializeField]
	private MainStateTag _exitTag;

	private UIWindow _uiWindow;

	private SettingsHolder _settingsHolder;

	private IPlatform _currentPlatform;

	public event Action OnSuccessfulSignIn;

	[UsedImplicitly]
	private void Awake()
	{
		_uiWindow = GetComponent<UIWindow>();
		_settingsHolder = SceneController.Instance.SettingsHolder;
		_currentPlatform = global::PlatformLayer.Platform;
		_pressFireButton.onClick.AddListener(OnButtonPressed);
		_pressFireButton.interactable = true;
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnButtonPressed).AddBlocker(new UIWindowOpenKeyActionBlocker(_uiWindow)).AddBlocker(new ExtendedButtonInteractableKeyActionHandlerBlocker(_pressFireButton)));
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnButtonPressed);
		}
		_pressFireButton.onClick.RemoveListener(OnButtonPressed);
	}

	public void Show()
	{
		_uiWindow.Show();
		Singleton<UINavigation>.Instance.StateMachine.Enter(_enteringTag);
		if (global::PlatformLayer.GameProvider.IsSignInUIRequired)
		{
			_pressFireButton.interactable = false;
			_currentPlatform.UserManagement.SignIn(OnSignInCompleted, isSignInUiRequired: true);
		}
		if (global::PlatformLayer.GameProvider.IsShowSignOutMessage)
		{
			Singleton<SignOutConfirmationBox>.Instance.Activate("Consoles/GUI_SIGNOUT_POPUP_TITLE", "Consoles/GUI_SIGNOUT_POPUP_MESSAGE");
		}
	}

	private void OnButtonPressed()
	{
		_pressFireButton.interactable = false;
		_currentPlatform.UserManagement.SignIn(OnSignInCompleted, global::PlatformLayer.GameProvider.IsSignInUIRequired);
	}

	private void OnSignInCompleted(bool signedIn)
	{
		if (signedIn)
		{
			if (global::PlatformLayer.Instance.IsDelayedInit)
			{
				global::PlatformLayer.UserData.CurrentAuthStatus = PlatformUserData.EPlatformAuthStatus.Authorised;
				StartCoroutine(InitUserData());
			}
			else
			{
				_pressFireButton.interactable = true;
			}
		}
		else
		{
			_pressFireButton.interactable = true;
		}
	}

	private IEnumerator InitUserData()
	{
		SceneController.Instance.ShowLoadingScreen();
		SaveData.Instance.LoadRootData();
		yield return SaveData.Instance.LoadRulebase();
		yield return SceneController.Instance.InitAndLoadYML();
		yield return SceneController.Instance.RefreshEntitlements();
		SceneController.Instance.LoadDlcLanguageUpdate();
		yield return _settingsHolder.Setup();
		_settingsHolder.SavePrimarySettingsOnMachine();
		ObjectPool.ClearAllExceptCards();
		yield return Singleton<InputManager>.Instance.InitKeyBindings();
		BoltVoiceBridge.Instance.SetUpUsername();
		_currentPlatform.PlatformSocial.RegisterInviteEvent();
		LoadMenu();
		SceneController.Instance.DisableLoadingScreen();
	}

	private void LoadMenu()
	{
		_uiWindow.Hide();
		this.OnSuccessfulSignIn?.Invoke();
	}
}
