using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Events;
using MapRuleLibrary.YML.Locations;
using MapRuleLibrary.YML.Quest;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace MapRuleLibrary.MapState;

[Serializable]
public class CMapScenarioState : CLocationState, ISerializable
{
	private const string AutoSaveStateSuffix = "_AutoSave";

	private const string InitialStateSuffix = "_Initial";

	public ScenarioState NonSerializedInitialState;

	private static volatile bool m_IsRegenerating;

	private CCustomLevelData m_CustomLevelData;

	public bool IsInitialised { get; private set; }

	public string QuestName { get; private set; }

	public bool IsStartingScenario { get; private set; }

	public string RoadEventID { get; private set; }

	public EMapScenarioType MapScenarioType { get; private set; }

	public string ScenarioID { get; private set; }

	public string ScenarioFilePath { get; private set; }

	public Extensions.RandomState ScenarioGenerationRNGState { get; private set; }

	public List<string> CachedMonsterClasses { get; private set; }

	public float CachedFinalThreatModifier { get; private set; }

	public ScenarioState InitialState { get; private set; }

	public ScenarioState AutoSaveState { get; private set; }

	public string MatchSessionID { get; set; }

	public float TimeActiveSec { get; set; }

	public List<RewardGroup> ChestRewards { get; set; }

	public CQuestState QuestState => AdventureState.MapState.AllQuestStates.SingleOrDefault((CQuestState s) => s.ID == QuestName);

	public CQuest Quest => MapRuleLibraryClient.MRLYML.Quests.SingleOrDefault((CQuest s) => s.ID == QuestName);

	public CHeadquarters Headquarters => MapRuleLibraryClient.MRLYML.Headquarters;

	public CMapScenario MapScenario
	{
		get
		{
			if (!IsStartingScenario)
			{
				return Quest.MapScenario;
			}
			return Headquarters.StartingScenarios.SingleOrDefault((CMapScenario s) => s.ID == base.ID);
		}
	}

	public bool IsIntroScenario => Headquarters?.TutorialQuestNames.SingleOrDefault((string s) => s == QuestName) != null;

	public bool IsTutorialOrIntroScenario
	{
		get
		{
			if (!IsStartingScenario)
			{
				return IsIntroScenario;
			}
			return true;
		}
	}

	public string LocalisedStartKey => ScenarioID + "_Start_{0}";

	public string LocalisedSuccessKey => ScenarioID + "_Success_{0}";

	public string LocalisedOpenRoomKey => ScenarioID + "_{0}_{1}";

	public ScenarioState CurrentState
	{
		get
		{
			if (AutoSaveState != null)
			{
				return AutoSaveState;
			}
			if (InitialState != null)
			{
				return InitialState;
			}
			return null;
		}
	}

	public CCustomLevelData CustomLevelData
	{
		get
		{
			if (m_CustomLevelData == null)
			{
				m_CustomLevelData = ScenarioRuleClient.SRLYML.GetCustomLevel(ScenarioID);
			}
			return m_CustomLevelData;
		}
	}

	public CLevelMessage ScenarioStartMessage
	{
		get
		{
			CLevelMessage value = null;
			if (AdventureState.MapState.ScenarioStartMessages.TryGetValue(base.ID, out value))
			{
				return value;
			}
			return null;
		}
	}

	public CLevelMessage ScenarioCompleteMessage
	{
		get
		{
			CLevelMessage value = null;
			if (AdventureState.MapState.ScenarioCompleteMessages.TryGetValue(base.ID, out value))
			{
				return value;
			}
			return null;
		}
	}

	public List<CLevelMessage> ScenarioRoomRevealMessages
	{
		get
		{
			List<CLevelMessage> value = null;
			if (AdventureState.MapState.ScenarioRoomRevealMessages.TryGetValue(base.ID, out value))
			{
				return value;
			}
			return null;
		}
	}

	public bool HasPredefinedMessages
	{
		get
		{
			if (ScenarioStartMessage == null && ScenarioCompleteMessage == null)
			{
				if (ScenarioRoomRevealMessages != null)
				{
					return ScenarioRoomRevealMessages.Count > 0;
				}
				return false;
			}
			return true;
		}
	}

	public bool LoadInitialState => AutoSaveState == null;

	public int ScenarioLevelToUse
	{
		get
		{
			if (QuestState != null)
			{
				return QuestState.ScenarioLevelToUse;
			}
			return AdventureState.MapState.MapParty.ScenarioLevel;
		}
	}

	public CMapScenarioState()
	{
	}

	public CMapScenarioState(CMapScenarioState state, ReferenceDictionary references)
		: base(state, references)
	{
		IsInitialised = state.IsInitialised;
		QuestName = state.QuestName;
		IsStartingScenario = state.IsStartingScenario;
		RoadEventID = state.RoadEventID;
		MapScenarioType = state.MapScenarioType;
		ScenarioID = state.ScenarioID;
		ScenarioFilePath = state.ScenarioFilePath;
		ScenarioGenerationRNGState = references.Get(state.ScenarioGenerationRNGState);
		if (ScenarioGenerationRNGState == null && state.ScenarioGenerationRNGState != null)
		{
			ScenarioGenerationRNGState = new Extensions.RandomState(state.ScenarioGenerationRNGState, references);
			references.Add(state.ScenarioGenerationRNGState, ScenarioGenerationRNGState);
		}
		CachedMonsterClasses = references.Get(state.CachedMonsterClasses);
		if (CachedMonsterClasses == null && state.CachedMonsterClasses != null)
		{
			CachedMonsterClasses = new List<string>();
			for (int i = 0; i < state.CachedMonsterClasses.Count; i++)
			{
				string item = state.CachedMonsterClasses[i];
				CachedMonsterClasses.Add(item);
			}
			references.Add(state.CachedMonsterClasses, CachedMonsterClasses);
		}
		CachedFinalThreatModifier = state.CachedFinalThreatModifier;
		InitialState = references.Get(state.InitialState);
		if (InitialState == null && state.InitialState != null)
		{
			InitialState = new ScenarioState(state.InitialState, references);
			references.Add(state.InitialState, InitialState);
		}
		AutoSaveState = references.Get(state.AutoSaveState);
		if (AutoSaveState == null && state.AutoSaveState != null)
		{
			AutoSaveState = new ScenarioState(state.AutoSaveState, references);
			references.Add(state.AutoSaveState, AutoSaveState);
		}
		NonSerializedInitialState = references.Get(state.NonSerializedInitialState);
		if (NonSerializedInitialState == null && state.NonSerializedInitialState != null)
		{
			NonSerializedInitialState = new ScenarioState(state.NonSerializedInitialState, references);
			references.Add(state.NonSerializedInitialState, NonSerializedInitialState);
		}
		MatchSessionID = state.MatchSessionID;
		TimeActiveSec = state.TimeActiveSec;
		ChestRewards = references.Get(state.ChestRewards);
		if (ChestRewards == null && state.ChestRewards != null)
		{
			ChestRewards = new List<RewardGroup>();
			for (int j = 0; j < state.ChestRewards.Count; j++)
			{
				RewardGroup rewardGroup = state.ChestRewards[j];
				RewardGroup rewardGroup2 = references.Get(rewardGroup);
				if (rewardGroup2 == null && rewardGroup != null)
				{
					rewardGroup2 = new RewardGroup(rewardGroup, references);
					references.Add(rewardGroup, rewardGroup2);
				}
				ChestRewards.Add(rewardGroup2);
			}
			references.Add(state.ChestRewards, ChestRewards);
		}
		m_CustomLevelData = references.Get(state.m_CustomLevelData);
		if (m_CustomLevelData == null && state.m_CustomLevelData != null)
		{
			m_CustomLevelData = new CCustomLevelData(state.m_CustomLevelData, references);
			references.Add(state.m_CustomLevelData, m_CustomLevelData);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("IsInitialised", IsInitialised);
		info.AddValue("QuestName", QuestName);
		info.AddValue("RoadEventID", RoadEventID);
		info.AddValue("MapScenarioType", MapScenarioType);
		info.AddValue("ScenarioID", ScenarioID);
		info.AddValue("ScenarioFilePath", ScenarioFilePath);
		info.AddValue("ScenarioGenerationRNGState", ScenarioGenerationRNGState);
		info.AddValue("CachedMonsterClasses", CachedMonsterClasses);
		info.AddValue("CachedFinalThreatModifier", CachedFinalThreatModifier);
		info.AddValue("InitialState", InitialState);
		info.AddValue("AutoSaveState", AutoSaveState);
		info.AddValue("IsStartingScenario", IsStartingScenario);
		info.AddValue("MatchSessionID", MatchSessionID);
	}

	public CMapScenarioState(SerializationInfo info, StreamingContext context)
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
				case "IsInitialised":
					IsInitialised = info.GetBoolean("IsInitialised");
					break;
				case "QuestName":
					QuestName = info.GetString("QuestName");
					break;
				case "RoadEventID":
					RoadEventID = info.GetString("RoadEventID");
					if (RoadEventID == "ADaggerintheChestID")
					{
						RoadEventID = "ADaggerInTheChestID";
					}
					if (RoadEventID == "TheWomanoftheWoodsID")
					{
						RoadEventID = "TheWomanOftheWoodsID";
					}
					if (RoadEventID == "ShadowsontheWallID")
					{
						RoadEventID = "ShadowsOnTheWallID";
					}
					break;
				case "MapScenarioType":
					MapScenarioType = (EMapScenarioType)info.GetValue("MapScenarioType", typeof(EMapScenarioType));
					break;
				case "ScenarioID":
					ScenarioID = info.GetString("ScenarioID");
					break;
				case "ScenarioFilePath":
					ScenarioFilePath = info.GetString("ScenarioFilePath");
					break;
				case "ScenarioGenerationRNGState":
					ScenarioGenerationRNGState = (Extensions.RandomState)info.GetValue("ScenarioGenerationRNGState", typeof(Extensions.RandomState));
					break;
				case "CachedMonsterClasses":
					CachedMonsterClasses = (List<string>)info.GetValue("CachedMonsterClasses", typeof(List<string>));
					break;
				case "CachedFinalThreatModifier":
					CachedFinalThreatModifier = (float)info.GetValue("CachedFinalThreatModifier", typeof(float));
					break;
				case "InitialState":
					InitialState = (ScenarioState)info.GetValue("InitialState", typeof(ScenarioState));
					break;
				case "AutoSaveState":
					AutoSaveState = (ScenarioState)info.GetValue("AutoSaveState", typeof(ScenarioState));
					break;
				case "IsStartingScenario":
					IsStartingScenario = info.GetBoolean("IsStartingScenario");
					break;
				case "MatchSessionID":
					MatchSessionID = info.GetString("MatchSessionID");
					break;
				case "ScenarioFileName":
					ScenarioID = info.GetString("ScenarioFileName");
					break;
				case "RoadEventName":
				{
					string text = info.GetString("RoadEventName");
					if (text != null)
					{
						if (text.Contains(" ") || !text.Contains("ID"))
						{
							text = text.Replace(" ", string.Empty).Replace("?", string.Empty);
							RoadEventID = text + "ID";
						}
						else
						{
							RoadEventID = text;
						}
					}
					if (RoadEventID == "ADaggerintheChestID")
					{
						RoadEventID = "ADaggerInTheChestID";
					}
					if (RoadEventID == "TheWomanoftheWoodsID")
					{
						RoadEventID = "TheWomanOftheWoodsID";
					}
					if (RoadEventID == "ShadowsontheWallID")
					{
						RoadEventID = "ShadowsOnTheWallID";
					}
					break;
				}
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CMapScenarioState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (Quest != null)
		{
			if (base.UnlockConditionState == null && base.Location.UnlockCondition != null)
			{
				base.UnlockConditionState = new CUnlockConditionState(base.Location.UnlockCondition);
			}
			if (base.UnlockConditionState != null)
			{
				base.UnlockConditionState.CacheUnlockCondition(base.Location.UnlockCondition);
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
	}

	public CMapScenarioState(CMapScenario mapScenario, string questName, bool isStartingScenario = false)
		: base(mapScenario)
	{
		IsInitialised = false;
		QuestName = questName;
		IsStartingScenario = isStartingScenario;
		CachedMonsterClasses = new List<string>();
	}

	public void Init()
	{
		if (IsInitialised)
		{
			return;
		}
		ScenarioID = MapScenario.RollForScenario();
		ScenarioDefinition scenarioDefinition = ScenarioRuleClient.SRLYML.GetScenarioDefinition(ScenarioID);
		if (scenarioDefinition != null)
		{
			ScenarioFilePath = scenarioDefinition.FileName;
			MapScenarioType = EMapScenarioType.YML;
			base.Mesh = scenarioDefinition.ScenarioMeshes[AdventureState.MapState.MapRNG.Next(scenarioDefinition.ScenarioMeshes.Count)];
		}
		else
		{
			MapScenarioType = EMapScenarioType.Custom;
			CCustomLevelData customLevel = ScenarioRuleClient.SRLYML.GetCustomLevel(ScenarioID);
			if (customLevel == null)
			{
				throw new Exception("Unable to load custom level with name " + ScenarioID);
			}
			if (customLevel.MapIconMaterialNames != null && customLevel.MapIconMaterialNames.Count > 0)
			{
				base.Mesh = customLevel.MapIconMaterialNames[AdventureState.MapState.MapRNG.Next(customLevel.MapIconMaterialNames.Count)];
			}
			else if (AdventureState.MapState.IsCampaign)
			{
				string text = new string(Quest.ID.Where(char.IsDigit).ToArray());
				base.Mesh = "Campaign_Icon_" + text;
			}
			else
			{
				base.Mesh = "UNDEFINED";
			}
		}
		RoadEventID = RollForRoadEvent();
		IsInitialised = true;
		SimpleLog.AddToSimpleLog("MapRNG (map initialize): " + AdventureState.MapState.PeekMapRNG);
	}

	public void OnMapStateAdventureStarted()
	{
		if (MapScenarioType == EMapScenarioType.YML && AdventureState.MapState.MapParty.SelectedCharacters.Count() != 0)
		{
			ScenarioDefinition scenarioDefinition = ScenarioRuleClient.SRLYML.GetScenarioDefinition(ScenarioID);
			if (scenarioDefinition == null || MapScenario.ScenarioPool.SingleOrDefault((Tuple<string, int> x) => x.Item1 == ScenarioID) == null || CachedMonsterClasses == null || ScenarioGenerationRNGState == null)
			{
				ScenarioID = MapScenario.RollForScenario();
				if (scenarioDefinition != null)
				{
					ScenarioFilePath = scenarioDefinition.FileName;
					MapScenarioType = EMapScenarioType.YML;
					base.Mesh = scenarioDefinition.ScenarioMeshes[AdventureState.MapState.MapRNG.Next(scenarioDefinition.ScenarioMeshes.Count)];
					RegenerateMapScenario(Quest.Chapter);
					SimpleLog.AddToSimpleLog("MapRNG (refresh from yml): " + AdventureState.MapState.PeekMapRNG);
				}
			}
		}
		if (CachedMonsterClasses != null && CachedMonsterClasses.Count > 0)
		{
			for (int num = CachedMonsterClasses.Count - 1; num >= 0; num--)
			{
				if (MonsterClassManager.Find(CachedMonsterClasses[num]) == null)
				{
					CachedMonsterClasses.RemoveAt(num);
				}
			}
		}
		if (InitialState != null)
		{
			NonSerializedInitialState = InitialState.Copy();
		}
	}

	public void RegenerateMapScenario(int scenarioChapter)
	{
		while (m_IsRegenerating)
		{
			Thread.Sleep(5);
		}
		m_IsRegenerating = true;
		if (!IsInitialised)
		{
			Init();
		}
		RemoveAssociatedDataFromMapState();
		RoadEventID = RollForRoadEvent();
		SimpleLog.AddToSimpleLog("MapRNG (regenerate scenario roll road): " + AdventureState.MapState.PeekMapRNG);
		if (MapScenarioType == EMapScenarioType.YML)
		{
			ScenarioDefinition scenarioDefinition = ScenarioRuleClient.SRLYML.GetScenarioDefinition(ScenarioID);
			if (scenarioDefinition != null)
			{
				float num = AdventureState.MapState.MapParty.ThreatLevel;
				CHeadquarters.ChapterDifficulty chapterDifficulty = AdventureState.MapState.HeadquartersState.Headquarters.ChapterDifficulties.SingleOrDefault((CHeadquarters.ChapterDifficulty s) => s.Chapter == scenarioChapter);
				if (chapterDifficulty != null)
				{
					int subChapter = AdventureState.MapState.GetHighestUnlockedSubChapter(scenarioChapter);
					CHeadquarters.SubChapterDifficulty subChapterDifficulty = chapterDifficulty.SubChapterDifficulties.SingleOrDefault((CHeadquarters.SubChapterDifficulty s) => s.SubChapter == subChapter);
					if (subChapterDifficulty != null)
					{
						num *= subChapterDifficulty.GetModifier(AdventureState.MapState.DifficultySetting);
					}
				}
				base.Mesh = scenarioDefinition.ScenarioMeshes[AdventureState.MapState.MapRNG.Next(scenarioDefinition.ScenarioMeshes.Count)];
				ScenarioGenerationRNGState = AdventureState.MapState.MapRNG.Save();
				CachedFinalThreatModifier = num;
				GenerateYMLScenario(scenarioDefinition, AdventureState.MapState.MapRNG.Next(), ScenarioLevelToUse, num);
				CachedMonsterClasses = new List<string>();
				foreach (EnemyState monster in NonSerializedInitialState.Monsters)
				{
					if (!CachedMonsterClasses.Contains(monster.ClassID))
					{
						CachedMonsterClasses.Add(monster.ClassID);
					}
				}
				SimpleLog.AddToSimpleLog("MapRNG (regenerate yml map): " + AdventureState.MapState.PeekMapRNG);
			}
			else
			{
				DLLDebug.LogError("Unable to find a single scenario definition for scenario with ID: " + ScenarioID);
			}
		}
		else
		{
			if (MapScenarioType != EMapScenarioType.Custom)
			{
				throw new Exception("Invalid MapScenarioType");
			}
			CCustomLevelData customLevelData = CustomLevelData;
			if (customLevelData == null)
			{
				throw new Exception("Unable to load custom level with name " + ScenarioID);
			}
			NonSerializedInitialState = customLevelData.ScenarioState.Copy();
			NonSerializedInitialState.SetScenarioLevel((QuestState != null) ? ScenarioLevelToUse : AdventureState.MapState.MapParty.ScenarioLevel);
			ScenarioGenerationRNGState = AdventureState.MapState.MapRNG.Save();
			SharedLibrary.Random random = ScenarioGenerationRNGState.Restore();
			if (customLevelData.RandomiseOnLoad)
			{
				NonSerializedInitialState.SeedFromMap = random.Next();
			}
			CachedMonsterClasses = new List<string>();
			foreach (EnemyState monster2 in customLevelData.ScenarioState.Monsters)
			{
				if (!CachedMonsterClasses.Contains(monster2.ClassID))
				{
					CachedMonsterClasses.Add(monster2.ClassID);
				}
			}
			if (customLevelData.MapIconMaterialNames != null && customLevelData.MapIconMaterialNames.Count > 0)
			{
				base.Mesh = customLevelData.MapIconMaterialNames[random.Next(customLevelData.MapIconMaterialNames.Count)];
			}
			else if (AdventureState.MapState.IsCampaign)
			{
				string text = new string(Quest.ID.Where(char.IsDigit).ToArray());
				base.Mesh = "Campaign_Icon_" + text;
			}
			else
			{
				base.Mesh = "UNDEFINED";
			}
			SimpleLog.AddToSimpleLog("MapRNG (regenerate custom map): " + AdventureState.MapState.PeekMapRNG);
		}
		m_IsRegenerating = false;
	}

	public void CheckForNonSerializedInitialScenario()
	{
		if (NonSerializedInitialState != null)
		{
			return;
		}
		if (MapScenarioType == EMapScenarioType.YML && ScenarioGenerationRNGState != null)
		{
			ScenarioDefinition scenarioDefinition = ScenarioRuleClient.SRLYML.GetScenarioDefinition(ScenarioID);
			SharedLibrary.Random random = ScenarioGenerationRNGState.Restore();
			GenerateYMLScenario(scenarioDefinition, random.Next(), ScenarioLevelToUse, CachedFinalThreatModifier, random);
		}
		else
		{
			if (MapScenarioType != EMapScenarioType.Custom)
			{
				return;
			}
			NonSerializedInitialState = CustomLevelData.ScenarioState.Copy();
			NonSerializedInitialState.SetScenarioLevel(ScenarioLevelToUse);
			if (CustomLevelData.RandomiseOnLoad)
			{
				if (ScenarioGenerationRNGState == null)
				{
					ScenarioGenerationRNGState = AdventureState.MapState.MapRNG.Save();
				}
				SharedLibrary.Random random2 = ScenarioGenerationRNGState.Restore();
				NonSerializedInitialState.SeedFromMap = random2.Next();
			}
			SimpleLog.AddToSimpleLog("MapRNG (initial custom scenario): " + AdventureState.MapState.PeekMapRNG);
		}
	}

	public void EnterScenario(string matchSessionID)
	{
		if (InitialState == null)
		{
			InitialState = NonSerializedInitialState.Copy();
		}
		MatchSessionID = matchSessionID;
		ChestRewards = new List<RewardGroup>();
		if (AdventureState.MapState.IsCampaign)
		{
			for (int num = InitialState.ChestProps.Count - 1; num >= 0; num--)
			{
				CObjectChest cObjectChest = (CObjectChest)InitialState.ChestProps[num];
				if (cObjectChest.ObjectType != ScenarioManager.ObjectImportType.GoalChest && cObjectChest.ChestTreasureTablesID != null && cObjectChest.ChestTreasureTablesID.Count > 0)
				{
					foreach (string item in cObjectChest.ChestTreasureTablesID)
					{
						if (AdventureState.MapState.AlreadyRewardedChestTreasureTableIDs.Contains(item))
						{
							InitialState.Props.Remove(cObjectChest);
							break;
						}
					}
				}
			}
		}
		CheckForPlayerStates();
		CheckForScenarioModifierActivation();
	}

	public bool EncounteredEventAvailable(CCardDeck eventDeck, string eventType, string locationID)
	{
		CRoadEvent cRoadEvent = null;
		string roadEvent = null;
		if (AdventureState.MapState.IsCampaign && !locationID.ToUpper().Contains("_SOLO_"))
		{
			CRoadEvent cRoadEvent2 = null;
			bool flag = true;
			roadEvent = eventDeck.DrawCard(CCardDeck.EShuffle.None, CCardDeck.EDiscard.None);
			if (!string.IsNullOrEmpty(roadEvent))
			{
				cRoadEvent = MapRuleLibraryClient.MRLYML.RoadEvents.SingleOrDefault((CRoadEvent e) => e.ID == roadEvent);
			}
			else
			{
				flag = false;
			}
			while (flag)
			{
				if (cRoadEvent == cRoadEvent2)
				{
					eventDeck.AddCard(cRoadEvent.ID, CCardDeck.EAddCard.Top);
					cRoadEvent = null;
					flag = false;
					continue;
				}
				if (cRoadEvent2 == null)
				{
					cRoadEvent2 = cRoadEvent;
				}
				if ((locationID.ToUpper().Contains("JOTL") && cRoadEvent.Expansion == "") || !locationID.ToUpper().Contains(cRoadEvent.Expansion.ToUpper()))
				{
					cRoadEvent = GetNewEvent(eventDeck, cRoadEvent);
				}
				else if (cRoadEvent.RequiredClass != null && cRoadEvent.RequiredClass.Count > 0 && !cRoadEvent.RequiredClass.Contains("None"))
				{
					bool flag2 = false;
					foreach (string rclass in cRoadEvent.RequiredClass)
					{
						if (AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter s) => s.CharacterID == rclass) != null)
						{
							flag2 = true;
						}
					}
					if (flag2)
					{
						flag = false;
					}
					else
					{
						cRoadEvent = GetNewEvent(eventDeck, cRoadEvent);
					}
				}
				else
				{
					flag = false;
				}
			}
		}
		if (cRoadEvent != null)
		{
			eventDeck.AddCard(cRoadEvent.ID, CCardDeck.EAddCard.Top, allowDups: false);
		}
		return cRoadEvent != null;
	}

	private CRoadEvent GetNewEvent(CCardDeck eventDeck, CRoadEvent rolledEvent)
	{
		eventDeck.AddCard(rolledEvent.ID, CCardDeck.EAddCard.Bottom, allowDups: false);
		string roadEvent = eventDeck.DrawCard(CCardDeck.EShuffle.None, CCardDeck.EDiscard.None);
		rolledEvent = MapRuleLibraryClient.MRLYML.RoadEvents.SingleOrDefault((CRoadEvent e) => e.ID == roadEvent);
		if (rolledEvent == null)
		{
			rolledEvent = MapRuleLibraryClient.MRLYML.CityEvents.SingleOrDefault((CRoadEvent e) => e.ID == roadEvent);
		}
		return rolledEvent;
	}

	public void CheckExpansionLoaded(CCardDeck eventDeck, string eventType)
	{
		if (eventType == "RoadEvent" && !AdventureState.MapState.MapParty.JOTLEventsLoaded && MapRuleLibraryClient.MRLYML.InitialEvents.JOTLEvents != null && MapRuleLibraryClient.MRLYML.InitialEvents.JOTLEvents.Count > 0)
		{
			AdventureState.MapState.MapParty.JOTLEventsLoaded = true;
			if (!eventDeck.Cards.Any((string x) => x.ToUpper().Contains("JOTL")))
			{
				CCardDeck cCardDeck = new CCardDeck(MapRuleLibraryClient.MRLYML.InitialEvents.JOTLEvents);
				cCardDeck.Shuffle();
				eventDeck.ExpandDeck(cCardDeck.Cards);
			}
		}
	}

	public bool ShouldEncounterRoadEvent(string locationID)
	{
		CheckExpansionLoaded(AdventureState.MapState.MapParty.RoadEventDeck, "RoadEvent");
		if (AdventureState.MapState.IsCampaign && !locationID.ToUpper().Contains("_SOLO_"))
		{
			return true;
		}
		if (RoadEventID != string.Empty)
		{
			int num = 0;
			if (MapScenario.EventChance != null)
			{
				num = MapScenario.EventChance[AdventureState.MapState.MapParty.PartyLevel];
			}
			else if (QuestState.Quest.EventChance != null)
			{
				num = QuestState.Quest.EventChance[AdventureState.MapState.MapParty.PartyLevel];
			}
			else if (AdventureState.MapState.HeadquartersState.Headquarters.EventChance != null)
			{
				num = AdventureState.MapState.HeadquartersState.Headquarters.EventChance[AdventureState.MapState.MapParty.PartyLevel];
			}
			int num2 = AdventureState.MapState.MapRNG.Next(100);
			SimpleLog.AddToSimpleLog("MapRNG (Should Encounter Road Event): " + AdventureState.MapState.PeekMapRNG);
			if (num2 <= num)
			{
				return true;
			}
		}
		return false;
	}

	public void ReRollRoadEvent()
	{
		RoadEventID = RollForRoadEvent();
	}

	private string RollForRoadEvent()
	{
		string text = string.Empty;
		if (MapScenario.EventPool != null && MapScenario.EventPool.Count > 0)
		{
			text = MapYMLShared.RollTupleList(FilterEvents(MapScenario.EventPool));
		}
		else
		{
			if (!IsStartingScenario && QuestState?.Quest.EventPool != null)
			{
				CQuestState questState = QuestState;
				if (questState != null && questState.Quest.EventPool.Count > 0)
				{
					text = MapYMLShared.RollTupleList(FilterEvents(QuestState.Quest.EventPool));
					goto IL_0185;
				}
			}
			if (AdventureState.MapState.HeadquartersState.Headquarters.EventPool != null && AdventureState.MapState.HeadquartersState.Headquarters.EventPool.Count > 0)
			{
				text = MapYMLShared.RollTupleList(FilterEvents(AdventureState.MapState.HeadquartersState.Headquarters.EventPool.Where((Tuple<string, int> t) => !AdventureState.MapState.UsedEventNames.Contains(t.Item1)).ToList()));
				if (text == string.Empty || text == null)
				{
					AdventureState.MapState.UsedEventNames.Clear();
					text = MapYMLShared.RollTupleList(FilterEvents(AdventureState.MapState.HeadquartersState.Headquarters.EventPool));
				}
				if (text != null && text != string.Empty)
				{
					AdventureState.MapState.UsedEventNames.Add(text);
				}
			}
		}
		goto IL_0185;
		IL_0185:
		return text;
	}

	private List<Tuple<string, int>> FilterEvents(List<Tuple<string, int>> roadEvents)
	{
		List<Tuple<string, int>> list = roadEvents.ToList();
		foreach (Tuple<string, int> roadEventTuple in list.ToList())
		{
			CRoadEvent cRoadEvent = MapRuleLibraryClient.MRLYML.RoadEvents.SingleOrDefault((CRoadEvent s) => s.ID == roadEventTuple.Item1);
			if (cRoadEvent == null)
			{
				DLLDebug.LogError("Can't find road event with ID " + roadEventTuple.Item1);
				list.Remove(roadEventTuple);
				continue;
			}
			bool flag = false;
			foreach (CItem item in cRoadEvent.GetEventItemRewards())
			{
				if (item.YMLData.IsProsperityItem && !AdventureState.MapState.IsCampaign)
				{
					if (!AdventureState.MapState.MapParty.GetAllUnlockedItems().Any((CItem a) => a.ID == item.ID) || (from w in AdventureState.MapState.MapParty.GetAllUnlockedItems()
						where w.ID == item.ID
						select w).Count() >= AdventureState.MapState.HeadquartersState.Headquarters.GetMaxItemStock(item.YMLData.Rarity))
					{
						flag = true;
						break;
					}
				}
				else if ((from w in AdventureState.MapState.MapParty.GetAllUnlockedItems()
					where w.ID == item.ID
					select w).Count() >= AdventureState.MapState.HeadquartersState.Headquarters.GetMaxItemStock(item.YMLData.Rarity))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				list.Remove(roadEventTuple);
			}
		}
		return list;
	}

	private void CheckForPlayerStates()
	{
		if (InitialState == null || InitialState.Players.Count > 0)
		{
			return;
		}
		List<PlayerState> list = AdventureState.MapState.MapParty.ExportPlayerStates(InitialState);
		if (Quest.QuestCharacterRequirements != null && Quest.QuestCharacterRequirements.Count > 0)
		{
			int num = int.MaxValue;
			List<string> list2 = new List<string>();
			foreach (QuestYML.CQuestCharacterRequirement questCharacterRequirement in Quest.QuestCharacterRequirements)
			{
				if (questCharacterRequirement.RequiredCharacterCount.HasValue && questCharacterRequirement.RequiredCharacterCount.Value < num)
				{
					num = questCharacterRequirement.RequiredCharacterCount.Value;
				}
				if (!string.IsNullOrEmpty(questCharacterRequirement.RequiredCharacterID))
				{
					list2.Add(questCharacterRequirement.RequiredCharacterID);
				}
			}
			if (list.Count > num)
			{
				int num2 = list.Count - 1;
				while (num2 >= 0 && list.Count((PlayerState x) => !x.IsDead) > num)
				{
					PlayerState playerState = list[num2];
					if (!list2.Contains(playerState.ClassID))
					{
						playerState.CauseOfDeath = CActor.ECauseOfDeath.ActorRemovedFromMap;
					}
					num2--;
				}
			}
			if (list.Count((PlayerState x) => !x.IsDead) > num)
			{
				int num3 = list.Count - 1;
				while (num3 >= 0 && list.Count((PlayerState x) => !x.IsDead) > num)
				{
					list[num3].CauseOfDeath = CActor.ECauseOfDeath.ActorRemovedFromMap;
					num3--;
				}
			}
		}
		InitialState.Players.AddRange(list);
	}

	private void GenerateYMLScenario(ScenarioDefinition scenario, int seed, int scenarioLevel, float partyThreatLevel = 1f, SharedLibrary.Random rngState = null)
	{
		if (scenario == null)
		{
			DLLDebug.LogError("ScenarioDefinition in GenerateYMLScenario is null");
			return;
		}
		int charCount = ((AdventureState.MapState.MapParty.SelectedCharacters.Count() > 0) ? AdventureState.MapState.MapParty.SelectedCharacters.Count() : 2);
		string debugOutput;
		List<Tuple<ScenarioMessage, string>> roomRevealedMessageToMapGuid;
		ScenarioState nonSerializedInitialState = CreateNewScenario(scenario, seed, scenarioLevel, scenario.FileName, MapScenario.LocalisedName, MapScenario.LocalisedDescription, charCount, (rngState != null) ? rngState : AdventureState.MapState.MapRNG, out debugOutput, out roomRevealedMessageToMapGuid, partyThreatLevel);
		NonSerializedInitialState = nonSerializedInitialState;
		if (!AdventureState.MapState.ScenarioStartMessages.Keys.Contains(base.ID))
		{
			CLevelMessage cLevelMessage = CLevelMessage.CreateLevelMessageFromYML(scenario.ScenarioStartMessage, CLevelTrigger.ELevelMessagePredefinedDisplayTrigger.ScenarioStart);
			if (cLevelMessage != null)
			{
				AdventureState.MapState.ScenarioStartMessages.Add(base.ID, cLevelMessage);
			}
		}
		if (!AdventureState.MapState.ScenarioCompleteMessages.Keys.Contains(base.ID))
		{
			CLevelMessage cLevelMessage2 = CLevelMessage.CreateLevelMessageFromYML(scenario.ScenarioCompleteMessage, CLevelTrigger.ELevelMessagePredefinedDisplayTrigger.ScenarioEnd);
			if (cLevelMessage2 != null)
			{
				AdventureState.MapState.ScenarioCompleteMessages.Add(base.ID, cLevelMessage2);
			}
		}
		if (!AdventureState.MapState.ScenarioRoomRevealMessages.Keys.Contains(base.ID))
		{
			List<CLevelMessage> list = new List<CLevelMessage>();
			foreach (Tuple<ScenarioMessage, string> item in roomRevealedMessageToMapGuid)
			{
				CLevelMessage cLevelMessage3 = CLevelMessage.CreateLevelMessageFromYML(item.Item1, CLevelTrigger.ELevelMessagePredefinedDisplayTrigger.RoomRevealed, item.Item2);
				if (cLevelMessage3 != null)
				{
					list.Add(cLevelMessage3);
				}
			}
			AdventureState.MapState.ScenarioRoomRevealMessages.Add(base.ID, list);
		}
		SimpleLog.AddToSimpleLog("MapRNG (generate yml scenario): " + AdventureState.MapState.PeekMapRNG);
	}

	private void CheckForScenarioModifierActivation()
	{
		if (CurrentState == null || !CurrentState.IsFirstLoad)
		{
			return;
		}
		foreach (CScenarioModifier scenarioModifier in CurrentState.ScenarioModifiers)
		{
			if (scenarioModifier.ScenarioModifierActivationType != EScenarioModifierActivationType.None && scenarioModifier.ScenarioModifierActivationID != null && ShouldBeActivated(scenarioModifier))
			{
				scenarioModifier.SetDeactivated(deactivate: false);
			}
		}
	}

	public static bool ShouldBeActivated(CScenarioModifier scenarioModifier)
	{
		bool result = false;
		switch (scenarioModifier.ScenarioModifierActivationType)
		{
		case EScenarioModifierActivationType.AchievementCompleted:
		{
			CPartyAchievement cPartyAchievement = AdventureState.MapState.MapParty.Achievements.SingleOrDefault((CPartyAchievement x) => x.ID == scenarioModifier.ScenarioModifierActivationID);
			if (cPartyAchievement != null && cPartyAchievement.State >= EAchievementState.Completed)
			{
				result = true;
			}
			break;
		}
		case EScenarioModifierActivationType.QuestCompleted:
		{
			CQuestState cQuestState = AdventureState.MapState.AllCompletedQuests.SingleOrDefault((CQuestState x) => x.ID == scenarioModifier.ScenarioModifierActivationID);
			if (cQuestState != null && cQuestState.QuestState == CQuestState.EQuestState.Completed)
			{
				result = true;
			}
			break;
		}
		case EScenarioModifierActivationType.PersonalQuestOwnerInParty:
			foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
			{
				if (selectedCharacter.PersonalQuest.ID == scenarioModifier.ScenarioModifierActivationID)
				{
					result = true;
					break;
				}
			}
			break;
		}
		return result;
	}

	public static ScenarioState CreateNewScenario(ScenarioDefinition scenario, int seed, int scenarioLevel, string scenarioFileName, string scenarioName, string scenarioDescription, int charCount, SharedLibrary.Random rng, out string debugOutput, out List<Tuple<ScenarioMessage, string>> roomRevealedMessageToMapGuid, float partyThreatLevel = 1f)
	{
		debugOutput = string.Empty;
		List<CMap> list = new List<CMap>();
		roomRevealedMessageToMapGuid = new List<Tuple<ScenarioMessage, string>>();
		ScenarioLevelTableEntry scenarioLevelTableEntry = null;
		scenarioLevelTableEntry = ((scenario.ScenarioLevelTable == null) ? AdventureState.MapState.HeadquartersState.Headquarters.SLT.Entries[scenarioLevel] : scenario.ScenarioLevelTable.Entries[scenarioLevel]);
		ScenarioLayout scenarioLayout = scenario.ScenarioLayout;
		ScenarioState scenarioState = new ScenarioState(scenarioName, scenarioDescription, scenario.ID, seed, scenarioLevel, EScenarioType.YML, scenario.FileName, scenarioLayout.WinningObjectives, scenarioLayout.LosingObjectives, scenarioLayout.ScenarioModifiers, scenario.BaseStyle, scenario.ChestTreasureTables, scenario.RewardTreasureTables);
		debugOutput = debugOutput + "Generating Scenario " + scenarioName + "\n";
		debugOutput += $"  - PartyThreatLevel multiplier: {partyThreatLevel}\n";
		try
		{
			foreach (ScenarioRoomEntry room in scenarioLayout.Rooms)
			{
				ScenarioPossibleRoom selectedPossibleRoom = room.PossibleRooms[rng.Next(room.PossibleRooms.Count)];
				try
				{
					ScenarioRoomsYML.RoguelikeRoom roguelikeRoom = new ScenarioRoomsYML.RoguelikeRoom(ScenarioRuleClient.SRLYML.ScenarioRooms.Rooms.Single((ScenarioRoomsYML.RoguelikeRoom x) => x.RoomID == selectedPossibleRoom.Name), selectedPossibleRoom);
					byte[] array = new byte[16];
					rng.NextBytes(array);
					CMap cMap = new CMap(new Guid(array), room.Name, roguelikeRoom.MapTiles[rng.Next(roguelikeRoom.MapTiles.Count)], roguelikeRoom.RoomID, selectedPossibleRoom, room.ParentRoom, room.IsDungeonExitRoom, room.IsAdditionalDungeonEntrance);
					if (room.RoomRevealedMessage != null)
					{
						roomRevealedMessageToMapGuid.Add(new Tuple<ScenarioMessage, string>(room.RoomRevealedMessage, cMap.MapGuid));
					}
					List<MonsterSpawnRates> list2 = new List<MonsterSpawnRates>();
					if (selectedPossibleRoom.MonsterGroup == "DefaultMonsterGroup")
					{
						foreach (MonsterSpawnRates item2 in ScenarioRuleClient.SRLYML.MonsterData.MonsterFamilies.Single((MonsterFamily x) => x.FamilyID == scenario.MonsterFamily).DefaultMonsterGroup)
						{
							list2.Add(item2.Copy());
						}
					}
					else
					{
						MonsterGroup monsterGroup = ScenarioRuleClient.SRLYML.MonsterData.MonsterGroups.SingleOrDefault((MonsterGroup x) => x.GroupID == selectedPossibleRoom.MonsterGroup);
						if (monsterGroup != null)
						{
							foreach (MonsterSpawnRates monster in monsterGroup.Monsters)
							{
								list2.Add(monster.Copy());
							}
						}
						else
						{
							DLLDebug.LogError("An invalid monster group " + selectedPossibleRoom.MonsterGroup + " is referenced in the file " + scenarioFileName);
						}
					}
					Dictionary<CMonsterClass, int> dictionary = new Dictionary<CMonsterClass, int>();
					int baseEliteChance = 25;
					int num = 0;
					float num2 = (float)room.ThreatLevel * partyThreatLevel;
					List<CThreatAllocationBin> list3 = new List<CThreatAllocationBin>();
					foreach (MonsterSpawnRates monsterEntry in list2)
					{
						MonsterThreatValuesEntry monsterThreatValuesEntry = ScenarioRuleClient.SRLYML.MonsterData.MonsterThreatValues.SingleOrDefault((MonsterThreatValuesEntry x) => x.ClassID == monsterEntry.SpawnRatesID);
						if (monsterThreatValuesEntry != null)
						{
							CThreatAllocationBin item = new CThreatAllocationBin(monsterEntry, monsterThreatValuesEntry, num2, baseEliteChance, (charCount > 0) ? charCount : 2);
							list3.Add(item);
						}
						else
						{
							DLLDebug.LogError("Unable to find threat values for monster " + monsterEntry.SpawnRatesID);
						}
					}
					List<CThreatAllocationBin> list4 = list3;
					while (list4.Count > 0)
					{
						foreach (int item3 in from x in Enumerable.Range(0, list4.Count)
							orderby rng.Next()
							select x)
						{
							if (list4[item3].MinThreshold > 0)
							{
								bool flag = false;
								if ((float)(num + list4[item3].ThreatValues.Elite) <= num2)
								{
									flag = rng.Next(100) < list4[item3].EliteChance;
								}
								list4[item3].AddMonster(flag);
								num += (flag ? list4[item3].ThreatValues.Elite : list4[item3].ThreatValues.Normal);
							}
							if ((float)num >= num2)
							{
								break;
							}
						}
						if ((float)num >= num2)
						{
							break;
						}
						list4 = list3.Where((CThreatAllocationBin x) => x.TotalThreat < x.MinThreshold).ToList();
					}
					List<CThreatAllocationBin> list5 = list3.Where((CThreatAllocationBin x) => x.TotalThreat < x.MaxThreshold).ToList();
					while (list5.Count > 0 && (float)num < num2)
					{
						int index = rng.Next(list5.Count);
						bool flag2 = false;
						if ((float)(num + list5[index].ThreatValues.Elite) <= num2)
						{
							flag2 = rng.Next(100) < list5[index].EliteChance;
						}
						list5[index].AddMonster(flag2);
						num += (flag2 ? list5[index].ThreatValues.Elite : list5[index].ThreatValues.Normal);
						if (list5[index].TotalThreat >= list5[index].MaxThreshold)
						{
							list5.Remove(list5[index]);
						}
					}
					foreach (CThreatAllocationBin item4 in list3)
					{
						CMonsterClass cMonsterClass = MonsterClassManager.Find(item4.MonsterEntry.SpawnRatesID);
						if (cMonsterClass != null)
						{
							dictionary.Add(cMonsterClass, item4.GetMonsterCount(elite: false));
							CMonsterClass cMonsterClass2 = MonsterClassManager.FindEliteVariantOfClass(cMonsterClass.ID);
							if (cMonsterClass2 != null)
							{
								dictionary.Add(cMonsterClass2, item4.GetMonsterCount(elite: true));
							}
							else
							{
								DLLDebug.LogError("Unable to find Monster Class for elite version of monster: " + item4.MonsterEntry.SpawnRatesID);
							}
						}
						else
						{
							DLLDebug.LogError("Unable to find Monster Class for monster: " + item4.MonsterEntry.SpawnRatesID);
						}
					}
					if (room.BossInHere && scenario.BossID != null)
					{
						CMonsterClass cMonsterClass3 = MonsterClassManager.Find(scenario.BossID);
						if (cMonsterClass3 != null)
						{
							debugOutput = debugOutput + "  Adding Boss " + scenario.BossID + "\n";
							dictionary.Add(cMonsterClass3, scenario.BossCount);
						}
						else
						{
							DLLDebug.LogError("Unable to find Monster Class for boss " + scenario.BossID);
						}
					}
					foreach (KeyValuePair<CMonsterClass, int> item5 in dictionary)
					{
						for (int num3 = 0; num3 < item5.Value; num3++)
						{
							scenarioState.Monsters.Add(new EnemyState(item5.Key.ID, rng.Next(item5.Key.Models.Count), cMap.MapGuid, CActor.EType.Enemy));
						}
					}
					foreach (Tuple<string, List<int>> allyMonster in selectedPossibleRoom.AllyMonsters)
					{
						for (int num4 = 0; num4 < allyMonster.Item2[charCount - 1]; num4++)
						{
							CMonsterClass cMonsterClass4 = MonsterClassManager.Find(allyMonster.Item1);
							if (cMonsterClass4 != null)
							{
								scenarioState.AllyMonsters.Add(new EnemyState(cMonsterClass4.ID, rng.Next(cMonsterClass4.Models.Count), cMap.MapGuid, CActor.EType.Ally));
							}
							else
							{
								DLLDebug.LogError("Proc gen was unable to find monsterpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartial class for Ally Monster" + allyMonster.Item1 + " in the file " + scenarioFileName);
							}
						}
					}
					foreach (Tuple<string, List<int>> enemy2Monster in selectedPossibleRoom.Enemy2Monsters)
					{
						for (int num5 = 0; num5 < enemy2Monster.Item2[charCount - 1]; num5++)
						{
							CMonsterClass cMonsterClass5 = MonsterClassManager.Find(enemy2Monster.Item1);
							if (cMonsterClass5 != null)
							{
								scenarioState.Enemy2Monsters.Add(new EnemyState(cMonsterClass5.ID, rng.Next(cMonsterClass5.Models.Count), cMap.MapGuid, CActor.EType.Enemy2));
							}
							else
							{
								DLLDebug.LogError("Proc gen was unable to find monsterpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartial class for Enemy 2 Monster" + enemy2Monster.Item1 + " in the file " + scenarioFileName);
							}
						}
					}
					foreach (Tuple<string, List<int>> @object in selectedPossibleRoom.Objects)
					{
						for (int num6 = 0; num6 < @object.Item2[charCount - 1]; num6++)
						{
							CObjectClass cObjectClass = MonsterClassManager.FindObjectClass(@object.Item1);
							if (cObjectClass != null)
							{
								scenarioState.Objects.Add(new ObjectState(cObjectClass.ID, rng.Next(cObjectClass.Models.Count), cMap.MapGuid, CActor.EType.Enemy));
							}
							else
							{
								DLLDebug.LogError("Proc gen was unable to find objectpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartial class for Objects" + @object.Item1 + " in the file " + scenarioFileName);
							}
						}
					}
					debugOutput = debugOutput + "  Adding room " + roguelikeRoom.RoomID + "\n";
					int propCount = GetPropCount(roguelikeRoom.GoldPiles, rng);
					if (propCount > 0)
					{
						for (int num7 = 0; num7 < propCount; num7++)
						{
							scenarioState.Props.Add(new CObjectGoldPile(rng, EPropType.GoldPile.ToString(), ScenarioManager.ObjectImportType.MoneyToken, cMap.MapGuid));
						}
						debugOutput = debugOutput + "  - Added " + propCount + " gold piles to room " + roguelikeRoom.RoomID + "\n";
					}
					int propCount2 = GetPropCount(roguelikeRoom.TreasureChestChance, rng);
					if (propCount2 > 0)
					{
						for (int num8 = 0; num8 < propCount2; num8++)
						{
							scenarioState.Props.Add(new CObjectChest(rng, EPropType.Chest.ToString(), ScenarioManager.ObjectImportType.Chest, cMap.MapGuid));
						}
						debugOutput = debugOutput + "  - Added " + propCount2 + " chests to " + roguelikeRoom.RoomID + "\n";
					}
					int count = selectedPossibleRoom.GoalChests.Count;
					if (count > 0)
					{
						for (int num9 = 0; num9 < count; num9++)
						{
							scenarioState.Props.Add(new CObjectChest(rng, EPropType.GoalChest.ToString(), ScenarioManager.ObjectImportType.GoalChest, cMap.MapGuid, selectedPossibleRoom.GoalChests[num9]));
						}
						debugOutput = debugOutput + "  - Added " + count + " goal chests to " + roguelikeRoom.RoomID + "\n";
					}
					int propCount3 = GetPropCount(roguelikeRoom.Traps, rng);
					if (propCount3 > 0)
					{
						for (int num10 = 0; num10 < propCount3; num10++)
						{
							scenarioState.Props.Add(new CObjectTrap(rng, EPropType.Trap.ToString(), ScenarioManager.ObjectImportType.Trap, cMap.MapGuid, new List<CCondition.ENegativeCondition>(), damage: true, scenarioLevelTableEntry.TrapDamage));
						}
						debugOutput = debugOutput + "  - Added " + propCount3 + " traps to " + roguelikeRoom.RoomID + "\n";
					}
					int propCount4 = GetPropCount(roguelikeRoom.OneHexObstacles, rng);
					if (propCount4 > 0)
					{
						for (int num11 = 0; num11 < propCount4; num11++)
						{
							scenarioState.Props.Add(new CObjectObstacle(rng, EPropType.OneHexObstacle.ToString(), ScenarioManager.ObjectImportType.Obstacle, cMap.MapGuid));
						}
						debugOutput = debugOutput + "  - Added " + propCount4 + " single hex obstacles to " + roguelikeRoom.RoomID + "\n";
					}
					int propCount5 = GetPropCount(roguelikeRoom.TwoHexObstacles, rng);
					if (propCount5 > 0)
					{
						for (int num12 = 0; num12 < propCount5; num12++)
						{
							scenarioState.Props.Add(new CObjectObstacle(rng, EPropType.TwoHexObstacle.ToString(), ScenarioManager.ObjectImportType.Obstacle, cMap.MapGuid));
						}
						debugOutput = debugOutput + "  - Added " + propCount5 + " two hex obstacles to " + roguelikeRoom.RoomID + "\n";
					}
					int propCount6 = GetPropCount(roguelikeRoom.ThreeHexObstacles, rng);
					if (propCount6 > 0)
					{
						for (int num13 = 0; num13 < propCount6; num13++)
						{
							scenarioState.Props.Add(new CObjectObstacle(rng, EPropType.ThreeHexObstacle.ToString(), ScenarioManager.ObjectImportType.Obstacle, cMap.MapGuid));
						}
						debugOutput = debugOutput + "  - Added " + propCount6 + " three hex obstacles to " + roguelikeRoom.RoomID + "\n";
					}
					foreach (SpawnerData spawnerData in selectedPossibleRoom.SpawnerDatas)
					{
						byte[] array2 = new byte[16];
						rng.NextBytes(array2);
						Guid guid = new Guid(array2);
						SpawnerData.ESpawnerEntryDifficulty spawnerEntryDifficulty = SpawnerData.SpawnerEntryDifficulties.SingleOrDefault((SpawnerData.ESpawnerEntryDifficulty s) => s.ToString() == AdventureState.MapState.DifficultySetting.ToString());
						scenarioState.Spawners.Add(new CSpawner(spawnerData, null, cMap.MapGuid, guid.ToString(), spawnerEntryDifficulty));
					}
					int propCount7 = GetPropCount(roguelikeRoom.PressurePlates, rng);
					if (propCount7 > 0)
					{
						for (int num14 = 0; num14 < propCount7; num14++)
						{
							scenarioState.Props.Add(new CObjectPressurePlate(rng, EPropType.PressurePlate.ToString(), ScenarioManager.ObjectImportType.PressurePlate, cMap.MapGuid));
						}
						debugOutput = debugOutput + "  - Added " + propCount7 + " pressure plates to " + roguelikeRoom.RoomID + "\n";
					}
					int propCount8 = GetPropCount(roguelikeRoom.TerrainHotCoals, rng);
					if (propCount8 > 0)
					{
						for (int num15 = 0; num15 < propCount8; num15++)
						{
							scenarioState.Props.Add(new CObjectHazardousTerrain(rng, EPropType.TerrainHotCoals.ToString(), ScenarioManager.ObjectImportType.TerrainHotCoals, cMap.MapGuid));
						}
						debugOutput = debugOutput + "  - Added " + propCount8 + " terrain hot coals to " + roguelikeRoom.RoomID + "\n";
					}
					int propCount9 = GetPropCount(roguelikeRoom.TerrainWater, rng);
					if (propCount9 > 0)
					{
						for (int num16 = 0; num16 < propCount9; num16++)
						{
							scenarioState.Props.Add(new CObjectDifficultTerrain(rng, EPropType.TerrainWater.ToString(), ScenarioManager.ObjectImportType.TerrainWater, cMap.MapGuid));
						}
						debugOutput = debugOutput + "  - Added " + propCount9 + " terrain hot coals to " + roguelikeRoom.RoomID + "\n";
					}
					int propCount10 = GetPropCount(roguelikeRoom.TerrainThorns, rng);
					if (propCount10 > 0)
					{
						for (int num17 = 0; num17 < propCount10; num17++)
						{
							scenarioState.Props.Add(new CObjectHazardousTerrain(rng, EPropType.TerrainThorns.ToString(), ScenarioManager.ObjectImportType.TerrainThorns, cMap.MapGuid));
						}
						debugOutput = debugOutput + "  - Added " + propCount10 + " terrain thorns to " + roguelikeRoom.RoomID + "\n";
					}
					int propCount11 = GetPropCount(roguelikeRoom.TerrainRubble, rng);
					if (propCount11 > 0)
					{
						for (int num18 = 0; num18 < propCount11; num18++)
						{
							scenarioState.Props.Add(new CObjectDifficultTerrain(rng, EPropType.TerrainRubble.ToString(), ScenarioManager.ObjectImportType.TerrainRubble, cMap.MapGuid));
						}
						debugOutput = debugOutput + "  - Added " + propCount11 + " terrain hot coals to " + roguelikeRoom.RoomID + "\n";
					}
					list.Add(cMap);
				}
				catch (Exception ex)
				{
					DLLDebug.LogError("Unable to create room for ProcGenMapInput.  The definition for room type " + selectedPossibleRoom.Name + " is not valid.  Scenario: " + scenarioFileName + "\n" + ex.Message + "\n" + ex.StackTrace);
				}
			}
		}
		catch (Exception ex2)
		{
			DLLDebug.LogError("An unexpected exception occurred attempting to create ProcGenMapInput.\n" + ex2.Message + "\n" + ex2.StackTrace);
		}
		if (list.Count == 0)
		{
			DLLDebug.LogError("No maps have been added to the ProcGenMapInput.  Procgen will load in with the default values.");
		}
		CMap.SetChildren(list);
		scenarioState.Maps = list;
		debugOutput += "\n";
		return scenarioState;
	}

	private static int GetPropCount(List<int> chances, SharedLibrary.Random rng)
	{
		if (chances == null)
		{
			return 0;
		}
		int num = 0;
		foreach (int chance in chances)
		{
			if (rng.Next(100) + 1 <= chance)
			{
				num++;
			}
		}
		return num;
	}

	public void ClearAutoSaveState()
	{
		AutoSaveState = null;
	}

	public void UpdateAutoSaveState(ScenarioState saveState)
	{
		AutoSaveState = saveState;
	}

	public void UpdateInitialSaveState(ScenarioState initialState)
	{
		InitialState = initialState;
	}

	public void RemoveAssociatedDataFromMapState()
	{
		InitialState = null;
		AutoSaveState = null;
		try
		{
			AdventureState.MapState.ScenarioStartMessages.Remove(base.ID);
		}
		catch
		{
		}
		try
		{
			AdventureState.MapState.ScenarioCompleteMessages.Remove(base.ID);
		}
		catch
		{
		}
		try
		{
			AdventureState.MapState.ScenarioRoomRevealMessages.Remove(base.ID);
		}
		catch
		{
		}
	}
}
