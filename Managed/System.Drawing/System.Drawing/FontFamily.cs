using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace System.Drawing;

/// <summary>Defines a group of type faces having a similar basic design and certain variations in styles. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
public sealed class FontFamily : MarshalByRefObject, IDisposable
{
	private string name;

	private IntPtr nativeFontFamily = IntPtr.Zero;

	internal IntPtr NativeObject => nativeFontFamily;

	internal IntPtr NativeFamily => nativeFontFamily;

	/// <summary>Gets the name of this <see cref="T:System.Drawing.FontFamily" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that represents the name of this <see cref="T:System.Drawing.FontFamily" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public string Name
	{
		get
		{
			if (nativeFontFamily == IntPtr.Zero)
			{
				throw new ArgumentException("Name", global::Locale.GetText("Object was disposed."));
			}
			if (name == null)
			{
				refreshName();
			}
			return name;
		}
	}

	/// <summary>Gets a generic monospace <see cref="T:System.Drawing.FontFamily" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.FontFamily" /> that represents a generic monospace font.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static FontFamily GenericMonospace => new FontFamily(GenericFontFamilies.Monospace);

	/// <summary>Gets a generic sans serif <see cref="T:System.Drawing.FontFamily" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.FontFamily" /> object that represents a generic sans serif font.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static FontFamily GenericSansSerif => new FontFamily(GenericFontFamilies.SansSerif);

	/// <summary>Gets a generic serif <see cref="T:System.Drawing.FontFamily" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.FontFamily" /> that represents a generic serif font.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static FontFamily GenericSerif => new FontFamily(GenericFontFamilies.Serif);

	/// <summary>Returns an array that contains all the <see cref="T:System.Drawing.FontFamily" /> objects associated with the current graphics context.</summary>
	/// <returns>An array of <see cref="T:System.Drawing.FontFamily" /> objects associated with the current graphics context.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static FontFamily[] Families => new InstalledFontCollection().Families;

	internal FontFamily(IntPtr fntfamily)
	{
		nativeFontFamily = fntfamily;
	}

	internal unsafe void refreshName()
	{
		if (!(nativeFontFamily == IntPtr.Zero))
		{
			char* ptr = stackalloc char[32];
			GDIPlus.CheckStatus(GDIPlus.GdipGetFamilyName(nativeFontFamily, (IntPtr)ptr, 0));
			name = Marshal.PtrToStringUni((IntPtr)ptr);
		}
	}

	~FontFamily()
	{
		Dispose();
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.FontFamily" /> from the specified generic font family.</summary>
	/// <param name="genericFamily">The <see cref="T:System.Drawing.Text.GenericFontFamilies" /> from which to create the new <see cref="T:System.Drawing.FontFamily" />. </param>
	public FontFamily(GenericFontFamilies genericFamily)
	{
		GDIPlus.CheckStatus(genericFamily switch
		{
			GenericFontFamilies.SansSerif => GDIPlus.GdipGetGenericFontFamilySansSerif(out nativeFontFamily), 
			GenericFontFamilies.Serif => GDIPlus.GdipGetGenericFontFamilySerif(out nativeFontFamily), 
			_ => GDIPlus.GdipGetGenericFontFamilyMonospace(out nativeFontFamily), 
		});
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.FontFamily" /> with the specified name.</summary>
	/// <param name="name">The name of the new <see cref="T:System.Drawing.FontFamily" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is an empty string ("").-or-<paramref name="name" /> specifies a font that is not installed on the computer running the application.-or-<paramref name="name" /> specifies a font that is not a TrueType font.</exception>
	public FontFamily(string name)
		: this(name, null)
	{
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.FontFamily" /> in the specified <see cref="T:System.Drawing.Text.FontCollection" /> with the specified name.</summary>
	/// <param name="name">A <see cref="T:System.String" /> that represents the name of the new <see cref="T:System.Drawing.FontFamily" />. </param>
	/// <param name="fontCollection">The <see cref="T:System.Drawing.Text.FontCollection" /> that contains this <see cref="T:System.Drawing.FontFamily" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is an empty string ("").-or-<paramref name="name" /> specifies a font that is not installed on the computer running the application.-or-<paramref name="name" /> specifies a font that is not a TrueType font.</exception>
	public FontFamily(string name, FontCollection fontCollection)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateFontFamilyFromName(name, fontCollection?._nativeFontCollection ?? IntPtr.Zero, out nativeFontFamily));
	}

	/// <summary>Returns the cell ascent, in design units, of the <see cref="T:System.Drawing.FontFamily" /> of the specified style.</summary>
	/// <returns>The cell ascent for this <see cref="T:System.Drawing.FontFamily" /> that uses the specified <see cref="T:System.Drawing.FontStyle" />.</returns>
	/// <param name="style">A <see cref="T:System.Drawing.FontStyle" /> that contains style information for the font. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public int GetCellAscent(FontStyle style)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipGetCellAscent(nativeFontFamily, (int)style, out var ascent));
		return ascent;
	}

	/// <summary>Returns the cell descent, in design units, of the <see cref="T:System.Drawing.FontFamily" /> of the specified style. </summary>
	/// <returns>The cell descent metric for this <see cref="T:System.Drawing.FontFamily" /> that uses the specified <see cref="T:System.Drawing.FontStyle" />.</returns>
	/// <param name="style">A <see cref="T:System.Drawing.FontStyle" /> that contains style information for the font. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public int GetCellDescent(FontStyle style)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipGetCellDescent(nativeFontFamily, (int)style, out var descent));
		return descent;
	}

	/// <summary>Gets the height, in font design units, of the em square for the specified style.</summary>
	/// <returns>The height of the em square.</returns>
	/// <param name="style">The <see cref="T:System.Drawing.FontStyle" /> for which to get the em height. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public int GetEmHeight(FontStyle style)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipGetEmHeight(nativeFontFamily, (int)style, out var emHeight));
		return emHeight;
	}

	/// <summary>Returns the line spacing, in design units, of the <see cref="T:System.Drawing.FontFamily" /> of the specified style. The line spacing is the vertical distance between the base lines of two consecutive lines of text. </summary>
	/// <returns>The distance between two consecutive lines of text.</returns>
	/// <param name="style">The <see cref="T:System.Drawing.FontStyle" /> to apply. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public int GetLineSpacing(FontStyle style)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipGetLineSpacing(nativeFontFamily, (int)style, out var spacing));
		return spacing;
	}

	/// <summary>Indicates whether the specified <see cref="T:System.Drawing.FontStyle" /> enumeration is available.</summary>
	/// <returns>true if the specified <see cref="T:System.Drawing.FontStyle" /> is available; otherwise, false.</returns>
	/// <param name="style">The <see cref="T:System.Drawing.FontStyle" /> to test. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoDocumentationNote("When used with libgdiplus this method always return true (styles are created on demand).")]
	public bool IsStyleAvailable(FontStyle style)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipIsStyleAvailable(nativeFontFamily, (int)style, out var styleAvailable));
		return styleAvailable;
	}

	/// <summary>Releases all resources used by this <see cref="T:System.Drawing.FontFamily" />.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Dispose()
	{
		if (nativeFontFamily != IntPtr.Zero)
		{
			Status status = GDIPlus.GdipDeleteFontFamily(nativeFontFamily);
			nativeFontFamily = IntPtr.Zero;
			GC.SuppressFinalize(this);
			GDIPlus.CheckStatus(status);
		}
	}

	/// <summary>Indicates whether the specified object is a <see cref="T:System.Drawing.FontFamily" /> and is identical to this <see cref="T:System.Drawing.FontFamily" />.</summary>
	/// <returns>true if <paramref name="obj" /> is a <see cref="T:System.Drawing.FontFamily" /> and is identical to this <see cref="T:System.Drawing.FontFamily" />; otherwise, false.</returns>
	/// <param name="obj">The object to test. </param>
	/// <filterpriority>1</filterpriority>
	public override bool Equals(object obj)
	{
		if (!(obj is FontFamily fontFamily))
		{
			return false;
		}
		return Name == fontFamily.Name;
	}

	/// <summary>Gets a hash code for this <see cref="T:System.Drawing.FontFamily" />.</summary>
	/// <returns>The hash code for this <see cref="T:System.Drawing.FontFamily" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}

	/// <summary>Returns an array that contains all the <see cref="T:System.Drawing.FontFamily" /> objects available for the specified graphics context.</summary>
	/// <returns>An array of <see cref="T:System.Drawing.FontFamily" /> objects available for the specified <see cref="T:System.Drawing.Graphics" /> object.</returns>
	/// <param name="graphics">The <see cref="T:System.Drawing.Graphics" /> object from which to return <see cref="T:System.Drawing.FontFamily" /> objects. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="graphics " />isnull.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static FontFamily[] GetFamilies(Graphics graphics)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		return new InstalledFontCollection().Families;
	}

	/// <summary>Returns the name, in the specified language, of this <see cref="T:System.Drawing.FontFamily" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that represents the name, in the specified language, of this <see cref="T:System.Drawing.FontFamily" />. </returns>
	/// <param name="language">The language in which the name is returned. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoLimitation("The language parameter is ignored. We always return the name using the default system language.")]
	public string GetName(int language)
	{
		return Name;
	}

	/// <summary>Converts this <see cref="T:System.Drawing.FontFamily" /> to a human-readable string representation.</summary>
	/// <returns>The string that represents this <see cref="T:System.Drawing.FontFamily" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override string ToString()
	{
		return "[FontFamily: Name=" + Name + "]";
	}
}
