using System.Runtime.InteropServices;

namespace Sony.NP;

public class Auth
{
	public struct NpClientId
	{
		public const int NP_CLIENT_ID_MAX_LEN = 128;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
		internal string id;

		public string Id
		{
			get
			{
				return id;
			}
			set
			{
				if (value.Length > 128)
				{
					throw new NpToolkitException("The size of the string is more than " + 128 + " characters.");
				}
				id = value;
			}
		}
	}

	public struct NpClientSecret
	{
		public const int NP_CLIENT_SECRET_MAX_LEN = 256;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
		internal string secret;

		public string Secret
		{
			get
			{
				return secret;
			}
			set
			{
				if (value.Length > 256)
				{
					throw new NpToolkitException("The size of the string is more than " + 256 + " characters.");
				}
				secret = value;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetAuthCodeRequest : RequestBase
	{
		public const int MAX_SIZE_SCOPE = 511;

		internal NpClientId clientId;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
		internal string scope;

		public NpClientId ClientId
		{
			get
			{
				return clientId;
			}
			set
			{
				clientId = value;
			}
		}

		public string Scope
		{
			get
			{
				return scope;
			}
			set
			{
				if (value.Length > 511)
				{
					throw new NpToolkitException("The size of the string is more than " + 511 + " characters.");
				}
				scope = value;
			}
		}

		public GetAuthCodeRequest()
			: base(ServiceTypes.Auth, FunctionTypes.AuthGetAuthCode)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetIdTokenRequest : RequestBase
	{
		public const int MAX_SIZE_SCOPE = 511;

		internal NpClientId clientId;

		internal NpClientSecret clientSecret;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
		internal string scope;

		public NpClientId ClientId
		{
			get
			{
				return clientId;
			}
			set
			{
				clientId = value;
			}
		}

		public NpClientSecret ClientSecret
		{
			get
			{
				return clientSecret;
			}
			set
			{
				clientSecret = value;
			}
		}

		public string Scope
		{
			get
			{
				return scope;
			}
			set
			{
				if (value.Length > 511)
				{
					throw new NpToolkitException("The size of the string is more than " + 511 + " characters.");
				}
				scope = value;
			}
		}

		public GetIdTokenRequest()
			: base(ServiceTypes.Auth, FunctionTypes.AuthGetIdToken)
		{
		}
	}

	public enum IssuerIdType
	{
		Invalid = -1,
		Development = 1,
		Certification = 8,
		Live = 256
	}

	public class AuthCodeResponse : ResponseBase
	{
		internal string authCode;

		internal IssuerIdType issuerId;

		public string AuthCode => authCode;

		public IssuerIdType IssuerId => issuerId;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.AuthCodeBegin);
			memoryBuffer.ReadString(ref authCode);
			issuerId = (IssuerIdType)memoryBuffer.ReadUInt32();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.AuthCodeEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class IdTokenResponse : ResponseBase
	{
		internal string idToken;

		public string IdToken => idToken;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.IdTokenBegin);
			memoryBuffer.ReadString(ref idToken);
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.IdTokenEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetAuthCode(GetAuthCodeRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetIdToken(GetIdTokenRequest request, out APIResult result);

	public static int GetAuthCode(GetAuthCodeRequest request, AuthCodeResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetAuthCode(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetIdToken(GetIdTokenRequest request, IdTokenResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetIdToken(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}
}
