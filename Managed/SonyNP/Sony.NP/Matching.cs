using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Sony.NP;

public class Matching
{
	public enum AttributeType
	{
		Invalid,
		Integer,
		Binary
	}

	public enum AttributeScope
	{
		Invalid,
		Room,
		Member
	}

	public enum RoomAttributeVisibility
	{
		Invalid,
		Internal,
		External,
		Search
	}

	public enum RoomVisibility
	{
		Invalid,
		PublicRoom,
		PrivateRoom,
		ReserveSlots
	}

	public enum RoomMigrationType
	{
		OwnerBind,
		OwnerMigration
	}

	public enum TopologyType
	{
		Invalid,
		None,
		Mesh,
		Star
	}

	public struct AttributeMetadata
	{
		public const int MAX_SIZE_NAME = 31;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		internal string name;

		internal AttributeType type;

		internal AttributeScope scope;

		internal RoomAttributeVisibility roomAttributeVisibility;

		internal uint size;

		public string Name => name;

		public AttributeType Type => type;

		public AttributeScope Scope => scope;

		public RoomAttributeVisibility RoomVisibility => roomAttributeVisibility;

		public uint Size => size;

		private void InternalSetAttribute(string name, AttributeType type, AttributeScope scope, RoomAttributeVisibility roomAttributeVisibility, uint size)
		{
			if (name.Length > 31)
			{
				throw new NpToolkitException("Attribute " + name + " : The size of the name string is more than " + 31 + " characters.");
			}
			if (type == AttributeType.Invalid)
			{
				throw new NpToolkitException("Attribute " + name + " : Can't set an Invalid type.");
			}
			switch (scope)
			{
			case AttributeScope.Invalid:
				throw new NpToolkitException("Attribute " + name + " : Can't set an Invalid scope.");
			case AttributeScope.Room:
				if (roomAttributeVisibility == RoomAttributeVisibility.Invalid)
				{
					throw new NpToolkitException("Attribute " + name + " : Can't set an Invalid roomAttributeVisibility when Scope is Room.");
				}
				break;
			}
			if (type == AttributeType.Integer && size != 8)
			{
				throw new NpToolkitException("Attribute " + name + " : Integer attribute must be size 8.");
			}
			if (type == AttributeType.Binary && size > 256)
			{
				throw new NpToolkitException("Attribute " + name + " : Binary attribute size must not be more than " + 256);
			}
			if (scope == AttributeScope.Member && roomAttributeVisibility != RoomAttributeVisibility.Invalid)
			{
				throw new NpToolkitException("Attribute " + name + " : A Member attribute can't set a RoomAttributeVisibility of " + roomAttributeVisibility.ToString() + ". It must always be set to RoomAttributeVisibility.Invalid.");
			}
			if (roomAttributeVisibility == RoomAttributeVisibility.Search && type == AttributeType.Binary && size > 64)
			{
				throw new NpToolkitException("Attribute " + name + " : A Binary Search attribute can't be more than 64 bytes.");
			}
			this.name = name;
			this.type = type;
			this.scope = scope;
			this.roomAttributeVisibility = roomAttributeVisibility;
			this.size = size;
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.ReadString(ref name);
			type = (AttributeType)buffer.ReadUInt32();
			scope = (AttributeScope)buffer.ReadUInt32();
			roomAttributeVisibility = (RoomAttributeVisibility)buffer.ReadUInt32();
			size = buffer.ReadUInt32();
		}

		public static AttributeMetadata CreateIntegerAttribute(string name, AttributeScope scope, RoomAttributeVisibility roomAttributeVisibility)
		{
			AttributeMetadata result = default(AttributeMetadata);
			result.InternalSetAttribute(name, AttributeType.Integer, scope, roomAttributeVisibility, 8u);
			return result;
		}

		public static AttributeMetadata CreateBinaryAttribute(string name, AttributeScope scope, RoomAttributeVisibility roomAttributeVisibility, uint size)
		{
			AttributeMetadata result = default(AttributeMetadata);
			result.InternalSetAttribute(name, AttributeType.Binary, scope, roomAttributeVisibility, size);
			return result;
		}
	}

	public struct Attribute
	{
		public const int MAX_SIZE_BIN_VALUE = 256;

		internal AttributeMetadata metadata;

		internal int intValue;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		internal byte[] binValue;

		public AttributeMetadata Metadata => metadata;

		public int IntValue
		{
			get
			{
				if (metadata.type != AttributeType.Integer)
				{
					throw new NpToolkitException("Attribute " + metadata.name + " : This is not an interger attribute type.");
				}
				return intValue;
			}
			set
			{
				if (metadata.type != AttributeType.Integer)
				{
					throw new NpToolkitException("Attribute " + metadata.name + " : Expecting an interger attribute type.");
				}
				intValue = value;
			}
		}

		public byte[] BinValue
		{
			get
			{
				if (metadata.type != AttributeType.Binary)
				{
					throw new NpToolkitException("Attribute " + metadata.name + " : This is not a binary attribute type.");
				}
				if (metadata.size == 0)
				{
					return null;
				}
				byte[] destinationArray = new byte[metadata.size];
				Array.Copy(binValue, destinationArray, metadata.size);
				return binValue;
			}
			set
			{
				if (metadata.type != AttributeType.Binary)
				{
					throw new NpToolkitException("Attribute " + metadata.name + " : Expected a binary attribute type.");
				}
				if (value == null)
				{
					throw new NpToolkitException("Attribute " + metadata.name + " : Expected a non-null byte array.");
				}
				if (value.Length > 256)
				{
					throw new NpToolkitException("Attribute " + metadata.name + " : Binary array is more than " + 256);
				}
				if (value.Length > metadata.size)
				{
					throw new NpToolkitException("Attribute " + metadata.name + " : Array size of " + value.Length + " can't exceed " + metadata.size + " bytes defined in metadata.");
				}
				if (binValue == null)
				{
					binValue = new byte[256];
				}
				value.CopyTo(binValue, 0);
			}
		}

		internal void Read(MemoryBuffer buffer)
		{
			metadata.Read(buffer);
			if (metadata.type == AttributeType.Integer)
			{
				intValue = buffer.ReadInt32();
			}
			else if (metadata.type == AttributeType.Binary)
			{
				buffer.ReadData(ref binValue);
			}
		}

		public static Attribute CreateIntegerAttribute(AttributeMetadata metadata, int intValue)
		{
			return new Attribute
			{
				metadata = metadata,
				IntValue = intValue
			};
		}

		public static Attribute CreateBinaryAttribute(AttributeMetadata metadata, byte[] binValue)
		{
			return new Attribute
			{
				metadata = metadata,
				BinValue = binValue
			};
		}
	}

	public struct SessionImage
	{
		public const int IMAGE_PATH_MAX_LEN = 255;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		internal string sessionImgPath;

		public string SessionImgPath
		{
			get
			{
				return sessionImgPath;
			}
			set
			{
				if (value.Length > 255)
				{
					throw new NpToolkitException("The size of the image path string is more than " + 255 + " characters.");
				}
				sessionImgPath = value;
			}
		}

		internal bool IsValid()
		{
			if (sessionImgPath == null || sessionImgPath.Length == 0)
			{
				return false;
			}
			return true;
		}

		internal bool Exists()
		{
			if (sessionImgPath == null || sessionImgPath.Length == 0)
			{
				return false;
			}
			return true;
		}
	}

	public struct LocalizedSessionInfo
	{
		public const int SESSION_NAME_LEN = 63;

		public const int STATUS_LEN = 255;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		private string sessionName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		private string status;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
		internal string languageCode;

		public string SessionName
		{
			get
			{
				return sessionName;
			}
			set
			{
				if (value.Length > 63)
				{
					throw new NpToolkitException("The size of the session name is more than " + 63 + " characters.");
				}
				sessionName = value;
			}
		}

		public string Status
		{
			get
			{
				return status;
			}
			set
			{
				if (value.Length > 255)
				{
					throw new NpToolkitException("The size of the status string is more than " + 255 + " characters.");
				}
				status = value;
			}
		}

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

		public LocalizedSessionInfo(string sessionName, string status, Core.LanguageCode languageCode)
		{
			this.sessionName = "";
			this.status = "";
			this.languageCode = "";
			SessionName = sessionName;
			Status = status;
			LanguageCode = languageCode;
		}
	}

	public struct PresenceOptionData
	{
		public const int NP_MATCHING2_PRESENCE_OPTION_DATA_SIZE = 16;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		internal byte[] data;

		[MarshalAs(UnmanagedType.U1)]
		internal byte length;

		public byte[] Data
		{
			get
			{
				if (length == 0)
				{
					return null;
				}
				byte[] array = new byte[length];
				Array.Copy(data, array, length);
				return array;
			}
			set
			{
				if (data == null)
				{
					data = new byte[16];
				}
				if (value != null)
				{
					if (value.Length > 16)
					{
						throw new NpToolkitException("The size of the data array is more than " + 16);
					}
					value.CopyTo(data, 0);
					length = (byte)value.Length;
				}
				else
				{
					length = 0;
				}
			}
		}

		public string DataAsString
		{
			get
			{
				if (length == 0)
				{
					return "";
				}
				return Encoding.ASCII.GetString(data, 0, length);
			}
			set
			{
				if (data == null)
				{
					data = new byte[16];
				}
				if (value != null)
				{
					byte[] bytes = Encoding.ASCII.GetBytes(value);
					if (bytes.Length > 16)
					{
						throw new NpToolkitException("The size of the ASCII string is more than " + 16 + " characters.");
					}
					bytes.CopyTo(data, 0);
					length = (byte)bytes.Length;
				}
				else
				{
					if (data == null)
					{
						data = new byte[16];
					}
					length = 0;
				}
			}
		}

		internal void Init()
		{
			data = new byte[16];
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.ReadData(ref data);
		}
	}

	public enum SearchOperatorTypes
	{
		Invalid,
		Equals,
		NotEquals,
		LessThan,
		LessEqualsThan,
		GreaterThan,
		GreaterEqualsThan
	}

	public struct SearchClause
	{
		internal Attribute attributeToCompare;

		internal SearchOperatorTypes operatorType;

		public Attribute AttributeToCompare
		{
			get
			{
				return attributeToCompare;
			}
			set
			{
				attributeToCompare = value;
			}
		}

		public SearchOperatorTypes OperatorType
		{
			get
			{
				return operatorType;
			}
			set
			{
				operatorType = value;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SetInitConfigurationRequest : RequestBase
	{
		public const int MAX_ATTRIBUTES = 64;

		private ulong numAttributes;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		private AttributeMetadata[] attributes = new AttributeMetadata[64];

		public AttributeMetadata[] Attributes
		{
			get
			{
				if (numAttributes == 0)
				{
					return null;
				}
				AttributeMetadata[] array = new AttributeMetadata[numAttributes];
				Array.Copy(attributes, array, (int)numAttributes);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 64)
					{
						throw new NpToolkitException("The size of the array is more than " + 64);
					}
					value.CopyTo(attributes, 0);
					numAttributes = (uint)value.Length;
				}
				else
				{
					numAttributes = 0uL;
				}
				ValidateAttributes();
			}
		}

		private void ValidateAttributes()
		{
			uint num = 0u;
			uint num2 = 0u;
			uint num3 = 0u;
			uint num4 = 0u;
			uint num5 = 0u;
			for (ulong num6 = 0uL; num6 < numAttributes; num6++)
			{
				if (attributes[num6].scope == AttributeScope.Member)
				{
					num += attributes[num6].size;
					continue;
				}
				if (attributes[num6].scope == AttributeScope.Room)
				{
					if (attributes[num6].roomAttributeVisibility == RoomAttributeVisibility.Internal)
					{
						num2 += attributes[num6].size;
						continue;
					}
					if (attributes[num6].roomAttributeVisibility == RoomAttributeVisibility.External)
					{
						num3 += attributes[num6].size;
						continue;
					}
					if (attributes[num6].roomAttributeVisibility == RoomAttributeVisibility.Search)
					{
						if (attributes[num6].type == AttributeType.Binary)
						{
							num4++;
							continue;
						}
						if (attributes[num6].type == AttributeType.Integer)
						{
							num5++;
							continue;
						}
						throw new NpToolkitException("Attribute " + attributes[num6].name + " : Type is not set to either Binary or Integer.");
					}
					throw new NpToolkitException("Attribute " + attributes[num6].name + " : RoomAttributeVisibility is not set to either Internal or External.");
				}
				throw new NpToolkitException("Attribute " + attributes[num6].name + " : " + num6 + " : Scope is not set to either Member or Room.");
			}
			if (num > 64)
			{
				throw new NpToolkitException("The sum of all member attributes has to be a max of 64.");
			}
			if (num2 > 448)
			{
				throw new NpToolkitException("The sum of all internal room attributes has to be a max of 448 bytes. ");
			}
			if (num3 > 448)
			{
				throw new NpToolkitException("The sum of all external room attributes has to be a max of 448 bytes");
			}
			if (num4 > 1)
			{
				throw new NpToolkitException("Only 1 binary search variable is permitted.");
			}
			if (num5 > 8)
			{
				throw new NpToolkitException("Only 8 interger search variables are permitted.");
			}
		}

		public SetInitConfigurationRequest()
			: base(ServiceTypes.Matching, FunctionTypes.MatchingSetInitConfiguration)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetWorldsRequest : RequestBase
	{
		public GetWorldsRequest()
			: base(ServiceTypes.Matching, FunctionTypes.MatchingGetWorlds)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class CreateRoomRequest : RequestBase
	{
		public const int MAX_ATTRIBUTES = 64;

		public const int MAX_SIZE_ROOM_NAME = 63;

		public const int MAX_SIZE_ROOM_STATUS = 255;

		public const int MAX_SIZE_FIXED_DATA = 1047552;

		public const int MAX_SIZE_CHANGEABLE_DATA = 1024;

		public const int MAX_SIZE_LOCALIZATIONS = 10;

		internal ulong numAttributes;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		internal Attribute[] attributes = new Attribute[64];

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		internal string name;

		internal NpMatching2SessionPassword password;

		internal RoomVisibility visibility;

		internal uint numReservedSlots;

		internal ulong fixedDataSize;

		[MarshalAs(UnmanagedType.LPArray)]
		internal byte[] fixedData;

		internal ulong changeableDataSize;

		[MarshalAs(UnmanagedType.LPArray)]
		internal byte[] changeableData;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		internal string status;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
		internal LocalizedSessionInfo[] localizations = new LocalizedSessionInfo[10];

		internal SessionImage image;

		internal RoomMigrationType ownershipMigration;

		internal TopologyType topology;

		internal uint maxNumMembers;

		internal NpMatching2WorldNumber worldNumber;

		[MarshalAs(UnmanagedType.I1)]
		internal bool displayOnSystem;

		[MarshalAs(UnmanagedType.I1)]
		internal bool isSystemJoinable;

		[MarshalAs(UnmanagedType.I1)]
		internal bool joinAllLocalUsers;

		[MarshalAs(UnmanagedType.I1)]
		internal bool isNatRestricted;

		[MarshalAs(UnmanagedType.I1)]
		internal bool isCrossplatform;

		[MarshalAs(UnmanagedType.I1)]
		internal bool allowBlockedUsersOfOwner;

		[MarshalAs(UnmanagedType.I1)]
		internal bool allowBlockedUsersOfMembers;

		public Attribute[] Attributes
		{
			get
			{
				if (numAttributes == 0)
				{
					return null;
				}
				Attribute[] array = new Attribute[numAttributes];
				Array.Copy(attributes, array, (int)numAttributes);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 64)
					{
						throw new NpToolkitException("The size of the array is more than " + 64);
					}
					value.CopyTo(attributes, 0);
					numAttributes = (uint)value.Length;
				}
				else
				{
					numAttributes = 0uL;
				}
			}
		}

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				if (value.Length > 63)
				{
					throw new NpToolkitException("The size of the name string is more than " + 63 + " characters.");
				}
				name = value;
			}
		}

		public NpMatching2SessionPassword Password
		{
			get
			{
				return password;
			}
			set
			{
				password = value;
			}
		}

		public RoomVisibility Visibility
		{
			get
			{
				return visibility;
			}
			set
			{
				visibility = value;
			}
		}

		public uint NumReservedSlots
		{
			get
			{
				return numReservedSlots;
			}
			set
			{
				numReservedSlots = value;
			}
		}

		public byte[] FixedData
		{
			get
			{
				return fixedData;
			}
			set
			{
				if (value.Length > 1047552)
				{
					throw new NpToolkitException("The size of the fixed data array is more than " + 1047552 + " bytes.");
				}
				fixedData = value;
				fixedDataSize = (ulong)((value != null) ? value.Length : 0);
			}
		}

		public byte[] ChangeableData
		{
			get
			{
				return changeableData;
			}
			set
			{
				if (value.Length > 1024)
				{
					throw new NpToolkitException("The size of the changeable data array is more than " + 1024 + " bytes.");
				}
				changeableData = value;
				changeableDataSize = (ulong)((value != null) ? value.Length : 0);
			}
		}

		public string Status
		{
			get
			{
				return status;
			}
			set
			{
				if (value.Length > 255)
				{
					throw new NpToolkitException("The size of the status string is more than " + 255 + " characters.");
				}
				status = value;
			}
		}

		public LocalizedSessionInfo[] Localizations
		{
			get
			{
				LocalizedSessionInfo[] array = new LocalizedSessionInfo[10];
				Array.Copy(localizations, array, 10);
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
					value.CopyTo(localizations, 0);
				}
			}
		}

		public SessionImage Image
		{
			get
			{
				return image;
			}
			set
			{
				image = value;
			}
		}

		public bool DisplayOnSystem
		{
			get
			{
				return displayOnSystem;
			}
			set
			{
				displayOnSystem = value;
			}
		}

		public bool IsSystemJoinable
		{
			get
			{
				return isSystemJoinable;
			}
			set
			{
				isSystemJoinable = value;
			}
		}

		public bool JoinAllLocalUsers
		{
			get
			{
				return joinAllLocalUsers;
			}
			set
			{
				joinAllLocalUsers = value;
			}
		}

		public bool IsNatRestricted
		{
			get
			{
				return isNatRestricted;
			}
			set
			{
				isNatRestricted = value;
			}
		}

		public RoomMigrationType OwnershipMigration
		{
			get
			{
				return ownershipMigration;
			}
			set
			{
				ownershipMigration = value;
			}
		}

		public TopologyType Topology
		{
			get
			{
				return topology;
			}
			set
			{
				topology = value;
			}
		}

		public uint MaxNumMembers
		{
			get
			{
				return maxNumMembers;
			}
			set
			{
				maxNumMembers = value;
			}
		}

		public NpMatching2WorldNumber WorldNumber
		{
			get
			{
				return worldNumber;
			}
			set
			{
				worldNumber = value;
			}
		}

		public bool IsCrossplatform
		{
			get
			{
				return isCrossplatform;
			}
			set
			{
				isCrossplatform = value;
			}
		}

		public bool AllowBlockedUsersOfOwner
		{
			get
			{
				return allowBlockedUsersOfOwner;
			}
			set
			{
				allowBlockedUsersOfOwner = value;
			}
		}

		public bool AllowBlockedUsersOfMembers
		{
			get
			{
				return allowBlockedUsersOfMembers;
			}
			set
			{
				allowBlockedUsersOfMembers = value;
			}
		}

		public CreateRoomRequest()
			: base(ServiceTypes.Matching, FunctionTypes.MatchingCreateRoom)
		{
			numReservedSlots = 0u;
			displayOnSystem = true;
			isSystemJoinable = true;
			joinAllLocalUsers = false;
			isNatRestricted = false;
			ownershipMigration = RoomMigrationType.OwnerBind;
			topology = TopologyType.None;
			worldNumber.num = 1;
			isCrossplatform = false;
			allowBlockedUsersOfOwner = false;
			allowBlockedUsersOfMembers = true;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class LeaveRoomRequest : RequestBase
	{
		internal ulong roomId;

		internal PresenceOptionData notificationDataToMembers;

		public ulong RoomId
		{
			get
			{
				return roomId;
			}
			set
			{
				roomId = value;
			}
		}

		public PresenceOptionData NotificationDataToMembers
		{
			get
			{
				return notificationDataToMembers;
			}
			set
			{
				notificationDataToMembers = value;
			}
		}

		public LeaveRoomRequest()
			: base(ServiceTypes.Matching, FunctionTypes.MatchingLeaveRoom)
		{
			notificationDataToMembers.Init();
		}
	}

	public enum RoomsSearchScope
	{
		All,
		FriendsRooms,
		RecentlyMetRooms,
		CustomUsersList
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SearchRoomsRequest : RequestBase
	{
		public const int MAX_SEARCH_CLAUSES = 64;

		public const int MAX_PAGE_SIZE = 20;

		public const int MIN_OFFSET = 1;

		public const int MAX_NUM_USERS_TO_SEARCH_IN_ROOMS = 20;

		internal ulong numSearchClauses;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		internal SearchClause[] searchClauses = new SearchClause[64];

		internal ulong numUsersToSearchInRooms;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
		internal Core.NpAccountId[] usersToSearchInRooms = new Core.NpAccountId[20];

		internal int offset;

		internal int pageSize;

		internal RoomsSearchScope searchScope;

		internal NpMatching2WorldNumber worldNumber;

		[MarshalAs(UnmanagedType.I1)]
		internal bool provideRandomRooms;

		[MarshalAs(UnmanagedType.I1)]
		internal bool quickJoin;

		[MarshalAs(UnmanagedType.I1)]
		internal bool applyNatTypeFilter;

		public SearchClause[] SearchClauses
		{
			get
			{
				if (numSearchClauses == 0)
				{
					return null;
				}
				SearchClause[] array = new SearchClause[numSearchClauses];
				Array.Copy(searchClauses, array, (int)numSearchClauses);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 64)
					{
						throw new NpToolkitException("The size of the array is more than " + 64);
					}
					value.CopyTo(searchClauses, 0);
					numSearchClauses = (uint)value.Length;
				}
				else
				{
					numSearchClauses = 0uL;
				}
			}
		}

		public Core.NpAccountId[] UsersToSearchInRooms
		{
			get
			{
				if (numUsersToSearchInRooms == 0)
				{
					return null;
				}
				Core.NpAccountId[] array = new Core.NpAccountId[numUsersToSearchInRooms];
				Array.Copy(usersToSearchInRooms, array, (int)numUsersToSearchInRooms);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 20)
					{
						throw new NpToolkitException("The size of the array is larger than " + 20);
					}
					value.CopyTo(usersToSearchInRooms, 0);
					numUsersToSearchInRooms = (ulong)value.Length;
				}
				else
				{
					numUsersToSearchInRooms = 0uL;
				}
			}
		}

		public int Offset
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

		public int PageSize
		{
			get
			{
				return pageSize;
			}
			set
			{
				pageSize = value;
			}
		}

		public RoomsSearchScope SearchScope
		{
			get
			{
				return searchScope;
			}
			set
			{
				searchScope = value;
			}
		}

		public NpMatching2WorldNumber WorldNumber
		{
			get
			{
				return worldNumber;
			}
			set
			{
				worldNumber = value;
			}
		}

		public bool ProvideRandomRooms
		{
			get
			{
				return provideRandomRooms;
			}
			set
			{
				provideRandomRooms = value;
			}
		}

		public bool QuickJoin
		{
			get
			{
				return quickJoin;
			}
			set
			{
				quickJoin = value;
			}
		}

		public bool ApplyNatTypeFilter
		{
			get
			{
				return applyNatTypeFilter;
			}
			set
			{
				applyNatTypeFilter = value;
			}
		}

		public SearchRoomsRequest()
			: base(ServiceTypes.Matching, FunctionTypes.MatchingSearchRooms)
		{
			offset = 1;
			pageSize = 20;
			worldNumber.num = 1;
			provideRandomRooms = false;
			quickJoin = false;
			applyNatTypeFilter = true;
		}
	}

	public enum RoomJoiningType
	{
		Room,
		BoundSessionId
	}

	[StructLayout(LayoutKind.Sequential)]
	public class JoinRoomRequest : RequestBase
	{
		public const int MAX_ATTRIBUTES = 64;

		internal NpMatching2SessionPassword password;

		internal ulong numMemberAttributes;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		internal Attribute[] memberAttributes = new Attribute[64];

		internal PresenceOptionData notificationDataToMembers;

		internal ulong roomId;

		internal NpSessionId boundSessionId;

		internal RoomJoiningType identifyRoomBy;

		[MarshalAs(UnmanagedType.I1)]
		internal bool joinAllLocalUsers;

		[MarshalAs(UnmanagedType.I1)]
		internal bool allowBlockedUsers;

		public NpMatching2SessionPassword Password
		{
			get
			{
				return password;
			}
			set
			{
				password = value;
			}
		}

		public Attribute[] MemberAttributes
		{
			get
			{
				if (numMemberAttributes == 0)
				{
					return null;
				}
				Attribute[] array = new Attribute[numMemberAttributes];
				Array.Copy(memberAttributes, array, (int)numMemberAttributes);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 64)
					{
						throw new NpToolkitException("The size of the array is larger than " + 64);
					}
					value.CopyTo(memberAttributes, 0);
					numMemberAttributes = (ulong)value.Length;
				}
				else
				{
					numMemberAttributes = 0uL;
				}
			}
		}

		public PresenceOptionData NotificationDataToMembers
		{
			get
			{
				return notificationDataToMembers;
			}
			set
			{
				notificationDataToMembers = value;
			}
		}

		public ulong RoomId
		{
			get
			{
				return roomId;
			}
			set
			{
				if (identifyRoomBy != RoomJoiningType.Room)
				{
					throw new NpToolkitException("Can't set RoomId if IdentifyRoomBy isn't RoomJoiningType.Room.");
				}
				roomId = value;
			}
		}

		public NpSessionId BoundSessionId
		{
			get
			{
				return boundSessionId;
			}
			set
			{
				if (identifyRoomBy != RoomJoiningType.BoundSessionId)
				{
					throw new NpToolkitException("Can't set BoundSessionId if IdentifyRoomBy isn't RoomJoiningType.BoundSessionId.");
				}
				boundSessionId = value;
			}
		}

		public RoomJoiningType IdentifyRoomBy
		{
			get
			{
				return identifyRoomBy;
			}
			set
			{
				identifyRoomBy = value;
			}
		}

		public bool JoinAllLocalUsers
		{
			get
			{
				return joinAllLocalUsers;
			}
			set
			{
				joinAllLocalUsers = value;
			}
		}

		public bool AllowBlockedUsers
		{
			get
			{
				return allowBlockedUsers;
			}
			set
			{
				allowBlockedUsers = value;
			}
		}

		public JoinRoomRequest()
			: base(ServiceTypes.Matching, FunctionTypes.MatchingJoinRoom)
		{
			joinAllLocalUsers = false;
			allowBlockedUsers = true;
			notificationDataToMembers.Init();
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetRoomPingTimeRequest : RequestBase
	{
		internal ulong roomId;

		public ulong RoomId
		{
			get
			{
				return roomId;
			}
			set
			{
				roomId = value;
			}
		}

		public GetRoomPingTimeRequest()
			: base(ServiceTypes.Matching, FunctionTypes.MatchingGetRoomPingTime)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class KickOutRoomMemberRequest : RequestBase
	{
		internal ulong roomId;

		internal PresenceOptionData notificationDataToMembers;

		internal ushort memberId;

		[MarshalAs(UnmanagedType.I1)]
		internal bool allowRejoin;

		public ulong RoomId
		{
			get
			{
				return roomId;
			}
			set
			{
				roomId = value;
			}
		}

		public PresenceOptionData NotificationDataToMembers
		{
			get
			{
				return notificationDataToMembers;
			}
			set
			{
				if (value.data == null || value.length != 16)
				{
					notificationDataToMembers.Init();
				}
				notificationDataToMembers = value;
			}
		}

		public ushort MemberId
		{
			get
			{
				return memberId;
			}
			set
			{
				memberId = value;
			}
		}

		public bool AllowRejoin
		{
			get
			{
				return allowRejoin;
			}
			set
			{
				allowRejoin = value;
			}
		}

		public KickOutRoomMemberRequest()
			: base(ServiceTypes.Matching, FunctionTypes.MatchingKickOutRoomMember)
		{
			notificationDataToMembers.Init();
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SendRoomMessageRequest : RequestBase
	{
		public const int MESSAGE_MAX_SIZE = 1023;

		public const int MAX_MEMBERS = 32;

		internal ulong roomId;

		internal ulong numMembers;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		internal ushort[] members = new ushort[32];

		internal ulong dataSize;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
		internal byte[] data = new byte[1024];

		[MarshalAs(UnmanagedType.I1)]
		internal bool isChatMsg;

		public ulong RoomId
		{
			get
			{
				return roomId;
			}
			set
			{
				roomId = value;
			}
		}

		public ushort[] Members
		{
			get
			{
				if (numMembers == 0)
				{
					return null;
				}
				ushort[] array = new ushort[numMembers];
				Array.Copy(members, array, (int)numMembers);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 32)
					{
						throw new NpToolkitException("The size of the array is larger than " + 32);
					}
					value.CopyTo(members, 0);
					numMembers = (ulong)value.Length;
				}
				else
				{
					numMembers = 0uL;
				}
			}
		}

		public byte[] Data
		{
			get
			{
				if (dataSize == 0)
				{
					return null;
				}
				byte[] array = new byte[dataSize];
				Array.Copy(data, array, (int)dataSize);
				return array;
			}
			set
			{
				if (data == null)
				{
					data = new byte[1023];
				}
				if (value != null)
				{
					if (value.Length > 1023)
					{
						throw new NpToolkitException("The size of the data array is more than " + 1023);
					}
					value.CopyTo(data, 0);
					dataSize = (byte)value.Length;
				}
				else
				{
					dataSize = 0uL;
				}
			}
		}

		public string DataAsString
		{
			get
			{
				if (dataSize == 0)
				{
					return "";
				}
				return Encoding.UTF8.GetString(data, 0, (int)dataSize);
			}
			set
			{
				if (data == null)
				{
					data = new byte[1023];
				}
				if (value != null)
				{
					byte[] bytes = Encoding.UTF8.GetBytes(value);
					if (bytes.Length > 1023)
					{
						throw new NpToolkitException("The size of the string is more than " + 1023 + " bytes.");
					}
					bytes.CopyTo(data, 0);
					dataSize = (byte)bytes.Length;
				}
				else
				{
					if (data == null)
					{
						data = new byte[1023];
					}
					dataSize = 0uL;
				}
			}
		}

		public bool IsChatMsg
		{
			get
			{
				return isChatMsg;
			}
			set
			{
				isChatMsg = value;
			}
		}

		public SendRoomMessageRequest()
			: base(ServiceTypes.Matching, FunctionTypes.MatchingSendRoomMessage)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetAttributesRequest : RequestBase
	{
		internal ulong roomId;

		internal AttributeScope scope;

		internal RoomAttributeVisibility roomAttributeVisibility;

		internal ushort memberId;

		public ulong RoomId
		{
			get
			{
				return roomId;
			}
			set
			{
				roomId = value;
			}
		}

		public AttributeScope Scope => scope;

		public RoomAttributeVisibility RoomAttributeVisibility
		{
			get
			{
				return roomAttributeVisibility;
			}
			set
			{
				roomAttributeVisibility = value;
				scope = AttributeScope.Room;
			}
		}

		public ushort MemberId
		{
			get
			{
				return memberId;
			}
			set
			{
				memberId = value;
				scope = AttributeScope.Member;
			}
		}

		public GetAttributesRequest()
			: base(ServiceTypes.Matching, FunctionTypes.MatchingGetAttributes)
		{
		}
	}

	public enum DataType
	{
		Fixed,
		Changeable
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetDataRequest : RequestBase
	{
		internal ulong roomId;

		internal DataType type;

		public ulong RoomId
		{
			get
			{
				return roomId;
			}
			set
			{
				roomId = value;
			}
		}

		public DataType Type
		{
			get
			{
				return type;
			}
			set
			{
				type = value;
			}
		}

		public GetDataRequest()
			: base(ServiceTypes.Matching, FunctionTypes.MatchingGetData)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SetMembersAsRecentlyMetRequest : RequestBase
	{
		public const int NUM_RECENTLY_MET_MAX_LEN = 19;

		public const int TEXT_MAX_LEN = 2083;

		internal ulong numMembers;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
		internal ushort[] members = new ushort[19];

		internal ulong roomId;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2084)]
		internal string text;

		public ushort[] Members
		{
			get
			{
				if (numMembers == 0)
				{
					return null;
				}
				ushort[] array = new ushort[numMembers];
				Array.Copy(members, array, (int)numMembers);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 19)
					{
						throw new NpToolkitException("The size of the array is larger than " + 19);
					}
					value.CopyTo(members, 0);
					numMembers = (ulong)value.Length;
				}
				else
				{
					numMembers = 0uL;
				}
			}
		}

		public ulong RoomId
		{
			get
			{
				return roomId;
			}
			set
			{
				roomId = value;
			}
		}

		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				if (value.Length > 2083)
				{
					throw new NpToolkitException("The size of the string is more than " + 2083 + " characters.");
				}
				text = value;
			}
		}

		public SetMembersAsRecentlyMetRequest()
			: base(ServiceTypes.Matching, FunctionTypes.MatchingSetMembersAsRecentlyMet)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SendInvitationRequest : RequestBase
	{
		public const int MAX_SIZE_ATTACHMENT = 1048576;

		public const int MAX_NUM_RECIPIENTS = 16;

		public const int MAX_SIZE_USER_MESSAGE = 511;

		internal ulong roomId;

		internal ulong numRecipients;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		internal Core.NpAccountId[] recipients = new Core.NpAccountId[16];

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
		internal string userMessage;

		internal ulong attachmentSize;

		[MarshalAs(UnmanagedType.LPArray)]
		internal byte[] attachment;

		internal int maxNumberRecipientsToAdd;

		[MarshalAs(UnmanagedType.I1)]
		internal bool recipientsEditableByUser;

		[MarshalAs(UnmanagedType.I1)]
		internal bool enableDialog;

		public ulong RoomId
		{
			get
			{
				return roomId;
			}
			set
			{
				roomId = value;
			}
		}

		public Core.NpAccountId[] Recipients
		{
			get
			{
				if (numRecipients == 0)
				{
					return null;
				}
				Core.NpAccountId[] array = new Core.NpAccountId[numRecipients];
				Array.Copy(recipients, array, (int)numRecipients);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 16)
					{
						throw new NpToolkitException("The size of the array is larger than " + 16);
					}
					value.CopyTo(recipients, 0);
					numRecipients = (ulong)value.Length;
				}
				else
				{
					numRecipients = 0uL;
				}
			}
		}

		public string UserMessage
		{
			get
			{
				return userMessage;
			}
			set
			{
				if (value.Length > 511)
				{
					throw new NpToolkitException("The size of the user message string is more than " + 511 + " characters.");
				}
				userMessage = value;
			}
		}

		public byte[] Attachment
		{
			get
			{
				return attachment;
			}
			set
			{
				if (value.Length > 1048576)
				{
					throw new NpToolkitException("The size of the attachment array is more than " + 1048576);
				}
				attachment = value;
				attachmentSize = (byte)value.Length;
			}
		}

		public int MaxNumberRecipientsToAdd
		{
			get
			{
				return maxNumberRecipientsToAdd;
			}
			set
			{
				maxNumberRecipientsToAdd = value;
			}
		}

		public bool RecipientsEditableByUser
		{
			get
			{
				return recipientsEditableByUser;
			}
			set
			{
				recipientsEditableByUser = value;
			}
		}

		public bool EnableDialog
		{
			get
			{
				return enableDialog;
			}
			set
			{
				enableDialog = value;
			}
		}

		public SendInvitationRequest()
			: base(ServiceTypes.Matching, FunctionTypes.MatchingSendInvitation)
		{
		}
	}

	public enum SetRoomInfoType
	{
		Invalid,
		MemberInfo,
		RoomExternalInfo,
		RoomInternalInfo,
		RoomSessionInfo,
		RoomTopology
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SetRoomInfoRequest : RequestBase
	{
		public struct MemberInformation
		{
			internal ulong numMemberAttributes;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			internal Attribute[] memberAttributes;

			internal ushort memberId;

			public Attribute[] MemberAttributes
			{
				get
				{
					if (numMemberAttributes == 0)
					{
						return null;
					}
					Attribute[] array = new Attribute[numMemberAttributes];
					Array.Copy(memberAttributes, array, (int)numMemberAttributes);
					return array;
				}
				set
				{
					if (memberAttributes == null)
					{
						memberAttributes = new Attribute[8];
					}
					if (value != null)
					{
						if (value.Length > 8)
						{
							throw new NpToolkitException("The size of the attributes array is more than " + 8);
						}
						value.CopyTo(memberAttributes, 0);
						numMemberAttributes = (ulong)value.Length;
					}
					else
					{
						numMemberAttributes = 0uL;
					}
				}
			}

			public ushort MemberId
			{
				get
				{
					return memberId;
				}
				set
				{
					memberId = value;
				}
			}

			internal void Init()
			{
				memberAttributes = new Attribute[8];
			}
		}

		public struct ExternalRoomInformation
		{
			internal ulong numExternalAttributes;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
			internal Attribute[] externalAttributes;

			internal ulong numSearchAttributes;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
			internal Attribute[] searchAttributes;

			public Attribute[] ExternalAttributes
			{
				get
				{
					if (numExternalAttributes == 0)
					{
						return null;
					}
					Attribute[] array = new Attribute[numExternalAttributes];
					Array.Copy(externalAttributes, array, (int)numExternalAttributes);
					return array;
				}
				set
				{
					if (externalAttributes == null)
					{
						externalAttributes = new Attribute[64];
					}
					if (value != null)
					{
						if (value.Length > 64)
						{
							throw new NpToolkitException("The size of the attributes array is more than " + 64);
						}
						value.CopyTo(externalAttributes, 0);
						numExternalAttributes = (ulong)value.Length;
					}
					else
					{
						numExternalAttributes = 0uL;
					}
				}
			}

			public Attribute[] SearchAttributes
			{
				get
				{
					if (numSearchAttributes == 0)
					{
						return null;
					}
					Attribute[] array = new Attribute[numSearchAttributes];
					Array.Copy(searchAttributes, array, (int)numSearchAttributes);
					return array;
				}
				set
				{
					if (searchAttributes == null)
					{
						searchAttributes = new Attribute[64];
					}
					if (value != null)
					{
						if (value.Length > 64)
						{
							throw new NpToolkitException("The size of the attributes array is more than " + 64);
						}
						value.CopyTo(searchAttributes, 0);
						numSearchAttributes = (ulong)value.Length;
					}
					else
					{
						numSearchAttributes = 0uL;
					}
				}
			}

			internal void Init()
			{
				externalAttributes = new Attribute[64];
				searchAttributes = new Attribute[64];
			}
		}

		public struct InternalRoomInformation
		{
			internal ulong numInternalAttributes;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
			internal Attribute[] internalAttributes;

			internal Core.OptionalBoolean allowBlockedUsersOfMembers;

			internal Core.OptionalBoolean joinAllLocalUsers;

			internal Core.OptionalBoolean isNatRestricted;

			internal uint numReservedSlots;

			internal RoomVisibility visibility;

			internal Core.OptionalBoolean closeRoom;

			public Attribute[] InternalAttributes
			{
				get
				{
					if (numInternalAttributes == 0)
					{
						return null;
					}
					Attribute[] array = new Attribute[numInternalAttributes];
					Array.Copy(internalAttributes, array, (int)numInternalAttributes);
					return array;
				}
				set
				{
					if (internalAttributes == null)
					{
						internalAttributes = new Attribute[64];
					}
					if (value != null)
					{
						if (value.Length > 64)
						{
							throw new NpToolkitException("The size of the attributes array is more than " + 64);
						}
						value.CopyTo(internalAttributes, 0);
						numInternalAttributes = (ulong)value.Length;
					}
					else
					{
						numInternalAttributes = 0uL;
					}
				}
			}

			public Core.OptionalBoolean AllowBlockedUsersOfMembers
			{
				get
				{
					return allowBlockedUsersOfMembers;
				}
				set
				{
					allowBlockedUsersOfMembers = value;
				}
			}

			public Core.OptionalBoolean JoinAllLocalUsers
			{
				get
				{
					return joinAllLocalUsers;
				}
				set
				{
					joinAllLocalUsers = value;
				}
			}

			public Core.OptionalBoolean IsNatRestricted
			{
				get
				{
					return isNatRestricted;
				}
				set
				{
					isNatRestricted = value;
				}
			}

			public uint NumReservedSlots
			{
				get
				{
					return numReservedSlots;
				}
				set
				{
					numReservedSlots = value;
				}
			}

			public RoomVisibility Visibility
			{
				get
				{
					return visibility;
				}
				set
				{
					visibility = value;
				}
			}

			public Core.OptionalBoolean CloseRoom
			{
				get
				{
					return closeRoom;
				}
				set
				{
					closeRoom = value;
				}
			}

			internal void Init()
			{
				internalAttributes = new Attribute[64];
			}
		}

		public struct RoomSessionInformation
		{
			internal Core.OptionalBoolean displayOnSystem;

			internal Core.OptionalBoolean isSystemJoinable;

			internal ulong changeableDataSize;

			[MarshalAs(UnmanagedType.LPArray)]
			internal byte[] changeableData;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			internal string status;

			internal ulong numLocalizations;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
			internal LocalizedSessionInfo[] localizations;

			internal SessionImage image;

			public Core.OptionalBoolean DisplayOnSystem
			{
				get
				{
					return displayOnSystem;
				}
				set
				{
					displayOnSystem = value;
				}
			}

			public Core.OptionalBoolean IsSystemJoinable
			{
				get
				{
					return isSystemJoinable;
				}
				set
				{
					isSystemJoinable = value;
				}
			}

			public byte[] ChangeableData
			{
				get
				{
					return changeableData;
				}
				set
				{
					if (value.Length > 1024)
					{
						throw new NpToolkitException("The size of the changeable data array is more than " + 1024 + " bytes.");
					}
					changeableData = value;
					changeableDataSize = (ulong)value.Length;
				}
			}

			public string Status
			{
				get
				{
					return status;
				}
				set
				{
					if (value.Length > 255)
					{
						throw new NpToolkitException("The size of the status string is more than " + 255 + " characters.");
					}
					status = value;
				}
			}

			public LocalizedSessionInfo[] Localizations
			{
				get
				{
					if (numLocalizations == 0)
					{
						return null;
					}
					LocalizedSessionInfo[] array = new LocalizedSessionInfo[numLocalizations];
					Array.Copy(localizations, array, (int)numLocalizations);
					return array;
				}
				set
				{
					if (value != null)
					{
						if (value.Length > 10)
						{
							throw new NpToolkitException("The size of the localization array is more than " + 10);
						}
						value.CopyTo(localizations, 0);
						numLocalizations = (ulong)value.Length;
					}
					else
					{
						numLocalizations = 0uL;
					}
				}
			}

			public SessionImage Image
			{
				get
				{
					return image;
				}
				set
				{
					image = value;
				}
			}

			internal void Init()
			{
				localizations = new LocalizedSessionInfo[10];
			}
		}

		public const int MAX_MEMBER_ATTRIBUTES = 8;

		public const int MAX_ATTRIBUTES = 64;

		internal ulong roomId;

		internal SetRoomInfoType roomInfoType;

		internal MemberInformation memberInfo;

		internal ExternalRoomInformation externalRoomInfo;

		internal InternalRoomInformation internalRoomInfo;

		internal RoomSessionInformation roomSessionInfo;

		internal TopologyType roomTopology;

		public ulong RoomId
		{
			get
			{
				return roomId;
			}
			set
			{
				roomId = value;
			}
		}

		public SetRoomInfoType RoomInfoType
		{
			get
			{
				return roomInfoType;
			}
			set
			{
				roomInfoType = value;
			}
		}

		public MemberInformation MemberInfo
		{
			get
			{
				return memberInfo;
			}
			set
			{
				memberInfo = value;
			}
		}

		public ExternalRoomInformation ExternalRoomInfo
		{
			get
			{
				return externalRoomInfo;
			}
			set
			{
				externalRoomInfo = value;
			}
		}

		public InternalRoomInformation InternalRoomInfo
		{
			get
			{
				return internalRoomInfo;
			}
			set
			{
				internalRoomInfo = value;
			}
		}

		public RoomSessionInformation RoomSessionInfo
		{
			get
			{
				return roomSessionInfo;
			}
			set
			{
				roomSessionInfo = value;
			}
		}

		public TopologyType RoomTopology
		{
			get
			{
				return roomTopology;
			}
			set
			{
				roomTopology = value;
			}
		}

		public SetRoomInfoRequest()
			: base(ServiceTypes.Matching, FunctionTypes.MatchingSetRoomInfo)
		{
			memberInfo.Init();
			externalRoomInfo.Init();
			internalRoomInfo.Init();
			roomSessionInfo.Init();
		}
	}

	public struct NpMatching2SessionPassword
	{
		public const int NP_MATCHING2_SESSION_PASSWORD_SIZE = 8;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
		internal string password;

		public string Password
		{
			get
			{
				return password;
			}
			set
			{
				if (value.Length > 8)
				{
					throw new NpToolkitException("The size of the password string is more than " + 8 + " characters.");
				}
				password = value;
			}
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.ReadString(ref password);
		}

		public override string ToString()
		{
			return password;
		}

		public static implicit operator NpMatching2SessionPassword(string value)
		{
			return new NpMatching2SessionPassword
			{
				Password = value
			};
		}
	}

	public struct NpSessionId
	{
		public const int NP_SESSION_ID_MAX_SIZE = 45;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 46)]
		internal string data;

		public string Data => data;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.ReadString(ref data);
		}

		public void SetFromBuffer(byte[] raw)
		{
			data = ((raw == null || raw.Length == 0) ? "" : Encoding.UTF8.GetString(raw, 0, raw.Length));
		}

		public byte[] GetRawBuffer()
		{
			return Encoding.UTF8.GetBytes(data);
		}

		public override string ToString()
		{
			return data;
		}
	}

	public struct NpMatching2WorldId
	{
		internal uint id;

		public uint Id
		{
			get
			{
				return id;
			}
			set
			{
				id = value;
			}
		}

		internal void Read(MemoryBuffer buffer)
		{
			id = buffer.ReadUInt32();
		}
	}

	public struct NpMatching2WorldNumber
	{
		internal ushort num;

		public ushort Num
		{
			get
			{
				return num;
			}
			set
			{
				num = value;
			}
		}

		internal void Read(MemoryBuffer buffer)
		{
			num = buffer.ReadUInt16();
		}

		public static implicit operator NpMatching2WorldNumber(ushort value)
		{
			return new NpMatching2WorldNumber
			{
				num = value
			};
		}
	}

	public struct World
	{
		internal NpMatching2WorldId worldId;

		internal uint currentNumberOfRooms;

		internal uint currentNumberOfMembers;

		internal NpMatching2WorldNumber worldNumber;

		public NpMatching2WorldId WorldId => worldId;

		public uint CurrentNumberOfRooms => currentNumberOfRooms;

		public uint CurrentNumberOfMembers => currentNumberOfMembers;

		public NpMatching2WorldNumber WorldNumber => worldNumber;

		internal void Read(MemoryBuffer buffer)
		{
			worldId.Read(buffer);
			currentNumberOfRooms = buffer.ReadUInt32();
			currentNumberOfMembers = buffer.ReadUInt32();
			worldNumber.Read(buffer);
		}
	}

	public enum SignalingStatus
	{
		NotApplicable,
		Established,
		EstablishedFailToGetInformation,
		Dead
	}

	public enum NatType
	{
		Invalid,
		NatType1,
		NatType2,
		NatType3
	}

	public class MemberSignalingInformation
	{
		internal NatType natType;

		internal SignalingStatus status;

		internal uint roundTripTime;

		internal NetworkUtils.NetInAddr ipAddress;

		internal ushort port;

		internal ushort portNetworkOrder;

		public NatType NatType => natType;

		public SignalingStatus Status => status;

		public uint RoundTripTime => roundTripTime;

		public NetworkUtils.NetInAddr IpAddress => ipAddress;

		public ushort Port => port;

		public ushort PortNetworkOrder => portNetworkOrder;

		internal void Read(MemoryBuffer buffer)
		{
			natType = (NatType)buffer.ReadUInt32();
			status = (SignalingStatus)buffer.ReadUInt32();
			roundTripTime = buffer.ReadUInt32();
			ipAddress.Read(buffer);
			port = buffer.ReadUInt16();
			portNetworkOrder = buffer.ReadUInt16();
		}
	}

	public class Member
	{
		internal Core.OnlineUser onlineUser = new Core.OnlineUser();

		internal Attribute[] memberAttributes;

		internal DateTime joinedDate;

		internal MemberSignalingInformation signalingInformation = new MemberSignalingInformation();

		internal Core.PlatformType platform;

		internal ushort roomMemberId;

		internal bool isOwner;

		internal bool isMe;

		public Core.OnlineUser OnlineUser => onlineUser;

		public Attribute[] MemberAttributes => memberAttributes;

		public DateTime JoinedDate => joinedDate;

		public MemberSignalingInformation SignalingInformation => signalingInformation;

		public Core.PlatformType Platform => platform;

		public ushort RoomMemberId => roomMemberId;

		public bool IsOwner => isOwner;

		public bool IsMe => isMe;

		internal void Read(MemoryBuffer buffer)
		{
			onlineUser.Read(buffer);
			ulong num = buffer.ReadUInt64();
			memberAttributes = new Attribute[num];
			for (ulong num2 = 0uL; num2 < num; num2++)
			{
				memberAttributes[num2].Read(buffer);
			}
			joinedDate = Core.ReadRtcTick(buffer);
			signalingInformation.Read(buffer);
			platform = (Core.PlatformType)buffer.ReadUInt32();
			roomMemberId = buffer.ReadUInt16();
			isOwner = buffer.ReadBool();
			isMe = buffer.ReadBool();
		}
	}

	public class Room
	{
		internal ushort matchingContext;

		internal ushort serverId;

		internal uint worldId;

		internal ulong roomId;

		internal Attribute[] attributes;

		internal string name;

		internal Member[] currentMembers;

		internal ulong numMaxMembers;

		internal TopologyType topology;

		internal uint numReservedSlots;

		internal bool isNatRestricted;

		internal bool allowBlockedUsersOfOwner;

		internal bool allowBlockedUsersOfMembers;

		internal bool joinAllLocalUsers;

		internal RoomMigrationType ownershipMigration;

		internal RoomVisibility visibility;

		internal NpMatching2SessionPassword password;

		internal NpSessionId boundSessionId;

		internal bool isSystemJoinable;

		internal bool displayOnSystem;

		internal bool hasChangeableData;

		internal bool hasFixedData;

		internal bool isCrossplatform;

		internal bool isClosed;

		public ushort MatchingContext => matchingContext;

		public ushort ServerId => serverId;

		public uint WorldId => worldId;

		public ulong RoomId => roomId;

		public Attribute[] Attributes => attributes;

		public string Name => name;

		public Member[] CurrentMembers => currentMembers;

		public ulong NumMaxMembers => numMaxMembers;

		public TopologyType Topology => topology;

		public uint NumReservedSlots => numReservedSlots;

		public bool IsNatRestricted => isNatRestricted;

		public bool AllowBlockedUsersOfOwner => allowBlockedUsersOfOwner;

		public bool AllowBlockedUsersOfMembers => allowBlockedUsersOfMembers;

		public bool JoinAllLocalUsers => joinAllLocalUsers;

		public RoomMigrationType OwnershipMigration => ownershipMigration;

		public RoomVisibility Visibility => visibility;

		public NpMatching2SessionPassword Password => password;

		public NpSessionId BoundSessionId => boundSessionId;

		public bool IsSystemJoinable => isSystemJoinable;

		public bool DisplayOnSystem => displayOnSystem;

		public bool HasChangeableData => hasChangeableData;

		public bool HasFixedData => hasFixedData;

		public bool IsCrossplatform => isCrossplatform;

		public bool IsClosed => isClosed;

		public ushort FindRoomMemberId(Core.NpAccountId accountId)
		{
			if (currentMembers == null)
			{
				return 0;
			}
			for (int i = 0; i < currentMembers.Length; i++)
			{
				if (currentMembers[i].OnlineUser.accountId == accountId)
				{
					return currentMembers[i].roomMemberId;
				}
			}
			return 0;
		}

		public ushort FindRoomMemberId(Core.OnlineID onlineId)
		{
			if (currentMembers == null)
			{
				return 0;
			}
			for (int i = 0; i < currentMembers.Length; i++)
			{
				if (currentMembers[i].OnlineUser.onlineId == onlineId)
				{
					return currentMembers[i].roomMemberId;
				}
			}
			return 0;
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.RoomBegin);
			matchingContext = buffer.ReadUInt16();
			serverId = buffer.ReadUInt16();
			worldId = buffer.ReadUInt32();
			roomId = buffer.ReadUInt64();
			ulong num = buffer.ReadUInt64();
			attributes = new Attribute[num];
			for (ulong num2 = 0uL; num2 < num; num2++)
			{
				attributes[num2].Read(buffer);
			}
			buffer.ReadString(ref name);
			ulong num3 = buffer.ReadUInt64();
			currentMembers = new Member[num3];
			for (ulong num2 = 0uL; num2 < num3; num2++)
			{
				currentMembers[num2] = new Member();
				currentMembers[num2].Read(buffer);
			}
			numMaxMembers = buffer.ReadUInt64();
			topology = (TopologyType)buffer.ReadUInt32();
			numReservedSlots = buffer.ReadUInt32();
			isNatRestricted = buffer.ReadBool();
			allowBlockedUsersOfOwner = buffer.ReadBool();
			allowBlockedUsersOfMembers = buffer.ReadBool();
			joinAllLocalUsers = buffer.ReadBool();
			ownershipMigration = (RoomMigrationType)buffer.ReadUInt32();
			visibility = (RoomVisibility)buffer.ReadUInt32();
			password.Read(buffer);
			boundSessionId.Read(buffer);
			isSystemJoinable = buffer.ReadBool();
			displayOnSystem = buffer.ReadBool();
			hasChangeableData = buffer.ReadBool();
			hasFixedData = buffer.ReadBool();
			isCrossplatform = buffer.ReadBool();
			isClosed = buffer.ReadBool();
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.RoomEnd);
		}
	}

	public class WorldsResponse : ResponseBase
	{
		internal World[] worlds;

		public World[] Worlds => worlds;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.WorldsBegin);
			ulong num = memoryBuffer.ReadUInt64();
			worlds = new World[num];
			for (ulong num2 = 0uL; num2 < num; num2++)
			{
				worlds[num2].Read(memoryBuffer);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.WorldsEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class RoomResponse : ResponseBase
	{
		internal Room room;

		public Room Room => room;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.CreateRoomBegin);
			room = new Room();
			room.Read(memoryBuffer);
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.CreateRoomEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class RoomsResponse : ResponseBase
	{
		internal Room[] rooms;

		public Room[] Rooms => rooms;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.RoomsBegin);
			ulong num = memoryBuffer.ReadUInt64();
			rooms = new Room[num];
			for (ulong num2 = 0uL; num2 < num; num2++)
			{
				rooms[num2] = new Room();
				rooms[num2].Read(memoryBuffer);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.RoomsEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class GetRoomPingTimeResponse : ResponseBase
	{
		private uint roundTripTime;

		public uint RoundTripTime => roundTripTime;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.RoomPingTimeBegin);
			roundTripTime = memoryBuffer.ReadUInt32();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.RoomPingTimeEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class GetDataResponse : ResponseBase
	{
		internal byte[] data;

		internal DataType type;

		public byte[] Data => data;

		public DataType Type => type;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GetDataBegin);
			type = (DataType)memoryBuffer.ReadUInt32();
			memoryBuffer.ReadData(ref data);
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GetDataEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public enum Reasons
	{
		MemberJoined,
		MemberLeft,
		MemberSignalingUpdate,
		MemberInfoUpdated,
		OwnerChanged,
		RoomDestroyed,
		RoomKickedOut,
		RoomExternalInfoUpdated,
		RoomInternalInfoUpdated,
		RoomTopologyUpdated,
		RoomSessionInfoUpdated
	}

	public enum Causes
	{
		Unknown = 1,
		LeaveAction = 1,
		KickoutAction = 2,
		GrantOwnerAction = 3,
		ServerOperation = 4,
		MemberDisappeared = 5,
		ServerInternal = 6,
		ConnectionError = 7,
		SignedOut = 8,
		SystemError = 9,
		ContextError = 10,
		ContextAction = 11
	}

	public class RefreshRoomResponse : ResponseBase
	{
		public class OwnerInformation
		{
			public const int OWNER_EXCHANGE_SIZE = 2;

			internal NpMatching2SessionPassword password;

			internal ushort[] oldAndNewOwners;

			public NpMatching2SessionPassword Password => password;

			public ushort[] OldAndNewOwners => oldAndNewOwners;

			internal void Read(MemoryBuffer buffer)
			{
				password.Read(buffer);
				for (int i = 0; i < 2; i++)
				{
					oldAndNewOwners[i] = buffer.ReadUInt16();
				}
			}

			internal OwnerInformation()
			{
				oldAndNewOwners = new ushort[2];
			}
		}

		public class RoomExternalInformation
		{
			internal Attribute[] attributes;

			public Attribute[] Attributes => attributes;

			internal void Read(MemoryBuffer buffer)
			{
				ulong num = buffer.ReadUInt64();
				attributes = new Attribute[num];
				for (ulong num2 = 0uL; num2 < num; num2++)
				{
					attributes[num2] = default(Attribute);
					attributes[num2].Read(buffer);
				}
			}
		}

		public class RoomInternalInformation
		{
			internal Attribute[] attributes;

			internal Core.OptionalBoolean allowBlockedUsersOfMembers;

			internal Core.OptionalBoolean joinAllLocalUsers;

			internal Core.OptionalBoolean isNatRestricted;

			internal uint numReservedSlots;

			internal RoomVisibility visibility;

			internal Core.OptionalBoolean closeRoom;

			public Attribute[] Attributes => attributes;

			public Core.OptionalBoolean AllowBlockedUsersOfMembers => allowBlockedUsersOfMembers;

			public Core.OptionalBoolean JoinAllLocalUsers => joinAllLocalUsers;

			public Core.OptionalBoolean IsNatRestricted => isNatRestricted;

			public uint NumReservedSlots => numReservedSlots;

			public RoomVisibility Visibility => visibility;

			public Core.OptionalBoolean CloseRoom => closeRoom;

			internal void Read(MemoryBuffer buffer)
			{
				ulong num = buffer.ReadUInt64();
				attributes = new Attribute[num];
				for (ulong num2 = 0uL; num2 < num; num2++)
				{
					attributes[num2] = default(Attribute);
					attributes[num2].Read(buffer);
				}
				allowBlockedUsersOfMembers = (Core.OptionalBoolean)buffer.ReadUInt32();
				joinAllLocalUsers = (Core.OptionalBoolean)buffer.ReadUInt32();
				isNatRestricted = (Core.OptionalBoolean)buffer.ReadUInt32();
				numReservedSlots = buffer.ReadUInt32();
				visibility = (RoomVisibility)buffer.ReadUInt32();
				closeRoom = (Core.OptionalBoolean)buffer.ReadUInt32();
			}
		}

		public class RoomSessionInformation
		{
			internal Core.OptionalBoolean displayOnSystem;

			internal Core.OptionalBoolean isSystemJoinable;

			internal Core.OptionalBoolean hasChangeableData;

			internal NpSessionId boundSessionId;

			public Core.OptionalBoolean DisplayOnSystem => displayOnSystem;

			public Core.OptionalBoolean IsSystemJoinable => isSystemJoinable;

			public Core.OptionalBoolean HasChangeableData => hasChangeableData;

			public NpSessionId BoundSessionId => boundSessionId;

			internal void Read(MemoryBuffer buffer)
			{
				displayOnSystem = (Core.OptionalBoolean)buffer.ReadUInt32();
				isSystemJoinable = (Core.OptionalBoolean)buffer.ReadUInt32();
				hasChangeableData = (Core.OptionalBoolean)buffer.ReadUInt32();
				boundSessionId.Read(buffer);
			}
		}

		internal ulong roomId;

		internal PresenceOptionData notificationFromMember;

		internal Reasons reason;

		internal Causes cause;

		internal OwnerInformation ownerInfo;

		internal Member memberInfo;

		internal long roomLeftError;

		internal RoomExternalInformation roomExternalInfo;

		internal RoomInternalInformation roomInternalInfo;

		internal RoomSessionInformation roomSessionInfo;

		internal TopologyType roomTopology;

		public ulong RoomId => roomId;

		public PresenceOptionData NotificationFromMember => notificationFromMember;

		public Reasons Reason => reason;

		public Causes Cause => cause;

		public OwnerInformation OwnerInfo => ownerInfo;

		public Member MemberInfo => memberInfo;

		public long RoomLeftError => roomLeftError;

		public RoomExternalInformation RoomExternalInfo => roomExternalInfo;

		public RoomInternalInformation RoomInternalInfo => roomInternalInfo;

		public RoomSessionInformation RoomSessionInfo => roomSessionInfo;

		public TopologyType RoomTopology => roomTopology;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.RefreshRoomBegin);
			roomId = memoryBuffer.ReadUInt64();
			notificationFromMember.Read(memoryBuffer);
			reason = (Reasons)memoryBuffer.ReadUInt32();
			cause = (Causes)memoryBuffer.ReadUInt32();
			if (reason == Reasons.MemberJoined || reason == Reasons.MemberLeft || reason == Reasons.MemberSignalingUpdate || reason == Reasons.MemberInfoUpdated)
			{
				memberInfo = new Member();
				memberInfo.Read(memoryBuffer);
			}
			else if (reason == Reasons.OwnerChanged)
			{
				ownerInfo = new OwnerInformation();
				ownerInfo.Read(memoryBuffer);
			}
			else if (reason == Reasons.RoomDestroyed || reason == Reasons.RoomKickedOut)
			{
				roomLeftError = memoryBuffer.ReadInt64();
			}
			else if (reason == Reasons.RoomExternalInfoUpdated)
			{
				roomExternalInfo = new RoomExternalInformation();
				roomExternalInfo.Read(memoryBuffer);
			}
			else if (reason == Reasons.RoomInternalInfoUpdated)
			{
				roomInternalInfo = new RoomInternalInformation();
				roomInternalInfo.Read(memoryBuffer);
			}
			else if (reason == Reasons.RoomSessionInfoUpdated)
			{
				roomSessionInfo = new RoomSessionInformation();
				roomSessionInfo.Read(memoryBuffer);
			}
			else if (reason == Reasons.RoomTopologyUpdated)
			{
				roomTopology = (TopologyType)memoryBuffer.ReadUInt32();
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.RefreshRoomEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class NewRoomMessageResponse : ResponseBase
	{
		internal ulong roomId;

		internal byte[] data;

		internal ushort sender;

		internal bool isChatMsg;

		internal bool isFiltered;

		public ulong RoomId => roomId;

		public byte[] Data => data;

		public string DataAsString
		{
			get
			{
				if (!isChatMsg)
				{
					throw new NpToolkitException("Room message data is not a UTF-8 string.");
				}
				if (data == null)
				{
					return "";
				}
				return Encoding.UTF8.GetString(data, 0, data.Length);
			}
		}

		public ushort Sender => sender;

		public bool IsChatMsg => isChatMsg;

		public bool IsFiltered => isFiltered;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NewRoomMessageBegin);
			roomId = memoryBuffer.ReadUInt64();
			memoryBuffer.ReadData(ref data);
			sender = memoryBuffer.ReadUInt16();
			isChatMsg = memoryBuffer.ReadBool();
			isFiltered = memoryBuffer.ReadBool();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NewRoomMessageEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public enum CurrentPlatform
	{
		NotSet,
		PSVita,
		PS4
	}

	public class InvitationReceivedResponse : ResponseBase
	{
		internal Core.OnlineUser localUpdatedUser = new Core.OnlineUser();

		internal Core.OnlineUser remoteUser = new Core.OnlineUser();

		internal CurrentPlatform platform;

		public Core.OnlineUser LocalUpdatedUser => localUpdatedUser;

		public Core.OnlineUser RemoteUser => remoteUser;

		public CurrentPlatform Platform => platform;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.InvitationReceivedBegin);
			localUpdatedUser.Read(memoryBuffer);
			remoteUser.Read(memoryBuffer);
			platform = (CurrentPlatform)memoryBuffer.ReadUInt32();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.InvitationReceivedEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public struct NpInvitationId
	{
		public const int NP_INVITATION_ID_MAX_SIZE = 60;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 60)]
		internal string id;

		public string Id => id;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.ReadString(ref id);
		}

		public override string ToString()
		{
			return id;
		}
	}

	public class SessionInvitationEventResponse : ResponseBase
	{
		internal NpSessionId sessionId;

		internal NpInvitationId invitationId;

		internal bool acceptedInvite;

		internal Core.OnlineID onlineId = new Core.OnlineID();

		internal Core.UserServiceUserId userId;

		internal Core.OnlineID referralOnlineId = new Core.OnlineID();

		internal Core.NpAccountId referralAccountId;

		public NpSessionId SessionId => sessionId;

		public NpInvitationId InvitationId => invitationId;

		public bool AcceptedInvite => acceptedInvite;

		public Core.OnlineID OnlineId => onlineId;

		public Core.UserServiceUserId UserId => userId;

		public Core.OnlineID ReferralOnlineId => referralOnlineId;

		public Core.NpAccountId ReferralAccountId => referralAccountId;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.SessionInvitationEventBegin);
			sessionId.Read(memoryBuffer);
			invitationId.Read(memoryBuffer);
			int num = memoryBuffer.ReadInt32();
			if ((num & 1) != 0)
			{
				acceptedInvite = true;
			}
			onlineId.Read(memoryBuffer);
			userId.Read(memoryBuffer);
			referralOnlineId.Read(memoryBuffer);
			referralAccountId.Read(memoryBuffer);
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.SessionInvitationEventEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public struct NpPlayTogetherInvitee
	{
		internal Core.NpAccountId accountId;

		internal Core.OnlineID onlineId;

		public Core.NpAccountId AccountId => accountId;

		public Core.OnlineID OnlineId => onlineId;

		internal void Read(MemoryBuffer buffer)
		{
			onlineId = new Core.OnlineID();
			accountId.Read(buffer);
			onlineId.Read(buffer);
		}
	}

	public class PlayTogetherHostEventResponse : ResponseBase
	{
		internal Core.UserServiceUserId userId;

		internal NpPlayTogetherInvitee[] invitees;

		public Core.UserServiceUserId UserId => userId;

		public NpPlayTogetherInvitee[] Invitees => invitees;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PlayTogetherHostEventBegin);
			userId.Read(memoryBuffer);
			uint num = memoryBuffer.ReadUInt32();
			invitees = new NpPlayTogetherInvitee[num];
			for (int i = 0; i < num; i++)
			{
				invitees[i].Read(memoryBuffer);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PlayTogetherHostEventEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public const int INVALID_ROOM_MEMBER_ID = 0;

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxSetInitConfiguration(SetInitConfigurationRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetWorlds(GetWorldsRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxCreateRoom(CreateRoomRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxLeaveRoom(LeaveRoomRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxSearchRooms(SearchRoomsRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxJoinRoom(JoinRoomRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetRoomPingTime(GetRoomPingTimeRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxKickOutRoomMember(KickOutRoomMemberRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxSendRoomMessage(SendRoomMessageRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetAttributes(GetAttributesRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxSetRoomInfo(SetRoomInfoRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxSetMembersAsRecentlyMet(SetMembersAsRecentlyMetRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxSendInvitation(SendInvitationRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetData(GetDataRequest request, out APIResult result);

	public static int SetInitConfiguration(SetInitConfigurationRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxSetInitConfiguration(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetWorlds(GetWorldsRequest request, WorldsResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetWorlds(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int CreateRoom(CreateRoomRequest request, RoomResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		if (!request.image.IsValid())
		{
			throw new NpToolkitException("Request Image hasn't be defined. A session can't be created without an image.");
		}
		if (!request.image.Exists())
		{
			throw new NpToolkitException("Request Image doesn't exists. A session can't be created without an image. " + request.image.sessionImgPath);
		}
		if (request.status == null || request.status.Length == 0)
		{
			throw new NpToolkitException("Request Status text doesn't exists. A session can't be created without Status text being set.");
		}
		APIResult result;
		int num = PrxCreateRoom(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int LeaveRoom(LeaveRoomRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxLeaveRoom(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int SearchRooms(SearchRoomsRequest request, RoomsResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxSearchRooms(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int JoinRoom(JoinRoomRequest request, RoomResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxJoinRoom(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetRoomPingTime(GetRoomPingTimeRequest request, GetRoomPingTimeResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetRoomPingTime(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int KickOutRoomMember(KickOutRoomMemberRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxKickOutRoomMember(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int SendRoomMessage(SendRoomMessageRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxSendRoomMessage(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetAttributes(GetAttributesRequest request, RefreshRoomResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetAttributes(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetData(GetDataRequest request, GetDataResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetData(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int SendInvitation(SendInvitationRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxSendInvitation(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int SetRoomInfo(SetRoomInfoRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxSetRoomInfo(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int SetMembersAsRecentlyMet(SetMembersAsRecentlyMetRequest request, Core.EmptyResponse response)
	{
		if (Main.initResult.sceSDKVersion < 72351744)
		{
			throw new NpToolkitException("SetMembersAsRecentlyMet is only available in SDK version 4.5 or greater.");
		}
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxSetMembersAsRecentlyMet(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}
}
