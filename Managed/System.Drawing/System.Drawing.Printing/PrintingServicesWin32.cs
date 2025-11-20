using System.Runtime.InteropServices;
using System.Text;

namespace System.Drawing.Printing;

internal class PrintingServicesWin32 : PrintingServices
{
	internal struct PRINTER_INFO
	{
		public IntPtr pServerName;

		public IntPtr pPrinterName;

		public IntPtr pShareName;

		public IntPtr pPortName;

		public IntPtr pDriverName;

		public IntPtr pComment;

		public IntPtr pLocation;

		public IntPtr pDevMode;

		public IntPtr pSepFile;

		public IntPtr pPrintProcessor;

		public IntPtr pDatatype;

		public IntPtr pParameters;

		public IntPtr pSecurityDescriptor;

		public uint Attributes;

		public uint Priority;

		public uint DefaultPriority;

		public uint StartTime;

		public uint UntilTime;

		public uint Status;

		public uint cJobs;

		public uint AveragePPM;
	}

	internal struct DOCINFO
	{
		public int cbSize;

		public IntPtr lpszDocName;

		public IntPtr lpszOutput;

		public IntPtr lpszDatatype;

		public int fwType;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct DEVMODE
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string dmDeviceName;

		public short dmSpecVersion;

		public short dmDriverVersion;

		public short dmSize;

		public short dmDriverExtra;

		public int dmFields;

		public short dmOrientation;

		public short dmPaperSize;

		public short dmPaperLength;

		public short dmPaperWidth;

		public short dmScale;

		public short dmCopies;

		public short dmDefaultSource;

		public short dmPrintQuality;

		public short dmColor;

		public short dmDuplex;

		public short dmYResolution;

		public short dmTTOption;

		public short dmCollate;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string dmFormName;

		public short dmLogPixels;

		public short dmBitsPerPel;

		public int dmPelsWidth;

		public int dmPelsHeight;

		public int dmDisplayFlags;

		public int dmDisplayFrequency;

		public int dmICMMethod;

		public int dmICMIntent;

		public int dmMediaType;

		public int dmDitherType;

		public int dmReserved1;

		public int dmReserved2;

		public int dmPanningWidth;

		public int dmPanningHeight;
	}

	internal enum DCCapabilities : short
	{
		DC_FIELDS = 1,
		DC_PAPERS,
		DC_PAPERSIZE,
		DC_MINEXTENT,
		DC_MAXEXTENT,
		DC_BINS,
		DC_DUPLEX,
		DC_SIZE,
		DC_EXTRA,
		DC_VERSION,
		DC_DRIVER,
		DC_BINNAMES,
		DC_ENUMRESOLUTIONS,
		DC_FILEDEPENDENCIES,
		DC_TRUETYPE,
		DC_PAPERNAMES,
		DC_ORIENTATION,
		DC_COPIES,
		DC_BINADJUST,
		DC_EMF_COMPLIANT,
		DC_DATATYPE_PRODUCED,
		DC_COLLATE,
		DC_MANUFACTURER,
		DC_MODEL,
		DC_PERSONALITY,
		DC_PRINTRATE,
		DC_PRINTRATEUNIT,
		DC_PRINTERMEM,
		DC_MEDIAREADY,
		DC_STAPLE,
		DC_PRINTRATEPPM,
		DC_COLORDEVICE,
		DC_NUP
	}

	[Flags]
	internal enum PrinterStatus : uint
	{
		PS_PAUSED = 1u,
		PS_ERROR = 2u,
		PS_PENDING_DELETION = 4u,
		PS_PAPER_JAM = 8u,
		PS_PAPER_OUT = 0x10u,
		PS_MANUAL_FEED = 0x20u,
		PS_PAPER_PROBLEM = 0x40u,
		PS_OFFLINE = 0x80u,
		PS_IO_ACTIVE = 0x100u,
		PS_BUSY = 0x200u,
		PS_PRINTING = 0x400u,
		PS_OUTPUT_BIN_FULL = 0x800u,
		PS_NOT_AVAILABLE = 0x1000u,
		PS_WAITING = 0x2000u,
		PS_PROCESSING = 0x4000u,
		PS_INITIALIZING = 0x8000u,
		PS_WARMING_UP = 0x10000u,
		PS_TONER_LOW = 0x20000u,
		PS_NO_TONER = 0x40000u,
		PS_PAGE_PUNT = 0x80000u,
		PS_USER_INTERVENTION = 0x100000u,
		PS_OUT_OF_MEMORY = 0x200000u,
		PS_DOOR_OPEN = 0x400000u,
		PS_SERVER_UNKNOWN = 0x800000u,
		PS_POWER_SAVE = 0x1000000u
	}

	internal enum DevCapabilities
	{
		TECHNOLOGY = 2
	}

	internal enum PrinterType
	{
		DT_PLOTTER,
		DT_RASDIPLAY,
		DT_RASPRINTER,
		DT_RASCAMERA,
		DT_CHARSTREAM,
		DT_METAFILE,
		DT_DISPFILE
	}

	[Flags]
	internal enum EnumPrinters : uint
	{
		PRINTER_ENUM_DEFAULT = 1u,
		PRINTER_ENUM_LOCAL = 2u,
		PRINTER_ENUM_CONNECTIONS = 4u,
		PRINTER_ENUM_FAVORITE = 4u,
		PRINTER_ENUM_NAME = 8u,
		PRINTER_ENUM_REMOTE = 0x10u,
		PRINTER_ENUM_SHARED = 0x20u,
		PRINTER_ENUM_NETWORK = 0x40u
	}

	private bool is_printer_valid;

	internal override string DefaultPrinter
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(1024);
			int bufferSize = stringBuilder.Capacity;
			if (Win32GetDefaultPrinter(stringBuilder, ref bufferSize) > 0 && IsPrinterValid(stringBuilder.ToString()))
			{
				return stringBuilder.ToString();
			}
			return string.Empty;
		}
	}

	internal static PrinterSettings.StringCollection InstalledPrinters
	{
		get
		{
			PrinterSettings.StringCollection stringCollection = new PrinterSettings.StringCollection(new string[0]);
			uint pcbNeeded = 0u;
			uint pcReturned = 0u;
			Win32EnumPrinters(6, null, 2u, IntPtr.Zero, 0u, ref pcbNeeded, ref pcReturned);
			if (pcbNeeded == 0)
			{
				return stringCollection;
			}
			IntPtr intPtr;
			IntPtr ptr = (intPtr = Marshal.AllocHGlobal((int)pcbNeeded));
			try
			{
				Win32EnumPrinters(6, null, 2u, intPtr, pcbNeeded, ref pcbNeeded, ref pcReturned);
				for (int i = 0; i < pcReturned; i++)
				{
					PRINTER_INFO structure = (PRINTER_INFO)Marshal.PtrToStructure(ptr, typeof(PRINTER_INFO));
					string value = Marshal.PtrToStringUni(structure.pPrinterName);
					stringCollection.Add(value);
					ptr = new IntPtr(ptr.ToInt64() + Marshal.SizeOf(structure));
				}
				return stringCollection;
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
	}

	internal PrintingServicesWin32()
	{
	}

	internal override bool IsPrinterValid(string printer)
	{
		if ((printer == null) | (printer == string.Empty))
		{
			return false;
		}
		int num = Win32DocumentProperties(IntPtr.Zero, IntPtr.Zero, printer, IntPtr.Zero, IntPtr.Zero, 0);
		is_printer_valid = num > 0;
		return is_printer_valid;
	}

	internal override void LoadPrinterSettings(string printer, PrinterSettings settings)
	{
		IntPtr phPrinter = IntPtr.Zero;
		IntPtr intPtr = IntPtr.Zero;
		settings.maximum_copies = Win32DeviceCapabilities(printer, null, DCCapabilities.DC_COPIES, IntPtr.Zero, IntPtr.Zero);
		int num = Win32DeviceCapabilities(printer, null, DCCapabilities.DC_DUPLEX, IntPtr.Zero, IntPtr.Zero);
		settings.can_duplex = num == 1;
		num = Win32DeviceCapabilities(printer, null, DCCapabilities.DC_COLORDEVICE, IntPtr.Zero, IntPtr.Zero);
		settings.supports_color = num == 1;
		num = Win32DeviceCapabilities(printer, null, DCCapabilities.DC_ORIENTATION, IntPtr.Zero, IntPtr.Zero);
		if (num != -1)
		{
			settings.landscape_angle = num;
		}
		_ = IntPtr.Zero;
		IntPtr hDc = Win32CreateIC(null, printer, null, IntPtr.Zero);
		num = Win32GetDeviceCaps(hDc, 2);
		settings.is_plotter = num == 0;
		Win32DeleteDC(hDc);
		try
		{
			Win32OpenPrinter(printer, out phPrinter, IntPtr.Zero);
			num = Win32DocumentProperties(IntPtr.Zero, phPrinter, null, IntPtr.Zero, IntPtr.Zero, 0);
			if (num < 0)
			{
				return;
			}
			intPtr = Marshal.AllocHGlobal(num);
			num = Win32DocumentProperties(IntPtr.Zero, phPrinter, null, intPtr, IntPtr.Zero, 2);
			DEVMODE dEVMODE = (DEVMODE)Marshal.PtrToStructure(intPtr, typeof(DEVMODE));
			LoadPrinterPaperSizes(printer, settings);
			foreach (PaperSize paperSize in settings.PaperSizes)
			{
				if (paperSize.Kind == (PaperKind)dEVMODE.dmPaperSize)
				{
					settings.DefaultPageSettings.PaperSize = paperSize;
					break;
				}
			}
			LoadPrinterPaperSources(printer, settings);
			foreach (PaperSource paperSource in settings.PaperSources)
			{
				if (paperSource.Kind == (PaperSourceKind)dEVMODE.dmDefaultSource)
				{
					settings.DefaultPageSettings.PaperSource = paperSource;
					break;
				}
			}
		}
		finally
		{
			Win32ClosePrinter(phPrinter);
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
	}

	internal override void LoadPrinterResolutions(string printer, PrinterSettings settings)
	{
		IntPtr zero = IntPtr.Zero;
		settings.PrinterResolutions.Clear();
		LoadDefaultResolutions(settings.PrinterResolutions);
		int num = Win32DeviceCapabilities(printer, null, DCCapabilities.DC_ENUMRESOLUTIONS, IntPtr.Zero, IntPtr.Zero);
		if (num == -1)
		{
			return;
		}
		IntPtr ptr = (zero = Marshal.AllocHGlobal(num * 2 * Marshal.SizeOf(zero)));
		num = Win32DeviceCapabilities(printer, null, DCCapabilities.DC_ENUMRESOLUTIONS, zero, IntPtr.Zero);
		if (num != -1)
		{
			for (int i = 0; i < num; i++)
			{
				int num2 = Marshal.ReadInt32(ptr);
				ptr = new IntPtr(ptr.ToInt64() + Marshal.SizeOf(num2));
				int num3 = Marshal.ReadInt32(ptr);
				ptr = new IntPtr(ptr.ToInt64() + Marshal.SizeOf(num3));
				settings.PrinterResolutions.Add(new PrinterResolution(PrinterResolutionKind.Custom, num2, num3));
			}
		}
		Marshal.FreeHGlobal(zero);
	}

	private void LoadPrinterPaperSizes(string printer, PrinterSettings settings)
	{
		IntPtr intPtr = IntPtr.Zero;
		IntPtr intPtr2 = IntPtr.Zero;
		IntPtr intPtr3 = IntPtr.Zero;
		if (settings.PaperSizes == null)
		{
			settings.paper_sizes = new PrinterSettings.PaperSizeCollection(new PaperSize[0]);
		}
		else
		{
			settings.PaperSizes.Clear();
		}
		int num = Win32DeviceCapabilities(printer, null, DCCapabilities.DC_PAPERSIZE, IntPtr.Zero, IntPtr.Zero);
		if (num == -1)
		{
			return;
		}
		try
		{
			IntPtr ptr = (intPtr2 = Marshal.AllocHGlobal(num * 2 * 4));
			IntPtr ptr2 = (intPtr = Marshal.AllocHGlobal(num * 64 * 2));
			IntPtr ptr3 = (intPtr3 = Marshal.AllocHGlobal(num * 2));
			int num2 = Win32DeviceCapabilities(printer, null, DCCapabilities.DC_PAPERSIZE, intPtr2, IntPtr.Zero);
			if (num2 != -1)
			{
				num2 = Win32DeviceCapabilities(printer, null, DCCapabilities.DC_PAPERS, intPtr3, IntPtr.Zero);
				num2 = Win32DeviceCapabilities(printer, null, DCCapabilities.DC_PAPERNAMES, intPtr, IntPtr.Zero);
				for (int i = 0; i < num2; i++)
				{
					int value = Marshal.ReadInt32(ptr, i * 8);
					int value2 = Marshal.ReadInt32(ptr, i * 8 + 4);
					value = PrinterUnitConvert.Convert(value, PrinterUnit.TenthsOfAMillimeter, PrinterUnit.Display);
					value2 = PrinterUnitConvert.Convert(value2, PrinterUnit.TenthsOfAMillimeter, PrinterUnit.Display);
					string name = Marshal.PtrToStringUni(ptr2);
					ptr2 = new IntPtr(ptr2.ToInt64() + 128);
					PaperKind rawKind = (PaperKind)Marshal.ReadInt16(ptr3);
					ptr3 = new IntPtr(ptr3.ToInt64() + 2);
					PaperSize paperSize = new PaperSize(name, value, value2);
					paperSize.RawKind = (int)rawKind;
					settings.PaperSizes.Add(paperSize);
				}
			}
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
			if (intPtr2 != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr2);
			}
			if (intPtr3 != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr3);
			}
		}
	}

	internal static bool StartDoc(GraphicsPrinter gr, string doc_name, string output_file)
	{
		DOCINFO lpdi = default(DOCINFO);
		lpdi.cbSize = Marshal.SizeOf(lpdi);
		lpdi.lpszDocName = Marshal.StringToHGlobalUni(doc_name);
		lpdi.lpszOutput = IntPtr.Zero;
		lpdi.lpszDatatype = IntPtr.Zero;
		lpdi.fwType = 0;
		int num = Win32StartDoc(gr.Hdc, ref lpdi);
		Marshal.FreeHGlobal(lpdi.lpszDocName);
		if (num <= 0)
		{
			return false;
		}
		return true;
	}

	private void LoadPrinterPaperSources(string printer, PrinterSettings settings)
	{
		IntPtr intPtr = IntPtr.Zero;
		IntPtr intPtr2 = IntPtr.Zero;
		if (settings.PaperSources == null)
		{
			settings.paper_sources = new PrinterSettings.PaperSourceCollection(new PaperSource[0]);
		}
		else
		{
			settings.PaperSources.Clear();
		}
		int num = Win32DeviceCapabilities(printer, null, DCCapabilities.DC_BINNAMES, IntPtr.Zero, IntPtr.Zero);
		if (num == -1)
		{
			return;
		}
		try
		{
			IntPtr ptr = (intPtr = Marshal.AllocHGlobal(num * 2 * 24));
			IntPtr ptr2 = (intPtr2 = Marshal.AllocHGlobal(num * 2));
			int num2 = Win32DeviceCapabilities(printer, null, DCCapabilities.DC_BINNAMES, intPtr, IntPtr.Zero);
			if (num2 != -1)
			{
				num2 = Win32DeviceCapabilities(printer, null, DCCapabilities.DC_BINS, intPtr2, IntPtr.Zero);
				for (int i = 0; i < num2; i++)
				{
					string name = Marshal.PtrToStringUni(ptr);
					PaperSourceKind kind = (PaperSourceKind)Marshal.ReadInt16(ptr2);
					settings.PaperSources.Add(new PaperSource(kind, name));
					ptr = new IntPtr(ptr.ToInt64() + 48);
					ptr2 = new IntPtr(ptr2.ToInt64() + 2);
				}
			}
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
			if (intPtr2 != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr2);
			}
		}
	}

	internal static bool StartPage(GraphicsPrinter gr)
	{
		if (Win32StartPage(gr.Hdc) <= 0)
		{
			return false;
		}
		return true;
	}

	internal static bool EndPage(GraphicsPrinter gr)
	{
		if (Win32EndPage(gr.Hdc) <= 0)
		{
			return false;
		}
		return true;
	}

	internal static bool EndDoc(GraphicsPrinter gr)
	{
		int num = Win32EndDoc(gr.Hdc);
		Win32DeleteDC(gr.Hdc);
		gr.Graphics.Dispose();
		if (num <= 0)
		{
			return false;
		}
		return true;
	}

	internal static IntPtr CreateGraphicsContext(PrinterSettings settings, PageSettings default_page_settings)
	{
		_ = IntPtr.Zero;
		return Win32CreateDC(null, settings.PrinterName, null, IntPtr.Zero);
	}

	internal override void GetPrintDialogInfo(string printer, ref string port, ref string type, ref string status, ref string comment)
	{
		PRINTER_INFO pRINTER_INFO = default(PRINTER_INFO);
		int dwNeeded = 0;
		Win32OpenPrinter(printer, out var phPrinter, IntPtr.Zero);
		if (!(phPrinter == IntPtr.Zero))
		{
			Win32GetPrinter(phPrinter, 2, IntPtr.Zero, 0, ref dwNeeded);
			IntPtr intPtr = Marshal.AllocHGlobal(dwNeeded);
			Win32GetPrinter(phPrinter, 2, intPtr, dwNeeded, ref dwNeeded);
			pRINTER_INFO = (PRINTER_INFO)Marshal.PtrToStructure(intPtr, typeof(PRINTER_INFO));
			Marshal.FreeHGlobal(intPtr);
			port = Marshal.PtrToStringUni(pRINTER_INFO.pPortName);
			comment = Marshal.PtrToStringUni(pRINTER_INFO.pComment);
			type = Marshal.PtrToStringUni(pRINTER_INFO.pDriverName);
			status = GetPrinterStatusMsg(pRINTER_INFO.Status);
			Win32ClosePrinter(phPrinter);
		}
	}

	private string GetPrinterStatusMsg(uint status)
	{
		string text = string.Empty;
		if (status == 0)
		{
			return "Ready";
		}
		if ((status & 1) != 0)
		{
			text += "Paused; ";
		}
		if ((status & 2) != 0)
		{
			text += "Error; ";
		}
		if ((status & 4) != 0)
		{
			text += "Pending deletion; ";
		}
		if ((status & 8) != 0)
		{
			text += "Paper jam; ";
		}
		if ((status & 0x10) != 0)
		{
			text += "Paper out; ";
		}
		if ((status & 0x20) != 0)
		{
			text += "Manual feed; ";
		}
		if ((status & 0x40) != 0)
		{
			text += "Paper problem; ";
		}
		if ((status & 0x80) != 0)
		{
			text += "Offline; ";
		}
		if ((status & 0x100) != 0)
		{
			text += "I/O active; ";
		}
		if ((status & 0x200) != 0)
		{
			text += "Busy; ";
		}
		if ((status & 0x400) != 0)
		{
			text += "Printing; ";
		}
		if ((status & 0x800) != 0)
		{
			text += "Output bin full; ";
		}
		if ((status & 0x1000) != 0)
		{
			text += "Not available; ";
		}
		if ((status & 0x2000) != 0)
		{
			text += "Waiting; ";
		}
		if ((status & 0x4000) != 0)
		{
			text += "Processing; ";
		}
		if ((status & 0x8000) != 0)
		{
			text += "Initializing; ";
		}
		if ((status & 0x10000) != 0)
		{
			text += "Warming up; ";
		}
		if ((status & 0x20000) != 0)
		{
			text += "Toner low; ";
		}
		if ((status & 0x40000) != 0)
		{
			text += "No toner; ";
		}
		if ((status & 0x80000) != 0)
		{
			text += "Page punt; ";
		}
		if ((status & 0x100000) != 0)
		{
			text += "User intervention; ";
		}
		if ((status & 0x200000) != 0)
		{
			text += "Out of memory; ";
		}
		if ((status & 0x400000) != 0)
		{
			text += "Door open; ";
		}
		if ((status & 0x800000) != 0)
		{
			text += "Server unkown; ";
		}
		if ((status & 0x1000000) != 0)
		{
			text += "Power save; ";
		}
		return text;
	}

	[DllImport("winspool.drv", CharSet = CharSet.Unicode, EntryPoint = "OpenPrinter", SetLastError = true)]
	private static extern int Win32OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

	[DllImport("winspool.drv", CharSet = CharSet.Unicode, EntryPoint = "GetPrinter", SetLastError = true)]
	private static extern int Win32GetPrinter(IntPtr hPrinter, int level, IntPtr dwBuf, int size, ref int dwNeeded);

	[DllImport("winspool.drv", CharSet = CharSet.Unicode, EntryPoint = "ClosePrinter", SetLastError = true)]
	private static extern int Win32ClosePrinter(IntPtr hPrinter);

	[DllImport("winspool.drv", CharSet = CharSet.Unicode, EntryPoint = "DeviceCapabilities", SetLastError = true)]
	private static extern int Win32DeviceCapabilities(string device, string port, DCCapabilities cap, IntPtr outputBuffer, IntPtr deviceMode);

	[DllImport("winspool.drv", CharSet = CharSet.Unicode, EntryPoint = "EnumPrinters", SetLastError = true)]
	private static extern int Win32EnumPrinters(int Flags, string Name, uint Level, IntPtr pPrinterEnum, uint cbBuf, ref uint pcbNeeded, ref uint pcReturned);

	[DllImport("winspool.drv", CharSet = CharSet.Unicode, EntryPoint = "GetDefaultPrinter", SetLastError = true)]
	private static extern int Win32GetDefaultPrinter(StringBuilder buffer, ref int bufferSize);

	[DllImport("winspool.drv", CharSet = CharSet.Unicode, EntryPoint = "DocumentProperties", SetLastError = true)]
	private static extern int Win32DocumentProperties(IntPtr hwnd, IntPtr hPrinter, string pDeviceName, IntPtr pDevModeOutput, IntPtr pDevModeInput, int fMode);

	[DllImport("gdi32.dll", EntryPoint = "CreateDC")]
	private static extern IntPtr Win32CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

	[DllImport("gdi32.dll", EntryPoint = "CreateIC")]
	private static extern IntPtr Win32CreateIC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

	[DllImport("gdi32.dll", CharSet = CharSet.Unicode, EntryPoint = "StartDoc")]
	private static extern int Win32StartDoc(IntPtr hdc, [In] ref DOCINFO lpdi);

	[DllImport("gdi32.dll", EntryPoint = "StartPage")]
	private static extern int Win32StartPage(IntPtr hDC);

	[DllImport("gdi32.dll", EntryPoint = "EndPage")]
	private static extern int Win32EndPage(IntPtr hdc);

	[DllImport("gdi32.dll", EntryPoint = "EndDoc")]
	private static extern int Win32EndDoc(IntPtr hdc);

	[DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
	public static extern IntPtr Win32DeleteDC(IntPtr hDc);

	[DllImport("gdi32.dll", EntryPoint = "GetDeviceCaps")]
	public static extern int Win32GetDeviceCaps(IntPtr hDc, int index);
}
