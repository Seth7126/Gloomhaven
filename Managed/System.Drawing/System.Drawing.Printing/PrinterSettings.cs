using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Imaging;

namespace System.Drawing.Printing;

/// <summary>Specifies information about how a document is printed, including the printer that prints it, when printing from a Windows Forms application.</summary>
[Serializable]
public class PrinterSettings : ICloneable
{
	/// <summary>Contains a collection of <see cref="T:System.Drawing.Printing.PaperSource" /> objects.</summary>
	public class PaperSourceCollection : ICollection, IEnumerable
	{
		private ArrayList _PaperSources = new ArrayList();

		/// <summary>Gets the number of different paper sources in the collection.</summary>
		/// <returns>The number of different paper sources in the collection.</returns>
		public int Count => _PaperSources.Count;

		/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.Count" />.</summary>
		int ICollection.Count => _PaperSources.Count;

		/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.IsSynchronized" />.</summary>
		bool ICollection.IsSynchronized => false;

		/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.SyncRoot" />.</summary>
		object ICollection.SyncRoot => this;

		/// <summary>Gets the <see cref="T:System.Drawing.Printing.PaperSource" /> at a specified index.</summary>
		/// <returns>The <see cref="T:System.Drawing.Printing.PaperSource" /> at the specified index.</returns>
		/// <param name="index">The index of the <see cref="T:System.Drawing.Printing.PaperSource" /> to get. </param>
		public virtual PaperSource this[int index] => _PaperSources[index] as PaperSource;

		/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PrinterSettings.PaperSourceCollection" /> class.</summary>
		/// <param name="array">An array of type <see cref="T:System.Drawing.Printing.PaperSource" />. </param>
		public PaperSourceCollection(PaperSource[] array)
		{
			foreach (PaperSource value in array)
			{
				_PaperSources.Add(value);
			}
		}

		/// <summary>Adds the specified <see cref="T:System.Drawing.Printing.PaperSource" /> to end of the <see cref="T:System.Drawing.Printing.PrinterSettings.PaperSourceCollection" />.</summary>
		/// <returns>The zero-based index where the <see cref="T:System.Drawing.Printing.PaperSource" /> was added.</returns>
		/// <param name="paperSource">The <see cref="T:System.Drawing.Printing.PaperSource" /> to add to the collection.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public int Add(PaperSource paperSource)
		{
			return _PaperSources.Add(paperSource);
		}

		/// <summary>Copies the contents of the current <see cref="T:System.Drawing.Printing.PrinterSettings.PaperSourceCollection" /> to the specified array, starting at the specified index.</summary>
		/// <param name="paperSources">A zero-based array that receives the items copied from the <see cref="T:System.Drawing.Printing.PrinterSettings.PaperSourceCollection" />.</param>
		/// <param name="index">The index at which to start copying items.</param>
		public void CopyTo(PaperSource[] paperSources, int index)
		{
			throw new NotImplementedException();
		}

		/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _PaperSources.GetEnumerator();
		}

		/// <summary>Returns an enumerator that can iterate through the collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Drawing.Printing.PrinterSettings.PaperSourceCollection" />.</returns>
		public IEnumerator GetEnumerator()
		{
			return _PaperSources.GetEnumerator();
		}

		/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
		/// <param name="array">The destination array for the contents of the collection.</param>
		/// <param name="index">The index at which to start the copy operation.</param>
		void ICollection.CopyTo(Array array, int index)
		{
			_PaperSources.CopyTo(array, index);
		}

		internal void Clear()
		{
			_PaperSources.Clear();
		}
	}

	/// <summary>Contains a collection of <see cref="T:System.Drawing.Printing.PaperSize" /> objects.</summary>
	public class PaperSizeCollection : ICollection, IEnumerable
	{
		private ArrayList _PaperSizes = new ArrayList();

		/// <summary>Gets the number of different paper sizes in the collection.</summary>
		/// <returns>The number of different paper sizes in the collection.</returns>
		public int Count => _PaperSizes.Count;

		/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.Count" />.</summary>
		int ICollection.Count => _PaperSizes.Count;

		/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.IsSynchronized" />.</summary>
		bool ICollection.IsSynchronized => false;

		/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.SyncRoot" />.</summary>
		object ICollection.SyncRoot => this;

		/// <summary>Gets the <see cref="T:System.Drawing.Printing.PaperSize" /> at a specified index.</summary>
		/// <returns>The <see cref="T:System.Drawing.Printing.PaperSize" /> at the specified index.</returns>
		/// <param name="index">The index of the <see cref="T:System.Drawing.Printing.PaperSize" /> to get. </param>
		public virtual PaperSize this[int index] => _PaperSizes[index] as PaperSize;

		/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PrinterSettings.PaperSizeCollection" /> class.</summary>
		/// <param name="array">An array of type <see cref="T:System.Drawing.Printing.PaperSize" />. </param>
		public PaperSizeCollection(PaperSize[] array)
		{
			foreach (PaperSize value in array)
			{
				_PaperSizes.Add(value);
			}
		}

		/// <summary>Adds a <see cref="T:System.Drawing.Printing.PrinterResolution" /> to the end of the collection.</summary>
		/// <returns>The zero-based index of the newly added item.</returns>
		/// <param name="paperSize">The <see cref="T:System.Drawing.Printing.PaperSize" /> to add to the collection.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public int Add(PaperSize paperSize)
		{
			return _PaperSizes.Add(paperSize);
		}

		/// <summary>Copies the contents of the current <see cref="T:System.Drawing.Printing.PrinterSettings.PaperSizeCollection" /> to the specified array, starting at the specified index.</summary>
		/// <param name="paperSizes">A zero-based array that receives the items copied from the <see cref="T:System.Drawing.Printing.PrinterSettings.PaperSizeCollection" />.</param>
		/// <param name="index">The index at which to start copying items.</param>
		public void CopyTo(PaperSize[] paperSizes, int index)
		{
			throw new NotImplementedException();
		}

		/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
		/// <returns>An enumerator associated with the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _PaperSizes.GetEnumerator();
		}

		/// <summary>Returns an enumerator that can iterate through the collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Drawing.Printing.PrinterSettings.PaperSizeCollection" />.</returns>
		public IEnumerator GetEnumerator()
		{
			return _PaperSizes.GetEnumerator();
		}

		/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
		/// <param name="array">A zero-based array that receives the items copied from the collection.</param>
		/// <param name="index">The index at which to start copying items.</param>
		void ICollection.CopyTo(Array array, int index)
		{
			_PaperSizes.CopyTo(array, index);
		}

		internal void Clear()
		{
			_PaperSizes.Clear();
		}
	}

	/// <summary>Contains a collection of <see cref="T:System.Drawing.Printing.PrinterResolution" /> objects.</summary>
	public class PrinterResolutionCollection : ICollection, IEnumerable
	{
		private ArrayList _PrinterResolutions = new ArrayList();

		/// <summary>Gets the number of available printer resolutions in the collection.</summary>
		/// <returns>The number of available printer resolutions in the collection.</returns>
		public int Count => _PrinterResolutions.Count;

		/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.Count" />.</summary>
		int ICollection.Count => _PrinterResolutions.Count;

		/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.IsSynchronized" />.</summary>
		bool ICollection.IsSynchronized => false;

		/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.SyncRoot" />.</summary>
		object ICollection.SyncRoot => this;

		/// <summary>Gets the <see cref="T:System.Drawing.Printing.PrinterResolution" /> at a specified index.</summary>
		/// <returns>The <see cref="T:System.Drawing.Printing.PrinterResolution" /> at the specified index.</returns>
		/// <param name="index">The index of the <see cref="T:System.Drawing.Printing.PrinterResolution" /> to get. </param>
		public virtual PrinterResolution this[int index] => _PrinterResolutions[index] as PrinterResolution;

		/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PrinterSettings.PrinterResolutionCollection" /> class.</summary>
		/// <param name="array">An array of type <see cref="T:System.Drawing.Printing.PrinterResolution" />. </param>
		public PrinterResolutionCollection(PrinterResolution[] array)
		{
			foreach (PrinterResolution value in array)
			{
				_PrinterResolutions.Add(value);
			}
		}

		/// <summary>Adds a <see cref="T:System.Drawing.Printing.PrinterResolution" /> to the end of the collection.</summary>
		/// <returns>The zero-based index of the newly added item.</returns>
		/// <param name="printerResolution">The <see cref="T:System.Drawing.Printing.PrinterResolution" /> to add to the collection.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public int Add(PrinterResolution printerResolution)
		{
			return _PrinterResolutions.Add(printerResolution);
		}

		/// <summary>Copies the contents of the current <see cref="T:System.Drawing.Printing.PrinterSettings.PrinterResolutionCollection" /> to the specified array, starting at the specified index.</summary>
		/// <param name="printerResolutions">A zero-based array that receives the items copied from the <see cref="T:System.Drawing.Printing.PrinterSettings.PrinterResolutionCollection" />.</param>
		/// <param name="index">The index at which to start copying items.</param>
		public void CopyTo(PrinterResolution[] printerResolutions, int index)
		{
			throw new NotImplementedException();
		}

		/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _PrinterResolutions.GetEnumerator();
		}

		/// <summary>Returns an enumerator that can iterate through the collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Drawing.Printing.PrinterSettings.PrinterResolutionCollection" />.</returns>
		public IEnumerator GetEnumerator()
		{
			return _PrinterResolutions.GetEnumerator();
		}

		/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
		/// <param name="array">The destination array.</param>
		/// <param name="index">The index at which to start the copy operation.</param>
		void ICollection.CopyTo(Array array, int index)
		{
			_PrinterResolutions.CopyTo(array, index);
		}

		internal void Clear()
		{
			_PrinterResolutions.Clear();
		}
	}

	/// <summary>Contains a collection of <see cref="T:System.String" /> objects.</summary>
	public class StringCollection : ICollection, IEnumerable
	{
		private ArrayList _Strings = new ArrayList();

		/// <summary>Gets the number of strings in the collection.</summary>
		/// <returns>The number of strings in the collection.</returns>
		public int Count => _Strings.Count;

		/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.Count" />.</summary>
		int ICollection.Count => _Strings.Count;

		/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.IsSynchronized" />.</summary>
		bool ICollection.IsSynchronized => false;

		/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.SyncRoot" />.</summary>
		object ICollection.SyncRoot => this;

		/// <summary>Gets the <see cref="T:System.String" /> at a specified index.</summary>
		/// <returns>The <see cref="T:System.String" /> at the specified index.</returns>
		/// <param name="index">The index of the <see cref="T:System.String" /> to get. </param>
		public virtual string this[int index] => _Strings[index] as string;

		/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PrinterSettings.StringCollection" /> class.</summary>
		/// <param name="array">An array of type <see cref="T:System.String" />. </param>
		public StringCollection(string[] array)
		{
			foreach (string value in array)
			{
				_Strings.Add(value);
			}
		}

		/// <summary>Adds a string to the end of the collection.</summary>
		/// <returns>The zero-based index of the newly added item.</returns>
		/// <param name="value">The string to add to the collection.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public int Add(string value)
		{
			return _Strings.Add(value);
		}

		/// <summary>Copies the contents of the current <see cref="T:System.Drawing.Printing.PrinterSettings.PrinterResolutionCollection" /> to the specified array, starting at the specified index</summary>
		/// <param name="strings">A zero-based array that receives the items copied from the <see cref="T:System.Drawing.Printing.PrinterSettings.StringCollection" />.</param>
		/// <param name="index">The index at which to start copying items.</param>
		public void CopyTo(string[] strings, int index)
		{
			throw new NotImplementedException();
		}

		/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _Strings.GetEnumerator();
		}

		/// <summary>Returns an enumerator that can iterate through the collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Drawing.Printing.PrinterSettings.StringCollection" />.</returns>
		public IEnumerator GetEnumerator()
		{
			return _Strings.GetEnumerator();
		}

		/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
		/// <param name="array">The array for items to be copied to.</param>
		/// <param name="index">The starting index.</param>
		void ICollection.CopyTo(Array array, int index)
		{
			_Strings.CopyTo(array, index);
		}
	}

	private string printer_name;

	private string print_filename;

	private short copies;

	private int maximum_page;

	private int minimum_page;

	private int from_page;

	private int to_page;

	private bool collate;

	private PrintRange print_range;

	internal int maximum_copies;

	internal bool can_duplex;

	internal bool supports_color;

	internal int landscape_angle;

	private bool print_tofile;

	internal PrinterResolutionCollection printer_resolutions;

	internal PaperSizeCollection paper_sizes;

	internal PaperSourceCollection paper_sources;

	private PageSettings default_pagesettings;

	private Duplex duplex;

	internal bool is_plotter;

	private PrintingServices printing_services;

	internal NameValueCollection printer_capabilities;

	/// <summary>Gets a value indicating whether the printer supports double-sided printing.</summary>
	/// <returns>true if the printer supports double-sided printing; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool CanDuplex => can_duplex;

	/// <summary>Gets or sets a value indicating whether the printed document is collated.</summary>
	/// <returns>true if the printed document is collated; otherwise, false. The default is false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool Collate
	{
		get
		{
			return collate;
		}
		set
		{
			collate = value;
		}
	}

	/// <summary>Gets or sets the number of copies of the document to print.</summary>
	/// <returns>The number of copies to print. The default is 1.</returns>
	/// <exception cref="T:System.ArgumentException">The value of the <see cref="P:System.Drawing.Printing.PrinterSettings.Copies" /> property is less than zero. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public short Copies
	{
		get
		{
			return copies;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException("The value of the Copies property is less than zero.");
			}
			copies = value;
		}
	}

	/// <summary>Gets the default page settings for this printer.</summary>
	/// <returns>A <see cref="T:System.Drawing.Printing.PageSettings" /> that represents the default page settings for this printer.</returns>
	public PageSettings DefaultPageSettings
	{
		get
		{
			if (default_pagesettings == null)
			{
				default_pagesettings = new PageSettings(this, SupportsColor, landscape: false, new PaperSize("A4", 827, 1169), new PaperSource(PaperSourceKind.FormSource, "Tray"), new PrinterResolution(PrinterResolutionKind.Medium, 200, 200));
			}
			return default_pagesettings;
		}
	}

	/// <summary>Gets or sets the printer setting for double-sided printing.</summary>
	/// <returns>One of the <see cref="T:System.Drawing.Printing.Duplex" /> values. The default is determined by the printer.</returns>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The value of the <see cref="P:System.Drawing.Printing.PrinterSettings.Duplex" /> property is not one of the <see cref="T:System.Drawing.Printing.Duplex" /> values. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Duplex Duplex
	{
		get
		{
			return duplex;
		}
		set
		{
			duplex = value;
		}
	}

	/// <summary>Gets or sets the page number of the first page to print.</summary>
	/// <returns>The page number of the first page to print.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Printing.PrinterSettings.FromPage" /> property's value is less than zero. </exception>
	public int FromPage
	{
		get
		{
			return from_page;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException("The value of the FromPage property is less than zero");
			}
			from_page = value;
		}
	}

	/// <summary>Gets the names of all printers installed on the computer.</summary>
	/// <returns>A <see cref="T:System.Drawing.Printing.PrinterSettings.StringCollection" /> that represents the names of all printers installed on the computer.</returns>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The available printers could not be enumerated. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static StringCollection InstalledPrinters => SysPrn.GlobalService.InstalledPrinters;

	/// <summary>Gets a value indicating whether the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property designates the default printer, except when the user explicitly sets <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" />.</summary>
	/// <returns>true if <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> designates the default printer; otherwise, false.</returns>
	public bool IsDefaultPrinter => printer_name == printing_services.DefaultPrinter;

	/// <summary>Gets a value indicating whether the printer is a plotter.</summary>
	/// <returns>true if the printer is a plotter; false if the printer is a raster.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsPlotter => is_plotter;

	/// <summary>Gets a value indicating whether the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property designates a valid printer.</summary>
	/// <returns>true if the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property designates a valid printer; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsValid => printing_services.IsPrinterValid(printer_name);

	/// <summary>Gets the angle, in degrees, that the portrait orientation is rotated to produce the landscape orientation.</summary>
	/// <returns>The angle, in degrees, that the portrait orientation is rotated to produce the landscape orientation.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public int LandscapeAngle => landscape_angle;

	/// <summary>Gets the maximum number of copies that the printer enables the user to print at a time.</summary>
	/// <returns>The maximum number of copies that the printer enables the user to print at a time.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public int MaximumCopies => maximum_copies;

	/// <summary>Gets or sets the maximum <see cref="P:System.Drawing.Printing.PrinterSettings.FromPage" /> or <see cref="P:System.Drawing.Printing.PrinterSettings.ToPage" /> that can be selected in a <see cref="T:System.Windows.Forms.PrintDialog" />.</summary>
	/// <returns>The maximum <see cref="P:System.Drawing.Printing.PrinterSettings.FromPage" /> or <see cref="P:System.Drawing.Printing.PrinterSettings.ToPage" /> that can be selected in a <see cref="T:System.Windows.Forms.PrintDialog" />.</returns>
	/// <exception cref="T:System.ArgumentException">The value of the <see cref="P:System.Drawing.Printing.PrinterSettings.MaximumPage" /> property is less than zero. </exception>
	public int MaximumPage
	{
		get
		{
			return maximum_page;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException("The value of the MaximumPage property is less than zero");
			}
			maximum_page = value;
		}
	}

	/// <summary>Gets or sets the minimum <see cref="P:System.Drawing.Printing.PrinterSettings.FromPage" /> or <see cref="P:System.Drawing.Printing.PrinterSettings.ToPage" /> that can be selected in a <see cref="T:System.Windows.Forms.PrintDialog" />.</summary>
	/// <returns>The minimum <see cref="P:System.Drawing.Printing.PrinterSettings.FromPage" /> or <see cref="P:System.Drawing.Printing.PrinterSettings.ToPage" /> that can be selected in a <see cref="T:System.Windows.Forms.PrintDialog" />.</returns>
	/// <exception cref="T:System.ArgumentException">The value of the <see cref="P:System.Drawing.Printing.PrinterSettings.MinimumPage" /> property is less than zero. </exception>
	public int MinimumPage
	{
		get
		{
			return minimum_page;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException("The value of the MaximumPage property is less than zero");
			}
			minimum_page = value;
		}
	}

	/// <summary>Gets the paper sizes that are supported by this printer.</summary>
	/// <returns>A <see cref="T:System.Drawing.Printing.PrinterSettings.PaperSizeCollection" /> that represents the paper sizes that are supported by this printer.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public PaperSizeCollection PaperSizes
	{
		get
		{
			if (!IsValid)
			{
				throw new InvalidPrinterException(this);
			}
			return paper_sizes;
		}
	}

	/// <summary>Gets the paper source trays that are available on the printer.</summary>
	/// <returns>A <see cref="T:System.Drawing.Printing.PrinterSettings.PaperSourceCollection" /> that represents the paper source trays that are available on this printer.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public PaperSourceCollection PaperSources
	{
		get
		{
			if (!IsValid)
			{
				throw new InvalidPrinterException(this);
			}
			return paper_sources;
		}
	}

	/// <summary>Gets or sets the file name, when printing to a file.</summary>
	/// <returns>The file name, when printing to a file.</returns>
	public string PrintFileName
	{
		get
		{
			return print_filename;
		}
		set
		{
			print_filename = value;
		}
	}

	/// <summary>Gets or sets the name of the printer to use.</summary>
	/// <returns>The name of the printer to use.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string PrinterName
	{
		get
		{
			return printer_name;
		}
		set
		{
			if (!(printer_name == value))
			{
				printer_name = value;
				printing_services.LoadPrinterSettings(printer_name, this);
			}
		}
	}

	/// <summary>Gets all the resolutions that are supported by this printer.</summary>
	/// <returns>A <see cref="T:System.Drawing.Printing.PrinterSettings.PrinterResolutionCollection" /> that represents the resolutions that are supported by this printer.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public PrinterResolutionCollection PrinterResolutions
	{
		get
		{
			if (!IsValid)
			{
				throw new InvalidPrinterException(this);
			}
			if (printer_resolutions == null)
			{
				printer_resolutions = new PrinterResolutionCollection(new PrinterResolution[0]);
				printing_services.LoadPrinterResolutions(printer_name, this);
			}
			return printer_resolutions;
		}
	}

	/// <summary>Gets or sets the page numbers that the user has specified to be printed.</summary>
	/// <returns>One of the <see cref="T:System.Drawing.Printing.PrintRange" /> values.</returns>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The value of the <see cref="P:System.Drawing.Printing.PrinterSettings.PrintRange" /> property is not one of the <see cref="T:System.Drawing.Printing.PrintRange" /> values. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public PrintRange PrintRange
	{
		get
		{
			return print_range;
		}
		set
		{
			if (value != PrintRange.AllPages && value != PrintRange.Selection && value != PrintRange.SomePages)
			{
				throw new InvalidEnumArgumentException("The value of the PrintRange property is not one of the PrintRange values");
			}
			print_range = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the printing output is sent to a file instead of a port.</summary>
	/// <returns>true if the printing output is sent to a file; otherwise, false. The default is false.</returns>
	public bool PrintToFile
	{
		get
		{
			return print_tofile;
		}
		set
		{
			print_tofile = value;
		}
	}

	/// <summary>Gets a value indicating whether this printer supports color printing.</summary>
	/// <returns>true if this printer supports color; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool SupportsColor => supports_color;

	/// <summary>Gets or sets the number of the last page to print.</summary>
	/// <returns>The number of the last page to print.</returns>
	/// <exception cref="T:System.ArgumentException">The value of the <see cref="P:System.Drawing.Printing.PrinterSettings.ToPage" /> property is less than zero. </exception>
	public int ToPage
	{
		get
		{
			return to_page;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException("The value of the ToPage property is less than zero");
			}
			to_page = value;
		}
	}

	internal NameValueCollection PrinterCapabilities
	{
		get
		{
			if (printer_capabilities == null)
			{
				printer_capabilities = new NameValueCollection();
			}
			return printer_capabilities;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PrinterSettings" /> class.</summary>
	public PrinterSettings()
		: this(SysPrn.CreatePrintingService())
	{
	}

	internal PrinterSettings(PrintingServices printing_services)
	{
		this.printing_services = printing_services;
		printer_name = printing_services.DefaultPrinter;
		ResetToDefaults();
		printing_services.LoadPrinterSettings(printer_name, this);
	}

	private void ResetToDefaults()
	{
		printer_resolutions = null;
		paper_sizes = null;
		paper_sources = null;
		default_pagesettings = null;
		maximum_page = 9999;
		copies = 1;
		collate = true;
	}

	/// <summary>Creates a copy of this <see cref="T:System.Drawing.Printing.PrinterSettings" />.</summary>
	/// <returns>A copy of this object.</returns>
	public object Clone()
	{
		return new PrinterSettings(printing_services);
	}

	/// <summary>Returns a <see cref="T:System.Drawing.Graphics" /> that contains printer information that is useful when creating a <see cref="T:System.Drawing.Printing.PrintDocument" />. </summary>
	/// <returns>A <see cref="T:System.Drawing.Graphics" /> that contains information from a printer.</returns>
	/// <exception cref="T:System.Drawing.Printing.InvalidPrinterException">The printer named in the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property does not exist. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO("PrinterSettings.CreateMeasurementGraphics")]
	public Graphics CreateMeasurementGraphics()
	{
		throw new NotImplementedException();
	}

	/// <summary>Returns a <see cref="T:System.Drawing.Graphics" /> that contains printer information, optionally specifying the origin at the margins.</summary>
	/// <returns>A <see cref="T:System.Drawing.Graphics" /> that contains printer information from the <see cref="T:System.Drawing.Printing.PageSettings" />.</returns>
	/// <param name="honorOriginAtMargins">true to indicate the origin at the margins; otherwise, false. </param>
	[System.MonoTODO("PrinterSettings.CreateMeasurementGraphics")]
	public Graphics CreateMeasurementGraphics(bool honorOriginAtMargins)
	{
		throw new NotImplementedException();
	}

	/// <summary>Returns a <see cref="T:System.Drawing.Graphics" /> that contains printer information associated with the specified <see cref="T:System.Drawing.Printing.PageSettings" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Graphics" /> that contains printer information from the <see cref="T:System.Drawing.Printing.PageSettings" />.</returns>
	/// <param name="pageSettings">The <see cref="T:System.Drawing.Printing.PageSettings" /> to retrieve a graphics object for.</param>
	[System.MonoTODO("PrinterSettings.CreateMeasurementGraphics")]
	public Graphics CreateMeasurementGraphics(PageSettings pageSettings)
	{
		throw new NotImplementedException();
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Graphics" /> associated with the specified page settings and optionally specifying the origin at the margins.</summary>
	/// <returns>A <see cref="T:System.Drawing.Graphics" /> that contains printer information from the <see cref="T:System.Drawing.Printing.PageSettings" />.</returns>
	/// <param name="pageSettings">The <see cref="T:System.Drawing.Printing.PageSettings" /> to retrieve a <see cref="T:System.Drawing.Graphics" /> object for.</param>
	/// <param name="honorOriginAtMargins">true to specify the origin at the margins; otherwise, false. </param>
	[System.MonoTODO("PrinterSettings.CreateMeasurementGraphics")]
	public Graphics CreateMeasurementGraphics(PageSettings pageSettings, bool honorOriginAtMargins)
	{
		throw new NotImplementedException();
	}

	/// <summary>Creates a handle to a DEVMODE structure that corresponds to the printer settings.</summary>
	/// <returns>A handle to a DEVMODE structure.</returns>
	/// <exception cref="T:System.Drawing.Printing.InvalidPrinterException">The printer named in the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property does not exist. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The printer's initialization information could not be retrieved. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO("PrinterSettings.GetHdevmode")]
	public IntPtr GetHdevmode()
	{
		throw new NotImplementedException();
	}

	/// <summary>Creates a handle to a DEVMODE structure that corresponds to the printer and the page settings specified through the <paramref name="pageSettings" /> parameter.</summary>
	/// <returns>A handle to a DEVMODE structure.</returns>
	/// <param name="pageSettings">The <see cref="T:System.Drawing.Printing.PageSettings" /> object that the DEVMODE structure's handle corresponds to. </param>
	/// <exception cref="T:System.Drawing.Printing.InvalidPrinterException">The printer named in the <see cref="P:System.Drawing.Printing.PrinterSettings.PrinterName" /> property does not exist. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The printer's initialization information could not be retrieved. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO("PrinterSettings.GetHdevmode")]
	public IntPtr GetHdevmode(PageSettings pageSettings)
	{
		throw new NotImplementedException();
	}

	/// <summary>Creates a handle to a DEVNAMES structure that corresponds to the printer settings.</summary>
	/// <returns>A handle to a DEVNAMES structure.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO("PrinterSettings.GetHdevname")]
	public IntPtr GetHdevnames()
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets a value indicating whether the printer supports printing the specified image file.</summary>
	/// <returns>true if the printer supports printing the specified image; otherwise, false.</returns>
	/// <param name="image">The image to print.</param>
	[System.MonoTODO("IsDirectPrintingSupported")]
	public bool IsDirectPrintingSupported(Image image)
	{
		throw new NotImplementedException();
	}

	/// <summary>Returns a value indicating whether the printer supports printing the specified image format.</summary>
	/// <returns>true if the printer supports printing the specified image format; otherwise, false.</returns>
	/// <param name="imageFormat">An <see cref="T:System.Drawing.Imaging.ImageFormat" /> to print.</param>
	[System.MonoTODO("IsDirectPrintingSupported")]
	public bool IsDirectPrintingSupported(ImageFormat imageFormat)
	{
		throw new NotImplementedException();
	}

	/// <summary>Copies the relevant information out of the given handle and into the <see cref="T:System.Drawing.Printing.PrinterSettings" />.</summary>
	/// <param name="hdevmode">The handle to a Win32 DEVMODE structure. </param>
	/// <exception cref="T:System.ArgumentException">The printer handle is not valid. </exception>
	[System.MonoTODO("PrinterSettings.SetHdevmode")]
	public void SetHdevmode(IntPtr hdevmode)
	{
		throw new NotImplementedException();
	}

	/// <summary>Copies the relevant information out of the given handle and into the <see cref="T:System.Drawing.Printing.PrinterSettings" />.</summary>
	/// <param name="hdevnames">The handle to a Win32 DEVNAMES structure. </param>
	/// <exception cref="T:System.ArgumentException">The printer handle is invalid. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO("PrinterSettings.SetHdevnames")]
	public void SetHdevnames(IntPtr hdevnames)
	{
		throw new NotImplementedException();
	}

	/// <summary>Provides information about the <see cref="T:System.Drawing.Printing.PrinterSettings" /> in string form.</summary>
	/// <returns>A string.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override string ToString()
	{
		return "Printer [PrinterSettings " + printer_name + " Copies=" + copies + " Collate=" + collate + " Duplex=" + can_duplex + " FromPage=" + from_page + " LandscapeAngle=" + landscape_angle + " MaximumCopies=" + maximum_copies + " OutputPort= ToPage=" + to_page + "]";
	}
}
