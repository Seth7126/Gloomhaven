using System;
using System.Runtime.InteropServices;

namespace Sony.NP;

public class ResponseBase
{
	internal int returnCode;

	internal bool locked;

	internal ServerErrorManaged serverError;

	public int ReturnCodeValue => returnCode;

	public Core.ReturnCodes ReturnCode => (Core.ReturnCodes)returnCode;

	public bool Locked => locked;

	public ServerErrorManaged ServerError => serverError;

	public bool IsErrorCode
	{
		get
		{
			if ((uint)returnCode >= 2181038080u && (uint)returnCode <= 2197815295u)
			{
				return true;
			}
			return false;
		}
	}

	public bool HasServerError => serverError != null;

	[DllImport("UnityNpToolkit2")]
	private static extern void PrxReadResponseBase(uint nptRequestId, int apiCalled, out int returnCode, out bool locked, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern void PrxReadResponseBaseLockedState(uint nptRequestId, int apiCalled, out bool locked, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern void PrxReadResponseCompleted(uint nptRequestId, int apiCalled, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxReadHasServerError(uint nptRequestId, int apiCalled, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern void PrxMarshalResponse(uint npRequestId, int apiCalled, out NpMemoryBuffer data, out APIResult result);

	internal ResponseBase()
	{
	}

	internal void PopulateFromNative(uint nptRequestId, FunctionTypes apiCalled, RequestBase request)
	{
		ReadResult(nptRequestId, apiCalled, request);
		int apiCalled2 = Compatibility.ConvertFromEnum(apiCalled);
		PrxReadResponseCompleted(nptRequestId, apiCalled2, out var result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
	}

	protected internal virtual void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
	{
		int apiCalled2 = Compatibility.ConvertFromEnum(apiCalled);
		PrxReadResponseBase(id, apiCalled2, out returnCode, out var flag, out var result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		if (PrxReadHasServerError(id, apiCalled2, out result))
		{
			serverError = new ServerErrorManaged();
			serverError.ReadResult(id, apiCalled);
		}
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		locked = flag;
	}

	internal void UpdateAsyncState(uint nptRequestId, FunctionTypes apiCalled)
	{
		int apiCalled2 = Compatibility.ConvertFromEnum(apiCalled);
		PrxReadResponseBaseLockedState(nptRequestId, apiCalled2, out locked, out var result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
	}

	internal MemoryBuffer BeginReadResponseBuffer(uint id, FunctionTypes apiCalled, out APIResult result)
	{
		NpMemoryBuffer data = default(NpMemoryBuffer);
		int apiCalled2 = Compatibility.ConvertFromEnum(apiCalled);
		PrxMarshalResponse(id, apiCalled2, out data, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		MemoryBuffer memoryBuffer = new MemoryBuffer(data);
		memoryBuffer.CheckStartMarker();
		return memoryBuffer;
	}

	internal void EndReadResponseBuffer(MemoryBuffer readBuffer)
	{
		readBuffer.CheckEndMarker();
	}

	public string ConvertReturnCodeToString(FunctionTypes apiCalled)
	{
		string text = "(0x" + returnCode.ToString("X8") + ")";
		Core.ReturnCodes returnCodes = (Core.ReturnCodes)returnCode;
		if (apiCalled != FunctionTypes.Invalid)
		{
			switch (apiCalled)
			{
			case FunctionTypes.CommerceDisplayCategoryBrowseDialog:
			case FunctionTypes.CommerceDisplayProductBrowseDialog:
			case FunctionTypes.CommerceDisplayVoucherCodeInputDialog:
			case FunctionTypes.CommerceDisplayCheckoutDialog:
			case FunctionTypes.CommerceDisplayJoinPlusDialog:
			case FunctionTypes.CommerceDisplayDownloadListDialog:
			case FunctionTypes.FriendsDisplayFriendRequestDialog:
			case FunctionTypes.FriendsDisplayBlockUserDialog:
			case FunctionTypes.NpUtilsDisplaySigninDialog:
			case FunctionTypes.SessionDisplayReceivedInvitationsDialog:
			case FunctionTypes.TrophyDisplayTrophyListDialog:
			case FunctionTypes.UserProfileDisplayUserProfileDialog:
			case FunctionTypes.UserProfileDisplayGriefReportingDialog:
			case FunctionTypes.MessagingDisplayReceivedGameDataMessagesDialog:
				switch (returnCodes)
				{
				case Core.ReturnCodes.SUCCESS:
					return text + " (DIALOG_RESULT_OK) ";
				case Core.ReturnCodes.DIALOG_RESULT_USER_CANCELED:
					return text + " (DIALOG_RESULT_USER_CANCELED) ";
				}
				break;
			case FunctionTypes.TrophyUnlock:
				if (returnCodes == Core.ReturnCodes.DIALOG_RESULT_USER_CANCELED)
				{
					return text + " (TROPHY_PLATINUM_UNLOCKED) ";
				}
				break;
			default:
				if (returnCode == 0)
				{
					return text += " (SUCCESS) ";
				}
				break;
			}
		}
		return (!Enum.IsDefined(typeof(Core.ReturnCodes), returnCodes)) ? (text + " (UNKNOWN) ") : (text + " (" + returnCodes.ToString() + ") ");
	}
}
