using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Drawing.Printing;

internal class PrintingServicesUnix : PrintingServices
{
	public struct DOCINFO
	{
		public PrinterSettings settings;

		public PageSettings default_page_settings;

		public string title;

		public string filename;
	}

	public struct PPD_SIZE
	{
		public int marked;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)]
		public string name;

		public float width;

		public float length;

		public float left;

		public float bottom;

		public float right;

		public float top;
	}

	public struct PPD_GROUP
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
		public string text;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)]
		public string name;

		public int num_options;

		public IntPtr options;

		public int num_subgroups;

		public IntPtr subgrups;
	}

	public struct PPD_OPTION
	{
		public byte conflicted;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
		public string keyword;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
		public string defchoice;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
		public string text;

		public int ui;

		public int section;

		public float order;

		public int num_choices;

		public IntPtr choices;
	}

	public struct PPD_CHOICE
	{
		public byte marked;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
		public string choice;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
		public string text;

		public IntPtr code;

		public IntPtr option;
	}

	public struct PPD_FILE
	{
		public int language_level;

		public int color_device;

		public int variable_sizes;

		public int accurate_screens;

		public int contone_only;

		public int landscape;

		public int model_number;

		public int manual_copies;

		public int throughput;

		public int colorspace;

		public IntPtr patches;

		public int num_emulations;

		public IntPtr emulations;

		public IntPtr jcl_begin;

		public IntPtr jcl_ps;

		public IntPtr jcl_end;

		public IntPtr lang_encoding;

		public IntPtr lang_version;

		public IntPtr modelname;

		public IntPtr ttrasterizer;

		public IntPtr manufacturer;

		public IntPtr product;

		public IntPtr nickname;

		public IntPtr shortnickname;

		public int num_groups;

		public IntPtr groups;

		public int num_sizes;

		public IntPtr sizes;
	}

	public struct CUPS_OPTIONS
	{
		public IntPtr name;

		public IntPtr val;
	}

	public struct CUPS_DESTS
	{
		public IntPtr name;

		public IntPtr instance;

		public int is_default;

		public int num_options;

		public IntPtr options;
	}

	private static Hashtable doc_info;

	private static bool cups_installed;

	private static Hashtable installed_printers;

	private static string default_printer;

	private static string tmpfile;

	internal static PrinterSettings.StringCollection InstalledPrinters
	{
		get
		{
			LoadPrinters();
			PrinterSettings.StringCollection stringCollection = new PrinterSettings.StringCollection(new string[0]);
			foreach (object key in installed_printers.Keys)
			{
				stringCollection.Add(key.ToString());
			}
			return stringCollection;
		}
	}

	internal override string DefaultPrinter
	{
		get
		{
			if (installed_printers.Count == 0)
			{
				LoadPrinters();
			}
			return default_printer;
		}
	}

	internal PrintingServicesUnix()
	{
	}

	static PrintingServicesUnix()
	{
		doc_info = new Hashtable();
		default_printer = string.Empty;
		installed_printers = new Hashtable();
		CheckCupsInstalled();
	}

	private static void CheckCupsInstalled()
	{
		try
		{
			cupsGetDefault();
		}
		catch (DllNotFoundException)
		{
			Console.WriteLine("libcups not found. To have printing support, you need cups installed");
			cups_installed = false;
			return;
		}
		cups_installed = true;
	}

	private IntPtr OpenPrinter(string printer)
	{
		try
		{
			return ppdOpenFile(Marshal.PtrToStringAnsi(cupsGetPPD(printer)));
		}
		catch (Exception)
		{
			Console.WriteLine("There was an error opening the printer {0}. Please check your cups installation.");
		}
		return IntPtr.Zero;
	}

	private void ClosePrinter(ref IntPtr handle)
	{
		try
		{
			if (handle != IntPtr.Zero)
			{
				ppdClose(handle);
			}
		}
		finally
		{
			handle = IntPtr.Zero;
		}
	}

	private static int OpenDests(ref IntPtr ptr)
	{
		try
		{
			return cupsGetDests(ref ptr);
		}
		catch
		{
			ptr = IntPtr.Zero;
		}
		return 0;
	}

	private static void CloseDests(ref IntPtr ptr, int count)
	{
		try
		{
			if (ptr != IntPtr.Zero)
			{
				cupsFreeDests(count, ptr);
			}
		}
		finally
		{
			ptr = IntPtr.Zero;
		}
	}

	internal override bool IsPrinterValid(string printer)
	{
		if (!cups_installed || ((printer == null) | (printer == string.Empty)))
		{
			return false;
		}
		return installed_printers.Contains(printer);
	}

	internal override void LoadPrinterSettings(string printer, PrinterSettings settings)
	{
		if (!cups_installed || printer == null || printer == string.Empty)
		{
			return;
		}
		if (installed_printers.Count == 0)
		{
			LoadPrinters();
		}
		if (((SysPrn.Printer)installed_printers[printer]).Settings != null)
		{
			SysPrn.Printer printer2 = (SysPrn.Printer)installed_printers[printer];
			settings.can_duplex = printer2.Settings.can_duplex;
			settings.is_plotter = printer2.Settings.is_plotter;
			settings.landscape_angle = printer2.Settings.landscape_angle;
			settings.maximum_copies = printer2.Settings.maximum_copies;
			settings.paper_sizes = printer2.Settings.paper_sizes;
			settings.paper_sources = printer2.Settings.paper_sources;
			settings.printer_capabilities = printer2.Settings.printer_capabilities;
			settings.printer_resolutions = printer2.Settings.printer_resolutions;
			settings.supports_color = printer2.Settings.supports_color;
			return;
		}
		settings.PrinterCapabilities.Clear();
		IntPtr ptr = IntPtr.Zero;
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		string text = string.Empty;
		int num = 0;
		try
		{
			num = OpenDests(ref ptr);
			if (num == 0)
			{
				return;
			}
			int num2 = Marshal.SizeOf(typeof(CUPS_DESTS));
			zero = ptr;
			for (int i = 0; i < num; i++)
			{
				if (Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(zero)).Equals(printer))
				{
					text = printer;
					break;
				}
				zero = (IntPtr)((long)zero + num2);
			}
			if (!text.Equals(printer))
			{
				return;
			}
			zero2 = OpenPrinter(printer);
			if (!(zero2 == IntPtr.Zero))
			{
				CUPS_DESTS cUPS_DESTS = (CUPS_DESTS)Marshal.PtrToStructure(zero, typeof(CUPS_DESTS));
				NameValueCollection nameValueCollection = new NameValueCollection();
				NameValueCollection paper_names = new NameValueCollection();
				NameValueCollection paper_sources = new NameValueCollection();
				LoadPrinterOptions(cUPS_DESTS.options, cUPS_DESTS.num_options, zero2, nameValueCollection, paper_names, out var defsize, paper_sources, out var defsource);
				if (settings.paper_sizes == null)
				{
					settings.paper_sizes = new PrinterSettings.PaperSizeCollection(new PaperSize[0]);
				}
				else
				{
					settings.paper_sizes.Clear();
				}
				if (settings.paper_sources == null)
				{
					settings.paper_sources = new PrinterSettings.PaperSourceCollection(new PaperSource[0]);
				}
				else
				{
					settings.paper_sources.Clear();
				}
				settings.DefaultPageSettings.PaperSource = LoadPrinterPaperSources(settings, defsource, paper_sources);
				settings.DefaultPageSettings.PaperSize = LoadPrinterPaperSizes(zero2, settings, defsize, paper_names);
				LoadPrinterResolutionsAndDefault(printer, settings, zero2);
				PPD_FILE pPD_FILE = (PPD_FILE)Marshal.PtrToStructure(zero2, typeof(PPD_FILE));
				settings.landscape_angle = pPD_FILE.landscape;
				settings.supports_color = ((pPD_FILE.color_device != 0) ? true : false);
				settings.can_duplex = nameValueCollection["Duplex"] != null;
				ClosePrinter(ref zero2);
				((SysPrn.Printer)installed_printers[printer]).Settings = settings;
			}
		}
		finally
		{
			CloseDests(ref ptr, num);
		}
	}

	private static void LoadPrinterOptions(IntPtr options, int numOptions, IntPtr ppd, NameValueCollection list, NameValueCollection paper_names, out string defsize, NameValueCollection paper_sources, out string defsource)
	{
		int num = Marshal.SizeOf(typeof(CUPS_OPTIONS));
		LoadOptionList(ppd, "PageSize", paper_names, out defsize);
		LoadOptionList(ppd, "InputSlot", paper_sources, out defsource);
		for (int i = 0; i < numOptions; i++)
		{
			CUPS_OPTIONS obj = (CUPS_OPTIONS)Marshal.PtrToStructure(options, typeof(CUPS_OPTIONS));
			string text = Marshal.PtrToStringAnsi(obj.name);
			string text2 = Marshal.PtrToStringAnsi(obj.val);
			if (text == "PageSize")
			{
				defsize = text2;
			}
			else if (text == "InputSlot")
			{
				defsource = text2;
			}
			list.Add(text, text2);
			options = (IntPtr)((long)options + num);
		}
	}

	private static NameValueCollection LoadPrinterOptions(IntPtr options, int numOptions)
	{
		int num = Marshal.SizeOf(typeof(CUPS_OPTIONS));
		NameValueCollection nameValueCollection = new NameValueCollection();
		for (int i = 0; i < numOptions; i++)
		{
			CUPS_OPTIONS obj = (CUPS_OPTIONS)Marshal.PtrToStructure(options, typeof(CUPS_OPTIONS));
			string name = Marshal.PtrToStringAnsi(obj.name);
			string value = Marshal.PtrToStringAnsi(obj.val);
			nameValueCollection.Add(name, value);
			options = (IntPtr)((long)options + num);
		}
		return nameValueCollection;
	}

	private static void LoadOptionList(IntPtr ppd, string option_name, NameValueCollection list, out string defoption)
	{
		IntPtr zero = IntPtr.Zero;
		int num = Marshal.SizeOf(typeof(PPD_CHOICE));
		defoption = null;
		zero = ppdFindOption(ppd, option_name);
		if (zero != IntPtr.Zero)
		{
			PPD_OPTION pPD_OPTION = (PPD_OPTION)Marshal.PtrToStructure(zero, typeof(PPD_OPTION));
			defoption = pPD_OPTION.defchoice;
			zero = pPD_OPTION.choices;
			for (int i = 0; i < pPD_OPTION.num_choices; i++)
			{
				PPD_CHOICE pPD_CHOICE = (PPD_CHOICE)Marshal.PtrToStructure(zero, typeof(PPD_CHOICE));
				list.Add(pPD_CHOICE.choice, pPD_CHOICE.text);
				zero = (IntPtr)((long)zero + num);
			}
		}
	}

	internal override void LoadPrinterResolutions(string printer, PrinterSettings settings)
	{
		IntPtr handle = OpenPrinter(printer);
		if (!(handle == IntPtr.Zero))
		{
			LoadPrinterResolutionsAndDefault(printer, settings, handle);
			ClosePrinter(ref handle);
		}
	}

	private PrinterResolution ParseResolution(string resolution)
	{
		if (string.IsNullOrEmpty(resolution))
		{
			return null;
		}
		int num = resolution.IndexOf("dpi");
		if (num == -1)
		{
			return null;
		}
		resolution = resolution.Substring(0, num);
		int num2;
		int y;
		try
		{
			if (resolution.Contains("x"))
			{
				string[] array = resolution.Split(new char[1] { 'x' });
				num2 = Convert.ToInt32(array[0]);
				y = Convert.ToInt32(array[1]);
			}
			else
			{
				num2 = Convert.ToInt32(resolution);
				y = num2;
			}
		}
		catch (Exception)
		{
			return null;
		}
		return new PrinterResolution(PrinterResolutionKind.Custom, num2, y);
	}

	private PaperSize LoadPrinterPaperSizes(IntPtr ppd_handle, PrinterSettings settings, string def_size, NameValueCollection paper_names)
	{
		PaperSize result = new PaperSize(GetPaperKind(827, 1169), "A4", 827, 1169);
		PPD_FILE pPD_FILE = (PPD_FILE)Marshal.PtrToStructure(ppd_handle, typeof(PPD_FILE));
		IntPtr intPtr = pPD_FILE.sizes;
		for (int i = 0; i < pPD_FILE.num_sizes; i++)
		{
			PPD_SIZE structure = (PPD_SIZE)Marshal.PtrToStructure(intPtr, typeof(PPD_SIZE));
			string name = paper_names[structure.name];
			float num = structure.width * 100f / 72f;
			float num2 = structure.length * 100f / 72f;
			PaperKind paperKind = GetPaperKind((int)num, (int)num2);
			PaperSize paperSize = new PaperSize(paperKind, name, (int)num, (int)num2);
			paperSize.RawKind = (int)paperKind;
			if (def_size == paperSize.Kind.ToString())
			{
				result = paperSize;
			}
			settings.paper_sizes.Add(paperSize);
			intPtr = (IntPtr)((long)intPtr + Marshal.SizeOf(structure));
		}
		return result;
	}

	private PaperSource LoadPrinterPaperSources(PrinterSettings settings, string def_source, NameValueCollection paper_sources)
	{
		PaperSource paperSource = null;
		foreach (string paper_source in paper_sources)
		{
			settings.paper_sources.Add(new PaperSource(paper_source switch
			{
				"Auto" => PaperSourceKind.AutomaticFeed, 
				"Standard" => PaperSourceKind.AutomaticFeed, 
				"Tray" => PaperSourceKind.AutomaticFeed, 
				"Envelope" => PaperSourceKind.Envelope, 
				"Manual" => PaperSourceKind.Manual, 
				_ => PaperSourceKind.Custom, 
			}, paper_sources[paper_source]));
			if (def_source == paper_source)
			{
				paperSource = settings.paper_sources[settings.paper_sources.Count - 1];
			}
		}
		if (paperSource == null && settings.paper_sources.Count > 0)
		{
			return settings.paper_sources[0];
		}
		return paperSource;
	}

	private void LoadPrinterResolutionsAndDefault(string printer, PrinterSettings settings, IntPtr ppd_handle)
	{
		if (settings.printer_resolutions == null)
		{
			settings.printer_resolutions = new PrinterSettings.PrinterResolutionCollection(new PrinterResolution[0]);
		}
		else
		{
			settings.printer_resolutions.Clear();
		}
		NameValueCollection nameValueCollection = new NameValueCollection();
		LoadOptionList(ppd_handle, "Resolution", nameValueCollection, out var defoption);
		foreach (object key in nameValueCollection.Keys)
		{
			PrinterResolution printerResolution = ParseResolution(key.ToString());
			settings.PrinterResolutions.Add(printerResolution);
		}
		PrinterResolution printerResolution2 = ParseResolution(defoption);
		if (printerResolution2 == null)
		{
			printerResolution2 = ParseResolution("300dpi");
		}
		if (nameValueCollection.Count == 0)
		{
			settings.PrinterResolutions.Add(printerResolution2);
		}
		settings.DefaultPageSettings.PrinterResolution = printerResolution2;
	}

	private static void LoadPrinters()
	{
		installed_printers.Clear();
		if (!cups_installed)
		{
			return;
		}
		IntPtr ptr = IntPtr.Zero;
		int num = 0;
		int num2 = Marshal.SizeOf(typeof(CUPS_DESTS));
		string type;
		string text2;
		string comment;
		string text = (type = (text2 = (comment = string.Empty)));
		int num3 = 0;
		try
		{
			num = OpenDests(ref ptr);
			IntPtr intPtr = ptr;
			for (int i = 0; i < num; i++)
			{
				CUPS_DESTS cUPS_DESTS = (CUPS_DESTS)Marshal.PtrToStructure(intPtr, typeof(CUPS_DESTS));
				string text3 = Marshal.PtrToStringAnsi(cUPS_DESTS.name);
				if (cUPS_DESTS.is_default == 1)
				{
					default_printer = text3;
				}
				if (text.Equals(string.Empty))
				{
					text = text3;
				}
				NameValueCollection nameValueCollection = LoadPrinterOptions(cUPS_DESTS.options, cUPS_DESTS.num_options);
				if (nameValueCollection["printer-state"] != null)
				{
					num3 = int.Parse(nameValueCollection["printer-state"]);
				}
				if (nameValueCollection["printer-comment"] != null)
				{
					comment = nameValueCollection["printer-state"];
				}
				installed_printers.Add(text3, new SysPrn.Printer(string.Empty, type, num3 switch
				{
					4 => "Printing", 
					5 => "Stopped", 
					_ => "Ready", 
				}, comment));
				intPtr = (IntPtr)((long)intPtr + num2);
			}
		}
		finally
		{
			CloseDests(ref ptr, num);
		}
		if (default_printer.Equals(string.Empty))
		{
			default_printer = text;
		}
	}

	internal override void GetPrintDialogInfo(string printer, ref string port, ref string type, ref string status, ref string comment)
	{
		int num = 0;
		int num2 = -1;
		bool flag = false;
		IntPtr ptr = IntPtr.Zero;
		int num3 = Marshal.SizeOf(typeof(CUPS_DESTS));
		if (!cups_installed)
		{
			return;
		}
		try
		{
			num = OpenDests(ref ptr);
			if (num == 0)
			{
				return;
			}
			IntPtr intPtr = ptr;
			for (int i = 0; i < num; i++)
			{
				if (Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(intPtr)).Equals(printer))
				{
					flag = true;
					break;
				}
				intPtr = (IntPtr)((long)intPtr + num3);
			}
			if (flag)
			{
				CUPS_DESTS cUPS_DESTS = (CUPS_DESTS)Marshal.PtrToStructure(intPtr, typeof(CUPS_DESTS));
				NameValueCollection nameValueCollection = LoadPrinterOptions(cUPS_DESTS.options, cUPS_DESTS.num_options);
				if (nameValueCollection["printer-state"] != null)
				{
					num2 = int.Parse(nameValueCollection["printer-state"]);
				}
				if (nameValueCollection["printer-comment"] != null)
				{
					comment = nameValueCollection["printer-state"];
				}
				switch (num2)
				{
				case 4:
					status = "Printing";
					break;
				case 5:
					status = "Stopped";
					break;
				default:
					status = "Ready";
					break;
				}
			}
		}
		finally
		{
			CloseDests(ref ptr, num);
		}
	}

	private PaperKind GetPaperKind(int width, int height)
	{
		if (width == 827 && height == 1169)
		{
			return PaperKind.A4;
		}
		if (width == 583 && height == 827)
		{
			return PaperKind.A5;
		}
		if (width == 717 && height == 1012)
		{
			return PaperKind.B5;
		}
		if (width == 693 && height == 984)
		{
			return PaperKind.B5Envelope;
		}
		if (width == 638 && height == 902)
		{
			return PaperKind.C5Envelope;
		}
		if (width == 449 && height == 638)
		{
			return PaperKind.C6Envelope;
		}
		if (width == 1700 && height == 2200)
		{
			return PaperKind.CSheet;
		}
		if (width == 433 && height == 866)
		{
			return PaperKind.DLEnvelope;
		}
		if (width == 2200 && height == 3400)
		{
			return PaperKind.DSheet;
		}
		if (width == 3400 && height == 4400)
		{
			return PaperKind.ESheet;
		}
		if (width == 725 && height == 1050)
		{
			return PaperKind.Executive;
		}
		if (width == 850 && height == 1300)
		{
			return PaperKind.Folio;
		}
		if (width == 850 && height == 1200)
		{
			return PaperKind.GermanStandardFanfold;
		}
		if (width == 1700 && height == 1100)
		{
			return PaperKind.Ledger;
		}
		if (width == 850 && height == 1400)
		{
			return PaperKind.Legal;
		}
		if (width == 927 && height == 1500)
		{
			return PaperKind.LegalExtra;
		}
		if (width == 850 && height == 1100)
		{
			return PaperKind.Letter;
		}
		if (width == 927 && height == 1200)
		{
			return PaperKind.LetterExtra;
		}
		if (width == 850 && height == 1269)
		{
			return PaperKind.LetterPlus;
		}
		if (width == 387 && height == 750)
		{
			return PaperKind.MonarchEnvelope;
		}
		if (width == 387 && height == 887)
		{
			return PaperKind.Number9Envelope;
		}
		if (width == 413 && height == 950)
		{
			return PaperKind.Number10Envelope;
		}
		if (width == 450 && height == 1037)
		{
			return PaperKind.Number11Envelope;
		}
		if (width == 475 && height == 1100)
		{
			return PaperKind.Number12Envelope;
		}
		if (width == 500 && height == 1150)
		{
			return PaperKind.Number14Envelope;
		}
		if (width == 363 && height == 650)
		{
			return PaperKind.PersonalEnvelope;
		}
		if (width == 1000 && height == 1100)
		{
			return PaperKind.Standard10x11;
		}
		if (width == 1000 && height == 1400)
		{
			return PaperKind.Standard10x14;
		}
		if (width == 1100 && height == 1700)
		{
			return PaperKind.Standard11x17;
		}
		if (width == 1200 && height == 1100)
		{
			return PaperKind.Standard12x11;
		}
		if (width == 1500 && height == 1100)
		{
			return PaperKind.Standard15x11;
		}
		if (width == 900 && height == 1100)
		{
			return PaperKind.Standard9x11;
		}
		if (width == 550 && height == 850)
		{
			return PaperKind.Statement;
		}
		if (width == 1100 && height == 1700)
		{
			return PaperKind.Tabloid;
		}
		if (width == 1487 && height == 1100)
		{
			return PaperKind.USStandardFanfold;
		}
		return PaperKind.Custom;
	}

	internal static int GetCupsOptions(PrinterSettings printer_settings, PageSettings page_settings, out IntPtr options)
	{
		options = IntPtr.Zero;
		PaperSize paperSize = page_settings.PaperSize;
		int num = paperSize.Width * 72 / 100;
		int num2 = paperSize.Height * 72 / 100;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("copies=" + printer_settings.Copies + " Collate=" + printer_settings.Collate + " ColorModel=" + (page_settings.Color ? "Color" : "Black") + " PageSize=" + $"Custom.{num}x{num2}" + " landscape=" + page_settings.Landscape);
		if (printer_settings.CanDuplex)
		{
			if (printer_settings.Duplex == Duplex.Simplex)
			{
				stringBuilder.Append(" Duplex=None");
			}
			else
			{
				stringBuilder.Append(" Duplex=DuplexNoTumble");
			}
		}
		return cupsParseOptions(stringBuilder.ToString(), 0, ref options);
	}

	internal static bool StartDoc(GraphicsPrinter gr, string doc_name, string output_file)
	{
		DOCINFO dOCINFO = (DOCINFO)doc_info[gr.Hdc];
		dOCINFO.title = doc_name;
		return true;
	}

	internal static bool EndDoc(GraphicsPrinter gr)
	{
		DOCINFO dOCINFO = (DOCINFO)doc_info[gr.Hdc];
		gr.Graphics.Dispose();
		IntPtr options;
		int cupsOptions = GetCupsOptions(dOCINFO.settings, dOCINFO.default_page_settings, out options);
		cupsPrintFile(dOCINFO.settings.PrinterName, dOCINFO.filename, dOCINFO.title, cupsOptions, options);
		cupsFreeOptions(cupsOptions, options);
		doc_info.Remove(gr.Hdc);
		if (tmpfile != null)
		{
			try
			{
				File.Delete(tmpfile);
			}
			catch
			{
			}
		}
		return true;
	}

	internal static bool StartPage(GraphicsPrinter gr)
	{
		return true;
	}

	internal static bool EndPage(GraphicsPrinter gr)
	{
		GdipGetPostScriptSavePage(gr.Hdc);
		return true;
	}

	internal static IntPtr CreateGraphicsContext(PrinterSettings settings, PageSettings default_page_settings)
	{
		IntPtr graphics = IntPtr.Zero;
		string filename;
		if (!settings.PrintToFile)
		{
			StringBuilder stringBuilder = new StringBuilder(1024);
			int capacity = stringBuilder.Capacity;
			cupsTempFd(stringBuilder, capacity);
			filename = (tmpfile = stringBuilder.ToString());
		}
		else
		{
			filename = settings.PrintFileName;
		}
		PaperSize paperSize = default_page_settings.PaperSize;
		int num;
		int num2;
		if (default_page_settings.Landscape)
		{
			num = paperSize.Height;
			num2 = paperSize.Width;
		}
		else
		{
			num = paperSize.Width;
			num2 = paperSize.Height;
		}
		GdipGetPostScriptGraphicsContext(filename, num * 72 / 100, num2 * 72 / 100, default_page_settings.PrinterResolution.X, default_page_settings.PrinterResolution.Y, ref graphics);
		DOCINFO dOCINFO = new DOCINFO
		{
			filename = filename,
			settings = settings,
			default_page_settings = default_page_settings
		};
		doc_info.Add(graphics, dOCINFO);
		return graphics;
	}

	[DllImport("libcups", CharSet = CharSet.Ansi)]
	private static extern int cupsGetDests(ref IntPtr dests);

	[DllImport("libcups")]
	private static extern void cupsFreeDests(int num_dests, IntPtr dests);

	[DllImport("libcups", CharSet = CharSet.Ansi)]
	private static extern IntPtr cupsTempFd(StringBuilder sb, int len);

	[DllImport("libcups", CharSet = CharSet.Ansi)]
	private static extern IntPtr cupsGetDefault();

	[DllImport("libcups", CharSet = CharSet.Ansi)]
	private static extern int cupsPrintFile(string printer, string filename, string title, int num_options, IntPtr options);

	[DllImport("libcups", CharSet = CharSet.Ansi)]
	private static extern IntPtr cupsGetPPD(string printer);

	[DllImport("libcups", CharSet = CharSet.Ansi)]
	private static extern IntPtr ppdOpenFile(string filename);

	[DllImport("libcups", CharSet = CharSet.Ansi)]
	private static extern IntPtr ppdFindOption(IntPtr ppd_file, string keyword);

	[DllImport("libcups")]
	private static extern void ppdClose(IntPtr ppd);

	[DllImport("libcups", CharSet = CharSet.Ansi)]
	private static extern int cupsParseOptions(string arg, int number_of_options, ref IntPtr options);

	[DllImport("libcups")]
	private static extern void cupsFreeOptions(int number_options, IntPtr options);

	[DllImport("gdiplus.dll", CharSet = CharSet.Ansi)]
	private static extern int GdipGetPostScriptGraphicsContext(string filename, int with, int height, double dpix, double dpiy, ref IntPtr graphics);

	[DllImport("gdiplus.dll")]
	private static extern int GdipGetPostScriptSavePage(IntPtr graphics);
}
