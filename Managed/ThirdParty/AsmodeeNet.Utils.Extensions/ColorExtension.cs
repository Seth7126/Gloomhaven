using UnityEngine;

namespace AsmodeeNet.Utils.Extensions;

public static class ColorExtension
{
	public static string ToHex(this Color32 color, bool includeAlpha = false)
	{
		string text = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
		if (includeAlpha)
		{
			text += color.a.ToString("X2");
		}
		return text;
	}

	public static string ToHex(this Color color, bool includeAlpha = false)
	{
		return ((Color32)color).ToHex(includeAlpha);
	}
}
