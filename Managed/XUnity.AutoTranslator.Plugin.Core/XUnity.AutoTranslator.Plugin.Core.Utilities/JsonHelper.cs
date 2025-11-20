using System.Globalization;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities;

public static class JsonHelper
{
	public static string Unescape(string str)
	{
		if (str == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder(str);
		bool flag = false;
		for (int i = 0; i < stringBuilder.Length; i++)
		{
			char c = stringBuilder[i];
			if (flag)
			{
				bool flag2 = true;
				char c2 = '\0';
				switch (c)
				{
				case 'b':
					c2 = '\b';
					break;
				case 'f':
					c2 = '\f';
					break;
				case 'n':
					c2 = '\n';
					break;
				case 'r':
					c2 = '\r';
					break;
				case 't':
					c2 = '\t';
					break;
				case '"':
					c2 = '"';
					break;
				case '\\':
					c2 = '\\';
					break;
				case 'u':
					c2 = 'u';
					break;
				default:
					flag2 = false;
					break;
				}
				if (flag2)
				{
					if (c2 == 'u')
					{
						char value = (char)int.Parse(new string(new char[4]
						{
							stringBuilder[i + 1],
							stringBuilder[i + 2],
							stringBuilder[i + 3],
							stringBuilder[i + 4]
						}), NumberStyles.HexNumber);
						stringBuilder.Remove(--i, 6);
						stringBuilder.Insert(i, value);
					}
					else
					{
						stringBuilder.Remove(--i, 2);
						stringBuilder.Insert(i, c2);
					}
				}
				flag = false;
			}
			else if (c == '\\')
			{
				flag = true;
			}
		}
		return stringBuilder.ToString();
	}

	public static string Escape(string str)
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
			case '"':
			case '\\':
				stringBuilder.Append('\\');
				stringBuilder.Append(c);
				break;
			case '\b':
				stringBuilder.Append("\\b");
				break;
			case '\t':
				stringBuilder.Append("\\t");
				break;
			case '\n':
				stringBuilder.Append("\\n");
				break;
			case '\f':
				stringBuilder.Append("\\f");
				break;
			case '\r':
				stringBuilder.Append("\\r");
				break;
			case '\u0085':
				stringBuilder.Append("\\u0085");
				break;
			case '\u2028':
				stringBuilder.Append("\\u2028");
				break;
			case '\u2029':
				stringBuilder.Append("\\u2029");
				break;
			default:
				stringBuilder.Append(c);
				break;
			}
		}
		return stringBuilder.ToString();
	}
}
