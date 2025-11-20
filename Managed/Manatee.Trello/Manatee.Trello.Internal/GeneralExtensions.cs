using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Manatee.Trello.Internal;

internal static class GeneralExtensions
{
	private class Description
	{
		public object Value { get; set; }

		public string String { get; set; }
	}

	private static readonly Dictionary<Type, List<Description>> Descriptions = new Dictionary<Type, List<Description>>();

	private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

	private static readonly DateTime TrelloMinDate;

	public static string ToLowerString<T>(this T item)
	{
		return item.ToString().ToLower();
	}

	public static void Replace<T>(this List<T> list, T replace, T with)
	{
		int num = list.IndexOf(replace);
		if (num != -1)
		{
			list[num] = with;
		}
	}

	public static bool BeginsWith(this string str, string beginning)
	{
		if (beginning.Length <= str.Length)
		{
			return str.Substring(0, beginning.Length) == beginning;
		}
		return true;
	}

	public static bool IsNullOrWhiteSpace(this string value)
	{
		return string.IsNullOrWhiteSpace(value);
	}

	public static string Join(this IEnumerable<string> segments, string separator)
	{
		return string.Join(separator, segments);
	}

	public static string GetDescription<T>(this T enumerationValue) where T : struct
	{
		if (!enumerationValue.GetType().GetTypeInfo().IsEnum)
		{
			throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
		}
		EnsureDescriptions<T>();
		if (typeof(T).GetTypeInfo().GetCustomAttributes(typeof(FlagsAttribute), inherit: false).Any())
		{
			return BuildFlagsValues(enumerationValue, ",");
		}
		return Descriptions[typeof(T)].First((Description d) => object.Equals(d.Value, enumerationValue)).String;
	}

	public static bool IsNotFoundError(this TrelloInteractionException e)
	{
		if (e.InnerException != null)
		{
			return e.InnerException.Message.ToLower().Contains("not found");
		}
		return false;
	}

	public static string FlagsEnumToCommaList<T>(this T value)
	{
		return value.ToLowerString().Replace(" ", string.Empty);
	}

	public static bool In<T>(this T obj, params T[] values)
	{
		return values.Contains(obj);
	}

	public static DateTime ExtractCreationDate(this string id)
	{
		if (id.IsNullOrWhiteSpace())
		{
			throw new InvalidOperationException("Cannot extract creation date until ID is downloaded.");
		}
		int num = int.Parse(id.Substring(0, 8), NumberStyles.HexNumber);
		DateTime unixEpoch = UnixEpoch;
		return unixEpoch.AddSeconds(num);
	}

	public static DateTime Encode(this DateTime date)
	{
		if (!(date <= TrelloMinDate))
		{
			return date;
		}
		return TrelloMinDate;
	}

	public static DateTime Decode(this DateTime date)
	{
		if (!(date == TrelloMinDate))
		{
			return date;
		}
		return DateTime.MinValue;
	}

	private static void EnsureDescriptions<T>()
	{
		Type typeFromHandle = typeof(T);
		if (Descriptions.ContainsKey(typeFromHandle))
		{
			return;
		}
		lock (Descriptions)
		{
			if (!Descriptions.ContainsKey(typeFromHandle))
			{
				List<Description> value = (from T n in Enum.GetValues(typeFromHandle)
					select new Description
					{
						Value = n,
						String = GetDescription<T>(n.ToString())
					}).ToList();
				Descriptions.Add(typeFromHandle, value);
			}
		}
	}

	private static string GetDescription<T>(string name)
	{
		List<object> list = typeof(T).GetTypeInfo().GetDeclaredField(name)?.GetCustomAttributes(typeof(DisplayAttribute), inherit: false).ToList();
		if (list == null || !list.Any())
		{
			return name;
		}
		return ((DisplayAttribute)list[0]).Description;
	}

	private static string BuildFlagsValues<T>(T obj, string separator)
	{
		List<Description> list = Descriptions[typeof(T)];
		long num = Convert.ToInt64(obj);
		int num2 = list.Count - 1;
		List<string> list2 = new List<string>();
		while (num > 0 && num2 >= 0)
		{
			long num3 = Convert.ToInt64(list[num2].Value);
			if (num >= num3)
			{
				list2.Insert(0, list[num2].String);
				num -= num3;
			}
			num2--;
		}
		return list2.Distinct().Join(separator);
	}

	public static IEnumerable<Enum> GetFlags(this Enum input)
	{
		foreach (Enum value in Enum.GetValues(input.GetType()))
		{
			if (input.HasFlag(value))
			{
				yield return value;
			}
		}
	}

	public static string Mask(this string source)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < source.Length; i++)
		{
			stringBuilder.Append((i % 5 == 0) ? '_' : source[i]);
		}
		return stringBuilder.ToString();
	}

	static GeneralExtensions()
	{
		DateTime minValue = DateTime.MinValue;
		TrelloMinDate = minValue.ToUniversalTime().AddHours(12.0);
	}
}
