namespace System.Drawing.Printing;

/// <summary>Controls how a document is printed, when printing from a Windows Forms application.</summary>
public abstract class PrintController
{
	/// <summary>Gets a value indicating whether the <see cref="T:System.Drawing.Printing.PrintController" /> is used for print preview.</summary>
	/// <returns>false in all cases.</returns>
	public virtual bool IsPreview => false;

	/// <summary>When overridden in a derived class, completes the control sequence that determines when and how to print a page of a document.</summary>
	/// <param name="document">A <see cref="T:System.Drawing.Printing.PrintDocument" /> that represents the document currently being printed. </param>
	/// <param name="e">A <see cref="T:System.Drawing.Printing.PrintPageEventArgs" /> that contains the event data. </param>
	public virtual void OnEndPage(PrintDocument document, PrintPageEventArgs e)
	{
	}

	/// <summary>When overridden in a derived class, begins the control sequence that determines when and how to print a document.</summary>
	/// <param name="document">A <see cref="T:System.Drawing.Printing.PrintDocument" /> that represents the document currently being printed. </param>
	/// <param name="e">A <see cref="T:System.Drawing.Printing.PrintEventArgs" /> that contains the event data. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public virtual void OnStartPrint(PrintDocument document, PrintEventArgs e)
	{
	}

	/// <summary>When overridden in a derived class, completes the control sequence that determines when and how to print a document.</summary>
	/// <param name="document">A <see cref="T:System.Drawing.Printing.PrintDocument" /> that represents the document currently being printed. </param>
	/// <param name="e">A <see cref="T:System.Drawing.Printing.PrintEventArgs" /> that contains the event data. </param>
	public virtual void OnEndPrint(PrintDocument document, PrintEventArgs e)
	{
	}

	/// <summary>When overridden in a derived class, begins the control sequence that determines when and how to print a page of a document.</summary>
	/// <returns>A <see cref="T:System.Drawing.Graphics" /> that represents a page from a <see cref="T:System.Drawing.Printing.PrintDocument" />.</returns>
	/// <param name="document">A <see cref="T:System.Drawing.Printing.PrintDocument" /> that represents the document currently being printed. </param>
	/// <param name="e">A <see cref="T:System.Drawing.Printing.PrintPageEventArgs" /> that contains the event data. </param>
	public virtual Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
	{
		return null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PrintController" /> class.</summary>
	protected PrintController()
	{
	}
}
