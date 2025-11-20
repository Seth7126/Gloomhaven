using System;
using System.Text.RegularExpressions;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing;

internal class RegexTranslationSplitter
{
	public Regex CompiledRegex { get; set; }

	public string Original { get; set; }

	public string Translation { get; set; }

	public string Key { get; set; }

	public string Value { get; set; }

	public RegexTranslationSplitter(string key, string value)
	{
		Key = key;
		Value = value;
		if (key.StartsWith("sr:"))
		{
			key = key.Substring(3, key.Length - 3);
		}
		int num = key.IndexOf('"');
		if (num != -1)
		{
			num++;
			int num2 = key.LastIndexOf('"');
			if (num2 == num - 1)
			{
				throw new Exception("Regex with key: '" + Key + "' starts with a \" but does not end with a \".");
			}
			key = key.Substring(num, num2 - num);
		}
		if (value.StartsWith("sr:"))
		{
			value = value.Substring(3, value.Length - 3);
		}
		num = value.IndexOf('"');
		if (num != -1)
		{
			num++;
			int num3 = value.LastIndexOf('"');
			if (num3 == num - 1)
			{
				throw new Exception("Regex with value: '" + Value + "' starts with a \" but does not end with a \".");
			}
			value = value.Substring(num, num3 - num);
		}
		if (!key.StartsWith("^"))
		{
			key = "^" + key;
		}
		if (!key.EndsWith("$"))
		{
			key += "$";
		}
		CompiledRegex = new Regex(key, AutoTranslationPlugin.RegexCompiledSupportedFlag);
		Original = key;
		Translation = value;
	}
}
