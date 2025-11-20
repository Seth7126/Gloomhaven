using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions;

public static class StringBuilderExtensions
{
	public static bool EndsWithWhitespaceOrNewline(this StringBuilder builder)
	{
		if (builder.Length == 0)
		{
			return true;
		}
		char c = builder[builder.Length - 1];
		if (!char.IsWhiteSpace(c))
		{
			return c == '\n';
		}
		return true;
	}

	internal static StringBuilder Reverse(this StringBuilder text)
	{
		if (text.Length > 1)
		{
			int num = text.Length / 2;
			for (int i = 0; i < num; i++)
			{
				int index = text.Length - (i + 1);
				char value = text[i];
				char value2 = text[index];
				text[i] = value2;
				text[index] = value;
			}
		}
		return text;
	}
}
