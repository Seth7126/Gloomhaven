using System;
using System.Runtime.InteropServices;

namespace Sony.NP;

public class Trophies
{
	[StructLayout(LayoutKind.Sequential)]
	public class RegisterTrophyPackRequest : RequestBase
	{
		public RegisterTrophyPackRequest()
			: base(ServiceTypes.Trophy, FunctionTypes.TrophyRegisterTrophyPack)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class UnlockTrophyRequest : RequestBase
	{
		internal int trophyId;

		public int TrophyId
		{
			get
			{
				return trophyId;
			}
			set
			{
				trophyId = value;
			}
		}

		public UnlockTrophyRequest()
			: base(ServiceTypes.Trophy, FunctionTypes.TrophyUnlock)
		{
			trophyId = -1;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SetScreenshotRequest : RequestBase
	{
		public const int INVALID_TROPHY_ID = -1;

		public const int MAX_NUMBER_TROPHIES = 4;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		internal int[] trophiesIds;

		internal uint numTrophiesIds;

		[MarshalAs(UnmanagedType.I1)]
		internal bool assignToAllUsers;

		public int[] TrophiesIds
		{
			get
			{
				if (numTrophiesIds == 0)
				{
					return null;
				}
				int[] array = new int[numTrophiesIds];
				Array.Copy(trophiesIds, array, numTrophiesIds);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 4)
					{
						throw new NpToolkitException("The size of the TrophyIds array is more than " + 4);
					}
					value.CopyTo(trophiesIds, 0);
					numTrophiesIds = (uint)value.Length;
				}
				else
				{
					numTrophiesIds = 0u;
				}
			}
		}

		public bool AssignToAllUsers
		{
			get
			{
				return assignToAllUsers;
			}
			set
			{
				assignToAllUsers = value;
			}
		}

		public SetScreenshotRequest()
			: base(ServiceTypes.Trophy, FunctionTypes.TrophySetScreenshot)
		{
			trophiesIds = new int[4];
			numTrophiesIds = 0u;
			assignToAllUsers = true;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetUnlockedTrophiesRequest : RequestBase
	{
		public GetUnlockedTrophiesRequest()
			: base(ServiceTypes.Trophy, FunctionTypes.TrophyGetUnlockedTrophies)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class DisplayTrophyListDialogRequest : RequestBase
	{
		public DisplayTrophyListDialogRequest()
			: base(ServiceTypes.Trophy, FunctionTypes.TrophyDisplayTrophyListDialog)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetTrophyPackSummaryRequest : RequestBase
	{
		[MarshalAs(UnmanagedType.I1)]
		internal bool retrieveTrophyPackSummaryIcon;

		public bool RetrieveTrophyPackSummaryIcon
		{
			get
			{
				return retrieveTrophyPackSummaryIcon;
			}
			set
			{
				retrieveTrophyPackSummaryIcon = value;
			}
		}

		public GetTrophyPackSummaryRequest()
			: base(ServiceTypes.Trophy, FunctionTypes.TrophyGetTrophyPackSummary)
		{
			retrieveTrophyPackSummaryIcon = false;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetTrophyPackGroupRequest : RequestBase
	{
		internal int groupId;

		[MarshalAs(UnmanagedType.I1)]
		internal bool retrieveTrophyPackGroupIcon;

		public int GroupId
		{
			get
			{
				return groupId;
			}
			set
			{
				groupId = value;
			}
		}

		public bool RetrieveTrophyPackGroupIcon
		{
			get
			{
				return retrieveTrophyPackGroupIcon;
			}
			set
			{
				retrieveTrophyPackGroupIcon = value;
			}
		}

		public GetTrophyPackGroupRequest()
			: base(ServiceTypes.Trophy, FunctionTypes.TrophyGetTrophyPackGroup)
		{
			groupId = -1;
			retrieveTrophyPackGroupIcon = false;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetTrophyPackTrophyRequest : RequestBase
	{
		internal int trophyId;

		internal bool retrieveTrophyPackTrophyIcon;

		public int TrophyId
		{
			get
			{
				return trophyId;
			}
			set
			{
				trophyId = value;
			}
		}

		public bool RetrieveTrophyPackTrophyIcon
		{
			get
			{
				return retrieveTrophyPackTrophyIcon;
			}
			set
			{
				retrieveTrophyPackTrophyIcon = value;
			}
		}

		public GetTrophyPackTrophyRequest()
			: base(ServiceTypes.Trophy, FunctionTypes.TrophyGetTrophyPackTrophy)
		{
			trophyId = -1;
			retrieveTrophyPackTrophyIcon = false;
		}
	}

	public class UnlockedTrophiesResponse : ResponseBase
	{
		internal int[] trophyIds;

		public int[] TrophyIds => trophyIds;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.UnlockedTrophiesBegin);
			uint num = memoryBuffer.ReadUInt32();
			trophyIds = new int[num];
			for (int i = 0; i < num; i++)
			{
				trophyIds[i] = memoryBuffer.ReadInt32();
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.UnlockedTrophiesEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public struct NpTrophyGameDetails
	{
		internal uint numGroups;

		internal uint numTrophies;

		internal uint numPlatinum;

		internal uint numGold;

		internal uint numSilver;

		internal uint numBronze;

		internal string title;

		internal string description;

		public uint NumGroups => numGroups;

		public uint NumTrophies => numTrophies;

		public uint NumPlatinum => numPlatinum;

		public uint NumGold => numGold;

		public uint NumSilver => numSilver;

		public uint NumBronze => numBronze;

		public string Title => title;

		public string Description => description;

		internal void Read(MemoryBuffer buffer)
		{
			numGroups = buffer.ReadUInt32();
			numTrophies = buffer.ReadUInt32();
			numPlatinum = buffer.ReadUInt32();
			numGold = buffer.ReadUInt32();
			numSilver = buffer.ReadUInt32();
			numBronze = buffer.ReadUInt32();
			buffer.ReadString(ref title);
			buffer.ReadString(ref description);
		}
	}

	public struct NpTrophyGameData
	{
		internal uint unlockedTrophies;

		internal uint unlockedPlatinum;

		internal uint unlockedGold;

		internal uint unlockedSilver;

		internal uint unlockedBronze;

		internal uint progressPercentage;

		public uint UnlockedTrophies => unlockedTrophies;

		public uint UnlockedPlatinum => unlockedPlatinum;

		public uint UnlockedGold => unlockedGold;

		public uint UnlockedSilver => unlockedSilver;

		public uint UnlockedBronze => unlockedBronze;

		public uint ProgressPercentage => progressPercentage;

		internal void Read(MemoryBuffer buffer)
		{
			unlockedTrophies = buffer.ReadUInt32();
			unlockedPlatinum = buffer.ReadUInt32();
			unlockedGold = buffer.ReadUInt32();
			unlockedSilver = buffer.ReadUInt32();
			unlockedBronze = buffer.ReadUInt32();
			progressPercentage = buffer.ReadUInt32();
		}
	}

	public class TrophyPackSummaryResponse : ResponseBase
	{
		internal Icon icon = null;

		internal NpTrophyGameDetails staticConfiguration;

		internal NpTrophyGameData userProgress;

		public Icon Icon => icon;

		public NpTrophyGameDetails StaticConfiguration => staticConfiguration;

		public NpTrophyGameData UserProgress => userProgress;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TrophyPackSummaryBegin);
			icon = Icon.ReadAndCreate(memoryBuffer);
			staticConfiguration.Read(memoryBuffer);
			userProgress.Read(memoryBuffer);
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TrophyPackSummaryEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public struct NpTrophyGroupDetails
	{
		internal int groupId;

		internal uint numTrophies;

		internal uint numPlatinum;

		internal uint numGold;

		internal uint numSilver;

		internal uint numBronze;

		internal string title;

		internal string description;

		public int GroupId => groupId;

		public uint NumTrophies => numTrophies;

		public uint NumPlatinum => numPlatinum;

		public uint NumGold => numGold;

		public uint NumSilver => numSilver;

		public uint NumBronze => numBronze;

		public string Title => title;

		public string Description => description;

		internal void Read(MemoryBuffer buffer)
		{
			groupId = buffer.ReadInt32();
			numTrophies = buffer.ReadUInt32();
			numPlatinum = buffer.ReadUInt32();
			numGold = buffer.ReadUInt32();
			numSilver = buffer.ReadUInt32();
			numBronze = buffer.ReadUInt32();
			buffer.ReadString(ref title);
			buffer.ReadString(ref description);
		}
	}

	public struct NpTrophyGroupData
	{
		internal int groupId;

		internal uint unlockedTrophies;

		internal uint unlockedPlatinum;

		internal uint unlockedGold;

		internal uint unlockedSilver;

		internal uint unlockedBronze;

		internal uint progressPercentage;

		public int GroupId => groupId;

		public uint UnlockedTrophies => unlockedTrophies;

		public uint UnlockedPlatinum => unlockedPlatinum;

		public uint UnlockedGold => unlockedGold;

		public uint UnlockedSilver => unlockedSilver;

		public uint UnlockedBronze => unlockedBronze;

		public uint ProgressPercentage => progressPercentage;

		internal void Read(MemoryBuffer buffer)
		{
			groupId = buffer.ReadInt32();
			unlockedTrophies = buffer.ReadUInt32();
			unlockedPlatinum = buffer.ReadUInt32();
			unlockedGold = buffer.ReadUInt32();
			unlockedSilver = buffer.ReadUInt32();
			unlockedBronze = buffer.ReadUInt32();
			progressPercentage = buffer.ReadUInt32();
		}
	}

	public class TrophyPackGroupResponse : ResponseBase
	{
		internal Icon icon = null;

		internal NpTrophyGroupDetails staticConfiguration;

		internal NpTrophyGroupData userProgress;

		public Icon Icon => icon;

		public NpTrophyGroupDetails StaticConfiguration => staticConfiguration;

		public NpTrophyGroupData UserProgress => userProgress;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TrophyPackGroupBegin);
			icon = Icon.ReadAndCreate(memoryBuffer);
			staticConfiguration.Read(memoryBuffer);
			userProgress.Read(memoryBuffer);
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TrophyPackGroupEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public enum TrophyGrade
	{
		Unknown,
		Platinum,
		Gold,
		Silver,
		Bronze
	}

	public struct NpTrophyDetails
	{
		internal int trophyId;

		internal TrophyGrade trophyGrade;

		internal int groupId;

		internal bool hidden;

		internal string name;

		internal string description;

		public int TrophyId => trophyId;

		public TrophyGrade TrophyGrade => trophyGrade;

		public int GroupId => groupId;

		public bool Hidden => hidden;

		public string Name => name;

		public string Description => description;

		internal void Read(MemoryBuffer buffer)
		{
			trophyId = buffer.ReadInt32();
			trophyGrade = (TrophyGrade)buffer.ReadInt32();
			groupId = buffer.ReadInt32();
			hidden = buffer.ReadBool();
			buffer.ReadString(ref name);
			buffer.ReadString(ref description);
		}
	}

	public struct NpTrophyData
	{
		internal int trophyId;

		internal bool unlocked;

		internal DateTime timestamp;

		public int TrophyId => trophyId;

		public bool Unlocked => unlocked;

		public DateTime Timestamp => timestamp;

		internal void Read(MemoryBuffer buffer)
		{
			trophyId = buffer.ReadInt32();
			unlocked = buffer.ReadBool();
			timestamp = Core.ReadRtcTick(buffer);
		}
	}

	public class TrophyPackTrophyResponse : ResponseBase
	{
		internal Icon icon = null;

		internal NpTrophyDetails staticConfiguration;

		internal NpTrophyData userProgress;

		public Icon Icon => icon;

		public NpTrophyDetails StaticConfiguration => staticConfiguration;

		public NpTrophyData UserProgress => userProgress;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TrophyPackTrophyBegin);
			icon = Icon.ReadAndCreate(memoryBuffer);
			staticConfiguration.Read(memoryBuffer);
			userProgress.Read(memoryBuffer);
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TrophyPackTrophyEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxRegisterTrophyPack(RegisterTrophyPackRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxUnlockTrophy(UnlockTrophyRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxSetScreenshot(SetScreenshotRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetUnlockedTrophies(GetUnlockedTrophiesRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxDisplayTrophyListDialog(DisplayTrophyListDialogRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetTrophyPackSummary(GetTrophyPackSummaryRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetTrophyPackGroup(GetTrophyPackGroupRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetTrophyPackTrophy(GetTrophyPackTrophyRequest request, out APIResult result);

	public static int RegisterTrophyPack(RegisterTrophyPackRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxRegisterTrophyPack(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int SetScreenshot(SetScreenshotRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxSetScreenshot(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int UnlockTrophy(UnlockTrophyRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		if (request.trophyId < 0)
		{
			throw new NpToolkitException("Invalid trophy id has been used.");
		}
		APIResult result;
		int num = PrxUnlockTrophy(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetUnlockedTrophies(GetUnlockedTrophiesRequest request, UnlockedTrophiesResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetUnlockedTrophies(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int DisplayTrophyListDialog(DisplayTrophyListDialogRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxDisplayTrophyListDialog(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetTrophyPackSummary(GetTrophyPackSummaryRequest request, TrophyPackSummaryResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetTrophyPackSummary(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetTrophyPackGroup(GetTrophyPackGroupRequest request, TrophyPackGroupResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetTrophyPackGroup(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetTrophyPackTrophy(GetTrophyPackTrophyRequest request, TrophyPackTrophyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetTrophyPackTrophy(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}
}
