using System.Collections.Generic;

namespace PlayEveryWare.EpicOnlineServices;

public static class EmptyPredicates
{
	public static bool IsEmptyOrNull(string s)
	{
		if (s != null)
		{
			return s.Length == 0;
		}
		return true;
	}

	public static bool IsEmptyOrNull(bool? b)
	{
		return !b.HasValue;
	}

	public static bool IsEmptyOrNull<T>(List<T> list)
	{
		if (list != null)
		{
			return list.Count == 0;
		}
		return true;
	}

	public static bool IsEmptyOrNull(IEmpty o)
	{
		return o?.IsEmpty() ?? true;
	}

	public static bool IsEmptyOrNullOrContainsOnlyEmpty(List<string> list)
	{
		if (list != null && list.Count != 0)
		{
			return list.TrueForAll(IsEmptyOrNull);
		}
		return true;
	}

	public static bool IsEmptyOrContainsOnlyEmpty(List<string> list)
	{
		if (list.Count != 0)
		{
			return list.TrueForAll(IsEmptyOrNull);
		}
		return true;
	}

	public static T NewIfNull<T>(T value) where T : new()
	{
		if (value == null)
		{
			return new T();
		}
		return value;
	}

	public static string NewIfNull(string value)
	{
		if (value != null)
		{
			return value;
		}
		return "";
	}
}
