using System.Runtime.InteropServices;

namespace System.Data;

internal static class SafeNativeMethods
{
	internal static IntPtr LocalAlloc(IntPtr initialSize)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(initialSize);
		ZeroMemory(intPtr, (int)initialSize);
		return intPtr;
	}

	internal static void LocalFree(IntPtr ptr)
	{
		Marshal.FreeHGlobal(ptr);
	}

	internal static void ZeroMemory(IntPtr ptr, int length)
	{
		Marshal.Copy(new byte[length], 0, ptr, length);
	}

	[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true, ThrowOnUnmappableChar = true)]
	internal static extern IntPtr GetProcAddress(IntPtr HModule, [In][MarshalAs(UnmanagedType.LPStr)] string funcName);
}
