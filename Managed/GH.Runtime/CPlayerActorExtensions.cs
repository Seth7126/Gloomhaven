using ScenarioRuleLibrary;

public static class CPlayerActorExtensions
{
	public static bool IsCardSelectionReady(this CPlayerActor playerActor)
	{
		CardsHandUI hand = CardsHandManager.Instance.GetHand(playerActor);
		bool flag = hand != null && hand.IsShortRestSelected();
		if (playerActor.IsPlayerByDefault() && ScenarioManager.Scenario.HasActor(playerActor) && (!FFSNetwork.IsOnline || playerActor.IsUnderMyControl) && ((playerActor.CharacterClass.RoundAbilityCards.Count < 2 && !playerActor.CharacterClass.LongRest) || (playerActor.CharacterClass.ImprovedShortRest && playerActor.CharacterClass.LongRest && flag) || Choreographer.s_Choreographer.HaltMultiplayerProgression || (flag && FFSNetwork.IsOnline) || (FFSNetwork.IsOnline && Singleton<UIConfirmationBoxManager>.Instance.IsOpen)))
		{
			return false;
		}
		return true;
	}

	public static bool IsUnderControlOrSingle(this CPlayerActor playerActor)
	{
		if (FFSNetwork.IsOnline)
		{
			return playerActor.IsUnderMyControl;
		}
		return true;
	}

	public static bool HasAbilityCards(this CPlayerActor playerActor)
	{
		if (playerActor.CharacterClass.HandAbilityCards.Count <= 0)
		{
			return playerActor.CharacterClass.ActivatedAbilityCards.Count > 0;
		}
		return true;
	}

	public static bool IsDeadOrHasNotAbilityCards(this CPlayerActor playerActor)
	{
		if (!playerActor.IsDead)
		{
			return !playerActor.HasAbilityCards();
		}
		return true;
	}
}
