using System.Runtime.CompilerServices;
using System.Security;

namespace System;

[FriendAccessAllowed]
internal class CLRConfig
{
	[FriendAccessAllowed]
	[SecurityCritical]
	[SuppressUnmanagedCodeSecurity]
	internal static bool CheckLegacyManagedDeflateStream()
	{
		return false;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SuppressUnmanagedCodeSecurity]
	[SecurityCritical]
	internal static extern bool CheckThrowUnobservedTaskExceptions();
}
