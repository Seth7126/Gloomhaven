using System.Collections.Generic;
using MapRuleLibrary.Party;

public interface IBattleGoalService
{
	bool HasShownBattleGoalIntro { get; }

	List<CBattleGoalState> GetAvailableBattleGoals(string characterID);

	CBattleGoalState GetChosenBattleGoal(string characterID);

	void ChooseBattleGoal(ICharacter character, CBattleGoalState battleGoal);

	void RemoveBattleGoal(ICharacter character, CBattleGoalState battleGoal);

	void SetBattleGoalIntroShown();
}
