using System;
using System.Collections.Generic;
using System.Linq;

namespace UdpKit;

internal static class UdpUtils
{
	public static string Join<T>(this IEnumerable<T> items, string seperator)
	{
		return string.Join(seperator, items.Select((T x) => x.ToString()).ToArray());
	}

	public static bool HasValue(this string value)
	{
		if (value == null)
		{
			return false;
		}
		if (value.Length == 0)
		{
			return false;
		}
		if (value.Trim().Length == 0)
		{
			return false;
		}
		return true;
	}

	public static byte[] ReadToken(byte[] buffer, int size, int tokenStart)
	{
		byte[] array = null;
		if (size > tokenStart)
		{
			array = new byte[size - tokenStart];
			Buffer.BlockCopy(buffer, tokenStart, array, 0, size - tokenStart);
		}
		return array;
	}
}
