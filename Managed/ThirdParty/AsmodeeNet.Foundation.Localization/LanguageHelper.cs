using System;

namespace AsmodeeNet.Foundation.Localization;

public static class LanguageHelper
{
	public static string ToXsdLanguage(this LocalizationManager.Language lang)
	{
		return lang.ToString().Replace("_", "-");
	}

	public static LocalizationManager.Language LanguageFromXsdLanguage(string xsdLang)
	{
		if (string.IsNullOrEmpty(xsdLang))
		{
			return LocalizationManager.Language.unknown;
		}
		try
		{
			return (LocalizationManager.Language)Enum.Parse(typeof(LocalizationManager.Language), xsdLang.Replace("-", "_"));
		}
		catch
		{
			return LocalizationManager.Language.unknown;
		}
	}
}
