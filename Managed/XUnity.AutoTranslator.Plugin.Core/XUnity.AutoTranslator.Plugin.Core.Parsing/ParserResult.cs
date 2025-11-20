using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing;

internal class ParserResult
{
	private static readonly string IgnoredNameEnding = "_i";

	public ParserResultOrigin Origin { get; }

	public string OriginalText { get; }

	public string TranslationTemplate { get; }

	public List<ArgumentedUntranslatedTextInfo> Arguments { get; }

	public Match Match { get; }

	public Regex Regex { get; }

	public bool AllowPartialTranslation { get; }

	public bool CacheCombinedResult { get; }

	public bool PersistCombinedResult { get; }

	public bool PersistTokenResult { get; }

	public int Priority => (int)Origin;

	public ParserResult(ParserResultOrigin origin, string originalText, string template, bool allowPartialTranslation, bool cacheCombinedResult, bool persistCombinedResult, bool persistTokenResult, List<ArgumentedUntranslatedTextInfo> args)
	{
		Origin = origin;
		OriginalText = originalText;
		TranslationTemplate = template;
		AllowPartialTranslation = allowPartialTranslation;
		CacheCombinedResult = cacheCombinedResult;
		PersistCombinedResult = persistCombinedResult;
		PersistTokenResult = persistTokenResult;
		Arguments = args;
	}

	public ParserResult(ParserResultOrigin origin, string originalText, string template, bool allowPartialTranslation, bool cacheCombinedResult, bool persistCombinedResult, bool persistTokenResult, Regex regex, Match match)
	{
		Origin = origin;
		OriginalText = originalText;
		TranslationTemplate = template;
		AllowPartialTranslation = allowPartialTranslation;
		CacheCombinedResult = cacheCombinedResult;
		PersistCombinedResult = persistCombinedResult;
		PersistTokenResult = persistTokenResult;
		Match = match;
		Regex = regex;
	}

	public string GetTranslationFromParts(Func<UntranslatedTextInfo, string> getTranslation)
	{
		bool flag = true;
		StringBuilder stringBuilder = new StringBuilder(TranslationTemplate);
		if (Match != null)
		{
			GroupCollection groups = Match.Groups;
			string[] groupNames = Regex.GetGroupNames();
			for (int num = groupNames.Length - 1; num > 0; num--)
			{
				string text = groupNames[num];
				bool flag2 = text.EndsWith(IgnoredNameEnding);
				int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var result);
				Group obj;
				string oldValue;
				if (result != 0)
				{
					obj = groups[result];
					oldValue = "$" + result;
				}
				else
				{
					obj = groups[text];
					oldValue = "${" + text + "}";
				}
				if (obj.Success)
				{
					string value = obj.Value;
					string text2 = (flag2 ? RomanizationHelper.PostProcess(value, Settings.RegexPostProcessing) : getTranslation(new UntranslatedTextInfo(value)));
					if (text2 != null)
					{
						stringBuilder = stringBuilder.Replace(oldValue, text2);
					}
					else
					{
						flag = false;
					}
				}
				else
				{
					stringBuilder = stringBuilder.Replace(oldValue, string.Empty);
				}
			}
		}
		else
		{
			foreach (ArgumentedUntranslatedTextInfo argument in Arguments)
			{
				string text3 = getTranslation(argument.Info);
				if (text3 != null)
				{
					stringBuilder = stringBuilder.Replace(argument.Key, text3);
				}
				else
				{
					flag = false;
				}
			}
		}
		if (flag)
		{
			return stringBuilder.ToString();
		}
		return null;
	}
}
