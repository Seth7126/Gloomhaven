namespace System.Drawing.Printing;

/// <summary>Specifies the printer's duplex setting.</summary>
public enum Duplex
{
	/// <summary>The printer's default duplex setting.</summary>
	Default = -1,
	/// <summary>Single-sided printing.</summary>
	Simplex = 1,
	/// <summary>Double-sided, horizontal printing.</summary>
	Horizontal = 3,
	/// <summary>Double-sided, vertical printing.</summary>
	Vertical = 2
}
