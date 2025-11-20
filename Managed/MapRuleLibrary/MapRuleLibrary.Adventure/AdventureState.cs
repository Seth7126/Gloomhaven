using MapRuleLibrary.PhaseManager;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;

namespace MapRuleLibrary.Adventure;

public static class AdventureState
{
	public static CMapState MapState { get; private set; }

	public static void StartAdventure(CMapState mapState, bool skipTutorial = false, bool skipIntro = false)
	{
		MapState = mapState;
		MapState.Initialise(skipTutorial, skipIntro);
		ScenarioManager.SetHouseRules(MapState.HouseRulesSetting);
		ScenarioRuleClient.Reset();
		if (MapState.DLLMode == ScenarioManager.EDLLMode.Guildmaster && !MapState.TutorialCompleted && MapState.HeadquartersState.Headquarters.StartingScenarios != null)
		{
			MapState.StartTutorial();
		}
		else if (MapState.CurrentMapPhase == null || MapState.CurrentMapPhase.Type == EMapPhaseType.None)
		{
			MapState.InitMapPhase();
		}
		if (MapState.ForceRegenerate)
		{
			MapState.UpdateScenarioStates();
		}
	}

	public static void End()
	{
		MapState = null;
	}

	public static void UpdateMapState(CMapState mapState)
	{
		MapState = mapState;
	}
}
