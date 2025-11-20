using Microsoft.Win32.SafeHandles;

namespace System.Net.Security;

internal sealed class SafeFreeCertContext : SafeHandleZeroOrMinusOneIsInvalid
{
	private const uint CRYPT_ACQUIRE_SILENT_FLAG = 64u;

	internal SafeFreeCertContext()
		: base(ownsHandle: true)
	{
	}

	internal void Set(IntPtr value)
	{
		handle = value;
	}

	protected override bool ReleaseHandle()
	{
		global::Interop.Crypt32.CertFreeCertificateContext(handle);
		return true;
	}
}
