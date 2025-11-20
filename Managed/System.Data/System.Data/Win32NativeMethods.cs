using System.Data.SqlClient;
using System.Runtime.InteropServices;

namespace System.Data;

internal static class Win32NativeMethods
{
	internal static bool IsTokenRestrictedWrapper(IntPtr token)
	{
		bool isRestricted;
		uint num = SNINativeMethodWrapper.UnmanagedIsTokenRestricted(token, out isRestricted);
		if (num != 0)
		{
			Marshal.ThrowExceptionForHR((int)num);
		}
		return isRestricted;
	}
}
