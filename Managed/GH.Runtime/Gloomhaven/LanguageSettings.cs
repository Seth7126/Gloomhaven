using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsmodeeNet.Foundation;
using AsmodeeNet.Foundation.Localization;
using I2.Loc;
using UnityEngine;

namespace Gloomhaven;

public class LanguageSettings : MonoBehaviour
{
	[SerializeField]
	private List<string> m_IgnoredLanguages;

	[SerializeField]
	private ControllerInputAreaLocal m_ControllerArea;

	[SerializeField]
	private LanguageOption _keyboardMouseLanguageOption;

	[SerializeField]
	private LanguageOption _gamepadLanguageOption;

	private LanguageOption _currentLanguageOption;

	private Action _disposeLanguageOptionAction;

	private void Awake()
	{
		m_ControllerArea.OnFocusedArea.AddListener(EnableNavigation);
		m_ControllerArea.OnUnfocusedArea.AddListener(DisableNavigation);
		InitLanguageOption(_keyboardMouseLanguageOption);
		InitLanguageOption(_gamepadLanguageOption);
	}

	private void OnEnable()
	{
		ChooseLanguageOptionByPlatform();
		string currentLanguage = I2.Loc.LocalizationManager.CurrentLanguage;
		_currentLanguageOption.SelectWithoutNotify(currentLanguage);
	}

	public IEnumerator Initialize(bool reloadCardAssets = true)
	{
		I2.Loc.LocalizationManager.CurrentLanguage = SaveData.Instance.RootData.CurrentLanguage;
		if (reloadCardAssets)
		{
			yield return SceneController.Instance.StartCoroutine(SceneController.Instance.ReloadGlobalCardAssets());
		}
	}

	private void EnableNavigation()
	{
		bool flag = SaveData.Instance?.Global != null;
		SetInteractableLanguageOption(_currentLanguageOption, flag);
		if (flag)
		{
			_currentLanguageOption.EnableNavigation();
			_currentLanguageOption.Selectable.Select();
			_disposeLanguageOptionAction = delegate
			{
				_currentLanguageOption.DisableNavigation();
				_currentLanguageOption.Selectable.DisableNavigation();
			};
		}
	}

	private void DisableNavigation()
	{
		_disposeLanguageOptionAction?.Invoke();
		_disposeLanguageOptionAction = null;
	}

	private void ChooseLanguageOptionByPlatform()
	{
		bool gamePadInUse = InputManager.GamePadInUse;
		_currentLanguageOption = (gamePadInUse ? _gamepadLanguageOption : _keyboardMouseLanguageOption);
		_gamepadLanguageOption.gameObject.SetActive(gamePadInUse);
		_keyboardMouseLanguageOption.gameObject.SetActive(!gamePadInUse);
	}

	private void InitLanguageOption(LanguageOption option)
	{
		List<string> languages = I2.Loc.LocalizationManager.GetAllLanguages().Except(m_IgnoredLanguages).ToList();
		option.Initialize(languages);
	}

	private void SetInteractableLanguageOption(LanguageOption option, bool interactable)
	{
		Color textColor = (interactable ? UIInfoTools.Instance.basicTextColor : UIInfoTools.Instance.greyedOutTextColor);
		option.SetInteractable(interactable, textColor);
	}

	public void ApplyLanguage(string language)
	{
		I2.Loc.LocalizationManager.CurrentLanguage = language;
		SaveData.Instance.RootData.CurrentLanguage = language;
		PlatformLayer.Instance.StartupLanguage?.SaveLanguage(language);
		CoreApplication.Instance.LocalizationManager.CurrentLanguage = LanguageStringToEnum(language);
		Task.Run(async delegate
		{
			while (SaveData.Instance.SaveQueue.IsAnyOperationExecuting)
			{
				await Task.Delay(50);
			}
			UnityMainThreadDispatcher.Instance().Enqueue(delegate
			{
				SaveData.Instance.SaveRootData();
			});
		});
		if (PlatformLayer.Instance.IsDelayedInit)
		{
			SaveGlobalSavenOnMachine();
		}
		StartCoroutine(SceneController.Instance.ReloadGlobalCardAssets());
	}

	private AsmodeeNet.Foundation.Localization.LocalizationManager.Language LanguageStringToEnum(string languageString)
	{
		return languageString switch
		{
			"French" => AsmodeeNet.Foundation.Localization.LocalizationManager.Language.fr_FR, 
			"German" => AsmodeeNet.Foundation.Localization.LocalizationManager.Language.de_DE, 
			"Spanish" => AsmodeeNet.Foundation.Localization.LocalizationManager.Language.es_ES, 
			_ => AsmodeeNet.Foundation.Localization.LocalizationManager.Language.en_US, 
		};
	}

	private void SaveGlobalSavenOnMachine()
	{
		string value = JsonUtility.ToJson(SaveData.Instance.RootData);
		PlayerPrefs.SetString("GloomSaven.dat", value);
		PlayerPrefs.Save();
	}
}
