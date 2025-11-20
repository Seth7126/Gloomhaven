using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;

namespace ScenarioRuleLibrary;

public class CCharacterClass : CClass
{
	public static ECharacter[] CharacterNames = (ECharacter[])Enum.GetValues(typeof(ECharacter));

	private List<CAbilityCard> m_AbilityCardsPool = new List<CAbilityCard>();

	private List<CAbilityCard> m_RoundAbilityCards = new List<CAbilityCard>();

	private List<CAbilityCard> m_HandAbilityCards = new List<CAbilityCard>();

	private List<CAbilityCard> m_DiscardedAbilityCards = new List<CAbilityCard>();

	private List<CBaseCard> m_ActivatedCards = new List<CBaseCard>();

	private List<CAbilityCard> m_LostAbilityCards = new List<CAbilityCard>();

	private List<CAbilityCard> m_PermanentlyLostAbilityCards = new List<CAbilityCard>();

	private List<CAbilityCard> m_ExtraTurnCards = new List<CAbilityCard>();

	private List<CAbilityCard> m_SupplyCards = new List<CAbilityCard>();

	private List<AttackModifierYMLData> m_AttackModifierCardsPool = new List<AttackModifierYMLData>();

	private List<AttackModifierYMLData> m_LastDrawnAttackModifierCards = new List<AttackModifierYMLData>();

	private List<AttackModifierYMLData> m_AttackModifierCards = new List<AttackModifierYMLData>();

	private List<AttackModifierYMLData> m_DiscardedAttackModifierCards = new List<AttackModifierYMLData>();

	private bool m_ShuffleAttackModifierCards;

	private bool m_LongRest;

	private bool m_HasLongRested;

	private bool m_HasShortRested;

	private bool m_HasImprovedShortRested;

	private bool m_ImprovedShortRest;

	private CAbilityCard m_InitiativeAbilityCard;

	private CAbilityCard m_SubInitiativeAbilityCard;

	private CAbilityCard m_ExtraTurnInitiativeAbilityCard;

	private int m_InitiativeBonus;

	private int m_NumberAbilityCardsInBattle;

	private List<CAbilityCard> m_SelectedAbilityCards;

	private int m_Health;

	private int m_Level;

	private CharacterYMLData m_CharacterYML;

	private List<PerksYMLData> m_Perks = new List<PerksYMLData>();

	private HeroSummonYMLData m_CompanionSummonData;

	public List<CAbilityCard> AbilityCardsPool => m_AbilityCardsPool;

	public List<CAbilityCard> SelectedAbilityCards => m_SelectedAbilityCards;

	public List<CAbilityCard> UnselectedAbilityCards => (from x in m_AbilityCardsPool.Except(SelectedAbilityCards)
		where !x.SupplyCard
		select x).ToList();

	public List<CAbilityCard> SupplyCards => m_SupplyCards;

	public List<CAbilityCard> GivenCards => m_HandAbilityCards.Except(SelectedAbilityCards).ToList();

	public List<CAbilityCard> RoundCardsSelectedInCardSelection { get; set; }

	public List<CAbilityCard> RoundAbilityCards => m_RoundAbilityCards;

	public List<CAbilityCard> HandAbilityCards => m_HandAbilityCards;

	public List<CAbilityCard> DiscardedAbilityCards => m_DiscardedAbilityCards;

	public List<CAbilityCard> PermanentlyLostAbilityCards => m_PermanentlyLostAbilityCards;

	public List<CBaseCard> ActivatedCards => m_ActivatedCards;

	public List<CAbilityCard> ActivatedAbilityCards => (from s in m_ActivatedCards
		where s is CAbilityCard
		select s as CAbilityCard).ToList();

	public List<CAbilityCard> LostAbilityCards => m_LostAbilityCards;

	public List<CAbilityCard> ExtraTurnCards => m_ExtraTurnCards;

	public List<CAbilityCard> NextPendingExtraTurnCards
	{
		get
		{
			if (PendingExtraTurnCardsData.Count <= 0)
			{
				return new List<CAbilityCard>();
			}
			return PendingExtraTurnCardsData[0].PendingExtraTurnCards;
		}
	}

	public List<CAbilityCard> AllPendingExtraTurnCards
	{
		get
		{
			if (PendingExtraTurnCardsData.Count <= 0)
			{
				return new List<CAbilityCard>();
			}
			return PendingExtraTurnCardsData.SelectMany((CAbilityExtraTurn.PendingExtraTurnData x) => x.PendingExtraTurnCards).ToList();
		}
	}

	public List<CAbilityExtraTurn.PendingExtraTurnData> PendingExtraTurnCardsData { get; set; }

	public List<CAbilityCard> ExtraTurnCardsSelectedInCardSelection
	{
		get
		{
			if (ExtraTurnCardsSelectedInCardSelectionStack.Count <= 0)
			{
				return new List<CAbilityCard>();
			}
			return ExtraTurnCardsSelectedInCardSelectionStack.Peek();
		}
	}

	public Stack<List<CAbilityCard>> ExtraTurnCardsSelectedInCardSelectionStack { get; set; }

	public List<AttackModifierYMLData> AttackModifierCards => m_AttackModifierCards;

	public List<AttackModifierYMLData> AttackModifierCardsPool => m_AttackModifierCardsPool;

	public List<AttackModifierYMLData> DiscardedAttackModifierCards => m_DiscardedAttackModifierCards;

	public List<AttackModifierYMLData> LastDrawnAttackModifierCards => m_LastDrawnAttackModifierCards;

	public bool LongRest
	{
		get
		{
			return m_LongRest;
		}
		set
		{
			m_LongRest = value;
		}
	}

	public bool HasLongRested
	{
		get
		{
			return m_HasLongRested;
		}
		set
		{
			m_HasLongRested = value;
		}
	}

	public bool HasShortRested
	{
		get
		{
			return m_HasShortRested;
		}
		set
		{
			m_HasShortRested = value;
		}
	}

	public bool HasImprovedShortRested
	{
		get
		{
			return m_HasImprovedShortRested;
		}
		set
		{
			m_HasImprovedShortRested = value;
		}
	}

	public bool ShortRestCardRedrawn { get; set; }

	public CAbilityCard ShortRestCardBurned { get; set; }

	public bool ImprovedShortRest
	{
		get
		{
			return m_ImprovedShortRest;
		}
		set
		{
			m_ImprovedShortRest = value;
		}
	}

	public CAbilityCard InitiativeAbilityCard => m_InitiativeAbilityCard;

	public CAbilityCard SubInitiativeAbilityCard => m_SubInitiativeAbilityCard;

	public CAbilityCard ExtraTurnInitiativeAbilityCard => m_ExtraTurnInitiativeAbilityCard;

	public int InitiativeBonus => m_InitiativeBonus;

	public int NumberAbilityCardsInBattle => m_NumberAbilityCardsInBattle;

	public bool InitialState { get; set; }

	public string CharacterID => m_CharacterYML.ID;

	public ECharacter CharacterModel => m_CharacterYML.Model;

	public HeroSummonYMLData CompanionSummonData => m_CompanionSummonData;

	public int ModelInstanceID { get; private set; }

	public int[] HealthTable => m_CharacterYML.HealthTable;

	public List<PerksYMLData> Perks => m_Perks;

	public CharacterYMLData CharacterYML => m_CharacterYML;

	public override int Health()
	{
		return m_Health;
	}

	public CCharacterClass(CharacterYMLData character, List<CAbilityCard> abilityCards, int level)
		: base(character.ID, character.Model.ToString(), character.LocKey)
	{
		CCharacterClass cCharacterClass = this;
		m_CharacterYML = character;
		m_Level = level;
		m_Health = character.HealthTable[level];
		m_NumberAbilityCardsInBattle = character.NumberAbilityCardsInBattle;
		m_AbilityCardsPool = abilityCards.Where((CAbilityCard x) => !x.SupplyCard).ToList();
		m_SupplyCards = abilityCards.Where((CAbilityCard x) => x.SupplyCard).ToList();
		m_AttackModifierCardsPool = character.AttackModifierDeck.GetAllAttackModifiers;
		if (character.CompanionSummonID != null)
		{
			m_CompanionSummonData = ScenarioRuleClient.SRLYML.HeroSummons.Single((HeroSummonYMLData s) => s.ID == character.CompanionSummonID);
		}
		RoundCardsSelectedInCardSelection = new List<CAbilityCard>();
		ExtraTurnCardsSelectedInCardSelectionStack = new Stack<List<CAbilityCard>>();
		PendingExtraTurnCardsData = new List<CAbilityExtraTurn.PendingExtraTurnData>();
		Reset();
		ModelInstanceID = (int)(CharacterModel + CharacterClassManager.Classes.Where((CCharacterClass w) => w.CharacterModel == cCharacterClass.CharacterModel).Count() * 1000);
	}

	public void MoveAbilityCard(CAbilityCard abilityCard, List<CAbilityCard> fromAbilityCardList, List<CAbilityCard> toAbilityCardList, string fromCardPileName, string toCardPileName, bool sendNetworkSelectedCardsWhenDone = false)
	{
		string[] array = Environment.StackTrace.Split(new string[1] { Environment.NewLine }, 3, StringSplitOptions.None);
		string text = ((array.Length > 1) ? ("\n" + array[1] + ((array.Length > 2) ? ("\n" + array[2]) : "")) : "");
		DLLDebug.Log("Moving ability card: " + abilityCard.Name + " from pile: " + fromCardPileName + " to: " + toCardPileName + text);
		if (fromAbilityCardList != null)
		{
			if (fromAbilityCardList.Contains(abilityCard))
			{
				fromAbilityCardList.Remove(abilityCard);
			}
			else
			{
				DLLDebug.LogError("Error: Tried to move Ability Card " + abilityCard.Name + " from a list that did not contain it");
			}
		}
		if (toAbilityCardList != null)
		{
			if (toAbilityCardList.Contains(abilityCard))
			{
				DLLDebug.LogError("Error: Tried to move Ability Card " + abilityCard.Name + " into a list that already contained it");
			}
			else
			{
				toAbilityCardList.Add(abilityCard);
			}
		}
		CheckCardPilesForDuplicates(abilityCard);
		int count = HandAbilityCards.Count;
		int discardSize = ActivatedAbilityCards.Count((CAbilityCard x) => x != null && x.SelectedAction?.CardPile == CBaseCard.ECardPile.Discarded) + DiscardedAbilityCards.Count;
		SEventLogMessageHandler.AddEventLogMessage(new SEventHand(CharacterID, count, discardSize));
		CPlayerFinishedMovingAbilityCard message = new CPlayerFinishedMovingAbilityCard(GetActor())
		{
			m_SendNetworkSelectedRoundCards = sendNetworkSelectedCardsWhenDone
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	private void CheckCardPilesForDuplicates(CAbilityCard abilityCard)
	{
		int num = 0;
		string text = "";
		if (RoundAbilityCards.Contains(abilityCard))
		{
			num++;
			text += "ROUND";
		}
		if (HandAbilityCards.Contains(abilityCard))
		{
			num++;
			text += "HAND, ";
		}
		if (ActivatedAbilityCards.Contains(abilityCard))
		{
			num++;
			text += "ACTIVATED, ";
		}
		if (DiscardedAbilityCards.Contains(abilityCard))
		{
			num++;
			text += "DISCARD, ";
		}
		if (LostAbilityCards.Contains(abilityCard))
		{
			num++;
			text += "LOST, ";
		}
		if (PermanentlyLostAbilityCards.Contains(abilityCard))
		{
			num++;
			text += "PERMALOST, ";
		}
		if (num > 1)
		{
			DLLDebug.LogError("Duplicate card: " + abilityCard.Name + " found in piles: " + text + " after moving cards\n" + Environment.StackTrace);
		}
	}

	public void ActivateCard(CBaseCard cardToActivate)
	{
		if (CardActivated(cardToActivate))
		{
			return;
		}
		if (cardToActivate == null)
		{
			DLLDebug.LogError("Attempting to add a null card to the Activated pile.\n" + Environment.StackTrace);
			return;
		}
		m_ActivatedCards.Add(cardToActivate);
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
		}
	}

	public bool CardActivated(CBaseCard cardToActivate)
	{
		return m_ActivatedCards.Contains(cardToActivate);
	}

	public void SetInitiativeAbilityCard(CAbilityCard abilityCard)
	{
		m_InitiativeAbilityCard = abilityCard;
	}

	public void SetSubInitiativeAbilityCard(CAbilityCard abilityCard)
	{
		m_SubInitiativeAbilityCard = abilityCard;
	}

	public void SetExtraTurnInitiativeAbilityCard(CAbilityCard abilityCard)
	{
		m_ExtraTurnInitiativeAbilityCard = abilityCard;
	}

	public void ResetInitiativeBonus()
	{
		m_InitiativeBonus = 0;
	}

	public void UpdateInitiativeBonus(int initiativeBonus)
	{
		m_InitiativeBonus = initiativeBonus;
		if (GetActor() is CPlayerActor cPlayerActor)
		{
			SimpleLog.AddToSimpleLog(cPlayerActor.Class.ID + " Initiative adjusted " + initiativeBonus + ".  New Initiative " + cPlayerActor.Initiative());
		}
	}

	public static CBaseCard.ECardPile GetCardPile(string name)
	{
		return ((CBaseCard.ECardPile[])Enum.GetValues(typeof(CBaseCard.ECardPile))).SingleOrDefault((CBaseCard.ECardPile x) => x.ToString() == name);
	}

	public List<AttackModifierYMLData> DrawAttackModifierCards(CActor actor, int attackStrength, EAdvantageStatuses advStatus, out List<AttackModifierYMLData> notUsed)
	{
		bool shuffle;
		List<AttackModifierYMLData> list = CActor.DrawAttackModifierCards(actor, attackStrength, advStatus, m_AttackModifierCards, m_DiscardedAttackModifierCards, out shuffle, out notUsed);
		m_LastDrawnAttackModifierCards = list.ToList();
		m_LastDrawnAttackModifierCards.AddRange(notUsed);
		m_ShuffleAttackModifierCards |= shuffle;
		return list;
	}

	private void MoveAbilityCardToPile(CAbilityCard abilityCard, List<CAbilityCard> pileToMoveFrom, string pileToMoveFromName)
	{
		if (abilityCard.SupplyCard)
		{
			pileToMoveFrom.Remove(abilityCard);
			CharacterClassManager.AllAbilityCardInstances.Remove(abilityCard);
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.SingleOrDefault((CPlayerActor x) => x.CharacterClass == this);
			if (cPlayerActor != null)
			{
				CSupplyCardUsed_MessageData cSupplyCardUsed_MessageData = new CSupplyCardUsed_MessageData(cPlayerActor);
				cSupplyCardUsed_MessageData.m_ActorUsedCard = cPlayerActor;
				cSupplyCardUsed_MessageData.m_SupplyCardUsed = abilityCard;
				ScenarioRuleClient.MessageHandler(cSupplyCardUsed_MessageData);
			}
			return;
		}
		CBaseCard.ECardPile eCardPile = CBaseCard.ECardPile.Discarded;
		CBaseCard.ECardPile eCardPile2 = CBaseCard.ECardPile.None;
		if (abilityCard.SelectedAction != null)
		{
			eCardPile = ((!abilityCard.ActionHasHappened) ? CBaseCard.ECardPile.Discarded : abilityCard.SelectedAction.CardPile);
		}
		if (abilityCard.ActiveBonuses.Count > 0)
		{
			eCardPile = CBaseCard.ECardPile.Activated;
		}
		if (abilityCard.CurrentCardPile == CBaseCard.ECardPile.Activated && eCardPile != CBaseCard.ECardPile.Activated)
		{
			CRefreshActiveBonusUI_MessageData message = new CRefreshActiveBonusUI_MessageData(GameState.InternalCurrentActor)
			{
				m_Actor = GameState.InternalCurrentActor
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		abilityCard.CurrentCardPile = eCardPile;
		switch (eCardPile)
		{
		case CBaseCard.ECardPile.Activated:
		{
			if (pileToMoveFrom.Contains(abilityCard))
			{
				pileToMoveFrom?.Remove(abilityCard);
				if (!m_ActivatedCards.Contains(abilityCard))
				{
					if (abilityCard == null)
					{
						DLLDebug.LogError("Attempting to add a null card to the Activated pile.\n" + Environment.StackTrace);
						return;
					}
					m_ActivatedCards.Add(abilityCard);
					foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
					{
						allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
					}
				}
			}
			CRefreshActiveBonusUI_MessageData message2 = new CRefreshActiveBonusUI_MessageData(GameState.InternalCurrentActor)
			{
				m_Actor = GameState.InternalCurrentActor
			};
			ScenarioRuleClient.MessageHandler(message2);
			CAction selectedAction = abilityCard.SelectedAction;
			eCardPile2 = ((selectedAction != null && selectedAction.CardPile == CBaseCard.ECardPile.Discarded) ? CBaseCard.ECardPile.Discarded : CBaseCard.ECardPile.Lost);
			break;
		}
		case CBaseCard.ECardPile.Lost:
			abilityCard.ActionHasHappened = false;
			abilityCard.SetSelectedAction(null);
			MoveAbilityCard(abilityCard, pileToMoveFrom, LostAbilityCards, pileToMoveFromName, "LostAbilityCards");
			break;
		case CBaseCard.ECardPile.Discarded:
			abilityCard.ActionHasHappened = false;
			abilityCard.SetSelectedAction(null);
			MoveAbilityCard(abilityCard, pileToMoveFrom, DiscardedAbilityCards, pileToMoveFromName, "DiscardedAbilityCards");
			break;
		case CBaseCard.ECardPile.PermanentlyLost:
			abilityCard.ActionHasHappened = false;
			abilityCard.SetSelectedAction(null);
			MoveAbilityCard(abilityCard, pileToMoveFrom, PermanentlyLostAbilityCards, pileToMoveFromName, "PermanentlyLostAbilityCards");
			break;
		}
		if (pileToMoveFrom != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventDiscardCard(base.ID, abilityCard.ID, (eCardPile2 == CBaseCard.ECardPile.None) ? eCardPile : eCardPile2));
		}
	}

	public void DiscardRoundAbilityCards(bool extraTurn = false)
	{
		if (extraTurn)
		{
			for (int num = ExtraTurnCards.Count - 1; num >= 0; num--)
			{
				MoveAbilityCardToPile(ExtraTurnCards[num], ExtraTurnCards, "ExtraTurnCards");
			}
		}
		else
		{
			for (int num2 = RoundAbilityCards.Count - 1; num2 >= 0; num2--)
			{
				MoveAbilityCardToPile(RoundAbilityCards[num2], RoundAbilityCards, "RoundAbilityCards");
			}
		}
	}

	public void ClearRoundAbilityCards()
	{
		foreach (CAbilityCard item in m_RoundAbilityCards.ToList())
		{
			MoveAbilityCard(item, m_RoundAbilityCards, m_HandAbilityCards, "RoundAbilityCards", "HandAbilityCards");
		}
	}

	public void DiscardRoundAbilityCard(CAbilityCard roundAbilityCard)
	{
		MoveAbilityCardToPile(roundAbilityCard, RoundAbilityCards, "RoundAbilityCards");
	}

	public void DiscardExtraTurnAbilityCard(CAbilityCard extraTurnAbilityCard)
	{
		MoveAbilityCardToPile(extraTurnAbilityCard, ExtraTurnCards, "ExtraTurnCards");
	}

	public void RestoreCachedAugmentOrSongAbilityCard(CAbilityCard roundAbilityCard)
	{
		roundAbilityCard.ActionHasHappened = true;
		if (DiscardedAbilityCards.Contains(roundAbilityCard))
		{
			DiscardedAbilityCards.Remove(roundAbilityCard);
			roundAbilityCard.SetSelectedAction(roundAbilityCard.TopAction);
			roundAbilityCard.CurrentCardPile = CBaseCard.ECardPile.Activated;
			if (!m_ActivatedCards.Contains(roundAbilityCard))
			{
				if (roundAbilityCard == null)
				{
					DLLDebug.LogError("Attempting to add a null card to the Activated pile.\n" + Environment.StackTrace);
					return;
				}
				m_ActivatedCards.Add(roundAbilityCard);
				foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
				{
					allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
				}
			}
			CheckCardPilesForDuplicates(roundAbilityCard);
		}
		CRefreshActiveBonusUI_MessageData message = new CRefreshActiveBonusUI_MessageData(GameState.InternalCurrentActor)
		{
			m_Actor = GameState.InternalCurrentActor
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public void CheckAttackModifierCardShuffle(bool force = false)
	{
		if (m_ShuffleAttackModifierCards || force)
		{
			if (GameState.ShuffleAttackModsEnabledForPlayers)
			{
				CActor.ShuffleAttackModifierCards(m_AttackModifierCards, m_DiscardedAttackModifierCards);
			}
			m_ShuffleAttackModifierCards = false;
		}
	}

	public override CBaseCard FindCardWithAbility(CAbility ability, CActor actor)
	{
		foreach (CAbilityCard roundAbilityCard in m_RoundAbilityCards)
		{
			if (roundAbilityCard.HasAbility(ability))
			{
				return roundAbilityCard;
			}
		}
		foreach (CAbilityCard item in m_AbilityCardsPool)
		{
			if (item.HasAbility(ability))
			{
				return item;
			}
		}
		foreach (ItemCardYMLData itemCard in ScenarioRuleClient.SRLYML.ItemCards)
		{
			if (itemCard.Data.Abilities != null && itemCard.Data.Abilities.Any((CAbility a) => a.HasID(ability.ID)))
			{
				return itemCard.GetItem;
			}
		}
		foreach (AttackModifierYMLData attackModifier in ScenarioRuleClient.SRLYML.AttackModifiers)
		{
			if (attackModifier.Abilities.Any((CAbility a) => a.HasID(ability.ID)))
			{
				return attackModifier.Card;
			}
		}
		foreach (CAbilityCard temporaryCard in base.TemporaryCards)
		{
			if (temporaryCard.HasAbility(ability))
			{
				return temporaryCard;
			}
		}
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			foreach (CAbility item2 in scenarioModifier.AllListedTriggerAbilities())
			{
				if (item2 != null && item2.HasID(ability.ID))
				{
					return scenarioModifier;
				}
			}
		}
		return null;
	}

	public CAbilityCard FindCardWithID(int id)
	{
		foreach (CAbilityCard roundAbilityCard in m_RoundAbilityCards)
		{
			if (roundAbilityCard.ID == id)
			{
				return roundAbilityCard;
			}
		}
		foreach (CAbilityCard item in m_AbilityCardsPool)
		{
			if (item.ID == id)
			{
				return item;
			}
		}
		return null;
	}

	public override CBaseCard FindCard(int id, string name)
	{
		foreach (CAbilityCard roundAbilityCard in m_RoundAbilityCards)
		{
			if (roundAbilityCard.ID == id && roundAbilityCard.Name == name)
			{
				return roundAbilityCard;
			}
		}
		foreach (CAbilityCard item in m_AbilityCardsPool)
		{
			if (item.ID == id && item.Name == name)
			{
				return item;
			}
		}
		foreach (CBaseCard activatedCard in m_ActivatedCards)
		{
			if (activatedCard is CItem cItem && cItem.ID == id && cItem.Name == name)
			{
				return cItem;
			}
		}
		foreach (AttackModifierYMLData attackModifier in ScenarioRuleClient.SRLYML.AttackModifiers)
		{
			if (attackModifier.Name == name)
			{
				return attackModifier.Card;
			}
		}
		return null;
	}

	public List<CActiveBonus> FindActiveBonusAuras(CActor actor)
	{
		try
		{
			List<CActiveBonus> list = new List<CActiveBonus>();
			foreach (CBaseCard activatedCard in m_ActivatedCards)
			{
				foreach (CActiveBonus activeBonuse in activatedCard.ActiveBonuses)
				{
					if (!activeBonuse.IsAura)
					{
						continue;
					}
					if (activeBonuse.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
					{
						if ((actor as CHeroSummonActor)?.Summoner == activeBonuse.Actor && !list.Contains(activeBonuse))
						{
							list.Add(activeBonuse);
						}
					}
					else if (actor == activeBonuse.Actor && !list.Contains(activeBonuse))
					{
						list.Add(activeBonuse);
					}
				}
			}
			return list;
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("Collection was modified"))
			{
				return FindActiveBonusAuras(actor);
			}
			throw ex;
		}
	}

	public List<CActiveBonus> FindCasterActiveBonuses(CActor caster)
	{
		try
		{
			List<CActiveBonus> list = new List<CActiveBonus>();
			foreach (CBaseCard activatedCard in m_ActivatedCards)
			{
				foreach (CActiveBonus activeBonuse in activatedCard.ActiveBonuses)
				{
					if (!list.Contains(activeBonuse) && caster == activeBonuse.Caster)
					{
						list.Add(activeBonuse);
					}
				}
			}
			return list;
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("Collection was modified"))
			{
				return FindCasterActiveBonuses(caster);
			}
			throw ex;
		}
	}

	public List<CActiveBonus> FindActiveBonuses(CActor actor)
	{
		try
		{
			List<CActiveBonus> list = new List<CActiveBonus>();
			foreach (CBaseCard activatedCard in m_ActivatedCards)
			{
				foreach (CActiveBonus activeBonuse in activatedCard.ActiveBonuses)
				{
					if (activeBonuse.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
					{
						if ((activeBonuse.Actor as CHeroSummonActor)?.Summoner == actor && !list.Contains(activeBonuse))
						{
							list.Add(activeBonuse);
						}
					}
					else
					{
						if (list.Contains(activeBonuse))
						{
							continue;
						}
						if (activeBonuse.IsAura)
						{
							if (activeBonuse.ValidActorsInRangeOfAura.Contains(actor) || activeBonuse.Actor == actor)
							{
								list.Add(activeBonuse);
							}
						}
						else if (actor == activeBonuse.Actor)
						{
							list.Add(activeBonuse);
						}
					}
				}
			}
			return list;
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("Collection was modified"))
			{
				return FindActiveBonuses(actor);
			}
			throw ex;
		}
	}

	public List<CActiveBonus> FindActiveBonuses(CAbility.EAbilityType type, CActor actor)
	{
		try
		{
			List<CActiveBonus> list = new List<CActiveBonus>();
			if (m_ActivatedCards.Any((CBaseCard a) => a == null))
			{
				DLLDebug.LogWarning("m_ActivatedCards contains null entries!");
				m_ActivatedCards.RemoveAll((CBaseCard r) => r == null);
			}
			foreach (CBaseCard activatedCard in m_ActivatedCards)
			{
				if (activatedCard.ActiveBonuses.Any((CActiveBonus a) => a == null))
				{
					DLLDebug.LogWarning("Ability Card " + activatedCard.Name + " contains null entries in the ActiveBonuses list!");
					activatedCard.ActiveBonuses.RemoveAll((CActiveBonus r) => r == null);
				}
				foreach (CActiveBonus activeBonuse in activatedCard.ActiveBonuses)
				{
					if (activeBonuse.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
					{
						if (activeBonuse.Type() == type && (activeBonuse.Actor as CHeroSummonActor)?.Summoner == actor && !list.Contains(activeBonuse))
						{
							list.Add(activeBonuse);
						}
					}
					else
					{
						if (activeBonuse.Type() != type || list.Contains(activeBonuse))
						{
							continue;
						}
						if (activeBonuse.IsAura)
						{
							if (activeBonuse.ValidActorsInRangeOfAura.Contains(actor))
							{
								list.Add(activeBonuse);
							}
						}
						else if (actor == activeBonuse.Actor)
						{
							list.Add(activeBonuse);
						}
					}
				}
			}
			return list;
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("Collection was modified"))
			{
				return FindActiveBonuses(type, actor);
			}
			throw ex;
		}
		catch (NullReferenceException ex2)
		{
			string text = "Null Objects: ";
			if (actor == null)
			{
				text += "actor ";
			}
			if (m_ActivatedCards == null)
			{
				text += "m_ActivatedCards ";
			}
			DLLDebug.LogError(ex2.Message + "\n" + text + "\n" + ex2.StackTrace);
			throw ex2;
		}
	}

	public List<CActiveBonus> FindActiveBonuses(CActiveBonus.EActiveBonusBehaviourType behaviour, CActor actor)
	{
		try
		{
			List<CActiveBonus> list = new List<CActiveBonus>();
			foreach (CBaseCard activatedCard in m_ActivatedCards)
			{
				foreach (CActiveBonus activeBonuse in activatedCard.ActiveBonuses)
				{
					if (activeBonuse.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
					{
						if (activeBonuse.Ability.ActiveBonusData.Behaviour == behaviour && (activeBonuse.Actor as CHeroSummonActor)?.Summoner == actor && !list.Contains(activeBonuse) && activeBonuse.IsSong)
						{
							list.Add(activeBonuse);
						}
					}
					else
					{
						if (activeBonuse.Ability.ActiveBonusData.Behaviour != behaviour || list.Contains(activeBonuse))
						{
							continue;
						}
						if (activeBonuse.IsAura)
						{
							if (activeBonuse.ValidActorsInRangeOfAura.Contains(actor))
							{
								list.Add(activeBonuse);
							}
						}
						else if (actor == activeBonuse.Actor)
						{
							list.Add(activeBonuse);
						}
					}
				}
			}
			return list;
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("Collection was modified"))
			{
				return FindActiveBonuses(behaviour, actor);
			}
			throw ex;
		}
	}

	public List<CActiveBonus> FindCasterActiveBonuses(CActiveBonus.EActiveBonusBehaviourType behaviour, CActor actor)
	{
		try
		{
			List<CActiveBonus> list = new List<CActiveBonus>();
			foreach (CBaseCard activatedCard in m_ActivatedCards)
			{
				foreach (CActiveBonus activeBonuse in activatedCard.ActiveBonuses)
				{
					if (activeBonuse.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
					{
						if (activeBonuse.Ability.ActiveBonusData.Behaviour == behaviour && (activeBonuse.Caster as CHeroSummonActor)?.Summoner == actor && !list.Contains(activeBonuse) && activeBonuse.IsSong)
						{
							list.Add(activeBonuse);
						}
					}
					else if (activeBonuse.Ability.ActiveBonusData.Behaviour == behaviour && !list.Contains(activeBonuse) && actor == activeBonuse.Caster)
					{
						list.Add(activeBonuse);
					}
				}
			}
			return list;
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("Collection was modified"))
			{
				return FindCasterActiveBonuses(behaviour, actor);
			}
			throw ex;
		}
	}

	public List<CActiveBonus> FindActiveBonuses(CActiveBonus.EActiveBonusDurationType durationType, CActor actor)
	{
		try
		{
			List<CActiveBonus> activeBonuses = new List<CActiveBonus>();
			if (durationType == CActiveBonus.EActiveBonusDurationType.Summon)
			{
				foreach (CBaseCard activatedCard in m_ActivatedCards)
				{
					activeBonuses.AddRange(from w in activatedCard.ActiveBonuses.FindAll((CActiveBonus x) => x.Duration == durationType && (x.Actor as CHeroSummonActor)?.Summoner == actor)
						where !activeBonuses.Contains(w)
						select w);
				}
			}
			else
			{
				foreach (CBaseCard activatedCard2 in m_ActivatedCards)
				{
					activeBonuses.AddRange(from w in activatedCard2.ActiveBonuses.FindAll((CActiveBonus x) => x.Duration == durationType && x.Actor == actor)
						where !activeBonuses.Contains(w)
						select w);
				}
			}
			return activeBonuses;
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("Collection was modified"))
			{
				return FindActiveBonuses(durationType, actor);
			}
			throw ex;
		}
	}

	public List<CActiveBonus> FindAllSongActiveBonuses(CActor actor, CActor soothsinger)
	{
		try
		{
			List<CActiveBonus> activeBonuses = new List<CActiveBonus>();
			foreach (CBaseCard activatedCard in m_ActivatedCards)
			{
				activeBonuses.AddRange(from w in activatedCard.ActiveBonuses.FindAll((CActiveBonus x) => x.IsSong && x.Ability.Song.Filter.IsValidTarget(actor, soothsinger, isTargetedAbility: false, actor.MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneAction, false))
					where !activeBonuses.Contains(w)
					select w);
			}
			return activeBonuses;
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("Collection was modified"))
			{
				return FindAllSongActiveBonuses(actor, soothsinger);
			}
			throw ex;
		}
	}

	public List<CActiveBonus> FindAllDoomActiveBonuses(CActor actor = null)
	{
		try
		{
			List<CActiveBonus> activeBonuses = new List<CActiveBonus>();
			foreach (CBaseCard activatedCard in m_ActivatedCards)
			{
				activeBonuses.AddRange(from w in activatedCard.ActiveBonuses.FindAll((CActiveBonus x) => x.IsDoom && (actor == null || x.Actor == actor))
					where !activeBonuses.Contains(w)
					select w);
			}
			return activeBonuses;
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("Collection was modified"))
			{
				return FindAllDoomActiveBonuses(actor);
			}
			throw ex;
		}
	}

	public bool HasActiveBonus(CActiveBonus activeBonus)
	{
		foreach (CAbilityCard roundAbilityCard in m_RoundAbilityCards)
		{
			if (roundAbilityCard.ActiveBonuses.Contains(activeBonus))
			{
				return true;
			}
		}
		foreach (CBaseCard activatedCard in m_ActivatedCards)
		{
			if (activatedCard.ActiveBonuses.Contains(activeBonus))
			{
				return true;
			}
		}
		return false;
	}

	public void CheckForFinishedActiveBonuses(CActor actor, int retries = 0)
	{
		bool flag = false;
		while (GameState.ThreadCheckingForActiveBonuses != null && GameState.ThreadCheckingForActiveBonuses != Thread.CurrentThread)
		{
			if (!flag)
			{
				flag = true;
				if (Thread.CurrentThread == ScenarioRuleClient.s_WorkThread)
				{
					DLLDebug.Log("Sleeping SRL Thread" + Environment.StackTrace);
				}
			}
			Thread.Sleep(5);
		}
		GameState.ThreadCheckingForActiveBonuses = Thread.CurrentThread;
		try
		{
			List<CAbilityCard> list = new List<CAbilityCard>();
			foreach (CAbilityCard roundAbilityCard in m_RoundAbilityCards)
			{
				foreach (CActiveBonus item2 in new List<CActiveBonus>(roundAbilityCard.ActiveBonuses))
				{
					if ((item2.Finished() || item2.Finishing()) && roundAbilityCard.ActiveBonuses.Contains(item2))
					{
						roundAbilityCard.ActiveBonuses.Remove(item2);
					}
					if (roundAbilityCard.ActiveBonuses.Count == 0)
					{
						if (m_ActivatedCards.Contains(roundAbilityCard))
						{
							m_ActivatedCards.Remove(roundAbilityCard);
						}
						if (!list.Contains(roundAbilityCard))
						{
							list.Add(roundAbilityCard);
						}
					}
				}
			}
			foreach (CAbilityCard item3 in list)
			{
				MoveAbilityCardToPile(item3, RoundAbilityCards, "RoundAbilityCards");
			}
			list.Clear();
			foreach (CBaseCard item4 in m_ActivatedCards.ToList())
			{
				foreach (CActiveBonus item5 in new List<CActiveBonus>(item4.ActiveBonuses))
				{
					if ((item5.Finished() || item5.Finishing()) && item4.ActiveBonuses.Contains(item5))
					{
						item4.ActiveBonuses.Remove(item5);
					}
					if (item4.ActiveBonuses.Count != 0)
					{
						continue;
					}
					if (m_ActivatedCards.Contains(item4))
					{
						m_ActivatedCards.Remove(item4);
					}
					if (item4.CardType == CBaseCard.ECardType.CharacterAbility && !list.Contains(item4))
					{
						list.Add(item4 as CAbilityCard);
					}
					CItem item = item4 as CItem;
					if (item != null)
					{
						CItem cItem = actor.Inventory.AllItems.SingleOrDefault((CItem s) => s.ID == item.ID);
						if (cItem != null)
						{
							cItem.SlotState = CItem.EItemSlotState.Active;
							actor.Inventory.HandleUsedItem(cItem);
						}
					}
				}
			}
			foreach (CAbilityCard item6 in list)
			{
				MoveAbilityCardToPile(item6, null, "NULL");
			}
			GameState.ThreadCheckingForActiveBonuses = null;
		}
		catch (Exception ex)
		{
			GameState.ThreadCheckingForActiveBonuses = null;
			if (retries < 3)
			{
				CheckForFinishedActiveBonuses(actor, retries + 1);
				return;
			}
			throw ex;
		}
	}

	public void ResetCards()
	{
		foreach (CAbilityCard item in m_AbilityCardsPool)
		{
			item.Reset();
		}
	}

	public void ResetCardAbilitiesAndEnhancementsOnly()
	{
		foreach (CAbilityCard item in m_AbilityCardsPool)
		{
			item.ResetAbilitiesAndEnhancementsOnly();
		}
	}

	public void Reset()
	{
		InitialState = true;
		m_HandAbilityCards.Clear();
		m_DiscardedAbilityCards.Clear();
		m_LostAbilityCards.Clear();
		m_PermanentlyLostAbilityCards.Clear();
		m_ActivatedCards.Clear();
		m_RoundAbilityCards.Clear();
		m_Perks.Clear();
		m_AttackModifierCards.Clear();
		m_DiscardedAttackModifierCards.Clear();
		RoundCardsSelectedInCardSelection.Clear();
		ExtraTurnCards.Clear();
		ExtraTurnCardsSelectedInCardSelectionStack.Clear();
		PendingExtraTurnCardsData.Clear();
		base.TemporaryCards.Clear();
		ResetCards();
		ResetInitiativeBonus();
		m_ShuffleAttackModifierCards = false;
		m_HasLongRested = false;
		m_HasShortRested = false;
		m_HasImprovedShortRested = false;
		m_LongRest = false;
		m_ImprovedShortRest = false;
		if (m_SelectedAbilityCards == null)
		{
			int num = NumberAbilityCardsInBattle;
			if (m_NumberAbilityCardsInBattle > m_AbilityCardsPool.Count)
			{
				DLLDebug.LogError("Number of cards taken in to battle by " + base.ID + " (" + NumberAbilityCardsInBattle + ") is greater than the number of loaded ability cards available (" + m_AbilityCardsPool.Count + ")");
				num = m_AbilityCardsPool.Count;
			}
			for (int i = 0; i < num; i++)
			{
				m_AbilityCardsPool[i].CurrentCardPile = CBaseCard.ECardPile.Hand;
				m_HandAbilityCards.Add(m_AbilityCardsPool[i]);
			}
		}
		else
		{
			int num2 = 0;
			foreach (CAbilityCard selectedAbilityCard in m_SelectedAbilityCards)
			{
				selectedAbilityCard.CurrentCardPile = CBaseCard.ECardPile.Hand;
				m_HandAbilityCards.Add(selectedAbilityCard);
				if (++num2 == m_NumberAbilityCardsInBattle)
				{
					break;
				}
			}
		}
		m_AttackModifierCardsPool = m_CharacterYML.AttackModifierDeck.GetAllAttackModifiers;
		ApplyPerksToList(m_Perks, ref m_AttackModifierCardsPool);
	}

	public void ResetAttackModifierDeck()
	{
		m_AttackModifierCards.Clear();
		m_DiscardedAttackModifierCards.Clear();
		if (ScenarioManager.CurrentScenarioState != null && ScenarioManager.CurrentScenarioState.ScenarioRNGNotNull)
		{
			foreach (AttackModifierYMLData item in m_AttackModifierCardsPool)
			{
				m_AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, m_AttackModifierCards.Count), item);
			}
			return;
		}
		m_AttackModifierCards = m_AttackModifierCardsPool.ToList();
	}

	public void SetHand(List<CAbilityCard> selectedAbilityCards)
	{
		m_SelectedAbilityCards = selectedAbilityCards;
		Reset();
	}

	public override void AddCurseCard(CActor actor, bool canGoOverLimit = false)
	{
		if (ScenarioManager.Scenario.AllPlayers.Select((CPlayerActor s) => (s.Class as CCharacterClass).m_AttackModifierCards.Where((AttackModifierYMLData x) => x.IsCurse).Count()).Sum() < 10 || canGoOverLimit)
		{
			DLLDebug.LogInfo(base.ID + " has added a curse card to their attack modifier deck.");
			m_AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, m_AttackModifierCards.Count), AttackModifiersYML.CreateCurse());
		}
		else
		{
			DLLDebug.LogInfo("Curse card was not added as there are already 10 curse cards in the attack modifier deck");
		}
	}

	public override void RemoveCurses(CActor actor)
	{
		m_AttackModifierCards.RemoveAll((AttackModifierYMLData x) => x.IsCurse);
	}

	public override void AddBlessCard(CActor actor, bool canGoOverLimit = false)
	{
		if (ScenarioManager.Scenario.AllPlayers.Select((CPlayerActor s) => (s.Class as CCharacterClass).m_AttackModifierCards.Where((AttackModifierYMLData x) => x.IsBless).Count()).Sum() + MonsterClassManager.MonsterBlessCount < 10 || canGoOverLimit)
		{
			DLLDebug.LogInfo(base.ID + " has added a bless card to their attack modifier deck.");
			m_AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, m_AttackModifierCards.Count), AttackModifiersYML.CreateBless());
		}
		else
		{
			DLLDebug.LogInfo("Bless card was not added as there are already 10 bless cards in the attack modifier deck");
		}
	}

	public override void RemoveBlesses(CActor actor)
	{
		m_AttackModifierCards.RemoveAll((AttackModifierYMLData x) => x.IsBless);
	}

	public void AddPerk(PerksYMLData perk)
	{
		m_Perks.Add(perk);
	}

	public void ApplyPerk(PerksYMLData perk)
	{
		m_Perks.Add(perk);
		ApplyPerksToList(new List<PerksYMLData> { perk }, ref m_AttackModifierCardsPool);
		ResetAttackModifierDeck();
	}

	public static void ApplyPerksToList(List<PerksYMLData> applyThesePerks, ref List<AttackModifierYMLData> applyToThisList)
	{
		foreach (PerksYMLData applyThesePerk in applyThesePerks)
		{
			foreach (AttackModifierYMLData item2 in applyThesePerk.CardsToAdd)
			{
				applyToThisList.Add(item2.Copy());
			}
			foreach (AttackModifierYMLData am in applyThesePerk.CardsToRemove)
			{
				if (applyToThisList.Any((AttackModifierYMLData a) => a.Name == am.Name))
				{
					AttackModifierYMLData item = applyToThisList.First((AttackModifierYMLData f) => f.Name == am.Name);
					applyToThisList.Remove(item);
				}
			}
		}
	}

	public void ApplyItemModifiersToList(List<CItem> items)
	{
		bool flag = false;
		foreach (PerksYMLData perk in m_Perks)
		{
			if (perk.IgnoreNegativeItemEffects)
			{
				flag = true;
				break;
			}
		}
		foreach (CItem item in items)
		{
			if (item.YMLData.Data.RemoveModifiers.Count <= 0)
			{
				continue;
			}
			foreach (KeyValuePair<AttackModifierYMLData, int> modifierEntry in item.YMLData.Data.RemoveModifiers)
			{
				for (int i = 0; i < modifierEntry.Value; i++)
				{
					AttackModifierYMLData attackModifierYMLData = m_AttackModifierCardsPool.FirstOrDefault((AttackModifierYMLData s) => s.Name == modifierEntry.Key.Name);
					if (attackModifierYMLData != null)
					{
						m_AttackModifierCardsPool.Remove(attackModifierYMLData);
					}
				}
			}
		}
		if (!flag)
		{
			foreach (CItem item2 in items)
			{
				if (item2.YMLData.Data.AdditionalModifiers.Count <= 0)
				{
					continue;
				}
				foreach (KeyValuePair<AttackModifierYMLData, int> additionalModifier in item2.YMLData.Data.AdditionalModifiers)
				{
					for (int num = 0; num < additionalModifier.Value; num++)
					{
						m_AttackModifierCardsPool.Add(additionalModifier.Key.Copy());
					}
				}
			}
		}
		ResetAttackModifierDeck();
	}

	public void ApplyEventModifiersToList(Dictionary<string, int> modifiers)
	{
		foreach (KeyValuePair<string, int> modifierEntry in modifiers)
		{
			for (int i = 0; i < modifierEntry.Value; i++)
			{
				AttackModifierYMLData attackModifierYMLData = ScenarioRuleClient.SRLYML.AttackModifiers.SingleOrDefault((AttackModifierYMLData s) => s.Name == modifierEntry.Key);
				if (attackModifierYMLData != null)
				{
					m_AttackModifierCardsPool.Add(attackModifierYMLData.Copy());
				}
			}
		}
		ResetAttackModifierDeck();
	}

	public void ApplyEnhancements(List<CEnhancement> enhancements)
	{
		foreach (CEnhancement enhancement in enhancements)
		{
			CAbilityCard cAbilityCard = AbilityCardsPool.SingleOrDefault((CAbilityCard s) => s.ID == enhancement.AbilityCardID);
			if (cAbilityCard != null)
			{
				CAbility cAbility = cAbilityCard.FindAbilityOnCard(enhancement.AbilityName);
				if (cAbility != null)
				{
					cAbility.EnhanceAbility(enhancement);
				}
				else
				{
					DLLDebug.LogError("Unable to find ability with Name " + enhancement.AbilityName + " on card " + cAbilityCard.Name);
				}
			}
			else
			{
				DLLDebug.LogError("Unable to find card with ID " + enhancement.AbilityCardID + " on character " + base.ID);
			}
		}
	}

	public void LoadAbilityDeck(AbilityDeckState deck)
	{
		InitialState = deck.InitialState;
		if (deck.SelectedAbilityCardIDsAndInstanceIDs != null)
		{
			try
			{
				m_SelectedAbilityCards.Clear();
				foreach (Tuple<int, int> selectedAbilityCardTuple in deck.SelectedAbilityCardIDsAndInstanceIDs)
				{
					CAbilityCard cAbilityCard = CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == selectedAbilityCardTuple.Item1);
					if (cAbilityCard.SupplyCard)
					{
						CAbilityCard cAbilityCard2 = cAbilityCard.Copy(selectedAbilityCardTuple.Item2);
						CharacterClassManager.AllAbilityCardInstances.Add(cAbilityCard2);
						cAbilityCard = cAbilityCard2;
					}
					m_SelectedAbilityCards.Add(cAbilityCard);
				}
			}
			catch
			{
				throw new Exception("Failed to load in SelectedAbilityCards from save data");
			}
		}
		if (deck.HandAbilityCardIDsAndInstanceIDs != null)
		{
			try
			{
				m_HandAbilityCards.Clear();
				foreach (Tuple<int, int> handAbilityCardTuple in deck.HandAbilityCardIDsAndInstanceIDs)
				{
					CAbilityCard cAbilityCard3 = CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == handAbilityCardTuple.Item1);
					if (cAbilityCard3.SupplyCard)
					{
						CAbilityCard cAbilityCard4 = cAbilityCard3.Copy(handAbilityCardTuple.Item2);
						CharacterClassManager.AllAbilityCardInstances.Add(cAbilityCard4);
						cAbilityCard3 = cAbilityCard4;
					}
					m_HandAbilityCards.Add(cAbilityCard3);
				}
			}
			catch
			{
				throw new Exception("Failed to load in HandAbilityCards from save data");
			}
		}
		if (deck.RoundAbilityCardIDsAndInstanceID != null)
		{
			try
			{
				m_RoundAbilityCards.Clear();
			}
			catch
			{
				throw new Exception("Failed to load in RoundAbilityCards from save data");
			}
		}
		if (deck.DiscardedAbilityCardIDs != null)
		{
			try
			{
				m_DiscardedAbilityCards = deck.DiscardedAbilityCardIDs.Select((int s) => CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == s)).ToList();
			}
			catch
			{
				throw new Exception("Failed to load in DiscardedAbilityCards from save data");
			}
		}
		if (deck.LostAbilityCardIDs != null)
		{
			try
			{
				m_LostAbilityCards = deck.LostAbilityCardIDs.Select((int s) => CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == s)).ToList();
			}
			catch
			{
				throw new Exception("Failed to load in LostAbilityCards from save data");
			}
		}
		if (deck.PermaLostAbilityCardIDs != null)
		{
			try
			{
				m_PermanentlyLostAbilityCards = deck.PermaLostAbilityCardIDs.Select((int s) => CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == s)).ToList();
			}
			catch
			{
				throw new Exception("Failed to load in PermaLostAbilityCards from save data");
			}
		}
		if (deck.ActivatedAbilityCardIDs != null)
		{
			try
			{
				foreach (Tuple<int, CBaseCard.ECardType> activatedAbilityCard in deck.ActivatedAbilityCardIDs)
				{
					switch (activatedAbilityCard.Item2)
					{
					case CBaseCard.ECardType.CharacterAbility:
					{
						CAbilityCard cAbilityCard5 = CharacterClassManager.AllAbilityCards.Single((CAbilityCard c) => c.ID == activatedAbilityCard.Item1);
						if (cAbilityCard5 != null)
						{
							m_ActivatedCards.Add(cAbilityCard5);
							break;
						}
						throw new Exception("Ability card with ID " + activatedAbilityCard.Item1 + " was null");
					}
					case CBaseCard.ECardType.Item:
					{
						CItem getItem = ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData c) => c.ID == activatedAbilityCard.Item1).GetItem;
						if (getItem != null)
						{
							m_ActivatedCards.Add(getItem);
							break;
						}
						throw new Exception("Item card with ID " + activatedAbilityCard.Item1 + " was null");
					}
					case CBaseCard.ECardType.AttackModifier:
					{
						AttackModifierCard card = ScenarioRuleClient.SRLYML.AttackModifiers.Single((AttackModifierYMLData c) => c.ID == activatedAbilityCard.Item1).Card;
						if (card != null)
						{
							m_ActivatedCards.Add(card);
							break;
						}
						throw new Exception("Attack Modifier card with ID " + activatedAbilityCard.Item1 + " was null");
					}
					default:
						throw new Exception("Invalid Activated Card Type.  Unable to load activated cards");
					}
				}
				for (int num = m_ActivatedCards.Count - 1; num >= 0; num--)
				{
					CBaseCard activatedCard = m_ActivatedCards[num];
					if (deck.ActiveBonuses.Any((ActiveBonusState a) => a.CardID == activatedCard.ID))
					{
						activatedCard.ActionHasHappened = true;
					}
					else
					{
						m_ActivatedCards.Remove(activatedCard);
						if (activatedCard is CAbilityCard item)
						{
							m_DiscardedAbilityCards.Add(item);
						}
					}
				}
			}
			catch
			{
				throw new Exception("Failed to load in ActivatedAbilityCards from save data");
			}
		}
		if (deck.InitiativeAbilityCardID.HasValue)
		{
			try
			{
				CAbilityCard initiativeAbilityCard = CharacterClassManager.AllAbilityCards.Single((CAbilityCard s) => s.ID == deck.InitiativeAbilityCardID.Value);
				m_InitiativeAbilityCard = initiativeAbilityCard;
			}
			catch
			{
				throw new Exception("Failed to load in InitiativeAbilityCard from save data");
			}
		}
		if (deck.Enhancements != null && deck.Enhancements.Count > 0)
		{
			ApplyEnhancements(deck.Enhancements);
		}
		if (deck.ActiveBonuses == null || deck.ActiveBonuses.Count <= 0)
		{
			return;
		}
		for (int num2 = deck.ActiveBonuses.Count - 1; num2 >= 0; num2--)
		{
			ActiveBonusState activeBonusState = deck.ActiveBonuses[num2];
			for (int num3 = 0; num3 < deck.ActiveBonuses.Count; num3++)
			{
				if (num2 != num3)
				{
					ActiveBonusState state = deck.ActiveBonuses[num3];
					if (ActiveBonusState.Compare(activeBonusState, state, string.Empty, string.Empty, isMPCompare: false).Count <= 0)
					{
						DLLDebug.LogError("Duplicate Active Bonus State removed from Character Class " + base.ID);
						deck.ActiveBonuses.Remove(activeBonusState);
						break;
					}
				}
			}
		}
		List<CAbility> list = new List<CAbility>();
		foreach (ActiveBonusState activeBonusState2 in deck.ActiveBonuses)
		{
			CItem cItem = null;
			CAbilityCard cAbilityCard6 = m_ActivatedCards.SingleOrDefault((CBaseCard s) => s.ID == activeBonusState2.CardID && (s.Name == activeBonusState2.CardName || s.Name.Contains(activeBonusState2.CardName.Replace(" ", string.Empty).Replace("'", string.Empty)))) as CAbilityCard;
			if (cAbilityCard6 == null)
			{
				cItem = m_ActivatedCards.SingleOrDefault((CBaseCard s) => s.ID == activeBonusState2.CardID && (s.Name == activeBonusState2.CardName || s.Name.Contains(activeBonusState2.CardName.Replace(" ", string.Empty).Replace("'", string.Empty)))) as CItem;
				if (cItem == null)
				{
					DLLDebug.LogError("Unable to find ability or item card with ID " + activeBonusState2.CardID + " for active bonus state");
					continue;
				}
			}
			CAbility cAbility = ((cAbilityCard6 != null) ? cAbilityCard6.FindAbilityOnCard(activeBonusState2.AbilityName) : cItem?.FindAbilityOnItemCard(activeBonusState2.AbilityName));
			if (cAbility == null)
			{
				DLLDebug.LogError("Unable to find ability " + activeBonusState2.AbilityName + " for active bonus state");
				continue;
			}
			CActor cActor = ScenarioManager.FindActor(activeBonusState2.ActorGuid);
			if (cActor == null)
			{
				DLLDebug.LogError("Unable to find actor attached to active bonus");
				continue;
			}
			CActor cActor2 = null;
			if (activeBonusState2.CasterGuid != null)
			{
				cActor2 = ScenarioManager.FindActor(activeBonusState2.CasterGuid);
			}
			else if (cAbilityCard6 != null)
			{
				cActor2 = ScenarioManager.FindActorWithAbilityCard(cAbilityCard6);
			}
			else if (cItem != null)
			{
				cActor2 = ScenarioManager.FindActorWithActivatedItemFromID(cItem.ID, this);
			}
			if (cActor2 == null)
			{
				DLLDebug.LogError("Unable to find caster attached to active bonus");
				continue;
			}
			CAbility cAbility2 = CAbility.CopyAbility(cAbility, generateNewID: false);
			if (!list.Contains(cAbility))
			{
				foreach (CEnhancement abilityEnhancement in cAbility2.AbilityEnhancements)
				{
					cAbility2.ApplyEnhancement(abilityEnhancement);
				}
				list.Add(cAbility);
			}
			int? bespokeStrength = null;
			if (activeBonusState2.BespokeBehaviourStrength != 0)
			{
				bespokeStrength = activeBonusState2.BespokeBehaviourStrength;
			}
			if (activeBonusState2.IsDoom)
			{
				CAbilityAddDoom cAbilityAddDoom = (CAbilityAddDoom)cAbilityCard6.FindAbilityOfType(CAbility.EAbilityType.AddDoom)[0];
				cActor2.AddDoom(cAbilityAddDoom.Doom, cActor, addDoomActiveBonuses: false);
				cAbilityCard6.AddActiveBonus(cAbility, cActor, cActor2, activeBonusState2.ID, activeBonusState2.Remaining, isAugment: false, isSong: false, loadingItemBonus: false, isDoom: true, bespokeStrength, activeBonusState2.ActiveBonusStartRound);
				cAbilityCard6.SetSelectedAction(activeBonusState2.IsTopAction ? cAbilityCard6.TopAction : cAbilityCard6.BottomAction);
			}
			else if (cAbility2 is CAbilityAttack { Augment: not null } || cAbility2 is CAbilityAddActiveBonus { Augment: not null })
			{
				cActor.AddAugmentOrSong(cAbility2, cActor2, activeBonusState2.ID, activeBonusState2.ActiveBonusStartRound);
				cAbilityCard6.SetSelectedAction(activeBonusState2.IsTopAction ? cAbilityCard6.TopAction : cAbilityCard6.BottomAction);
			}
			else if (cAbility2.Song != null)
			{
				cActor.AddAugmentOrSong(cAbility2, cActor2, activeBonusState2.ID, activeBonusState2.ActiveBonusStartRound);
				cAbilityCard6.SetSelectedAction(activeBonusState2.IsTopAction ? cAbilityCard6.TopAction : cAbilityCard6.BottomAction);
			}
			else if (cItem != null)
			{
				cItem.AddActiveBonus(cAbility2, cActor, cActor2, activeBonusState2.ID, activeBonusState2.Remaining, isAugment: false, isSong: false, loadingItemBonus: true, isDoom: false, bespokeStrength, activeBonusState2.ActiveBonusStartRound);
				cActor.Inventory.HandleActiveItemTriggered(cItem, ignoreConsume: true);
			}
			else
			{
				cAbilityCard6.SetSelectedAction(activeBonusState2.IsTopAction ? cAbilityCard6.TopAction : cAbilityCard6.BottomAction);
				cAbilityCard6.AddActiveBonus(cAbility2, cActor, cActor2, activeBonusState2.ID, activeBonusState2.Remaining, isAugment: false, isSong: false, loadingItemBonus: false, isDoom: false, bespokeStrength, activeBonusState2.ActiveBonusStartRound);
			}
		}
	}

	public void LoadAttackModifierDeck(AttackModifierDeckState deck)
	{
		if (deck.AttackModifierCardsAvailable != null)
		{
			m_AttackModifierCards.Clear();
			CClass.AddModifierCards(deck.AttackModifierCardsAvailable, ref m_AttackModifierCards);
		}
		if (deck.AttackModifierCardsDiscarded != null)
		{
			m_DiscardedAttackModifierCards.Clear();
			CClass.AddModifierCards(deck.AttackModifierCardsDiscarded, ref m_DiscardedAttackModifierCards);
		}
		m_AttackModifierCardsPool.Clear();
		m_AttackModifierCardsPool = (from w in m_AttackModifierCards.Concat(m_DiscardedAttackModifierCards)
			where !w.IsBless && !w.IsCurse
			select w).ToList();
	}

	public void AddAdditionalModifierCards(List<string> cardNames)
	{
		CClass.AddModifierCards(cardNames, ref m_AttackModifierCards);
	}

	public CActor GetActor()
	{
		return ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor s) => s.CharacterClass.ID == base.ID);
	}

	public CharacterResourceData GetCharacterResourceData(string resourceID)
	{
		return CharacterYML.CharacterResourceDatas.SingleOrDefault((CharacterResourceData x) => x.ID == resourceID);
	}

	public StartRoundCardState GetCurrentCardState()
	{
		return new StartRoundCardState(RoundAbilityCards, HandAbilityCards, DiscardedAbilityCards, LostAbilityCards, PermanentlyLostAbilityCards, ActivatedCards, InitiativeAbilityCard, SubInitiativeAbilityCard, HasShortRested, HasImprovedShortRested, ShortRestCardBurned, ImprovedShortRest, ShortRestCardRedrawn, LongRest);
	}

	public void ProxySetStartRoundDeckState(StartRoundCardState startRoundCardState)
	{
		m_HasShortRested = startRoundCardState.HasShortRested;
		m_HasImprovedShortRested = startRoundCardState.ImprovedShortRest;
		ImprovedShortRest = startRoundCardState.ImprovedShortRest;
		LongRest = startRoundCardState.LongRestSelected;
		for (int num = ActivatedCards.Count - 1; num >= 0; num--)
		{
			CBaseCard cBaseCard = ActivatedCards[num];
			if (!startRoundCardState.ActivatedCardIDs.Contains(cBaseCard.ID))
			{
				List<CActiveBonus> list = cBaseCard.ActiveBonuses.ToList();
				for (int num2 = list.Count - 1; num2 >= 0; num2--)
				{
					CClass.CancelActiveBonus(list[num2]);
				}
				cBaseCard.ActiveBonuses.Clear();
				m_ActivatedCards.Remove(cBaseCard);
			}
		}
		m_RoundAbilityCards.Clear();
		m_HandAbilityCards.Clear();
		m_DiscardedAbilityCards.Clear();
		m_LostAbilityCards.Clear();
		m_PermanentlyLostAbilityCards.Clear();
		foreach (int roundCardInstanceID in startRoundCardState.RoundAbilityCardInstanceIDs)
		{
			CAbilityCard item = CharacterClassManager.AllAbilityCardInstances.Single((CAbilityCard c) => c.CardInstanceID == roundCardInstanceID);
			m_RoundAbilityCards.Add(item);
		}
		foreach (int handAbilityCardInstanceID in startRoundCardState.HandAbilityCardInstanceIDs)
		{
			CAbilityCard item2 = CharacterClassManager.AllAbilityCardInstances.Single((CAbilityCard c) => c.CardInstanceID == handAbilityCardInstanceID);
			m_HandAbilityCards.Add(item2);
		}
		foreach (int discardedCardInstanceID in startRoundCardState.DiscardedAbilityCardInstanceIDs)
		{
			CAbilityCard item3 = CharacterClassManager.AllAbilityCardInstances.Single((CAbilityCard c) => c.CardInstanceID == discardedCardInstanceID);
			m_DiscardedAbilityCards.Add(item3);
		}
		foreach (int lostCardInstanceID in startRoundCardState.LostAbilityCardInstanceIDs)
		{
			CAbilityCard item4 = CharacterClassManager.AllAbilityCardInstances.Single((CAbilityCard c) => c.CardInstanceID == lostCardInstanceID);
			m_LostAbilityCards.Add(item4);
		}
		foreach (int permaLostCardInstanceID in startRoundCardState.PermaLostAbilityCardInstanceIDs)
		{
			CAbilityCard item5 = CharacterClassManager.AllAbilityCardInstances.Single((CAbilityCard c) => c.CardInstanceID == permaLostCardInstanceID);
			m_PermanentlyLostAbilityCards.Add(item5);
		}
		m_InitiativeAbilityCard = CharacterClassManager.AllAbilityCardInstances.SingleOrDefault((CAbilityCard c) => c.CardInstanceID == startRoundCardState.InitiativeCardID);
		m_SubInitiativeAbilityCard = CharacterClassManager.AllAbilityCardInstances.SingleOrDefault((CAbilityCard c) => c.CardInstanceID == startRoundCardState.SubInitiativeCardID);
		if (HandAbilityCards.Count + RoundAbilityCards.Count < 2 && DiscardedAbilityCards.Count < 2)
		{
			CPlayerActor cPlayerActor = (CPlayerActor)GetActor();
			bool actorWasAsleep = cPlayerActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
			GameState.KillActor(cPlayerActor, cPlayerActor, CActor.ECauseOfDeath.NoMoreCards, out var _, null, actorWasAsleep);
			CPlayersExhausted_MessageData cPlayersExhausted_MessageData = new CPlayersExhausted_MessageData(cPlayerActor);
			cPlayersExhausted_MessageData.m_Players = new List<CPlayerActor> { cPlayerActor };
			ScenarioRuleClient.MessageHandler(cPlayersExhausted_MessageData);
		}
	}
}
