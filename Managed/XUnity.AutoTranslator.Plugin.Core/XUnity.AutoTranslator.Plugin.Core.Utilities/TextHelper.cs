using System;
using System.Globalization;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities;

internal static class TextHelper
{
	public static string Encode(string text)
	{
		return EscapeNewlines(text);
	}

	public static string[] ReadTranslationLineAndDecode(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return null;
		}
		string[] array = new string[2];
		int num = 0;
		bool flag = false;
		int length = str.Length;
		StringBuilder stringBuilder = new StringBuilder((int)((double)length / 1.3));
		for (int i = 0; i < length; i++)
		{
			char c = str[i];
			if (flag)
			{
				switch (c)
				{
				case '=':
				case '\\':
					stringBuilder.Append(c);
					break;
				case 'n':
					stringBuilder.Append('\n');
					break;
				case 'r':
					stringBuilder.Append('\r');
					break;
				case 'u':
					if (i + 4 < length)
					{
						int num2 = int.Parse(new string(new char[4]
						{
							str[i + 1],
							str[i + 2],
							str[i + 3],
							str[i + 4]
						}), NumberStyles.HexNumber);
						stringBuilder.Append((char)num2);
						i += 4;
						break;
					}
					throw new Exception("Found invalid unicode in line: " + str);
				default:
					stringBuilder.Append('\\');
					stringBuilder.Append(c);
					break;
				}
				flag = false;
				continue;
			}
			switch (c)
			{
			case '\\':
				flag = true;
				break;
			case '=':
				if (num > 1)
				{
					return null;
				}
				array[num++] = stringBuilder.ToString();
				stringBuilder.Length = 0;
				break;
			case '%':
				if (i + 2 < length && str[i + 1] == '3' && str[i + 2] == 'D')
				{
					stringBuilder.Append('=');
					i += 2;
				}
				else
				{
					stringBuilder.Append(c);
				}
				break;
			case '/':
			{
				int num3 = i + 1;
				if (num3 < length && str[num3] == '/')
				{
					array[num++] = stringBuilder.ToString();
					if (num == 2)
					{
						return array;
					}
					return null;
				}
				stringBuilder.Append(c);
				break;
			}
			default:
				stringBuilder.Append(c);
				break;
			}
		}
		if (num != 1)
		{
			return null;
		}
		array[num++] = stringBuilder.ToString();
		return array;
	}

	internal static string EscapeNewlines(string str)
	{
		if (str == null || str.Length == 0)
		{
			return "";
		}
		int length = str.Length;
		StringBuilder stringBuilder = new StringBuilder(length + 4);
		for (int i = 0; i < length; i++)
		{
			char c = str[i];
			switch (c)
			{
			case '/':
			{
				int num = i + 1;
				if (num < length && str[num] == '/')
				{
					stringBuilder.Append('\\');
					stringBuilder.Append(c);
					stringBuilder.Append('\\');
					stringBuilder.Append(c);
					i++;
				}
				else
				{
					stringBuilder.Append(c);
				}
				break;
			}
			case '\\':
				stringBuilder.Append('\\');
				stringBuilder.Append(c);
				break;
			case '=':
				stringBuilder.Append('\\');
				stringBuilder.Append(c);
				break;
			case '\n':
				stringBuilder.Append("\\n");
				break;
			case '\r':
				stringBuilder.Append("\\r");
				break;
			default:
				stringBuilder.Append(c);
				break;
			}
		}
		return stringBuilder.ToString();
	}
}
