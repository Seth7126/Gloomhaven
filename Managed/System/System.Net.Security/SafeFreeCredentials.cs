using System.Runtime.InteropServices;

namespace System.Net.Security;

internal abstract class SafeFreeCredentials : SafeHandle
{
	internal global::Interop.SspiCli.CredHandle _handle;

	public override bool IsInvalid
	{
		get
		{
			if (!base.IsClosed)
			{
				return _handle.IsZero;
			}
			return true;
		}
	}

	protected SafeFreeCredentials()
		: base(IntPtr.Zero, ownsHandle: true)
	{
		_handle = default(global::Interop.SspiCli.CredHandle);
	}

	public unsafe static int AcquireCredentialsHandle(string package, global::Interop.SspiCli.CredentialUse intent, ref global::Interop.SspiCli.SEC_WINNT_AUTH_IDENTITY_W authdata, out SafeFreeCredentials outCredential)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(null, package, intent, authdata, "AcquireCredentialsHandle");
		}
		outCredential = new SafeFreeCredential_SECURITY();
		long timeStamp;
		int num = global::Interop.SspiCli.AcquireCredentialsHandleW(null, package, (int)intent, null, ref authdata, null, null, ref outCredential._handle, out timeStamp);
		if (num != 0)
		{
			outCredential.SetHandleAsInvalid();
		}
		return num;
	}

	public unsafe static int AcquireDefaultCredential(string package, global::Interop.SspiCli.CredentialUse intent, out SafeFreeCredentials outCredential)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(null, package, intent, "AcquireDefaultCredential");
		}
		outCredential = new SafeFreeCredential_SECURITY();
		long timeStamp;
		int num = global::Interop.SspiCli.AcquireCredentialsHandleW(null, package, (int)intent, null, IntPtr.Zero, null, null, ref outCredential._handle, out timeStamp);
		if (num != 0)
		{
			outCredential.SetHandleAsInvalid();
		}
		return num;
	}

	public unsafe static int AcquireCredentialsHandle(string package, global::Interop.SspiCli.CredentialUse intent, ref SafeSspiAuthDataHandle authdata, out SafeFreeCredentials outCredential)
	{
		outCredential = new SafeFreeCredential_SECURITY();
		long timeStamp;
		int num = global::Interop.SspiCli.AcquireCredentialsHandleW(null, package, (int)intent, null, authdata, null, null, ref outCredential._handle, out timeStamp);
		if (num != 0)
		{
			outCredential.SetHandleAsInvalid();
		}
		return num;
	}

	public unsafe static int AcquireCredentialsHandle(string package, global::Interop.SspiCli.CredentialUse intent, ref global::Interop.SspiCli.SCHANNEL_CRED authdata, out SafeFreeCredentials outCredential)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(null, package, intent, authdata, "AcquireCredentialsHandle");
		}
		int num = -1;
		IntPtr paCred = authdata.paCred;
		try
		{
			IntPtr paCred2 = new IntPtr(&paCred);
			if (paCred != IntPtr.Zero)
			{
				authdata.paCred = paCred2;
			}
			outCredential = new SafeFreeCredential_SECURITY();
			num = global::Interop.SspiCli.AcquireCredentialsHandleW(null, package, (int)intent, null, ref authdata, null, null, ref outCredential._handle, out var _);
		}
		finally
		{
			authdata.paCred = paCred;
		}
		if (num != 0)
		{
			outCredential.SetHandleAsInvalid();
		}
		return num;
	}
}
