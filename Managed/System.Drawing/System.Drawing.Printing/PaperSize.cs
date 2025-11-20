using System.Globalization;

namespace System.Drawing.Printing;

/// <summary>Specifies the size of a piece of paper.</summary>
[Serializable]
public class PaperSize
{
	private PaperKind _kind;

	private string _name;

	private int _width;

	private int _height;

	private bool _createdByDefaultConstructor;

	/// <summary>Gets or sets the height of the paper, in hundredths of an inch.</summary>
	/// <returns>The height of the paper, in hundredths of an inch.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Printing.PaperSize.Kind" /> property is not set to <see cref="F:System.Drawing.Printing.PaperKind.Custom" />. </exception>
	public int Height
	{
		get
		{
			return _height;
		}
		set
		{
			if (_kind != PaperKind.Custom && !_createdByDefaultConstructor)
			{
				throw new ArgumentException(global::SR.Format("PaperSize cannot be changed unless the Kind property is set to Custom."));
			}
			_height = value;
		}
	}

	/// <summary>Gets the type of paper.</summary>
	/// <returns>One of the <see cref="T:System.Drawing.Printing.PaperKind" /> values.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Printing.PaperSize.Kind" /> property is not set to <see cref="F:System.Drawing.Printing.PaperKind.Custom" />. </exception>
	public PaperKind Kind
	{
		get
		{
			if (_kind <= PaperKind.PrcEnvelopeNumber10Rotated && _kind != (PaperKind)48 && _kind != (PaperKind)49)
			{
				return _kind;
			}
			return PaperKind.Custom;
		}
	}

	/// <summary>Gets or sets the name of the type of paper.</summary>
	/// <returns>The name of the type of paper.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Printing.PaperSize.Kind" /> property is not set to <see cref="F:System.Drawing.Printing.PaperKind.Custom" />. </exception>
	public string PaperName
	{
		get
		{
			return _name;
		}
		set
		{
			if (_kind != PaperKind.Custom && !_createdByDefaultConstructor)
			{
				throw new ArgumentException(global::SR.Format("PaperSize cannot be changed unless the Kind property is set to Custom."));
			}
			_name = value;
		}
	}

	/// <summary>Gets or sets an integer representing one of the <see cref="T:System.Drawing.Printing.PaperSize" /> values or a custom value.</summary>
	/// <returns>An integer representing one of the <see cref="T:System.Drawing.Printing.PaperSize" /> values, or a custom value.</returns>
	public int RawKind
	{
		get
		{
			return (int)_kind;
		}
		set
		{
			_kind = (PaperKind)value;
		}
	}

	/// <summary>Gets or sets the width of the paper, in hundredths of an inch.</summary>
	/// <returns>The width of the paper, in hundredths of an inch.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Printing.PaperSize.Kind" /> property is not set to <see cref="F:System.Drawing.Printing.PaperKind.Custom" />. </exception>
	public int Width
	{
		get
		{
			return _width;
		}
		set
		{
			if (_kind != PaperKind.Custom && !_createdByDefaultConstructor)
			{
				throw new ArgumentException(global::SR.Format("PaperSize cannot be changed unless the Kind property is set to Custom."));
			}
			_width = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PaperSize" /> class.</summary>
	public PaperSize()
	{
		_kind = PaperKind.Custom;
		_name = string.Empty;
		_createdByDefaultConstructor = true;
	}

	internal PaperSize(PaperKind kind, string name, int width, int height)
	{
		_kind = kind;
		_name = name;
		_width = width;
		_height = height;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PaperSize" /> class.</summary>
	/// <param name="name">The name of the paper. </param>
	/// <param name="width">The width of the paper, in hundredths of an inch. </param>
	/// <param name="height">The height of the paper, in hundredths of an inch. </param>
	public PaperSize(string name, int width, int height)
	{
		_kind = PaperKind.Custom;
		_name = name;
		_width = width;
		_height = height;
	}

	/// <summary>Provides information about the <see cref="T:System.Drawing.Printing.PaperSize" /> in string form.</summary>
	/// <returns>A string.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override string ToString()
	{
		return "[PaperSize " + PaperName + " Kind=" + Kind.ToString() + " Height=" + Height.ToString(CultureInfo.InvariantCulture) + " Width=" + Width.ToString(CultureInfo.InvariantCulture) + "]";
	}
}
