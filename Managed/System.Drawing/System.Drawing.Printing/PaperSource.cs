namespace System.Drawing.Printing;

/// <summary>Specifies the paper tray from which the printer gets paper.</summary>
[Serializable]
public class PaperSource
{
	private string _name;

	private PaperSourceKind _kind;

	/// <summary>Gets the paper source.</summary>
	/// <returns>One of the <see cref="T:System.Drawing.Printing.PaperSourceKind" /> values.</returns>
	public PaperSourceKind Kind
	{
		get
		{
			if (_kind >= (PaperSourceKind)256)
			{
				return PaperSourceKind.Custom;
			}
			return _kind;
		}
	}

	/// <summary>Gets or sets the integer representing one of the <see cref="T:System.Drawing.Printing.PaperSourceKind" /> values or a custom value.</summary>
	/// <returns>The integer value representing one of the <see cref="T:System.Drawing.Printing.PaperSourceKind" /> values or a custom value. </returns>
	public int RawKind
	{
		get
		{
			return (int)_kind;
		}
		set
		{
			_kind = (PaperSourceKind)value;
		}
	}

	/// <summary>Gets or sets the name of the paper source.</summary>
	/// <returns>The name of the paper source.</returns>
	public string SourceName
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PaperSource" /> class. </summary>
	public PaperSource()
	{
		_kind = PaperSourceKind.Custom;
		_name = string.Empty;
	}

	internal PaperSource(PaperSourceKind kind, string name)
	{
		_kind = kind;
		_name = name;
	}

	/// <summary>Provides information about the <see cref="T:System.Drawing.Printing.PaperSource" /> in string form.</summary>
	/// <returns>A string.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override string ToString()
	{
		return "[PaperSource " + SourceName + " Kind=" + Kind.ToString() + "]";
	}
}
