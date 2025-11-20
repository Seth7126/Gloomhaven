using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Hydra.Sdk.Helpers;

public static class EntrySizeCalculator
{
	private static readonly Encoding _encoding = Encoding.ASCII;

	public static int GetSize(string value)
	{
		return (!string.IsNullOrEmpty(value)) ? _encoding.GetByteCount(value) : 0;
	}

	public static int GetSize(IEnumerable<object> args)
	{
		if (args == null)
		{
			return 0;
		}
		int num = 0;
		foreach (object arg in args)
		{
			if (arg == null)
			{
				continue;
			}
			Type type = arg.GetType();
			if (type.GetTypeInfo().IsEnum)
			{
				num += GetSize(EnumToString(arg, type));
				continue;
			}
			switch (Type.GetTypeCode(type))
			{
			case TypeCode.Boolean:
			case TypeCode.Char:
			case TypeCode.SByte:
			case TypeCode.Byte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				num += Marshal.SizeOf(arg);
				break;
			case TypeCode.String:
				num += GetSize((string)arg);
				break;
			case TypeCode.DateTime:
				num += Marshal.SizeOf(typeof(ulong));
				break;
			case TypeCode.Object:
				num += ((arg is Guid) ? (2 * Marshal.SizeOf(typeof(ulong))) : GetSize(arg.ToString()));
				break;
			default:
				throw new ArgumentException("Invalid argument type. Argument could be of a primitive type, System.DateTime, System.Decimal, System.Guid or System.String");
			}
		}
		return num;
	}

	public static string EnumToString(object value, Type enumType)
	{
		return $"{enumType.Name}.{value}";
	}
}
