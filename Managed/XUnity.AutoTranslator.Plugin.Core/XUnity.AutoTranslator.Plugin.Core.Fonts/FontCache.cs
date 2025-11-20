using System;
using System.Collections.Generic;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Fonts;

internal static class FontCache
{
	private static readonly Dictionary<int, Font> CachedFonts = new Dictionary<int, Font>();

	private static bool _hasReadOverrideFontTextMeshPro = false;

	private static Object OverrideFontTextMeshPro;

	private static bool _hasReadFallbackFontTextMeshPro = false;

	private static Object FallbackFontTextMeshPro;

	public static Font GetOrCreate(int size)
	{
		if (!CachedFonts.TryGetValue(size, out var value))
		{
			value = FontHelper.GetTextFont(size);
			CachedFonts.Add(size, value);
		}
		return value;
	}

	public static object GetOrCreateOverrideFontTextMeshPro()
	{
		if (!_hasReadOverrideFontTextMeshPro)
		{
			try
			{
				_hasReadOverrideFontTextMeshPro = true;
				OverrideFontTextMeshPro = FontHelper.GetTextMeshProFont(Settings.OverrideFontTextMeshPro);
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Error(e, "An error occurred while loading text mesh pro override font: " + Settings.OverrideFontTextMeshPro);
			}
		}
		return OverrideFontTextMeshPro;
	}

	public static Object GetOrCreateFallbackFontTextMeshPro()
	{
		if (!_hasReadFallbackFontTextMeshPro)
		{
			try
			{
				_hasReadFallbackFontTextMeshPro = true;
				FallbackFontTextMeshPro = FontHelper.GetTextMeshProFont(Settings.FallbackFontTextMeshPro);
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Error(e, "An error occurred while loading text mesh pro fallback font: " + Settings.FallbackFontTextMeshPro);
			}
		}
		return FallbackFontTextMeshPro;
	}
}
