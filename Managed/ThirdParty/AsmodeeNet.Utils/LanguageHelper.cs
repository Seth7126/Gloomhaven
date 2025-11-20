using UnityEngine;

namespace AsmodeeNet.Utils;

public class LanguageHelper
{
	public static string Get2LetterISOCodeFromSystemLanguage(SystemLanguage language)
	{
		string result = "en";
		switch (language)
		{
		case SystemLanguage.Afrikaans:
			result = "af";
			break;
		case SystemLanguage.Arabic:
			result = "ar";
			break;
		case SystemLanguage.Basque:
			result = "eu";
			break;
		case SystemLanguage.Belarusian:
			result = "by";
			break;
		case SystemLanguage.Bulgarian:
			result = "bg";
			break;
		case SystemLanguage.Catalan:
			result = "ca";
			break;
		case SystemLanguage.Chinese:
			result = "zh";
			break;
		case SystemLanguage.Czech:
			result = "cs";
			break;
		case SystemLanguage.Danish:
			result = "da";
			break;
		case SystemLanguage.Dutch:
			result = "nl";
			break;
		case SystemLanguage.English:
			result = "en";
			break;
		case SystemLanguage.Estonian:
			result = "et";
			break;
		case SystemLanguage.Faroese:
			result = "fo";
			break;
		case SystemLanguage.Finnish:
			result = "fi";
			break;
		case SystemLanguage.French:
			result = "fr";
			break;
		case SystemLanguage.German:
			result = "de";
			break;
		case SystemLanguage.Greek:
			result = "el";
			break;
		case SystemLanguage.Hebrew:
			result = "iw";
			break;
		case SystemLanguage.Hungarian:
			result = "hu";
			break;
		case SystemLanguage.Icelandic:
			result = "is";
			break;
		case SystemLanguage.Indonesian:
			result = "in";
			break;
		case SystemLanguage.Italian:
			result = "it";
			break;
		case SystemLanguage.Japanese:
			result = "ja";
			break;
		case SystemLanguage.Korean:
			result = "ko";
			break;
		case SystemLanguage.Latvian:
			result = "lv";
			break;
		case SystemLanguage.Lithuanian:
			result = "lt";
			break;
		case SystemLanguage.Norwegian:
			result = "no";
			break;
		case SystemLanguage.Polish:
			result = "pl";
			break;
		case SystemLanguage.Portuguese:
			result = "pt";
			break;
		case SystemLanguage.Romanian:
			result = "ro";
			break;
		case SystemLanguage.Russian:
			result = "ru";
			break;
		case SystemLanguage.SerboCroatian:
			result = "sh";
			break;
		case SystemLanguage.Slovak:
			result = "sk";
			break;
		case SystemLanguage.Slovenian:
			result = "sl";
			break;
		case SystemLanguage.Spanish:
			result = "es";
			break;
		case SystemLanguage.Swedish:
			result = "sv";
			break;
		case SystemLanguage.Thai:
			result = "th";
			break;
		case SystemLanguage.Turkish:
			result = "tr";
			break;
		case SystemLanguage.Ukrainian:
			result = "uk";
			break;
		case SystemLanguage.Unknown:
			result = "en";
			break;
		case SystemLanguage.Vietnamese:
			result = "vi";
			break;
		}
		return result;
	}
}
