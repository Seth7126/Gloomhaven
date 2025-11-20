using System;
using System.Runtime.InteropServices;

namespace Sony.NP;

public class Presence
{
	[StructLayout(LayoutKind.Sequential)]
	public class DeletePresenceRequest : RequestBase
	{
		[MarshalAs(UnmanagedType.I1)]
		internal bool deleteGameData;

		[MarshalAs(UnmanagedType.I1)]
		internal bool deleteGameStatus;

		public bool DeleteGameData
		{
			get
			{
				return deleteGameData;
			}
			set
			{
				deleteGameData = value;
			}
		}

		public bool DeleteGameStatus
		{
			get
			{
				return deleteGameStatus;
			}
			set
			{
				deleteGameStatus = value;
			}
		}

		public DeletePresenceRequest()
			: base(ServiceTypes.Presence, FunctionTypes.PresenceDeletePresence)
		{
			deleteGameData = true;
			deleteGameStatus = true;
		}
	}

	public struct LocalizedGameStatus
	{
		public const int MAX_SIZE_LOCALIZED_GAME_STATUS = 96;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
		internal string languageCode;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 97)]
		internal string gameStatus;

		public Core.LanguageCode LanguageCode
		{
			get
			{
				Core.LanguageCode languageCode = new Core.LanguageCode();
				languageCode.code = this.languageCode;
				return languageCode;
			}
			set
			{
				languageCode = value.code;
			}
		}

		public string GameStatus
		{
			get
			{
				return gameStatus;
			}
			set
			{
				if (value.Length > 96)
				{
					throw new NpToolkitException("The size of the game stutus string is more than " + 96 + " characters.");
				}
				gameStatus = value;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SetPresenceRequest : RequestBase
	{
		public const int MAX_LOCALIZED_STATUSES = 50;

		public const int MAX_SIZE_GAME_DATA = 128;

		public const int MAX_SIZE_DEFAULT_GAME_STATUS = 191;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 191)]
		internal string defaultGameStatus;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
		internal LocalizedGameStatus[] localizedGameStatuses;

		internal uint numLocalizedGameStatuses;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
		internal byte[] binaryGameData;

		internal uint binaryGameDataSize;

		public string DefaultGameStatus
		{
			get
			{
				return defaultGameStatus;
			}
			set
			{
				if (value.Length > 191)
				{
					throw new NpToolkitException("The size of the default game stutus string is more than " + 191 + " characters.");
				}
				defaultGameStatus = value;
			}
		}

		public LocalizedGameStatus[] LocalizedGameStatuses
		{
			get
			{
				if (numLocalizedGameStatuses == 0)
				{
					return null;
				}
				LocalizedGameStatus[] array = new LocalizedGameStatus[numLocalizedGameStatuses];
				Array.Copy(localizedGameStatuses, array, numLocalizedGameStatuses);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 50)
					{
						throw new NpToolkitException("The size of the localized game statuses array is more than " + 50);
					}
					value.CopyTo(localizedGameStatuses, 0);
					numLocalizedGameStatuses = (uint)value.Length;
				}
				else
				{
					numLocalizedGameStatuses = 0u;
				}
			}
		}

		public byte[] BinaryGameData
		{
			get
			{
				if (binaryGameData == null)
				{
					return null;
				}
				byte[] array = new byte[binaryGameDataSize];
				Array.Copy(binaryGameData, array, binaryGameDataSize);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 128)
					{
						throw new NpToolkitException("The size of the binary game data is more than " + 128 + " bytes.");
					}
					value.CopyTo(binaryGameData, 0);
					binaryGameDataSize = (uint)value.Length;
				}
				else
				{
					binaryGameDataSize = 0u;
				}
			}
		}

		public SetPresenceRequest()
			: base(ServiceTypes.Presence, FunctionTypes.PresenceSetPresence)
		{
			defaultGameStatus = "";
			localizedGameStatuses = new LocalizedGameStatus[50];
			numLocalizedGameStatuses = 0u;
			binaryGameData = new byte[128];
			binaryGameDataSize = 0u;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetPresenceRequest : RequestBase
	{
		internal Core.NpAccountId fromUser;

		internal bool inContext;

		public Core.NpAccountId FromUser
		{
			get
			{
				return fromUser;
			}
			set
			{
				fromUser = value;
			}
		}

		public bool InContext
		{
			get
			{
				return inContext;
			}
			set
			{
				inContext = value;
			}
		}

		public GetPresenceRequest()
			: base(ServiceTypes.Presence, FunctionTypes.PresenceGetPresence)
		{
			fromUser.id = 0uL;
			inContext = true;
		}
	}

	public class PlatformPresence
	{
		public const int MAX_SIZE_TITLE_NAME = 127;

		public const int MAX_SIZE_GAME_STATUS = 191;

		public const int MAX_SIZE_GAME_DATA = 128;

		internal Core.OnlineStatus onlineStatusOnPlatform;

		internal Core.PlatformType platform;

		internal Core.TitleId titleId = new Core.TitleId();

		internal string titleName = "";

		internal string gameStatus = "";

		internal byte[] binaryGameData;

		public Core.OnlineStatus OnlineStatusOnPlatform => onlineStatusOnPlatform;

		public Core.PlatformType Platform => platform;

		public Core.TitleId TitleId => titleId;

		public string TitleName => titleName;

		public string GameStatus => gameStatus;

		public byte[] BinaryGameData => binaryGameData;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PlatformPresenceBegin);
			onlineStatusOnPlatform = (Core.OnlineStatus)buffer.ReadUInt32();
			platform = (Core.PlatformType)buffer.ReadUInt32();
			titleId.Read(buffer);
			buffer.ReadString(ref titleName);
			buffer.ReadString(ref gameStatus);
			buffer.ReadData(ref binaryGameData);
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PlatformPresenceEnd);
		}

		public override string ToString()
		{
			string text = "";
			return text + string.Format("\n: Platform Presence : OS ({0}) Platform ({1}) TitleId ({2}) ", onlineStatusOnPlatform, platform, titleId.ToString(), titleName);
		}
	}

	public class UserPresence
	{
		public const int MAX_NUM_PLATFORM_PRESENCE = 3;

		internal Core.OnlineUser user = new Core.OnlineUser();

		internal Core.OnlineStatus psnOnlineStatus;

		internal Core.PlatformType mostRelevantPlatform;

		internal PlatformPresence[] platforms;

		public Core.OnlineUser User => user;

		public Core.OnlineStatus PsnOnlineStatus => psnOnlineStatus;

		public Core.PlatformType MostRelevantPlatform => mostRelevantPlatform;

		public PlatformPresence[] Platforms => platforms;

		internal UserPresence()
		{
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PresenceBegin);
			user.Read(buffer);
			psnOnlineStatus = (Core.OnlineStatus)buffer.ReadUInt32();
			mostRelevantPlatform = (Core.PlatformType)buffer.ReadUInt32();
			uint num = buffer.ReadUInt32();
			if (num == 0)
			{
				platforms = null;
			}
			else
			{
				platforms = new PlatformPresence[num];
				for (int i = 0; i < num; i++)
				{
					platforms[i] = new PlatformPresence();
					platforms[i].Read(buffer);
				}
			}
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PresenceEnd);
		}

		public override string ToString()
		{
			string text = "";
			int num = 0;
			if (platforms != null)
			{
				num = platforms.Length;
			}
			text += $"0x{User.accountId:X} : {User.onlineId.name} : PSN OS ({PsnOnlineStatus}) MRP ({MostRelevantPlatform}) #P ({num})'\n";
			for (int i = 0; i < num; i++)
			{
				text = text + Platforms[i].ToString() + "\n";
			}
			return text;
		}
	}

	public class PresenceResponse : ResponseBase
	{
		internal UserPresence userPresence = new UserPresence();

		public UserPresence UserPresence => userPresence;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			userPresence.Read(memoryBuffer);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public enum PresenceUpdateType
	{
		invalid,
		gameTitle,
		gameStatus,
		gameData
	}

	public class PresenceUpdateResponse : ResponseBase
	{
		public const int MAX_SIZE_GAME_STATUS = 191;

		public const int MAX_SIZE_GAME_DATA = 128;

		internal Core.OnlineUser localUpdatedUser = new Core.OnlineUser();

		internal Core.OnlineUser remoteUser = new Core.OnlineUser();

		internal Core.UserServiceUserId userId;

		internal PresenceUpdateType updateType;

		internal string gameStatus = "";

		internal byte[] binaryGameData;

		internal Core.PlatformType platform;

		public Core.OnlineUser LocalUpdatedUser => localUpdatedUser;

		public Core.OnlineUser RemoteUser => remoteUser;

		public Core.UserServiceUserId UserId => userId;

		public PresenceUpdateType UpdateType => updateType;

		public string GameStatus
		{
			get
			{
				if (updateType != PresenceUpdateType.gameStatus)
				{
					throw new NpToolkitException("GameStatus isn't valid unless 'UpdateType' is set to " + PresenceUpdateType.gameStatus);
				}
				return gameStatus;
			}
		}

		public byte[] BinaryGameData
		{
			get
			{
				if (updateType != PresenceUpdateType.gameData)
				{
					throw new NpToolkitException("BinaryGameData isn't valid unless 'UpdateType' is set to " + PresenceUpdateType.gameData);
				}
				return binaryGameData;
			}
		}

		public Core.PlatformType Platform => platform;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PresenceUpdateBegin);
			localUpdatedUser.Read(memoryBuffer);
			remoteUser.Read(memoryBuffer);
			userId = memoryBuffer.ReadInt32();
			updateType = (PresenceUpdateType)memoryBuffer.ReadInt32();
			memoryBuffer.ReadString(ref gameStatus);
			memoryBuffer.ReadData(ref binaryGameData);
			platform = (Core.PlatformType)memoryBuffer.ReadInt32();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PresenceUpdateEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxDeletePresence(DeletePresenceRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxSetPresence(SetPresenceRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetPresence(GetPresenceRequest request, out APIResult result);

	public static int DeletePresence(DeletePresenceRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxDeletePresence(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int SetPresence(SetPresenceRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxSetPresence(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetPresence(GetPresenceRequest request, PresenceResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetPresence(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}
}
