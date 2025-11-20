using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CMonsterAttackModifierDeck
{
	private List<AttackModifierYMLData> s_AttackModifierCardsPool = new List<AttackModifierYMLData>();

	private List<AttackModifierYMLData> s_LastDrawnAttackModifierCards = new List<AttackModifierYMLData>();

	private List<AttackModifierYMLData> s_AttackModifierCards = new List<AttackModifierYMLData>();

	private List<AttackModifierYMLData> s_DiscardedAttackModifierCards = new List<AttackModifierYMLData>();

	private bool s_ShuffleAttackModifierCards;

	public List<AttackModifierYMLData> AttackModifierCards => s_AttackModifierCards;

	public List<AttackModifierYMLData> AttackModifierCardsPool => s_AttackModifierCardsPool;

	public List<AttackModifierYMLData> DiscardedAttackModifierCards => s_DiscardedAttackModifierCards;

	public List<AttackModifierYMLData> LastDrawnAttackModifierCards => s_LastDrawnAttackModifierCards;

	public void LoadCardPool()
	{
		if (ScenarioManager.HouseRulesSettings.HasFlag(StateShared.EHouseRulesFlag.ReducedRandomness))
		{
			s_AttackModifierCardsPool = ScenarioRuleClient.SRLYML.AttackModifierDecks.Single((AttackModifierDeckYMLData s) => s.IsMonsterDeck && s.IsReducedRandomnessDeck).GetAllAttackModifiers.ToList();
		}
		else
		{
			s_AttackModifierCardsPool = ScenarioRuleClient.SRLYML.AttackModifierDecks.Single((AttackModifierDeckYMLData s) => s.IsMonsterDeck && !s.IsReducedRandomnessDeck).GetAllAttackModifiers.ToList();
		}
	}

	public List<AttackModifierYMLData> DrawAttackModifierCards(CActor actor, int attackStrength, EAdvantageStatuses advStatus, out List<AttackModifierYMLData> notUsed)
	{
		bool shuffle;
		List<AttackModifierYMLData> list = CActor.DrawAttackModifierCards(actor, attackStrength, advStatus, s_AttackModifierCards, s_DiscardedAttackModifierCards, out shuffle, out notUsed);
		s_LastDrawnAttackModifierCards = list.ToList();
		s_LastDrawnAttackModifierCards.AddRange(notUsed);
		s_ShuffleAttackModifierCards |= shuffle;
		return list;
	}

	public void CheckAttackModifierCardShuffle(bool force = false)
	{
		if (s_ShuffleAttackModifierCards || force)
		{
			if (GameState.ShuffleAttackModsEnabledForMonsters)
			{
				CActor.ShuffleAttackModifierCards(s_AttackModifierCards, s_DiscardedAttackModifierCards);
			}
			s_ShuffleAttackModifierCards = false;
		}
	}

	public void ResetAttackModifiers(AttackModifierDeckState deckState)
	{
		LoadCardPool();
		AttackModifierCards.Clear();
		s_DiscardedAttackModifierCards.Clear();
		s_ShuffleAttackModifierCards = false;
		if (ScenarioManager.CurrentScenarioState.EnemyClassManager == null || deckState?.AttackModifierCardsAvailable == null)
		{
			foreach (AttackModifierYMLData item in s_AttackModifierCardsPool)
			{
				s_AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, s_AttackModifierCards.Count), item);
			}
		}
		s_LastDrawnAttackModifierCards = new List<AttackModifierYMLData>();
	}

	public void LoadAttackModifierDeck(AttackModifierDeckState deckState)
	{
		if (deckState.AttackModifierCardsAvailable != null)
		{
			s_AttackModifierCards.Clear();
			CClass.AddModifierCards(deckState.AttackModifierCardsAvailable, ref s_AttackModifierCards);
		}
		if (deckState.AttackModifierCardsDiscarded != null)
		{
			s_DiscardedAttackModifierCards.Clear();
			CClass.AddModifierCards(deckState.AttackModifierCardsDiscarded, ref s_DiscardedAttackModifierCards);
		}
	}

	public void RandomizeAvailableAttackModifierCardsOrder()
	{
		List<AttackModifierYMLData> list = new List<AttackModifierYMLData>();
		list.AddRange(s_AttackModifierCards);
		s_AttackModifierCards.Clear();
		foreach (AttackModifierYMLData item in list)
		{
			s_AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, s_AttackModifierCards.Count), item);
		}
	}

	public void AddAdditionalModifierCards(List<string> cardNames)
	{
		CClass.AddModifierCards(cardNames, ref s_AttackModifierCards);
	}

	public string DebugSetAttackMod()
	{
		AttackModifierYMLData item = AttackModifierCards[0];
		AttackModifierCards.Remove(item);
		AttackModifierCards.Add(item);
		return "Next: " + AttackModifierCards[0].Name;
	}
}
