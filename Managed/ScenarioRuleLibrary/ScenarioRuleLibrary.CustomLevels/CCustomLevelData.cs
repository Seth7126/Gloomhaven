using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary.CustomLevels;

[Serializable]
public class CCustomLevelData : ISerializable
{
	public static ELevelPartyChoiceType[] LevelPartyChoiceTypes = ((ELevelPartyChoiceType[])Enum.GetValues(typeof(ELevelPartyChoiceType))).Where((ELevelPartyChoiceType t) => t != ELevelPartyChoiceType.ChooseOwnParty && t != ELevelPartyChoiceType.ChooseOwnPartyRestricted).ToArray();

	public string Name;

	public string YMLFile;

	public DLCRegistry.EDLCKey DLCUsed;

	public ScenarioState ScenarioState;

	public List<CLevelMessage> LevelMessages { get; private set; }

	public List<CLevelEvent> LevelEvents { get; set; }

	public bool EnemyLevelsScaleToPartyLevel { get; set; }

	public bool SetEnemyHealthToMaxOnPlay { get; set; }

	public bool EnemyMaxHealthBasedOnPartyLevel { get; set; }

	public bool ShouldPreventUnspecifiedInteraction { get; set; }

	public bool UseRealtime { get; set; }

	public bool ShuffleAttackModsEnabledForPlayers { get; set; }

	public bool ShuffleAbilityDecksEnabledForMonsters { get; set; }

	public bool ShuffleAttackModsEnabledForMonsters { get; set; }

	public bool RandomiseOnLoad { get; set; }

	public ELevelPartyChoiceType PartySpawnType { get; set; }

	public int PartySizeLimit { get; set; }

	public List<string> AllowedPartyItems { get; set; }

	public List<string> AllowedPartyCharacterIDs { get; set; }

	public List<CAllowedAbilitiesPerCharacter> AllowedAbilitiesPerCharacterList { get; set; }

	public bool UsesFixedMercStartingRotation { get; set; }

	public List<TileIndex> FixedFacingDirectionIndices { get; set; }

	public List<TileIndex> StartingTileIndexes { get; set; }

	public List<CApparanceOverrideDetails> ApparanceOverrideList { get; set; }

	public List<CStatBasedOnXOverrideDetails> StatBasedOnXOverrideList { get; set; }

	public List<string> MapIconMaterialNames { get; set; }

	public bool HasScriptedEvents
	{
		get
		{
			if ((LevelMessages == null || LevelMessages.Count <= 0) && (LevelEvents == null || LevelEvents.Count <= 0))
			{
				if (ScenarioState != null)
				{
					if (!ScenarioState.WinObjectives.Any((CObjective o) => o.ObjectiveType == EObjectiveType.CustomTrigger))
					{
						return ScenarioState.LoseObjectives.Any((CObjective o) => o.ObjectiveType == EObjectiveType.CustomTrigger);
					}
					return true;
				}
				return false;
			}
			return true;
		}
	}

	public bool HasApparanceOverrides => ApparanceOverrideList.Count > 0;

	public bool HasFixedPlayerFacingRotation
	{
		get
		{
			if (UsesFixedMercStartingRotation && FixedFacingDirectionIndices != null && FixedFacingDirectionIndices.Count > 1)
			{
				return !FixedFacingDirectionIndices.Any((TileIndex t) => t == null);
			}
			return false;
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Name", Name);
		info.AddValue("YMLFile", YMLFile);
		info.AddValue("DLCUsed", DLCUsed);
		info.AddValue("ScenarioState", ScenarioState);
		info.AddValue("LevelMessages", LevelMessages);
		info.AddValue("LevelEvents", LevelEvents);
		info.AddValue("EnemyLevelsScaleToPartyLevel", EnemyLevelsScaleToPartyLevel);
		info.AddValue("SetEnemyHealthToMaxOnPlay", SetEnemyHealthToMaxOnPlay);
		info.AddValue("EnemyMaxHealthBasedOnPartyLevel", EnemyMaxHealthBasedOnPartyLevel);
		info.AddValue("ShouldPreventUnspecifiedInteraction", ShouldPreventUnspecifiedInteraction);
		info.AddValue("UseRealtime", UseRealtime);
		info.AddValue("ShuffleAttackModsEnabledForPlayers", ShuffleAttackModsEnabledForPlayers);
		info.AddValue("ShuffleAbilityDecksEnabledForMonsters", ShuffleAbilityDecksEnabledForMonsters);
		info.AddValue("ShuffleAttackModsEnabledForMonsters", ShuffleAttackModsEnabledForMonsters);
		info.AddValue("RandomiseOnLoad", RandomiseOnLoad);
		if (PartySpawnType == ELevelPartyChoiceType.ChooseOwnParty || PartySpawnType == ELevelPartyChoiceType.ChooseOwnPartyRestricted)
		{
			PartySpawnType = ELevelPartyChoiceType.LoadAdventureParty;
		}
		info.AddValue("PartySpawnType", PartySpawnType);
		info.AddValue("PartySizeLimit", PartySizeLimit);
		info.AddValue("AllowedPartyItems", AllowedPartyItems);
		info.AddValue("AllowedPartyCharacterIDs", AllowedPartyCharacterIDs);
		info.AddValue("AllowedAbilitiesPerCharacterList", AllowedAbilitiesPerCharacterList);
		info.AddValue("MapIconMaterialNames", MapIconMaterialNames);
		info.AddValue("UsesFixedMercStartingRotation", UsesFixedMercStartingRotation);
		info.AddValue("FixedFacingDirectionIndices", FixedFacingDirectionIndices);
		info.AddValue("StartingTileIndexes", StartingTileIndexes);
		info.AddValue("ApparanceOverrideList", ApparanceOverrideList);
		info.AddValue("StatBasedOnXOverrideList", StatBasedOnXOverrideList);
	}

	public CCustomLevelData(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "Name":
					Name = info.GetString("Name");
					break;
				case "YMLFile":
					YMLFile = info.GetString("YMLFile");
					break;
				case "DLCUsed":
					DLCUsed = (DLCRegistry.EDLCKey)info.GetValue("DLCUsed", typeof(DLCRegistry.EDLCKey));
					break;
				case "ScenarioState":
					ScenarioState = (ScenarioState)info.GetValue("ScenarioState", typeof(ScenarioState));
					break;
				case "LevelMessages":
					LevelMessages = (List<CLevelMessage>)info.GetValue("LevelMessages", typeof(List<CLevelMessage>));
					break;
				case "LevelEvents":
					LevelEvents = (List<CLevelEvent>)info.GetValue("LevelEvents", typeof(List<CLevelEvent>));
					break;
				case "EnemyLevelsScaleToPartyLevel":
					EnemyLevelsScaleToPartyLevel = info.GetBoolean("EnemyLevelsScaleToPartyLevel");
					break;
				case "SetEnemyHealthToMaxOnPlay":
					SetEnemyHealthToMaxOnPlay = info.GetBoolean("SetEnemyHealthToMaxOnPlay");
					break;
				case "EnemyMaxHealthBasedOnPartyLevel":
					EnemyMaxHealthBasedOnPartyLevel = info.GetBoolean("EnemyMaxHealthBasedOnPartyLevel");
					break;
				case "ShouldPreventUnspecifiedInteraction":
					ShouldPreventUnspecifiedInteraction = info.GetBoolean("ShouldPreventUnspecifiedInteraction");
					break;
				case "UseRealtime":
					UseRealtime = info.GetBoolean("UseRealtime");
					break;
				case "ShuffleAttackModsEnabledForPlayers":
					ShuffleAttackModsEnabledForPlayers = info.GetBoolean("ShuffleAttackModsEnabledForPlayers");
					break;
				case "ShuffleAbilityDecksEnabledForMonsters":
					ShuffleAbilityDecksEnabledForMonsters = info.GetBoolean("ShuffleAbilityDecksEnabledForMonsters");
					break;
				case "ShuffleAttackModsEnabledForMonsters":
					ShuffleAttackModsEnabledForMonsters = info.GetBoolean("ShuffleAttackModsEnabledForMonsters");
					break;
				case "RandomiseOnLoad":
					RandomiseOnLoad = info.GetBoolean("RandomiseOnLoad");
					break;
				case "PartySpawnType":
				{
					ELevelPartyChoiceType eLevelPartyChoiceType = (ELevelPartyChoiceType)info.GetValue("PartySpawnType", typeof(ELevelPartyChoiceType));
					if (eLevelPartyChoiceType == ELevelPartyChoiceType.ChooseOwnParty || eLevelPartyChoiceType == ELevelPartyChoiceType.ChooseOwnPartyRestricted)
					{
						PartySpawnType = ELevelPartyChoiceType.LoadAdventureParty;
					}
					else
					{
						PartySpawnType = eLevelPartyChoiceType;
					}
					break;
				}
				case "PartySizeLimit":
					PartySizeLimit = info.GetInt32("PartySizeLimit");
					break;
				case "AllowedPartyItems":
					AllowedPartyItems = (List<string>)info.GetValue("AllowedPartyItems", typeof(List<string>));
					break;
				case "AllowedPartyCharacterIDs":
					AllowedPartyCharacterIDs = (List<string>)info.GetValue("AllowedPartyCharacterIDs", typeof(List<string>));
					break;
				case "AllowedAbilitiesPerCharacterList":
					AllowedAbilitiesPerCharacterList = (List<CAllowedAbilitiesPerCharacter>)info.GetValue("AllowedAbilitiesPerCharacterList", typeof(List<CAllowedAbilitiesPerCharacter>));
					break;
				case "MapIconMaterialNames":
					MapIconMaterialNames = (List<string>)info.GetValue("MapIconMaterialNames", typeof(List<string>));
					break;
				case "UsesFixedMercStartingRotation":
					UsesFixedMercStartingRotation = info.GetBoolean("UsesFixedMercStartingRotation");
					break;
				case "FixedFacingDirectionIndices":
					FixedFacingDirectionIndices = (List<TileIndex>)info.GetValue("FixedFacingDirectionIndices", typeof(List<TileIndex>));
					break;
				case "StartingTileIndexes":
					StartingTileIndexes = (List<TileIndex>)info.GetValue("StartingTileIndexes", typeof(List<TileIndex>));
					break;
				case "EnemyHealthBasedOnPartyLevel":
					SetEnemyHealthToMaxOnPlay = info.GetBoolean("EnemyHealthBasedOnPartyLevel");
					break;
				case "AllowedPartyCharacters":
				{
					List<string> obj = (List<string>)info.GetValue("AllowedPartyCharacters", typeof(List<string>));
					if (AllowedPartyCharacterIDs == null)
					{
						AllowedPartyCharacterIDs = new List<string>();
					}
					foreach (string item in obj)
					{
						AllowedPartyCharacterIDs.Add(item.Replace(" ", string.Empty) + "ID");
					}
					break;
				}
				case "ApparanceOverrideList":
					ApparanceOverrideList = (List<CApparanceOverrideDetails>)info.GetValue("ApparanceOverrideList", typeof(List<CApparanceOverrideDetails>));
					break;
				case "StatBasedOnXOverrideList":
					StatBasedOnXOverrideList = (List<CStatBasedOnXOverrideDetails>)info.GetValue("StatBasedOnXOverrideList", typeof(List<CStatBasedOnXOverrideDetails>));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CCustomLevelData entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (PartySizeLimit == 0)
		{
			PartySizeLimit = 4;
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (AllowedPartyItems == null)
		{
			AllowedPartyItems = new List<string>();
		}
		if (AllowedPartyCharacterIDs == null)
		{
			AllowedPartyCharacterIDs = new List<string>();
		}
		if (AllowedAbilitiesPerCharacterList == null)
		{
			AllowedAbilitiesPerCharacterList = new List<CAllowedAbilitiesPerCharacter>();
		}
		if (StartingTileIndexes == null)
		{
			StartingTileIndexes = new List<TileIndex>();
		}
		if (ApparanceOverrideList == null)
		{
			ApparanceOverrideList = new List<CApparanceOverrideDetails>();
		}
		if (StatBasedOnXOverrideList == null)
		{
			StatBasedOnXOverrideList = new List<CStatBasedOnXOverrideDetails>();
		}
	}

	public CCustomLevelData()
	{
		Name = string.Empty;
		SetDefaults();
	}

	public CCustomLevelData(string name, ScenarioState state)
	{
		Name = name;
		ScenarioState = state;
		SetDefaults();
	}

	public CCustomLevelData(string name, ScenarioDefinition scenarioDefinition)
	{
		Name = name;
		YMLFile = scenarioDefinition.FileName;
		ScenarioState = null;
		SetDefaults();
		PartySpawnType = ELevelPartyChoiceType.ChooseOwnParty;
	}

	public CCustomLevelData(CCustomLevelData data)
	{
		Name = data.Name;
		ScenarioState = data.ScenarioState;
		LevelMessages = data.LevelMessages;
		LevelEvents = data.LevelEvents;
		EnemyLevelsScaleToPartyLevel = data.EnemyLevelsScaleToPartyLevel;
		SetEnemyHealthToMaxOnPlay = data.SetEnemyHealthToMaxOnPlay;
		EnemyMaxHealthBasedOnPartyLevel = data.EnemyMaxHealthBasedOnPartyLevel;
		ShouldPreventUnspecifiedInteraction = data.ShouldPreventUnspecifiedInteraction;
		ShuffleAttackModsEnabledForPlayers = data.ShuffleAttackModsEnabledForPlayers;
		ShuffleAbilityDecksEnabledForMonsters = data.ShuffleAbilityDecksEnabledForMonsters;
		ShuffleAttackModsEnabledForMonsters = data.ShuffleAttackModsEnabledForMonsters;
		RandomiseOnLoad = data.RandomiseOnLoad;
		PartySpawnType = data.PartySpawnType;
		PartySizeLimit = data.PartySizeLimit;
		AllowedPartyItems = data.AllowedPartyItems;
		AllowedPartyCharacterIDs = data.AllowedPartyCharacterIDs;
		AllowedAbilitiesPerCharacterList = data.AllowedAbilitiesPerCharacterList;
		MapIconMaterialNames = data.MapIconMaterialNames;
		StartingTileIndexes = data.StartingTileIndexes;
		ApparanceOverrideList = data.ApparanceOverrideList;
		StatBasedOnXOverrideList = data.StatBasedOnXOverrideList;
	}

	private void SetDefaults()
	{
		LevelMessages = new List<CLevelMessage>();
		LevelEvents = new List<CLevelEvent>();
		PartySpawnType = ELevelPartyChoiceType.LoadAdventureParty;
		PartySizeLimit = 4;
		EnemyLevelsScaleToPartyLevel = true;
		SetEnemyHealthToMaxOnPlay = true;
		EnemyMaxHealthBasedOnPartyLevel = true;
		ShouldPreventUnspecifiedInteraction = false;
		ShuffleAttackModsEnabledForPlayers = true;
		ShuffleAbilityDecksEnabledForMonsters = true;
		ShuffleAttackModsEnabledForMonsters = true;
		RandomiseOnLoad = true;
		AllowedPartyItems = new List<string>();
		AllowedPartyCharacterIDs = new List<string>();
		AllowedAbilitiesPerCharacterList = new List<CAllowedAbilitiesPerCharacter>();
		MapIconMaterialNames = new List<string>();
		StartingTileIndexes = new List<TileIndex>();
		ApparanceOverrideList = new List<CApparanceOverrideDetails>();
		StatBasedOnXOverrideList = new List<CStatBasedOnXOverrideDetails>();
	}

	public CApparanceOverrideDetails GetApparanceOverrideDetailsForGUID(string GUID)
	{
		return ApparanceOverrideList.FirstOrDefault((CApparanceOverrideDetails o) => o.GUID == GUID);
	}

	public List<CStatBasedOnXOverrideDetails> GetStatBasedOnXEntriedForClass(string classID)
	{
		return StatBasedOnXOverrideList.Where((CStatBasedOnXOverrideDetails s) => s.AssociatedClassID == classID).ToList();
	}

	public CCustomLevelData(CCustomLevelData state, ReferenceDictionary references)
	{
		Name = state.Name;
		YMLFile = state.YMLFile;
		DLCUsed = state.DLCUsed;
		ScenarioState = references.Get(state.ScenarioState);
		if (ScenarioState == null && state.ScenarioState != null)
		{
			ScenarioState = new ScenarioState(state.ScenarioState, references);
			references.Add(state.ScenarioState, ScenarioState);
		}
		LevelMessages = references.Get(state.LevelMessages);
		if (LevelMessages == null && state.LevelMessages != null)
		{
			LevelMessages = new List<CLevelMessage>();
			for (int i = 0; i < state.LevelMessages.Count; i++)
			{
				CLevelMessage cLevelMessage = state.LevelMessages[i];
				CLevelMessage cLevelMessage2 = references.Get(cLevelMessage);
				if (cLevelMessage2 == null && cLevelMessage != null)
				{
					cLevelMessage2 = new CLevelMessage(cLevelMessage, references);
					references.Add(cLevelMessage, cLevelMessage2);
				}
				LevelMessages.Add(cLevelMessage2);
			}
			references.Add(state.LevelMessages, LevelMessages);
		}
		LevelEvents = references.Get(state.LevelEvents);
		if (LevelEvents == null && state.LevelEvents != null)
		{
			LevelEvents = new List<CLevelEvent>();
			for (int j = 0; j < state.LevelEvents.Count; j++)
			{
				CLevelEvent cLevelEvent = state.LevelEvents[j];
				CLevelEvent cLevelEvent2 = references.Get(cLevelEvent);
				if (cLevelEvent2 == null && cLevelEvent != null)
				{
					cLevelEvent2 = new CLevelEvent(cLevelEvent, references);
					references.Add(cLevelEvent, cLevelEvent2);
				}
				LevelEvents.Add(cLevelEvent2);
			}
			references.Add(state.LevelEvents, LevelEvents);
		}
		EnemyLevelsScaleToPartyLevel = state.EnemyLevelsScaleToPartyLevel;
		SetEnemyHealthToMaxOnPlay = state.SetEnemyHealthToMaxOnPlay;
		EnemyMaxHealthBasedOnPartyLevel = state.EnemyMaxHealthBasedOnPartyLevel;
		ShouldPreventUnspecifiedInteraction = state.ShouldPreventUnspecifiedInteraction;
		UseRealtime = state.UseRealtime;
		ShuffleAttackModsEnabledForPlayers = state.ShuffleAttackModsEnabledForPlayers;
		ShuffleAbilityDecksEnabledForMonsters = state.ShuffleAbilityDecksEnabledForMonsters;
		ShuffleAttackModsEnabledForMonsters = state.ShuffleAttackModsEnabledForMonsters;
		RandomiseOnLoad = state.RandomiseOnLoad;
		PartySpawnType = state.PartySpawnType;
		PartySizeLimit = state.PartySizeLimit;
		AllowedPartyItems = references.Get(state.AllowedPartyItems);
		if (AllowedPartyItems == null && state.AllowedPartyItems != null)
		{
			AllowedPartyItems = new List<string>();
			for (int k = 0; k < state.AllowedPartyItems.Count; k++)
			{
				string item = state.AllowedPartyItems[k];
				AllowedPartyItems.Add(item);
			}
			references.Add(state.AllowedPartyItems, AllowedPartyItems);
		}
		AllowedPartyCharacterIDs = references.Get(state.AllowedPartyCharacterIDs);
		if (AllowedPartyCharacterIDs == null && state.AllowedPartyCharacterIDs != null)
		{
			AllowedPartyCharacterIDs = new List<string>();
			for (int l = 0; l < state.AllowedPartyCharacterIDs.Count; l++)
			{
				string item2 = state.AllowedPartyCharacterIDs[l];
				AllowedPartyCharacterIDs.Add(item2);
			}
			references.Add(state.AllowedPartyCharacterIDs, AllowedPartyCharacterIDs);
		}
		AllowedAbilitiesPerCharacterList = references.Get(state.AllowedAbilitiesPerCharacterList);
		if (AllowedAbilitiesPerCharacterList == null && state.AllowedAbilitiesPerCharacterList != null)
		{
			AllowedAbilitiesPerCharacterList = new List<CAllowedAbilitiesPerCharacter>();
			for (int m = 0; m < state.AllowedAbilitiesPerCharacterList.Count; m++)
			{
				CAllowedAbilitiesPerCharacter cAllowedAbilitiesPerCharacter = state.AllowedAbilitiesPerCharacterList[m];
				CAllowedAbilitiesPerCharacter cAllowedAbilitiesPerCharacter2 = references.Get(cAllowedAbilitiesPerCharacter);
				if (cAllowedAbilitiesPerCharacter2 == null && cAllowedAbilitiesPerCharacter != null)
				{
					cAllowedAbilitiesPerCharacter2 = new CAllowedAbilitiesPerCharacter(cAllowedAbilitiesPerCharacter, references);
					references.Add(cAllowedAbilitiesPerCharacter, cAllowedAbilitiesPerCharacter2);
				}
				AllowedAbilitiesPerCharacterList.Add(cAllowedAbilitiesPerCharacter2);
			}
			references.Add(state.AllowedAbilitiesPerCharacterList, AllowedAbilitiesPerCharacterList);
		}
		UsesFixedMercStartingRotation = state.UsesFixedMercStartingRotation;
		FixedFacingDirectionIndices = references.Get(state.FixedFacingDirectionIndices);
		if (FixedFacingDirectionIndices == null && state.FixedFacingDirectionIndices != null)
		{
			FixedFacingDirectionIndices = new List<TileIndex>();
			for (int n = 0; n < state.FixedFacingDirectionIndices.Count; n++)
			{
				TileIndex tileIndex = state.FixedFacingDirectionIndices[n];
				TileIndex tileIndex2 = references.Get(tileIndex);
				if (tileIndex2 == null && tileIndex != null)
				{
					tileIndex2 = new TileIndex(tileIndex, references);
					references.Add(tileIndex, tileIndex2);
				}
				FixedFacingDirectionIndices.Add(tileIndex2);
			}
			references.Add(state.FixedFacingDirectionIndices, FixedFacingDirectionIndices);
		}
		StartingTileIndexes = references.Get(state.StartingTileIndexes);
		if (StartingTileIndexes == null && state.StartingTileIndexes != null)
		{
			StartingTileIndexes = new List<TileIndex>();
			for (int num = 0; num < state.StartingTileIndexes.Count; num++)
			{
				TileIndex tileIndex3 = state.StartingTileIndexes[num];
				TileIndex tileIndex4 = references.Get(tileIndex3);
				if (tileIndex4 == null && tileIndex3 != null)
				{
					tileIndex4 = new TileIndex(tileIndex3, references);
					references.Add(tileIndex3, tileIndex4);
				}
				StartingTileIndexes.Add(tileIndex4);
			}
			references.Add(state.StartingTileIndexes, StartingTileIndexes);
		}
		ApparanceOverrideList = references.Get(state.ApparanceOverrideList);
		if (ApparanceOverrideList == null && state.ApparanceOverrideList != null)
		{
			ApparanceOverrideList = new List<CApparanceOverrideDetails>();
			for (int num2 = 0; num2 < state.ApparanceOverrideList.Count; num2++)
			{
				CApparanceOverrideDetails cApparanceOverrideDetails = state.ApparanceOverrideList[num2];
				CApparanceOverrideDetails cApparanceOverrideDetails2 = references.Get(cApparanceOverrideDetails);
				if (cApparanceOverrideDetails2 == null && cApparanceOverrideDetails != null)
				{
					cApparanceOverrideDetails2 = new CApparanceOverrideDetails(cApparanceOverrideDetails, references);
					references.Add(cApparanceOverrideDetails, cApparanceOverrideDetails2);
				}
				ApparanceOverrideList.Add(cApparanceOverrideDetails2);
			}
			references.Add(state.ApparanceOverrideList, ApparanceOverrideList);
		}
		StatBasedOnXOverrideList = references.Get(state.StatBasedOnXOverrideList);
		if (StatBasedOnXOverrideList == null && state.StatBasedOnXOverrideList != null)
		{
			StatBasedOnXOverrideList = new List<CStatBasedOnXOverrideDetails>();
			for (int num3 = 0; num3 < state.StatBasedOnXOverrideList.Count; num3++)
			{
				CStatBasedOnXOverrideDetails cStatBasedOnXOverrideDetails = state.StatBasedOnXOverrideList[num3];
				CStatBasedOnXOverrideDetails cStatBasedOnXOverrideDetails2 = references.Get(cStatBasedOnXOverrideDetails);
				if (cStatBasedOnXOverrideDetails2 == null && cStatBasedOnXOverrideDetails != null)
				{
					cStatBasedOnXOverrideDetails2 = new CStatBasedOnXOverrideDetails(cStatBasedOnXOverrideDetails, references);
					references.Add(cStatBasedOnXOverrideDetails, cStatBasedOnXOverrideDetails2);
				}
				StatBasedOnXOverrideList.Add(cStatBasedOnXOverrideDetails2);
			}
			references.Add(state.StatBasedOnXOverrideList, StatBasedOnXOverrideList);
		}
		MapIconMaterialNames = references.Get(state.MapIconMaterialNames);
		if (MapIconMaterialNames == null && state.MapIconMaterialNames != null)
		{
			MapIconMaterialNames = new List<string>();
			for (int num4 = 0; num4 < state.MapIconMaterialNames.Count; num4++)
			{
				string item3 = state.MapIconMaterialNames[num4];
				MapIconMaterialNames.Add(item3);
			}
			references.Add(state.MapIconMaterialNames, MapIconMaterialNames);
		}
	}
}
