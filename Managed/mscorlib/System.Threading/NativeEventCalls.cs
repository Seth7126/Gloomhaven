using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using Microsoft.Win32.SafeHandles;

namespace System.Threading;

internal static class NativeEventCalls
{
	public unsafe static IntPtr CreateEvent_internal(bool manual, bool initial, string name, out int errorCode)
	{
		fixed (char* name2 = name)
		{
			return CreateEvent_icall(manual, initial, name2, name?.Length ?? 0, out errorCode);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern IntPtr CreateEvent_icall(bool manual, bool initial, char* name, int name_length, out int errorCode);

	public static bool SetEvent(SafeWaitHandle handle)
	{
		bool success = false;
		try
		{
			handle.DangerousAddRef(ref success);
			return SetEvent_internal(handle.DangerousGetHandle());
		}
		finally
		{
			if (success)
			{
				handle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool SetEvent_internal(IntPtr handle);

	public static bool ResetEvent(SafeWaitHandle handle)
	{
		bool success = false;
		try
		{
			handle.DangerousAddRef(ref success);
			return ResetEvent_internal(handle.DangerousGetHandle());
		}
		finally
		{
			if (success)
			{
				handle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool ResetEvent_internal(IntPtr handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void CloseEvent_internal(IntPtr handle);

	public unsafe static IntPtr OpenEvent_internal(string name, EventWaitHandleRights rights, out int errorCode)
	{
		fixed (char* name2 = name)
		{
			return OpenEvent_icall(name2, name?.Length ?? 0, rights, out errorCode);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern IntPtr OpenEvent_icall(char* name, int name_length, EventWaitHandleRights rights, out int errorCode);
}
