namespace System.Drawing.Printing;

/// <summary>Specifies the type of printing that code is allowed to do.</summary>
public enum PrintingPermissionLevel
{
	/// <summary>Provides full access to all printers.</summary>
	AllPrinting = 3,
	/// <summary>Provides printing programmatically to the default printer, along with safe printing through semirestricted dialog box. <see cref="F:System.Drawing.Printing.PrintingPermissionLevel.DefaultPrinting" /> is a subset of <see cref="F:System.Drawing.Printing.PrintingPermissionLevel.AllPrinting" />.</summary>
	DefaultPrinting = 2,
	/// <summary>Prevents access to printers. <see cref="F:System.Drawing.Printing.PrintingPermissionLevel.NoPrinting" /> is a subset of <see cref="F:System.Drawing.Printing.PrintingPermissionLevel.SafePrinting" />.</summary>
	NoPrinting = 0,
	/// <summary>Provides printing only from a restricted dialog box. <see cref="F:System.Drawing.Printing.PrintingPermissionLevel.SafePrinting" /> is a subset of <see cref="F:System.Drawing.Printing.PrintingPermissionLevel.DefaultPrinting" />.</summary>
	SafePrinting = 1
}
