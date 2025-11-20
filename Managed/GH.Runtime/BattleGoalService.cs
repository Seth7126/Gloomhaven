using System;
using System.Collections.Generic;
using FFSNet;
using GLOO.Introduction;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using Photon.Bolt;

public class BattleGoalService : IBattleGoalService
{
	private CQuestState quest;

	public bool HasShownBattleGoalIntro => AdventureState.MapState.MapParty.HasIntroduced(EIntroductionConcept.BattleGoal.ToString());

	public BattleGoalService(CQuestState quest)
	{
		this.quest = quest;
	}

	public List<CBattleGoalState> GetAvailableBattleGoals(string characterID)
	{
		return quest.GetAssignedBattleGoals(characterID);
	}

	public CBattleGoalState GetChosenBattleGoal(string characterID)
	{
		return quest.GetChosenBattleGoal(characterID);
	}

	public void ChooseBattleGoal(ICharacter character, CBattleGoalState battleGoal)
	{
		if (FFSNetwork.IsOnline && character.IsUnderMyControl)
		{
			Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: false);
			IProtocolToken supplementaryDataToken = new BattleGoalSelectionToken(battleGoal.ID, character.CharacterID, selected: true, quest.ID);
			Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.SelectBattleGoal, ActionPhaseType.MapLoadoutScreen, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
		}
		else
		{
			quest.ChooseBattleGoal(character.CharacterID, battleGoal);
		}
	}

	public void RemoveBattleGoal(ICharacter character, CBattleGoalState battleGoal)
	{
		if (FFSNetwork.IsOnline && character.IsUnderMyControl)
		{
			Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: false);
			IProtocolToken supplementaryDataToken = new BattleGoalSelectionToken(battleGoal.ID, character.CharacterID, selected: false, quest.ID);
			Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.SelectBattleGoal, ActionPhaseType.MapLoadoutScreen, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
		}
		else
		{
			quest.RemoveBattleGoal(character.CharacterID, battleGoal);
		}
	}

	public void SetBattleGoalIntroShown()
	{
		AdventureState.MapState.MapParty.MarkIntroDone(EIntroductionConcept.BattleGoal.ToString());
	}
}
