public static class BoolExtensions
{
	public static bool IsTrue(this bool item)
	{
		return item;
	}

	public static bool IsFalse(this bool item)
	{
		return !item;
	}

	public static bool IsNotTrue(this bool item)
	{
		return !item.IsTrue();
	}

	public static bool IsNotFalse(this bool item)
	{
		return !item.IsFalse();
	}

	public static bool Toggle(this bool item)
	{
		return !item;
	}

	public static int ToInt(this bool item)
	{
		if (!item)
		{
			return 0;
		}
		return 1;
	}

	public static string ToLowerString(this bool item)
	{
		return item.ToString().ToLower();
	}

	public static string ToYesNo(this bool item)
	{
		return item.ToString("Yes", "No");
	}

	public static string ToString(this bool item, string trueString, string falseString)
	{
		return item.ToType(trueString, falseString);
	}

	public static T ToType<T>(this bool item, T trueValue, T falseValue)
	{
		if (!item)
		{
			return falseValue;
		}
		return trueValue;
	}
}
