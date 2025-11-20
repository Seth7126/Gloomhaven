namespace MapRuleLibrary.Client;

public enum EMapDLLMessageType
{
	None,
	Move,
	MoveComplete,
	EnteredScenario,
	RoadEvent,
	GetMapMessagesForTrigger,
	EnteredScenarioDebugMode,
	CheckLockedContent,
	CheckCurrentJobQuests,
	SkipFTUE,
	UpdateQuestCompletion,
	CheckForDuplicateItems,
	OnMapStateAdventureStarted,
	OnMapLoaded,
	CheckAchievementsForPlatformTrophies,
	DeleteCharacter,
	SoloScenarioImportCompletion,
	Count
}
