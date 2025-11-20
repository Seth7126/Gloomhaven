using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing;

internal class RichTextParser
{
	private static readonly char[] TagNameEnders = new char[2] { '=', ' ' };

	private static readonly Regex TagRegex = new Regex("(<.*?>)", AutoTranslationPlugin.RegexCompiledSupportedFlag);

	private static readonly HashSet<string> IgnoreTags = new HashSet<string> { "ruby", "group" };

	private static readonly HashSet<string> KnownTags = new HashSet<string>
	{
		"b", "i", "size", "color", "em", "sup", "sub", "dash", "space", "u",
		"strike", "param", "format", "emoji", "speed", "sound", "line-height"
	};

	public bool CanApply(object ui)
	{
		if (Settings.HandleRichText)
		{
			return ui.SupportsRichText();
		}
		return false;
	}

	private static bool IsAllLatin(string value, int endIdx)
	{
		for (int i = 0; i < endIdx; i++)
		{
			char c = value[i];
			if ((c < 'A' || c > 'Z') && (c < 'a' || c > 'z') && c != '-' && c != '_')
			{
				return false;
			}
		}
		return true;
	}

	private static bool StartsWithPound(string value, int endIdx)
	{
		if (0 < value.Length)
		{
			return value[0] == '#';
		}
		return false;
	}

	public ParserResult Parse(string input, int scope)
	{
		if (!Settings.HandleRichText)
		{
			return null;
		}
		List<ArgumentedUntranslatedTextInfo> list = new List<ArgumentedUntranslatedTextInfo>();
		StringBuilder stringBuilder = new StringBuilder(input.Length);
		char c = 'A';
		bool flag = false;
		string[] array = TagRegex.Split(input);
		foreach (string text in array)
		{
			if (text.Length <= 0)
			{
				continue;
			}
			bool flag2 = text[0] == '<' && text[text.Length - 1] == '>';
			bool num = flag2 && text.Length > 1 && text[1] == '/';
			string text2 = null;
			if (num)
			{
				text2 = text.Substring(2, text.Length - 3);
			}
			else if (flag2)
			{
				text2 = text.Substring(1, text.Length - 2);
			}
			if (flag2)
			{
				string[] array2 = text2.Split(TagNameEnders);
				string text3 = ((array2.Length < 2) ? text2 : array2[0]);
				int length = text3.Length;
				if (KnownTags.Contains(text3) || (IsAllLatin(text3, length) && !IgnoreTags.Contains(text3)) || StartsWithPound(text3, length))
				{
					stringBuilder.Append(text);
					flag = false;
				}
				continue;
			}
			if (flag)
			{
				list[list.Count - 1].Info.UntranslatedText += text;
			}
			else
			{
				string text4 = "[[" + c++ + "]]";
				ArgumentedUntranslatedTextInfo item = new ArgumentedUntranslatedTextInfo
				{
					Key = text4,
					Info = new UntranslatedTextInfo(text)
				};
				list.Add(item);
				stringBuilder.Append(text4);
			}
			flag = true;
		}
		for (int j = 0; j < list.Count; j++)
		{
			ArgumentedUntranslatedTextInfo argumentedUntranslatedTextInfo = list[j];
			for (int k = 0; k < j; k++)
			{
				string untranslatedText = list[k].Info.UntranslatedText;
				if (!untranslatedText.IsNullOrWhiteSpace())
				{
					argumentedUntranslatedTextInfo.Info.ContextBefore.Add(untranslatedText);
				}
			}
			for (int l = j + 1; l < list.Count; l++)
			{
				string untranslatedText2 = list[l].Info.UntranslatedText;
				if (!untranslatedText2.IsNullOrWhiteSpace())
				{
					argumentedUntranslatedTextInfo.Info.ContextAfter.Add(untranslatedText2);
				}
			}
		}
		string text5 = stringBuilder.ToString();
		if (c == 'A' || text5.Length <= 5)
		{
			return null;
		}
		return new ParserResult(ParserResultOrigin.RichTextParser, input, text5, allowPartialTranslation: false, cacheCombinedResult: true, Settings.PersistRichTextMode == PersistRichTextMode.Final, Settings.PersistRichTextMode == PersistRichTextMode.Fragment, list);
	}
}
