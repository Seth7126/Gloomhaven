using System;
using I2.Loc;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

public class I2LocalizationClient : ILocalizationClient
{
	public event Action OnLanguageChanged;

	public I2LocalizationClient()
	{
		LocalizationManager.OnLocalizeEvent += LanguageChanged;
	}

	public string GetTranslation(string key)
	{
		string translation = LocalizationManager.GetTranslation(key);
		if (translation == null)
		{
			return key;
		}
		return translation;
	}

	private void LanguageChanged()
	{
		this.OnLanguageChanged?.Invoke();
	}
}
