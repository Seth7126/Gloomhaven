using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XUnity.Common.Extensions;

public static class StringExtensions
{
	private static readonly HashSet<char> InvalidFileNameChars = new HashSet<char>(Path.GetInvalidFileNameChars());

	public static string UseCorrectDirectorySeparators(this string path)
	{
		if (Path.DirectorySeparatorChar == '\\')
		{
			return path.Replace('/', Path.DirectorySeparatorChar);
		}
		if (Path.DirectorySeparatorChar == '/')
		{
			return path.Replace('\\', Path.DirectorySeparatorChar);
		}
		return path;
	}

	public static bool IsNullOrWhiteSpace(this string value)
	{
		if (value == null)
		{
			return true;
		}
		for (int i = 0; i < value.Length; i++)
		{
			if (!char.IsWhiteSpace(value[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static string MakeRelativePath(this string fullOrRelativePath, string basePath)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int i = 0;
		bool flag = false;
		string[] array = basePath.Split(':', '\\', '/');
		List<string> list = fullOrRelativePath.Split(':', '\\', '/').ToList();
		if (array.Length == 0 || list.Count <= 0 || array[0] != list[0])
		{
			flag = true;
		}
		bool flag2 = false;
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j] == "..")
			{
				if (flag2)
				{
					int num = j - 1;
					if (num >= 0)
					{
						list.RemoveAt(j);
						list.RemoveAt(num);
						j -= 2;
					}
				}
			}
			else
			{
				flag2 = true;
			}
		}
		if (!flag)
		{
			for (i = 1; i < array.Length && !(array[i] != list[i]); i++)
			{
			}
			for (int k = 0; k < array.Length - i; k++)
			{
				stringBuilder.Append(".." + Path.DirectorySeparatorChar);
			}
		}
		for (int l = i; l < list.Count - 1; l++)
		{
			string value = list[l];
			stringBuilder.Append(value).Append(Path.DirectorySeparatorChar);
		}
		int index = list.Count - 1;
		string value2 = list[index];
		stringBuilder.Append(value2);
		return stringBuilder.ToString();
	}

	public static string SanitizeForFileSystem(this string path)
	{
		StringBuilder stringBuilder = new StringBuilder(path.Length);
		foreach (char c in path)
		{
			if (!InvalidFileNameChars.Contains(c))
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static string SplitToLines(this string text, int maxStringLength, params char[] splitOnCharacters)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num;
		for (int i = 0; text.Length > i; i += num)
		{
			if (i != 0)
			{
				stringBuilder.Append('\n');
			}
			num = ((i + maxStringLength <= text.Length) ? text.Substring(i, maxStringLength).LastIndexOfAny(splitOnCharacters) : (text.Length - i));
			num = ((num == -1) ? maxStringLength : num);
			stringBuilder.Append(text.Substring(i, num).Trim());
		}
		return stringBuilder.ToString();
	}

	public static bool StartsWithStrict(this string str, string prefix)
	{
		int num = Math.Min(str.Length, prefix.Length);
		if (num < prefix.Length)
		{
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			if (str[i] != prefix[i])
			{
				return false;
			}
		}
		return true;
	}

	public static string GetBetween(this string strSource, string strStart, string strEnd)
	{
		int num = strSource.IndexOf(strStart, StringComparison.InvariantCulture);
		if (num != -1)
		{
			num += strStart.Length;
			int num2 = strSource.IndexOf(strEnd, num, StringComparison.InvariantCulture);
			if (num2 > num)
			{
				return strSource.Substring(num, num2 - num);
			}
		}
		return string.Empty;
	}

	public static bool RemindsOf(this string that, string other)
	{
		if (!that.StartsWith(other) && !other.StartsWith(that) && !that.EndsWith(other))
		{
			return other.EndsWith(that);
		}
		return true;
	}
}
