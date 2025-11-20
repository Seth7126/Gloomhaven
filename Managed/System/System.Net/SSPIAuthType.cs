using System.Net.Security;
using System.Runtime.InteropServices;

namespace System.Net;

internal class SSPIAuthType : SSPIInterface
{
	private static volatile SecurityPackageInfoClass[] s_securityPackages;

	public SecurityPackageInfoClass[] SecurityPackages
	{
		get
		{
			return s_securityPackages;
		}
		set
		{
			s_securityPackages = value;
		}
	}

	public int EnumerateSecurityPackages(out int pkgnum, out SafeFreeContextBuffer pkgArray)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Info(this, null, "EnumerateSecurityPackages");
		}
		return SafeFreeContextBuffer.EnumeratePackages(out pkgnum, out pkgArray);
	}

	public int AcquireCredentialsHandle(string moduleName, global::Interop.SspiCli.CredentialUse usage, ref global::Interop.SspiCli.SEC_WINNT_AUTH_IDENTITY_W authdata, out SafeFreeCredentials outCredential)
	{
		return SafeFreeCredentials.AcquireCredentialsHandle(moduleName, usage, ref authdata, out outCredential);
	}

	public int AcquireCredentialsHandle(string moduleName, global::Interop.SspiCli.CredentialUse usage, ref SafeSspiAuthDataHandle authdata, out SafeFreeCredentials outCredential)
	{
		return SafeFreeCredentials.AcquireCredentialsHandle(moduleName, usage, ref authdata, out outCredential);
	}

	public int AcquireDefaultCredential(string moduleName, global::Interop.SspiCli.CredentialUse usage, out SafeFreeCredentials outCredential)
	{
		return SafeFreeCredentials.AcquireDefaultCredential(moduleName, usage, out outCredential);
	}

	public int AcquireCredentialsHandle(string moduleName, global::Interop.SspiCli.CredentialUse usage, ref global::Interop.SspiCli.SCHANNEL_CRED authdata, out SafeFreeCredentials outCredential)
	{
		return SafeFreeCredentials.AcquireCredentialsHandle(moduleName, usage, ref authdata, out outCredential);
	}

	public int AcceptSecurityContext(ref SafeFreeCredentials credential, ref SafeDeleteContext context, SecurityBuffer inputBuffer, global::Interop.SspiCli.ContextFlags inFlags, global::Interop.SspiCli.Endianness endianness, SecurityBuffer outputBuffer, ref global::Interop.SspiCli.ContextFlags outFlags)
	{
		return SafeDeleteContext.AcceptSecurityContext(ref credential, ref context, inFlags, endianness, inputBuffer, null, outputBuffer, ref outFlags);
	}

	public int AcceptSecurityContext(SafeFreeCredentials credential, ref SafeDeleteContext context, SecurityBuffer[] inputBuffers, global::Interop.SspiCli.ContextFlags inFlags, global::Interop.SspiCli.Endianness endianness, SecurityBuffer outputBuffer, ref global::Interop.SspiCli.ContextFlags outFlags)
	{
		return SafeDeleteContext.AcceptSecurityContext(ref credential, ref context, inFlags, endianness, null, inputBuffers, outputBuffer, ref outFlags);
	}

	public int InitializeSecurityContext(ref SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, global::Interop.SspiCli.ContextFlags inFlags, global::Interop.SspiCli.Endianness endianness, SecurityBuffer inputBuffer, SecurityBuffer outputBuffer, ref global::Interop.SspiCli.ContextFlags outFlags)
	{
		return SafeDeleteContext.InitializeSecurityContext(ref credential, ref context, targetName, inFlags, endianness, inputBuffer, null, outputBuffer, ref outFlags);
	}

	public int InitializeSecurityContext(SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, global::Interop.SspiCli.ContextFlags inFlags, global::Interop.SspiCli.Endianness endianness, SecurityBuffer[] inputBuffers, SecurityBuffer outputBuffer, ref global::Interop.SspiCli.ContextFlags outFlags)
	{
		return SafeDeleteContext.InitializeSecurityContext(ref credential, ref context, targetName, inFlags, endianness, null, inputBuffers, outputBuffer, ref outFlags);
	}

	public int EncryptMessage(SafeDeleteContext context, ref global::Interop.SspiCli.SecBufferDesc inputOutput, uint sequenceNumber)
	{
		try
		{
			bool success = false;
			context.DangerousAddRef(ref success);
			return global::Interop.SspiCli.EncryptMessage(ref context._handle, 0u, ref inputOutput, sequenceNumber);
		}
		finally
		{
			context.DangerousRelease();
		}
	}

	public unsafe int DecryptMessage(SafeDeleteContext context, ref global::Interop.SspiCli.SecBufferDesc inputOutput, uint sequenceNumber)
	{
		int num = -2146893055;
		uint num2 = 0u;
		try
		{
			bool success = false;
			context.DangerousAddRef(ref success);
			num = global::Interop.SspiCli.DecryptMessage(ref context._handle, ref inputOutput, sequenceNumber, &num2);
		}
		finally
		{
			context.DangerousRelease();
		}
		if (num == 0 && num2 == 2147483649u)
		{
			NetEventSource.Fail(this, $"Expected qop = 0, returned value = {num2}", "DecryptMessage");
			throw new InvalidOperationException("Protocol error: A received message contains a valid signature but it was not encrypted as required by the effective Protection Level.");
		}
		return num;
	}

	public int MakeSignature(SafeDeleteContext context, ref global::Interop.SspiCli.SecBufferDesc inputOutput, uint sequenceNumber)
	{
		try
		{
			bool success = false;
			context.DangerousAddRef(ref success);
			return global::Interop.SspiCli.EncryptMessage(ref context._handle, 2147483649u, ref inputOutput, sequenceNumber);
		}
		finally
		{
			context.DangerousRelease();
		}
	}

	public unsafe int VerifySignature(SafeDeleteContext context, ref global::Interop.SspiCli.SecBufferDesc inputOutput, uint sequenceNumber)
	{
		try
		{
			bool success = false;
			uint num = 0u;
			context.DangerousAddRef(ref success);
			return global::Interop.SspiCli.DecryptMessage(ref context._handle, ref inputOutput, sequenceNumber, &num);
		}
		finally
		{
			context.DangerousRelease();
		}
	}

	public int QueryContextChannelBinding(SafeDeleteContext context, global::Interop.SspiCli.ContextAttribute attribute, out SafeFreeContextBufferChannelBinding binding)
	{
		binding = null;
		throw new NotSupportedException();
	}

	public unsafe int QueryContextAttributes(SafeDeleteContext context, global::Interop.SspiCli.ContextAttribute attribute, byte[] buffer, Type handleType, out SafeHandle refHandle)
	{
		refHandle = null;
		if (handleType != null)
		{
			if (handleType == typeof(SafeFreeContextBuffer))
			{
				refHandle = SafeFreeContextBuffer.CreateEmptyHandle();
			}
			else
			{
				if (!(handleType == typeof(SafeFreeCertContext)))
				{
					throw new ArgumentException(global::SR.Format("'{0}' is not a supported handle type.", handleType.FullName), "handleType");
				}
				refHandle = new SafeFreeCertContext();
			}
		}
		fixed (byte* buffer2 = buffer)
		{
			return SafeFreeContextBuffer.QueryContextAttributes(context, attribute, buffer2, refHandle);
		}
	}

	public int SetContextAttributes(SafeDeleteContext context, global::Interop.SspiCli.ContextAttribute attribute, byte[] buffer)
	{
		throw System.NotImplemented.ByDesignWithMessage("This method is not implemented by this class.");
	}

	public int QuerySecurityContextToken(SafeDeleteContext phContext, out SecurityContextTokenHandle phToken)
	{
		return GetSecurityContextToken(phContext, out phToken);
	}

	public int CompleteAuthToken(ref SafeDeleteContext refContext, SecurityBuffer[] inputBuffers)
	{
		return SafeDeleteContext.CompleteAuthToken(ref refContext, inputBuffers);
	}

	private static int GetSecurityContextToken(SafeDeleteContext phContext, out SecurityContextTokenHandle safeHandle)
	{
		safeHandle = null;
		try
		{
			bool success = false;
			phContext.DangerousAddRef(ref success);
			return global::Interop.SspiCli.QuerySecurityContextToken(ref phContext._handle, out safeHandle);
		}
		finally
		{
			phContext.DangerousRelease();
		}
	}

	public int ApplyControlToken(ref SafeDeleteContext refContext, SecurityBuffer[] inputBuffers)
	{
		throw new NotSupportedException();
	}
}
