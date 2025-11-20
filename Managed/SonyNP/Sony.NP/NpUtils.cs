using System.Runtime.InteropServices;

namespace Sony.NP;

public class NpUtils
{
	[StructLayout(LayoutKind.Sequential)]
	public class SetTitleIdForDevelopmentRequest : RequestBase
	{
		private const int SCE_NP_TITLE_ID_LEN = 12;

		public string titleId;

		public string titleSecretString;

		public uint titleSecretStringSize;

		public SetTitleIdForDevelopmentRequest()
			: base(ServiceTypes.NpUtils, FunctionTypes.NpUtilsSetTitleIdForDevelopment)
		{
			titleId = "";
			titleSecretString = "";
			titleSecretStringSize = 0u;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class DisplaySigninDialogRequest : RequestBase
	{
		public DisplaySigninDialogRequest()
			: base(ServiceTypes.NpUtils, FunctionTypes.NpUtilsDisplaySigninDialog)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class CheckAvailablityRequest : RequestBase
	{
		public CheckAvailablityRequest()
			: base(ServiceTypes.NpUtils, FunctionTypes.NpUtilsCheckAvailability)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class CheckPlusRequest : RequestBase
	{
		internal ulong features;

		public CheckPlusRequest()
			: base(ServiceTypes.NpUtils, FunctionTypes.NpUtilsCheckPlus)
		{
			features = 1uL;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetParentalControlInfoRequest : RequestBase
	{
		public GetParentalControlInfoRequest()
			: base(ServiceTypes.NpUtils, FunctionTypes.NpUtilsGetParentalControlInfo)
		{
		}
	}

	public class CheckPlusResponse : ResponseBase
	{
		internal bool authorized;

		public bool Authorized => authorized;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.CheckPlusBegin);
			authorized = memoryBuffer.ReadBool();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.CheckPlusEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class GetParentalControlInfoResponse : ResponseBase
	{
		private int age;

		internal bool contentRestriction;

		internal bool chatRestriction;

		internal bool ugcRestriction;

		public int Age => age;

		public bool ChatRestriction => chatRestriction;

		public bool UGCRestriction => ugcRestriction;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GetParentalControlInfoBegin);
			age = memoryBuffer.ReadInt32();
			contentRestriction = memoryBuffer.ReadBool();
			chatRestriction = memoryBuffer.ReadBool();
			ugcRestriction = memoryBuffer.ReadBool();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GetParentalControlInfoEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public enum SignInState
	{
		unknown,
		signedOut,
		signedIn
	}

	public enum LogInState
	{
		loggedIn,
		loggedOut,
		unknown
	}

	public enum StateChanged
	{
		none,
		signedInState,
		loggedInState
	}

	public class UserStateChangeResponse : ResponseBase
	{
		internal Core.UserServiceUserId userId;

		internal SignInState currentSignInState;

		internal LogInState currentLogInState;

		internal StateChanged stateChanged;

		public Core.UserServiceUserId UserId => userId;

		public SignInState CurrentSignInState => currentSignInState;

		public LogInState CurrentLogInState => currentLogInState;

		public StateChanged StateChanged => stateChanged;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.UserStateChangeBegin);
			userId = memoryBuffer.ReadInt32();
			currentSignInState = (SignInState)memoryBuffer.ReadInt32();
			currentLogInState = (LogInState)memoryBuffer.ReadInt32();
			stateChanged = (StateChanged)memoryBuffer.ReadInt32();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.UserStateChangeEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxSetTitleIdForDevelopment(SetTitleIdForDevelopmentRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxDisplaySigninDialog(DisplaySigninDialogRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxCheckAvailablity(CheckAvailablityRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxCheckPlus(CheckPlusRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetParentalControlInfo(GetParentalControlInfoRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern void PrxNotifyPlusFeature(int userId, ulong features, out APIResult result);

	public static int SetTitleIdForDevelopment(SetTitleIdForDevelopmentRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxSetTitleIdForDevelopment(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int DisplaySigninDialog(DisplaySigninDialogRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxDisplaySigninDialog(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int CheckAvailablity(CheckAvailablityRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxCheckAvailablity(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int CheckPlus(CheckPlusRequest request, CheckPlusResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxCheckPlus(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetParentalControlInfo(GetParentalControlInfoRequest request, GetParentalControlInfoResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetParentalControlInfo(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static void NotifyPlusFeature(Core.UserServiceUserId userId)
	{
		PrxNotifyPlusFeature(userId.id, 1uL, out var result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
	}
}
