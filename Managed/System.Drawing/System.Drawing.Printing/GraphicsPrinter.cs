namespace System.Drawing.Printing;

internal class GraphicsPrinter
{
	private Graphics graphics;

	private IntPtr hDC;

	internal Graphics Graphics
	{
		get
		{
			return graphics;
		}
		set
		{
			graphics = value;
		}
	}

	internal IntPtr Hdc => hDC;

	internal GraphicsPrinter(Graphics gr, IntPtr dc)
	{
		graphics = gr;
		hDC = dc;
	}
}
