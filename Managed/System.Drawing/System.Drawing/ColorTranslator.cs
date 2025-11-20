using System.ComponentModel;

namespace System.Drawing;

/// <summary>Translates colors to and from GDI+ <see cref="T:System.Drawing.Color" /> structures. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
public sealed class ColorTranslator
{
	private ColorTranslator()
	{
	}

	/// <summary>Translates an HTML color representation to a GDI+ <see cref="T:System.Drawing.Color" /> structure.</summary>
	/// <returns>The <see cref="T:System.Drawing.Color" /> structure that represents the translated HTML color or <see cref="F:System.Drawing.Color.Empty" /> if <paramref name="htmlColor" /> is null.</returns>
	/// <param name="htmlColor">The string representation of the Html color to translate. </param>
	/// <exception cref="T:System.Exception">
	///   <paramref name="htmlColor" /> is not a valid HTML color name.</exception>
	/// <filterpriority>1</filterpriority>
	public static Color FromHtml(string htmlColor)
	{
		if (string.IsNullOrEmpty(htmlColor))
		{
			return Color.Empty;
		}
		switch (htmlColor.ToLowerInvariant())
		{
		case "buttonface":
		case "threedface":
			return SystemColors.Control;
		case "buttonhighlight":
		case "threedlightshadow":
			return SystemColors.ControlLightLight;
		case "buttonshadow":
			return SystemColors.ControlDark;
		case "captiontext":
			return SystemColors.ActiveCaptionText;
		case "threeddarkshadow":
			return SystemColors.ControlDarkDark;
		case "threedhighlight":
			return SystemColors.ControlLight;
		case "background":
			return SystemColors.Desktop;
		case "buttontext":
			return SystemColors.ControlText;
		case "infobackground":
			return SystemColors.Info;
		case "lightgrey":
			return Color.LightGray;
		default:
			if (htmlColor[0] == '#' && htmlColor.Length == 4)
			{
				char c = htmlColor[1];
				char c2 = htmlColor[2];
				char c3 = htmlColor[3];
				htmlColor = new string(new char[7] { '#', c, c, c2, c2, c3, c3 });
			}
			return (Color)TypeDescriptor.GetConverter(typeof(Color)).ConvertFromString(htmlColor);
		}
	}

	internal static Color FromBGR(int bgr)
	{
		Color color = Color.FromArgb(255, bgr & 0xFF, (bgr >> 8) & 0xFF, (bgr >> 16) & 0xFF);
		Color result = KnownColors.FindColorMatch(color);
		if (!result.IsEmpty)
		{
			return result;
		}
		return color;
	}

	/// <summary>Translates an OLE color value to a GDI+ <see cref="T:System.Drawing.Color" /> structure.</summary>
	/// <returns>The <see cref="T:System.Drawing.Color" /> structure that represents the translated OLE color.</returns>
	/// <param name="oleColor">The OLE color to translate. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Color FromOle(int oleColor)
	{
		return FromBGR(oleColor);
	}

	/// <summary>Translates a Windows color value to a GDI+ <see cref="T:System.Drawing.Color" /> structure.</summary>
	/// <returns>The <see cref="T:System.Drawing.Color" /> structure that represents the translated Windows color.</returns>
	/// <param name="win32Color">The Windows color to translate. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Color FromWin32(int win32Color)
	{
		return FromBGR(win32Color);
	}

	/// <summary>Translates the specified <see cref="T:System.Drawing.Color" /> structure to an HTML string color representation.</summary>
	/// <returns>The string that represents the HTML color.</returns>
	/// <param name="c">The <see cref="T:System.Drawing.Color" /> structure to translate. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static string ToHtml(Color c)
	{
		if (c.IsEmpty)
		{
			return string.Empty;
		}
		if (c.IsSystemColor)
		{
			KnownColor knownColor = c.ToKnownColor();
			switch (knownColor)
			{
			case KnownColor.ActiveBorder:
			case KnownColor.ActiveCaption:
			case KnownColor.AppWorkspace:
			case KnownColor.GrayText:
			case KnownColor.Highlight:
			case KnownColor.HighlightText:
			case KnownColor.InactiveBorder:
			case KnownColor.InactiveCaption:
			case KnownColor.InactiveCaptionText:
			case KnownColor.InfoText:
			case KnownColor.Menu:
			case KnownColor.MenuText:
			case KnownColor.ScrollBar:
			case KnownColor.Window:
			case KnownColor.WindowFrame:
			case KnownColor.WindowText:
				return KnownColors.GetName(knownColor).ToLowerInvariant();
			case KnownColor.ActiveCaptionText:
				return "captiontext";
			case KnownColor.Control:
				return "buttonface";
			case KnownColor.ControlDark:
				return "buttonshadow";
			case KnownColor.ControlDarkDark:
				return "threeddarkshadow";
			case KnownColor.ControlLight:
				return "buttonface";
			case KnownColor.ControlLightLight:
				return "buttonhighlight";
			case KnownColor.ControlText:
				return "buttontext";
			case KnownColor.Desktop:
				return "background";
			case KnownColor.HotTrack:
				return "highlight";
			case KnownColor.Info:
				return "infobackground";
			default:
				return string.Empty;
			}
		}
		if (c.IsNamedColor)
		{
			if (c == Color.LightGray)
			{
				return "LightGrey";
			}
			return c.Name;
		}
		return FormatHtml(c.R, c.G, c.B);
	}

	private static char GetHexNumber(int b)
	{
		return (char)((b > 9) ? (55 + b) : (48 + b));
	}

	private static string FormatHtml(int r, int g, int b)
	{
		return new string(new char[7]
		{
			'#',
			GetHexNumber((r >> 4) & 0xF),
			GetHexNumber(r & 0xF),
			GetHexNumber((g >> 4) & 0xF),
			GetHexNumber(g & 0xF),
			GetHexNumber((b >> 4) & 0xF),
			GetHexNumber(b & 0xF)
		});
	}

	/// <summary>Translates the specified <see cref="T:System.Drawing.Color" /> structure to an OLE color.</summary>
	/// <returns>The OLE color value.</returns>
	/// <param name="c">The <see cref="T:System.Drawing.Color" /> structure to translate. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static int ToOle(Color c)
	{
		return (c.B << 16) | (c.G << 8) | c.R;
	}

	/// <summary>Translates the specified <see cref="T:System.Drawing.Color" /> structure to a Windows color.</summary>
	/// <returns>The Windows color value.</returns>
	/// <param name="c">The <see cref="T:System.Drawing.Color" /> structure to translate. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static int ToWin32(Color c)
	{
		return (c.B << 16) | (c.G << 8) | c.R;
	}
}
