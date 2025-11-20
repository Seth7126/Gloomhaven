using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core;

public static class TranslationHelper
{
	private static Dictionary<string, HashSet<string>> _registrations = new Dictionary<string, HashSet<string>>();

	private static List<string> LookupRegistration(IEnumerable<UntranslatedText> keys)
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (UntranslatedText key in keys)
		{
			if (_registrations.TryGetValue(key.TemplatedOriginal_Text, out var value))
			{
				hashSet.AddRange(value);
			}
			if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_ExternallyTrimmed && _registrations.TryGetValue(key.TemplatedOriginal_Text_ExternallyTrimmed, out value))
			{
				hashSet.AddRange(value);
			}
			if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_InternallyTrimmed && _registrations.TryGetValue(key.TemplatedOriginal_Text_InternallyTrimmed, out value))
			{
				hashSet.AddRange(value);
			}
			if ((object)key.TemplatedOriginal_Text_InternallyTrimmed != key.TemplatedOriginal_Text_FullyTrimmed && _registrations.TryGetValue(key.TemplatedOriginal_Text_FullyTrimmed, out value))
			{
				hashSet.AddRange(value);
			}
		}
		List<string> list = hashSet.ToList();
		list.Sort();
		return list;
	}

	internal static void DisplayTranslationInfo(string originalText, string translatedText)
	{
		if (!Settings.EnableTranslationHelper)
		{
			return;
		}
		UntranslatedText item = CreateTextKey(originalText);
		List<UntranslatedText> list = new List<UntranslatedText> { item };
		if (translatedText != null)
		{
			UntranslatedText item2 = CreateTextKey(translatedText);
			list.Add(item2);
		}
		List<string> list2 = LookupRegistration(list);
		if (list2.Count <= 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine();
		if (translatedText != null)
		{
			stringBuilder.Append("For the text \"").Append(originalText).AppendLine("\", which was translated to \"" + translatedText + "\" found the following potential source files:");
		}
		else
		{
			stringBuilder.Append("For the text \"").Append(originalText).AppendLine("\" found the following potential source files:");
		}
		foreach (string item3 in list2)
		{
			stringBuilder.Append("  ").AppendLine(item3);
		}
		XuaLogger.AutoTranslator.Info(stringBuilder.ToString());
	}

	public static void RegisterRedirectedResourceTextToPath(string text, string virtualFilePath)
	{
		if (Settings.EnableTranslationHelper)
		{
			AssociateTextKeyWithFile(CreateTextKey(text), virtualFilePath);
		}
	}

	private static UntranslatedText CreateTextKey(string text)
	{
		if (LanguageHelper.IsTranslatable(text))
		{
			return new UntranslatedText(text, isFromSpammingComponent: false, removeInternalWhitespace: true, Settings.FromLanguageUsesWhitespaceBetweenWords, enableTemplating: true, Settings.TemplateAllNumberAway);
		}
		return new UntranslatedText(text, isFromSpammingComponent: false, removeInternalWhitespace: true, Settings.ToLanguageUsesWhitespaceBetweenWords, enableTemplating: true, Settings.TemplateAllNumberAway);
	}

	private static void AssociateTextKeyWithFile(UntranslatedText key, string virtualFilePath)
	{
		AssociateTextWithFile(key.TemplatedOriginal_Text, virtualFilePath);
		if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_ExternallyTrimmed)
		{
			AssociateTextWithFile(key.TemplatedOriginal_Text_ExternallyTrimmed, virtualFilePath);
		}
		if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_InternallyTrimmed)
		{
			AssociateTextWithFile(key.TemplatedOriginal_Text_InternallyTrimmed, virtualFilePath);
		}
		if ((object)key.TemplatedOriginal_Text_InternallyTrimmed != key.TemplatedOriginal_Text_FullyTrimmed)
		{
			AssociateTextWithFile(key.TemplatedOriginal_Text_FullyTrimmed, virtualFilePath);
		}
	}

	private static void AssociateTextWithFile(string text, string virtualFilePath)
	{
		if (!_registrations.TryGetValue(text, out var value))
		{
			value = new HashSet<string>();
			_registrations[text] = value;
		}
		value.Add(virtualFilePath);
	}
}
