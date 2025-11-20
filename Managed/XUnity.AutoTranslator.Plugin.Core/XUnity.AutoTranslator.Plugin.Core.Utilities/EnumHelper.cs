using System;
using System.Linq;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities;

internal static class EnumHelper
{
	public static string GetNames(Type flagType, object value)
	{
		if ((FlagsAttribute)flagType.GetCustomAttributes(typeof(FlagsAttribute), inherit: false).FirstOrDefault() == null)
		{
			return Enum.GetName(flagType, value);
		}
		Array values = Enum.GetValues(flagType);
		string[] names = Enum.GetNames(flagType);
		string text = string.Empty;
		string[] array = names;
		foreach (string text2 in array)
		{
			object objA = Enum.Parse(flagType, text2, ignoreCase: true);
			foreach (object item in values)
			{
				long num = Convert.ToInt64(item);
				if ((Convert.ToInt64(value) & num) != 0L && object.Equals(objA, item))
				{
					text = text + text2 + ";";
					break;
				}
			}
		}
		if (text.EndsWith(";"))
		{
			text = text.Substring(0, text.Length - 1);
		}
		return text;
	}

	public static object GetValues(Type flagType, string commaSeparatedStringValue)
	{
		if ((FlagsAttribute)flagType.GetCustomAttributes(typeof(FlagsAttribute), inherit: false).FirstOrDefault() == null)
		{
			return Enum.Parse(flagType, commaSeparatedStringValue, ignoreCase: true);
		}
		string[] array = commaSeparatedStringValue.Split(new char[2] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		Array values = Enum.GetValues(flagType);
		Enum.GetUnderlyingType(flagType);
		long num = 0L;
		string[] array2 = array;
		foreach (string text in array2)
		{
			bool flag = false;
			foreach (object item in values)
			{
				string name = Enum.GetName(flagType, item);
				if (string.Equals(text, name, StringComparison.OrdinalIgnoreCase))
				{
					long num2 = Convert.ToInt64(item);
					num |= num2;
					flag = true;
				}
			}
			if (!flag)
			{
				throw new ArgumentException("Requested value '" + text + "' was not found.");
			}
		}
		return Convert.ChangeType(num, Enum.GetUnderlyingType(flagType));
	}
}
