using System;
using System.Globalization;
using ExIni;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions;

internal static class IniFileExtensions
{
	public static void Set<T>(this IniFile that, string section, string key, T value)
	{
		Type type = typeof(T).UnwrapNullable();
		IniKey iniKey = that[section][key];
		if (value == null)
		{
			iniKey.Value = string.Empty;
		}
		else if (type.IsEnum)
		{
			iniKey.Value = EnumHelper.GetNames(type, value);
		}
		else
		{
			iniKey.Value = Convert.ToString(value, CultureInfo.InvariantCulture);
		}
	}

	public static T GetOrDefault<T>(this IniFile that, string section, string key, T defaultValue)
	{
		Type type = typeof(T).UnwrapNullable();
		IniKey iniKey = that[section][key];
		try
		{
			string value = iniKey.Value;
			if (value == null)
			{
				if (defaultValue != null)
				{
					if (type.IsEnum)
					{
						iniKey.Value = EnumHelper.GetNames(type, defaultValue);
					}
					else
					{
						iniKey.Value = Convert.ToString(defaultValue, CultureInfo.InvariantCulture);
					}
				}
				else
				{
					iniKey.Value = string.Empty;
				}
				return defaultValue;
			}
			if (!string.IsNullOrEmpty(value))
			{
				if (type.IsEnum)
				{
					return (T)EnumHelper.GetValues(type, iniKey.Value);
				}
				return (T)Convert.ChangeType(iniKey.Value, type, CultureInfo.InvariantCulture);
			}
			return default(T);
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, $"Error occurred while reading config '{key}' in section '{section}'. Updating the config to its default value '{defaultValue}'.");
			if (defaultValue != null)
			{
				if (type.IsEnum)
				{
					iniKey.Value = EnumHelper.GetNames(type, defaultValue);
				}
				else
				{
					iniKey.Value = Convert.ToString(defaultValue, CultureInfo.InvariantCulture);
				}
			}
			else
			{
				iniKey.Value = string.Empty;
			}
			return defaultValue;
		}
	}
}
