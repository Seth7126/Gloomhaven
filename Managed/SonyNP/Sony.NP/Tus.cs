using System;
using System.Runtime.InteropServices;

namespace Sony.NP;

public class Tus
{
	public struct VirtualUserID
	{
		public const int NP_ONLINEID_MAX_LENGTH = 16;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
		internal string name;

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				if (value.Length > 16)
				{
					throw new NpToolkitException("VirtualUserID can't be more than " + 16 + " characters.");
				}
				name = value;
			}
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.ReadString(ref name);
		}

		public override string ToString()
		{
			return name;
		}
	}

	public struct VariableInput
	{
		internal long varValue;

		internal int slotId;

		public long Value
		{
			get
			{
				return varValue;
			}
			set
			{
				varValue = value;
			}
		}

		public int SlotId
		{
			get
			{
				return slotId;
			}
			set
			{
				slotId = value;
			}
		}
	}

	public struct UserInput
	{
		internal VirtualUserID virtualId;

		internal Core.NpAccountId realId;

		[MarshalAs(UnmanagedType.I1)]
		internal bool isVirtual;

		public VirtualUserID VirtualId
		{
			get
			{
				if (!isVirtual)
				{
					throw new NpToolkitException("The User is not a virtual user.");
				}
				return virtualId;
			}
			set
			{
				virtualId = value;
				isVirtual = true;
			}
		}

		public Core.NpAccountId RealId
		{
			get
			{
				if (!isVirtual)
				{
					throw new NpToolkitException("The User is not a real user.");
				}
				return realId;
			}
			set
			{
				realId = value;
				isVirtual = false;
			}
		}

		public bool IsVirtual => isVirtual;

		public UserInput(VirtualUserID id)
		{
			realId = 0uL;
			virtualId = id;
			isVirtual = true;
		}

		public UserInput(Core.NpAccountId id)
		{
			realId = id;
			virtualId = default(VirtualUserID);
			isVirtual = false;
		}
	}

	public class NpVariableBase
	{
		internal bool hasData;

		internal DateTime lastChangedDate;

		internal long variable;

		internal long oldVariable;

		internal Core.NpAccountId ownerAccountId;

		internal Core.NpAccountId lastChangedAuthorAccountId;

		public bool HasData => hasData;

		public DateTime LastChangedDate => lastChangedDate;

		public long Variable => variable;

		public long OldVariable => oldVariable;

		public Core.NpAccountId OwnerAccountId => ownerAccountId;

		public Core.NpAccountId LastChangedAuthorAccountId => lastChangedAuthorAccountId;

		internal void ReadBase(MemoryBuffer buffer)
		{
			hasData = buffer.ReadBool();
			lastChangedDate = Core.ReadRtcTick(buffer);
			variable = buffer.ReadInt64();
			oldVariable = buffer.ReadInt64();
			ownerAccountId.Read(buffer);
			lastChangedAuthorAccountId.Read(buffer);
		}
	}

	public class NpVariable : NpVariableBase
	{
		internal Core.OnlineID ownerId;

		internal Core.OnlineID lastChangedAuthorId;

		public Core.OnlineID OwnerId => ownerId;

		public Core.OnlineID LastChangedAuthorId => lastChangedAuthorId;

		internal void Read(MemoryBuffer buffer)
		{
			ReadBase(buffer);
			ownerId = new Core.OnlineID();
			ownerId.Read(buffer);
			lastChangedAuthorId = new Core.OnlineID();
			lastChangedAuthorId.Read(buffer);
		}
	}

	public class NpVariableForCrossSave : NpVariableBase
	{
		internal Core.NpId ownerId;

		internal Core.NpId lastChangedAuthorId;

		public Core.NpId OwnerId => ownerId;

		public Core.NpId LastChangedAuthorId => lastChangedAuthorId;

		internal void Read(MemoryBuffer buffer)
		{
			ReadBase(buffer);
			ownerId.Read(buffer);
			lastChangedAuthorId.Read(buffer);
		}
	}

	[Obsolete("Use TusDataStatusBase instead.")]
	public class NpTusDataStatusBase
	{
		public bool HasData => false;

		public DateTime LastChangedDate => default(DateTime);

		public byte[] Data => null;

		public byte[] SupplementaryInfo => null;

		public Core.NpAccountId OwnerAccountId => 0uL;

		public Core.NpAccountId LastChangedAuthorAccountId => 0uL;
	}

	[Obsolete("Use TusDataStatusForCrossSave instead.")]
	public class NpTusDataStatus : NpTusDataStatusBase
	{
		public Core.OnlineID OwnerId => new Core.OnlineID();

		public Core.OnlineID LastChangedAuthorId => new Core.OnlineID();
	}

	[Obsolete("Use TusDataStatusForCrossSave instead.")]
	public class NpTusDataStatusForCrossSave : NpTusDataStatusBase
	{
		public Core.NpId OwnerId => default(Core.NpId);

		public Core.NpId LastChangedAuthorId => default(Core.NpId);
	}

	public class TusDataStatusBase
	{
		internal bool hasData;

		internal DateTime lastChangedDate;

		internal byte[] supplementaryInfo;

		public bool HasData => hasData;

		public DateTime LastChangedDate => lastChangedDate;

		public byte[] SupplementaryInfo => supplementaryInfo;

		internal void ReadBase(MemoryBuffer buffer)
		{
			hasData = buffer.ReadBool();
			lastChangedDate = Core.ReadRtcTick(buffer);
			buffer.ReadData(ref supplementaryInfo);
		}
	}

	public class TusDataStatus : TusDataStatusBase
	{
		internal Core.OnlineUser owner;

		internal Core.OnlineUser lastChangedBy;

		public Core.OnlineUser Owner => owner;

		public Core.OnlineUser LastChangedBy => lastChangedBy;

		internal void Read(MemoryBuffer buffer)
		{
			ReadBase(buffer);
			owner = new Core.OnlineUser();
			owner.Read(buffer);
			lastChangedBy = new Core.OnlineUser();
			lastChangedBy.Read(buffer);
		}
	}

	public class TusDataStatusForCrossSave : TusDataStatusBase
	{
		internal Core.NpAccountId ownerAccountId;

		internal Core.NpAccountId lastChangedByAccountId;

		internal Core.NpId ownerNpId;

		internal Core.NpId lastChangedByNpId;

		public Core.NpAccountId OwnerAccountId => ownerAccountId;

		public Core.NpAccountId LastChangedByAccountId => lastChangedByAccountId;

		public Core.NpId OwnerNpId => ownerNpId;

		public Core.NpId LastChangedByNpId => lastChangedByNpId;

		internal void Read(MemoryBuffer buffer)
		{
			ReadBase(buffer);
			ownerAccountId.Read(buffer);
			lastChangedByAccountId.Read(buffer);
			ownerNpId.Read(buffer);
			lastChangedByNpId.Read(buffer);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SetVariablesRequest : RequestBase
	{
		public const int MAX_VARIABLE_SLOTS = 256;

		internal UserInput tusUser;

		internal ulong numVars;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		private VariableInput[] variables = new VariableInput[256];

		public VariableInput[] Vars
		{
			get
			{
				if (numVars == 0)
				{
					return null;
				}
				VariableInput[] array = new VariableInput[numVars];
				Array.Copy(variables, array, (int)numVars);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 256)
					{
						throw new NpToolkitException("The size of the array is larger than " + 256);
					}
					value.CopyTo(variables, 0);
					numVars = (ulong)value.Length;
				}
				else
				{
					numVars = 0uL;
				}
			}
		}

		public UserInput TusUser
		{
			get
			{
				return tusUser;
			}
			set
			{
				tusUser = value;
			}
		}

		[Obsolete("TargetUser is deprecated, please use TusUser instead.")]
		public Core.NpAccountId TargetUser
		{
			get
			{
				return 0uL;
			}
			set
			{
			}
		}

		[Obsolete("VirtualUserID is deprecated, please use TusUser instead.")]
		public VirtualUserID VirtualUserID
		{
			get
			{
				return default(VirtualUserID);
			}
			set
			{
			}
		}

		[Obsolete("IsVirtualUser is deprecated, please use TusUser instead.")]
		public bool IsVirtualUser => false;

		public SetVariablesRequest()
			: base(ServiceTypes.Tus, FunctionTypes.TusSetVariables)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetVariablesRequest : RequestBase
	{
		public const int MAX_VARIABLE_SLOTS = 256;

		internal UserInput tusUser;

		internal ulong numSlots;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		internal int[] slotIds = new int[256];

		[MarshalAs(UnmanagedType.I1)]
		internal bool forCrossSave;

		public int[] SlotIds
		{
			get
			{
				if (numSlots == 0)
				{
					return null;
				}
				int[] array = new int[numSlots];
				Array.Copy(slotIds, array, (int)numSlots);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 256)
					{
						throw new NpToolkitException("The size of the array is larger than " + 256);
					}
					value.CopyTo(slotIds, 0);
					numSlots = (ulong)value.Length;
				}
				else
				{
					numSlots = 0uL;
				}
			}
		}

		public UserInput TusUser
		{
			get
			{
				return tusUser;
			}
			set
			{
				tusUser = value;
			}
		}

		[Obsolete("TargetUser is deprecated, please use TusUser instead.")]
		public Core.NpAccountId TargetUser
		{
			get
			{
				return 0uL;
			}
			set
			{
			}
		}

		[Obsolete("VirtualUserID is deprecated, please use TusUser instead.")]
		public VirtualUserID VirtualUserID
		{
			get
			{
				return default(VirtualUserID);
			}
			set
			{
			}
		}

		[Obsolete("IsVirtualUser is deprecated, please use TusUser instead.")]
		public bool IsVirtualUser => false;

		public GetVariablesRequest()
			: base(ServiceTypes.Tus, FunctionTypes.TusGetVariables)
		{
		}
	}

	public struct DataContention
	{
		internal ulong lastChangedDateTicks;

		internal Core.NpAccountId requiredLastChangeUser;

		public DateTime LastChangedDate
		{
			get
			{
				return Core.RtcTicksToDateTime(lastChangedDateTicks);
			}
			set
			{
				lastChangedDateTicks = Core.DateTimeToRtcTicks(value);
			}
		}

		public Core.NpAccountId RequiredLastChangeUser
		{
			get
			{
				return requiredLastChangeUser;
			}
			set
			{
				requiredLastChangeUser = value;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class AddToAndGetVariableRequest : RequestBase
	{
		internal UserInput tusUser;

		internal VariableInput var;

		internal DataContention dataContention;

		[MarshalAs(UnmanagedType.I1)]
		internal bool forCrossSave;

		public UserInput TusUser
		{
			get
			{
				return tusUser;
			}
			set
			{
				tusUser = value;
			}
		}

		public VariableInput Var
		{
			get
			{
				return var;
			}
			set
			{
				var = value;
			}
		}

		public DataContention DataContention
		{
			get
			{
				return dataContention;
			}
			set
			{
				dataContention = value;
			}
		}

		public bool ForCrossSave
		{
			get
			{
				return forCrossSave;
			}
			set
			{
				forCrossSave = value;
			}
		}

		[Obsolete("TargetUser is deprecated, please use TusUser instead.")]
		public Core.NpAccountId TargetUser
		{
			get
			{
				return 0uL;
			}
			set
			{
			}
		}

		[Obsolete("VirtualUserID is deprecated, please use TusUser instead.")]
		public VirtualUserID VirtualUserID
		{
			get
			{
				return default(VirtualUserID);
			}
			set
			{
			}
		}

		[Obsolete("IsVirtualUser is deprecated, please use TusUser instead.")]
		public bool IsVirtualUser => false;

		public AddToAndGetVariableRequest()
			: base(ServiceTypes.Tus, FunctionTypes.TusAddToAndGetVariable)
		{
			dataContention.lastChangedDateTicks = 0uL;
			dataContention.requiredLastChangeUser = 0uL;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SetDataRequest : RequestBase
	{
		public const int NP_TUS_DATA_INFO_MAX_SIZE = 384;

		internal UserInput tusUser;

		[MarshalAs(UnmanagedType.LPArray)]
		internal byte[] data;

		internal ulong dataSize;

		internal ulong supplementaryInfoSize;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 384)]
		internal byte[] supplementaryInfo = new byte[384];

		internal DataContention dataContention;

		internal int slotId;

		public UserInput TusUser
		{
			get
			{
				return tusUser;
			}
			set
			{
				tusUser = value;
			}
		}

		public byte[] Data
		{
			get
			{
				return data;
			}
			set
			{
				data = value;
				dataSize = (ulong)((value != null) ? value.Length : 0);
			}
		}

		public byte[] SupplementaryInfo
		{
			get
			{
				if (supplementaryInfoSize == 0)
				{
					return null;
				}
				byte[] array = new byte[supplementaryInfoSize];
				Array.Copy(supplementaryInfo, array, (int)supplementaryInfoSize);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 384)
					{
						throw new NpToolkitException("The size of the array is larger than " + 384);
					}
					value.CopyTo(supplementaryInfo, 0);
					supplementaryInfoSize = (ulong)value.Length;
				}
				else
				{
					supplementaryInfoSize = 0uL;
				}
			}
		}

		public int SlotId
		{
			get
			{
				return slotId;
			}
			set
			{
				slotId = value;
			}
		}

		public DataContention DataContention
		{
			get
			{
				return dataContention;
			}
			set
			{
				dataContention = value;
			}
		}

		[Obsolete("TargetUser is deprecated, please use TusUser instead.")]
		public Core.NpAccountId TargetUser
		{
			get
			{
				return 0uL;
			}
			set
			{
			}
		}

		[Obsolete("VirtualUserID is deprecated, please use TusUser instead.")]
		public VirtualUserID VirtualUserID
		{
			get
			{
				return default(VirtualUserID);
			}
			set
			{
			}
		}

		[Obsolete("IsVirtualUser is deprecated, please use TusUser instead.")]
		public bool IsVirtualUser => false;

		public SetDataRequest()
			: base(ServiceTypes.Tus, FunctionTypes.TusSetData)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetDataRequest : RequestBase
	{
		internal UserInput tusUser;

		internal int slotId;

		[MarshalAs(UnmanagedType.I1)]
		internal bool forCrossSave;

		[MarshalAs(UnmanagedType.I1)]
		internal bool retrieveStatusOnly;

		public UserInput TusUser
		{
			get
			{
				return tusUser;
			}
			set
			{
				tusUser = value;
			}
		}

		public int SlotId
		{
			get
			{
				return slotId;
			}
			set
			{
				slotId = value;
			}
		}

		public bool ForCrossSave
		{
			get
			{
				return forCrossSave;
			}
			set
			{
				forCrossSave = value;
			}
		}

		public bool RetrieveStatusOnly
		{
			get
			{
				return retrieveStatusOnly;
			}
			set
			{
				retrieveStatusOnly = value;
			}
		}

		[Obsolete("TargetUser is deprecated, please use TusUser instead.")]
		public Core.NpAccountId TargetUser
		{
			get
			{
				return 0uL;
			}
			set
			{
			}
		}

		[Obsolete("VirtualUserID is deprecated, please use TusUser instead.")]
		public VirtualUserID VirtualUserID
		{
			get
			{
				return default(VirtualUserID);
			}
			set
			{
			}
		}

		[Obsolete("IsVirtualUser is deprecated, please use TusUser instead.")]
		public bool IsVirtualUser => false;

		public GetDataRequest()
			: base(ServiceTypes.Tus, FunctionTypes.TusGetData)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class DeleteDataRequest : RequestBase
	{
		public const int MAX_DATA_SLOTS = 64;

		internal UserInput tusUser;

		internal ulong numSlots;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		private int[] slotIds = new int[64];

		public UserInput TusUser
		{
			get
			{
				return tusUser;
			}
			set
			{
				tusUser = value;
			}
		}

		public int[] SlotIds
		{
			get
			{
				if (numSlots == 0)
				{
					return null;
				}
				int[] array = new int[numSlots];
				Array.Copy(slotIds, array, (int)numSlots);
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
					value.CopyTo(slotIds, 0);
					numSlots = (ulong)value.Length;
				}
				else
				{
					numSlots = 0uL;
				}
			}
		}

		[Obsolete("TargetUser is deprecated, please use TusUser instead.")]
		public Core.NpAccountId TargetUser
		{
			get
			{
				return 0uL;
			}
			set
			{
			}
		}

		[Obsolete("VirtualUserID is deprecated, please use TusUser instead.")]
		public VirtualUserID VirtualUserID
		{
			get
			{
				return default(VirtualUserID);
			}
			set
			{
			}
		}

		[Obsolete("IsVirtualUser is deprecated, please use TusUser instead.")]
		public bool IsVirtualUser => false;

		public DeleteDataRequest()
			: base(ServiceTypes.Tus, FunctionTypes.TusDeleteData)
		{
		}
	}

	public enum TryAndSetCompareOperator
	{
		None,
		Equal,
		NotEqual,
		GreaterThan,
		GreaterThanOrEqualTo,
		LessThan,
		LessThanOrEqualTo
	}

	[StructLayout(LayoutKind.Sequential)]
	public class TryAndSetVariableRequest : RequestBase
	{
		internal UserInput tusUser;

		internal VariableInput varToUpdate;

		internal DataContention dataContention;

		internal long compareValue;

		internal TryAndSetCompareOperator compareOperator;

		[MarshalAs(UnmanagedType.I1)]
		internal bool forCrossSave;

		public UserInput TusUser
		{
			get
			{
				return tusUser;
			}
			set
			{
				tusUser = value;
			}
		}

		public VariableInput VarToUpdate
		{
			get
			{
				return varToUpdate;
			}
			set
			{
				varToUpdate = value;
			}
		}

		public DataContention DataContention
		{
			get
			{
				return dataContention;
			}
			set
			{
				dataContention = value;
			}
		}

		public long CompareValue
		{
			get
			{
				return compareValue;
			}
			set
			{
				compareValue = value;
			}
		}

		public TryAndSetCompareOperator CompareOperator
		{
			get
			{
				return compareOperator;
			}
			set
			{
				compareOperator = value;
			}
		}

		public bool ForCrossSave
		{
			get
			{
				return forCrossSave;
			}
			set
			{
				forCrossSave = value;
			}
		}

		public TryAndSetVariableRequest()
			: base(ServiceTypes.Tus, FunctionTypes.TusTryAndSetVariable)
		{
		}
	}

	public enum FriendsVariableSortingOrder
	{
		DescDate = 1,
		AscDate,
		DescValue,
		AscValue
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetFriendsVariableRequest : RequestBase
	{
		public const int MAX_PAGE_SIZE = 100;

		internal uint pageSize;

		internal int slotId;

		internal FriendsVariableSortingOrder sortingOrder;

		internal uint startIndex;

		[MarshalAs(UnmanagedType.I1)]
		internal bool forCrossSave;

		[MarshalAs(UnmanagedType.I1)]
		internal bool includeMeIfFound;

		public int SlotId
		{
			get
			{
				return slotId;
			}
			set
			{
				slotId = value;
			}
		}

		public FriendsVariableSortingOrder SortingOrder
		{
			get
			{
				return sortingOrder;
			}
			set
			{
				sortingOrder = value;
			}
		}

		public uint StartIndex
		{
			get
			{
				return startIndex;
			}
			set
			{
				startIndex = value;
			}
		}

		public uint PageSize
		{
			get
			{
				return pageSize;
			}
			set
			{
				if (value > 100)
				{
					throw new NpToolkitException("The page size can't be larger than " + 100);
				}
				pageSize = value;
			}
		}

		public bool ForCrossSave
		{
			get
			{
				return forCrossSave;
			}
			set
			{
				forCrossSave = value;
			}
		}

		public bool IncludeMeIfFound
		{
			get
			{
				return includeMeIfFound;
			}
			set
			{
				includeMeIfFound = value;
			}
		}

		public GetFriendsVariableRequest()
			: base(ServiceTypes.Tus, FunctionTypes.TusGetFriendsVariable)
		{
			pageSize = 100u;
			includeMeIfFound = true;
			sortingOrder = FriendsVariableSortingOrder.DescDate;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetUsersVariableRequest : RequestBase
	{
		public const int MAX_NUM_USERS = 100;

		internal uint maxUsersToObtain;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
		internal VirtualUserID[] virtualUsersIds = new VirtualUserID[100];

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
		internal Core.NpAccountId[] realUsersIds = new Core.NpAccountId[100];

		internal int slotId;

		[MarshalAs(UnmanagedType.I1)]
		internal bool areVirtualUsers;

		[MarshalAs(UnmanagedType.I1)]
		internal bool forCrossSave;

		public VirtualUserID[] VirtualUsersIds
		{
			get
			{
				if (maxUsersToObtain == 0)
				{
					return null;
				}
				if (!areVirtualUsers)
				{
					throw new NpToolkitException("These are not virtual users.");
				}
				VirtualUserID[] array = new VirtualUserID[maxUsersToObtain];
				Array.Copy(virtualUsersIds, array, (int)maxUsersToObtain);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 100)
					{
						throw new NpToolkitException("The size of the array is larger than " + 100);
					}
					value.CopyTo(virtualUsersIds, 0);
					maxUsersToObtain = (uint)value.Length;
					areVirtualUsers = true;
				}
				else
				{
					maxUsersToObtain = 0u;
					areVirtualUsers = false;
				}
			}
		}

		public Core.NpAccountId[] RealUsersIds
		{
			get
			{
				if (maxUsersToObtain == 0)
				{
					return null;
				}
				if (areVirtualUsers)
				{
					throw new NpToolkitException("These are not real user Ids. Virtual users are stored here.");
				}
				Core.NpAccountId[] array = new Core.NpAccountId[maxUsersToObtain];
				Array.Copy(realUsersIds, array, (int)maxUsersToObtain);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 100)
					{
						throw new NpToolkitException("The size of the array is larger than " + 100);
					}
					value.CopyTo(realUsersIds, 0);
					maxUsersToObtain = (uint)value.Length;
					areVirtualUsers = false;
				}
				else
				{
					maxUsersToObtain = 0u;
					areVirtualUsers = false;
				}
			}
		}

		public int SlotId
		{
			get
			{
				return slotId;
			}
			set
			{
				slotId = value;
			}
		}

		public bool ForCrossSave
		{
			get
			{
				return forCrossSave;
			}
			set
			{
				forCrossSave = value;
			}
		}

		public bool AreVirtualUsers => areVirtualUsers;

		public GetUsersVariableRequest()
			: base(ServiceTypes.Tus, FunctionTypes.TusGetUsersVariable)
		{
			forCrossSave = false;
			areVirtualUsers = false;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetUsersDataStatusRequest : RequestBase
	{
		public const int MAX_NUM_USERS = 100;

		internal uint maxUsersToObtain;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
		internal VirtualUserID[] virtualUsersIds = new VirtualUserID[100];

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
		internal Core.NpAccountId[] realUsersIds = new Core.NpAccountId[100];

		internal int slotId;

		[MarshalAs(UnmanagedType.I1)]
		internal bool areVirtualUsers;

		[MarshalAs(UnmanagedType.I1)]
		internal bool forCrossSave;

		public VirtualUserID[] VirtualUsersIds
		{
			get
			{
				if (maxUsersToObtain == 0)
				{
					return null;
				}
				if (!areVirtualUsers)
				{
					throw new NpToolkitException("These are not virtual users.");
				}
				VirtualUserID[] array = new VirtualUserID[maxUsersToObtain];
				Array.Copy(virtualUsersIds, array, (int)maxUsersToObtain);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 100)
					{
						throw new NpToolkitException("The size of the array is larger than " + 100);
					}
					value.CopyTo(virtualUsersIds, 0);
					maxUsersToObtain = (uint)value.Length;
					areVirtualUsers = true;
				}
				else
				{
					maxUsersToObtain = 0u;
					areVirtualUsers = false;
				}
			}
		}

		public Core.NpAccountId[] RealUsersIds
		{
			get
			{
				if (maxUsersToObtain == 0)
				{
					return null;
				}
				if (areVirtualUsers)
				{
					throw new NpToolkitException("These are not real user Ids. Virtual users are stored here.");
				}
				Core.NpAccountId[] array = new Core.NpAccountId[maxUsersToObtain];
				Array.Copy(realUsersIds, array, (int)maxUsersToObtain);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 100)
					{
						throw new NpToolkitException("The size of the array is larger than " + 100);
					}
					value.CopyTo(realUsersIds, 0);
					maxUsersToObtain = (uint)value.Length;
					areVirtualUsers = false;
				}
				else
				{
					maxUsersToObtain = 0u;
					areVirtualUsers = false;
				}
			}
		}

		public int SlotId
		{
			get
			{
				return slotId;
			}
			set
			{
				slotId = value;
			}
		}

		public bool ForCrossSave
		{
			get
			{
				return forCrossSave;
			}
			set
			{
				forCrossSave = value;
			}
		}

		public bool AreVirtualUsers => areVirtualUsers;

		public GetUsersDataStatusRequest()
			: base(ServiceTypes.Tus, FunctionTypes.TusGetUsersDataStatus)
		{
			forCrossSave = false;
			areVirtualUsers = false;
		}
	}

	public enum FriendsDataStatusSortingOrder
	{
		DescDate = 1,
		AscDate
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetFriendsDataStatusRequest : RequestBase
	{
		public const int MAX_PAGE_SIZE = 100;

		private uint pageSize;

		private int slotId;

		private FriendsDataStatusSortingOrder sortingOrder;

		private uint startIndex;

		private bool forCrossSave;

		private bool includeMeIfFound;

		public uint PageSize
		{
			get
			{
				return pageSize;
			}
			set
			{
				if (value > 100)
				{
					throw new NpToolkitException("The page size can't be larger than " + 100);
				}
				pageSize = value;
			}
		}

		public int SlotId
		{
			get
			{
				return slotId;
			}
			set
			{
				slotId = value;
			}
		}

		public FriendsDataStatusSortingOrder SortingOrder
		{
			get
			{
				return sortingOrder;
			}
			set
			{
				sortingOrder = value;
			}
		}

		public uint StartIndex
		{
			get
			{
				return startIndex;
			}
			set
			{
				startIndex = value;
			}
		}

		public bool ForCrossSave
		{
			get
			{
				return forCrossSave;
			}
			set
			{
				forCrossSave = value;
			}
		}

		public bool IncludeMeIfFound
		{
			get
			{
				return includeMeIfFound;
			}
			set
			{
				includeMeIfFound = value;
			}
		}

		public GetFriendsDataStatusRequest()
			: base(ServiceTypes.Tus, FunctionTypes.TusGetFriendsDataStatus)
		{
			pageSize = 100u;
			forCrossSave = false;
			includeMeIfFound = true;
			sortingOrder = FriendsDataStatusSortingOrder.DescDate;
		}
	}

	public class VariablesResponse : ResponseBase
	{
		internal bool forCrossSave;

		internal NpVariable[] vars;

		internal NpVariableForCrossSave[] varsForCrossSave;

		public bool ForCrossSave => forCrossSave;

		public NpVariable[] Vars
		{
			get
			{
				if (forCrossSave)
				{
					throw new NpToolkitException("Vars isn't valid unless 'ForCrossSave' is set to false.");
				}
				return vars;
			}
		}

		public NpVariableForCrossSave[] VarsForCrossSave
		{
			get
			{
				if (!forCrossSave)
				{
					throw new NpToolkitException("VarsForCrossSave isn't valid unless 'ForCrossSave' is set to true.");
				}
				return varsForCrossSave;
			}
		}

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TusVariablesBegin);
			long num = memoryBuffer.ReadInt64();
			forCrossSave = memoryBuffer.ReadBool();
			if (forCrossSave)
			{
				varsForCrossSave = new NpVariableForCrossSave[num];
			}
			else
			{
				vars = new NpVariable[num];
			}
			for (int i = 0; i < num; i++)
			{
				if (forCrossSave)
				{
					varsForCrossSave[i] = new NpVariableForCrossSave();
					varsForCrossSave[i].Read(memoryBuffer);
				}
				else
				{
					vars[i] = new NpVariable();
					vars[i].Read(memoryBuffer);
				}
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TusVariablesEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	[Obsolete("AtomicAddToAndGetVariableResponse is deprecated, please use VariablesResponse instead.")]
	public class AtomicAddToAndGetVariableResponse : ResponseBase
	{
		internal bool forCrossSave;

		internal NpVariable var;

		internal NpVariableForCrossSave varForCrossSave;

		public bool ForCrossSave => forCrossSave;

		public NpVariable Var
		{
			get
			{
				if (forCrossSave)
				{
					throw new NpToolkitException("Vars isn't valid unless 'ForCrossSave' is set to false.");
				}
				return var;
			}
		}

		public NpVariableForCrossSave VarForCrossSave
		{
			get
			{
				if (!forCrossSave)
				{
					throw new NpToolkitException("VarsForCrossSave isn't valid unless 'ForCrossSave' is set to true.");
				}
				return varForCrossSave;
			}
		}

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TusAtomicAddToAndGetVariableBegin);
			forCrossSave = memoryBuffer.ReadBool();
			if (forCrossSave)
			{
				varForCrossSave = new NpVariableForCrossSave();
				varForCrossSave.Read(memoryBuffer);
			}
			else
			{
				var = new NpVariable();
				var.Read(memoryBuffer);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TusAtomicAddToAndGetVariableEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class GetDataResponse : ResponseBase
	{
		internal bool forCrossSave;

		internal TusDataStatus tusDataStatus;

		internal TusDataStatusForCrossSave tusDataStatusForCrossSave;

		internal byte[] data;

		public byte[] Data => data;

		public bool ForCrossSave => forCrossSave;

		[Obsolete("Use DataStatus instead.")]
		public NpTusDataStatus Status
		{
			get
			{
				if (forCrossSave)
				{
					throw new NpToolkitException("Vars isn't valid unless 'ForCrossSave' is set to false.");
				}
				return null;
			}
		}

		[Obsolete("Use DataStatusForCrossSave instead.")]
		public NpTusDataStatusForCrossSave StatusForCrossSave
		{
			get
			{
				if (!forCrossSave)
				{
					throw new NpToolkitException("VarsForCrossSave isn't valid unless 'ForCrossSave' is set to true.");
				}
				return null;
			}
		}

		public TusDataStatus DataStatus
		{
			get
			{
				if (forCrossSave)
				{
					throw new NpToolkitException("Vars isn't valid unless 'ForCrossSave' is set to false.");
				}
				return tusDataStatus;
			}
		}

		public TusDataStatusForCrossSave DataStatusForCrossSave
		{
			get
			{
				if (!forCrossSave)
				{
					throw new NpToolkitException("VarsForCrossSave isn't valid unless 'ForCrossSave' is set to true.");
				}
				return tusDataStatusForCrossSave;
			}
		}

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TusDataBegin);
			memoryBuffer.ReadData(ref data);
			forCrossSave = memoryBuffer.ReadBool();
			if (forCrossSave)
			{
				tusDataStatusForCrossSave = new TusDataStatusForCrossSave();
				tusDataStatusForCrossSave.Read(memoryBuffer);
			}
			else
			{
				tusDataStatus = new TusDataStatus();
				tusDataStatus.Read(memoryBuffer);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TusDataEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class FriendsVariablesResponse : ResponseBase
	{
		internal uint totalFriends;

		internal bool forCrossSave;

		internal NpVariable[] vars;

		internal NpVariableForCrossSave[] varsForCrossSave;

		public uint TotalFriends => totalFriends;

		public bool ForCrossSave => forCrossSave;

		public NpVariable[] Vars
		{
			get
			{
				if (forCrossSave)
				{
					throw new NpToolkitException("Vars isn't valid unless 'ForCrossSave' is set to false.");
				}
				return vars;
			}
		}

		public NpVariableForCrossSave[] VarsForCrossSave
		{
			get
			{
				if (!forCrossSave)
				{
					throw new NpToolkitException("VarsForCrossSave isn't valid unless 'ForCrossSave' is set to true.");
				}
				return varsForCrossSave;
			}
		}

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TusFriendsVariablesBegin);
			totalFriends = memoryBuffer.ReadUInt32();
			long num = memoryBuffer.ReadInt64();
			forCrossSave = memoryBuffer.ReadBool();
			if (forCrossSave)
			{
				varsForCrossSave = new NpVariableForCrossSave[num];
			}
			else
			{
				vars = new NpVariable[num];
			}
			for (int i = 0; i < num; i++)
			{
				if (forCrossSave)
				{
					varsForCrossSave[i] = new NpVariableForCrossSave();
					varsForCrossSave[i].Read(memoryBuffer);
				}
				else
				{
					vars[i] = new NpVariable();
					vars[i].Read(memoryBuffer);
				}
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TusFriendsVariablesEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class DataStatusesResponse : ResponseBase
	{
		internal bool forCrossSave;

		internal TusDataStatus[] statuses;

		internal TusDataStatusForCrossSave[] statusesForCrossSave;

		public bool ForCrossSave => forCrossSave;

		public TusDataStatus[] Statuses
		{
			get
			{
				if (forCrossSave)
				{
					throw new NpToolkitException("Statuses isn't valid unless 'ForCrossSave' is set to false.");
				}
				return statuses;
			}
		}

		public TusDataStatusForCrossSave[] StatusesForCrossSave
		{
			get
			{
				if (!forCrossSave)
				{
					throw new NpToolkitException("StatusesForCrossSave isn't valid unless 'ForCrossSave' is set to true.");
				}
				return statusesForCrossSave;
			}
		}

		internal void Read(MemoryBuffer readBuffer)
		{
			readBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TusDataStatusesBegin);
			long num = readBuffer.ReadInt64();
			forCrossSave = readBuffer.ReadBool();
			if (forCrossSave)
			{
				statusesForCrossSave = new TusDataStatusForCrossSave[num];
				for (int i = 0; i < num; i++)
				{
					statusesForCrossSave[i] = new TusDataStatusForCrossSave();
					statusesForCrossSave[i].Read(readBuffer);
				}
			}
			else
			{
				statuses = new TusDataStatus[num];
				for (int i = 0; i < num; i++)
				{
					statuses[i] = new TusDataStatus();
					statuses[i].Read(readBuffer);
				}
			}
			readBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TusDataStatusesEnd);
		}

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer readBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			Read(readBuffer);
			EndReadResponseBuffer(readBuffer);
		}
	}

	public class FriendsDataStatusesResponse : ResponseBase
	{
		internal ulong totalFriends;

		internal DataStatusesResponse friendsStatuses = new DataStatusesResponse();

		public ulong TotalFriends => totalFriends;

		public bool ForCrossSave => friendsStatuses.forCrossSave;

		public TusDataStatus[] Statuses
		{
			get
			{
				if (friendsStatuses.forCrossSave)
				{
					throw new NpToolkitException("Statuses isn't valid unless 'ForCrossSave' is set to false.");
				}
				return friendsStatuses.statuses;
			}
		}

		public TusDataStatusForCrossSave[] StatusesForCrossSave
		{
			get
			{
				if (!friendsStatuses.forCrossSave)
				{
					throw new NpToolkitException("StatusesForCrossSave isn't valid unless 'ForCrossSave' is set to true.");
				}
				return friendsStatuses.statusesForCrossSave;
			}
		}

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TusFriendsDataStatusesBegin);
			totalFriends = memoryBuffer.ReadUInt32();
			friendsStatuses.Read(memoryBuffer);
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TusFriendsDataStatusesEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	[Obsolete("Variable is deprecated, please use VariableInput instead.")]
	public struct Variable
	{
		public long Value
		{
			get
			{
				return 0L;
			}
			set
			{
			}
		}

		public int SlotId
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}
	}

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxTusSetVariables(SetVariablesRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxTusGetVariables(GetVariablesRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxTusAddToAndGetVariable(AddToAndGetVariableRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxTusSetData(SetDataRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxTusGetData(GetDataRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxTusDeleteData(DeleteDataRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxTusTryAndSetVariable(TryAndSetVariableRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxTusGetFriendsVariable(GetFriendsVariableRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxTusGetUsersVariable(GetUsersVariableRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxTusGetUsersDataStatus(GetUsersDataStatusRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxTusGetFriendsDataStatus(GetFriendsDataStatusRequest request, out APIResult result);

	public static int SetVariables(SetVariablesRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxTusSetVariables(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetVariables(GetVariablesRequest request, VariablesResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxTusGetVariables(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	[Obsolete("AddToAndGetVariable using AtomicAddToAndGetVariableResponse is obsolete, please use VariablesResponse version instead.")]
	public static int AddToAndGetVariable(AddToAndGetVariableRequest request, AtomicAddToAndGetVariableResponse response)
	{
		throw new NpToolkitException("AddToAndGetVariable using AtomicAddToAndGetVariableResponse object is Obsolete. Use VariablesResponse version instead.");
	}

	public static int AddToAndGetVariable(AddToAndGetVariableRequest request, VariablesResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxTusAddToAndGetVariable(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int SetData(SetDataRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxTusSetData(request, out result);
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
		int num = PrxTusGetData(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int DeleteData(DeleteDataRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxTusDeleteData(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int TryAndSetVariable(TryAndSetVariableRequest request, VariablesResponse response)
	{
		if (Main.initResult.sceSDKVersion < 83886080)
		{
			throw new NpToolkitException("This request is not available in SDK " + Main.initResult.SceSDKVersion.ToString() + ". Only available in SDK 5.0 or greater.");
		}
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxTusTryAndSetVariable(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetFriendsVariable(GetFriendsVariableRequest request, FriendsVariablesResponse response)
	{
		if (Main.initResult.sceSDKVersion < 83886080)
		{
			throw new NpToolkitException("This request is not available in SDK " + Main.initResult.SceSDKVersion.ToString() + ". Only available in SDK 5.0 or greater.");
		}
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxTusGetFriendsVariable(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetUsersVariable(GetUsersVariableRequest request, VariablesResponse response)
	{
		if (Main.initResult.sceSDKVersion < 83886080)
		{
			throw new NpToolkitException("This request is not available in SDK " + Main.initResult.SceSDKVersion.ToString() + ". Only available in SDK 5.0 or greater.");
		}
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxTusGetUsersVariable(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetUsersDataStatus(GetUsersDataStatusRequest request, DataStatusesResponse response)
	{
		if (Main.initResult.sceSDKVersion < 83886080)
		{
			throw new NpToolkitException("This request is not available in SDK " + Main.initResult.SceSDKVersion.ToString() + ". Only available in SDK 5.0 or greater.");
		}
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxTusGetUsersDataStatus(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetFriendsDataStatus(GetFriendsDataStatusRequest request, FriendsDataStatusesResponse response)
	{
		if (Main.initResult.sceSDKVersion < 83886080)
		{
			throw new NpToolkitException("This request is not available in SDK " + Main.initResult.SceSDKVersion.ToString() + ". Only available in SDK 5.0 or greater.");
		}
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxTusGetFriendsDataStatus(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}
}
