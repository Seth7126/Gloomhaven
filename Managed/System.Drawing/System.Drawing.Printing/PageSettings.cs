namespace System.Drawing.Printing;

/// <summary>Specifies settings that apply to a single, printed page.</summary>
[Serializable]
public class PageSettings : ICloneable
{
	internal bool color;

	internal bool landscape;

	internal PaperSize paperSize;

	internal PaperSource paperSource;

	internal PrinterResolution printerResolution;

	private Margins margins = new Margins();

	private float hardMarginX;

	private float hardMarginY;

	private RectangleF printableArea;

	private PrinterSettings printerSettings;

	/// <summary>Gets the size of the page, taking into account the page orientation specified by the <see cref="P:System.Drawing.Printing.PageSettings.Landscape" /> property.</summary>
	/// <returns>A <see cref="T:System.Drawing.Rectangle" /> that represents the length and width, in hundredths of an inch, of the page.</returns>
	/// <exception cref="T:System.Drawing.Printing.InvalidPrinterException">The printer named in the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property does not exist. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Rectangle Bounds
	{
		get
		{
			int width = paperSize.Width;
			int height = paperSize.Height;
			width -= margins.Left + margins.Right;
			height -= margins.Top + margins.Bottom;
			if (landscape)
			{
				int num = width;
				width = height;
				height = num;
			}
			return new Rectangle(margins.Left, margins.Top, width, height);
		}
	}

	/// <summary>Gets or sets a value indicating whether the page should be printed in color.</summary>
	/// <returns>true if the page should be printed in color; otherwise, false. The default is determined by the printer.</returns>
	/// <exception cref="T:System.Drawing.Printing.InvalidPrinterException">The printer named in the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property does not exist. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool Color
	{
		get
		{
			if (!printerSettings.IsValid)
			{
				throw new InvalidPrinterException(printerSettings);
			}
			return color;
		}
		set
		{
			color = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the page is printed in landscape or portrait orientation.</summary>
	/// <returns>true if the page should be printed in landscape orientation; otherwise, false. The default is determined by the printer.</returns>
	/// <exception cref="T:System.Drawing.Printing.InvalidPrinterException">The printer named in the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property does not exist. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool Landscape
	{
		get
		{
			if (!printerSettings.IsValid)
			{
				throw new InvalidPrinterException(printerSettings);
			}
			return landscape;
		}
		set
		{
			landscape = value;
		}
	}

	/// <summary>Gets or sets the margins for this page.</summary>
	/// <returns>A <see cref="T:System.Drawing.Printing.Margins" /> that represents the margins, in hundredths of an inch, for the page. The default is 1-inch margins on all sides.</returns>
	/// <exception cref="T:System.Drawing.Printing.InvalidPrinterException">The printer named in the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property does not exist. </exception>
	public Margins Margins
	{
		get
		{
			if (!printerSettings.IsValid)
			{
				throw new InvalidPrinterException(printerSettings);
			}
			return margins;
		}
		set
		{
			margins = value;
		}
	}

	/// <summary>Gets or sets the paper size for the page.</summary>
	/// <returns>A <see cref="T:System.Drawing.Printing.PaperSize" /> that represents the size of the paper. The default is the printer's default paper size.</returns>
	/// <exception cref="T:System.Drawing.Printing.InvalidPrinterException">The printer named in the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property does not exist or there is no default printer installed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public PaperSize PaperSize
	{
		get
		{
			if (!printerSettings.IsValid)
			{
				throw new InvalidPrinterException(printerSettings);
			}
			return paperSize;
		}
		set
		{
			if (value != null)
			{
				paperSize = value;
			}
		}
	}

	/// <summary>Gets or sets the page's paper source; for example, the printer's upper tray.</summary>
	/// <returns>A <see cref="T:System.Drawing.Printing.PaperSource" /> that specifies the source of the paper. The default is the printer's default paper source.</returns>
	/// <exception cref="T:System.Drawing.Printing.InvalidPrinterException">The printer named in the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property does not exist or there is no default printer installed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public PaperSource PaperSource
	{
		get
		{
			if (!printerSettings.IsValid)
			{
				throw new InvalidPrinterException(printerSettings);
			}
			return paperSource;
		}
		set
		{
			if (value != null)
			{
				paperSource = value;
			}
		}
	}

	/// <summary>Gets or sets the printer resolution for the page.</summary>
	/// <returns>A <see cref="T:System.Drawing.Printing.PrinterResolution" /> that specifies the printer resolution for the page. The default is the printer's default resolution.</returns>
	/// <exception cref="T:System.Drawing.Printing.InvalidPrinterException">The printer named in the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property does not exist or there is no default printer installed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public PrinterResolution PrinterResolution
	{
		get
		{
			if (!printerSettings.IsValid)
			{
				throw new InvalidPrinterException(printerSettings);
			}
			return printerResolution;
		}
		set
		{
			if (value != null)
			{
				printerResolution = value;
			}
		}
	}

	/// <summary>Gets or sets the printer settings associated with the page.</summary>
	/// <returns>A <see cref="T:System.Drawing.Printing.PrinterSettings" /> that represents the printer settings associated with the page.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public PrinterSettings PrinterSettings
	{
		get
		{
			return printerSettings;
		}
		set
		{
			printerSettings = value;
		}
	}

	/// <summary>Gets the x-coordinate, in hundredths of an inch, of the hard margin at the left of the page.</summary>
	/// <returns>The x-coordinate, in hundredths of an inch, of the left-hand hard margin.</returns>
	public float HardMarginX => hardMarginX;

	/// <summary>Gets the y-coordinate, in hundredths of an inch, of the hard margin at the top of the page.</summary>
	/// <returns>The y-coordinate, in hundredths of an inch, of the hard margin at the top of the page.</returns>
	public float HardMarginY => hardMarginY;

	/// <summary>Gets the bounds of the printable area of the page for the printer.</summary>
	/// <returns>A <see cref="T:System.Drawing.RectangleF" /> representing the length and width, in hundredths of an inch, of the area the printer is capable of printing in.</returns>
	public RectangleF PrintableArea => printableArea;

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PageSettings" /> class using the default printer.</summary>
	public PageSettings()
		: this(new PrinterSettings())
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PageSettings" /> class using a specified printer.</summary>
	/// <param name="printerSettings">The <see cref="T:System.Drawing.Printing.PrinterSettings" /> that describes the printer to use. </param>
	public PageSettings(PrinterSettings printerSettings)
	{
		PrinterSettings = printerSettings;
		color = printerSettings.DefaultPageSettings.color;
		landscape = printerSettings.DefaultPageSettings.landscape;
		paperSize = printerSettings.DefaultPageSettings.paperSize;
		paperSource = printerSettings.DefaultPageSettings.paperSource;
		printerResolution = printerSettings.DefaultPageSettings.printerResolution;
	}

	internal PageSettings(PrinterSettings printerSettings, bool color, bool landscape, PaperSize paperSize, PaperSource paperSource, PrinterResolution printerResolution)
	{
		PrinterSettings = printerSettings;
		this.color = color;
		this.landscape = landscape;
		this.paperSize = paperSize;
		this.paperSource = paperSource;
		this.printerResolution = printerResolution;
	}

	/// <summary>Creates a copy of this <see cref="T:System.Drawing.Printing.PageSettings" />.</summary>
	/// <returns>A copy of this object.</returns>
	public object Clone()
	{
		PrinterResolution printerResolution = new PrinterResolution(this.printerResolution.Kind, this.printerResolution.X, this.printerResolution.Y);
		PaperSource paperSource = new PaperSource(this.paperSource.Kind, this.paperSource.SourceName);
		PaperSize paperSize = new PaperSize(this.paperSize.PaperName, this.paperSize.Width, this.paperSize.Height);
		paperSize.RawKind = (int)this.paperSize.Kind;
		return new PageSettings(printerSettings, color, landscape, paperSize, paperSource, printerResolution)
		{
			Margins = (Margins)margins.Clone()
		};
	}

	/// <summary>Copies the relevant information from the <see cref="T:System.Drawing.Printing.PageSettings" /> to the specified DEVMODE structure.</summary>
	/// <param name="hdevmode">The handle to a Win32 DEVMODE structure. </param>
	/// <exception cref="T:System.Drawing.Printing.InvalidPrinterException">The printer named in the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property does not exist or there is no default printer installed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO("PageSettings.CopyToHdevmode")]
	public void CopyToHdevmode(IntPtr hdevmode)
	{
		throw new NotImplementedException();
	}

	/// <summary>Copies relevant information to the <see cref="T:System.Drawing.Printing.PageSettings" /> from the specified DEVMODE structure.</summary>
	/// <param name="hdevmode">The handle to a Win32 DEVMODE structure. </param>
	/// <exception cref="T:System.ArgumentException">The printer handle is not valid. </exception>
	/// <exception cref="T:System.Drawing.Printing.InvalidPrinterException">The printer named in the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property does not exist or there is no default printer installed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO("PageSettings.SetHdevmode")]
	public void SetHdevmode(IntPtr hdevmode)
	{
		throw new NotImplementedException();
	}

	/// <summary>Converts the <see cref="T:System.Drawing.Printing.PageSettings" /> to string form.</summary>
	/// <returns>A string showing the various property settings for the <see cref="T:System.Drawing.Printing.PageSettings" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override string ToString()
	{
		return string.Format(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("[PageSettings: Color={0}" + ", Landscape={1}", ", Margins={2}"), ", PaperSize={3}"), ", PaperSource={4}"), ", PrinterResolution={5}"), "]"), color, landscape, margins, paperSize, paperSource, printerResolution);
	}
}
