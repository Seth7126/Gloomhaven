using System.Runtime.InteropServices;

namespace System.Net;

[StructLayout(LayoutKind.Sequential)]
internal class SecPkgContext_Sizes
{
	public readonly int cbMaxToken;

	public readonly int cbMaxSignature;

	public readonly int cbBlockSize;

	public readonly int cbSecurityTrailer;

	public static readonly int SizeOf = Marshal.SizeOf<SecPkgContext_Sizes>();

	internal unsafe SecPkgContext_Sizes(byte[] memory)
	{
		fixed (byte* ptr = memory)
		{
			void* value = ptr;
			IntPtr ptr2 = new IntPtr(value);
			try
			{
				cbMaxToken = (int)checked((uint)Marshal.ReadInt32(ptr2));
				cbMaxSignature = (int)checked((uint)Marshal.ReadInt32(ptr2, 4));
				cbBlockSize = (int)checked((uint)Marshal.ReadInt32(ptr2, 8));
				cbSecurityTrailer = (int)checked((uint)Marshal.ReadInt32(ptr2, 12));
			}
			catch (OverflowException)
			{
				NetEventSource.Fail(this, "Negative size.", ".ctor");
				throw;
			}
		}
	}
}
