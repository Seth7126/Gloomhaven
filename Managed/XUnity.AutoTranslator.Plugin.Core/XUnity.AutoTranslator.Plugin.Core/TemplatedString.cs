using System.Collections.Generic;

namespace XUnity.AutoTranslator.Plugin.Core;

internal class TemplatedString
{
	public string Template { get; private set; }

	public Dictionary<string, string> Arguments { get; private set; }

	public TemplatedString(string template, Dictionary<string, string> arguments)
	{
		Template = template;
		Arguments = arguments;
	}

	public string Untemplate(string text)
	{
		foreach (KeyValuePair<string, string> argument in Arguments)
		{
			text = text.Replace(argument.Key, argument.Value);
		}
		return text;
	}

	public string PrepareUntranslatedText(string untranslatedText)
	{
		foreach (KeyValuePair<string, string> argument in Arguments)
		{
			string key = argument.Key;
			string newValue = CreateTranslatorFriendlyKey(key);
			untranslatedText = untranslatedText.Replace(key, newValue);
		}
		return untranslatedText;
	}

	public string FixTranslatedText(string translatedText, bool useTranslatorFriendlyArgs)
	{
		foreach (KeyValuePair<string, string> argument in Arguments)
		{
			string key = argument.Key;
			string translatorFriendlyKey = (useTranslatorFriendlyArgs ? CreateTranslatorFriendlyKey(key) : key);
			translatedText = ReplaceApproximateMatches(translatedText, translatorFriendlyKey, key);
		}
		return translatedText;
	}

	public static string CreateTranslatorFriendlyKey(string key)
	{
		char c = key[2];
		return "ZM" + (char)(c + 2) + "Z";
	}

	public static string ReplaceApproximateMatches(string translatedText, string translatorFriendlyKey, string key)
	{
		int num = translatorFriendlyKey.Length - 1;
		int num2 = num;
		int num3 = num;
		for (int num4 = translatedText.Length - 1; num4 >= 0; num4--)
		{
			char c = translatedText[num4];
			if (c != ' ' && c != '\u3000')
			{
				if ((c = char.ToUpperInvariant(c)) == char.ToUpperInvariant(translatorFriendlyKey[num2]) || c == char.ToUpperInvariant(translatorFriendlyKey[num2 = num]))
				{
					if (num2 == num)
					{
						num3 = num4;
					}
					num2--;
				}
				if (num2 < 0)
				{
					int count = num3 + 1 - num4;
					translatedText = translatedText.Remove(num4, count).Insert(num4, key);
					num2 = num;
				}
			}
		}
		return translatedText;
	}
}
