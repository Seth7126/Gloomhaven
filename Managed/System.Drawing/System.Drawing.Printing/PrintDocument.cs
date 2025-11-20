using System.ComponentModel;

namespace System.Drawing.Printing;

/// <summary>Defines a reusable object that sends output to a printer, when printing from a Windows Forms application.</summary>
[DefaultEvent("PrintPage")]
[DefaultProperty("DocumentName")]
[ToolboxItemFilter("System.Drawing.Printing", ToolboxItemFilterType.Allow)]
public class PrintDocument : Component
{
	private PageSettings defaultpagesettings;

	private PrinterSettings printersettings;

	private PrintController printcontroller;

	private string documentname;

	private bool originAtMargins;

	/// <summary>Gets or sets page settings that are used as defaults for all pages to be printed.</summary>
	/// <returns>A <see cref="T:System.Drawing.Printing.PageSettings" /> that specifies the default page settings for the document.</returns>
	[SRDescription("The settings for the current page.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public PageSettings DefaultPageSettings
	{
		get
		{
			return defaultpagesettings;
		}
		set
		{
			defaultpagesettings = value;
		}
	}

	/// <summary>Gets or sets the document name to display (for example, in a print status dialog box or printer queue) while printing the document.</summary>
	/// <returns>The document name to display while printing the document. The default is "document".</returns>
	[DefaultValue("document")]
	[SRDescription("The name of the document.")]
	public string DocumentName
	{
		get
		{
			return documentname;
		}
		set
		{
			documentname = value;
		}
	}

	/// <summary>Gets or sets the print controller that guides the printing process.</summary>
	/// <returns>The <see cref="T:System.Drawing.Printing.PrintController" /> that guides the printing process. The default is a new instance of the <see cref="T:System.Windows.Forms.PrintControllerWithStatusDialog" /> class.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[Browsable(false)]
	[SRDescription("The print controller object.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public PrintController PrintController
	{
		get
		{
			return printcontroller;
		}
		set
		{
			printcontroller = value;
		}
	}

	/// <summary>Gets or sets the printer that prints the document.</summary>
	/// <returns>A <see cref="T:System.Drawing.Printing.PrinterSettings" /> that specifies where and how the document is printed. The default is a <see cref="T:System.Drawing.Printing.PrinterSettings" /> with its properties set to their default values.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[SRDescription("The current settings for the active printer.")]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public PrinterSettings PrinterSettings
	{
		get
		{
			return printersettings;
		}
		set
		{
			printersettings = ((value == null) ? new PrinterSettings() : value);
		}
	}

	/// <summary>Gets or sets a value indicating whether the position of a graphics object associated with a page is located just inside the user-specified margins or at the top-left corner of the printable area of the page.</summary>
	/// <returns>true if the graphics origin starts at the page margins; false if the graphics origin is at the top-left corner of the printable page. The default is false.</returns>
	[SRDescription("Determines if the origin is set at the specified margins.")]
	[DefaultValue(false)]
	public bool OriginAtMargins
	{
		get
		{
			return originAtMargins;
		}
		set
		{
			originAtMargins = value;
		}
	}

	/// <summary>Occurs when the <see cref="M:System.Drawing.Printing.PrintDocument.Print" /> method is called and before the first page of the document prints.</summary>
	[SRDescription("Raised when printing begins")]
	public event PrintEventHandler BeginPrint;

	/// <summary>Occurs when the last page of the document has printed.</summary>
	[SRDescription("Raised when printing ends")]
	public event PrintEventHandler EndPrint;

	/// <summary>Occurs when the output to print for the current page is needed.</summary>
	[SRDescription("Raised when printing of a new page begins")]
	public event PrintPageEventHandler PrintPage;

	/// <summary>Occurs immediately before each <see cref="E:System.Drawing.Printing.PrintDocument.PrintPage" /> event.</summary>
	[SRDescription("Raised before printing of a new page begins")]
	public event QueryPageSettingsEventHandler QueryPageSettings;

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PrintDocument" /> class.</summary>
	public PrintDocument()
	{
		documentname = "document";
		printersettings = new PrinterSettings();
		defaultpagesettings = (PageSettings)printersettings.DefaultPageSettings.Clone();
		printcontroller = new StandardPrintController();
	}

	/// <summary>Starts the document's printing process.</summary>
	/// <exception cref="T:System.Drawing.Printing.InvalidPrinterException">The printer named in the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property does not exist. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Print()
	{
		PrintEventArgs e = new PrintEventArgs();
		OnBeginPrint(e);
		if (e.Cancel)
		{
			return;
		}
		PrintController.OnStartPrint(this, e);
		if (e.Cancel)
		{
			return;
		}
		Graphics graphics = null;
		if (e.GraphicsContext != null)
		{
			graphics = Graphics.FromHdc(e.GraphicsContext.Hdc);
			e.GraphicsContext.Graphics = graphics;
		}
		PrintPageEventArgs e3;
		do
		{
			QueryPageSettingsEventArgs e2 = new QueryPageSettingsEventArgs(DefaultPageSettings.Clone() as PageSettings);
			OnQueryPageSettings(e2);
			PageSettings pageSettings = e2.PageSettings;
			e3 = new PrintPageEventArgs(graphics, pageSettings.Bounds, new Rectangle(0, 0, pageSettings.PaperSize.Width, pageSettings.PaperSize.Height), pageSettings);
			e3.GraphicsContext = e.GraphicsContext;
			Graphics graphics2 = PrintController.OnStartPage(this, e3);
			e3.SetGraphics(graphics2);
			if (!e3.Cancel)
			{
				OnPrintPage(e3);
			}
			PrintController.OnEndPage(this, e3);
		}
		while (!e3.Cancel && e3.HasMorePages);
		OnEndPrint(e);
		PrintController.OnEndPrint(this, e);
	}

	/// <summary>Provides information about the print document, in string form.</summary>
	/// <returns>A string.</returns>
	public override string ToString()
	{
		return "[PrintDocument " + DocumentName + "]";
	}

	/// <summary>Raises the <see cref="E:System.Drawing.Printing.PrintDocument.BeginPrint" /> event. It is called after the <see cref="M:System.Drawing.Printing.PrintDocument.Print" /> method is called and before the first page of the document prints.</summary>
	/// <param name="e">A <see cref="T:System.Drawing.Printing.PrintEventArgs" /> that contains the event data. </param>
	protected virtual void OnBeginPrint(PrintEventArgs e)
	{
		if (this.BeginPrint != null)
		{
			this.BeginPrint(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Drawing.Printing.PrintDocument.EndPrint" /> event. It is called when the last page of the document has printed.</summary>
	/// <param name="e">A <see cref="T:System.Drawing.Printing.PrintEventArgs" /> that contains the event data. </param>
	protected virtual void OnEndPrint(PrintEventArgs e)
	{
		if (this.EndPrint != null)
		{
			this.EndPrint(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Drawing.Printing.PrintDocument.PrintPage" /> event. It is called before a page prints.</summary>
	/// <param name="e">A <see cref="T:System.Drawing.Printing.PrintPageEventArgs" /> that contains the event data. </param>
	protected virtual void OnPrintPage(PrintPageEventArgs e)
	{
		if (this.PrintPage != null)
		{
			this.PrintPage(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Drawing.Printing.PrintDocument.QueryPageSettings" /> event. It is called immediately before each <see cref="E:System.Drawing.Printing.PrintDocument.PrintPage" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Drawing.Printing.QueryPageSettingsEventArgs" /> that contains the event data. </param>
	protected virtual void OnQueryPageSettings(QueryPageSettingsEventArgs e)
	{
		if (this.QueryPageSettings != null)
		{
			this.QueryPageSettings(this, e);
		}
	}
}
