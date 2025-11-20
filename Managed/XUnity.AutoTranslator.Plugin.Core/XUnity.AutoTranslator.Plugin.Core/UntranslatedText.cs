using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core;

internal class UntranslatedText
{
	private bool? _isOnlyTemplate;

	public bool IsFromSpammingComponent { get; set; }

	public string LeadingWhitespace { get; }

	public string TrailingWhitespace { get; }

	public string Original_Text { get; }

	public string Original_Text_ExternallyTrimmed { get; }

	public string Original_Text_InternallyTrimmed { get; }

	public string Original_Text_FullyTrimmed { get; }

	public string TemplatedOriginal_Text { get; }

	public string TemplatedOriginal_Text_ExternallyTrimmed { get; }

	public string TemplatedOriginal_Text_InternallyTrimmed { get; }

	public string TemplatedOriginal_Text_FullyTrimmed { get; }

	public TemplatedString TemplatedText { get; }

	public bool IsTemplated => TemplatedText != null;

	public bool IsOnlyTemplate
	{
		get
		{
			if (!_isOnlyTemplate.HasValue)
			{
				_isOnlyTemplate = IsTemplated && !TemplatingHelper.ContainsUntemplatedCharacters(TemplatedOriginal_Text_ExternallyTrimmed);
			}
			return _isOnlyTemplate.Value;
		}
	}

	private static string PerformInternalTrimming(string text, bool whitespaceBetweenWords, ref StringBuilder builder)
	{
		if (builder != null)
		{
			builder.Length = 0;
		}
		else if (builder == null)
		{
			builder = new StringBuilder(64);
		}
		bool flag = false;
		int length = text.Length;
		int num = -1;
		int i;
		for (i = 0; i < length; i++)
		{
			char c = text[i];
			if (c == '\n')
			{
				int num2 = i - 1;
				while (num2 >= 0 && char.IsWhiteSpace(text[num2]))
				{
					num2--;
				}
				int j;
				for (j = i + 1; j < length && char.IsWhiteSpace(text[j]); j++)
				{
				}
				num2++;
				j--;
				int num3 = j - num2;
				char c2 = '\0';
				if (num3 > 0)
				{
					int num4 = 0;
					char c3 = text[num2];
					bool flag2 = false;
					num2++;
					for (int k = num2; k <= j; k++)
					{
						char c4 = text[k];
						if (c4 == c3)
						{
							if (!flag2)
							{
								num4++;
								builder.Append(c3);
								flag2 = true;
							}
							num4++;
							builder.Append(c4);
							c2 = c4;
						}
						else if (c3 == '\r' && c4 == '\n')
						{
							int index = k + 1;
							int index2 = k + 2;
							if (k + 2 > j)
							{
								continue;
							}
							char num5 = text[index];
							char c5 = text[index2];
							if (num5 == '\r' && c5 == '\n')
							{
								if (!flag2)
								{
									num4++;
									builder.Append('\r');
									builder.Append('\n');
									flag2 = true;
								}
								num4++;
								builder.Append('\r');
								builder.Append('\n');
								c2 = '\n';
								k++;
							}
						}
						else
						{
							flag2 = false;
							c3 = c4;
						}
					}
					if (num4 - 1 != num3)
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
				if (whitespaceBetweenWords && !char.IsWhiteSpace(c2) && builder.Length > 0 && builder[builder.Length - 1] != ' ')
				{
					flag = true;
					builder.Append(' ');
				}
				i = j;
				num = -1;
			}
			else if (!char.IsWhiteSpace(c))
			{
				if (num != -1)
				{
					for (int l = num; l < i; l++)
					{
						builder.Append(text[l]);
					}
					num = -1;
				}
				builder.Append(c);
			}
			else if (num == -1)
			{
				num = i;
			}
		}
		if (num != -1)
		{
			for (int m = num; m < i; m++)
			{
				builder.Append(text[m]);
			}
		}
		if (flag)
		{
			return builder.ToString();
		}
		return text;
	}

	private static string SurroundWithWhitespace(string text, string leadingWhitespace, string trailingWhitespace, ref StringBuilder builder)
	{
		if (leadingWhitespace != null || trailingWhitespace != null)
		{
			if (builder != null)
			{
				builder.Length = 0;
			}
			else if (builder == null)
			{
				builder = new StringBuilder(64);
			}
			if (leadingWhitespace != null)
			{
				builder.Append(leadingWhitespace);
			}
			builder.Append(text);
			if (trailingWhitespace != null)
			{
				builder.Append(trailingWhitespace);
			}
			return builder.ToString();
		}
		return text;
	}

	public UntranslatedText(string originalText, bool isFromSpammingComponent, bool removeInternalWhitespace, bool whitespaceBetweenWords, bool enableTemplating, bool templateAllNumbersAway)
	{
		IsFromSpammingComponent = isFromSpammingComponent;
		Original_Text = originalText;
		if (enableTemplating)
		{
			if (isFromSpammingComponent)
			{
				TemplatedText = originalText.TemplatizeByNumbers();
				if (TemplatedText != null)
				{
					originalText = TemplatedText.Template;
				}
			}
			else
			{
				TemplatedText = (templateAllNumbersAway ? originalText.TemplatizeByReplacementsAndNumbers() : originalText.TemplatizeByReplacements());
				if (TemplatedText != null)
				{
					originalText = TemplatedText.Template;
				}
			}
		}
		TemplatedOriginal_Text = originalText;
		bool isTemplated = IsTemplated;
		int i = 0;
		StringBuilder stringBuilder = null;
		for (; i < originalText.Length && char.IsWhiteSpace(originalText[i]); i++)
		{
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder(64);
			}
			stringBuilder.Append(originalText[i]);
		}
		if (i != 0)
		{
			LeadingWhitespace = stringBuilder?.ToString();
		}
		StringBuilder builder = stringBuilder;
		if (i != originalText.Length)
		{
			i = originalText.Length - 1;
			if (builder != null)
			{
				builder.Length = 0;
			}
			while (i > -1 && char.IsWhiteSpace(originalText[i]))
			{
				if (builder == null)
				{
					builder = new StringBuilder(64);
				}
				builder.Append(originalText[i]);
				i--;
			}
			if (i != originalText.Length - 1)
			{
				TrailingWhitespace = builder?.Reverse().ToString();
			}
		}
		int num = ((LeadingWhitespace != null) ? LeadingWhitespace.Length : 0);
		int num2 = ((TrailingWhitespace != null) ? TrailingWhitespace.Length : 0);
		if (num > 0 || num2 > 0)
		{
			Original_Text_ExternallyTrimmed = Original_Text.Substring(num, Original_Text.Length - num2 - num);
		}
		else
		{
			Original_Text_ExternallyTrimmed = Original_Text;
		}
		if (isTemplated)
		{
			TemplatedOriginal_Text_ExternallyTrimmed = TemplatedOriginal_Text.Substring(num, TemplatedOriginal_Text.Length - num2 - num);
		}
		else
		{
			TemplatedOriginal_Text_ExternallyTrimmed = Original_Text_ExternallyTrimmed;
		}
		if (removeInternalWhitespace)
		{
			Original_Text_FullyTrimmed = PerformInternalTrimming(Original_Text_ExternallyTrimmed, whitespaceBetweenWords, ref builder);
			bool flag = (object)Original_Text_FullyTrimmed == Original_Text_ExternallyTrimmed;
			Original_Text_InternallyTrimmed = (flag ? Original_Text : SurroundWithWhitespace(Original_Text_FullyTrimmed, LeadingWhitespace, TrailingWhitespace, ref builder));
		}
		else
		{
			Original_Text_FullyTrimmed = Original_Text_ExternallyTrimmed;
			Original_Text_InternallyTrimmed = Original_Text;
		}
		if (isTemplated)
		{
			if (removeInternalWhitespace)
			{
				TemplatedOriginal_Text_FullyTrimmed = PerformInternalTrimming(TemplatedOriginal_Text_ExternallyTrimmed, whitespaceBetweenWords, ref builder);
				bool flag2 = (object)TemplatedOriginal_Text_FullyTrimmed == TemplatedOriginal_Text_ExternallyTrimmed;
				TemplatedOriginal_Text_InternallyTrimmed = (flag2 ? TemplatedOriginal_Text : SurroundWithWhitespace(TemplatedOriginal_Text_FullyTrimmed, LeadingWhitespace, TrailingWhitespace, ref builder));
			}
			else
			{
				TemplatedOriginal_Text_FullyTrimmed = TemplatedOriginal_Text_ExternallyTrimmed;
				TemplatedOriginal_Text_InternallyTrimmed = TemplatedOriginal_Text;
			}
		}
		else
		{
			TemplatedOriginal_Text_FullyTrimmed = Original_Text_FullyTrimmed;
			TemplatedOriginal_Text_InternallyTrimmed = Original_Text_InternallyTrimmed;
		}
	}

	public string Untemplate(string text)
	{
		if (TemplatedText != null)
		{
			return TemplatedText.Untemplate(text);
		}
		return text;
	}

	public string PrepareUntranslatedText(string text)
	{
		if (TemplatedText != null)
		{
			return TemplatedText.PrepareUntranslatedText(text);
		}
		return text;
	}

	public string FixTranslatedText(string text, bool useTranslatorFriendlyArgs)
	{
		if (TemplatedText != null)
		{
			return TemplatedText.FixTranslatedText(text, useTranslatorFriendlyArgs);
		}
		return text;
	}

	public override bool Equals(object obj)
	{
		if (obj is UntranslatedText untranslatedText)
		{
			return TemplatedOriginal_Text_InternallyTrimmed == untranslatedText.TemplatedOriginal_Text_InternallyTrimmed;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return TemplatedOriginal_Text_InternallyTrimmed.GetHashCode();
	}
}
