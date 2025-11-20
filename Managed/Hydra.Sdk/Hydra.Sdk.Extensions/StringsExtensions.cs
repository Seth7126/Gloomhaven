using System;

namespace Hydra.Sdk.Extensions;

public static class StringsExtensions
{
	public static string Truncate(this string str, int length)
	{
		if (string.IsNullOrEmpty(str))
		{
			return str;
		}
		return str.Substring(0, Math.Min(str.Length, length));
	}
}
