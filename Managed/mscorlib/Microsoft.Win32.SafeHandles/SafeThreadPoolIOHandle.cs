using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.SafeHandles;

internal class SafeThreadPoolIOHandle : SafeHandle
{
	public override bool IsInvalid => handle == IntPtr.Zero;

	static SafeThreadPoolIOHandle()
	{
		if (!Environment.IsRunningOnWindows)
		{
			throw new PlatformNotSupportedException();
		}
	}

	private SafeThreadPoolIOHandle()
		: base(IntPtr.Zero, ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		Interop.mincore.CloseThreadpoolIo(handle);
		return true;
	}
}
