using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Shims;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities;

internal static class RomanizationHelper
{
	public static string PostProcess(string text, TextPostProcessing postProcessing)
	{
		if ((postProcessing & TextPostProcessing.ReplaceHtmlEntities) != TextPostProcessing.None)
		{
			text = WebUtility.HtmlDecode(text);
		}
		if ((postProcessing & TextPostProcessing.ReplaceMacronWithCircumflex) != TextPostProcessing.None)
		{
			text = ConvertMacronToCircumflex(text);
		}
		if ((postProcessing & TextPostProcessing.RemoveAllDiacritics) != TextPostProcessing.None)
		{
			text = text.RemoveAllDiacritics();
		}
		if ((postProcessing & TextPostProcessing.RemoveApostrophes) != TextPostProcessing.None)
		{
			text = RemoveNApostrophe(text);
		}
		if ((postProcessing & TextPostProcessing.ReplaceWideCharacters) != TextPostProcessing.None)
		{
			text = ReplaceWideCharacters(text);
		}
		return text;
	}

	public static string ReplaceWideCharacters(string input)
	{
		StringBuilder stringBuilder = new StringBuilder(input);
		int length = input.Length;
		bool flag = false;
		for (int i = 0; i < length; i++)
		{
			char c = stringBuilder[i];
			if (c >= '\uff00' && c <= '～')
			{
				flag = true;
				stringBuilder[i] = (char)(c - 65248);
			}
			if (c == '\u3000')
			{
				flag = true;
				stringBuilder[i] = ' ';
			}
		}
		if (!flag)
		{
			return input;
		}
		return stringBuilder.ToString();
	}

	public static string ConvertMacronToCircumflex(string romanizedJapaneseText)
	{
		StringBuilder stringBuilder = new StringBuilder(romanizedJapaneseText.Length);
		foreach (char c in romanizedJapaneseText)
		{
			switch (c)
			{
			case 'Ā':
				stringBuilder.Append('Â');
				break;
			case 'ā':
				stringBuilder.Append('â');
				break;
			case 'Ī':
				stringBuilder.Append('Î');
				break;
			case 'ī':
				stringBuilder.Append('î');
				break;
			case 'Ū':
				stringBuilder.Append('Û');
				break;
			case 'ū':
				stringBuilder.Append('û');
				break;
			case 'Ē':
				stringBuilder.Append('Ê');
				break;
			case 'ē':
				stringBuilder.Append('ê');
				break;
			case 'Ō':
				stringBuilder.Append('Ô');
				break;
			case 'ō':
				stringBuilder.Append('ô');
				break;
			default:
				stringBuilder.Append(c);
				break;
			}
		}
		return stringBuilder.ToString();
	}

	public static string RemoveNApostrophe(string romanizedJapaneseText)
	{
		return romanizedJapaneseText.Replace("n'", "n").Replace("n’", "n");
	}
}
