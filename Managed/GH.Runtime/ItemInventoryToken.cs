using System.Collections.Generic;
using FFSNet;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using UdpKit;

public sealed class ItemInventoryToken : StatePropertyToken
{
	public struct ItemData
	{
		public uint ItemNetworkID;

		public byte SlotIndex;

		public ItemState ItemState;
	}

	public enum ItemState : byte
	{
		NONE,
		Unbound,
		BoundButNotEquipped,
		Equipped,
		Sold
	}

	private short itemCount;

	public ItemData[] Items { get; private set; }

	public int PartyGold { get; set; }

	public ItemInventoryToken(CMapCharacter character, CMapParty party, EGoldMode goldMode)
	{
		List<CItem> allCharacterItems = character.GetAllCharacterItems(party);
		allCharacterItems.AddRange(character.MultiplayerSoldItems);
		allCharacterItems.AddRange(party.MultiplayerSoldItems);
		Console.LogInfo("Creating an inventory token for " + character.CharacterID + ". Item count: " + ((allCharacterItems != null) ? allCharacterItems.Count.ToString() : "NULL LIST"));
		if (allCharacterItems != null)
		{
			itemCount = (short)allCharacterItems.Count;
			Items = new ItemData[itemCount];
			for (int i = 0; i < itemCount; i++)
			{
				Items[i].ItemNetworkID = allCharacterItems[i].NetworkID;
				Items[i].ItemState = GetItemInventoryState(party, character, allCharacterItems[i]);
				Items[i].SlotIndex = ((allCharacterItems[i].SlotIndex == int.MaxValue) ? byte.MaxValue : ((byte)allCharacterItems[i].SlotIndex));
				Console.LogInfo("Item data added for " + allCharacterItems[i].Name + ". (NetworkID: " + Items[i].ItemNetworkID + ", ItemState: " + Items[i].ItemState.ToString() + ", SlotIndex: " + Items[i].SlotIndex + ".)");
			}
		}
		else
		{
			itemCount = 0;
			Items = new ItemData[itemCount];
		}
		if (goldMode == EGoldMode.PartyGold)
		{
			PartyGold = party.PartyGold;
		}
		character.MultiplayerSoldItems.Clear();
	}

	public ItemInventoryToken()
	{
		itemCount = 0;
		Items = new ItemData[itemCount];
		PartyGold = 0;
	}

	public override void Write(UdpPacket packet)
	{
		base.Write(packet);
		packet.WriteShort(itemCount);
		for (int i = 0; i < itemCount; i++)
		{
			packet.WriteUInt(Items[i].ItemNetworkID);
			packet.WriteByte(Items[i].SlotIndex);
			packet.WriteByte((byte)Items[i].ItemState);
		}
		packet.WriteInt(PartyGold);
	}

	public override void Read(UdpPacket packet)
	{
		base.Read(packet);
		itemCount = packet.ReadShort();
		Items = new ItemData[itemCount];
		for (int i = 0; i < itemCount; i++)
		{
			Items[i].ItemNetworkID = packet.ReadUInt();
			Items[i].SlotIndex = packet.ReadByte();
			Items[i].ItemState = (ItemState)packet.ReadByte();
		}
		PartyGold = packet.ReadInt();
	}

	private ItemState GetItemInventoryState(CMapParty party, CMapCharacter character, CItem item)
	{
		if (character.CheckEquippedItems.Contains(item))
		{
			return ItemState.Equipped;
		}
		if (character.CheckBoundItems.Contains(item))
		{
			return ItemState.BoundButNotEquipped;
		}
		if (character.MultiplayerSoldItems.Contains(item) || party.MultiplayerSoldItems.Contains(item))
		{
			return ItemState.Sold;
		}
		return ItemState.Unbound;
	}

	public override void Print(string customTitle = "")
	{
		Console.LogInfo(customTitle + GetRevisionString() + "Item count: " + ((Items != null) ? Items.Length.ToString() : "NULL LIST"));
	}

	public override string ToString()
	{
		return GetRevisionString() + "Item count: " + ((Items != null) ? Items.Length.ToString() : "NULL LIST");
	}
}
