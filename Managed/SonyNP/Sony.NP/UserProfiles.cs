using System;
using System.Runtime.InteropServices;

namespace Sony.NP;

public class UserProfiles
{
	public class LocalUsers
	{
		public const int MaxLocalUsers = 4;

		internal LocalLoginUserId[] localUsers = new LocalLoginUserId[4];

		public LocalLoginUserId[] LocalUsersIds => localUsers;
	}

	public struct LocalLoginUserId
	{
		internal Core.UserServiceUserId userId;

		internal Core.NpAccountId accountId;

		internal int sceErrorCode;

		public Core.UserServiceUserId UserId => userId;

		public Core.NpAccountId AccountId => accountId;

		public int SceErrorCode => sceErrorCode;
	}

	public class NpProfilesResponse : ResponseBase
	{
		internal Profiles.Profile[] profiles;

		public Profiles.Profile[] Profiles => profiles;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NpProfilesBegin);
			uint num = memoryBuffer.ReadUInt32();
			profiles = new Profiles.Profile[num];
			for (int i = 0; i < num; i++)
			{
				profiles[i] = new Profiles.Profile();
				profiles[i].Read(memoryBuffer);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NpProfilesEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetNpProfilesRquest : RequestBase
	{
		public const int MAX_SIZE_ACCOUNT_IDS = 50;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
		internal Core.NpAccountId[] accountIds;

		internal uint numValidAccountIds;

		public Core.NpAccountId[] AccountIds
		{
			get
			{
				if (numValidAccountIds == 0)
				{
					return null;
				}
				Core.NpAccountId[] array = new Core.NpAccountId[numValidAccountIds];
				Array.Copy(accountIds, array, numValidAccountIds);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 50)
					{
						throw new NpToolkitException("The size of the Account ids array is more than " + 50);
					}
					value.CopyTo(accountIds, 0);
					numValidAccountIds = (uint)value.Length;
				}
				else
				{
					numValidAccountIds = 0u;
				}
			}
		}

		public GetNpProfilesRquest()
			: base(ServiceTypes.UserProfile, FunctionTypes.UserProfileGetNpProfiles)
		{
			accountIds = new Core.NpAccountId[50];
			numValidAccountIds = 0u;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetVerifiedAccountsForTitleRequest : RequestBase
	{
		internal uint limit;

		public uint Limit
		{
			get
			{
				return limit;
			}
			set
			{
				limit = value;
			}
		}

		public GetVerifiedAccountsForTitleRequest()
			: base(ServiceTypes.UserProfile, FunctionTypes.UserProfileGetVerifiedAccountsForTitle)
		{
			limit = 10u;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class DisplayUserProfileDialogRequest : RequestBase
	{
		internal Core.NpAccountId targetAccountId;

		public Core.NpAccountId TargetAccountId
		{
			get
			{
				return targetAccountId;
			}
			set
			{
				targetAccountId = value;
			}
		}

		public DisplayUserProfileDialogRequest()
			: base(ServiceTypes.UserProfile, FunctionTypes.UserProfileDisplayUserProfileDialog)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class DisplayGriefReportingDialogRequest : RequestBase
	{
		public Core.NpAccountId targetAccountId;

		[MarshalAs(UnmanagedType.I1)]
		public bool reportOnlineId;

		[MarshalAs(UnmanagedType.I1)]
		public bool reportName;

		[MarshalAs(UnmanagedType.I1)]
		public bool reportPicture;

		[MarshalAs(UnmanagedType.I1)]
		public bool reportAboutMe;

		public Core.NpAccountId TargetAccountId
		{
			get
			{
				return targetAccountId;
			}
			set
			{
				targetAccountId = value;
			}
		}

		public bool ReportOnlineId
		{
			get
			{
				return reportOnlineId;
			}
			set
			{
				reportOnlineId = value;
			}
		}

		public bool ReportName
		{
			get
			{
				return reportName;
			}
			set
			{
				reportName = value;
			}
		}

		public bool ReportPicture
		{
			get
			{
				return reportPicture;
			}
			set
			{
				reportPicture = value;
			}
		}

		public bool ReportAboutMe
		{
			get
			{
				return reportAboutMe;
			}
			set
			{
				reportAboutMe = value;
			}
		}

		public DisplayGriefReportingDialogRequest()
			: base(ServiceTypes.UserProfile, FunctionTypes.UserProfileDisplayGriefReportingDialog)
		{
		}
	}

	[DllImport("UnityNpToolkit2")]
	private static extern void PrxGetLocalLoginUserIds([Out][MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] LocalLoginUserId[] users, int maxSize, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetNpProfiles(GetNpProfilesRquest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetVerifiedAccountsForTitle(GetVerifiedAccountsForTitleRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxDisplayUserProfileDialog(DisplayUserProfileDialogRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxDisplayGriefReportingDialog(DisplayGriefReportingDialogRequest request, out APIResult result);

	public static void GetLocalUsers(LocalUsers users)
	{
		PrxGetLocalLoginUserIds(users.localUsers, 4, out var result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
	}

	public static int GetNpProfiles(GetNpProfilesRquest request, NpProfilesResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetNpProfiles(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetVerifiedAccountsForTitle(GetVerifiedAccountsForTitleRequest request, NpProfilesResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetVerifiedAccountsForTitle(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int DisplayUserProfileDialog(DisplayUserProfileDialogRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxDisplayUserProfileDialog(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int DisplayGriefReportingDialog(DisplayGriefReportingDialogRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked.");
		}
		if (!request.reportAboutMe && !request.reportName && !request.reportOnlineId && !request.reportAboutMe)
		{
			throw new NpToolkitException("It is mandatory to specify at least one reason for the report.");
		}
		APIResult result;
		int num = PrxDisplayGriefReportingDialog(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}
}
