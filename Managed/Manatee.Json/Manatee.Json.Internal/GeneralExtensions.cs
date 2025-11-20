using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Manatee.Json.Internal;

internal static class GeneralExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool In<T>(this T value, params T[] collection)
	{
		return collection.Contains<T>(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInt(this double value)
	{
		return Math.Abs(Math.Ceiling(value) - Math.Floor(value)) < double.Epsilon;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetCollectionHashCode<T>(this IEnumerable<T> collection)
	{
		return collection.Aggregate<T, int>(0, (int current, T obj) => (current * 397) ^ (obj?.GetHashCode() ?? 0));
	}

	public static int GetCollectionHashCode<T>(this IEnumerable<KeyValuePair<string, T>> collection)
	{
		return collection.OrderBy<KeyValuePair<string, T>, string>((KeyValuePair<string, T> kvp) => kvp.Key, StringComparer.Ordinal).Aggregate(0, delegate(int current, KeyValuePair<string, T> kvp)
		{
			int num = ((current * 397) ^ kvp.Key.GetHashCode()) * 397;
			T value = kvp.Value;
			return num ^ ((value != null) ? value.GetHashCode() : 0);
		});
	}

	public static JsonValue? AsJsonValue(this object value)
	{
		if (value == null)
		{
			return null;
		}
		if (value is JsonValue result)
		{
			return result;
		}
		if (value is JsonArray jsonArray)
		{
			return jsonArray;
		}
		if (value is JsonObject jsonObject)
		{
			return jsonObject;
		}
		if (value is string text)
		{
			return text;
		}
		if (value is bool flag)
		{
			return flag;
		}
		if (value.IsNumber())
		{
			return Convert.ToDouble(value);
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNumber(this object? value)
	{
		if (!(value is double) && !(value is float) && !(value is int) && !(value is uint) && !(value is short) && !(value is ushort) && !(value is byte) && !(value is long))
		{
			return value is ulong;
		}
		return true;
	}
}
