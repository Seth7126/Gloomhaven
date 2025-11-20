using System.Runtime.InteropServices;

namespace System.Data.SqlClient;

internal sealed class SNIHandle : SafeHandle
{
	private readonly uint _status = uint.MaxValue;

	private readonly bool _fSync;

	public override bool IsInvalid => IntPtr.Zero == handle;

	internal uint Status => _status;

	internal SNIHandle(SNINativeMethodWrapper.ConsumerInfo myInfo, string serverName, byte[] spnBuffer, bool ignoreSniOpenTimeout, int timeout, out byte[] instanceName, bool flushCache, bool fSync, bool fParallel)
		: base(IntPtr.Zero, ownsHandle: true)
	{
		try
		{
		}
		finally
		{
			_fSync = fSync;
			instanceName = new byte[256];
			if (ignoreSniOpenTimeout)
			{
				timeout = -1;
			}
			_status = SNINativeMethodWrapper.SNIOpenSyncEx(myInfo, serverName, ref handle, spnBuffer, instanceName, flushCache, fSync, timeout, fParallel);
		}
	}

	internal SNIHandle(SNINativeMethodWrapper.ConsumerInfo myInfo, SNIHandle parent)
		: base(IntPtr.Zero, ownsHandle: true)
	{
		try
		{
		}
		finally
		{
			_status = SNINativeMethodWrapper.SNIOpenMarsSession(myInfo, parent, ref handle, parent._fSync);
		}
	}

	protected override bool ReleaseHandle()
	{
		IntPtr intPtr = handle;
		handle = IntPtr.Zero;
		if (IntPtr.Zero != intPtr && SNINativeMethodWrapper.SNIClose(intPtr) != 0)
		{
			return false;
		}
		return true;
	}
}
