using System;
using System.Collections;
using GLOOM;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class EULAScreen : MonoBehaviour
{
	private const int EuParam = 116;

	private const int UsParam = 115;

	private const string EULA_BODY_KEY = "eula_body";

	private const string EULA_EU_PS4_KEY = "eula_eu_ps4";

	private const string EULA_EU_PS5_KEY = "eula_eu_ps5";

	private const string EULA_US_PS4_KEY = "eula_us_ps4";

	private const string EULA_US_PS5_KEY = "eula_us_ps5";

	[SerializeField]
	private GameObject _message;

	[SerializeField]
	private Hotkey[] _hotkeys;

	[SerializeField]
	private TextMeshProUGUI _eulaBody;

	[SerializeField]
	private Button _submitButton;

	[SerializeField]
	private Button _cancelButton;

	private UIWindow _uiWindow;

	private Action _onAccepted;

	private void Awake()
	{
		_uiWindow = GetComponent<UIWindow>();
		if (InputManager.GamePadInUse)
		{
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnSubmitClicked).AddBlocker(new UIWindowOpenKeyActionBlocker(_uiWindow)));
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, OnCancelClicked).AddBlocker(new UIWindowOpenKeyActionBlocker(_uiWindow)));
		}
		else
		{
			_submitButton.onClick.AddListener(OnSubmitClicked);
			_cancelButton.onClick.AddListener(OnCancelClicked);
		}
	}

	private void OnDestroy()
	{
		if (InputManager.GamePadInUse)
		{
			if (Singleton<KeyActionHandlerController>.IsInitialized)
			{
				Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnSubmitClicked);
				Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnCancelClicked);
			}
		}
		else
		{
			_submitButton.onClick.RemoveListener(OnSubmitClicked);
			_cancelButton.onClick.RemoveListener(OnCancelClicked);
		}
	}

	private void ShowHotkeys()
	{
		Hotkey[] hotkeys = _hotkeys;
		for (int i = 0; i < hotkeys.Length; i++)
		{
			hotkeys[i].Initialize(Singleton<UINavigation>.Instance.Input);
		}
	}

	private void HideHotkeys()
	{
		Hotkey[] hotkeys = _hotkeys;
		foreach (Hotkey obj in hotkeys)
		{
			obj.Deinitialize();
			obj.DisplayHotkey(active: false);
		}
	}

	public void Show(Action onAccepted)
	{
		if (InputManager.GamePadInUse)
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.EULAScreen);
			ShowHotkeys();
		}
		_onAccepted = onAccepted;
		_uiWindow.Show();
		string translation = LocalizationManager.GetTranslation("eula_body");
		_eulaBody.text = translation;
	}

	private void OnSubmitClicked()
	{
		if (_message.activeInHierarchy)
		{
			_message.SetActive(value: false);
		}
		else
		{
			StartCoroutine(AddUser());
		}
	}

	private IEnumerator AddUser()
	{
		while (SaveData.Instance.SaveQueue.IsAnyOperationExecuting)
		{
			yield return null;
		}
		SaveData.Instance.Global.UsersAcceptedEULA.Add(PlatformLayer.UserData.PlatformAccountID);
		SaveData.Instance.SaveGlobalData();
		yield return null;
		_uiWindow.Hide();
		if (InputManager.GamePadInUse)
		{
			HideHotkeys();
			Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.MainOptions);
		}
		_onAccepted?.Invoke();
	}

	private void OnCancelClicked()
	{
		if (!_message.activeInHierarchy)
		{
			_message.SetActive(value: true);
		}
	}
}
