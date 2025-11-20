using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExIni;

internal class IniParser
{
	private static readonly Regex CommentRegex = new Regex("^;(?<com>.*)");

	private static readonly Regex KeyRegex = new Regex("^(?<key>[\\w\\s]+)=(?<val>.*)$");

	private static readonly Regex SectionRegex = new Regex("^\\[(?<sec>[\\w\\s]+)\\]$");

	private static readonly Regex VarRegex = new Regex("^\\@(?<key>[\\w\\s]+)=(?<val>.*)$");

	public static IniFile Parse(string iniString)
	{
		IniFile iniFile = new IniFile();
		string[] array = (from line in iniString.Split('\n')
			let trimmed = line.Trim()
			select trimmed.TrimEnd('\r')).ToArray();
		List<string> list = new List<string>();
		IniSection iniSection = null;
		bool flag = false;
		for (int num = 0; num < array.Length; num++)
		{
			string text = array[num];
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			if (IsComment(text))
			{
				string comment = GetComment(text);
				list.Add(comment);
				if (IsVariable(comment))
				{
					string[] variable = GetVariable(comment);
					string variable2 = variable[0];
					string value = variable[1];
					Environment.SetEnvironmentVariable(variable2, value);
				}
				flag = true;
			}
			else if (IsSection(text))
			{
				iniSection = iniFile[GetSection(text)];
				if (flag)
				{
					iniSection.Comments.Append(list.ToArray());
					list.Clear();
					flag = false;
				}
			}
			else if (IsKey(text))
			{
				if (iniSection == null)
				{
					throw new Exception($"{num}: Sectionless Key Value Pair");
				}
				string[] key = GetKey(text);
				string key2 = key[0];
				string value2 = key[1];
				if (flag)
				{
					iniSection[key2].Comments.Append(list.ToArray());
					list.Clear();
					flag = false;
				}
				iniSection[key2].Value = value2;
			}
		}
		if (flag)
		{
			iniFile.Comments.Append(list.ToArray());
		}
		return iniFile;
	}

	private static string GetComment(string line)
	{
		return CommentRegex.Match(line).Groups["com"].Value;
	}

	private static string[] GetKey(string line)
	{
		Match match = KeyRegex.Match(line);
		return new string[2]
		{
			match.Groups["key"].Value,
			match.Groups["val"].Value
		};
	}

	private static string GetSection(string line)
	{
		return SectionRegex.Match(line).Groups["sec"].Value;
	}

	private static string[] GetVariable(string line)
	{
		Match match = VarRegex.Match(line);
		return new string[2]
		{
			match.Groups["key"].Value,
			match.Groups["val"].Value
		};
	}

	private static bool IsComment(string line)
	{
		return CommentRegex.IsMatch(line);
	}

	private static bool IsKey(string line)
	{
		return KeyRegex.IsMatch(line);
	}

	private static bool IsSection(string line)
	{
		return SectionRegex.IsMatch(line);
	}

	private static bool IsVariable(string line)
	{
		return VarRegex.IsMatch(line);
	}
}
