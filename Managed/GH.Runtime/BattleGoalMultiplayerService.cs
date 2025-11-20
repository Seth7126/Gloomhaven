#define ENABLE_LOGS
using System.Linq;
using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;

public class BattleGoalMultiplayerService
{
	public static void ProxySelectBattleGoal(GameAction gameAction)
	{
		Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: true);
		BattleGoalSelectionToken token = gameAction.SupplementaryDataToken as BattleGoalSelectionToken;
		CQuestState cQuestState = AdventureState.MapState.AllQuests.First((CQuestState it) => it.ID == token.QuestID);
		Debug.LogFormat("ProxySelectBattleGoal: {0}  character: {1}  selected: {2}  quest: {3}", token.BattleGoalID, token.CharacterID, token.Selected, token.QuestID);
		if (token.Selected)
		{
			if (gameAction.PlayerID == PlayerRegistry.MyPlayer.PlayerID)
			{
				NewPartyDisplayUI.PartyDisplay.BattleGoalWindow.SelectionAllowed = true;
			}
			CBattleGoalState cBattleGoalState = cQuestState.GetAssignedBattleGoals(token.CharacterID).FirstOrDefault((CBattleGoalState it) => it.ID == token.BattleGoalID);
			if (cBattleGoalState != null)
			{
				cQuestState.ChooseBattleGoal(token.CharacterID, cBattleGoalState);
			}
		}
		else
		{
			cQuestState.RemoveBattleGoal(token.CharacterID, token.BattleGoalID);
		}
	}
}
