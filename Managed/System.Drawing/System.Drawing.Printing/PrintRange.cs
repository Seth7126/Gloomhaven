namespace System.Drawing.Printing;

/// <summary>Specifies the part of the document to print.</summary>
public enum PrintRange
{
	/// <summary>All pages are printed.</summary>
	AllPages = 0,
	/// <summary>The pages between <see cref="P:System.Drawing.Printing.PrinterSettings.FromPage" /> and <see cref="P:System.Drawing.Printing.PrinterSettings.ToPage" /> are printed.</summary>
	SomePages = 2,
	/// <summary>The selected pages are printed.</summary>
	Selection = 1,
	/// <summary>The currently displayed page is printed</summary>
	CurrentPage = 4194304
}
