using System.ComponentModel;

namespace System.Drawing.Printing;

/// <summary>Provides data for the <see cref="E:System.Drawing.Printing.PrintDocument.BeginPrint" /> and <see cref="E:System.Drawing.Printing.PrintDocument.EndPrint" /> events.</summary>
public class PrintEventArgs : CancelEventArgs
{
	private GraphicsPrinter graphics_context;

	private PrintAction action;

	/// <summary>Returns <see cref="F:System.Drawing.Printing.PrintAction.PrintToFile" /> in all cases.</summary>
	/// <returns>
	///   <see cref="F:System.Drawing.Printing.PrintAction.PrintToFile" /> in all cases.</returns>
	public PrintAction PrintAction => action;

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

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PrintEventArgs" /> class.</summary>
	public PrintEventArgs()
	{
	}

	internal PrintEventArgs(PrintAction action)
	{
		this.action = action;
	}
}
