using System;
using System.Linq;
using Assets.Script.Misc;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using Platforms.Profanity;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI;
using Script.GUI.Controller;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.MainMenuStates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UICharacterCreatorNameStep : UICharacterCreatorStep<string>
{
	[SerializeField]
	private TMP_InputField nameInputField;

	[SerializeField]
	private EscapableGameObject keyboardEscapable;

	[SerializeField]
	private ControllerInputKeyboard keyboardBehaviour;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private GameObject confirmWarning;

	[SerializeField]
	private HelpBoxLine warningHelpText;

	[SerializeField]
	private LocalHotkeys _hotkeyContainer;

	private CMapCharacter character;

	private IHotkeySession _hotkeySession;

	private SessionHotkey _backHotkey;

	private SessionHotkey _selectHotkey;

	private SessionHotkey _renameHotkey;

	private bool _isValid;

	private bool HasName => nameInputField.text.IsNOTNullOrEmpty();

	protected override void Awake()
	{
		base.Awake();
		keyboardEscapable.OnEscaped.AddListener(OnKeyboardEscaped);
		nameInputField.onValueChanged.AddListener(delegate(string text)
		{
			ChangeHotkeySelect();
			string text2 = new string(text.Where((char it) => !GlobalData.UnsupportedCharacters.Contains(it)).ToArray()).ToTitleCase();
			nameInputField.text = text2;
			string nameTrimed = text2.Trim();
			PlatformLayer.ProfanityFilter.CheckBadWordsAsync(nameTrimed, delegate(OperationResult opRes, bool foundBadWords)
			{
				if (nameTrimed.IsNullOrEmpty())
				{
					EnableConfirmationButton(enable: false);
					warningHelpText.Hide();
					_isValid = false;
				}
				else if (IsNameRepeated(nameTrimed))
				{
					EnableConfirmationButton(enable: false);
					warningHelpText.ShowWarning("GUI_CHARACTER_NAME_REPEATED");
					_isValid = false;
				}
				else if (foundBadWords)
				{
					EnableConfirmationButton(enable: false);
					warningHelpText.ShowWarning("CONSOLES/GUI_CHARACTER_INVALID_NAME");
					_isValid = false;
				}
				else if (nameInputField.text.Count(char.IsDigit) > 6)
				{
					EnableConfirmationButton(enable: false);
					warningHelpText.ShowWarning("CONSOLES/GUI_CHARACTER_NAME_SIX_DIGITS");
					_isValid = false;
				}
				else
				{
					EnableConfirmationButton(enable: true);
					warningHelpText.Hide();
					_isValid = true;
				}
				ChangeHotkeySelect();
				confirmWarning.SetActive(value: false);
			});
		});
		keyboardBehaviour.OnPlatformSpecificCallback += OnKeyboardEscaped;
	}

	protected override void OnDestroy()
	{
		nameInputField.onValueChanged.RemoveAllListeners();
		keyboardBehaviour.OnPlatformSpecificCallback -= OnKeyboardEscaped;
		nameInputField.onValueChanged.RemoveAllListeners();
		ClearAll();
	}

	private void ShowKeyboard()
	{
		if (window.IsOpen)
		{
			if (Singleton<UINavigation>.Instance.StateMachine.CurrentState is MainMenuState)
			{
				return;
			}
			keyboardBehaviour.enabled = true;
			if (!PlatformLayer.Instance.IsConsole && _hotkeyContainer != null)
			{
				_hotkeyContainer.gameObject.SetActive(value: false);
			}
		}
		ChangeHotkeySelect();
	}

	private void OnKeyboardEscaped()
	{
		keyboardBehaviour.enabled = false;
		if (_hotkeyContainer != null)
		{
			_hotkeyContainer.gameObject.SetActive(value: true);
		}
		ChangeHotkeySelect();
	}

	public bool IsNameRepeated(string characterName)
	{
		if (!AdventureState.MapState.MapParty.CheckCharacters.Exists((CMapCharacter it) => characterName.Equals(it.CharacterName, StringComparison.OrdinalIgnoreCase)))
		{
			return AdventureState.MapState.MapParty.RetiredCharacterRecords.Exists((CPlayerRecords it) => characterName.Equals(it.CharacterName, StringComparison.OrdinalIgnoreCase));
		}
		return true;
	}

	public ICallbackPromise<string> Show(ICharacterCreatorClass character, string currentName = null)
	{
		titleText.text = LocalizationManager.GetTranslation(character.Data.LocKey) + ":";
		nameInputField.text = currentName;
		bool flag = Singleton<MapFTUEManager>.Instance != null && !Singleton<MapFTUEManager>.Instance.HasCompletedStep(EMapFTUEStep.CreatedFirstCharacter);
		InitializeHotkeys();
		_backHotkey.SetShown(!flag);
		if (flag)
		{
			window.escapeKeyAction = UIWindow.EscapeKeyAction.Skip;
		}
		else
		{
			window.escapeKeyAction = UIWindow.EscapeKeyAction.HideOnlyThis;
		}
		cancelButton.gameObject.SetActive(!flag && !InputManager.GamePadInUse);
		ChangeHotkeySelect();
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.CreateNameStep);
		return ProcessStep();
	}

	public void IsValid()
	{
		Validate(delegate
		{
		});
	}

	protected override void Validate(Action<bool> callback)
	{
		if (nameInputField == null)
		{
			callback(obj: false);
			_isValid = false;
			return;
		}
		if (nameInputField.text.Trim().IsNullOrEmpty())
		{
			confirmWarning.SetActive(value: true);
			callback(obj: false);
			_isValid = false;
			return;
		}
		if (IsNameRepeated(nameInputField.text))
		{
			confirmWarning.SetActive(value: true);
			warningHelpText.HighlightWarning();
			callback(obj: false);
			_isValid = false;
			return;
		}
		PlatformLayer.ProfanityFilter.CheckBadWordsAsync(nameInputField.text, delegate(OperationResult opRes, bool foundBadWords)
		{
			if (foundBadWords)
			{
				confirmWarning.SetActive(value: true);
				warningHelpText.HighlightWarning();
				callback(obj: false);
				_isValid = false;
			}
			else if (nameInputField.text.Count(char.IsDigit) > 6)
			{
				confirmWarning.SetActive(value: true);
				warningHelpText.HighlightWarning();
				callback(obj: false);
				_isValid = false;
			}
			else
			{
				warningHelpText.Hide();
				confirmWarning.SetActive(value: false);
				_isValid = true;
				base.Validate(callback);
			}
		});
	}

	protected override void OnHidden()
	{
		window.escapeKeyAction = UIWindow.EscapeKeyAction.None;
		base.OnHidden();
		RemoveSelectBlockerForConfirm();
		OnKeyboardEscaped();
		ClearAll();
	}

	protected override string GetSelectedValue()
	{
		return nameInputField.text;
	}

	private void InitializeHotkeys()
	{
		_hotkeySession = _hotkeyContainer.GetSessionOrEmpty().GetHotkey(out _backHotkey, "Back").GetHotkey(out _selectHotkey, "Select")
			.GetHotkey(out _renameHotkey, "Rename");
	}

	private void DisposeHotkeys()
	{
		if (_hotkeySession != null)
		{
			_selectHotkey.Dispose();
			_renameHotkey.Dispose();
			_hotkeySession.Dispose();
			_hotkeySession = null;
		}
	}

	private void ChangeHotkeySelect()
	{
		if (InputManager.GamePadInUse)
		{
			bool hasName = HasName;
			if (_hotkeySession != null)
			{
				_renameHotkey.SetShown(hasName);
				_selectHotkey.SetShown(!hasName);
			}
			KeyAction keyAction = (hasName ? KeyAction.UI_RENAME : KeyAction.UI_SUBMIT);
			int keyAction2 = (hasName ? 41 : 300);
			InputManager.RegisterToOnPressed(keyAction, ShowKeyboard);
			InputManager.UnregisterToOnPressed((KeyAction)keyAction2, ShowKeyboard);
			bool active = hasName && !keyboardBehaviour.enabled && _isValid;
			longConfirmHandler.gameObject.SetActive(active);
		}
	}

	private void ClearAll()
	{
		DisposeHotkeys();
		InputManager.UnregisterToOnPressed(KeyAction.UI_SUBMIT, ShowKeyboard);
		InputManager.UnregisterToOnPressed(KeyAction.UI_RENAME, ShowKeyboard);
	}

	protected override void OnControllerAreaFocused()
	{
		base.OnControllerAreaFocused();
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.CreateNameStep);
	}

	protected override void ShowLongConfirm()
	{
	}

	protected override void HideLongConfirm()
	{
	}
}
