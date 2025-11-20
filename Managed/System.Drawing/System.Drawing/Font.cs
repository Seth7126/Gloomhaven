using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Drawing;

/// <summary>Defines a particular format for text, including font face, size, and style attributes. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[TypeConverter(typeof(FontConverter))]
[Editor("System.Drawing.Design.FontEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
[ComVisible(true)]
public sealed class Font : MarshalByRefObject, ISerializable, ICloneable, IDisposable
{
	private IntPtr fontObject = IntPtr.Zero;

	private string systemFontName;

	private string originalFontName;

	private float _size;

	private object olf;

	private const byte DefaultCharSet = 1;

	private static int CharSetOffset = -1;

	private bool _bold;

	private FontFamily _fontFamily;

	private byte _gdiCharSet;

	private bool _gdiVerticalFont;

	private bool _italic;

	private string _name;

	private float _sizeInPoints;

	private bool _strikeout;

	private FontStyle _style;

	private bool _underline;

	private GraphicsUnit _unit;

	private int _hashCode;

	internal IntPtr NativeObject => fontObject;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Drawing.Font" /> is bold.</summary>
	/// <returns>true if this <see cref="T:System.Drawing.Font" /> is bold; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool Bold => _bold;

	/// <summary>Gets the <see cref="T:System.Drawing.FontFamily" /> associated with this <see cref="T:System.Drawing.Font" />.</summary>
	/// <returns>The <see cref="T:System.Drawing.FontFamily" /> associated with this <see cref="T:System.Drawing.Font" />.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public FontFamily FontFamily => _fontFamily;

	/// <summary>Gets a byte value that specifies the GDI character set that this <see cref="T:System.Drawing.Font" /> uses.</summary>
	/// <returns>A byte value that specifies the GDI character set that this <see cref="T:System.Drawing.Font" /> uses. The default is 1.</returns>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public byte GdiCharSet => _gdiCharSet;

	/// <summary>Gets a Boolean value that indicates whether this <see cref="T:System.Drawing.Font" /> is derived from a GDI vertical font.</summary>
	/// <returns>true if this <see cref="T:System.Drawing.Font" /> is derived from a GDI vertical font; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool GdiVerticalFont => _gdiVerticalFont;

	/// <summary>Gets the line spacing of this font.</summary>
	/// <returns>The line spacing, in pixels, of this font. </returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Browsable(false)]
	public int Height => (int)Math.Ceiling(GetHeight());

	/// <summary>Gets a value indicating whether the font is a member of <see cref="T:System.Drawing.SystemFonts" />. </summary>
	/// <returns>true if the font is a member of <see cref="T:System.Drawing.SystemFonts" />; otherwise, false. The default is false.</returns>
	[Browsable(false)]
	public bool IsSystemFont => !string.IsNullOrEmpty(systemFontName);

	/// <summary>Gets a value that indicates whether this font has the italic style applied.</summary>
	/// <returns>true to indicate this font has the italic style applied; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool Italic => _italic;

	/// <summary>Gets the face name of this <see cref="T:System.Drawing.Font" />.</summary>
	/// <returns>A string representation of the face name of this <see cref="T:System.Drawing.Font" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[TypeConverter(typeof(FontConverter.FontNameConverter))]
	[Editor("System.Drawing.Design.FontNameEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
	public string Name => _name;

	/// <summary>Gets the em-size of this <see cref="T:System.Drawing.Font" /> measured in the units specified by the <see cref="P:System.Drawing.Font.Unit" /> property.</summary>
	/// <returns>The em-size of this <see cref="T:System.Drawing.Font" />.</returns>
	/// <filterpriority>1</filterpriority>
	public float Size => _size;

	/// <summary>Gets the em-size, in points, of this <see cref="T:System.Drawing.Font" />.</summary>
	/// <returns>The em-size, in points, of this <see cref="T:System.Drawing.Font" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Browsable(false)]
	public float SizeInPoints => _sizeInPoints;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Drawing.Font" /> specifies a horizontal line through the font.</summary>
	/// <returns>true if this <see cref="T:System.Drawing.Font" /> has a horizontal line through it; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool Strikeout => _strikeout;

	/// <summary>Gets style information for this <see cref="T:System.Drawing.Font" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.FontStyle" /> enumeration that contains style information for this <see cref="T:System.Drawing.Font" />.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public FontStyle Style => _style;

	/// <summary>Gets the name of the system font if the <see cref="P:System.Drawing.Font.IsSystemFont" /> property returns true.</summary>
	/// <returns>The name of the system font, if <see cref="P:System.Drawing.Font.IsSystemFont" /> returns true; otherwise, an empty string ("").</returns>
	[Browsable(false)]
	public string SystemFontName => systemFontName;

	/// <summary>Gets the name of the font originally specified.</summary>
	/// <returns>The string representing the name of the font originally specified.</returns>
	[Browsable(false)]
	public string OriginalFontName => originalFontName;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Drawing.Font" /> is underlined.</summary>
	/// <returns>true if this <see cref="T:System.Drawing.Font" /> is underlined; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool Underline => _underline;

	/// <summary>Gets the unit of measure for this <see cref="T:System.Drawing.Font" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.GraphicsUnit" /> that represents the unit of measure for this <see cref="T:System.Drawing.Font" />.</returns>
	/// <filterpriority>1</filterpriority>
	[TypeConverter(typeof(FontConverter.FontUnitConverter))]
	public GraphicsUnit Unit => _unit;

	private void CreateFont(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical)
	{
		originalFontName = familyName;
		FontFamily fontFamily;
		try
		{
			fontFamily = new FontFamily(familyName);
		}
		catch (Exception)
		{
			fontFamily = FontFamily.GenericSansSerif;
		}
		setProperties(fontFamily, emSize, style, unit, charSet, isVertical);
		Status status = GDIPlus.GdipCreateFont(fontFamily.NativeFamily, emSize, style, unit, out fontObject);
		if (status == Status.FontStyleNotFound)
		{
			throw new ArgumentException(global::Locale.GetText("Style {0} isn't supported by font {1}.", style.ToString(), familyName));
		}
		GDIPlus.CheckStatus(status);
	}

	private Font(SerializationInfo info, StreamingContext context)
	{
		string familyName = (string)info.GetValue("Name", typeof(string));
		float emSize = (float)info.GetValue("Size", typeof(float));
		FontStyle style = (FontStyle)info.GetValue("Style", typeof(FontStyle));
		GraphicsUnit unit = (GraphicsUnit)info.GetValue("Unit", typeof(GraphicsUnit));
		CreateFont(familyName, emSize, style, unit, 1, isVertical: false);
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.</summary>
	/// <param name="si">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data.</param>
	/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization.</param>
	void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
	{
		si.AddValue("Name", Name);
		si.AddValue("Size", Size);
		si.AddValue("Style", Style);
		si.AddValue("Unit", Unit);
	}

	~Font()
	{
		Dispose();
	}

	/// <summary>Releases all resources used by this <see cref="T:System.Drawing.Font" />.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public void Dispose()
	{
		if (fontObject != IntPtr.Zero)
		{
			Status status = GDIPlus.GdipDeleteFont(fontObject);
			fontObject = IntPtr.Zero;
			GC.SuppressFinalize(this);
			GDIPlus.CheckStatus(status);
		}
	}

	internal void SetSystemFontName(string newSystemFontName)
	{
		systemFontName = newSystemFontName;
	}

	internal void unitConversion(GraphicsUnit fromUnit, GraphicsUnit toUnit, float nSrc, out float nTrg)
	{
		float num = 0f;
		nTrg = 0f;
		switch (fromUnit)
		{
		case GraphicsUnit.Display:
			num = nSrc / 75f;
			break;
		case GraphicsUnit.Document:
			num = nSrc / 300f;
			break;
		case GraphicsUnit.Inch:
			num = nSrc;
			break;
		case GraphicsUnit.Millimeter:
			num = nSrc / 25.4f;
			break;
		case GraphicsUnit.World:
		case GraphicsUnit.Pixel:
			num = nSrc / Graphics.systemDpiX;
			break;
		case GraphicsUnit.Point:
			num = nSrc / 72f;
			break;
		default:
			throw new ArgumentException("Invalid GraphicsUnit");
		}
		switch (toUnit)
		{
		case GraphicsUnit.Display:
			nTrg = num * 75f;
			break;
		case GraphicsUnit.Document:
			nTrg = num * 300f;
			break;
		case GraphicsUnit.Inch:
			nTrg = num;
			break;
		case GraphicsUnit.Millimeter:
			nTrg = num * 25.4f;
			break;
		case GraphicsUnit.World:
		case GraphicsUnit.Pixel:
			nTrg = num * Graphics.systemDpiX;
			break;
		case GraphicsUnit.Point:
			nTrg = num * 72f;
			break;
		default:
			throw new ArgumentException("Invalid GraphicsUnit");
		}
	}

	private void setProperties(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical)
	{
		_name = family.Name;
		_fontFamily = family;
		_size = emSize;
		_unit = unit;
		_style = style;
		_gdiCharSet = charSet;
		_gdiVerticalFont = isVertical;
		unitConversion(unit, GraphicsUnit.Point, emSize, out _sizeInPoints);
		_bold = (_italic = (_strikeout = (_underline = false)));
		if ((style & FontStyle.Bold) == FontStyle.Bold)
		{
			_bold = true;
		}
		if ((style & FontStyle.Italic) == FontStyle.Italic)
		{
			_italic = true;
		}
		if ((style & FontStyle.Strikeout) == FontStyle.Strikeout)
		{
			_strikeout = true;
		}
		if ((style & FontStyle.Underline) == FontStyle.Underline)
		{
			_underline = true;
		}
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Font" /> from the specified Windows handle.</summary>
	/// <returns>The <see cref="T:System.Drawing.Font" /> this method creates.</returns>
	/// <param name="hfont">A Windows handle to a GDI font. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="hfont" /> points to an object that is not a TrueType font.</exception>
	/// <filterpriority>1</filterpriority>
	public static Font FromHfont(IntPtr hfont)
	{
		FontStyle fontStyle = FontStyle.Regular;
		LOGFONT lf = default(LOGFONT);
		if (hfont == IntPtr.Zero)
		{
			return new Font("Arial", 10f, FontStyle.Regular);
		}
		if (GDIPlus.RunningOnUnix())
		{
			GDIPlus.CheckStatus(GDIPlus.GdipCreateFontFromHfont(hfont, out var font, ref lf));
			if (lf.lfItalic != 0)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (lf.lfUnderline != 0)
			{
				fontStyle |= FontStyle.Underline;
			}
			if (lf.lfStrikeOut != 0)
			{
				fontStyle |= FontStyle.Strikeout;
			}
			if (lf.lfWeight > 400)
			{
				fontStyle |= FontStyle.Bold;
			}
			return new Font(size: (lf.lfHeight >= 0) ? ((float)lf.lfHeight) : ((float)(lf.lfHeight * -1)), newFontObject: font, familyName: lf.lfFaceName, style: fontStyle);
		}
		fontStyle = FontStyle.Regular;
		IntPtr dC = GDIPlus.GetDC(IntPtr.Zero);
		try
		{
			return FromLogFont(lf, dC);
		}
		finally
		{
			GDIPlus.ReleaseDC(IntPtr.Zero, dC);
		}
	}

	/// <summary>Returns a handle to this <see cref="T:System.Drawing.Font" />.</summary>
	/// <returns>A Windows handle to this <see cref="T:System.Drawing.Font" />.</returns>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operation was unsuccessful.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public IntPtr ToHfont()
	{
		if (fontObject == IntPtr.Zero)
		{
			throw new ArgumentException(global::Locale.GetText("Object has been disposed."));
		}
		if (GDIPlus.RunningOnUnix())
		{
			return fontObject;
		}
		if (olf == null)
		{
			olf = default(LOGFONT);
			ToLogFont(olf);
		}
		LOGFONT logfont = (LOGFONT)olf;
		return GDIPlus.CreateFontIndirect(ref logfont);
	}

	internal Font(IntPtr newFontObject, string familyName, FontStyle style, float size)
	{
		FontFamily family;
		try
		{
			family = new FontFamily(familyName);
		}
		catch (Exception)
		{
			family = FontFamily.GenericSansSerif;
		}
		setProperties(family, size, style, GraphicsUnit.Pixel, 0, isVertical: false);
		fontObject = newFontObject;
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Font" /> that uses the specified existing <see cref="T:System.Drawing.Font" /> and <see cref="T:System.Drawing.FontStyle" /> enumeration.</summary>
	/// <param name="prototype">The existing <see cref="T:System.Drawing.Font" /> from which to create the new <see cref="T:System.Drawing.Font" />. </param>
	/// <param name="newStyle">The <see cref="T:System.Drawing.FontStyle" /> to apply to the new <see cref="T:System.Drawing.Font" />. Multiple values of the <see cref="T:System.Drawing.FontStyle" /> enumeration can be combined with the OR operator. </param>
	public Font(Font prototype, FontStyle newStyle)
	{
		setProperties(prototype.FontFamily, prototype.Size, newStyle, prototype.Unit, prototype.GdiCharSet, prototype.GdiVerticalFont);
		GDIPlus.CheckStatus(GDIPlus.GdipCreateFont(_fontFamily.NativeFamily, Size, Style, Unit, out fontObject));
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Font" /> using a specified size and unit. Sets the style to <see cref="F:System.Drawing.FontStyle.Regular" />.</summary>
	/// <param name="family">The <see cref="T:System.Drawing.FontFamily" /> of the new <see cref="T:System.Drawing.Font" />. </param>
	/// <param name="emSize">The em-size of the new font in the units specified by the <paramref name="unit" /> parameter. </param>
	/// <param name="unit">The <see cref="T:System.Drawing.GraphicsUnit" /> of the new font. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="family" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="emSize" /> is less than or equal to 0, evaluates to infinity, or is not a valid number. </exception>
	public Font(FontFamily family, float emSize, GraphicsUnit unit)
		: this(family, emSize, FontStyle.Regular, unit, 1, gdiVerticalFont: false)
	{
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Font" /> using a specified size and unit. The style is set to <see cref="F:System.Drawing.FontStyle.Regular" />.</summary>
	/// <param name="familyName">A string representation of the <see cref="T:System.Drawing.FontFamily" /> for the new <see cref="T:System.Drawing.Font" />. </param>
	/// <param name="emSize">The em-size of the new font in the units specified by the <paramref name="unit" /> parameter. </param>
	/// <param name="unit">The <see cref="T:System.Drawing.GraphicsUnit" /> of the new font. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="emSize" /> is less than or equal to 0, evaluates to infinity, or is not a valid number. </exception>
	public Font(string familyName, float emSize, GraphicsUnit unit)
		: this(new FontFamily(familyName), emSize, FontStyle.Regular, unit, 1, gdiVerticalFont: false)
	{
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Font" /> using a specified size. </summary>
	/// <param name="family">The <see cref="T:System.Drawing.FontFamily" /> of the new <see cref="T:System.Drawing.Font" />. </param>
	/// <param name="emSize">The em-size, in points, of the new font. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="emSize" /> is less than or equal to 0, evaluates to infinity, or is not a valid number. </exception>
	public Font(FontFamily family, float emSize)
		: this(family, emSize, FontStyle.Regular, GraphicsUnit.Point, 1, gdiVerticalFont: false)
	{
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Font" /> using a specified size and style. </summary>
	/// <param name="family">The <see cref="T:System.Drawing.FontFamily" /> of the new <see cref="T:System.Drawing.Font" />. </param>
	/// <param name="emSize">The em-size, in points, of the new font. </param>
	/// <param name="style">The <see cref="T:System.Drawing.FontStyle" /> of the new font. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="emSize" /> is less than or equal to 0, evaluates to infinity, or is not a valid number. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="family" /> is null.</exception>
	public Font(FontFamily family, float emSize, FontStyle style)
		: this(family, emSize, style, GraphicsUnit.Point, 1, gdiVerticalFont: false)
	{
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Font" /> using a specified size, style, and unit.</summary>
	/// <param name="family">The <see cref="T:System.Drawing.FontFamily" /> of the new <see cref="T:System.Drawing.Font" />. </param>
	/// <param name="emSize">The em-size of the new font in the units specified by the <paramref name="unit" /> parameter. </param>
	/// <param name="style">The <see cref="T:System.Drawing.FontStyle" /> of the new font. </param>
	/// <param name="unit">The <see cref="T:System.Drawing.GraphicsUnit" /> of the new font. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="emSize" /> is less than or equal to 0, evaluates to infinity, or is not a valid number. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="family" /> is null.</exception>
	public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit)
		: this(family, emSize, style, unit, 1, gdiVerticalFont: false)
	{
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Font" /> using a specified size, style, unit, and character set.</summary>
	/// <param name="family">The <see cref="T:System.Drawing.FontFamily" /> of the new <see cref="T:System.Drawing.Font" />. </param>
	/// <param name="emSize">The em-size of the new font in the units specified by the <paramref name="unit" /> parameter. </param>
	/// <param name="style">The <see cref="T:System.Drawing.FontStyle" /> of the new font. </param>
	/// <param name="unit">The <see cref="T:System.Drawing.GraphicsUnit" /> of the new font. </param>
	/// <param name="gdiCharSet">A <see cref="T:System.Byte" /> that specifies a GDI character set to use for the new font. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="emSize" /> is less than or equal to 0, evaluates to infinity, or is not a valid number. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="family" /> is null.</exception>
	public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet)
		: this(family, emSize, style, unit, gdiCharSet, gdiVerticalFont: false)
	{
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Font" /> using a specified size, style, unit, and character set.</summary>
	/// <param name="family">The <see cref="T:System.Drawing.FontFamily" /> of the new <see cref="T:System.Drawing.Font" />. </param>
	/// <param name="emSize">The em-size of the new font in the units specified by the <paramref name="unit" /> parameter. </param>
	/// <param name="style">The <see cref="T:System.Drawing.FontStyle" /> of the new font. </param>
	/// <param name="unit">The <see cref="T:System.Drawing.GraphicsUnit" /> of the new font. </param>
	/// <param name="gdiCharSet">A <see cref="T:System.Byte" /> that specifies a GDI character set to use for this font. </param>
	/// <param name="gdiVerticalFont">A Boolean value indicating whether the new font is derived from a GDI vertical font. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="emSize" /> is less than or equal to 0, evaluates to infinity, or is not a valid number. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="family" /> is null </exception>
	public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont)
	{
		if (family == null)
		{
			throw new ArgumentNullException("family");
		}
		setProperties(family, emSize, style, unit, gdiCharSet, gdiVerticalFont);
		GDIPlus.CheckStatus(GDIPlus.GdipCreateFont(family.NativeFamily, emSize, style, unit, out fontObject));
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Font" /> using a specified size. </summary>
	/// <param name="familyName">A string representation of the <see cref="T:System.Drawing.FontFamily" /> for the new <see cref="T:System.Drawing.Font" />. </param>
	/// <param name="emSize">The em-size, in points, of the new font. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="emSize" /> is less than or equal to 0, evaluates to infinity or is not a valid number. </exception>
	public Font(string familyName, float emSize)
		: this(familyName, emSize, FontStyle.Regular, GraphicsUnit.Point, 1, gdiVerticalFont: false)
	{
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Font" /> using a specified size and style. </summary>
	/// <param name="familyName">A string representation of the <see cref="T:System.Drawing.FontFamily" /> for the new <see cref="T:System.Drawing.Font" />. </param>
	/// <param name="emSize">The em-size, in points, of the new font. </param>
	/// <param name="style">The <see cref="T:System.Drawing.FontStyle" /> of the new font. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="emSize" /> is less than or equal to 0, evaluates to infinity, or is not a valid number. </exception>
	public Font(string familyName, float emSize, FontStyle style)
		: this(familyName, emSize, style, GraphicsUnit.Point, 1, gdiVerticalFont: false)
	{
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Font" /> using a specified size, style, and unit.</summary>
	/// <param name="familyName">A string representation of the <see cref="T:System.Drawing.FontFamily" /> for the new <see cref="T:System.Drawing.Font" />. </param>
	/// <param name="emSize">The em-size of the new font in the units specified by the <paramref name="unit" /> parameter. </param>
	/// <param name="style">The <see cref="T:System.Drawing.FontStyle" /> of the new font. </param>
	/// <param name="unit">The <see cref="T:System.Drawing.GraphicsUnit" /> of the new font. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="emSize" /> is less than or equal to 0, evaluates to infinity or is not a valid number. </exception>
	public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit)
		: this(familyName, emSize, style, unit, 1, gdiVerticalFont: false)
	{
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Font" /> using a specified size, style, unit, and character set.</summary>
	/// <param name="familyName">A string representation of the <see cref="T:System.Drawing.FontFamily" /> for the new <see cref="T:System.Drawing.Font" />. </param>
	/// <param name="emSize">The em-size of the new font in the units specified by the <paramref name="unit" /> parameter. </param>
	/// <param name="style">The <see cref="T:System.Drawing.FontStyle" /> of the new font. </param>
	/// <param name="unit">The <see cref="T:System.Drawing.GraphicsUnit" /> of the new font. </param>
	/// <param name="gdiCharSet">A <see cref="T:System.Byte" /> that specifies a GDI character set to use for this font. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="emSize" /> is less than or equal to 0, evaluates to infinity, or is not a valid number. </exception>
	public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet)
		: this(familyName, emSize, style, unit, gdiCharSet, gdiVerticalFont: false)
	{
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Font" /> using the specified size, style, unit, and character set.</summary>
	/// <param name="familyName">A string representation of the <see cref="T:System.Drawing.FontFamily" /> for the new <see cref="T:System.Drawing.Font" />. </param>
	/// <param name="emSize">The em-size of the new font in the units specified by the <paramref name="unit" /> parameter. </param>
	/// <param name="style">The <see cref="T:System.Drawing.FontStyle" /> of the new font. </param>
	/// <param name="unit">The <see cref="T:System.Drawing.GraphicsUnit" /> of the new font. </param>
	/// <param name="gdiCharSet">A <see cref="T:System.Byte" /> that specifies a GDI character set to use for this font. </param>
	/// <param name="gdiVerticalFont">A Boolean value indicating whether the new <see cref="T:System.Drawing.Font" /> is derived from a GDI vertical font. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="emSize" /> is less than or equal to 0, evaluates to infinity, or is not a valid number. </exception>
	public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont)
	{
		CreateFont(familyName, emSize, style, unit, gdiCharSet, gdiVerticalFont);
	}

	internal Font(string familyName, float emSize, string systemName)
		: this(familyName, emSize, FontStyle.Regular, GraphicsUnit.Point, 1, gdiVerticalFont: false)
	{
		systemFontName = systemName;
	}

	/// <summary>Creates an exact copy of this <see cref="T:System.Drawing.Font" />.</summary>
	/// <returns>The <see cref="T:System.Drawing.Font" /> this method creates, cast as an <see cref="T:System.Object" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public object Clone()
	{
		return new Font(this, Style);
	}

	/// <summary>Indicates whether the specified object is a <see cref="T:System.Drawing.Font" /> and has the same <see cref="P:System.Drawing.Font.FontFamily" />, <see cref="P:System.Drawing.Font.GdiVerticalFont" />, <see cref="P:System.Drawing.Font.GdiCharSet" />, <see cref="P:System.Drawing.Font.Style" />, <see cref="P:System.Drawing.Font.Size" />, and <see cref="P:System.Drawing.Font.Unit" /> property values as this <see cref="T:System.Drawing.Font" />.</summary>
	/// <returns>true if the <paramref name="obj" /> parameter is a <see cref="T:System.Drawing.Font" /> and has the same <see cref="P:System.Drawing.Font.FontFamily" />, <see cref="P:System.Drawing.Font.GdiVerticalFont" />, <see cref="P:System.Drawing.Font.GdiCharSet" />, <see cref="P:System.Drawing.Font.Style" />, <see cref="P:System.Drawing.Font.Size" />, and <see cref="P:System.Drawing.Font.Unit" /> property values as this <see cref="T:System.Drawing.Font" />; otherwise, false.</returns>
	/// <param name="obj">The object to test. </param>
	/// <filterpriority>1</filterpriority>
	public override bool Equals(object obj)
	{
		if (!(obj is Font font))
		{
			return false;
		}
		if (font.FontFamily.Equals(FontFamily) && font.Size == Size && font.Style == Style && font.Unit == Unit && font.GdiCharSet == GdiCharSet && font.GdiVerticalFont == GdiVerticalFont)
		{
			return true;
		}
		return false;
	}

	/// <summary>Gets the hash code for this <see cref="T:System.Drawing.Font" />.</summary>
	/// <returns>The hash code for this <see cref="T:System.Drawing.Font" />.</returns>
	/// <filterpriority>1</filterpriority>
	public override int GetHashCode()
	{
		if (_hashCode == 0)
		{
			_hashCode = 17;
			_hashCode = _hashCode * 23 + _name.GetHashCode();
			_hashCode = _hashCode * 23 + FontFamily.GetHashCode();
			_hashCode = _hashCode * 23 + _size.GetHashCode();
			_hashCode = _hashCode * 23 + _unit.GetHashCode();
			_hashCode = _hashCode * 23 + _style.GetHashCode();
			_hashCode = _hashCode * 23 + _gdiCharSet;
			_hashCode = _hashCode * 23 + _gdiVerticalFont.GetHashCode();
		}
		return _hashCode;
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Font" /> from the specified Windows handle to a device context.</summary>
	/// <returns>The <see cref="T:System.Drawing.Font" /> this method creates.</returns>
	/// <param name="hdc">A handle to a device context. </param>
	/// <exception cref="T:System.ArgumentException">The font for the specified device context is not a TrueType font.</exception>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("The hdc parameter has no direct equivalent in libgdiplus.")]
	public static Font FromHdc(IntPtr hdc)
	{
		throw new NotImplementedException();
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Font" /> from the specified GDI logical font (LOGFONT) structure.</summary>
	/// <returns>The <see cref="T:System.Drawing.Font" /> that this method creates.</returns>
	/// <param name="lf">An <see cref="T:System.Object" /> that represents the GDI LOGFONT structure from which to create the <see cref="T:System.Drawing.Font" />. </param>
	/// <param name="hdc">A handle to a device context that contains additional information about the <paramref name="lf" /> structure. </param>
	/// <exception cref="T:System.ArgumentException">The font is not a TrueType font.</exception>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("The returned font may not have all it's properties initialized correctly.")]
	public static Font FromLogFont(object lf, IntPtr hdc)
	{
		LOGFONT lf2 = (LOGFONT)lf;
		GDIPlus.CheckStatus(GDIPlus.GdipCreateFontFromLogfont(hdc, ref lf2, out var ptr));
		return new Font(ptr, "Microsoft Sans Serif", FontStyle.Regular, 10f);
	}

	/// <summary>Returns the line spacing, in pixels, of this font. </summary>
	/// <returns>The line spacing, in pixels, of this font.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public float GetHeight()
	{
		return GetHeight(Graphics.systemDpiY);
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Font" /> from the specified GDI logical font (LOGFONT) structure.</summary>
	/// <returns>The <see cref="T:System.Drawing.Font" /> that this method creates.</returns>
	/// <param name="lf">An <see cref="T:System.Object" /> that represents the GDI LOGFONT structure from which to create the <see cref="T:System.Drawing.Font" />. </param>
	/// <filterpriority>1</filterpriority>
	public static Font FromLogFont(object lf)
	{
		if (GDIPlus.RunningOnUnix())
		{
			return FromLogFont(lf, IntPtr.Zero);
		}
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = GDIPlus.GetDC(IntPtr.Zero);
			return FromLogFont(lf, intPtr);
		}
		finally
		{
			GDIPlus.ReleaseDC(IntPtr.Zero, intPtr);
		}
	}

	/// <summary>Creates a GDI logical font (LOGFONT) structure from this <see cref="T:System.Drawing.Font" />.</summary>
	/// <param name="logFont">An <see cref="T:System.Object" /> that represents the LOGFONT structure that this method creates. </param>
	/// <filterpriority>1</filterpriority>
	public void ToLogFont(object logFont)
	{
		if (GDIPlus.RunningOnUnix())
		{
			using (Bitmap image = new Bitmap(1, 1, PixelFormat.Format32bppArgb))
			{
				using Graphics graphics = Graphics.FromImage(image);
				ToLogFont(logFont, graphics);
				return;
			}
		}
		IntPtr dC = GDIPlus.GetDC(IntPtr.Zero);
		try
		{
			using Graphics graphics2 = Graphics.FromHdc(dC);
			ToLogFont(logFont, graphics2);
		}
		finally
		{
			GDIPlus.ReleaseDC(IntPtr.Zero, dC);
		}
	}

	/// <summary>Creates a GDI logical font (LOGFONT) structure from this <see cref="T:System.Drawing.Font" />.</summary>
	/// <param name="logFont">An <see cref="T:System.Object" /> that represents the LOGFONT structure that this method creates. </param>
	/// <param name="graphics">A <see cref="T:System.Drawing.Graphics" /> that provides additional information for the LOGFONT structure. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="graphics" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void ToLogFont(object logFont, Graphics graphics)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		if (logFont == null)
		{
			throw new AccessViolationException("logFont");
		}
		if (!logFont.GetType().GetTypeInfo().IsLayoutSequential)
		{
			throw new ArgumentException("logFont", global::Locale.GetText("Layout must be sequential."));
		}
		Type typeFromHandle = typeof(LOGFONT);
		int num = Marshal.SizeOf(logFont);
		if (num < Marshal.SizeOf(typeFromHandle))
		{
			return;
		}
		IntPtr intPtr = Marshal.AllocHGlobal(num);
		Status status;
		try
		{
			Marshal.StructureToPtr(logFont, intPtr, fDeleteOld: false);
			status = GDIPlus.GdipGetLogFont(NativeObject, graphics.NativeObject, logFont);
			if (status != Status.Ok)
			{
				Marshal.PtrToStructure(intPtr, logFont);
			}
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
		if (CharSetOffset == -1)
		{
			CharSetOffset = (int)Marshal.OffsetOf(typeFromHandle, "lfCharSet");
		}
		GCHandle gCHandle = GCHandle.Alloc(logFont, GCHandleType.Pinned);
		try
		{
			IntPtr ptr = gCHandle.AddrOfPinnedObject();
			if (Marshal.ReadByte(ptr, CharSetOffset) == 0)
			{
				Marshal.WriteByte(ptr, CharSetOffset, 1);
			}
		}
		finally
		{
			gCHandle.Free();
		}
		GDIPlus.CheckStatus(status);
	}

	/// <summary>Returns the line spacing, in the current unit of a specified <see cref="T:System.Drawing.Graphics" />, of this font. </summary>
	/// <returns>The line spacing, in pixels, of this font.</returns>
	/// <param name="graphics">A <see cref="T:System.Drawing.Graphics" /> that holds the vertical resolution, in dots per inch, of the display device as well as settings for page unit and page scale. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="graphics" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public float GetHeight(Graphics graphics)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipGetFontHeight(fontObject, graphics.NativeObject, out var height));
		return height;
	}

	/// <summary>Returns the height, in pixels, of this <see cref="T:System.Drawing.Font" /> when drawn to a device with the specified vertical resolution.</summary>
	/// <returns>The height, in pixels, of this <see cref="T:System.Drawing.Font" />.</returns>
	/// <param name="dpi">The vertical resolution, in dots per inch, used to calculate the height of the font. </param>
	/// <filterpriority>1</filterpriority>
	public float GetHeight(float dpi)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipGetFontHeightGivenDPI(fontObject, dpi, out var height));
		return height;
	}

	/// <summary>Returns a human-readable string representation of this <see cref="T:System.Drawing.Font" />.</summary>
	/// <returns>A string that represents this <see cref="T:System.Drawing.Font" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override string ToString()
	{
		return $"[Font: Name={_name}, Size={Size}, Units={(int)_unit}, GdiCharSet={_gdiCharSet}, GdiVerticalFont={_gdiVerticalFont}]";
	}
}
