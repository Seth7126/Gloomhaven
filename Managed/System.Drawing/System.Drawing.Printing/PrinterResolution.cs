using System.ComponentModel;
using System.Globalization;

namespace System.Drawing.Printing;

/// <summary>Represents the resolution supported by a printer.</summary>
[Serializable]
public class PrinterResolution
{
	private int _x;

	private int _y;

	private PrinterResolutionKind _kind;

	/// <summary>Gets or sets the printer resolution.</summary>
	/// <returns>One of the <see cref="T:System.Drawing.Printing.PrinterResolutionKind" /> values.</returns>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The value assigned is not a member of the <see cref="T:System.Drawing.Printing.PrinterResolutionKind" /> enumeration.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public PrinterResolutionKind Kind
	{
		get
		{
			return _kind;
		}
		set
		{
			if (value < PrinterResolutionKind.High || value > PrinterResolutionKind.Custom)
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(PrinterResolutionKind));
			}
			_kind = value;
		}
	}

	/// <summary>Gets the horizontal printer resolution, in dots per inch.</summary>
	/// <returns>The horizontal printer resolution, in dots per inch, if <see cref="P:System.Drawing.Printing.PrinterResolution.Kind" /> is set to <see cref="F:System.Drawing.Printing.PrinterResolutionKind.Custom" />; otherwise, a dmPrintQuality value.</returns>
	public int X
	{
		get
		{
			return _x;
		}
		set
		{
			_x = value;
		}
	}

	/// <summary>Gets the vertical printer resolution, in dots per inch.</summary>
	/// <returns>The vertical printer resolution, in dots per inch.</returns>
	public int Y
	{
		get
		{
			return _y;
		}
		set
		{
			_y = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PrinterResolution" /> class. </summary>
	public PrinterResolution()
	{
		_kind = PrinterResolutionKind.Custom;
	}

	internal PrinterResolution(PrinterResolutionKind kind, int x, int y)
	{
		_kind = kind;
		_x = x;
		_y = y;
	}

	/// <summary>This member overrides the <see cref="M:System.Object.ToString" /> method.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains information about the <see cref="T:System.Drawing.Printing.PrinterResolution" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override string ToString()
	{
		if (_kind != PrinterResolutionKind.Custom)
		{
			return "[PrinterResolution " + Kind.ToString() + "]";
		}
		return "[PrinterResolution X=" + X.ToString(CultureInfo.InvariantCulture) + " Y=" + Y.ToString(CultureInfo.InvariantCulture) + "]";
	}
}
