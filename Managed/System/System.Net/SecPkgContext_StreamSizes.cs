using System.Runtime.InteropServices;

namespace System.Net;

[StructLayout(LayoutKind.Sequential)]
internal class SecPkgContext_StreamSizes
{
	public int cbHeader;

	public int cbTrailer;

	public int cbMaximumMessage;

	public int cBuffers;

	public int cbBlockSize;

	public static readonly int SizeOf = Marshal.SizeOf<SecPkgContext_StreamSizes>();

	internal unsafe SecPkgContext_StreamSizes(byte[] memory)
	{
		fixed (byte* ptr = memory)
		{
			void* value = ptr;
			IntPtr ptr2 = new IntPtr(value);
			try
			{
				cbHeader = (int)checked((uint)Marshal.ReadInt32(ptr2));
				cbTrailer = (int)checked((uint)Marshal.ReadInt32(ptr2, 4));
				cbMaximumMessage = (int)checked((uint)Marshal.ReadInt32(ptr2, 8));
				cBuffers = (int)checked((uint)Marshal.ReadInt32(ptr2, 12));
				cbBlockSize = (int)checked((uint)Marshal.ReadInt32(ptr2, 16));
			}
			catch (OverflowException)
			{
				NetEventSource.Fail(this, "Negative size.", ".ctor");
				throw;
			}
		}
	}
}
