using System.Collections;

namespace System.Drawing.Printing;

/// <summary>Specifies a print controller that displays a document on a screen as a series of images.</summary>
public class PreviewPrintController : PrintController
{
	private bool useantialias;

	private ArrayList pageInfoList;

	/// <summary>Gets a value indicating whether this controller is used for print preview. </summary>
	/// <returns>true in all cases.</returns>
	public override bool IsPreview => true;

	/// <summary>Gets or sets a value indicating whether to use anti-aliasing when displaying the print preview.</summary>
	/// <returns>true if the print preview uses anti-aliasing; otherwise, false. The default is false.</returns>
	public virtual bool UseAntiAlias
	{
		get
		{
			return useantialias;
		}
		set
		{
			useantialias = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PreviewPrintController" /> class.</summary>
	public PreviewPrintController()
	{
		pageInfoList = new ArrayList();
	}

	/// <summary>Completes the control sequence that determines when and how to preview a page in a print document.</summary>
	/// <param name="document">A <see cref="T:System.Drawing.Printing.PrintDocument" /> that represents the document being previewed. </param>
	/// <param name="e">A <see cref="T:System.Drawing.Printing.PrintPageEventArgs" /> that contains data about how to preview a page in the print document. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO]
	public override void OnEndPage(PrintDocument document, PrintPageEventArgs e)
	{
	}

	/// <summary>Begins the control sequence that determines when and how to preview a print document.</summary>
	/// <param name="document">A <see cref="T:System.Drawing.Printing.PrintDocument" /> that represents the document being previewed. </param>
	/// <param name="e">A <see cref="T:System.Drawing.Printing.PrintEventArgs" /> that contains data about how to print the document. </param>
	/// <exception cref="T:System.Drawing.Printing.InvalidPrinterException">The printer named in the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property does not exist. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO]
	public override void OnStartPrint(PrintDocument document, PrintEventArgs e)
	{
		if (!document.PrinterSettings.IsValid)
		{
			throw new InvalidPrinterException(document.PrinterSettings);
		}
		foreach (PreviewPageInfo pageInfo in pageInfoList)
		{
			pageInfo.Image.Dispose();
		}
		pageInfoList.Clear();
	}

	/// <summary>Completes the control sequence that determines when and how to preview a print document.</summary>
	/// <param name="document">A <see cref="T:System.Drawing.Printing.PrintDocument" /> that represents the document being previewed. </param>
	/// <param name="e">A <see cref="T:System.Drawing.Printing.PrintEventArgs" /> that contains data about how to preview the print document. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO]
	public override void OnEndPrint(PrintDocument document, PrintEventArgs e)
	{
	}

	/// <summary>Begins the control sequence that determines when and how to preview a page in a print document.</summary>
	/// <returns>A <see cref="T:System.Drawing.Graphics" /> that represents a page from a <see cref="T:System.Drawing.Printing.PrintDocument" />.</returns>
	/// <param name="document">A <see cref="T:System.Drawing.Printing.PrintDocument" /> that represents the document being previewed. </param>
	/// <param name="e">A <see cref="T:System.Drawing.Printing.PrintPageEventArgs" /> that contains data about how to preview a page in the print document. Initially, the <see cref="P:System.Drawing.Printing.PrintPageEventArgs.Graphics" /> property of this parameter will be null. The value returned from this method will be used to set this property. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO]
	public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
	{
		Image image = new Bitmap(e.PageSettings.PaperSize.Width, e.PageSettings.PaperSize.Height);
		PreviewPageInfo previewPageInfo = new PreviewPageInfo(image, new Size(e.PageSettings.PaperSize.Width, e.PageSettings.PaperSize.Height));
		pageInfoList.Add(previewPageInfo);
		Graphics graphics = Graphics.FromImage(previewPageInfo.Image);
		graphics.FillRectangle(new SolidBrush(Color.White), new Rectangle(new Point(0, 0), new Size(image.Width, image.Height)));
		return graphics;
	}

	/// <summary>Captures the pages of a document as a series of images.</summary>
	/// <returns>An array of type <see cref="T:System.Drawing.Printing.PreviewPageInfo" /> that contains the pages of a <see cref="T:System.Drawing.Printing.PrintDocument" /> as a series of images.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public PreviewPageInfo[] GetPreviewPageInfo()
	{
		PreviewPageInfo[] array = new PreviewPageInfo[pageInfoList.Count];
		pageInfoList.CopyTo(array);
		return array;
	}
}
