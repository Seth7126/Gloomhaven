using System;
using Manatee.Json.Internal;

namespace Manatee.Json.Path.Expressions;

internal static class ValueComparer
{
	public static bool Equal(object? a, object? b)
	{
		string text = _TryGetString(a);
		string text2 = _TryGetString(b);
		if (text != null && text2 != null)
		{
			return string.Compare(text, text2, StringComparison.Ordinal) == 0;
		}
		bool? flag = _TryGetBoolean(a);
		bool? flag2 = _TryGetBoolean(b);
		if (flag.HasValue && flag2.HasValue)
		{
			return flag == flag2;
		}
		double? num = _TryGetNumber(a);
		double? num2 = _TryGetNumber(b);
		if (num.HasValue && num2.HasValue)
		{
			return num == num2;
		}
		if (object.Equals(a, JsonValue.Null))
		{
			return object.Equals(b, JsonValue.Null);
		}
		return false;
	}

	public static bool LessThan(object? a, object? b)
	{
		string text = _TryGetString(a);
		string text2 = _TryGetString(b);
		if (text != null && text2 != null)
		{
			return string.Compare(text, text2, StringComparison.Ordinal) < 0;
		}
		double? num = _TryGetNumber(a);
		double? num2 = _TryGetNumber(b);
		if (num.HasValue && num2.HasValue)
		{
			return num < num2;
		}
		return false;
	}

	public static bool GreaterThan(object? a, object? b)
	{
		string text = _TryGetString(a);
		string text2 = _TryGetString(b);
		if (text != null && text2 != null)
		{
			return string.Compare(text, text2, StringComparison.Ordinal) > 0;
		}
		double? num = _TryGetNumber(a);
		double? num2 = _TryGetNumber(b);
		if (num.HasValue && num2.HasValue)
		{
			return num > num2;
		}
		return false;
	}

	public static bool LessThanEqual(object? a, object? b)
	{
		string text = _TryGetString(a);
		string text2 = _TryGetString(b);
		if (text != null && text2 != null)
		{
			return string.Compare(text, text2, StringComparison.Ordinal) <= 0;
		}
		double? num = _TryGetNumber(a);
		double? num2 = _TryGetNumber(b);
		if (num.HasValue && num2.HasValue)
		{
			return num <= num2;
		}
		return false;
	}

	public static bool GreaterThanEqual(object? a, object? b)
	{
		string text = _TryGetString(a);
		string text2 = _TryGetString(b);
		if (text != null && text2 != null)
		{
			return string.Compare(text, text2, StringComparison.Ordinal) >= 0;
		}
		double? num = _TryGetNumber(a);
		double? num2 = _TryGetNumber(b);
		if (num.HasValue && num2.HasValue)
		{
			return num >= num2;
		}
		return false;
	}

	private static string? _TryGetString(object? value)
	{
		JsonValue jsonValue = value as JsonValue;
		if (!(jsonValue != null) || jsonValue.Type != JsonValueType.String)
		{
			return value as string;
		}
		return jsonValue.String;
	}

	private static double? _TryGetNumber(object? value)
	{
		JsonValue jsonValue = value as JsonValue;
		if (jsonValue != null && jsonValue.Type == JsonValueType.Number)
		{
			return jsonValue.Number;
		}
		if (value.IsNumber())
		{
			return Convert.ToDouble(value);
		}
		return null;
	}

	private static bool? _TryGetBoolean(object? value)
	{
		JsonValue jsonValue = value as JsonValue;
		if (!(jsonValue != null) || jsonValue.Type != JsonValueType.Boolean)
		{
			return value as bool?;
		}
		return jsonValue.Boolean;
	}
}
