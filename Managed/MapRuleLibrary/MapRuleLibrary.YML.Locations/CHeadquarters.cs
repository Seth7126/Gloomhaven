using System;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.YML.Quest;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;

namespace MapRuleLibrary.YML.Locations;

public class CHeadquarters : CLocation
{
	public class ChapterDifficulty
	{
		public int Chapter;

		public List<SubChapterDifficulty> SubChapterDifficulties;

		public ChapterDifficulty(int chapter, List<SubChapterDifficulty> subChapterDifficulties)
		{
			Chapter = chapter;
			SubChapterDifficulties = subChapterDifficulties;
		}
	}

	public class SubChapterDifficulty
	{
		public int SubChapter;

		public float FriendlyMultiplier;

		public float EasyMultiplier;

		public float NormalMultiplier;

		public float HardMultiplier;

		public float InsaneMultiplier;

		public float DeadlyMultiplier;

		public SubChapterDifficulty(int subChapter, float friendlyMultiplier, float easyMultiplier, float normalMultiplier, float hardMultiplier, float insaneMultiplier, float deadlyMultiplier)
		{
			SubChapter = subChapter;
			FriendlyMultiplier = friendlyMultiplier;
			EasyMultiplier = easyMultiplier;
			NormalMultiplier = normalMultiplier;
			HardMultiplier = hardMultiplier;
			InsaneMultiplier = insaneMultiplier;
			DeadlyMultiplier = deadlyMultiplier;
		}

		public float GetModifier(EAdventureDifficulty difficulty)
		{
			return difficulty switch
			{
				EAdventureDifficulty.Friendly => FriendlyMultiplier, 
				EAdventureDifficulty.Easy => EasyMultiplier, 
				EAdventureDifficulty.Normal => NormalMultiplier, 
				EAdventureDifficulty.Hard => HardMultiplier, 
				EAdventureDifficulty.Insane => InsaneMultiplier, 
				EAdventureDifficulty.Deadly => DeadlyMultiplier, 
				_ => 1f, 
			};
		}
	}

	public static readonly EHQStores[] HQStores = (EHQStores[])Enum.GetValues(typeof(EHQStores));

	private List<Tuple<CItem.EItemRarity, int>> m_MaxItemStock;

	public string GameMode { get; private set; }

	public List<string> QuestNames { get; private set; }

	public List<string> TutorialQuestNames { get; private set; }

	public List<string> TutorialMessages { get; private set; }

	public int StartingPerksAmount { get; private set; }

	public List<string> RetirementTreasureTableNames { get; private set; }

	public List<string> CharacterUnlockAlternateTreasureTableNames { get; private set; }

	public List<string> StartupTreasureTableNames { get; private set; }

	public int CreateCharacterGoldPerLevelAmount { get; private set; }

	public List<string> CreateCharacterTreasureTableNames { get; private set; }

	public List<CMapScenario> StartingScenarios { get; private set; }

	public List<int> EventChance { get; private set; }

	public List<Tuple<string, int>> EventPool { get; private set; }

	public ScenarioLevelTable SLT { get; private set; }

	public List<CVector3> JobMapLocations { get; private set; }

	public List<Tuple<string, CVector3>> StreetMapLocations { get; private set; }

	public List<Tuple<string, CVector3>> WorldMapLabelLocations { get; private set; }

	public List<ChapterDifficulty> ChapterDifficulties { get; private set; }

	public List<int> LevelToXP { get; private set; }

	public List<int> LevelToProsperity { get; private set; }

	public List<int> ReputationToShopDiscount { get; private set; }

	public List<CQuest> Quests => MapRuleLibraryClient.MRLYML.Quests.Where((CQuest w) => QuestNames.Any((string a) => w.ID == a)).ToList();

	public List<CQuest> JobQuests => MapRuleLibraryClient.MRLYML.Quests.Where((CQuest w) => QuestNames.Any((string a) => w.ID == a && w.Type == EQuestType.Job)).ToList();

	public List<TreasureTable> RetirementTreasureTables => ScenarioRuleClient.SRLYML.TreasureTables.Where((TreasureTable w) => RetirementTreasureTableNames.Contains(w.Name)).ToList();

	public List<TreasureTable> CharacterUnlockAlternateTreasureTables => ScenarioRuleClient.SRLYML.TreasureTables.Where((TreasureTable w) => CharacterUnlockAlternateTreasureTableNames.Contains(w.Name)).ToList();

	public List<TreasureTable> StartupTreasureTables => ScenarioRuleClient.SRLYML.TreasureTables.Where((TreasureTable w) => StartupTreasureTableNames.Contains(w.Name)).ToList();

	public List<TreasureTable> CreateCharacterTreasureTables => ScenarioRuleClient.SRLYML.TreasureTables.Where((TreasureTable w) => CreateCharacterTreasureTableNames.Contains(w.Name)).ToList();

	public List<CAdventureDifficulty> Difficulty => MapRuleLibraryClient.MRLYML.Difficulty.Where((DifficultyYMLData w) => w.GameMode == GameMode).SelectMany((DifficultyYMLData s) => s.DifficultySettings).ToList();

	public int GetMaxItemStock(CItem.EItemRarity rarity)
	{
		if (m_MaxItemStock.Any((Tuple<CItem.EItemRarity, int> a) => a.Item1 == rarity))
		{
			int num = m_MaxItemStock.First((Tuple<CItem.EItemRarity, int> f) => f.Item1 == rarity).Item2;
			if (rarity == CItem.EItemRarity.Common)
			{
				num = Math.Max(num, AdventureState.MapState.MapParty.CheckCharacters.Count);
			}
			return num;
		}
		return 0;
	}

	public CHeadquarters(string id, string localisedName, string localisedDescription, string mesh, TileIndex mapLocation, List<TileIndex> jobMapLocations, List<Tuple<string, TileIndex>> streetMapLocations, CUnlockCondition unlockCondition, string fileName, string gameMode, List<string> questNames, List<string> tutorialQuestNames, List<string> tutorialMessages, int startingPerksAmount, List<string> retirementTreasureTableNames, List<string> characterUnlockAlternateTreasureTableNames, List<string> startupTreasureTableNames, int createCharacterGoldPerLevelAmount, List<string> createCharacterTreasureTableNames, List<CMapScenario> startingScenarios, List<int> eventChance, List<Tuple<string, int>> eventPool, ScenarioLevelTable slt, List<Tuple<CItem.EItemRarity, int>> maxItemStock, List<ChapterDifficulty> chapterDifficulties, List<int> levelToXP, List<int> levelToProsperity, List<int> reputationToShopDiscount, List<Tuple<string, TileIndex>> worldMapLabelLocations)
		: base(id, localisedName, localisedDescription, mesh, mapLocation, unlockCondition, fileName)
	{
		GameMode = gameMode;
		QuestNames = questNames;
		TutorialQuestNames = tutorialQuestNames;
		TutorialMessages = tutorialMessages;
		StartingPerksAmount = startingPerksAmount;
		RetirementTreasureTableNames = retirementTreasureTableNames;
		CharacterUnlockAlternateTreasureTableNames = characterUnlockAlternateTreasureTableNames;
		StartupTreasureTableNames = startupTreasureTableNames;
		CreateCharacterGoldPerLevelAmount = createCharacterGoldPerLevelAmount;
		CreateCharacterTreasureTableNames = createCharacterTreasureTableNames;
		StartingScenarios = startingScenarios;
		EventChance = eventChance;
		EventPool = eventPool;
		SLT = slt;
		ChapterDifficulties = chapterDifficulties;
		LevelToXP = levelToXP;
		LevelToProsperity = levelToProsperity;
		ReputationToShopDiscount = reputationToShopDiscount;
		JobMapLocations = new List<CVector3>();
		foreach (TileIndex jobMapLocation in jobMapLocations)
		{
			JobMapLocations.Add(MapYMLShared.GetScreenPointFromMap(jobMapLocation.X, jobMapLocation.Y));
		}
		StreetMapLocations = new List<Tuple<string, CVector3>>();
		foreach (Tuple<string, TileIndex> streetMapLocation in streetMapLocations)
		{
			StreetMapLocations.Add(new Tuple<string, CVector3>(streetMapLocation.Item1, MapYMLShared.GetScreenPointFromMap(streetMapLocation.Item2.X, streetMapLocation.Item2.Y)));
		}
		WorldMapLabelLocations = new List<Tuple<string, CVector3>>();
		foreach (Tuple<string, TileIndex> worldMapLabelLocation in worldMapLabelLocations)
		{
			WorldMapLabelLocations.Add(new Tuple<string, CVector3>(worldMapLabelLocation.Item1, MapYMLShared.GetScreenPointFromMap(worldMapLabelLocation.Item2.X, worldMapLabelLocation.Item2.Y)));
		}
		m_MaxItemStock = maxItemStock;
	}

	public new bool Validate()
	{
		bool result = base.Validate();
		if (GameMode == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(base.FileName, "No GameMode specified in Headquarters.  File: " + base.FileName);
			result = false;
		}
		if (EventPool != null || EventChance != null)
		{
			if (EventPool == null || EventChance == null)
			{
				SharedClient.ValidationRecord.RecordParseFailure(base.FileName, "Both Event Pool and Event Chance must be specified together.  File: " + base.FileName);
				result = false;
			}
			else if (EventChance.Count != 8)
			{
				SharedClient.ValidationRecord.RecordParseFailure(base.FileName, "Invalid Event Chance array size.  Must be exactly " + 8 + " length.  File: " + base.FileName);
				result = false;
			}
		}
		if (JobMapLocations.Count <= 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(base.FileName, "No JobLocations specified in Headquarters file: " + base.FileName);
			result = false;
		}
		if (MapRuleLibraryClient.MRLYML.MapMode == ScenarioManager.EDLLMode.Campaign && StreetMapLocations.Count <= 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(base.FileName, "No StreetMapLocations specified in Headquarters file: " + base.FileName);
			result = false;
		}
		if (LevelToXP == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(base.FileName, "CharacterXPTable is missing from HQ. File: " + base.FileName);
			result = false;
		}
		else if (LevelToXP.Count != 10)
		{
			SharedClient.ValidationRecord.RecordParseFailure(base.FileName, "CharacterXPTable has an invalid number of entries.  There must be exactly " + 9 + " entries. File: " + base.FileName);
			result = false;
		}
		if (SLT == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(base.FileName, "No Scenario Level Table specified in HQ. File: " + base.FileName);
			result = false;
		}
		return result;
	}

	public void UpdateData(string localisedName, string localisedDescription, string mesh, TileIndex mapLocation, List<TileIndex> jobMapLocations, List<TileIndex> removeJobMapLocations, List<Tuple<string, TileIndex>> streetMapLocations, List<Tuple<string, TileIndex>> removeStreetMapLocations, CUnlockCondition unlockCondition, string gameMode, List<string> questNames, List<string> removeQuestNames, List<string> tutorialQuestNames, List<string> removeTutorialQuestNames, List<string> tutorialMessages, List<string> removeTutorialMessages, int startingPerksAmount, List<string> retirementTreasureTableNames, List<string> removeRetirementTreasureTableNames, List<string> characterUnlockAlternateTreasureTableNames, List<string> removeCharacterUnlockAlternateTreasureTableNames, List<string> startupTreasureTableNames, List<string> removeStartupTreasureTableNames, int createCharacterGoldPerLevelAmount, List<string> createCharacterTreasureTableNames, List<string> removeCreateCharacterTreasureTableNames, List<CMapScenario> startingScenarios, List<int> eventChance, List<Tuple<string, int>> eventPool, List<string> removeEventPool, ScenarioLevelTable slt, List<Tuple<CItem.EItemRarity, int>> maxItemStock, List<ChapterDifficulty> chapterDifficulties, List<int> levelToXP, List<int> levelToProsperity, List<int> reputationToShopDiscount, List<Tuple<string, TileIndex>> worldMapLabelLocations)
	{
		UpdateData(localisedName, localisedDescription, mesh, mapLocation, unlockCondition);
		if (gameMode != null)
		{
			GameMode = gameMode;
		}
		if (jobMapLocations.Count > 0)
		{
			foreach (TileIndex jobMapLocation in jobMapLocations)
			{
				CVector3 location = MapYMLShared.GetScreenPointFromMap(jobMapLocation.X, jobMapLocation.Y);
				if (!JobMapLocations.Exists((CVector3 e) => CVector3.Compare(e, location)))
				{
					JobMapLocations.Add(location);
				}
			}
		}
		if (removeJobMapLocations.Count > 0)
		{
			foreach (TileIndex removeJobMapLocation in removeJobMapLocations)
			{
				CVector3 location2 = MapYMLShared.GetScreenPointFromMap(removeJobMapLocation.X, removeJobMapLocation.Y);
				CVector3 cVector = JobMapLocations.SingleOrDefault((CVector3 s) => CVector3.Compare(s, location2));
				if (cVector != null)
				{
					JobMapLocations.Remove(cVector);
				}
			}
		}
		if (MapRuleLibraryClient.MRLYML.MapMode == ScenarioManager.EDLLMode.Campaign && streetMapLocations.Count > 0)
		{
			foreach (Tuple<string, TileIndex> streetMapLocation in streetMapLocations)
			{
				CVector3 location3 = MapYMLShared.GetScreenPointFromMap(streetMapLocation.Item2.X, streetMapLocation.Item2.Y);
				if (!StreetMapLocations.Exists((Tuple<string, CVector3> e) => CVector3.Compare(e.Item2, location3)))
				{
					StreetMapLocations.Add(new Tuple<string, CVector3>(streetMapLocation.Item1, location3));
				}
			}
		}
		if (MapRuleLibraryClient.MRLYML.MapMode == ScenarioManager.EDLLMode.Campaign && removeStreetMapLocations.Count > 0)
		{
			foreach (Tuple<string, TileIndex> removeStreetMapLocation in removeStreetMapLocations)
			{
				CVector3 location4 = MapYMLShared.GetScreenPointFromMap(removeStreetMapLocation.Item2.X, removeStreetMapLocation.Item2.Y);
				Tuple<string, CVector3> tuple = StreetMapLocations.SingleOrDefault((Tuple<string, CVector3> s) => CVector3.Compare(s.Item2, location4));
				if (tuple != null)
				{
					StreetMapLocations.Remove(tuple);
				}
			}
		}
		if (worldMapLabelLocations.Count > 0)
		{
			foreach (Tuple<string, TileIndex> worldMapLabelLocation in worldMapLabelLocations)
			{
				CVector3 location5 = MapYMLShared.GetScreenPointFromMap(worldMapLabelLocation.Item2.X, worldMapLabelLocation.Item2.Y);
				if (!WorldMapLabelLocations.Exists((Tuple<string, CVector3> e) => CVector3.Compare(e.Item2, location5)))
				{
					WorldMapLabelLocations.Add(new Tuple<string, CVector3>(worldMapLabelLocation.Item1, location5));
				}
			}
		}
		if (questNames.Count > 0)
		{
			foreach (string questName in questNames)
			{
				if (!QuestNames.Contains(questName))
				{
					QuestNames.Add(questName);
				}
			}
		}
		if (removeQuestNames.Count > 0)
		{
			foreach (string removeQuestName in removeQuestNames)
			{
				if (QuestNames.Contains(removeQuestName))
				{
					QuestNames.Remove(removeQuestName);
				}
			}
		}
		if (tutorialQuestNames.Count > 0)
		{
			foreach (string tutorialQuestName in tutorialQuestNames)
			{
				if (!TutorialQuestNames.Contains(tutorialQuestName))
				{
					TutorialQuestNames.Add(tutorialQuestName);
				}
			}
		}
		if (removeTutorialQuestNames.Count > 0)
		{
			foreach (string removeTutorialQuestName in removeTutorialQuestNames)
			{
				if (TutorialQuestNames.Contains(removeTutorialQuestName))
				{
					TutorialQuestNames.Remove(removeTutorialQuestName);
				}
			}
		}
		if (tutorialMessages.Count > 0)
		{
			foreach (string tutorialMessage in tutorialMessages)
			{
				if (!TutorialMessages.Contains(tutorialMessage))
				{
					TutorialMessages.Add(tutorialMessage);
				}
			}
		}
		if (removeTutorialMessages.Count > 0)
		{
			foreach (string removeTutorialMessage in removeTutorialMessages)
			{
				if (TutorialMessages.Contains(removeTutorialMessage))
				{
					TutorialMessages.Remove(removeTutorialMessage);
				}
			}
		}
		StartingPerksAmount = startingPerksAmount;
		if (retirementTreasureTableNames.Count > 0)
		{
			foreach (string retirementTreasureTableName in retirementTreasureTableNames)
			{
				if (!RetirementTreasureTableNames.Contains(retirementTreasureTableName))
				{
					RetirementTreasureTableNames.Add(retirementTreasureTableName);
				}
			}
		}
		if (removeRetirementTreasureTableNames.Count > 0)
		{
			foreach (string removeRetirementTreasureTableName in removeRetirementTreasureTableNames)
			{
				if (RetirementTreasureTableNames.Contains(removeRetirementTreasureTableName))
				{
					RetirementTreasureTableNames.Remove(removeRetirementTreasureTableName);
				}
			}
		}
		if (characterUnlockAlternateTreasureTableNames.Count > 0)
		{
			foreach (string characterUnlockAlternateTreasureTableName in characterUnlockAlternateTreasureTableNames)
			{
				if (!CharacterUnlockAlternateTreasureTableNames.Contains(characterUnlockAlternateTreasureTableName))
				{
					CharacterUnlockAlternateTreasureTableNames.Add(characterUnlockAlternateTreasureTableName);
				}
			}
		}
		if (removeCharacterUnlockAlternateTreasureTableNames.Count > 0)
		{
			foreach (string removeCharacterUnlockAlternateTreasureTableName in removeCharacterUnlockAlternateTreasureTableNames)
			{
				if (CharacterUnlockAlternateTreasureTableNames.Contains(removeCharacterUnlockAlternateTreasureTableName))
				{
					CharacterUnlockAlternateTreasureTableNames.Remove(removeCharacterUnlockAlternateTreasureTableName);
				}
			}
		}
		if (startupTreasureTableNames.Count > 0)
		{
			foreach (string startupTreasureTableName in startupTreasureTableNames)
			{
				if (!StartupTreasureTableNames.Contains(startupTreasureTableName))
				{
					StartupTreasureTableNames.Add(startupTreasureTableName);
				}
			}
		}
		if (removeStartupTreasureTableNames.Count > 0)
		{
			foreach (string removeStartupTreasureTableName in removeStartupTreasureTableNames)
			{
				if (StartupTreasureTableNames.Contains(removeStartupTreasureTableName))
				{
					StartupTreasureTableNames.Remove(removeStartupTreasureTableName);
				}
			}
		}
		CreateCharacterGoldPerLevelAmount = createCharacterGoldPerLevelAmount;
		if (createCharacterTreasureTableNames.Count > 0)
		{
			foreach (string createCharacterTreasureTableName in createCharacterTreasureTableNames)
			{
				if (!CreateCharacterTreasureTableNames.Contains(createCharacterTreasureTableName))
				{
					CreateCharacterTreasureTableNames.Add(createCharacterTreasureTableName);
				}
			}
		}
		if (removeCreateCharacterTreasureTableNames.Count > 0)
		{
			foreach (string removeCreateCharacterTreasureTableName in removeCreateCharacterTreasureTableNames)
			{
				if (CreateCharacterTreasureTableNames.Contains(removeCreateCharacterTreasureTableName))
				{
					CreateCharacterTreasureTableNames.Remove(removeCreateCharacterTreasureTableName);
				}
			}
		}
		if (startingScenarios.Count > 0)
		{
			StartingScenarios = startingScenarios;
		}
		if (eventChance != null)
		{
			EventChance = eventChance;
		}
		if (eventPool != null && eventPool.Count > 0)
		{
			if (EventPool == null)
			{
				EventPool = new List<Tuple<string, int>>();
			}
			foreach (Tuple<string, int> roadEvent in eventPool)
			{
				Tuple<string, int> tuple2 = EventPool.SingleOrDefault((Tuple<string, int> s) => s.Item1 == roadEvent.Item1);
				if (tuple2 != null)
				{
					EventPool.Remove(tuple2);
				}
				EventPool.Add(roadEvent);
			}
		}
		if (removeEventPool != null && removeEventPool.Count > 0)
		{
			if (EventPool == null)
			{
				EventPool = new List<Tuple<string, int>>();
			}
			foreach (string roadEvent2 in removeEventPool)
			{
				Tuple<string, int> tuple3 = EventPool.SingleOrDefault((Tuple<string, int> s) => s.Item1 == roadEvent2);
				if (tuple3 != null)
				{
					EventPool.Remove(tuple3);
				}
			}
		}
		if (slt != null)
		{
			SLT = slt;
		}
		if (maxItemStock.Count > 0)
		{
			m_MaxItemStock = maxItemStock;
		}
		if (chapterDifficulties.Count > 0)
		{
			foreach (ChapterDifficulty chapterDifficulty in chapterDifficulties)
			{
				ChapterDifficulty chapterDifficulty2 = ChapterDifficulties.Find((ChapterDifficulty x) => x.Chapter == chapterDifficulty.Chapter);
				if (chapterDifficulty2 != null)
				{
					ChapterDifficulties.Remove(chapterDifficulty2);
				}
				ChapterDifficulties.Add(chapterDifficulty);
			}
		}
		if (levelToXP != null)
		{
			LevelToXP = levelToXP;
		}
		if (levelToProsperity != null)
		{
			LevelToProsperity = levelToProsperity;
		}
		if (reputationToShopDiscount != null)
		{
			ReputationToShopDiscount = reputationToShopDiscount;
		}
	}
}
