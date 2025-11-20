using System;
using System.Collections.Generic;
using GLOOM;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gloomhaven;

public class DropdownLanguageOption : LanguageOption
{
	[SerializeField]
	private TMP_Dropdown _dropdown;

	private SelectorWrapper<string> _selector;

	public override Selectable Selectable => _dropdown;

	private void OnDestroy()
	{
		_selector.OnValuedChanged.RemoveAllListeners();
	}

	public override void EnableNavigation()
	{
		base.EnableNavigation();
		_dropdown.SetNavigation(Navigation.Mode.Vertical);
	}

	public override void SetInteractable(bool interactable, Color textColor)
	{
		_dropdown.interactable = interactable;
		_dropdown.captionText.color = textColor;
	}

	public override void Initialize(List<string> languages)
	{
		_selector = new SelectorWrapper<string>(_dropdown, null);
		_selector.OnValuedChanged.AddListener(OnLanguageSelected);
		string Translation;
		List<SelectorOptData<string>> options = languages.ConvertAll((string it) => new SelectorOptData<string>(it, () => (!GLOOM.LocalizationManager.TryGetTranslation(it, out Translation)) ? it : Translation));
		_selector.SetOptions(options);
	}

	public override void SelectWithoutNotify(string language)
	{
		_selector.SetValueWithoutNotify(language);
	}

	private void OnLanguageSelected(string language)
	{
		if (CanChangeLanguage(language))
		{
			List<ErrorMessage.LabelAction> buttons = new List<ErrorMessage.LabelAction>
			{
				new ErrorMessage.LabelAction("GUI_CANCEL", delegate
				{
					_selector.SetValueWithoutNotify(I2.Loc.LocalizationManager.CurrentLanguage);
				}, KeyAction.UI_CANCEL),
				new ErrorMessage.LabelAction("GUI_APPLY_AND_LOAD", delegate
				{
					_settings.ApplyLanguage(language);
					_selector.RefreshTexts();
				}, KeyAction.UI_SUBMIT)
			};
			SceneController.Instance.GlobalErrorMessage.ShowGenericMessage("GUI_OPT_LANG_TITLE", "GUI_OPT_LANG_APPLY", Environment.StackTrace, buttons);
		}
	}
}
