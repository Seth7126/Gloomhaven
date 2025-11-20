using System;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Client;
using MapRuleLibrary.YML.Locations;
using MapRuleLibrary.YML.Message;
using MapRuleLibrary.YML.Shared;
using MapRuleLibrary.YML.VisibilitySpheres;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;

namespace MapRuleLibrary.YML.Quest;

public class CQuest
{
	public static readonly EQuestType[] QuestTypes = (EQuestType[])Enum.GetValues(typeof(EQuestType));

	public static readonly EQuestIconType[] QuestIconTypes = (EQuestIconType[])Enum.GetValues(typeof(EQuestIconType));

	public static readonly EQuestAreaType[] QuestAreaTypes = (EQuestAreaType[])Enum.GetValues(typeof(EQuestAreaType));

	public string ID { get; private set; }

	public int Chapter { get; set; }

	public EQuestType Type { get; set; }

	public EQuestIconType IconType { get; set; }

	public EQuestAreaType LocationArea { get; set; }

	public ECharacter CharacterIcon { get; set; }

	public List<string> CompletionRewards { get; set; }

	public CUnlockCondition UnlockCondition { get; set; }

	public CUnlockCondition BlockedCondition { get; set; }

	public List<QuestYML.CQuestCharacterRequirement> QuestCharacterRequirements { get; set; }

	public CMapScenario MapScenario { get; set; }

	public string StartingVillage { get; set; }

	public string EndingVillage { get; set; }

	public string LinkedQuestID { get; set; }

	public List<int> EventChance { get; set; }

	public List<Tuple<string, int>> EventPool { get; set; }

	public List<TreasureTable> CompletionTreasureTables => ScenarioRuleClient.SRLYML.TreasureTables.Where((TreasureTable w) => CompletionRewards.Contains(w.Name)).ToList();

	public List<MapDialogueLine> UnlockDialogueLines { get; set; }

	public List<MapDialogueLine> CompleteDialogueLines { get; set; }

	public List<VisibilitySphereYML.VisibilitySphereDefinition> UnlockSphereDefinitions { get; set; }

	public List<VisibilitySphereYML.VisibilitySphereDefinition> CompleteSphereDefinitions { get; set; }

	public string LoadoutImageId { get; set; }

	public string LoadoutAudioId { get; set; }

	public string FileName { get; private set; }

	public List<Tuple<string, string>> NarrativeTextImageOverride { get; set; }

	public List<Tuple<string, string>> NarrativeTextAudioOverride { get; set; }

	public string LocalisedNameKey => ID + "_NAME";

	public string LocalisedDescriptionKey => ID + "_DESC";

	public string LocalisedListDescriptionKey => ID + "_LIST";

	public string LocalisedIntroKey => ID + "_INTRO";

	public string LocalisedRewardKey => ID + "_REWARD";

	public string LocalisedIntroGloomhavenKey => ID + "_INTRO_GH_{0}";

	public string LocalisedIntroTravelKey => ID + "_INTRO_TRAVEL_{0}";

	public string LocalisedOutroTravelKey => ID + "_OUTRO_TRAVEL_{0}";

	public string LocalisedOutroGloomhavenKey => ID + "_OUTRO_GH_{0}";

	public string LocalisedCustomTreasureRewardKey { get; set; }

	public bool HideTreasureWhenCompleted { get; set; }

	public CQuest(string id, int chapter, EQuestType type, EQuestIconType iconType, ECharacter characterIcon, List<string> completionRewards, CUnlockCondition unlockCondition, CUnlockCondition blockedCondition, List<QuestYML.CQuestCharacterRequirement> questCharacterRequirements, CMapScenario mapScenario, string startingVillage, string endingVillage, string linkedQuestID, List<int> eventChance, List<Tuple<string, int>> eventPool, List<MapDialogueLine> unlockDialogueLines, List<MapDialogueLine> completeDialogueLines, List<VisibilitySphereYML.VisibilitySphereDefinition> unlockSphereDefinitions, List<VisibilitySphereYML.VisibilitySphereDefinition> completeSphereDefinitions, string loadoutImageId, string loadoutAudioId, List<Tuple<string, string>> narrativeTextImageOverride, List<Tuple<string, string>> narrativeTextAudioOverride, string fileName, EQuestAreaType locationArea, string localisedCustomTreasureRewardKey, bool hideTreasureWhenCompleted)
	{
		ID = id;
		Chapter = chapter;
		Type = type;
		IconType = iconType;
		CharacterIcon = characterIcon;
		CompletionRewards = completionRewards;
		UnlockCondition = unlockCondition;
		BlockedCondition = blockedCondition;
		QuestCharacterRequirements = questCharacterRequirements;
		MapScenario = mapScenario;
		StartingVillage = startingVillage;
		EndingVillage = endingVillage;
		LinkedQuestID = linkedQuestID;
		EventChance = eventChance;
		EventPool = eventPool;
		UnlockDialogueLines = unlockDialogueLines;
		CompleteDialogueLines = completeDialogueLines;
		UnlockSphereDefinitions = unlockSphereDefinitions;
		CompleteSphereDefinitions = completeSphereDefinitions;
		LoadoutImageId = loadoutImageId;
		NarrativeTextImageOverride = narrativeTextImageOverride;
		FileName = fileName;
		LoadoutAudioId = loadoutAudioId;
		NarrativeTextAudioOverride = narrativeTextAudioOverride;
		LocationArea = locationArea;
		LocalisedCustomTreasureRewardKey = localisedCustomTreasureRewardKey;
		HideTreasureWhenCompleted = hideTreasureWhenCompleted;
	}

	public bool Validate()
	{
		bool result = true;
		if (ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Quest ID specified for Quest in file " + FileName);
			result = false;
		}
		if (Chapter == int.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Quest Chapter specified for Quest in file " + FileName);
			result = false;
		}
		if (Type == EQuestType.None)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Quest Type specified for Quest in file " + FileName);
			result = false;
		}
		if (Type == EQuestType.City && MapRuleLibraryClient.MRLYML.MapMode != ScenarioManager.EDLLMode.Campaign)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "City is not a valid Quest Type for non-Campaign quests. Specified for Quest in file " + FileName);
			result = false;
		}
		if (Type == EQuestType.CityAdjacent && MapRuleLibraryClient.MRLYML.MapMode != ScenarioManager.EDLLMode.Campaign)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "CityAdjacent is not a valid Quest Type for non-Campaign quests. Specified for Quest in file " + FileName);
			result = false;
		}
		if (EventPool != null && EventPool.Count > 0)
		{
			if (EventChance == null)
			{
				SharedClient.ValidationRecord.RecordParseFailure(FileName, "Both Event Pool and Event Chance must be specified together for Quest in File: " + FileName);
				result = false;
			}
			else if (EventChance.Count != 8)
			{
				SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid Event Chance array size.  Must be exactly " + 8 + " length for quest in File: " + FileName);
				result = false;
			}
		}
		if (IconType == EQuestIconType.RequiredCharacter && QuestCharacterRequirements.Count <= 0 && CharacterIcon == ECharacter.None)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "For QuestIconType.RequiredCharacter a CharacterIcon or RequiredCharacters must be specified for quest in File: " + FileName);
			result = false;
		}
		if (UnlockCondition == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Quest UnlockCondition specified for Quest in file " + FileName);
			result = false;
		}
		if (MapScenario == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Quest MapScenario specified for Quest in file " + FileName);
			result = false;
		}
		if (StartingVillage == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Quest StartingVillage specified for Quest in file " + FileName);
			result = false;
		}
		if (EndingVillage == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Quest EndingVillage specified for Quest in file " + FileName);
			result = false;
		}
		return result;
	}

	public void UpdateData(int chapter, EQuestType type, EQuestIconType iconType, ECharacter characterIcon, List<string> completionRewards, CUnlockCondition unlockCondition, CUnlockCondition blockedCondition, List<QuestYML.CQuestCharacterRequirement> questCharacterRequirements, bool nullQuestCharacterRequirement, CMapScenario mapScenario, string startingVillage, string endingVillage, string linkedQuestID, List<int> eventChance, List<Tuple<string, int>> eventPool, List<string> removeEventPool, List<MapDialogueLine> unlockDialogueLines, List<MapDialogueLine> completeDialogueLines, List<VisibilitySphereYML.VisibilitySphereDefinition> unlockSphereDefinitions, List<VisibilitySphereYML.VisibilitySphereDefinition> completeSphereDefinitions, string loadoutImageId, string loadoutAudioId, List<Tuple<string, string>> narrativeTextImageOverride, List<Tuple<string, string>> narrativeTextAudioOverride, EQuestAreaType locationArea, string localisedCustomTreasureRewardKey, bool hideTreasureWhenCompleted)
	{
		if (chapter != int.MaxValue)
		{
			Chapter = chapter;
		}
		if (type != EQuestType.None)
		{
			Type = type;
		}
		if (iconType != EQuestIconType.None)
		{
			IconType = iconType;
		}
		if (characterIcon != ECharacter.None)
		{
			CharacterIcon = characterIcon;
		}
		if (completionRewards.Count > 0)
		{
			CompletionRewards = completionRewards;
		}
		if (unlockCondition != null)
		{
			UnlockCondition = unlockCondition;
		}
		if (blockedCondition != null)
		{
			BlockedCondition = blockedCondition;
		}
		if (questCharacterRequirements.Count > 0)
		{
			QuestCharacterRequirements = questCharacterRequirements;
		}
		if (nullQuestCharacterRequirement)
		{
			QuestCharacterRequirements = new List<QuestYML.CQuestCharacterRequirement>();
		}
		if (mapScenario != null)
		{
			MapScenario = mapScenario;
		}
		if (startingVillage != string.Empty)
		{
			StartingVillage = startingVillage;
		}
		if (endingVillage != string.Empty)
		{
			EndingVillage = endingVillage;
		}
		if (linkedQuestID != string.Empty)
		{
			LinkedQuestID = linkedQuestID;
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
				Tuple<string, int> tuple = EventPool.SingleOrDefault((Tuple<string, int> s) => s.Item1 == roadEvent.Item1);
				if (tuple != null)
				{
					EventPool.Remove(tuple);
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
				Tuple<string, int> tuple2 = EventPool.SingleOrDefault((Tuple<string, int> s) => s.Item1 == roadEvent2);
				if (tuple2 != null)
				{
					EventPool.Remove(tuple2);
				}
			}
		}
		if (unlockDialogueLines.Count > 0)
		{
			UnlockDialogueLines = unlockDialogueLines;
		}
		if (completeDialogueLines.Count > 0)
		{
			CompleteDialogueLines = completeDialogueLines;
		}
		if (unlockSphereDefinitions.Count > 0)
		{
			UnlockSphereDefinitions = unlockSphereDefinitions;
		}
		if (completeSphereDefinitions.Count > 0)
		{
			CompleteSphereDefinitions = completeSphereDefinitions;
		}
		if (loadoutImageId != null)
		{
			LoadoutImageId = loadoutImageId;
		}
		if (narrativeTextImageOverride != null)
		{
			NarrativeTextImageOverride = narrativeTextImageOverride;
		}
		if (loadoutAudioId != null)
		{
			LoadoutAudioId = loadoutAudioId;
		}
		if (narrativeTextAudioOverride != null)
		{
			NarrativeTextAudioOverride = narrativeTextAudioOverride;
		}
		if (locationArea != EQuestAreaType.None)
		{
			LocationArea = locationArea;
		}
		if (localisedCustomTreasureRewardKey != null)
		{
			LocalisedCustomTreasureRewardKey = localisedCustomTreasureRewardKey;
		}
		HideTreasureWhenCompleted = hideTreasureWhenCompleted;
	}
}
