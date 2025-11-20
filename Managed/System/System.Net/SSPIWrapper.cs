using System.ComponentModel;
using System.Globalization;
using System.Net.Security;
using System.Runtime.InteropServices;

namespace System.Net;

internal static class SSPIWrapper
{
	private enum OP
	{
		Encrypt = 1,
		Decrypt,
		MakeSignature,
		VerifySignature
	}

	internal static SecurityPackageInfoClass[] EnumerateSecurityPackages(SSPIInterface secModule)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(null, null, "EnumerateSecurityPackages");
		}
		if (secModule.SecurityPackages == null)
		{
			lock (secModule)
			{
				if (secModule.SecurityPackages == null)
				{
					int pkgnum = 0;
					SafeFreeContextBuffer pkgArray = null;
					try
					{
						int num = secModule.EnumerateSecurityPackages(out pkgnum, out pkgArray);
						if (NetEventSource.IsEnabled)
						{
							NetEventSource.Info(null, $"arrayBase: {pkgArray}", "EnumerateSecurityPackages");
						}
						if (num != 0)
						{
							throw new Win32Exception(num);
						}
						SecurityPackageInfoClass[] array = new SecurityPackageInfoClass[pkgnum];
						for (int i = 0; i < pkgnum; i++)
						{
							array[i] = new SecurityPackageInfoClass(pkgArray, i);
							if (NetEventSource.IsEnabled)
							{
								NetEventSource.Log.EnumerateSecurityPackages(array[i].Name);
							}
						}
						secModule.SecurityPackages = array;
					}
					finally
					{
						pkgArray?.Dispose();
					}
				}
			}
		}
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Exit(null, null, "EnumerateSecurityPackages");
		}
		return secModule.SecurityPackages;
	}

	internal static SecurityPackageInfoClass GetVerifyPackageInfo(SSPIInterface secModule, string packageName)
	{
		return GetVerifyPackageInfo(secModule, packageName, throwIfMissing: false);
	}

	internal static SecurityPackageInfoClass GetVerifyPackageInfo(SSPIInterface secModule, string packageName, bool throwIfMissing)
	{
		SecurityPackageInfoClass[] array = EnumerateSecurityPackages(secModule);
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (string.Compare(array[i].Name, packageName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return array[i];
				}
			}
		}
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Log.SspiPackageNotFound(packageName);
		}
		if (throwIfMissing)
		{
			throw new NotSupportedException("The requested security package is not supported.");
		}
		return null;
	}

	public static SafeFreeCredentials AcquireDefaultCredential(SSPIInterface secModule, string package, global::Interop.SspiCli.CredentialUse intent)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(null, package, "AcquireDefaultCredential");
			NetEventSource.Log.AcquireDefaultCredential(package, intent);
		}
		SafeFreeCredentials outCredential = null;
		int num = secModule.AcquireDefaultCredential(package, intent, out outCredential);
		if (num != 0)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Error(null, global::SR.Format("{0} failed with error {1}.", "AcquireDefaultCredential", $"0x{num:X}"), "AcquireDefaultCredential");
			}
			throw new Win32Exception(num);
		}
		return outCredential;
	}

	public static SafeFreeCredentials AcquireCredentialsHandle(SSPIInterface secModule, string package, global::Interop.SspiCli.CredentialUse intent, ref global::Interop.SspiCli.SEC_WINNT_AUTH_IDENTITY_W authdata)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(null, package, "AcquireCredentialsHandle");
			NetEventSource.Log.AcquireCredentialsHandle(package, intent, authdata);
		}
		SafeFreeCredentials outCredential = null;
		int num = secModule.AcquireCredentialsHandle(package, intent, ref authdata, out outCredential);
		if (num != 0)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Error(null, global::SR.Format("{0} failed with error {1}.", "AcquireCredentialsHandle", $"0x{num:X}"), "AcquireCredentialsHandle");
			}
			throw new Win32Exception(num);
		}
		return outCredential;
	}

	public static SafeFreeCredentials AcquireCredentialsHandle(SSPIInterface secModule, string package, global::Interop.SspiCli.CredentialUse intent, ref SafeSspiAuthDataHandle authdata)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Log.AcquireCredentialsHandle(package, intent, authdata);
		}
		SafeFreeCredentials outCredential = null;
		int num = secModule.AcquireCredentialsHandle(package, intent, ref authdata, out outCredential);
		if (num != 0)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Error(null, global::SR.Format("{0} failed with error {1}.", "AcquireCredentialsHandle", $"0x{num:X}"), "AcquireCredentialsHandle");
			}
			throw new Win32Exception(num);
		}
		return outCredential;
	}

	public static SafeFreeCredentials AcquireCredentialsHandle(SSPIInterface secModule, string package, global::Interop.SspiCli.CredentialUse intent, global::Interop.SspiCli.SCHANNEL_CRED scc)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(null, package, "AcquireCredentialsHandle");
			NetEventSource.Log.AcquireCredentialsHandle(package, intent, scc);
		}
		SafeFreeCredentials outCredential = null;
		int num = secModule.AcquireCredentialsHandle(package, intent, ref scc, out outCredential);
		if (num != 0)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Error(null, global::SR.Format("{0} failed with error {1}.", "AcquireCredentialsHandle", $"0x{num:X}"), "AcquireCredentialsHandle");
			}
			throw new Win32Exception(num);
		}
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Exit(null, outCredential, "AcquireCredentialsHandle");
		}
		return outCredential;
	}

	internal static int InitializeSecurityContext(SSPIInterface secModule, ref SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, global::Interop.SspiCli.ContextFlags inFlags, global::Interop.SspiCli.Endianness datarep, SecurityBuffer inputBuffer, SecurityBuffer outputBuffer, ref global::Interop.SspiCli.ContextFlags outFlags)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Log.InitializeSecurityContext(credential, context, targetName, inFlags);
		}
		int num = secModule.InitializeSecurityContext(ref credential, ref context, targetName, inFlags, datarep, inputBuffer, outputBuffer, ref outFlags);
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Log.SecurityContextInputBuffer("InitializeSecurityContext", inputBuffer?.size ?? 0, outputBuffer.size, (global::Interop.SECURITY_STATUS)num);
		}
		return num;
	}

	internal static int InitializeSecurityContext(SSPIInterface secModule, SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, global::Interop.SspiCli.ContextFlags inFlags, global::Interop.SspiCli.Endianness datarep, SecurityBuffer[] inputBuffers, SecurityBuffer outputBuffer, ref global::Interop.SspiCli.ContextFlags outFlags)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Log.InitializeSecurityContext(credential, context, targetName, inFlags);
		}
		int num = secModule.InitializeSecurityContext(credential, ref context, targetName, inFlags, datarep, inputBuffers, outputBuffer, ref outFlags);
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Log.SecurityContextInputBuffers("InitializeSecurityContext", (inputBuffers != null) ? inputBuffers.Length : 0, outputBuffer.size, (global::Interop.SECURITY_STATUS)num);
		}
		return num;
	}

	internal static int AcceptSecurityContext(SSPIInterface secModule, ref SafeFreeCredentials credential, ref SafeDeleteContext context, global::Interop.SspiCli.ContextFlags inFlags, global::Interop.SspiCli.Endianness datarep, SecurityBuffer inputBuffer, SecurityBuffer outputBuffer, ref global::Interop.SspiCli.ContextFlags outFlags)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Log.AcceptSecurityContext(credential, context, inFlags);
		}
		int num = secModule.AcceptSecurityContext(ref credential, ref context, inputBuffer, inFlags, datarep, outputBuffer, ref outFlags);
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Log.SecurityContextInputBuffer("AcceptSecurityContext", inputBuffer?.size ?? 0, outputBuffer.size, (global::Interop.SECURITY_STATUS)num);
		}
		return num;
	}

	internal static int AcceptSecurityContext(SSPIInterface secModule, SafeFreeCredentials credential, ref SafeDeleteContext context, global::Interop.SspiCli.ContextFlags inFlags, global::Interop.SspiCli.Endianness datarep, SecurityBuffer[] inputBuffers, SecurityBuffer outputBuffer, ref global::Interop.SspiCli.ContextFlags outFlags)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Log.AcceptSecurityContext(credential, context, inFlags);
		}
		int num = secModule.AcceptSecurityContext(credential, ref context, inputBuffers, inFlags, datarep, outputBuffer, ref outFlags);
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Log.SecurityContextInputBuffers("AcceptSecurityContext", (inputBuffers != null) ? inputBuffers.Length : 0, outputBuffer.size, (global::Interop.SECURITY_STATUS)num);
		}
		return num;
	}

	internal static int CompleteAuthToken(SSPIInterface secModule, ref SafeDeleteContext context, SecurityBuffer[] inputBuffers)
	{
		int num = secModule.CompleteAuthToken(ref context, inputBuffers);
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Log.OperationReturnedSomething("CompleteAuthToken", (global::Interop.SECURITY_STATUS)num);
		}
		return num;
	}

	internal static int ApplyControlToken(SSPIInterface secModule, ref SafeDeleteContext context, SecurityBuffer[] inputBuffers)
	{
		int num = secModule.ApplyControlToken(ref context, inputBuffers);
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Log.OperationReturnedSomething("ApplyControlToken", (global::Interop.SECURITY_STATUS)num);
		}
		return num;
	}

	public static int QuerySecurityContextToken(SSPIInterface secModule, SafeDeleteContext context, out SecurityContextTokenHandle token)
	{
		return secModule.QuerySecurityContextToken(context, out token);
	}

	public static int EncryptMessage(SSPIInterface secModule, SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
	{
		return EncryptDecryptHelper(OP.Encrypt, secModule, context, input, sequenceNumber);
	}

	public static int DecryptMessage(SSPIInterface secModule, SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
	{
		return EncryptDecryptHelper(OP.Decrypt, secModule, context, input, sequenceNumber);
	}

	internal static int MakeSignature(SSPIInterface secModule, SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
	{
		return EncryptDecryptHelper(OP.MakeSignature, secModule, context, input, sequenceNumber);
	}

	public static int VerifySignature(SSPIInterface secModule, SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
	{
		return EncryptDecryptHelper(OP.VerifySignature, secModule, context, input, sequenceNumber);
	}

	private unsafe static int EncryptDecryptHelper(OP op, SSPIInterface secModule, SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
	{
		global::Interop.SspiCli.SecBufferDesc inputOutput = new global::Interop.SspiCli.SecBufferDesc(input.Length);
		global::Interop.SspiCli.SecBuffer[] array = new global::Interop.SspiCli.SecBuffer[input.Length];
		fixed (global::Interop.SspiCli.SecBuffer* pBuffers = array)
		{
			inputOutput.pBuffers = pBuffers;
			GCHandle[] array2 = new GCHandle[input.Length];
			byte[][] array3 = new byte[input.Length][];
			try
			{
				for (int i = 0; i < input.Length; i++)
				{
					SecurityBuffer securityBuffer = input[i];
					array[i].cbBuffer = securityBuffer.size;
					array[i].BufferType = securityBuffer.type;
					if (securityBuffer.token == null || securityBuffer.token.Length == 0)
					{
						array[i].pvBuffer = IntPtr.Zero;
						continue;
					}
					array2[i] = GCHandle.Alloc(securityBuffer.token, GCHandleType.Pinned);
					array[i].pvBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(securityBuffer.token, securityBuffer.offset);
					array3[i] = securityBuffer.token;
				}
				int num;
				switch (op)
				{
				case OP.Encrypt:
					num = secModule.EncryptMessage(context, ref inputOutput, sequenceNumber);
					break;
				case OP.Decrypt:
					num = secModule.DecryptMessage(context, ref inputOutput, sequenceNumber);
					break;
				case OP.MakeSignature:
					num = secModule.MakeSignature(context, ref inputOutput, sequenceNumber);
					break;
				case OP.VerifySignature:
					num = secModule.VerifySignature(context, ref inputOutput, sequenceNumber);
					break;
				default:
					NetEventSource.Fail(null, $"Unknown OP: {op}", "EncryptDecryptHelper");
					throw System.NotImplemented.ByDesignWithMessage("This method is not implemented by this class.");
				}
				for (int j = 0; j < input.Length; j++)
				{
					SecurityBuffer securityBuffer2 = input[j];
					securityBuffer2.size = array[j].cbBuffer;
					securityBuffer2.type = array[j].BufferType;
					checked
					{
						if (securityBuffer2.size == 0)
						{
							securityBuffer2.offset = 0;
							securityBuffer2.token = null;
						}
						else
						{
							int k;
							for (k = 0; k < input.Length; k++)
							{
								if (array3[k] != null)
								{
									byte* ptr = unchecked((byte*)(void*)Marshal.UnsafeAddrOfPinnedArrayElement(array3[k], 0));
									if ((void*)array[j].pvBuffer >= ptr && unchecked((nuint)(void*)array[j].pvBuffer) + unchecked((nuint)securityBuffer2.size) <= unchecked((nuint)ptr) + unchecked((nuint)array3[k].Length))
									{
										securityBuffer2.offset = (int)(unchecked((byte*)(void*)array[j].pvBuffer) - ptr);
										securityBuffer2.token = array3[k];
										break;
									}
								}
							}
							if (k >= input.Length)
							{
								NetEventSource.Fail(null, "Output buffer out of range.", "EncryptDecryptHelper");
								securityBuffer2.size = 0;
								securityBuffer2.offset = 0;
								securityBuffer2.token = null;
							}
						}
						if (securityBuffer2.offset < 0 || securityBuffer2.offset > ((securityBuffer2.token != null) ? securityBuffer2.token.Length : 0))
						{
							NetEventSource.Fail(null, $"'offset' out of range.  [{securityBuffer2.offset}]", "EncryptDecryptHelper");
						}
					}
					if (securityBuffer2.size < 0 || securityBuffer2.size > ((securityBuffer2.token != null) ? (securityBuffer2.token.Length - securityBuffer2.offset) : 0))
					{
						NetEventSource.Fail(null, $"'size' out of range.  [{securityBuffer2.size}]", "EncryptDecryptHelper");
					}
				}
				if (NetEventSource.IsEnabled)
				{
					switch (num)
					{
					case 590625:
						NetEventSource.Error(null, global::SR.Format("{0} returned {1}.", op, "SEC_I_RENEGOTIATE"), "EncryptDecryptHelper");
						break;
					default:
						NetEventSource.Error(null, global::SR.Format("{0} failed with error {1}.", op, $"0x{0:X}"), "EncryptDecryptHelper");
						break;
					case 0:
						break;
					}
				}
				return num;
			}
			finally
			{
				for (int l = 0; l < array2.Length; l++)
				{
					if (array2[l].IsAllocated)
					{
						array2[l].Free();
					}
				}
			}
		}
	}

	public static SafeFreeContextBufferChannelBinding QueryContextChannelBinding(SSPIInterface secModule, SafeDeleteContext securityContext, global::Interop.SspiCli.ContextAttribute contextAttribute)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(null, contextAttribute, "QueryContextChannelBinding");
		}
		SafeFreeContextBufferChannelBinding refHandle;
		int num = secModule.QueryContextChannelBinding(securityContext, contextAttribute, out refHandle);
		if (num != 0)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Exit(null, $"ERROR = {ErrorDescription(num)}", "QueryContextChannelBinding");
			}
			return null;
		}
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Exit(null, refHandle, "QueryContextChannelBinding");
		}
		return refHandle;
	}

	public static object QueryContextAttributes(SSPIInterface secModule, SafeDeleteContext securityContext, global::Interop.SspiCli.ContextAttribute contextAttribute)
	{
		int errorCode;
		return QueryContextAttributes(secModule, securityContext, contextAttribute, out errorCode);
	}

	public unsafe static object QueryContextAttributes(SSPIInterface secModule, SafeDeleteContext securityContext, global::Interop.SspiCli.ContextAttribute contextAttribute, out int errorCode)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(null, contextAttribute, "QueryContextAttributes");
		}
		int num = IntPtr.Size;
		Type handleType = null;
		switch (contextAttribute)
		{
		case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_SIZES:
			num = SecPkgContext_Sizes.SizeOf;
			break;
		case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_STREAM_SIZES:
			num = SecPkgContext_StreamSizes.SizeOf;
			break;
		case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_NAMES:
			handleType = typeof(SafeFreeContextBuffer);
			break;
		case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_PACKAGE_INFO:
			handleType = typeof(SafeFreeContextBuffer);
			break;
		case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_NEGOTIATION_INFO:
			handleType = typeof(SafeFreeContextBuffer);
			num = sizeof(SecPkgContext_NegotiationInfoW);
			break;
		case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_CLIENT_SPECIFIED_TARGET:
			handleType = typeof(SafeFreeContextBuffer);
			break;
		case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_REMOTE_CERT_CONTEXT:
			handleType = typeof(SafeFreeCertContext);
			break;
		case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_LOCAL_CERT_CONTEXT:
			handleType = typeof(SafeFreeCertContext);
			break;
		case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_ISSUER_LIST_EX:
			num = Marshal.SizeOf<global::Interop.SspiCli.SecPkgContext_IssuerListInfoEx>();
			handleType = typeof(SafeFreeContextBuffer);
			break;
		case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_CONNECTION_INFO:
			num = Marshal.SizeOf<SecPkgContext_ConnectionInfo>();
			break;
		case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_APPLICATION_PROTOCOL:
			num = Marshal.SizeOf<global::Interop.SecPkgContext_ApplicationProtocol>();
			break;
		default:
			throw new ArgumentException(global::SR.Format("The specified value is not valid in the '{0}' enumeration.", "contextAttribute"), "contextAttribute");
		}
		SafeHandle refHandle = null;
		object obj = null;
		try
		{
			byte[] array = new byte[num];
			errorCode = secModule.QueryContextAttributes(securityContext, contextAttribute, array, handleType, out refHandle);
			if (errorCode != 0)
			{
				if (NetEventSource.IsEnabled)
				{
					NetEventSource.Exit(null, $"ERROR = {ErrorDescription(errorCode)}", "QueryContextAttributes");
				}
				return null;
			}
			switch (contextAttribute)
			{
			case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_SIZES:
				obj = new SecPkgContext_Sizes(array);
				break;
			case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_STREAM_SIZES:
				obj = new SecPkgContext_StreamSizes(array);
				break;
			case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_NAMES:
				obj = Marshal.PtrToStringUni(refHandle.DangerousGetHandle());
				break;
			case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_PACKAGE_INFO:
				obj = new SecurityPackageInfoClass(refHandle, 0);
				break;
			case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_NEGOTIATION_INFO:
				fixed (byte* ptr2 = &array[0])
				{
					void* ptr3 = ptr2;
					obj = new NegotiationInfoClass(refHandle, (int)((SecPkgContext_NegotiationInfoW*)ptr3)->NegotiationState);
				}
				break;
			case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_CLIENT_SPECIFIED_TARGET:
				obj = Marshal.PtrToStringUni(refHandle.DangerousGetHandle());
				break;
			case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_REMOTE_CERT_CONTEXT:
			case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_LOCAL_CERT_CONTEXT:
				obj = refHandle;
				refHandle = null;
				break;
			case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_ISSUER_LIST_EX:
				obj = new global::Interop.SspiCli.SecPkgContext_IssuerListInfoEx(refHandle, array);
				refHandle = null;
				break;
			case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_CONNECTION_INFO:
				obj = new SecPkgContext_ConnectionInfo(array);
				break;
			case global::Interop.SspiCli.ContextAttribute.SECPKG_ATTR_APPLICATION_PROTOCOL:
				fixed (byte* ptr = array)
				{
					void* value = ptr;
					obj = Marshal.PtrToStructure<global::Interop.SecPkgContext_ApplicationProtocol>(new IntPtr(value));
				}
				break;
			}
		}
		finally
		{
			refHandle?.Dispose();
		}
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Exit(null, obj, "QueryContextAttributes");
		}
		return obj;
	}

	public static string ErrorDescription(int errorCode)
	{
		if (errorCode == -1)
		{
			return "An exception when invoking Win32 API";
		}
		return (global::Interop.SECURITY_STATUS)errorCode switch
		{
			global::Interop.SECURITY_STATUS.InvalidHandle => "Invalid handle", 
			global::Interop.SECURITY_STATUS.InvalidToken => "Invalid token", 
			global::Interop.SECURITY_STATUS.ContinueNeeded => "Continue needed", 
			global::Interop.SECURITY_STATUS.IncompleteMessage => "Message incomplete", 
			global::Interop.SECURITY_STATUS.WrongPrincipal => "Wrong principal", 
			global::Interop.SECURITY_STATUS.TargetUnknown => "Target unknown", 
			global::Interop.SECURITY_STATUS.PackageNotFound => "Package not found", 
			global::Interop.SECURITY_STATUS.BufferNotEnough => "Buffer not enough", 
			global::Interop.SECURITY_STATUS.MessageAltered => "Message altered", 
			global::Interop.SECURITY_STATUS.UntrustedRoot => "Untrusted root", 
			_ => "0x" + errorCode.ToString("x", NumberFormatInfo.InvariantInfo), 
		};
	}
}
