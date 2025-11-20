using System.Runtime.InteropServices;

namespace System.Drawing.Internal;

[StructLayout(LayoutKind.Sequential)]
internal class GPPOINT
{
	internal int X;

	internal int Y;

	internal GPPOINT()
	{
	}

	internal GPPOINT(Point pt)
	{
		X = pt.X;
		Y = pt.Y;
	}
}
