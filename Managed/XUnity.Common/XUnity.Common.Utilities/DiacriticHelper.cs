using System.Globalization;
using System.Linq;
using System.Text;

namespace XUnity.Common.Utilities;

public static class DiacriticHelper
{
	public static string RemoveAllDiacritics(this string input)
	{
		return new string((from c in input.SafeNormalize(NormalizationForm.FormD)
			where CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark
			select c).ToArray()).SafeNormalize();
	}

	private static string SafeNormalize(this string input, NormalizationForm normalizationForm = NormalizationForm.FormC)
	{
		return ReplaceNonCharacters(input, '?').Normalize(normalizationForm);
	}

	private static string ReplaceNonCharacters(string input, char replacement)
	{
		StringBuilder stringBuilder = new StringBuilder(input.Length);
		for (int i = 0; i < input.Length; i++)
		{
			if (char.IsSurrogatePair(input, i))
			{
				int num = char.ConvertToUtf32(input, i);
				i++;
				if (IsValidCodePoint(num))
				{
					stringBuilder.Append(char.ConvertFromUtf32(num));
				}
				else
				{
					stringBuilder.Append(replacement);
				}
			}
			else
			{
				char c = input[i];
				if (IsValidCodePoint(c))
				{
					stringBuilder.Append(c);
				}
				else
				{
					stringBuilder.Append(replacement);
				}
			}
		}
		return stringBuilder.ToString();
	}

	private static bool IsValidCodePoint(int point)
	{
		if (point >= 64976)
		{
			if (point >= 65008 && (point & 0xFFFF) != 65535 && (point & 0xFFFE) != 65534)
			{
				return point <= 1114111;
			}
			return false;
		}
		return true;
	}
}
