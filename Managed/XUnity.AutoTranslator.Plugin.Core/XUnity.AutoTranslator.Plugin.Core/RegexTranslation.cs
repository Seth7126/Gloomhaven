using System;
using System.Text.RegularExpressions;

namespace XUnity.AutoTranslator.Plugin.Core;

internal class RegexTranslation
{
	public Regex CompiledRegex { get; set; }

	public string Original { get; set; }

	public string Translation { get; set; }

	public string Key { get; set; }

	public string Value { get; set; }

	public RegexTranslation(string key, string value)
	{
		Key = key;
		Value = value;
		if (key.StartsWith("r:"))
		{
			key = key.Substring(2, key.Length - 2);
		}
		int num = key.IndexOf('"');
		if (num != -1)
		{
			num++;
			int num2 = key.LastIndexOf('"');
			if (num2 == num - 1)
			{
				throw new Exception("Splitter regex with key: '" + Key + "' starts with a \" but does not end with a \".");
			}
			key = key.Substring(num, num2 - num);
		}
		if (value.StartsWith("r:"))
		{
			value = value.Substring(2, value.Length - 2);
		}
		num = value.IndexOf('"');
		if (num != -1)
		{
			num++;
			int num3 = value.LastIndexOf('"');
			if (num3 == num - 1)
			{
				throw new Exception("Splitter regex with value: '" + Value + "' starts with a \" but does not end with a \".");
			}
			value = value.Substring(num, num3 - num);
		}
		CompiledRegex = new Regex(key, AutoTranslationPlugin.RegexCompiledSupportedFlag);
		Original = key;
		Translation = value;
	}
}
