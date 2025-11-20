using System.Collections.Generic;
using System.IO;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing;

internal class GameLogTextParser
{
	public bool CanApply(object ui)
	{
		return ui.SupportsLineParser();
	}

	public ParserResult Parse(string input, int scope, IReadOnlyTextTranslationCache cache)
	{
		StringReader stringReader = new StringReader(input);
		bool flag = false;
		StringBuilder stringBuilder = new StringBuilder(input.Length);
		List<ArgumentedUntranslatedTextInfo> list = new List<ArgumentedUntranslatedTextInfo>();
		char c = 'A';
		string text = null;
		while ((text = stringReader.ReadLine()) != null)
		{
			if (!string.IsNullOrEmpty(text))
			{
				if (cache.IsTranslatable(text, isToken: true, scope))
				{
					flag = true;
					string value = "[[" + c++ + "]]";
					stringBuilder.Append(value).Append('\n');
					list.Add(new ArgumentedUntranslatedTextInfo
					{
						Info = new UntranslatedTextInfo(text)
					});
				}
				else
				{
					stringBuilder.Append(text).Append('\n');
				}
			}
			else
			{
				stringBuilder.Append('\n');
			}
		}
		if (!flag)
		{
			return null;
		}
		if (!input.EndsWith("\r\n") && !input.EndsWith("\n"))
		{
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
		}
		if (list.Count > 1)
		{
			return new ParserResult(ParserResultOrigin.GameLogTextParser, input, stringBuilder.ToString(), allowPartialTranslation: false, cacheCombinedResult: false, persistCombinedResult: false, persistTokenResult: true, list);
		}
		return null;
	}
}
