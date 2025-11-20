namespace System.Drawing.Printing;

internal class SysPrn
{
	internal class Printer
	{
		public readonly string Comment;

		public readonly string Port;

		public readonly string Type;

		public readonly string Status;

		public PrinterSettings Settings;

		public Printer(string port, string type, string status, string comment)
		{
			Port = port;
			Type = type;
			Status = status;
			Comment = comment;
		}
	}

	private static GlobalPrintingServices global_printing_services;

	private static bool is_unix;

	internal static GlobalPrintingServices GlobalService
	{
		get
		{
			if (global_printing_services == null)
			{
				if (is_unix)
				{
					global_printing_services = new GlobalPrintingServicesUnix();
				}
				else
				{
					global_printing_services = new GlobalPrintingServicesWin32();
				}
			}
			return global_printing_services;
		}
	}

	static SysPrn()
	{
		is_unix = GDIPlus.RunningOnUnix();
	}

	internal static PrintingServices CreatePrintingService()
	{
		if (is_unix)
		{
			return new PrintingServicesUnix();
		}
		return new PrintingServicesWin32();
	}

	internal static void GetPrintDialogInfo(string printer, ref string port, ref string type, ref string status, ref string comment)
	{
		CreatePrintingService().GetPrintDialogInfo(printer, ref port, ref type, ref status, ref comment);
	}
}
