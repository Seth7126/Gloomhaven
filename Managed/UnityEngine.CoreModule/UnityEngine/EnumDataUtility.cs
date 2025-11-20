using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityEngine;

internal static class EnumDataUtility
{
	private static readonly Dictionary<Type, EnumData> s_NonObsoleteEnumData = new Dictionary<Type, EnumData>();

	private static readonly Dictionary<Type, EnumData> s_EnumData = new Dictionary<Type, EnumData>();

	internal static EnumData GetCachedEnumData(Type enumType, bool excludeObsolete = true, Func<string, string> nicifyName = null)
	{
		if (excludeObsolete && s_NonObsoleteEnumData.TryGetValue(enumType, out var value))
		{
			return value;
		}
		if (!excludeObsolete && s_EnumData.TryGetValue(enumType, out value))
		{
			return value;
		}
		value = new EnumData
		{
			underlyingType = Enum.GetUnderlyingType(enumType)
		};
		value.unsigned = (object)value.underlyingType == typeof(byte) || (object)value.underlyingType == typeof(ushort) || (object)value.underlyingType == typeof(uint) || (object)value.underlyingType == typeof(ulong);
		FieldInfo[] fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
		List<FieldInfo> list = new List<FieldInfo>();
		int num = fields.Length;
		for (int i = 0; i < num; i++)
		{
			if (CheckObsoleteAddition(fields[i], excludeObsolete))
			{
				list.Add(fields[i]);
			}
		}
		if (!list.Any())
		{
			string[] array = new string[1] { "" };
			Enum[] values = new Enum[0];
			int[] flagValues = new int[1];
			value.values = values;
			value.flagValues = flagValues;
			value.displayNames = array;
			value.names = array;
			value.tooltip = array;
			value.flags = true;
			value.serializable = true;
			return value;
		}
		try
		{
			string location = list.First().Module.Assembly.Location;
			if (!string.IsNullOrEmpty(location))
			{
				list = list.OrderBy((FieldInfo f) => f.MetadataToken).ToList();
			}
		}
		catch
		{
		}
		value.displayNames = list.Select((FieldInfo f) => EnumNameFromEnumField(f, nicifyName)).ToArray();
		if (value.displayNames.Distinct().Count() != value.displayNames.Length)
		{
			Debug.LogWarning("Enum " + enumType.Name + " has multiple entries with the same display name, this prevents selection in EnumPopup.");
		}
		value.tooltip = list.Select((FieldInfo f) => EnumTooltipFromEnumField(f)).ToArray();
		value.values = list.Select((FieldInfo f) => (Enum)f.GetValue(null)).ToArray();
		value.flagValues = (value.unsigned ? value.values.Select((Enum v) => (int)Convert.ToUInt64(v)).ToArray() : value.values.Select((Enum v) => (int)Convert.ToInt64(v)).ToArray());
		value.names = new string[value.values.Length];
		for (int num2 = 0; num2 < value.values.Length; num2++)
		{
			value.names[num2] = value.values[num2].ToString();
		}
		if ((object)value.underlyingType == typeof(ushort))
		{
			int num3 = 0;
			for (int num4 = value.flagValues.Length; num3 < num4; num3++)
			{
				if ((long)value.flagValues[num3] == 65535)
				{
					value.flagValues[num3] = -1;
				}
			}
		}
		else if ((object)value.underlyingType == typeof(byte))
		{
			int num5 = 0;
			for (int num6 = value.flagValues.Length; num5 < num6; num5++)
			{
				if ((long)value.flagValues[num5] == 255)
				{
					value.flagValues[num5] = -1;
				}
			}
		}
		value.flags = enumType.IsDefined(typeof(FlagsAttribute), inherit: false);
		value.serializable = (object)value.underlyingType != typeof(long) && (object)value.underlyingType != typeof(ulong);
		if (excludeObsolete)
		{
			s_NonObsoleteEnumData[enumType] = value;
		}
		else
		{
			s_EnumData[enumType] = value;
		}
		return value;
	}

	internal static int EnumFlagsToInt(EnumData enumData, Enum enumValue)
	{
		if (enumData.unsigned)
		{
			if ((object)enumData.underlyingType == typeof(uint))
			{
				return (int)Convert.ToUInt32(enumValue);
			}
			if ((object)enumData.underlyingType == typeof(ushort))
			{
				ushort num = Convert.ToUInt16(enumValue);
				return (num == ushort.MaxValue) ? (-1) : num;
			}
			byte b = Convert.ToByte(enumValue);
			return (b == byte.MaxValue) ? (-1) : b;
		}
		return Convert.ToInt32(enumValue);
	}

	internal static Enum IntToEnumFlags(Type enumType, int value)
	{
		EnumData cachedEnumData = GetCachedEnumData(enumType);
		if (cachedEnumData.unsigned)
		{
			if ((object)cachedEnumData.underlyingType == typeof(uint))
			{
				uint num = (uint)value;
				return Enum.Parse(enumType, num.ToString()) as Enum;
			}
			if ((object)cachedEnumData.underlyingType == typeof(ushort))
			{
				return Enum.Parse(enumType, ((ushort)value).ToString()) as Enum;
			}
			return Enum.Parse(enumType, ((byte)value).ToString()) as Enum;
		}
		return Enum.Parse(enumType, value.ToString()) as Enum;
	}

	private static bool CheckObsoleteAddition(FieldInfo field, bool excludeObsolete)
	{
		object[] customAttributes = field.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false);
		if (customAttributes.Length != 0)
		{
			if (excludeObsolete)
			{
				return false;
			}
			return !((ObsoleteAttribute)customAttributes.First()).IsError;
		}
		return true;
	}

	private static string EnumTooltipFromEnumField(FieldInfo field)
	{
		object[] customAttributes = field.GetCustomAttributes(typeof(TooltipAttribute), inherit: false);
		if (customAttributes.Length != 0)
		{
			return ((TooltipAttribute)customAttributes.First()).tooltip;
		}
		return string.Empty;
	}

	private static string EnumNameFromEnumField(FieldInfo field, Func<string, string> nicifyName)
	{
		object[] customAttributes = field.GetCustomAttributes(typeof(InspectorNameAttribute), inherit: false);
		if (customAttributes.Length != 0)
		{
			return ((InspectorNameAttribute)customAttributes.First()).displayName;
		}
		if (field.IsDefined(typeof(ObsoleteAttribute), inherit: false))
		{
			return NicifyName() + " (Obsolete)";
		}
		return NicifyName();
		string NicifyName()
		{
			return (nicifyName == null) ? field.Name : nicifyName(field.Name);
		}
	}
}
