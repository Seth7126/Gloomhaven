using System;
using System.Runtime.InteropServices;

namespace Sony.NP;

public class Friends
{
	public class Friend
	{
		internal Profiles.Profile profile = new Profiles.Profile();

		internal Presence.UserPresence presence = new Presence.UserPresence();

		public Profiles.Profile Profile => profile;

		public Presence.UserPresence Presence => presence;

		public override string ToString()
		{
			string text = "Profile:\n" + profile.ToString();
			return text + "\nPresence:\n" + presence.ToString();
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.FriendBegin);
			profile.Read(buffer);
			presence.Read(buffer);
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.FriendEnd);
		}
	}

	public enum FriendsRetrievalModes
	{
		invalid,
		all,
		online,
		inContext,
		tryCached
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetFriendsRequest : RequestBase
	{
		internal FriendsRetrievalModes mode;

		internal uint limit;

		internal uint offset;

		public FriendsRetrievalModes Mode
		{
			get
			{
				return mode;
			}
			set
			{
				mode = value;
			}
		}

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

		public uint Offset
		{
			get
			{
				return offset;
			}
			set
			{
				offset = value;
			}
		}

		public GetFriendsRequest()
			: base(ServiceTypes.Friends, FunctionTypes.FriendsGetFriends)
		{
			mode = FriendsRetrievalModes.invalid;
			limit = 0u;
			offset = 0u;
		}
	}

	public class FriendsResponse : ResponseBase
	{
		internal Friend[] friends;

		public Friend[] Friends => friends;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.FriendsBegin);
			uint num = memoryBuffer.ReadUInt32();
			friends = new Friend[num];
			for (int i = 0; i < num; i++)
			{
				friends[i] = new Friend();
				friends[i].Read(memoryBuffer);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.FriendsEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class FriendsOfFriend
	{
		internal Core.OnlineUser originalFriend;

		internal Core.OnlineUser[] users;

		public Core.OnlineUser OriginalFriend => originalFriend;

		public Core.OnlineUser[] Users => users;

		public FriendsOfFriend()
		{
			originalFriend = new Core.OnlineUser();
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetFriendsOfFriendsRequest : RequestBase
	{
		public const int MAX_ACCOUNT_IDS = 10;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
		internal Core.NpAccountId[] accountIds;

		internal uint numAccountIds;

		public Core.NpAccountId[] AccountIds
		{
			get
			{
				if (numAccountIds == 0)
				{
					return null;
				}
				Core.NpAccountId[] array = new Core.NpAccountId[numAccountIds];
				Array.Copy(accountIds, array, numAccountIds);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 10)
					{
						throw new NpToolkitException("The size of the array is more than " + 10);
					}
					value.CopyTo(accountIds, 0);
					numAccountIds = (uint)value.Length;
				}
				else
				{
					numAccountIds = 0u;
				}
			}
		}

		public GetFriendsOfFriendsRequest()
			: base(ServiceTypes.Friends, FunctionTypes.FriendsGetFriendsOfFriends)
		{
			accountIds = new Core.NpAccountId[10];
			numAccountIds = 0u;
		}
	}

	public class FriendsOfFriendsResponse : ResponseBase
	{
		internal FriendsOfFriend[] friendsOfFriends;

		public FriendsOfFriend[] FriendsOfFriends => friendsOfFriends;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.FriendsOfFriendsBegin);
			uint num = memoryBuffer.ReadUInt32();
			friendsOfFriends = new FriendsOfFriend[num];
			for (int i = 0; i < num; i++)
			{
				friendsOfFriends[i] = new FriendsOfFriend();
				friendsOfFriends[i].originalFriend.Read(memoryBuffer);
				uint num2 = memoryBuffer.ReadUInt32();
				friendsOfFriends[i].users = new Core.OnlineUser[num2];
				for (int j = 0; j < num2; j++)
				{
					friendsOfFriends[i].users[j] = new Core.OnlineUser();
					friendsOfFriends[i].users[j].Read(memoryBuffer);
				}
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.FriendsOfFriendsEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public enum BlockedUsersRetrievalMode
	{
		invalid,
		all,
		tryCached
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetBlockedUsersRquest : RequestBase
	{
		internal BlockedUsersRetrievalMode mode;

		internal uint limit;

		internal uint offset;

		public BlockedUsersRetrievalMode Mode
		{
			get
			{
				return mode;
			}
			set
			{
				mode = value;
			}
		}

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

		public uint Offset
		{
			get
			{
				return offset;
			}
			set
			{
				offset = value;
			}
		}

		public GetBlockedUsersRquest()
			: base(ServiceTypes.Friends, FunctionTypes.FriendsGetBlockedUsers)
		{
			mode = BlockedUsersRetrievalMode.invalid;
			limit = 0u;
			offset = 0u;
		}
	}

	public class BlockedUsersResponse : ResponseBase
	{
		internal Core.OnlineUser[] users;

		public Core.OnlineUser[] Users => users;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.BlockedUsersBegin);
			uint num = memoryBuffer.ReadUInt32();
			users = new Core.OnlineUser[num];
			for (int i = 0; i < num; i++)
			{
				users[i] = new Core.OnlineUser();
				users[i].Read(memoryBuffer);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.BlockedUsersEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class DisplayFriendRequestDialogRequest : RequestBase
	{
		internal Core.NpAccountId targetUser;

		public Core.NpAccountId TargetUser
		{
			get
			{
				return targetUser;
			}
			set
			{
				targetUser = value;
			}
		}

		public DisplayFriendRequestDialogRequest()
			: base(ServiceTypes.Friends, FunctionTypes.FriendsDisplayFriendRequestDialog)
		{
			targetUser.id = 0uL;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class DisplayBlockUserDialogRequest : RequestBase
	{
		internal Core.NpAccountId targetUser;

		public Core.NpAccountId TargetUser
		{
			get
			{
				return targetUser;
			}
			set
			{
				targetUser = value;
			}
		}

		public DisplayBlockUserDialogRequest()
			: base(ServiceTypes.Friends, FunctionTypes.FriendsDisplayBlockUserDialog)
		{
			targetUser.id = 0uL;
		}
	}

	public enum FriendListUpdateEvents
	{
		none,
		friendAdded,
		friendRemoved,
		friendOnlineStatusChanged
	}

	public class FriendListUpdateResponse : ResponseBase
	{
		internal Core.OnlineUser localUpdatedUser = new Core.OnlineUser();

		internal Core.OnlineUser remoteUser = new Core.OnlineUser();

		internal Core.UserServiceUserId userId;

		internal FriendListUpdateEvents eventType;

		public Core.OnlineUser LocalUpdatedUser => localUpdatedUser;

		public Core.OnlineUser RemoteUser => remoteUser;

		public Core.UserServiceUserId UserId => userId;

		public FriendListUpdateEvents EventType => eventType;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.FriendListUpdateBegin);
			localUpdatedUser.Read(memoryBuffer);
			remoteUser.Read(memoryBuffer);
			userId = memoryBuffer.ReadInt32();
			eventType = (FriendListUpdateEvents)memoryBuffer.ReadInt32();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.FriendListUpdateEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class BlocklistUpdateResponse : ResponseBase
	{
		internal Core.OnlineUser localUpdatedUser = new Core.OnlineUser();

		internal Core.UserServiceUserId userId;

		public Core.OnlineUser LocalUpdatedUser => localUpdatedUser;

		public Core.UserServiceUserId UserId => userId;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.BlocklistUpdateBegin);
			localUpdatedUser.Read(memoryBuffer);
			userId = memoryBuffer.ReadInt32();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.BlocklistUpdateEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetFriends(GetFriendsRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetFriendsOfFriends(GetFriendsOfFriendsRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetBlockedUsers(GetBlockedUsersRquest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxDisplayFriendRequestDialog(DisplayFriendRequestDialogRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxDisplayBlockUserDialog(DisplayBlockUserDialogRequest request, out APIResult result);

	public static int GetFriends(GetFriendsRequest request, FriendsResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetFriends(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetFriendsOfFriends(GetFriendsOfFriendsRequest request, FriendsOfFriendsResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetFriendsOfFriends(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetBlockedUsers(GetBlockedUsersRquest request, BlockedUsersResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetBlockedUsers(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int DisplayFriendRequestDialog(DisplayFriendRequestDialogRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxDisplayFriendRequestDialog(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int DisplayBlockUserDialog(DisplayBlockUserDialogRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxDisplayBlockUserDialog(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}
}
