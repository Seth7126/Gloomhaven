using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Numerics.Hashing;

namespace System.Drawing;

/// <summary>Represents an ARGB (alpha, red, green, blue) color.</summary>
/// <filterpriority>1</filterpriority>
/// <completionlist cref="T:System.Drawing.Color" />
[Serializable]
[TypeConverter(typeof(ColorConverter))]
[Editor("System.Drawing.Design.ColorEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
[DebuggerDisplay("{NameAndARGBValue}")]
public readonly struct Color : IEquatable<Color>
{
	/// <summary>Represents a color that is null.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly Color Empty;

	private const short StateKnownColorValid = 1;

	private const short StateARGBValueValid = 2;

	private const short StateValueMask = 2;

	private const short StateNameValid = 8;

	private const long NotDefinedValue = 0L;

	private const int ARGBAlphaShift = 24;

	private const int ARGBRedShift = 16;

	private const int ARGBGreenShift = 8;

	private const int ARGBBlueShift = 0;

	private readonly string name;

	private readonly long value;

	private readonly short knownColor;

	private readonly short state;

	/// <summary>Gets a system-defined color.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Transparent => new Color(KnownColor.Transparent);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFF0F8FF.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color AliceBlue => new Color(KnownColor.AliceBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFAEBD7.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color AntiqueWhite => new Color(KnownColor.AntiqueWhite);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF00FFFF.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Aqua => new Color(KnownColor.Aqua);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF7FFFD4.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Aquamarine => new Color(KnownColor.Aquamarine);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFF0FFFF.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Azure => new Color(KnownColor.Azure);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFF5F5DC.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Beige => new Color(KnownColor.Beige);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFE4C4.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Bisque => new Color(KnownColor.Bisque);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF000000.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Black => new Color(KnownColor.Black);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFEBCD.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color BlanchedAlmond => new Color(KnownColor.BlanchedAlmond);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF0000FF.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Blue => new Color(KnownColor.Blue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF8A2BE2.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color BlueViolet => new Color(KnownColor.BlueViolet);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFA52A2A.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Brown => new Color(KnownColor.Brown);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFDEB887.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color BurlyWood => new Color(KnownColor.BurlyWood);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF5F9EA0.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color CadetBlue => new Color(KnownColor.CadetBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF7FFF00.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Chartreuse => new Color(KnownColor.Chartreuse);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFD2691E.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Chocolate => new Color(KnownColor.Chocolate);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFF7F50.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Coral => new Color(KnownColor.Coral);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF6495ED.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color CornflowerBlue => new Color(KnownColor.CornflowerBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFF8DC.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Cornsilk => new Color(KnownColor.Cornsilk);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFDC143C.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Crimson => new Color(KnownColor.Crimson);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF00FFFF.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Cyan => new Color(KnownColor.Cyan);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF00008B.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkBlue => new Color(KnownColor.DarkBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF008B8B.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkCyan => new Color(KnownColor.DarkCyan);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFB8860B.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkGoldenrod => new Color(KnownColor.DarkGoldenrod);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFA9A9A9.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkGray => new Color(KnownColor.DarkGray);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF006400.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkGreen => new Color(KnownColor.DarkGreen);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFBDB76B.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkKhaki => new Color(KnownColor.DarkKhaki);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF8B008B.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkMagenta => new Color(KnownColor.DarkMagenta);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF556B2F.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkOliveGreen => new Color(KnownColor.DarkOliveGreen);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFF8C00.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkOrange => new Color(KnownColor.DarkOrange);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF9932CC.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkOrchid => new Color(KnownColor.DarkOrchid);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF8B0000.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkRed => new Color(KnownColor.DarkRed);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFE9967A.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkSalmon => new Color(KnownColor.DarkSalmon);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF8FBC8F.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkSeaGreen => new Color(KnownColor.DarkSeaGreen);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF483D8B.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkSlateBlue => new Color(KnownColor.DarkSlateBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF2F4F4F.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkSlateGray => new Color(KnownColor.DarkSlateGray);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF00CED1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkTurquoise => new Color(KnownColor.DarkTurquoise);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF9400D3.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DarkViolet => new Color(KnownColor.DarkViolet);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFF1493.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DeepPink => new Color(KnownColor.DeepPink);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF00BFFF.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DeepSkyBlue => new Color(KnownColor.DeepSkyBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF696969.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DimGray => new Color(KnownColor.DimGray);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF1E90FF.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color DodgerBlue => new Color(KnownColor.DodgerBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFB22222.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Firebrick => new Color(KnownColor.Firebrick);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFFAF0.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color FloralWhite => new Color(KnownColor.FloralWhite);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF228B22.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color ForestGreen => new Color(KnownColor.ForestGreen);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFF00FF.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Fuchsia => new Color(KnownColor.Fuchsia);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFDCDCDC.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Gainsboro => new Color(KnownColor.Gainsboro);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFF8F8FF.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color GhostWhite => new Color(KnownColor.GhostWhite);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFD700.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Gold => new Color(KnownColor.Gold);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFDAA520.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Goldenrod => new Color(KnownColor.Goldenrod);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF808080.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> strcture representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Gray => new Color(KnownColor.Gray);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF008000.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Green => new Color(KnownColor.Green);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFADFF2F.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color GreenYellow => new Color(KnownColor.GreenYellow);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFF0FFF0.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Honeydew => new Color(KnownColor.Honeydew);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFF69B4.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color HotPink => new Color(KnownColor.HotPink);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFCD5C5C.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color IndianRed => new Color(KnownColor.IndianRed);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF4B0082.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Indigo => new Color(KnownColor.Indigo);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFFFF0.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Ivory => new Color(KnownColor.Ivory);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFF0E68C.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Khaki => new Color(KnownColor.Khaki);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFE6E6FA.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Lavender => new Color(KnownColor.Lavender);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFF0F5.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LavenderBlush => new Color(KnownColor.LavenderBlush);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF7CFC00.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LawnGreen => new Color(KnownColor.LawnGreen);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFFACD.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LemonChiffon => new Color(KnownColor.LemonChiffon);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFADD8E6.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LightBlue => new Color(KnownColor.LightBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFF08080.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LightCoral => new Color(KnownColor.LightCoral);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFE0FFFF.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LightCyan => new Color(KnownColor.LightCyan);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFAFAD2.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LightGoldenrodYellow => new Color(KnownColor.LightGoldenrodYellow);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF90EE90.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LightGreen => new Color(KnownColor.LightGreen);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFD3D3D3.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LightGray => new Color(KnownColor.LightGray);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFB6C1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LightPink => new Color(KnownColor.LightPink);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFA07A.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LightSalmon => new Color(KnownColor.LightSalmon);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF20B2AA.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LightSeaGreen => new Color(KnownColor.LightSeaGreen);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF87CEFA.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LightSkyBlue => new Color(KnownColor.LightSkyBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF778899.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LightSlateGray => new Color(KnownColor.LightSlateGray);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFB0C4DE.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LightSteelBlue => new Color(KnownColor.LightSteelBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFFFE0.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LightYellow => new Color(KnownColor.LightYellow);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF00FF00.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Lime => new Color(KnownColor.Lime);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF32CD32.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color LimeGreen => new Color(KnownColor.LimeGreen);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFAF0E6.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Linen => new Color(KnownColor.Linen);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFF00FF.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Magenta => new Color(KnownColor.Magenta);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF800000.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Maroon => new Color(KnownColor.Maroon);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF66CDAA.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color MediumAquamarine => new Color(KnownColor.MediumAquamarine);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF0000CD.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color MediumBlue => new Color(KnownColor.MediumBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFBA55D3.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color MediumOrchid => new Color(KnownColor.MediumOrchid);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF9370DB.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color MediumPurple => new Color(KnownColor.MediumPurple);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF3CB371.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color MediumSeaGreen => new Color(KnownColor.MediumSeaGreen);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF7B68EE.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color MediumSlateBlue => new Color(KnownColor.MediumSlateBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF00FA9A.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color MediumSpringGreen => new Color(KnownColor.MediumSpringGreen);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF48D1CC.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color MediumTurquoise => new Color(KnownColor.MediumTurquoise);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFC71585.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color MediumVioletRed => new Color(KnownColor.MediumVioletRed);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF191970.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color MidnightBlue => new Color(KnownColor.MidnightBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFF5FFFA.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color MintCream => new Color(KnownColor.MintCream);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFE4E1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color MistyRose => new Color(KnownColor.MistyRose);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFE4B5.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Moccasin => new Color(KnownColor.Moccasin);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFDEAD.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color NavajoWhite => new Color(KnownColor.NavajoWhite);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF000080.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Navy => new Color(KnownColor.Navy);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFDF5E6.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color OldLace => new Color(KnownColor.OldLace);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF808000.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Olive => new Color(KnownColor.Olive);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF6B8E23.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color OliveDrab => new Color(KnownColor.OliveDrab);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFA500.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Orange => new Color(KnownColor.Orange);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFF4500.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color OrangeRed => new Color(KnownColor.OrangeRed);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFDA70D6.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Orchid => new Color(KnownColor.Orchid);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFEEE8AA.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color PaleGoldenrod => new Color(KnownColor.PaleGoldenrod);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF98FB98.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color PaleGreen => new Color(KnownColor.PaleGreen);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFAFEEEE.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color PaleTurquoise => new Color(KnownColor.PaleTurquoise);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFDB7093.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color PaleVioletRed => new Color(KnownColor.PaleVioletRed);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFEFD5.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color PapayaWhip => new Color(KnownColor.PapayaWhip);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFDAB9.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color PeachPuff => new Color(KnownColor.PeachPuff);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFCD853F.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Peru => new Color(KnownColor.Peru);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFC0CB.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Pink => new Color(KnownColor.Pink);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFDDA0DD.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Plum => new Color(KnownColor.Plum);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFB0E0E6.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color PowderBlue => new Color(KnownColor.PowderBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF800080.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Purple => new Color(KnownColor.Purple);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFF0000.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Red => new Color(KnownColor.Red);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFBC8F8F.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color RosyBrown => new Color(KnownColor.RosyBrown);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF4169E1.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color RoyalBlue => new Color(KnownColor.RoyalBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF8B4513.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color SaddleBrown => new Color(KnownColor.SaddleBrown);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFA8072.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Salmon => new Color(KnownColor.Salmon);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFF4A460.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color SandyBrown => new Color(KnownColor.SandyBrown);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF2E8B57.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color SeaGreen => new Color(KnownColor.SeaGreen);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFF5EE.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color SeaShell => new Color(KnownColor.SeaShell);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFA0522D.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Sienna => new Color(KnownColor.Sienna);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFC0C0C0.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Silver => new Color(KnownColor.Silver);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF87CEEB.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color SkyBlue => new Color(KnownColor.SkyBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF6A5ACD.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color SlateBlue => new Color(KnownColor.SlateBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF708090.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color SlateGray => new Color(KnownColor.SlateGray);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFFAFA.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Snow => new Color(KnownColor.Snow);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF00FF7F.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color SpringGreen => new Color(KnownColor.SpringGreen);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF4682B4.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color SteelBlue => new Color(KnownColor.SteelBlue);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFD2B48C.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Tan => new Color(KnownColor.Tan);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF008080.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Teal => new Color(KnownColor.Teal);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFD8BFD8.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Thistle => new Color(KnownColor.Thistle);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFF6347.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Tomato => new Color(KnownColor.Tomato);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF40E0D0.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Turquoise => new Color(KnownColor.Turquoise);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFEE82EE.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Violet => new Color(KnownColor.Violet);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFF5DEB3.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Wheat => new Color(KnownColor.Wheat);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFFFFF.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color White => new Color(KnownColor.White);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFF5F5F5.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color WhiteSmoke => new Color(KnownColor.WhiteSmoke);

	/// <summary>Gets a system-defined color that has an ARGB value of #FFFFFF00.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color Yellow => new Color(KnownColor.Yellow);

	/// <summary>Gets a system-defined color that has an ARGB value of #FF9ACD32.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> representing a system-defined color.</returns>
	/// <filterpriority>1</filterpriority>
	public static Color YellowGreen => new Color(KnownColor.YellowGreen);

	/// <summary>Gets the red component value of this <see cref="T:System.Drawing.Color" /> structure.</summary>
	/// <returns>The red component value of this <see cref="T:System.Drawing.Color" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public byte R => (byte)((Value >> 16) & 0xFF);

	/// <summary>Gets the green component value of this <see cref="T:System.Drawing.Color" /> structure.</summary>
	/// <returns>The green component value of this <see cref="T:System.Drawing.Color" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public byte G => (byte)((Value >> 8) & 0xFF);

	/// <summary>Gets the blue component value of this <see cref="T:System.Drawing.Color" /> structure.</summary>
	/// <returns>The blue component value of this <see cref="T:System.Drawing.Color" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public byte B => (byte)(Value & 0xFF);

	/// <summary>Gets the alpha component value of this <see cref="T:System.Drawing.Color" /> structure.</summary>
	/// <returns>The alpha component value of this <see cref="T:System.Drawing.Color" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public byte A => (byte)((Value >> 24) & 0xFF);

	/// <summary>Gets a value indicating whether this <see cref="T:System.Drawing.Color" /> structure is a predefined color. Predefined colors are represented by the elements of the <see cref="T:System.Drawing.KnownColor" /> enumeration.</summary>
	/// <returns>true if this <see cref="T:System.Drawing.Color" /> was created from a predefined color by using either the <see cref="M:System.Drawing.Color.FromName(System.String)" /> method or the <see cref="M:System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor)" /> method; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public bool IsKnownColor => (state & 1) != 0;

	/// <summary>Specifies whether this <see cref="T:System.Drawing.Color" /> structure is uninitialized.</summary>
	/// <returns>This property returns true if this color is uninitialized; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public bool IsEmpty => state == 0;

	/// <summary>Gets a value indicating whether this <see cref="T:System.Drawing.Color" /> structure is a named color or a member of the <see cref="T:System.Drawing.KnownColor" /> enumeration.</summary>
	/// <returns>true if this <see cref="T:System.Drawing.Color" /> was created by using either the <see cref="M:System.Drawing.Color.FromName(System.String)" /> method or the <see cref="M:System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor)" /> method; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public bool IsNamedColor
	{
		get
		{
			if ((state & 8) == 0)
			{
				return IsKnownColor;
			}
			return true;
		}
	}

	/// <summary>Gets a value indicating whether this <see cref="T:System.Drawing.Color" /> structure is a system color. A system color is a color that is used in a Windows display element. System colors are represented by elements of the <see cref="T:System.Drawing.KnownColor" /> enumeration.</summary>
	/// <returns>true if this <see cref="T:System.Drawing.Color" /> was created from a system color by using either the <see cref="M:System.Drawing.Color.FromName(System.String)" /> method or the <see cref="M:System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor)" /> method; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public bool IsSystemColor
	{
		get
		{
			if (IsKnownColor)
			{
				if (knownColor > 26)
				{
					return knownColor > 167;
				}
				return true;
			}
			return false;
		}
	}

	private string NameAndARGBValue => $"{{Name={Name}, ARGB=({A}, {R}, {G}, {B})}}";

	/// <summary>Gets the name of this <see cref="T:System.Drawing.Color" />.</summary>
	/// <returns>The name of this <see cref="T:System.Drawing.Color" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string Name
	{
		get
		{
			if ((state & 8) != 0)
			{
				return name;
			}
			if (IsKnownColor)
			{
				return KnownColorTable.KnownColorToName((KnownColor)knownColor);
			}
			return Convert.ToString(value, 16);
		}
	}

	private long Value
	{
		get
		{
			if ((state & 2) != 0)
			{
				return value;
			}
			if (IsKnownColor)
			{
				return KnownColorTable.KnownColorToArgb((KnownColor)knownColor);
			}
			return 0L;
		}
	}

	internal Color(KnownColor knownColor)
	{
		value = 0L;
		state = 1;
		name = null;
		this.knownColor = (short)knownColor;
	}

	private Color(long value, short state, string name, KnownColor knownColor)
	{
		this.value = value;
		this.state = state;
		this.name = name;
		this.knownColor = (short)knownColor;
	}

	private static void CheckByte(int value, string name)
	{
		if (value < 0 || value > 255)
		{
			throw new ArgumentException(global::SR.Format("Value of '{1}' is not valid for '{0}'. '{0}' should be greater than or equal to {2} and less than or equal to {3}.", name, value, 0, 255));
		}
	}

	private static long MakeArgb(byte alpha, byte red, byte green, byte blue)
	{
		return (long)(uint)((red << 16) | (green << 8) | blue | (alpha << 24)) & 0xFFFFFFFFL;
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Color" /> structure from a 32-bit ARGB value.</summary>
	/// <returns>The <see cref="T:System.Drawing.Color" /> structure that this method creates.</returns>
	/// <param name="argb">A value specifying the 32-bit ARGB value. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Color FromArgb(int argb)
	{
		return new Color(argb & 0xFFFFFFFFu, 2, null, (KnownColor)0);
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Color" /> structure from the four ARGB component (alpha, red, green, and blue) values. Although this method allows a 32-bit value to be passed for each component, the value of each component is limited to 8 bits.</summary>
	/// <returns>The <see cref="T:System.Drawing.Color" /> that this method creates.</returns>
	/// <param name="alpha">The alpha component. Valid values are 0 through 255. </param>
	/// <param name="red">The red component. Valid values are 0 through 255. </param>
	/// <param name="green">The green component. Valid values are 0 through 255. </param>
	/// <param name="blue">The blue component. Valid values are 0 through 255. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="alpha" />, <paramref name="red" />, <paramref name="green" />, or <paramref name="blue" /> is less than 0 or greater than 255.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Color FromArgb(int alpha, int red, int green, int blue)
	{
		CheckByte(alpha, "alpha");
		CheckByte(red, "red");
		CheckByte(green, "green");
		CheckByte(blue, "blue");
		return new Color(MakeArgb((byte)alpha, (byte)red, (byte)green, (byte)blue), 2, null, (KnownColor)0);
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Color" /> structure from the specified <see cref="T:System.Drawing.Color" /> structure, but with the new specified alpha value. Although this method allows a 32-bit value to be passed for the alpha value, the value is limited to 8 bits.</summary>
	/// <returns>The <see cref="T:System.Drawing.Color" /> that this method creates.</returns>
	/// <param name="alpha">The alpha value for the new <see cref="T:System.Drawing.Color" />. Valid values are 0 through 255. </param>
	/// <param name="baseColor">The <see cref="T:System.Drawing.Color" /> from which to create the new <see cref="T:System.Drawing.Color" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="alpha" /> is less than 0 or greater than 255.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Color FromArgb(int alpha, Color baseColor)
	{
		CheckByte(alpha, "alpha");
		return new Color(MakeArgb((byte)alpha, baseColor.R, baseColor.G, baseColor.B), 2, null, (KnownColor)0);
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Color" /> structure from the specified 8-bit color values (red, green, and blue). The alpha value is implicitly 255 (fully opaque). Although this method allows a 32-bit value to be passed for each color component, the value of each component is limited to 8 bits.</summary>
	/// <returns>The <see cref="T:System.Drawing.Color" /> that this method creates.</returns>
	/// <param name="red">The red component value for the new <see cref="T:System.Drawing.Color" />. Valid values are 0 through 255. </param>
	/// <param name="green">The green component value for the new <see cref="T:System.Drawing.Color" />. Valid values are 0 through 255. </param>
	/// <param name="blue">The blue component value for the new <see cref="T:System.Drawing.Color" />. Valid values are 0 through 255. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="red" />, <paramref name="green" />, or <paramref name="blue" /> is less than 0 or greater than 255.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Color FromArgb(int red, int green, int blue)
	{
		return FromArgb(255, red, green, blue);
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Color" /> structure from the specified predefined color.</summary>
	/// <returns>The <see cref="T:System.Drawing.Color" /> that this method creates.</returns>
	/// <param name="color">An element of the <see cref="T:System.Drawing.KnownColor" /> enumeration. </param>
	/// <filterpriority>1</filterpriority>
	public static Color FromKnownColor(KnownColor color)
	{
		if (color > (KnownColor)0 && color <= KnownColor.MenuHighlight)
		{
			return new Color(color);
		}
		return FromName(color.ToString());
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Color" /> structure from the specified name of a predefined color.</summary>
	/// <returns>The <see cref="T:System.Drawing.Color" /> that this method creates.</returns>
	/// <param name="name">A string that is the name of a predefined color. Valid names are the same as the names of the elements of the <see cref="T:System.Drawing.KnownColor" /> enumeration. </param>
	/// <filterpriority>1</filterpriority>
	public static Color FromName(string name)
	{
		if (ColorTable.TryGetNamedColor(name, out var result))
		{
			return result;
		}
		return new Color(0L, 8, name, (KnownColor)0);
	}

	/// <summary>Gets the hue-saturation-brightness (HSB) brightness value for this <see cref="T:System.Drawing.Color" /> structure.</summary>
	/// <returns>The brightness of this <see cref="T:System.Drawing.Color" />. The brightness ranges from 0.0 through 1.0, where 0.0 represents black and 1.0 represents white.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public float GetBrightness()
	{
		float num = (float)(int)R / 255f;
		float num2 = (float)(int)G / 255f;
		float num3 = (float)(int)B / 255f;
		float num4 = num;
		float num5 = num;
		if (num2 > num4)
		{
			num4 = num2;
		}
		else if (num2 < num5)
		{
			num5 = num2;
		}
		if (num3 > num4)
		{
			num4 = num3;
		}
		else if (num3 < num5)
		{
			num5 = num3;
		}
		return (num4 + num5) / 2f;
	}

	/// <summary>Gets the hue-saturation-brightness (HSB) hue value, in degrees, for this <see cref="T:System.Drawing.Color" /> structure.</summary>
	/// <returns>The hue, in degrees, of this <see cref="T:System.Drawing.Color" />. The hue is measured in degrees, ranging from 0.0 through 360.0, in HSB color space.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public float GetHue()
	{
		if (R == G && G == B)
		{
			return 0f;
		}
		float num = (float)(int)R / 255f;
		float num2 = (float)(int)G / 255f;
		float num3 = (float)(int)B / 255f;
		float num4 = num;
		float num5 = num;
		if (num2 > num4)
		{
			num4 = num2;
		}
		else if (num2 < num5)
		{
			num5 = num2;
		}
		if (num3 > num4)
		{
			num4 = num3;
		}
		else if (num3 < num5)
		{
			num5 = num3;
		}
		float num6 = num4 - num5;
		float num7 = ((num == num4) ? ((num2 - num3) / num6) : ((num2 != num4) ? (4f + (num - num2) / num6) : (2f + (num3 - num) / num6)));
		num7 *= 60f;
		if (num7 < 0f)
		{
			num7 += 360f;
		}
		return num7;
	}

	/// <summary>Gets the hue-saturation-brightness (HSB) saturation value for this <see cref="T:System.Drawing.Color" /> structure.</summary>
	/// <returns>The saturation of this <see cref="T:System.Drawing.Color" />. The saturation ranges from 0.0 through 1.0, where 0.0 is grayscale and 1.0 is the most saturated.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public float GetSaturation()
	{
		float num = (float)(int)R / 255f;
		float num2 = (float)(int)G / 255f;
		float num3 = (float)(int)B / 255f;
		float result = 0f;
		float num4 = num;
		float num5 = num;
		if (num2 > num4)
		{
			num4 = num2;
		}
		else if (num2 < num5)
		{
			num5 = num2;
		}
		if (num3 > num4)
		{
			num4 = num3;
		}
		else if (num3 < num5)
		{
			num5 = num3;
		}
		if (num4 != num5)
		{
			result = ((!((double)((num4 + num5) / 2f) <= 0.5)) ? ((num4 - num5) / (2f - num4 - num5)) : ((num4 - num5) / (num4 + num5)));
		}
		return result;
	}

	/// <summary>Gets the 32-bit ARGB value of this <see cref="T:System.Drawing.Color" /> structure.</summary>
	/// <returns>The 32-bit ARGB value of this <see cref="T:System.Drawing.Color" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int ToArgb()
	{
		return (int)Value;
	}

	/// <summary>Gets the <see cref="T:System.Drawing.KnownColor" /> value of this <see cref="T:System.Drawing.Color" /> structure.</summary>
	/// <returns>An element of the <see cref="T:System.Drawing.KnownColor" /> enumeration, if the <see cref="T:System.Drawing.Color" /> is created from a predefined color by using either the <see cref="M:System.Drawing.Color.FromName(System.String)" /> method or the <see cref="M:System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor)" /> method; otherwise, 0.</returns>
	/// <filterpriority>1</filterpriority>
	public KnownColor ToKnownColor()
	{
		return (KnownColor)knownColor;
	}

	/// <summary>Converts this <see cref="T:System.Drawing.Color" /> structure to a human-readable string.</summary>
	/// <returns>A string that is the name of this <see cref="T:System.Drawing.Color" />, if the <see cref="T:System.Drawing.Color" /> is created from a predefined color by using either the <see cref="M:System.Drawing.Color.FromName(System.String)" /> method or the <see cref="M:System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor)" /> method; otherwise, a string that consists of the ARGB component names and their values.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override string ToString()
	{
		if ((state & 8) != 0 || (state & 1) != 0)
		{
			return "Color [" + Name + "]";
		}
		if ((state & 2) != 0)
		{
			return "Color [A=" + A + ", R=" + R + ", G=" + G + ", B=" + B + "]";
		}
		return "Color [Empty]";
	}

	/// <summary>Tests whether two specified <see cref="T:System.Drawing.Color" /> structures are equivalent.</summary>
	/// <returns>true if the two <see cref="T:System.Drawing.Color" /> structures are equal; otherwise, false.</returns>
	/// <param name="left">The <see cref="T:System.Drawing.Color" /> that is to the left of the equality operator. </param>
	/// <param name="right">The <see cref="T:System.Drawing.Color" /> that is to the right of the equality operator. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator ==(Color left, Color right)
	{
		if (left.value == right.value && left.state == right.state && left.knownColor == right.knownColor)
		{
			return left.name == right.name;
		}
		return false;
	}

	/// <summary>Tests whether two specified <see cref="T:System.Drawing.Color" /> structures are different.</summary>
	/// <returns>true if the two <see cref="T:System.Drawing.Color" /> structures are different; otherwise, false.</returns>
	/// <param name="left">The <see cref="T:System.Drawing.Color" /> that is to the left of the inequality operator. </param>
	/// <param name="right">The <see cref="T:System.Drawing.Color" /> that is to the right of the inequality operator. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator !=(Color left, Color right)
	{
		return !(left == right);
	}

	/// <summary>Tests whether the specified object is a <see cref="T:System.Drawing.Color" /> structure and is equivalent to this <see cref="T:System.Drawing.Color" /> structure.</summary>
	/// <returns>true if <paramref name="obj" /> is a <see cref="T:System.Drawing.Color" /> structure equivalent to this <see cref="T:System.Drawing.Color" /> structure; otherwise, false.</returns>
	/// <param name="obj">The object to test. </param>
	/// <filterpriority>1</filterpriority>
	public override bool Equals(object obj)
	{
		if (obj is Color)
		{
			return Equals((Color)obj);
		}
		return false;
	}

	public bool Equals(Color other)
	{
		return this == other;
	}

	/// <summary>Returns a hash code for this <see cref="T:System.Drawing.Color" /> structure.</summary>
	/// <returns>An integer value that specifies the hash code for this <see cref="T:System.Drawing.Color" />.</returns>
	/// <filterpriority>1</filterpriority>
	public override int GetHashCode()
	{
		if ((name != null) & !IsKnownColor)
		{
			return name.GetHashCode();
		}
		return System.Numerics.Hashing.HashHelpers.Combine(System.Numerics.Hashing.HashHelpers.Combine(value.GetHashCode(), state.GetHashCode()), knownColor.GetHashCode());
	}
}
