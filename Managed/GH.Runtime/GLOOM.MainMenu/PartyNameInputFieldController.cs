using System;
using System.Linq;
using Platforms.Profanity;
using SM.Gamepad;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GLOOM.MainMenu;

public class PartyNameInputFieldController : MonoBehaviour
{
	private const string RepeatedCampaignPartyNameErrorLocKey = "GUI_CAMPAIGN_REPEATED_NAME";

	private const string InvalidCampaignPartyNameErrorLocKey = "GUI_CAMPAIGN_INVALID_NAME";

	private const string NumbersInARowCampaignPartyNameErrorLocKey = "GUI_CAMPAIGN_INVALID_NAME";

	private const string RepeatedGuildmasterPartyNameErrorLocKey = "GUI_GUILDMASTER_REPEATED_NAME";

	private const string InvalidGuildmasterPartyNameErrorLocKey = "GUI_GUILDMASTER_INVALID_NAME";

	private const string NumbersInARowGuildmasterPartyNameErrorLocKey = "GUI_GUILDMASTER_INVALID_NAME";

	[SerializeField]
	private TMP_InputField _partyNameInputField;

	[SerializeField]
	private TextMeshProUGUI _warningText;

	[SerializeField]
	private HelpBoxLine _helpBoxLine;

	[SerializeField]
	private GameObject _frame;

	private Color _defaultInputColor;

	private IGameModeService _gameModeService;

	private bool _isNameSelected;

	private UINavigationSelectable _uiNavigationSelectable;

	public bool IsSelected { get; private set; }

	public TMP_InputField PartyNameInputField => _partyNameInputField;

	public event Action NameInputFieldSelected;

	public event Action NameInputFieldDeselected;

	public event Action<bool> NameValidated;

	private void Awake()
	{
		_defaultInputColor = _partyNameInputField.textComponent.color;
		_uiNavigationSelectable = GetComponent<UINavigationSelectable>();
		SubscribeOnEvents();
	}

	protected void OnDestroy()
	{
		if (InputManager.GamePadInUse)
		{
			_uiNavigationSelectable.OnNavigationSelectedEvent -= OnInputFieldSelected;
			_uiNavigationSelectable.OnNavigationDeselectedEvent -= OnInputFieldDeselected;
		}
		_partyNameInputField.onValueChanged.RemoveListener(ValidateName);
		_partyNameInputField.onSelect.RemoveAllListeners();
	}

	public void InitializeGameModeService(IGameModeService service)
	{
		_gameModeService = service;
	}

	public void SetWarningText(string text)
	{
		_warningText.text = text;
	}

	public void SetActiveWarning(bool isActive)
	{
		_warningText.gameObject.SetActive(isActive);
	}

	private void ClearWarning()
	{
		_partyNameInputField.textComponent.color = _defaultInputColor;
		HideHelpBox();
	}

	public void ShowHelpBoxLine(HelpBoxLineErrorType helpBoxLineErrorType, HelpBox.FormatTarget useWarningFormat = HelpBox.FormatTarget.NONE)
	{
		bool flag = _gameModeService.GetType() == typeof(CampaignModeService);
		switch (helpBoxLineErrorType)
		{
		case HelpBoxLineErrorType.None:
			throw new Exception("Need set error type");
		case HelpBoxLineErrorType.Repeated:
			_helpBoxLine.Show(flag ? "GUI_CAMPAIGN_REPEATED_NAME" : "GUI_GUILDMASTER_REPEATED_NAME", null, null, useWarningFormat);
			break;
		case HelpBoxLineErrorType.Invalid:
			_helpBoxLine.Show(flag ? "GUI_CAMPAIGN_INVALID_NAME" : "GUI_GUILDMASTER_INVALID_NAME", null, null, useWarningFormat);
			break;
		case HelpBoxLineErrorType.NumbersInARow:
			_helpBoxLine.Show(flag ? "GUI_CAMPAIGN_INVALID_NAME" : "GUI_GUILDMASTER_INVALID_NAME", null, null, useWarningFormat);
			break;
		default:
			throw new ArgumentOutOfRangeException("helpBoxLineErrorType", helpBoxLineErrorType, null);
		}
	}

	public void HideHelpBox()
	{
		_helpBoxLine.Hide();
	}

	public void HighlightWarningHelpBox()
	{
		_helpBoxLine.HighlightWarning();
	}

	public void FocusOnInputFieldIfNeed()
	{
		if (_partyNameInputField.text.IsNullOrEmpty())
		{
			_partyNameInputField.ActivateInputField();
			_partyNameInputField.Select();
		}
	}

	public void ValidateName(string partyName)
	{
		SetActiveWarning(isActive: false);
		if (!string.IsNullOrWhiteSpace(partyName))
		{
			partyName = partyName.ToTitleCase();
			foreach (char item in GlobalData.UnsupportedCharacters.Where((char a) => partyName.Contains(a)))
			{
				partyName = partyName.RemoveChar(item);
			}
			if (partyName.Length >= 2 && partyName[^1] == ' ' && partyName[^2] == ' ')
			{
				partyName = partyName[..^1];
			}
			_partyNameInputField.text = partyName;
		}
		bool isValid = true;
		PlatformLayer.ProfanityFilter.CheckBadWordsAsync(partyName, delegate(OperationResult opRes, bool foundBadWords)
		{
			if (foundBadWords)
			{
				isValid = false;
				ShowHelpBoxLine(HelpBoxLineErrorType.Invalid, HelpBox.FormatTarget.ALL);
				_partyNameInputField.textComponent.color = _defaultInputColor;
				SetWarningText(partyName);
			}
			else if (string.IsNullOrWhiteSpace(partyName))
			{
				isValid = false;
				_partyNameInputField.textComponent.color = _defaultInputColor;
			}
			else if (_gameModeService.ValidateExistsGame(partyName))
			{
				isValid = false;
				ShowHelpBoxLine(HelpBoxLineErrorType.Repeated, HelpBox.FormatTarget.ALL);
				_partyNameInputField.textComponent.color = UIInfoTools.Instance.warningColor;
				SetWarningText(partyName);
			}
			else if (partyName.Count(char.IsDigit) > 6)
			{
				isValid = false;
				ShowHelpBoxLine(HelpBoxLineErrorType.NumbersInARow, HelpBox.FormatTarget.ALL);
				_partyNameInputField.textComponent.color = UIInfoTools.Instance.warningColor;
				SetWarningText(partyName);
			}
			else
			{
				ClearWarning();
			}
			bool flag = string.IsNullOrEmpty(_partyNameInputField.text);
			SetActiveWarning(!flag && !isValid);
			this.NameValidated?.Invoke(isValid);
		});
	}

	public void SetInputFieldText(string text)
	{
		_partyNameInputField.text = text;
	}

	public string GetInputFieldText()
	{
		return _partyNameInputField.text;
	}

	public bool InputFieldIsCurrentSelectable()
	{
		return EventSystem.current.currentSelectedGameObject == _partyNameInputField.gameObject;
	}

	public string GetPlaceholderText()
	{
		return (_partyNameInputField.placeholder as TMP_Text)?.text;
	}

	public void IsValidName(Action<bool> callback)
	{
		string partyName = GetInputFieldText();
		if (string.IsNullOrWhiteSpace(partyName))
		{
			callback(obj: false);
			return;
		}
		if (_gameModeService.ValidateExistsGame(partyName))
		{
			HighlightWarningHelpBox();
			SetWarningText(partyName);
			callback(obj: false);
			return;
		}
		PlatformLayer.ProfanityFilter.CheckBadWordsAsync(partyName, delegate(OperationResult opRes, bool foundBadWords)
		{
			if (foundBadWords)
			{
				HighlightWarningHelpBox();
				SetWarningText(partyName);
				callback(obj: false);
			}
			else if (partyName.Count(char.IsDigit) > 6)
			{
				HighlightWarningHelpBox();
				SetWarningText(partyName);
				callback(obj: false);
			}
			else
			{
				callback(obj: true);
			}
		});
	}

	private void SubscribeOnEvents()
	{
		if (InputManager.GamePadInUse)
		{
			_uiNavigationSelectable.OnNavigationSelectedEvent += OnInputFieldSelected;
			_uiNavigationSelectable.OnNavigationDeselectedEvent += OnInputFieldDeselected;
		}
		_partyNameInputField.onValueChanged.AddListener(ValidateName);
	}

	private void OnInputFieldSelected(IUiNavigationSelectable uiNavigationSelectable)
	{
		_frame.SetActive(value: true);
		IsSelected = true;
		this.NameInputFieldSelected?.Invoke();
	}

	private void OnInputFieldDeselected(IUiNavigationSelectable uiNavigationSelectable)
	{
		_frame.SetActive(value: false);
		IsSelected = false;
		this.NameInputFieldDeselected?.Invoke();
	}
}
