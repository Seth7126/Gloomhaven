using System.ComponentModel;
using System.Drawing.Text;

namespace System.Drawing;

/// <summary>Encapsulates text layout information (such as alignment, orientation and tab stops) display manipulations (such as ellipsis insertion and national digit substitution) and OpenType features. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
public sealed class StringFormat : MarshalByRefObject, IDisposable, ICloneable
{
	private IntPtr nativeStrFmt = IntPtr.Zero;

	private int language;

	/// <summary>Gets or sets horizontal alignment of the string..</summary>
	/// <returns>A <see cref="T:System.Drawing.StringAlignment" /> enumeration that specifies the horizontal  alignment of the string.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public StringAlignment Alignment
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetStringFormatAlign(nativeStrFmt, out var align));
			return align;
		}
		set
		{
			if (value < StringAlignment.Near || value > StringAlignment.Far)
			{
				throw new InvalidEnumArgumentException("Alignment");
			}
			GDIPlus.CheckStatus(GDIPlus.GdipSetStringFormatAlign(nativeStrFmt, value));
		}
	}

	/// <summary>Gets or sets the vertical alignment of the string.</summary>
	/// <returns>A <see cref="T:System.Drawing.StringAlignment" /> enumeration that represents the vertical line alignment.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public StringAlignment LineAlignment
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetStringFormatLineAlign(nativeStrFmt, out var align));
			return align;
		}
		set
		{
			if (value < StringAlignment.Near || value > StringAlignment.Far)
			{
				throw new InvalidEnumArgumentException("Alignment");
			}
			GDIPlus.CheckStatus(GDIPlus.GdipSetStringFormatLineAlign(nativeStrFmt, value));
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Drawing.StringFormatFlags" /> enumeration that contains formatting information.</summary>
	/// <returns>A <see cref="T:System.Drawing.StringFormatFlags" /> enumeration that contains formatting information.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public StringFormatFlags FormatFlags
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetStringFormatFlags(nativeStrFmt, out var flags));
			return flags;
		}
		set
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetStringFormatFlags(nativeStrFmt, value));
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Drawing.Text.HotkeyPrefix" /> object for this <see cref="T:System.Drawing.StringFormat" /> object.</summary>
	/// <returns>The <see cref="T:System.Drawing.Text.HotkeyPrefix" /> object for this <see cref="T:System.Drawing.StringFormat" /> object, the default is <see cref="F:System.Drawing.Text.HotkeyPrefix.None" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public HotkeyPrefix HotkeyPrefix
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetStringFormatHotkeyPrefix(nativeStrFmt, out var hotkeyPrefix));
			return hotkeyPrefix;
		}
		set
		{
			if (value < HotkeyPrefix.None || value > HotkeyPrefix.Hide)
			{
				throw new InvalidEnumArgumentException("HotkeyPrefix");
			}
			GDIPlus.CheckStatus(GDIPlus.GdipSetStringFormatHotkeyPrefix(nativeStrFmt, value));
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Drawing.StringTrimming" /> enumeration for this <see cref="T:System.Drawing.StringFormat" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.StringTrimming" /> enumeration that indicates how text drawn with this <see cref="T:System.Drawing.StringFormat" /> object is trimmed when it exceeds the edges of the layout rectangle.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public StringTrimming Trimming
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetStringFormatTrimming(nativeStrFmt, out var trimming));
			return trimming;
		}
		set
		{
			if (value < StringTrimming.None || value > StringTrimming.EllipsisPath)
			{
				throw new InvalidEnumArgumentException("Trimming");
			}
			GDIPlus.CheckStatus(GDIPlus.GdipSetStringFormatTrimming(nativeStrFmt, value));
		}
	}

	/// <summary>Gets a generic default <see cref="T:System.Drawing.StringFormat" /> object.</summary>
	/// <returns>The generic default <see cref="T:System.Drawing.StringFormat" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static StringFormat GenericDefault
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipStringFormatGetGenericDefault(out var format));
			return new StringFormat(format);
		}
	}

	/// <summary>Gets the language that is used when local digits are substituted for western digits.</summary>
	/// <returns>A National Language Support (NLS) language identifier that identifies the language that will be used when local digits are substituted for western digits. You can pass the <see cref="P:System.Globalization.CultureInfo.LCID" /> property of a <see cref="T:System.Globalization.CultureInfo" /> object as the NLS language identifier. For example, suppose you create a <see cref="T:System.Globalization.CultureInfo" /> object by passing the string "ar-EG" to a <see cref="T:System.Globalization.CultureInfo" /> constructor. If you pass the <see cref="P:System.Globalization.CultureInfo.LCID" /> property of that <see cref="T:System.Globalization.CultureInfo" /> object along with.<see cref="F:System.Drawing.StringDigitSubstitute.Traditional" /> to the <see cref="M:System.Drawing.StringFormat.SetDigitSubstitution(System.Int32,System.Drawing.StringDigitSubstitute)" /> method, then Arabic-Indic digits will be substituted for western digits at display time.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public int DigitSubstitutionLanguage => language;

	/// <summary>Gets a generic typographic <see cref="T:System.Drawing.StringFormat" /> object.</summary>
	/// <returns>A generic typographic <see cref="T:System.Drawing.StringFormat" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static StringFormat GenericTypographic
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipStringFormatGetGenericTypographic(out var format));
			return new StringFormat(format);
		}
	}

	/// <summary>Gets the method to be used for digit substitution.</summary>
	/// <returns>A <see cref="T:System.Drawing.StringDigitSubstitute" /> enumeration value that specifies how to substitute characters in a string that cannot be displayed because they are not supported by the current font.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public StringDigitSubstitute DigitSubstitutionMethod
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetStringFormatDigitSubstitution(nativeStrFmt, language, out var substitute));
			return substitute;
		}
	}

	internal IntPtr NativeObject
	{
		get
		{
			return nativeStrFmt;
		}
		set
		{
			nativeStrFmt = value;
		}
	}

	internal IntPtr nativeFormat => nativeStrFmt;

	/// <summary>Initializes a new <see cref="T:System.Drawing.StringFormat" /> object.</summary>
	public StringFormat()
		: this((StringFormatFlags)0, 0)
	{
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.StringFormat" /> object with the specified <see cref="T:System.Drawing.StringFormatFlags" /> enumeration and language.</summary>
	/// <param name="options">The <see cref="T:System.Drawing.StringFormatFlags" /> enumeration for the new <see cref="T:System.Drawing.StringFormat" /> object. </param>
	/// <param name="language">A value that indicates the language of the text. </param>
	public StringFormat(StringFormatFlags options, int language)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateStringFormat(options, language, out nativeStrFmt));
	}

	internal StringFormat(IntPtr native)
	{
		nativeStrFmt = native;
	}

	~StringFormat()
	{
		Dispose(disposing: false);
	}

	/// <summary>Releases all resources used by this <see cref="T:System.Drawing.StringFormat" /> object.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (nativeStrFmt != IntPtr.Zero)
		{
			Status status = GDIPlus.GdipDeleteStringFormat(nativeStrFmt);
			nativeStrFmt = IntPtr.Zero;
			GDIPlus.CheckStatus(status);
		}
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.StringFormat" /> object from the specified existing <see cref="T:System.Drawing.StringFormat" /> object.</summary>
	/// <param name="format">The <see cref="T:System.Drawing.StringFormat" /> object from which to initialize the new <see cref="T:System.Drawing.StringFormat" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null.</exception>
	public StringFormat(StringFormat format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCloneStringFormat(format.NativeObject, out nativeStrFmt));
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.StringFormat" /> object with the specified <see cref="T:System.Drawing.StringFormatFlags" /> enumeration.</summary>
	/// <param name="options">The <see cref="T:System.Drawing.StringFormatFlags" /> enumeration for the new <see cref="T:System.Drawing.StringFormat" /> object. </param>
	public StringFormat(StringFormatFlags options)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateStringFormat(options, 0, out nativeStrFmt));
	}

	/// <summary>Specifies an array of <see cref="T:System.Drawing.CharacterRange" /> structures that represent the ranges of characters measured by a call to the <see cref="M:System.Drawing.Graphics.MeasureCharacterRanges(System.String,System.Drawing.Font,System.Drawing.RectangleF,System.Drawing.StringFormat)" /> method.</summary>
	/// <param name="ranges">An array of <see cref="T:System.Drawing.CharacterRange" /> structures that specifies the ranges of characters measured by a call to the <see cref="M:System.Drawing.Graphics.MeasureCharacterRanges(System.String,System.Drawing.Font,System.Drawing.RectangleF,System.Drawing.StringFormat)" /> method. </param>
	/// <exception cref="T:System.OverflowException">More than 32 character ranges are set.</exception>
	/// <filterpriority>1</filterpriority>
	public void SetMeasurableCharacterRanges(CharacterRange[] ranges)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipSetStringFormatMeasurableCharacterRanges(nativeStrFmt, ranges.Length, ranges));
	}

	internal int GetMeasurableCharacterRangeCount()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipGetStringFormatMeasurableCharacterRangeCount(nativeStrFmt, out var cnt));
		return cnt;
	}

	/// <summary>Creates an exact copy of this <see cref="T:System.Drawing.StringFormat" /> object.</summary>
	/// <returns>The <see cref="T:System.Drawing.StringFormat" /> object this method creates.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public object Clone()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCloneStringFormat(nativeStrFmt, out var format));
		return new StringFormat(format);
	}

	/// <summary>Converts this <see cref="T:System.Drawing.StringFormat" /> object to a human-readable string.</summary>
	/// <returns>A string representation of this <see cref="T:System.Drawing.StringFormat" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override string ToString()
	{
		return "[StringFormat, FormatFlags=" + FormatFlags.ToString() + "]";
	}

	/// <summary>Sets tab stops for this <see cref="T:System.Drawing.StringFormat" /> object.</summary>
	/// <param name="firstTabOffset">The number of spaces between the beginning of a line of text and the first tab stop. </param>
	/// <param name="tabStops">An array of distances between tab stops in the units specified by the <see cref="P:System.Drawing.Graphics.PageUnit" /> property. </param>
	/// <filterpriority>1</filterpriority>
	public void SetTabStops(float firstTabOffset, float[] tabStops)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipSetStringFormatTabStops(nativeStrFmt, firstTabOffset, tabStops.Length, tabStops));
	}

	/// <summary>Specifies the language and method to be used when local digits are substituted for western digits.</summary>
	/// <param name="language">A National Language Support (NLS) language identifier that identifies the language that will be used when local digits are substituted for western digits. You can pass the <see cref="P:System.Globalization.CultureInfo.LCID" /> property of a <see cref="T:System.Globalization.CultureInfo" /> object as the NLS language identifier. For example, suppose you create a <see cref="T:System.Globalization.CultureInfo" /> object by passing the string "ar-EG" to a <see cref="T:System.Globalization.CultureInfo" /> constructor. If you pass the <see cref="P:System.Globalization.CultureInfo.LCID" /> property of that <see cref="T:System.Globalization.CultureInfo" /> object along with <see cref="F:System.Drawing.StringDigitSubstitute.Traditional" /> to the <see cref="M:System.Drawing.StringFormat.SetDigitSubstitution(System.Int32,System.Drawing.StringDigitSubstitute)" /> method, then Arabic-Indic digits will be substituted for western digits at display time. </param>
	/// <param name="substitute">An element of the <see cref="T:System.Drawing.StringDigitSubstitute" /> enumeration that specifies how digits are displayed. </param>
	/// <filterpriority>1</filterpriority>
	public void SetDigitSubstitution(int language, StringDigitSubstitute substitute)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipSetStringFormatDigitSubstitution(nativeStrFmt, this.language, substitute));
	}

	/// <summary>Gets the tab stops for this <see cref="T:System.Drawing.StringFormat" /> object.</summary>
	/// <returns>An array of distances (in number of spaces) between tab stops.</returns>
	/// <param name="firstTabOffset">The number of spaces between the beginning of a text line and the first tab stop. </param>
	/// <filterpriority>1</filterpriority>
	public float[] GetTabStops(out float firstTabOffset)
	{
		int count = 0;
		firstTabOffset = 0f;
		GDIPlus.CheckStatus(GDIPlus.GdipGetStringFormatTabStopCount(nativeStrFmt, out count));
		float[] array = new float[count];
		if (count != 0)
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetStringFormatTabStops(nativeStrFmt, count, out firstTabOffset, array));
		}
		return array;
	}
}
