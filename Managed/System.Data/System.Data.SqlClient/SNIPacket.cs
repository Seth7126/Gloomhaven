using System.Runtime.InteropServices;

namespace System.Data.SqlClient;

internal sealed class SNIPacket : SafeHandle
{
	public override bool IsInvalid => IntPtr.Zero == handle;

	internal SNIPacket(SafeHandle sniHandle)
		: base(IntPtr.Zero, ownsHandle: true)
	{
		SNINativeMethodWrapper.SNIPacketAllocate(sniHandle, SNINativeMethodWrapper.IOType.WRITE, ref handle);
		if (IntPtr.Zero == handle)
		{
			throw SQL.SNIPacketAllocationFailure();
		}
	}

	protected override bool ReleaseHandle()
	{
		IntPtr intPtr = handle;
		handle = IntPtr.Zero;
		if (IntPtr.Zero != intPtr)
		{
			SNINativeMethodWrapper.SNIPacketRelease(intPtr);
		}
		return true;
	}
}
