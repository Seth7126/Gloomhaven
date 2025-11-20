using System.Collections;
using Code.State;
using GLOOM.MainMenu;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class EarlyAccessNotificationManager : Singleton<EarlyAccessNotificationManager>
{
	[SerializeField]
	private ControllerInputAreaLocal _inputAreaLocal;

	[SerializeField]
	private Hotkey _hotkey;

	public GameObject earlyAccessNotification;

	public string notificationId = "Notification.EarlyAccess.v3";

	private UIWindow _window;

	protected override void Awake()
	{
		base.Awake();
		_window = GetComponent<UIWindow>();
		_window.onHidden.AddListener(delegate
		{
			ControllerInputScroll.SetGlobalEnabled(enabled: true);
			MainMenuUIManager.Instance.RequestDisableInteraction(disable: false, this);
		});
		if (_inputAreaLocal != null)
		{
			_inputAreaLocal.OnFocusedArea.AddListener(OnAreaFocused);
			_inputAreaLocal.OnUnfocusedArea.AddListener(OnAreaUnfocused);
		}
	}

	protected override void OnDestroy()
	{
		if (InputManager.GamePadInUse)
		{
			_hotkey.Deinitialize();
		}
		base.OnDestroy();
	}

	private void Start()
	{
		if (PlayerPrefs.GetInt(notificationId, 0) != 1 && !PlatformLayer.Networking.HasInvitePending)
		{
			_window.ShowOrUpdateStartingState();
			ShowEarlyAccessNotification();
		}
	}

	public void ShowEarlyAccessNotification()
	{
		if (base.gameObject.activeSelf)
		{
			earlyAccessNotification.SetActive(value: true);
			MainMenuUIManager.Instance.RequestDisableInteraction(disable: true, this);
			ControllerInputScroll.SetGlobalEnabled(enabled: false);
			if (!InputManager.GamePadInUse)
			{
				StartCoroutine(ShowDelayedNotification());
			}
			else
			{
				ShowNotification();
			}
		}
	}

	public IEnumerator ShowDelayedNotification()
	{
		yield return new WaitForEndOfFrame();
		ShowNotification();
	}

	private void OnAreaFocused()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.MenuInfo);
	}

	private void OnAreaUnfocused()
	{
		if (!SceneController.Instance.IsLoading)
		{
			IStateFilter stateFilter = new StateFilterByType(typeof(GamepadDisconnectionBoxState)).InverseFilter();
			Singleton<UINavigation>.Instance.StateMachine.ToPreviousState(stateFilter);
		}
	}

	private void ShowNotification()
	{
		_window.Show();
		if (InputManager.GamePadInUse)
		{
			_hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
		StoreShownNotification();
	}

	private void StoreShownNotification()
	{
		PlayerPrefs.SetInt(notificationId, 1);
		PlayerPrefs.Save();
	}
}
