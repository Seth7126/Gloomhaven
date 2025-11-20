using System;

namespace Microsoft.Win32.SafeHandles;

/// <summary>Represents a safe handle to the Windows registry.</summary>
public sealed class SafeRegistryHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	protected override bool ReleaseHandle()
	{
		return Interop.Advapi32.RegCloseKey(handle) == 0;
	}

	internal SafeRegistryHandle()
		: base(ownsHandle: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.SafeHandles.SafeRegistryHandle" /> class. </summary>
	/// <param name="preexistingHandle">An object that represents the pre-existing handle to use.</param>
	/// <param name="ownsHandle">true to reliably release the handle during the finalization phase; false to prevent reliable release.</param>
	public SafeRegistryHandle(IntPtr preexistingHandle, bool ownsHandle)
		: base(ownsHandle)
	{
		SetHandle(preexistingHandle);
	}
}
