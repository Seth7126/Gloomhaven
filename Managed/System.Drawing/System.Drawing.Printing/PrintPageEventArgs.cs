namespace System.Drawing.Printing;

/// <summary>Provides data for the <see cref="E:System.Drawing.Printing.PrintDocument.PrintPage" /> event.</summary>
public class PrintPageEventArgs : EventArgs
{
	private bool cancel;

	private Graphics graphics;

	private bool hasmorePages;

	private Rectangle marginBounds;

	private Rectangle pageBounds;

	private PageSettings pageSettings;

	private GraphicsPrinter graphics_context;

	/// <summary>Gets or sets a value indicating whether the print job should be canceled.</summary>
	/// <returns>true if the print job should be canceled; otherwise, false.</returns>
	public bool Cancel
	{
		get
		{
			return cancel;
		}
		set
		{
			cancel = value;
		}
	}

	/// <summary>Gets the <see cref="T:System.Drawing.Graphics" /> used to paint the page.</summary>
	/// <returns>The <see cref="T:System.Drawing.Graphics" /> used to paint the page.</returns>
	public Graphics Graphics => graphics;

	/// <summary>Gets or sets a value indicating whether an additional page should be printed.</summary>
	/// <returns>true if an additional page should be printed; otherwise, false. The default is false.</returns>
	public bool HasMorePages
	{
		get
		{
			return hasmorePages;
		}
		set
		{
			hasmorePages = value;
		}
	}

	/// <summary>Gets the rectangular area that represents the portion of the page inside the margins.</summary>
	/// <returns>The rectangular area, measured in hundredths of an inch, that represents the portion of the page inside the margins. </returns>
	public Rectangle MarginBounds => marginBounds;

	/// <summary>Gets the rectangular area that represents the total area of the page.</summary>
	/// <returns>The rectangular area that represents the total area of the page.</returns>
	public Rectangle PageBounds => pageBounds;

	/// <summary>Gets the page settings for the current page.</summary>
	/// <returns>The page settings for the current page.</returns>
	public PageSettings PageSettings => pageSettings;

	internal GraphicsPrinter GraphicsContext
	{
		get
		{
			return graphics_context;
		}
		set
		{
			graphics_context = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PrintPageEventArgs" /> class.</summary>
	/// <param name="graphics">The <see cref="T:System.Drawing.Graphics" /> used to paint the item. </param>
	/// <param name="marginBounds">The area between the margins. </param>
	/// <param name="pageBounds">The total area of the paper. </param>
	/// <param name="pageSettings">The <see cref="T:System.Drawing.Printing.PageSettings" /> for the page. </param>
	public PrintPageEventArgs(Graphics graphics, Rectangle marginBounds, Rectangle pageBounds, PageSettings pageSettings)
	{
		this.graphics = graphics;
		this.marginBounds = marginBounds;
		this.pageBounds = pageBounds;
		this.pageSettings = pageSettings;
	}

	internal void SetGraphics(Graphics g)
	{
		graphics = g;
	}
}
