using System;

namespace SM.Gamepad;

public interface ILocalizationClient
{
	event Action OnLanguageChanged;

	string GetTranslation(string key);
}
