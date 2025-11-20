using System;
using System.IO;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.Win32.SafeHandles;

internal static class Interop
{
	internal static class Crypt32
	{
		internal struct CRYPT_OID_INFO
		{
			public int cbSize;

			public IntPtr pszOID;

			public IntPtr pwszName;

			public OidGroup dwGroupId;

			public int AlgId;

			public int cbData;

			public IntPtr pbData;

			public string OID => Marshal.PtrToStringAnsi(pszOID);

			public string Name => Marshal.PtrToStringUni(pwszName);
		}

		internal enum CryptOidInfoKeyType
		{
			CRYPT_OID_INFO_OID_KEY = 1,
			CRYPT_OID_INFO_NAME_KEY,
			CRYPT_OID_INFO_ALGID_KEY,
			CRYPT_OID_INFO_SIGN_KEY,
			CRYPT_OID_INFO_CNG_ALGID_KEY,
			CRYPT_OID_INFO_CNG_SIGN_KEY
		}

		internal static class AuthType
		{
			internal const uint AUTHTYPE_CLIENT = 1u;

			internal const uint AUTHTYPE_SERVER = 2u;
		}

		internal static class CertChainPolicyIgnoreFlags
		{
			internal const uint CERT_CHAIN_POLICY_IGNORE_NOT_TIME_VALID_FLAG = 1u;

			internal const uint CERT_CHAIN_POLICY_IGNORE_CTL_NOT_TIME_VALID_FLAG = 2u;

			internal const uint CERT_CHAIN_POLICY_IGNORE_NOT_TIME_NESTED_FLAG = 4u;

			internal const uint CERT_CHAIN_POLICY_IGNORE_INVALID_BASIC_CONSTRAINTS_FLAG = 8u;

			internal const uint CERT_CHAIN_POLICY_ALLOW_UNKNOWN_CA_FLAG = 16u;

			internal const uint CERT_CHAIN_POLICY_IGNORE_WRONG_USAGE_FLAG = 32u;

			internal const uint CERT_CHAIN_POLICY_IGNORE_INVALID_NAME_FLAG = 64u;

			internal const uint CERT_CHAIN_POLICY_IGNORE_INVALID_POLICY_FLAG = 128u;

			internal const uint CERT_CHAIN_POLICY_IGNORE_END_REV_UNKNOWN_FLAG = 256u;

			internal const uint CERT_CHAIN_POLICY_IGNORE_CTL_SIGNER_REV_UNKNOWN_FLAG = 512u;

			internal const uint CERT_CHAIN_POLICY_IGNORE_CA_REV_UNKNOWN_FLAG = 1024u;

			internal const uint CERT_CHAIN_POLICY_IGNORE_ROOT_REV_UNKNOWN_FLAG = 2048u;

			internal const uint CERT_CHAIN_POLICY_IGNORE_ALL = 4095u;
		}

		internal static class CertChainPolicy
		{
			internal const int CERT_CHAIN_POLICY_BASE = 1;

			internal const int CERT_CHAIN_POLICY_AUTHENTICODE = 2;

			internal const int CERT_CHAIN_POLICY_AUTHENTICODE_TS = 3;

			internal const int CERT_CHAIN_POLICY_SSL = 4;

			internal const int CERT_CHAIN_POLICY_BASIC_CONSTRAINTS = 5;

			internal const int CERT_CHAIN_POLICY_NT_AUTH = 6;

			internal const int CERT_CHAIN_POLICY_MICROSOFT_ROOT = 7;

			internal const int CERT_CHAIN_POLICY_EV = 8;
		}

		internal static class CertChainPolicyErrors
		{
			internal const uint TRUST_E_CERT_SIGNATURE = 2148098052u;

			internal const uint CRYPT_E_REVOKED = 2148081680u;

			internal const uint CERT_E_UNTRUSTEDROOT = 2148204809u;

			internal const uint CERT_E_UNTRUSTEDTESTROOT = 2148204813u;

			internal const uint CERT_E_CHAINING = 2148204810u;

			internal const uint CERT_E_WRONG_USAGE = 2148204816u;

			internal const uint CERT_E_EXPIRE = 2148204801u;

			internal const uint CERT_E_INVALID_NAME = 2148204820u;

			internal const uint CERT_E_INVALID_POLICY = 2148204819u;

			internal const uint TRUST_E_BASIC_CONSTRAINTS = 2148098073u;

			internal const uint CERT_E_CRITICAL = 2148204805u;

			internal const uint CERT_E_VALIDITYPERIODNESTING = 2148204802u;

			internal const uint CRYPT_E_NO_REVOCATION_CHECK = 2148081682u;

			internal const uint CRYPT_E_REVOCATION_OFFLINE = 2148081683u;

			internal const uint CERT_E_PURPOSE = 2148204806u;

			internal const uint CERT_E_REVOKED = 2148204812u;

			internal const uint CERT_E_REVOCATION_FAILURE = 2148204814u;

			internal const uint CERT_E_CN_NO_MATCH = 2148204815u;

			internal const uint CERT_E_ROLE = 2148204803u;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct CERT_CONTEXT
		{
			internal uint dwCertEncodingType;

			internal IntPtr pbCertEncoded;

			internal uint cbCertEncoded;

			internal IntPtr pCertInfo;

			internal IntPtr hCertStore;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct SSL_EXTRA_CERT_CHAIN_POLICY_PARA
		{
			internal uint cbSize;

			internal uint dwAuthType;

			internal uint fdwChecks;

			internal unsafe char* pwszServerName;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct CERT_CHAIN_POLICY_PARA
		{
			public uint cbSize;

			public uint dwFlags;

			public unsafe SSL_EXTRA_CERT_CHAIN_POLICY_PARA* pvExtraPolicyPara;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct CERT_CHAIN_POLICY_STATUS
		{
			public uint cbSize;

			public uint dwError;

			public int lChainIndex;

			public int lElementIndex;

			public unsafe void* pvExtraPolicyStatus;
		}

		internal static CRYPT_OID_INFO FindOidInfo(CryptOidInfoKeyType keyType, string key, OidGroup group, bool fallBackToAllGroups)
		{
			IntPtr intPtr = IntPtr.Zero;
			try
			{
				intPtr = keyType switch
				{
					CryptOidInfoKeyType.CRYPT_OID_INFO_OID_KEY => Marshal.StringToCoTaskMemAnsi(key), 
					CryptOidInfoKeyType.CRYPT_OID_INFO_NAME_KEY => Marshal.StringToCoTaskMemUni(key), 
					_ => throw new NotSupportedException(), 
				};
				if (!OidGroupWillNotUseActiveDirectory(group))
				{
					OidGroup oidGroup = group | (OidGroup)(-2147483648);
					IntPtr intPtr2 = CryptFindOIDInfo(keyType, intPtr, oidGroup);
					if (intPtr2 != IntPtr.Zero)
					{
						return Marshal.PtrToStructure<CRYPT_OID_INFO>(intPtr2);
					}
				}
				IntPtr intPtr3 = CryptFindOIDInfo(keyType, intPtr, group);
				if (intPtr3 != IntPtr.Zero)
				{
					return Marshal.PtrToStructure<CRYPT_OID_INFO>(intPtr3);
				}
				if (fallBackToAllGroups && group != OidGroup.All)
				{
					IntPtr intPtr4 = CryptFindOIDInfo(keyType, intPtr, OidGroup.All);
					if (intPtr4 != IntPtr.Zero)
					{
						return Marshal.PtrToStructure<CRYPT_OID_INFO>(intPtr4);
					}
				}
				return new CRYPT_OID_INFO
				{
					AlgId = -1
				};
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.FreeCoTaskMem(intPtr);
				}
			}
		}

		private static bool OidGroupWillNotUseActiveDirectory(OidGroup group)
		{
			if (group != OidGroup.HashAlgorithm && group != OidGroup.EncryptionAlgorithm && group != OidGroup.PublicKeyAlgorithm && group != OidGroup.SignatureAlgorithm && group != OidGroup.Attribute && group != OidGroup.ExtensionOrAttribute)
			{
				return group == OidGroup.KeyDerivationFunction;
			}
			return true;
		}

		[DllImport("crypt32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr CryptFindOIDInfo(CryptOidInfoKeyType dwKeyType, IntPtr pvKey, OidGroup group);

		[DllImport("crypt32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CertFreeCertificateContext(IntPtr pCertContext);

		[DllImport("crypt32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CertVerifyCertificateChainPolicy(IntPtr pszPolicyOID, SafeX509ChainHandle pChainContext, [In] ref CERT_CHAIN_POLICY_PARA pPolicyPara, [In][Out] ref CERT_CHAIN_POLICY_STATUS pPolicyStatus);
	}

	internal enum BOOL
	{
		FALSE,
		TRUE
	}

	internal static class Libraries
	{
		internal const string Advapi32 = "advapi32.dll";

		internal const string BCrypt = "BCrypt.dll";

		internal const string CoreComm_L1_1_1 = "api-ms-win-core-comm-l1-1-1.dll";

		internal const string Crypt32 = "crypt32.dll";

		internal const string Error_L1 = "api-ms-win-core-winrt-error-l1-1-0.dll";

		internal const string HttpApi = "httpapi.dll";

		internal const string IpHlpApi = "iphlpapi.dll";

		internal const string Kernel32 = "kernel32.dll";

		internal const string Memory_L1_3 = "api-ms-win-core-memory-l1-1-3.dll";

		internal const string Mswsock = "mswsock.dll";

		internal const string NCrypt = "ncrypt.dll";

		internal const string NtDll = "ntdll.dll";

		internal const string Odbc32 = "odbc32.dll";

		internal const string OleAut32 = "oleaut32.dll";

		internal const string PerfCounter = "perfcounter.dll";

		internal const string RoBuffer = "api-ms-win-core-winrt-robuffer-l1-1-0.dll";

		internal const string Secur32 = "secur32.dll";

		internal const string Shell32 = "shell32.dll";

		internal const string SspiCli = "sspicli.dll";

		internal const string User32 = "user32.dll";

		internal const string Version = "version.dll";

		internal const string WebSocket = "websocket.dll";

		internal const string WinHttp = "winhttp.dll";

		internal const string Ws2_32 = "ws2_32.dll";

		internal const string Wtsapi32 = "wtsapi32.dll";

		internal const string CompressionNative = "clrcompression.dll";
	}

	internal enum SECURITY_STATUS
	{
		OK = 0,
		ContinueNeeded = 590610,
		CompleteNeeded = 590611,
		CompAndContinue = 590612,
		ContextExpired = 590615,
		CredentialsNeeded = 590624,
		Renegotiate = 590625,
		OutOfMemory = -2146893056,
		InvalidHandle = -2146893055,
		Unsupported = -2146893054,
		TargetUnknown = -2146893053,
		InternalError = -2146893052,
		PackageNotFound = -2146893051,
		NotOwner = -2146893050,
		CannotInstall = -2146893049,
		InvalidToken = -2146893048,
		CannotPack = -2146893047,
		QopNotSupported = -2146893046,
		NoImpersonation = -2146893045,
		LogonDenied = -2146893044,
		UnknownCredentials = -2146893043,
		NoCredentials = -2146893042,
		MessageAltered = -2146893041,
		OutOfSequence = -2146893040,
		NoAuthenticatingAuthority = -2146893039,
		IncompleteMessage = -2146893032,
		IncompleteCredentials = -2146893024,
		BufferNotEnough = -2146893023,
		WrongPrincipal = -2146893022,
		TimeSkew = -2146893020,
		UntrustedRoot = -2146893019,
		IllegalMessage = -2146893018,
		CertUnknown = -2146893017,
		CertExpired = -2146893016,
		AlgorithmMismatch = -2146893007,
		SecurityQosFailed = -2146893006,
		SmartcardLogonRequired = -2146892994,
		UnsupportedPreauth = -2146892989,
		BadBinding = -2146892986,
		DowngradeDetected = -2146892976,
		ApplicationProtocolMismatch = -2146892953
	}

	internal enum ApplicationProtocolNegotiationStatus
	{
		None,
		Success,
		SelectedClientOnly
	}

	internal enum ApplicationProtocolNegotiationExt
	{
		None,
		NPN,
		ALPN
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class SecPkgContext_ApplicationProtocol
	{
		private const int MaxProtocolIdSize = 255;

		public ApplicationProtocolNegotiationStatus ProtoNegoStatus;

		public ApplicationProtocolNegotiationExt ProtoNegoExt;

		public byte ProtocolIdSize;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
		public byte[] ProtocolId;

		public byte[] Protocol => new Span<byte>(ProtocolId, 0, ProtocolIdSize).ToArray();
	}

	internal class Kernel32
	{
		internal class IOReparseOptions
		{
			internal const uint IO_REPARSE_TAG_FILE_PLACEHOLDER = 2147483669u;

			internal const uint IO_REPARSE_TAG_MOUNT_POINT = 2684354563u;
		}

		internal class FileOperations
		{
			internal const int OPEN_EXISTING = 3;

			internal const int COPY_FILE_FAIL_IF_EXISTS = 1;

			internal const int FILE_ACTION_ADDED = 1;

			internal const int FILE_ACTION_REMOVED = 2;

			internal const int FILE_ACTION_MODIFIED = 3;

			internal const int FILE_ACTION_RENAMED_OLD_NAME = 4;

			internal const int FILE_ACTION_RENAMED_NEW_NAME = 5;

			internal const int FILE_FLAG_BACKUP_SEMANTICS = 33554432;

			internal const int FILE_FLAG_FIRST_PIPE_INSTANCE = 524288;

			internal const int FILE_FLAG_OVERLAPPED = 1073741824;

			internal const int FILE_LIST_DIRECTORY = 1;
		}

		internal struct SECURITY_ATTRIBUTES
		{
			internal uint nLength;

			internal IntPtr lpSecurityDescriptor;

			internal BOOL bInheritHandle;
		}

		internal const uint SEM_FAILCRITICALERRORS = 1u;

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CloseHandle(IntPtr handle);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "CreateFileW", ExactSpelling = true, SetLastError = true)]
		private unsafe static extern IntPtr CreateFilePrivate(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES* securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

		internal unsafe static SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, ref SECURITY_ATTRIBUTES securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile)
		{
			lpFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
			fixed (SECURITY_ATTRIBUTES* securityAttrs2 = &securityAttrs)
			{
				IntPtr intPtr = CreateFilePrivate(lpFileName, dwDesiredAccess, dwShareMode, securityAttrs2, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
				try
				{
					return new SafeFileHandle(intPtr, ownsHandle: true);
				}
				catch
				{
					CloseHandle(intPtr);
					throw;
				}
			}
		}

		internal static SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, FileMode dwCreationDisposition, int dwFlagsAndAttributes)
		{
			IntPtr intPtr = CreateFile_IntPtr(lpFileName, dwDesiredAccess, dwShareMode, dwCreationDisposition, dwFlagsAndAttributes);
			try
			{
				return new SafeFileHandle(intPtr, ownsHandle: true);
			}
			catch
			{
				CloseHandle(intPtr);
				throw;
			}
		}

		internal unsafe static IntPtr CreateFile_IntPtr(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, FileMode dwCreationDisposition, int dwFlagsAndAttributes)
		{
			lpFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
			return CreateFilePrivate(lpFileName, dwDesiredAccess, dwShareMode, null, dwCreationDisposition, dwFlagsAndAttributes, IntPtr.Zero);
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal unsafe static extern bool ReadDirectoryChangesW(SafeFileHandle hDirectory, byte[] lpBuffer, uint nBufferLength, [MarshalAs(UnmanagedType.Bool)] bool bWatchSubtree, int dwNotifyFilter, out int lpBytesReturned, NativeOverlapped* lpOverlapped, IntPtr lpCompletionRoutine);
	}

	internal static class SspiCli
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct CredHandle
		{
			private IntPtr dwLower;

			private IntPtr dwUpper;

			public bool IsZero
			{
				get
				{
					if (dwLower == IntPtr.Zero)
					{
						return dwUpper == IntPtr.Zero;
					}
					return false;
				}
			}

			internal void SetToInvalid()
			{
				dwLower = IntPtr.Zero;
				dwUpper = IntPtr.Zero;
			}

			public override string ToString()
			{
				return dwLower.ToString("x") + ":" + dwUpper.ToString("x");
			}
		}

		internal enum ContextAttribute
		{
			SECPKG_ATTR_SIZES = 0,
			SECPKG_ATTR_NAMES = 1,
			SECPKG_ATTR_LIFESPAN = 2,
			SECPKG_ATTR_DCE_INFO = 3,
			SECPKG_ATTR_STREAM_SIZES = 4,
			SECPKG_ATTR_AUTHORITY = 6,
			SECPKG_ATTR_PACKAGE_INFO = 10,
			SECPKG_ATTR_NEGOTIATION_INFO = 12,
			SECPKG_ATTR_UNIQUE_BINDINGS = 25,
			SECPKG_ATTR_ENDPOINT_BINDINGS = 26,
			SECPKG_ATTR_CLIENT_SPECIFIED_TARGET = 27,
			SECPKG_ATTR_APPLICATION_PROTOCOL = 35,
			SECPKG_ATTR_REMOTE_CERT_CONTEXT = 83,
			SECPKG_ATTR_LOCAL_CERT_CONTEXT = 84,
			SECPKG_ATTR_ROOT_STORE = 85,
			SECPKG_ATTR_ISSUER_LIST_EX = 89,
			SECPKG_ATTR_CONNECTION_INFO = 90,
			SECPKG_ATTR_UI_INFO = 104
		}

		[Flags]
		internal enum ContextFlags
		{
			Zero = 0,
			Delegate = 1,
			MutualAuth = 2,
			ReplayDetect = 4,
			SequenceDetect = 8,
			Confidentiality = 0x10,
			UseSessionKey = 0x20,
			AllocateMemory = 0x100,
			Connection = 0x800,
			InitExtendedError = 0x4000,
			AcceptExtendedError = 0x8000,
			InitStream = 0x8000,
			AcceptStream = 0x10000,
			InitIntegrity = 0x10000,
			AcceptIntegrity = 0x20000,
			InitManualCredValidation = 0x80000,
			InitUseSuppliedCreds = 0x80,
			InitIdentify = 0x20000,
			AcceptIdentify = 0x80000,
			ProxyBindings = 0x4000000,
			AllowMissingBindings = 0x10000000,
			UnverifiedTargetName = 0x20000000
		}

		internal enum Endianness
		{
			SECURITY_NETWORK_DREP = 0,
			SECURITY_NATIVE_DREP = 0x10
		}

		internal enum CredentialUse
		{
			SECPKG_CRED_INBOUND = 1,
			SECPKG_CRED_OUTBOUND,
			SECPKG_CRED_BOTH
		}

		internal struct CERT_CHAIN_ELEMENT
		{
			public uint cbSize;

			public IntPtr pCertContext;
		}

		internal struct SecPkgContext_IssuerListInfoEx
		{
			public SafeHandle aIssuers;

			public uint cIssuers;

			public unsafe SecPkgContext_IssuerListInfoEx(SafeHandle handle, byte[] nativeBuffer)
			{
				aIssuers = handle;
				fixed (byte* ptr = nativeBuffer)
				{
					cIssuers = *(uint*)(ptr + IntPtr.Size);
				}
			}
		}

		internal struct SCHANNEL_CRED
		{
			[Flags]
			public enum Flags
			{
				Zero = 0,
				SCH_CRED_NO_SYSTEM_MAPPER = 2,
				SCH_CRED_NO_SERVERNAME_CHECK = 4,
				SCH_CRED_MANUAL_CRED_VALIDATION = 8,
				SCH_CRED_NO_DEFAULT_CREDS = 0x10,
				SCH_CRED_AUTO_CRED_VALIDATION = 0x20,
				SCH_SEND_AUX_RECORD = 0x200000,
				SCH_USE_STRONG_CRYPTO = 0x400000
			}

			public const int CurrentVersion = 4;

			public int dwVersion;

			public int cCreds;

			public IntPtr paCred;

			public IntPtr hRootStore;

			public int cMappers;

			public IntPtr aphMappers;

			public int cSupportedAlgs;

			public IntPtr palgSupportedAlgs;

			public int grbitEnabledProtocols;

			public int dwMinimumCipherStrength;

			public int dwMaximumCipherStrength;

			public int dwSessionLifespan;

			public Flags dwFlags;

			public int reserved;
		}

		internal struct SecBuffer
		{
			public int cbBuffer;

			public SecurityBufferType BufferType;

			public IntPtr pvBuffer;

			public unsafe static readonly int Size = sizeof(SecBuffer);
		}

		internal struct SecBufferDesc
		{
			public readonly int ulVersion;

			public readonly int cBuffers;

			public unsafe void* pBuffers;

			public unsafe SecBufferDesc(int count)
			{
				ulVersion = 0;
				cBuffers = count;
				pBuffers = null;
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct SEC_WINNT_AUTH_IDENTITY_W
		{
			internal string User;

			internal int UserLength;

			internal string Domain;

			internal int DomainLength;

			internal string Password;

			internal int PasswordLength;

			internal int Flags;
		}

		internal const uint SECQOP_WRAP_NO_ENCRYPT = 2147483649u;

		internal const int SEC_I_RENEGOTIATE = 590625;

		internal const int SECPKG_NEGOTIATION_COMPLETE = 0;

		internal const int SECPKG_NEGOTIATION_OPTIMISTIC = 1;

		[DllImport("sspicli.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern int EncryptMessage(ref CredHandle contextHandle, [In] uint qualityOfProtection, [In][Out] ref SecBufferDesc inputOutput, [In] uint sequenceNumber);

		[DllImport("sspicli.dll", ExactSpelling = true, SetLastError = true)]
		internal unsafe static extern int DecryptMessage([In] ref CredHandle contextHandle, [In][Out] ref SecBufferDesc inputOutput, [In] uint sequenceNumber, uint* qualityOfProtection);

		[DllImport("sspicli.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern int QuerySecurityContextToken(ref CredHandle phContext, out SecurityContextTokenHandle handle);

		[DllImport("sspicli.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern int FreeContextBuffer([In] IntPtr contextBuffer);

		[DllImport("sspicli.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern int FreeCredentialsHandle(ref CredHandle handlePtr);

		[DllImport("sspicli.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern int DeleteSecurityContext(ref CredHandle handlePtr);

		[DllImport("sspicli.dll", ExactSpelling = true, SetLastError = true)]
		internal unsafe static extern int AcceptSecurityContext(ref CredHandle credentialHandle, [In] void* inContextPtr, [In] SecBufferDesc* inputBuffer, [In] ContextFlags inFlags, [In] Endianness endianness, ref CredHandle outContextPtr, [In][Out] ref SecBufferDesc outputBuffer, [In][Out] ref ContextFlags attributes, out long timeStamp);

		[DllImport("sspicli.dll", ExactSpelling = true, SetLastError = true)]
		internal unsafe static extern int QueryContextAttributesW(ref CredHandle contextHandle, [In] ContextAttribute attribute, [In] void* buffer);

		[DllImport("sspicli.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern int SetContextAttributesW(ref CredHandle contextHandle, [In] ContextAttribute attribute, [In] byte[] buffer, [In] int bufferSize);

		[DllImport("sspicli.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern int EnumerateSecurityPackagesW(out int pkgnum, out SafeFreeContextBuffer_SECURITY handle);

		[DllImport("sspicli.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		internal unsafe static extern int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] ref SEC_WINNT_AUTH_IDENTITY_W authdata, [In] void* keyCallback, [In] void* keyArgument, ref CredHandle handlePtr, out long timeStamp);

		[DllImport("sspicli.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		internal unsafe static extern int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] IntPtr zero, [In] void* keyCallback, [In] void* keyArgument, ref CredHandle handlePtr, out long timeStamp);

		[DllImport("sspicli.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		internal unsafe static extern int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] SafeSspiAuthDataHandle authdata, [In] void* keyCallback, [In] void* keyArgument, ref CredHandle handlePtr, out long timeStamp);

		[DllImport("sspicli.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		internal unsafe static extern int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] ref SCHANNEL_CRED authData, [In] void* keyCallback, [In] void* keyArgument, ref CredHandle handlePtr, out long timeStamp);

		[DllImport("sspicli.dll", ExactSpelling = true, SetLastError = true)]
		internal unsafe static extern int InitializeSecurityContextW(ref CredHandle credentialHandle, [In] void* inContextPtr, [In] byte* targetName, [In] ContextFlags inFlags, [In] int reservedI, [In] Endianness endianness, [In] SecBufferDesc* inputBuffer, [In] int reservedII, ref CredHandle outContextPtr, [In][Out] ref SecBufferDesc outputBuffer, [In][Out] ref ContextFlags attributes, out long timeStamp);

		[DllImport("sspicli.dll", ExactSpelling = true, SetLastError = true)]
		internal unsafe static extern int CompleteAuthToken([In] void* inContextPtr, [In][Out] ref SecBufferDesc inputBuffers);

		[DllImport("sspicli.dll", ExactSpelling = true, SetLastError = true)]
		internal unsafe static extern int ApplyControlToken([In] void* inContextPtr, [In][Out] ref SecBufferDesc inputBuffers);

		[DllImport("sspicli.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern SECURITY_STATUS SspiFreeAuthIdentity([In] IntPtr authData);

		[DllImport("sspicli.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		internal static extern SECURITY_STATUS SspiEncodeStringsAsAuthIdentity([In] string userName, [In] string domainName, [In] string password, out SafeSspiAuthDataHandle authData);
	}
}
