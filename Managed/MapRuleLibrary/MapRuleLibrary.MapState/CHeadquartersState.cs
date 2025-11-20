using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Locations;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace MapRuleLibrary.MapState;

[Serializable]
public class CHeadquartersState : CLocationState, ISerializable
{
	private volatile List<CItem> MerchantStock;

	public bool IsInitialised { get; private set; }

	public bool MerchantUnlocked { get; private set; }

	public bool EnhancerUnlocked { get; private set; }

	public List<EEnhancement> EnhancerStock { get; private set; }

	public int EnhancementSlots { get; set; }

	public bool TrainerUnlocked { get; private set; }

	public List<CPartyAchievement> CurrentAchievements { get; private set; }

	public bool TempleUnlocked { get; private set; }

	public bool PartyUIUnlocked { get; private set; }

	public bool EnhancerHasNewStock { get; set; }

	public bool MultiplayerUnlocked { get; private set; }

	public int CurrentStartingPerksAmount { get; set; }

	public bool HasShownIntroTownRecords { get; set; }

	public List<CQuestState> QuestStates { get; private set; }

	public List<CMapScenarioState> StartingScenarioStates { get; private set; }

	public List<CStoreLocationState> StoreLocationStates { get; private set; }

	public int CurrentStartingScenarioIndex { get; protected set; }

	public CHeadquarters Headquarters => MapRuleLibraryClient.MRLYML.Headquarters;

	public CMapScenarioState CurrentStartingScenario
	{
		get
		{
			if (StartingScenarioStates.Count <= 0)
			{
				return null;
			}
			return StartingScenarioStates[CurrentStartingScenarioIndex];
		}
	}

	public List<CItem> CheckMerchantStock
	{
		get
		{
			lock (MerchantStock)
			{
				return MerchantStock.ToList();
			}
		}
	}

	public CHeadquartersState(CHeadquartersState state, ReferenceDictionary references)
		: base(state, references)
	{
		IsInitialised = state.IsInitialised;
		MerchantUnlocked = state.MerchantUnlocked;
		EnhancerUnlocked = state.EnhancerUnlocked;
		EnhancerStock = references.Get(state.EnhancerStock);
		if (EnhancerStock == null && state.EnhancerStock != null)
		{
			EnhancerStock = new List<EEnhancement>();
			for (int i = 0; i < state.EnhancerStock.Count; i++)
			{
				EEnhancement item = state.EnhancerStock[i];
				EnhancerStock.Add(item);
			}
			references.Add(state.EnhancerStock, EnhancerStock);
		}
		EnhancementSlots = state.EnhancementSlots;
		TrainerUnlocked = state.TrainerUnlocked;
		CurrentAchievements = references.Get(state.CurrentAchievements);
		if (CurrentAchievements == null && state.CurrentAchievements != null)
		{
			CurrentAchievements = new List<CPartyAchievement>();
			for (int j = 0; j < state.CurrentAchievements.Count; j++)
			{
				CPartyAchievement cPartyAchievement = state.CurrentAchievements[j];
				CPartyAchievement cPartyAchievement2 = references.Get(cPartyAchievement);
				if (cPartyAchievement2 == null && cPartyAchievement != null)
				{
					cPartyAchievement2 = new CPartyAchievement(cPartyAchievement, references);
					references.Add(cPartyAchievement, cPartyAchievement2);
				}
				CurrentAchievements.Add(cPartyAchievement2);
			}
			references.Add(state.CurrentAchievements, CurrentAchievements);
		}
		TempleUnlocked = state.TempleUnlocked;
		PartyUIUnlocked = state.PartyUIUnlocked;
		EnhancerHasNewStock = state.EnhancerHasNewStock;
		MultiplayerUnlocked = state.MultiplayerUnlocked;
		CurrentStartingPerksAmount = state.CurrentStartingPerksAmount;
		HasShownIntroTownRecords = state.HasShownIntroTownRecords;
		QuestStates = references.Get(state.QuestStates);
		if (QuestStates == null && state.QuestStates != null)
		{
			QuestStates = new List<CQuestState>();
			for (int k = 0; k < state.QuestStates.Count; k++)
			{
				CQuestState cQuestState = state.QuestStates[k];
				CQuestState cQuestState2 = references.Get(cQuestState);
				if (cQuestState2 == null && cQuestState != null)
				{
					CQuestState cQuestState3 = ((!(cQuestState is CJobQuestState state2)) ? new CQuestState(cQuestState, references) : new CJobQuestState(state2, references));
					cQuestState2 = cQuestState3;
					references.Add(cQuestState, cQuestState2);
				}
				QuestStates.Add(cQuestState2);
			}
			references.Add(state.QuestStates, QuestStates);
		}
		StartingScenarioStates = references.Get(state.StartingScenarioStates);
		if (StartingScenarioStates == null && state.StartingScenarioStates != null)
		{
			StartingScenarioStates = new List<CMapScenarioState>();
			for (int l = 0; l < state.StartingScenarioStates.Count; l++)
			{
				CMapScenarioState cMapScenarioState = state.StartingScenarioStates[l];
				CMapScenarioState cMapScenarioState2 = references.Get(cMapScenarioState);
				if (cMapScenarioState2 == null && cMapScenarioState != null)
				{
					cMapScenarioState2 = new CMapScenarioState(cMapScenarioState, references);
					references.Add(cMapScenarioState, cMapScenarioState2);
				}
				StartingScenarioStates.Add(cMapScenarioState2);
			}
			references.Add(state.StartingScenarioStates, StartingScenarioStates);
		}
		StoreLocationStates = references.Get(state.StoreLocationStates);
		if (StoreLocationStates == null && state.StoreLocationStates != null)
		{
			StoreLocationStates = new List<CStoreLocationState>();
			for (int m = 0; m < state.StoreLocationStates.Count; m++)
			{
				CStoreLocationState cStoreLocationState = state.StoreLocationStates[m];
				CStoreLocationState cStoreLocationState2 = references.Get(cStoreLocationState);
				if (cStoreLocationState2 == null && cStoreLocationState != null)
				{
					cStoreLocationState2 = new CStoreLocationState(cStoreLocationState, references);
					references.Add(cStoreLocationState, cStoreLocationState2);
				}
				StoreLocationStates.Add(cStoreLocationState2);
			}
			references.Add(state.StoreLocationStates, StoreLocationStates);
		}
		CurrentStartingScenarioIndex = state.CurrentStartingScenarioIndex;
		MerchantStock = references.Get(state.MerchantStock);
		if (MerchantStock != null || state.MerchantStock == null)
		{
			return;
		}
		MerchantStock = new List<CItem>();
		for (int n = 0; n < state.MerchantStock.Count; n++)
		{
			CItem cItem = state.MerchantStock[n];
			CItem cItem2 = references.Get(cItem);
			if (cItem2 == null && cItem != null)
			{
				cItem2 = new CItem(cItem, references);
				references.Add(cItem, cItem2);
			}
			MerchantStock.Add(cItem2);
		}
		references.Add(state.MerchantStock, MerchantStock);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("MerchantUnlocked", MerchantUnlocked);
		info.AddValue("EnhancerUnlocked", EnhancerUnlocked);
		info.AddValue("EnhancerStock", EnhancerStock);
		info.AddValue("EnhancementSlots", EnhancementSlots);
		info.AddValue("TrainerUnlocked", TrainerUnlocked);
		info.AddValue("CurrentAchievements", CurrentAchievements);
		info.AddValue("TempleUnlocked", TempleUnlocked);
		info.AddValue("PartyUIUnlocked", PartyUIUnlocked);
		info.AddValue("QuestStates", QuestStates);
		info.AddValue("StartingScenarioStates", StartingScenarioStates);
		info.AddValue("StoreLocationStates", StoreLocationStates);
		info.AddValue("CurrentStartingScenarioIndex", CurrentStartingScenarioIndex);
		info.AddValue("EnhancerHasNewStock", EnhancerHasNewStock);
		info.AddValue("MultiplayerUnlocked", MultiplayerUnlocked);
		info.AddValue("CurrentStartingPerksAmount", CurrentStartingPerksAmount);
		info.AddValue("HasShownIntroTownRecords", HasShownIntroTownRecords);
		lock (MerchantStock)
		{
			info.AddValue("MerchantStock", MerchantStock);
		}
	}

	public CHeadquartersState(SerializationInfo info, StreamingContext context)
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
				case "MerchantUnlocked":
					MerchantUnlocked = info.GetBoolean("MerchantUnlocked");
					break;
				case "MerchantStock":
					MerchantStock = (List<CItem>)info.GetValue("MerchantStock", typeof(List<CItem>));
					break;
				case "EnhancerUnlocked":
					EnhancerUnlocked = info.GetBoolean("EnhancerUnlocked");
					break;
				case "EnhancerStock":
					EnhancerStock = (List<EEnhancement>)info.GetValue("EnhancerStock", typeof(List<EEnhancement>));
					break;
				case "EnhancementSlots":
					EnhancementSlots = info.GetInt32("EnhancementSlots");
					break;
				case "TrainerUnlocked":
					TrainerUnlocked = info.GetBoolean("TrainerUnlocked");
					break;
				case "CurrentAchievements":
					CurrentAchievements = (List<CPartyAchievement>)info.GetValue("CurrentAchievements", typeof(List<CPartyAchievement>));
					break;
				case "TempleUnlocked":
					TempleUnlocked = info.GetBoolean("TempleUnlocked");
					break;
				case "PartyUIUnlocked":
					PartyUIUnlocked = info.GetBoolean("PartyUIUnlocked");
					break;
				case "QuestStates":
					QuestStates = (List<CQuestState>)info.GetValue("QuestStates", typeof(List<CQuestState>));
					break;
				case "StartingScenarioStates":
					StartingScenarioStates = (List<CMapScenarioState>)info.GetValue("StartingScenarioStates", typeof(List<CMapScenarioState>));
					break;
				case "StoreLocationStates":
					StoreLocationStates = (List<CStoreLocationState>)info.GetValue("StoreLocationStates", typeof(List<CStoreLocationState>));
					break;
				case "CurrentStartingScenarioIndex":
					CurrentStartingScenarioIndex = info.GetInt32("CurrentStartingScenarioIndex");
					break;
				case "EnhancerHasNewStock":
					EnhancerHasNewStock = info.GetBoolean("EnhancerHasNewStock");
					break;
				case "MultiplayerUnlocked":
					MultiplayerUnlocked = info.GetBoolean("MultiplayerUnlocked");
					break;
				case "CurrentStartingPerksAmount":
					CurrentStartingPerksAmount = info.GetInt32("CurrentStartingPerksAmount");
					break;
				case "HasShownIntroTownRecords":
					HasShownIntroTownRecords = info.GetBoolean("HasShownIntroTownRecords");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CHeadquartersState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (base.Location != null)
		{
			if (base.UnlockConditionState == null && base.Location.UnlockCondition != null)
			{
				base.UnlockConditionState = new CUnlockConditionState(base.Location.UnlockCondition);
			}
			if (base.UnlockConditionState != null && base.Location.UnlockCondition != null)
			{
				base.UnlockConditionState.CacheUnlockCondition(base.Location.UnlockCondition);
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (StoreLocationStates == null)
		{
			StoreLocationStates = new List<CStoreLocationState>();
		}
		if (MerchantStock != null)
		{
			MerchantStock.RemoveAll((CItem x) => x == null);
		}
	}

	public CHeadquartersState()
		: base(MapRuleLibraryClient.MRLYML.Headquarters)
	{
		IsInitialised = false;
		MerchantUnlocked = false;
		MerchantStock = new List<CItem>();
		EnhancerUnlocked = false;
		EnhancerStock = new List<EEnhancement>();
		TrainerUnlocked = false;
		CurrentAchievements = new List<CPartyAchievement>();
		TempleUnlocked = false;
		PartyUIUnlocked = false;
		QuestStates = new List<CQuestState>();
		StartingScenarioStates = new List<CMapScenarioState>();
		StoreLocationStates = new List<CStoreLocationState>();
		CurrentStartingScenarioIndex = 0;
		EnhancementSlots = 0;
		EnhancerHasNewStock = false;
		MultiplayerUnlocked = false;
		HasShownIntroTownRecords = false;
		foreach (CStoreLocation storeLocation in MapRuleLibraryClient.MRLYML.StoreLocations)
		{
			StoreLocationStates.Add(new CStoreLocationState(storeLocation));
		}
	}

	public void Init()
	{
		if (IsInitialised)
		{
			return;
		}
		if (Headquarters != null)
		{
			CurrentStartingPerksAmount = Headquarters.StartingPerksAmount;
			foreach (CQuest quest in Headquarters.Quests)
			{
				if (quest.Type != EQuestType.Job)
				{
					QuestStates.Add(new CQuestState(quest));
				}
			}
			if (Headquarters.StartingScenarios != null && Headquarters.StartingScenarios.Count > 0)
			{
				for (int i = 0; i < Headquarters.StartingScenarios.Count; i++)
				{
					StartingScenarioStates.Add(new CMapScenarioState(Headquarters.StartingScenarios[i], base.ID, isStartingScenario: true));
				}
			}
			foreach (CMapScenarioState startingScenarioState in StartingScenarioStates)
			{
				startingScenarioState.Init();
				startingScenarioState.RegenerateMapScenario(1);
			}
		}
		base.Mesh = Headquarters.Mesh;
		IsInitialised = true;
	}

	public void OnMapStateAdventureStarted()
	{
		EnhancementSlots = AdventureState.MapState.FindAppliedEnhancementSlotRewards() + (AdventureState.MapState.IsCampaign ? AdventureState.MapState.MapParty.ProsperityLevel : 0);
		foreach (CQuestState questState in QuestStates)
		{
			questState.OnMapStateAdventureStarted();
		}
		List<CQuestState> list = new List<CQuestState>();
		foreach (CQuestState questState2 in QuestStates)
		{
			if (questState2.Quest == null)
			{
				list.Add(questState2);
			}
		}
		foreach (CQuestState item2 in list)
		{
			QuestStates.Remove(item2);
		}
		foreach (CQuest quest in Headquarters.Quests)
		{
			if (quest.Type != EQuestType.Job && !QuestStates.Any((CQuestState q) => q.Quest.ID == quest.ID))
			{
				CQuestState cQuestState = new CQuestState(quest);
				QuestStates.Add(cQuestState);
				cQuestState.Init();
			}
		}
		foreach (CStoreLocation storeLocation in MapRuleLibraryClient.MRLYML.StoreLocations)
		{
			if (!StoreLocationStates.Any((CStoreLocationState s) => s.ID == storeLocation.ID))
			{
				CStoreLocationState item = new CStoreLocationState(storeLocation);
				StoreLocationStates.Add(item);
			}
		}
		lock (MerchantStock)
		{
			if (MerchantStock == null)
			{
				return;
			}
			foreach (CItem item3 in MerchantStock)
			{
				if (item3 != null && item3.NetworkID == 0)
				{
					item3.NetworkID = AdventureState.MapState.GetNextItemNetworkID();
					SimpleLog.AddToSimpleLog("(CMapState.Initialise) (MerchantStock) Updating NetworkID for " + item3.Name + " to " + item3.NetworkID);
				}
			}
		}
	}

	public List<CJobQuestState> AvailableJobQuests()
	{
		List<CJobQuestState> list = new List<CJobQuestState>();
		list.AddRange(AdventureState.MapState.AllAvailableUnlockedJobQuestStates.Where((CJobQuestState q) => Headquarters.JobQuests.Any((CQuest j) => q.Quest.ID == j.ID)));
		return list;
	}

	public void RollForJobQuest()
	{
		if (Headquarters.JobMapLocations.Count > 0)
		{
			List<CJobQuestState> list = AvailableJobQuests();
			int index = AdventureState.MapState.MapRNG.Next(list.Count);
			CJobQuestState cJobQuestState = list[index];
			int index2 = AdventureState.MapState.MapRNG.Next(Headquarters.JobMapLocations.Count);
			CVector3 cVector = Headquarters.JobMapLocations[index2];
			if (cJobQuestState != null && cVector != null)
			{
				cJobQuestState.SetJobLocationAndTimeout(cVector, base.ID);
				cJobQuestState.Init();
				AdventureState.MapState.CurrentJobQuestStates.Add(cJobQuestState);
			}
		}
		SimpleLog.AddToSimpleLog("MapRNG (Roll for job quest): " + AdventureState.MapState.PeekMapRNG);
	}

	public bool UpdateStartingScenariosCompletion()
	{
		CurrentStartingScenarioIndex++;
		if (CurrentStartingScenarioIndex > StartingScenarioStates.Count - 1)
		{
			return true;
		}
		return false;
	}

	public void AddItemToMerchantStock(CItem addItem)
	{
		lock (MerchantStock)
		{
			MerchantStock.Add(addItem);
		}
	}

	public void RemoveItemFromMerchantStock(CItem removeItem)
	{
		lock (MerchantStock)
		{
			MerchantStock.Remove(removeItem);
		}
	}

	public void UnlockMerchant()
	{
		MerchantUnlocked = true;
		CContentUnlocked_MapClientMessage message = new CContentUnlocked_MapClientMessage("Merchant", "guild_character", null);
		if (MapRuleLibraryClient.Instance.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message);
		}
		else
		{
			DLLDebug.LogWarning("Message handler not set");
		}
	}

	public void UnlockEnhancer()
	{
		EnhancerUnlocked = true;
		CContentUnlocked_MapClientMessage message = new CContentUnlocked_MapClientMessage("Enhancer", "guild_character", null);
		if (MapRuleLibraryClient.Instance.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message);
		}
		else
		{
			DLLDebug.LogWarning("Message handler not set");
		}
	}

	public void UnlockTrainer()
	{
		TrainerUnlocked = true;
		CContentUnlocked_MapClientMessage message = new CContentUnlocked_MapClientMessage("Trainer", "guild_character", null);
		if (MapRuleLibraryClient.Instance.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message);
		}
		else
		{
			DLLDebug.LogWarning("Message handler not set");
		}
	}

	public void UnlockTemple()
	{
		TempleUnlocked = true;
		CContentUnlocked_MapClientMessage message = new CContentUnlocked_MapClientMessage("Temple", "guild_character", null);
		if (MapRuleLibraryClient.Instance.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message);
		}
		else
		{
			DLLDebug.LogWarning("Message handler not set");
		}
	}

	public void UnlockPartyUI(bool usingMap = true)
	{
		PartyUIUnlocked = true;
		if (usingMap)
		{
			CContentUnlocked_MapClientMessage message = new CContentUnlocked_MapClientMessage("PartyUI", "party_ui", null);
			if (MapRuleLibraryClient.Instance.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message);
			}
			else
			{
				DLLDebug.LogWarning("Message handler not set");
			}
		}
	}

	public void UnlockMultiplayer()
	{
		MultiplayerUnlocked = true;
		CContentUnlocked_MapClientMessage message = new CContentUnlocked_MapClientMessage("Multiplayer", "multiplayer", null);
		if (MapRuleLibraryClient.Instance.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message);
		}
		else
		{
			DLLDebug.LogWarning("Message handler not set");
		}
	}
}
