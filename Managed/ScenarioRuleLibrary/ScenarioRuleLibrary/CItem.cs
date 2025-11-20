using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("Name:{YMLData.Name} GUID:{ItemGuid} NetworkID:{NetworkID}")]
public class CItem : CBaseCard, ISerializable
{
	[Serializable]
	public enum EItemSlotState
	{
		None,
		Equipped,
		Useable,
		Selected,
		Locked,
		Consumed,
		Spent,
		Active
	}

	[Serializable]
	public enum EUsageType
	{
		None,
		Spent,
		Consumed,
		Unrestricted
	}

	[Serializable]
	public enum EItemSlot
	{
		None,
		Head,
		Body,
		Legs,
		OneHand,
		TwoHand,
		SmallItem,
		QuestItem
	}

	[Serializable]
	[Flags]
	public enum EItemTrigger
	{
		None = 0,
		PassiveEffect = 1,
		AtStartOfRound = 2,
		DuringOwnTurn = 4,
		SingleTarget = 8,
		SingleAbility = 0x10,
		EntireAction = 0x20,
		OnAttacked = 0x40,
		AtEndOfTurn = 0x80
	}

	[Serializable]
	public enum EItemType
	{
		None,
		Ability,
		Override
	}

	[Serializable]
	public enum EItemRarity
	{
		None,
		Common,
		Rare,
		Relic
	}

	public static EItemSlotState[] ItemSlotStates = (EItemSlotState[])Enum.GetValues(typeof(EItemSlotState));

	public static EUsageType[] UsageTypes = (EUsageType[])Enum.GetValues(typeof(EUsageType));

	public static EItemSlot[] ItemSlots = (EItemSlot[])Enum.GetValues(typeof(EItemSlot));

	public static EItemTrigger[] ItemTriggers = (EItemTrigger[])Enum.GetValues(typeof(EItemTrigger));

	public static EItemType[] ItemTypes = (EItemType[])Enum.GetValues(typeof(EItemType));

	public static EItemRarity[] ItemRarities = (EItemRarity[])Enum.GetValues(typeof(EItemRarity));

	private ItemCardYMLData m_YMLData;

	public string ItemGuid { get; private set; }

	public EItemSlotState SlotState { get; set; }

	public int SlotIndex { get; set; }

	public int RoundUsed { get; set; }

	public bool IsSlotIndexSet { get; set; }

	public uint NetworkID { get; set; }

	public bool IsNew { get; set; }

	public List<ElementInfusionBoardManager.EElement> ChosenElement { get; set; } = new List<ElementInfusionBoardManager.EElement>();

	public CActor SingleTarget { get; set; }

	public ItemCardYMLData YMLData
	{
		get
		{
			if (m_YMLData == null)
			{
				m_YMLData = ScenarioRuleClient.SRLYML.ItemCards.SingleOrDefault((ItemCardYMLData s) => s.ID == base.ID);
				if (m_YMLData == null)
				{
					DLLDebug.LogError("Error: Unable to find YML for item with ID " + base.ID + " but this item has been serialized within the Save Data.");
					throw new Exception("Error: Unable to find YML for item with ID " + base.ID + " but this item has been serialized within the Save Data.");
				}
			}
			return m_YMLData;
		}
	}

	public int SellPrice => (int)((float)YMLData.Cost / 2f);

	public bool Tradeable => YMLData.Tradeable;

	public CItem()
	{
	}

	public CItem(CItem state, ReferenceDictionary references)
		: base(state, references)
	{
		ItemGuid = state.ItemGuid;
		SlotState = state.SlotState;
		SlotIndex = state.SlotIndex;
		RoundUsed = state.RoundUsed;
		IsSlotIndexSet = state.IsSlotIndexSet;
		NetworkID = state.NetworkID;
		IsNew = state.IsNew;
		ChosenElement = references.Get(state.ChosenElement);
		if (ChosenElement == null && state.ChosenElement != null)
		{
			ChosenElement = new List<ElementInfusionBoardManager.EElement>();
			for (int i = 0; i < state.ChosenElement.Count; i++)
			{
				ElementInfusionBoardManager.EElement item = state.ChosenElement[i];
				ChosenElement.Add(item);
			}
			references.Add(state.ChosenElement, ChosenElement);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ItemGuid", ItemGuid);
		info.AddValue("SlotState", SlotState);
		info.AddValue("SlotIndex", SlotIndex);
		info.AddValue("RoundUsed", RoundUsed);
		info.AddValue("IsSlotIndexSet", IsSlotIndexSet);
		info.AddValue("NetworkID", NetworkID);
	}

	protected CItem(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "ItemGuid":
					ItemGuid = info.GetString("ItemGuid");
					break;
				case "SlotState":
					SlotState = (EItemSlotState)info.GetValue("SlotState", typeof(EItemSlotState));
					break;
				case "SlotIndex":
					SlotIndex = info.GetInt32("SlotIndex");
					break;
				case "RoundUsed":
					RoundUsed = info.GetInt32("RoundUsed");
					break;
				case "IsSlotIndexSet":
					IsSlotIndexSet = info.GetBoolean("IsSlotIndexSet");
					break;
				case "NetworkID":
					NetworkID = (uint)info.GetValue("NetworkID", typeof(uint));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CItem entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CItem(int id)
		: base(id, ECardType.Item, id.ToString())
	{
		ItemGuid = Guid.NewGuid().ToString();
		SlotState = EItemSlotState.None;
		SlotIndex = int.MaxValue;
		RoundUsed = 0;
		IsSlotIndexSet = false;
		IsNew = true;
		NetworkID = 1u;
	}

	public CItem(int id, string itemGuid, uint networkID)
		: base(id, ECardType.Item, id.ToString())
	{
		ItemGuid = itemGuid;
		SlotState = EItemSlotState.None;
		SlotIndex = int.MaxValue;
		RoundUsed = 0;
		IsSlotIndexSet = false;
		IsNew = true;
		NetworkID = networkID;
	}

	public CItem Copy()
	{
		return new CItem(base.ID, ItemGuid, NetworkID)
		{
			SlotState = SlotState,
			SlotIndex = SlotIndex,
			RoundUsed = RoundUsed,
			IsSlotIndexSet = IsSlotIndexSet,
			IsNew = IsNew
		};
	}

	public CItem Copy(Guid newGuid, uint networkID)
	{
		return new CItem(base.ID, newGuid.ToString(), networkID)
		{
			SlotState = SlotState,
			SlotIndex = SlotIndex,
			RoundUsed = RoundUsed,
			IsSlotIndexSet = IsSlotIndexSet,
			IsNew = IsNew
		};
	}

	public bool HasCondition(CCondition.EPositiveCondition condition)
	{
		if (YMLData.Data.Overrides != null && YMLData.Data.Overrides.Exists((CAbilityOverride it) => it.PositiveConditions != null && it.PositiveConditions.Contains(condition)))
		{
			return true;
		}
		if (YMLData.Data.Abilities != null && YMLData.Data.Abilities.Exists((CAbility it) => it.PositiveConditions != null && it.PositiveConditions.ContainsKey(condition)))
		{
			return true;
		}
		return false;
	}

	public bool HasCondition(CCondition.ENegativeCondition condition)
	{
		if (YMLData.Data.Overrides != null && YMLData.Data.Overrides.Exists((CAbilityOverride it) => it.NegativeConditions != null && it.NegativeConditions.Contains(condition)))
		{
			return true;
		}
		if (YMLData.Data.Abilities != null && YMLData.Data.Abilities.Exists((CAbility it) => it.NegativeConditions != null && it.NegativeConditions.ContainsKey(condition)))
		{
			return true;
		}
		return false;
	}

	public bool CanEquipItem(string characterClassID)
	{
		if (YMLData.ValidEquipCharacterClassIDs.Count > 0)
		{
			return YMLData.ValidEquipCharacterClassIDs.Contains(characterClassID);
		}
		return true;
	}

	public CAbility FindAbilityOnItemCard(string name)
	{
		return YMLData.Data.Abilities.SingleOrDefault((CAbility s) => s.Name == name);
	}

	public static List<Tuple<int, string>> Compare(CItem item1, CItem item2, string ownerType, string ownerGuid, string ownerID, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(CBaseCard.Compare(item1, item2, isMPCompare));
			if (item1.ItemGuid != item2.ItemGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1301, "Item ItemGuid does not match.", new List<string[]>
				{
					new string[3] { "Owner Type", ownerType, ownerType },
					new string[3] { "Owner GUID", ownerGuid, ownerGuid },
					new string[3] { "Owner ID", ownerID, ownerID },
					new string[3]
					{
						"Item Name",
						item1.YMLData.Name,
						item2.YMLData.Name
					},
					new string[3] { "Item GUID", item1.ItemGuid, item2.ItemGuid }
				});
			}
			if (item1.SlotIndex != item2.SlotIndex)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1303, "Item SlotIndex does not match.", new List<string[]>
				{
					new string[3] { "Owner Type", ownerType, ownerType },
					new string[3] { "Owner GUID", ownerGuid, ownerGuid },
					new string[3] { "Owner ID", ownerID, ownerID },
					new string[3] { "Item GUID", item1.ItemGuid, item2.ItemGuid },
					new string[3]
					{
						"Item Name",
						item1.YMLData.Name,
						item2.YMLData.Name
					},
					new string[3]
					{
						"Item SlotIndex",
						item1.SlotIndex.ToString(),
						item2.SlotIndex.ToString()
					}
				});
			}
			if (item1.IsSlotIndexSet != item2.IsSlotIndexSet)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1304, "Item IsSlotIndexSet does not match.", new List<string[]>
				{
					new string[3] { "Owner Type", ownerType, ownerType },
					new string[3] { "Owner GUID", ownerGuid, ownerGuid },
					new string[3] { "Owner ID", ownerID, ownerID },
					new string[3] { "Item GUID", item1.ItemGuid, item2.ItemGuid },
					new string[3]
					{
						"Item Name",
						item1.YMLData.Name,
						item2.YMLData.Name
					},
					new string[3]
					{
						"Item IsSlotIndexSet",
						item1.IsSlotIndexSet.ToString(),
						item2.IsSlotIndexSet.ToString()
					}
				});
			}
			if (item1.NetworkID != item2.NetworkID)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1306, "Item NetworkID does not match.", new List<string[]>
				{
					new string[3] { "Owner Type", ownerType, ownerType },
					new string[3] { "Owner GUID", ownerGuid, ownerGuid },
					new string[3] { "Owner ID", ownerID, ownerID },
					new string[3] { "Item GUID", item1.ItemGuid, item2.ItemGuid },
					new string[3]
					{
						"Item Name",
						item1.YMLData.Name,
						item2.YMLData.Name
					},
					new string[3]
					{
						"Item NetworkID",
						item1.NetworkID.ToString(),
						item2.NetworkID.ToString()
					}
				});
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(1399, "Exception during Item State compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
