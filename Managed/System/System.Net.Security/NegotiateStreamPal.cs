using System.ComponentModel;
using System.Globalization;

namespace System.Net.Security;

internal static class NegotiateStreamPal
{
	internal static int QueryMaxTokenSize(string package)
	{
		return SSPIWrapper.GetVerifyPackageInfo(GlobalSSPI.SSPIAuth, package, throwIfMissing: true).MaxToken;
	}

	internal static SafeFreeCredentials AcquireDefaultCredential(string package, bool isServer)
	{
		return SSPIWrapper.AcquireDefaultCredential(GlobalSSPI.SSPIAuth, package, isServer ? global::Interop.SspiCli.CredentialUse.SECPKG_CRED_INBOUND : global::Interop.SspiCli.CredentialUse.SECPKG_CRED_OUTBOUND);
	}

	internal static SafeFreeCredentials AcquireCredentialsHandle(string package, bool isServer, NetworkCredential credential)
	{
		SafeSspiAuthDataHandle authData = null;
		try
		{
			global::Interop.SECURITY_STATUS sECURITY_STATUS = global::Interop.SspiCli.SspiEncodeStringsAsAuthIdentity(credential.UserName, credential.Domain, credential.Password, out authData);
			if (sECURITY_STATUS != global::Interop.SECURITY_STATUS.OK)
			{
				if (NetEventSource.IsEnabled)
				{
					NetEventSource.Error(null, global::SR.Format("{0} failed with error {1}.", "SspiEncodeStringsAsAuthIdentity", $"0x{(int)sECURITY_STATUS:X}"), "AcquireCredentialsHandle");
				}
				throw new Win32Exception((int)sECURITY_STATUS);
			}
			return SSPIWrapper.AcquireCredentialsHandle(GlobalSSPI.SSPIAuth, package, isServer ? global::Interop.SspiCli.CredentialUse.SECPKG_CRED_INBOUND : global::Interop.SspiCli.CredentialUse.SECPKG_CRED_OUTBOUND, ref authData);
		}
		finally
		{
			authData?.Dispose();
		}
	}

	internal static string QueryContextClientSpecifiedSpn(SafeDeleteContext securityContext)
	{
		return SSPIWrapper.QueryContextAttributes(GlobalSSPI.SSPIAuth, securityContext, global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_CLIENT_SPECIFIED_TARGET) as string;
	}

	internal static string QueryContextAuthenticationPackage(SafeDeleteContext securityContext)
	{
		return (SSPIWrapper.QueryContextAttributes(GlobalSSPI.SSPIAuth, securityContext, global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_NEGOTIATION_INFO) as NegotiationInfoClass)?.AuthenticationPackage;
	}

	internal static SecurityStatusPal InitializeSecurityContext(SafeFreeCredentials credentialsHandle, ref SafeDeleteContext securityContext, string spn, ContextFlagsPal requestedContextFlags, SecurityBuffer[] inSecurityBufferArray, SecurityBuffer outSecurityBuffer, ref ContextFlagsPal contextFlags)
	{
		global::Interop.SspiCli.ContextFlags outFlags = global::Interop.SspiCli.ContextFlags.Zero;
		int win32SecurityStatus = SSPIWrapper.InitializeSecurityContext(GlobalSSPI.SSPIAuth, credentialsHandle, ref securityContext, spn, ContextFlagsAdapterPal.GetInteropFromContextFlagsPal(requestedContextFlags), global::Interop.SspiCli.Endianness.SECURITY_NETWORK_DREP, inSecurityBufferArray, outSecurityBuffer, ref outFlags);
		contextFlags = ContextFlagsAdapterPal.GetContextFlagsPalFromInterop(outFlags);
		return SecurityStatusAdapterPal.GetSecurityStatusPalFromInterop((global::Interop.SECURITY_STATUS)win32SecurityStatus);
	}

	internal static SecurityStatusPal CompleteAuthToken(ref SafeDeleteContext securityContext, SecurityBuffer[] inSecurityBufferArray)
	{
		return SecurityStatusAdapterPal.GetSecurityStatusPalFromInterop((global::Interop.SECURITY_STATUS)SSPIWrapper.CompleteAuthToken(GlobalSSPI.SSPIAuth, ref securityContext, inSecurityBufferArray));
	}

	internal static SecurityStatusPal AcceptSecurityContext(SafeFreeCredentials credentialsHandle, ref SafeDeleteContext securityContext, ContextFlagsPal requestedContextFlags, SecurityBuffer[] inSecurityBufferArray, SecurityBuffer outSecurityBuffer, ref ContextFlagsPal contextFlags)
	{
		global::Interop.SspiCli.ContextFlags outFlags = global::Interop.SspiCli.ContextFlags.Zero;
		int win32SecurityStatus = SSPIWrapper.AcceptSecurityContext(GlobalSSPI.SSPIAuth, credentialsHandle, ref securityContext, ContextFlagsAdapterPal.GetInteropFromContextFlagsPal(requestedContextFlags), global::Interop.SspiCli.Endianness.SECURITY_NETWORK_DREP, inSecurityBufferArray, outSecurityBuffer, ref outFlags);
		contextFlags = ContextFlagsAdapterPal.GetContextFlagsPalFromInterop(outFlags);
		return SecurityStatusAdapterPal.GetSecurityStatusPalFromInterop((global::Interop.SECURITY_STATUS)win32SecurityStatus);
	}

	internal static Win32Exception CreateExceptionFromError(SecurityStatusPal statusCode)
	{
		return new Win32Exception((int)SecurityStatusAdapterPal.GetInteropFromSecurityStatusPal(statusCode));
	}

	internal static int VerifySignature(SafeDeleteContext securityContext, byte[] buffer, int offset, int count)
	{
		if (offset < 0 || offset > ((buffer != null) ? buffer.Length : 0))
		{
			NetEventSource.Info("Argument 'offset' out of range.", null, "VerifySignature");
			throw new ArgumentOutOfRangeException("offset");
		}
		if (count < 0 || count > ((buffer != null) ? (buffer.Length - offset) : 0))
		{
			NetEventSource.Info("Argument 'count' out of range.", null, "VerifySignature");
			throw new ArgumentOutOfRangeException("count");
		}
		SecurityBuffer[] array = new SecurityBuffer[2]
		{
			new SecurityBuffer(buffer, offset, count, SecurityBufferType.SECBUFFER_STREAM),
			new SecurityBuffer(0, SecurityBufferType.SECBUFFER_DATA)
		};
		int num = SSPIWrapper.VerifySignature(GlobalSSPI.SSPIAuth, securityContext, array, 0u);
		if (num != 0)
		{
			NetEventSource.Info("VerifySignature threw error: " + num.ToString("x", NumberFormatInfo.InvariantInfo), null, "VerifySignature");
			throw new Win32Exception(num);
		}
		if (array[1].type != SecurityBufferType.SECBUFFER_DATA)
		{
			throw new InternalException();
		}
		return array[1].size;
	}

	internal static int MakeSignature(SafeDeleteContext securityContext, byte[] buffer, int offset, int count, ref byte[] output)
	{
		SecPkgContext_Sizes secPkgContext_Sizes = SSPIWrapper.QueryContextAttributes(GlobalSSPI.SSPIAuth, securityContext, global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_SIZES) as SecPkgContext_Sizes;
		int num = count + secPkgContext_Sizes.cbMaxSignature;
		if (output == null || output.Length < num)
		{
			output = new byte[num];
		}
		Buffer.BlockCopy(buffer, offset, output, secPkgContext_Sizes.cbMaxSignature, count);
		SecurityBuffer[] array = new SecurityBuffer[2]
		{
			new SecurityBuffer(output, 0, secPkgContext_Sizes.cbMaxSignature, SecurityBufferType.SECBUFFER_TOKEN),
			new SecurityBuffer(output, secPkgContext_Sizes.cbMaxSignature, count, SecurityBufferType.SECBUFFER_DATA)
		};
		int num2 = SSPIWrapper.MakeSignature(GlobalSSPI.SSPIAuth, securityContext, array, 0u);
		if (num2 != 0)
		{
			NetEventSource.Info("MakeSignature threw error: " + num2.ToString("x", NumberFormatInfo.InvariantInfo), null, "MakeSignature");
			throw new Win32Exception(num2);
		}
		return array[0].size + array[1].size;
	}
}
