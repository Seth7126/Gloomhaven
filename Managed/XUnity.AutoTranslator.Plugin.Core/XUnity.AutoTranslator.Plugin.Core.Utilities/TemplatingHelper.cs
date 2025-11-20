using System;
using System.Collections.Generic;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities;

internal static class TemplatingHelper
{
	public static bool ContainsUntemplatedCharacters(string text)
	{
		bool flag = false;
		int length = text.Length;
		for (int i = 0; i < length; i++)
		{
			char c = text[i];
			if (flag)
			{
				if (c == '}')
				{
					int num = i + 1;
					if (num < length && text[num] == '}')
					{
						i++;
						flag = false;
					}
				}
			}
			else if (c == '{')
			{
				int num2 = i + 1;
				if (num2 < length && text[num2] == '{')
				{
					i++;
					flag = true;
				}
			}
			else if (!char.IsWhiteSpace(c))
			{
				return true;
			}
		}
		return false;
	}

	public static TemplatedString TemplatizeByReplacementsAndNumbers(this string str)
	{
		TemplatedString templatedString = str.TemplatizeByReplacements();
		if (templatedString == null)
		{
			return str.TemplatizeByNumbers();
		}
		return templatedString.Template.TemplatizeByNumbers(templatedString);
	}

	public static TemplatedString TemplatizeByReplacements(this string str)
	{
		if (Settings.Replacements.Count == 0)
		{
			return null;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		char c = 'A';
		bool flag = false;
		foreach (KeyValuePair<string, string> replacement in Settings.Replacements)
		{
			string key = replacement.Key;
			string value = replacement.Value;
			if (string.IsNullOrEmpty(value))
			{
				int num = -1;
				while ((num = str.IndexOf(key, StringComparison.InvariantCulture)) != -1)
				{
					flag = true;
					str = str.Remove(num, key.Length);
				}
				continue;
			}
			string text = null;
			int num2 = -1;
			while ((num2 = str.IndexOf(key, StringComparison.InvariantCulture)) != -1)
			{
				if (text == null)
				{
					text = "{{" + c++ + "}}";
					dictionary.Add(text, value);
				}
				str = str.Remove(num2, key.Length).Insert(num2, text);
			}
		}
		if (dictionary.Count > 0 || flag)
		{
			return new TemplatedString(str, dictionary);
		}
		return null;
	}

	public static bool IsNumberOrDotOrControl(char c)
	{
		if ('*' > c || c > ':')
		{
			if ('０' <= c)
			{
				return c <= '９';
			}
			return false;
		}
		return true;
	}

	public static bool IsNumber(char c)
	{
		if ('0' > c || c > '9')
		{
			if ('０' <= c)
			{
				return c <= '９';
			}
			return false;
		}
		return true;
	}

	public static TemplatedString TemplatizeByNumbers(this string str, TemplatedString existingTemplatedString = null)
	{
		int num = 0;
		if (existingTemplatedString != null)
		{
			num = existingTemplatedString.Arguments.Count;
			str = existingTemplatedString.Template;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		bool flag = false;
		StringBuilder stringBuilder = null;
		char c = (char)(65 + num);
		int num2 = -1;
		int num3 = -1;
		for (int i = 0; i < str.Length; i++)
		{
			char c2 = str[i];
			if (flag)
			{
				if (IsNumber(c2))
				{
					num3 = i;
				}
				if (IsNumberOrDotOrControl(c2))
				{
					stringBuilder.Append(c2);
					continue;
				}
				int num4 = i - num3 - 1;
				stringBuilder.Remove(stringBuilder.Length - num4, num4);
				string text = stringBuilder.ToString();
				string text2 = "{{" + c + "}}";
				dictionary.Add(text2, text);
				c = (char)(c + 1);
				stringBuilder = null;
				flag = false;
				str = str.Remove(num2, num3 - num2 + 1).Insert(num2, text2);
				i += text2.Length - text.Length;
			}
			else if (IsNumber(c2))
			{
				flag = true;
				stringBuilder = new StringBuilder();
				stringBuilder.Append(c2);
				num2 = i;
				num3 = i;
			}
		}
		if (stringBuilder != null)
		{
			int num5 = str.Length - num3 - 1;
			stringBuilder.Remove(stringBuilder.Length - num5, num5);
			string value = stringBuilder.ToString();
			string text3 = "{{" + c + "}}";
			dictionary.Add(text3, value);
			str = str.Remove(num2, str.Length - num2 - num5).Insert(num2, text3);
		}
		if (dictionary.Count > 0)
		{
			Dictionary<string, string> dictionary2 = existingTemplatedString?.Arguments ?? dictionary;
			if (dictionary != dictionary2)
			{
				foreach (KeyValuePair<string, string> item in dictionary)
				{
					dictionary2.Add(item.Key, item.Value);
				}
			}
			return new TemplatedString(str, dictionary2);
		}
		return existingTemplatedString;
	}
}
