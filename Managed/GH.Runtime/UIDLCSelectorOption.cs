#define ENABLE_LOGS
using System;
using AsmodeeNet.Foundation;
using GLOOM;
using GLOOM.MainMenu.DLC;
using SM.Gamepad;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIDLCSelectorOption : LocalizedListener
{
	[Serializable]
	public class DLCSelectorEvent : UnityEvent<DLCRegistry.EDLCKey, bool>
	{
	}

	[Header("Gamepad")]
	[SerializeField]
	private Toggle _gamepadToggle;

	[SerializeField]
	private UINavigationSelectable _selectable;

	[SerializeField]
	private TextMeshProUGUI _description;

	[SerializeField]
	private DLCsContentInfo _dlCsContentInfo;

	[SerializeField]
	private Image _dlsPromo;

	[SerializeField]
	private GameObject _dlcEnabledPanel;

	[SerializeField]
	private GameObject _dlcPurchaseablePanel;

	[SerializeField]
	private GameObject _dlcComingSoonPanel;

	[Header("Keyboard and mouse")]
	[SerializeField]
	private Toggle toggle;

	[Space(5f)]
	[SerializeField]
	private TextLocalizedListener title;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	[SerializeField]
	private AudioButtonProfile audioProfile;

	public DLCSelectorEvent OnToggledDLC;

	private bool _isSelectedByGamepad;

	private DLCConfig.EDLCState _dlcState = DLCConfig.EDLCState.Unavailable;

	private bool _interactable;

	public DLCRegistry.EDLCKey DLC { get; private set; }

	public bool ToggledOn => _gamepadToggle.isOn;

	public bool Interactable => _interactable;

	public bool DLCAvailable => _dlcState == DLCConfig.EDLCState.Available;

	public static event Action<UIDLCSelectorOption> UIDLCOptionSelected;

	public event Action OnDLCStateChanged;

	private void OnDestroy()
	{
		OnToggledDLC = null;
		if (InputManager.GamePadInUse)
		{
			_gamepadToggle.onValueChanged.RemoveAllListeners();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (InputManager.GamePadInUse)
		{
			SubscribeGamepadEvents();
		}
		else
		{
			SubscribeKeyboardAndMouseEvents();
		}
	}

	protected override void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			base.OnDisable();
			DisableNavigation();
			if (InputManager.GamePadInUse)
			{
				UnsubscribeGamepadEvents();
			}
		}
	}

	private void SubscribeKeyboardAndMouseEvents()
	{
		toggle.onValueChanged.AddListener(OnToggled);
	}

	private void UnsubscribeGamepadEvents()
	{
		if (InputManager.GamePadInUse)
		{
			_gamepadToggle.onValueChanged.RemoveListener(OnToggled);
			_selectable.OnNavigationSelectedEvent -= OnSelectedGamepad;
			_selectable.OnNavigationDeselectedEvent -= OnDeselectedGamepad;
		}
	}

	private void SubscribeGamepadEvents()
	{
		if (InputManager.GamePadInUse)
		{
			_gamepadToggle.onValueChanged.AddListener(OnToggled);
			_selectable.OnNavigationSelectedEvent += OnSelectedGamepad;
			_selectable.OnNavigationDeselectedEvent += OnDeselectedGamepad;
		}
	}

	public void OnSubmitPressed()
	{
		if (!InputManager.GamePadInUse && (!toggle.interactable || !toggle.gameObject.activeInHierarchy))
		{
			PlatformLayer.DLC.OpenPlatformStoreDLCOverlay(DLC);
		}
		else if (_gamepadToggle.interactable && _gamepadToggle.gameObject.activeInHierarchy)
		{
			if (audioProfile != null)
			{
				PlaySound(_gamepadToggle.isOn ? audioProfile.mouseUpAudioItem : audioProfile.mouseDownAudioItem);
			}
			SetSelected(!_gamepadToggle.isOn);
		}
	}

	private void OnSelectedGamepad(IUiNavigationSelectable uiNavigationSelectable)
	{
		_isSelectedByGamepad = true;
		ExecuteEvents.Execute(base.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.selectHandler);
		if (audioProfile != null)
		{
			PlaySound(audioProfile.mouseEnterAudioItem);
		}
		UIDLCSelectorOption.UIDLCOptionSelected?.Invoke(this);
	}

	private void OnDeselectedGamepad(IUiNavigationSelectable uiNavigationSelectable)
	{
		_isSelectedByGamepad = false;
		if (audioProfile != null)
		{
			PlaySound(audioProfile.mouseEnterAudioItem);
		}
		ExecuteEvents.Execute(base.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.deselectHandler);
	}

	private void OnToggled(bool isOn)
	{
		SetSelected(isOn);
		OnToggledDLC?.Invoke(DLC, isOn);
	}

	private void SetDLC(DLCRegistry.EDLCKey dlc)
	{
		DLC = dlc;
		_description.text = _dlCsContentInfo.GetDlcDescription(dlc);
		_dlsPromo.sprite = _dlCsContentInfo.GetDlsPromo(dlc);
	}

	public void SetUnavailableDLC(DLCRegistry.EDLCKey dlc)
	{
		toggle.interactable = false;
		SetDLC(dlc);
	}

	public void SetInteractable(bool isInteractable)
	{
		_gamepadToggle.interactable = isInteractable;
		_interactable = isInteractable;
	}

	public void SetAvailableDLC(DLCRegistry.EDLCKey dlc)
	{
		SetDLCState(DLCConfig.EDLCState.Available);
		toggle?.gameObject.SetActive(value: true);
		_dlcEnabledPanel.SetActive(value: true);
		_dlcPurchaseablePanel.SetActive(value: false);
		_dlcComingSoonPanel.SetActive(value: false);
		SetDLC(dlc);
	}

	public void SetPurchaseableDLC(DLCRegistry.EDLCKey dlc)
	{
		SetDLCState(DLCConfig.EDLCState.Unavailable);
		toggle?.gameObject.SetActive(value: false);
		_dlcEnabledPanel.SetActive(value: false);
		_dlcPurchaseablePanel.SetActive(value: true);
		_dlcComingSoonPanel.SetActive(value: false);
		SetDLC(dlc);
	}

	public void SetComingSoonDLC(DLCRegistry.EDLCKey dlc)
	{
		SetDLCState(DLCConfig.EDLCState.ComingSoon);
		toggle?.gameObject.SetActive(value: false);
		_dlcEnabledPanel.SetActive(value: false);
		_dlcPurchaseablePanel.SetActive(value: false);
		_dlcComingSoonPanel.SetActive(value: true);
		SetDLC(dlc);
	}

	public void SetSelected(bool selected)
	{
		if (!InputManager.GamePadInUse)
		{
			toggle.isOn = selected;
		}
		else
		{
			_gamepadToggle.isOn = selected;
		}
	}

	private void SetDLCState(DLCConfig.EDLCState state)
	{
		_dlcState = state;
		this.OnDLCStateChanged?.Invoke();
	}

	protected override void OnLanguageChanged()
	{
		if (description == null)
		{
			Debug.LogWarning("DLC description is null! it will not be translated");
			return;
		}
		description.text = string.Format(LocalizationManager.GetTranslation("GUI_DLC_SELECTOR_DESCR"), LocalizationManager.GetTranslation(DLC.ToString()));
		tooltip.SetText(string.Format(LocalizationManager.GetTranslation((!toggle.interactable) ? "GUI_DLC_SELECTOR_UNAVAILABLE_TOOLTIP" : (toggle.isOn ? "GUI_DLC_SELECTOR_ENABLED_TOOLTIP" : "GUI_DLC_SELECTOR_DISABLED_TOOLTIP")), LocalizationManager.GetTranslation(DLC.ToString())), refreshTooltip: true);
	}

	public void EnableNavigation(bool select)
	{
		toggle.SetNavigation(Navigation.Mode.Automatic);
		if (select)
		{
			toggle.Select();
		}
	}

	public void DisableNavigation()
	{
		if (!InputManager.GamePadInUse)
		{
			toggle.DisableNavigation();
		}
	}

	private void PlaySound(string audioItem)
	{
		if (!string.IsNullOrEmpty(audioItem) && AudioController.GetAudioItem(audioItem) != null)
		{
			AudioControllerUtils.PlaySound(audioItem);
		}
	}
}
