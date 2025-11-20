#define ENABLE_LOGS
using GLOOM;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class YesNoDialog : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private ExtendedButton yesButton;

	[SerializeField]
	private ExtendedButton noButton;

	[SerializeField]
	private RectTransform box;

	[SerializeField]
	private ControllerInputAreaLocal _controllerAreaLocal;

	private UIWindow window;

	private UnityAction _yesAction;

	private UnityAction _noAction;

	private bool _autoClose;

	private void Awake()
	{
		window = GetComponent<UIWindow>();
		window.IsPopUp = true;
		InitGamepadInput();
	}

	private void InitGamepadInput()
	{
		if (InputManager.GamePadInUse)
		{
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnControllerSubmit).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(_controllerAreaLocal)));
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, OnControllerCancelled).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(_controllerAreaLocal)));
			_controllerAreaLocal.OnFocusedArea.AddListener(OnFocus);
			_controllerAreaLocal.OnUnfocusedArea.AddListener(OnUnfocused);
		}
	}

	private void OnUnfocused()
	{
	}

	private void OnFocus()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.YesNoCharacterActionDialog);
	}

	private void DisableGamepadInput()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null && InputManager.GamePadInUse)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnControllerSubmit);
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnControllerCancelled);
		}
	}

	private void OnControllerCancelled()
	{
		OnNoClickHandle();
		Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState();
	}

	private void OnControllerSubmit()
	{
		OnYesClickHandle();
	}

	private void OnDestroy()
	{
		yesButton.onClick.RemoveAllListeners();
		noButton.onClick.RemoveAllListeners();
		DisableGamepadInput();
	}

	public void Init(string descriptionKey, bool autoClose, UnityAction yesAction, UnityAction noAction = null)
	{
		_yesAction = yesAction;
		_noAction = noAction;
		_autoClose = autoClose;
		descriptionText.text = LocalizationManager.GetTranslation(descriptionKey);
		yesButton.onClick.RemoveAllListeners();
		noButton.onClick.RemoveAllListeners();
		window.onShown.RemoveAllListeners();
		window.onHidden.RemoveAllListeners();
		window.onShown.AddListener(delegate
		{
			CleanButtons();
			Debug.Log("onShown");
			window.onHidden.AddListener(delegate
			{
				Debug.Log("onHidden clean onhidden");
				RemoveAllListeners();
				Debug.Log("onHidden call no action");
				_noAction?.Invoke();
			});
			yesButton.onClick.AddListener(OnYesClickHandle);
			noButton.onClick.AddListener(OnNoClickHandle);
		});
	}

	private void OnNoClickHandle()
	{
		Debug.Log("No button clean onhidden");
		RemoveAllListeners();
		Hide();
		Debug.Log("noButton call no action");
		_noAction?.Invoke();
	}

	private void OnYesClickHandle()
	{
		RemoveAllListeners();
		Hide();
		Debug.Log("Yes button clean onhidden");
		_yesAction?.Invoke();
	}

	private void RemoveAllListeners()
	{
		window.onHidden.RemoveAllListeners();
		noButton.onClick.RemoveAllListeners();
		yesButton.onClick.RemoveAllListeners();
	}

	public void PrepareInteractabilityForPlayer(CPlayerActor playerContext)
	{
		InteractabilityIsolatedUIControl component = yesButton.GetComponent<InteractabilityIsolatedUIControl>();
		InteractabilityIsolatedUIControl component2 = noButton.GetComponent<InteractabilityIsolatedUIControl>();
		if (component != null)
		{
			component.ControlSecondIdentifier = playerContext.ActorGuid;
		}
		if (component2 != null)
		{
			component2.ControlSecondIdentifier = playerContext.ActorGuid;
		}
	}

	private void CleanButtons()
	{
		yesButton.ClearSelectedState();
		noButton.ClearSelectedState();
	}

	public void Hide()
	{
		window.Hide();
		if (InputManager.GamePadInUse && _controllerAreaLocal != null)
		{
			_controllerAreaLocal.Destroy();
		}
	}

	public void Show(RectTransform position = null)
	{
		if (InputManager.GamePadInUse)
		{
			window.Show();
			_controllerAreaLocal.Enable();
			return;
		}
		if (position != null)
		{
			box.position = position.position;
		}
		else
		{
			box.anchoredPosition = Vector2.zero;
		}
		window.Show();
	}
}
