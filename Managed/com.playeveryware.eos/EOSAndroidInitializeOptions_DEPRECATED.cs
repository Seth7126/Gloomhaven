using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public class EOSAndroidInitializeOptions_DEPRECATED : IDisposable
{
	public int ApiVersion;

	public IntPtr VM;

	public IntPtr OptionalInternalDirectory;

	public IntPtr OptionalExternalDirectory;

	public void Dispose()
	{
	}
}
