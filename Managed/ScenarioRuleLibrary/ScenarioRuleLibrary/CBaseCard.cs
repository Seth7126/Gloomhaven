using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CBaseCard : ISerializable
{
	[Serializable]
	public enum ActionType
	{
		TopAction,
		DefaultAttackAction,
		DefaultMoveAction,
		BottomAction,
		NA
	}

	[Serializable]
	public enum ECardType
	{
		None,
		CharacterAbility,
		HeroSummonAbility,
		MonsterAbility,
		Item,
		AttackModifier,
		ScenarioModifier
	}

	[Serializable]
	public enum ECardPile
	{
		None,
		Discarded,
		Round,
		Hand,
		Activated,
		Lost,
		PermanentlyLost
	}

	public static ActionType[] ActionTypes = (ActionType[])Enum.GetValues(typeof(ActionType));

	private string m_StringID;

	public string Name
	{
		get
		{
			try
			{
				switch (CardType)
				{
				case ECardType.CharacterAbility:
					return ScenarioRuleClient.SRLYML.AbilityCards.Single((AbilityCardYMLData s) => s.ID == ID).Name;
				case ECardType.HeroSummonAbility:
					return ScenarioRuleClient.SRLYML.HeroSummons.Single((HeroSummonYMLData s) => s.ID == m_StringID).LocKey;
				case ECardType.MonsterAbility:
					return (this as CBaseAbilityCard).ClassID + ID;
				case ECardType.Item:
					return ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData s) => s.ID == ID).Name;
				case ECardType.AttackModifier:
					return m_StringID;
				case ECardType.ScenarioModifier:
					return m_StringID;
				default:
					DLLDebug.LogError("Invalid card type set for base card.");
					return string.Empty;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Invalid BaseCard ID.  ID = " + ID + " CardType = " + CardType.ToString() + "\n" + ex.Message + "\n" + ex.StackTrace);
				return string.Empty;
			}
		}
	}

	public int ID { get; private set; }

	public ECardType CardType { get; private set; }

	public List<CActiveBonus> ActiveBonuses { get; private set; }

	public ECardPile CurrentCardPile { get; set; }

	public bool ActionHasHappened { get; set; }

	public string StrictName => Name.Replace("ABILITY_CARD_", "");

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ID", ID);
		info.AddValue("m_StringID", m_StringID);
		info.AddValue("CardType", CardType);
		info.AddValue("CurrentCardPile", CurrentCardPile);
		info.AddValue("ActionHasHappened", ActionHasHappened);
	}

	protected CBaseCard(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "ID":
					ID = info.GetInt32("ID");
					break;
				case "m_StringID":
					m_StringID = info.GetString("m_StringID");
					break;
				case "CardType":
					CardType = (ECardType)info.GetValue("CardType", typeof(ECardType));
					break;
				case "CurrentCardPile":
					CurrentCardPile = (ECardPile)info.GetValue("CurrentCardPile", typeof(ECardPile));
					break;
				case "ActionHasHappened":
					ActionHasHappened = info.GetBoolean("ActionHasHappened");
					break;
				case "Name":
					m_StringID = info.GetString("Name");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CBaseCard entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		ActiveBonuses = new List<CActiveBonus>();
	}

	public CBaseCard(int id, ECardType cardType, string stringID)
	{
		ID = id;
		m_StringID = stringID;
		CardType = cardType;
		ActiveBonuses = new List<CActiveBonus>();
		CurrentCardPile = ECardPile.None;
		ActionHasHappened = false;
	}

	public CActiveBonus AddActiveBonus(CAbility ability, CActor actor, CActor caster, int? iD = null, int? remaining = null, bool isAugment = false, bool isSong = false, bool loadingItemBonus = false, bool isDoom = false, int? bespokeStrength = null, int? activeBonusStartRound = null, bool bonusCheck = true)
	{
		if (!activeBonusStartRound.HasValue)
		{
			activeBonusStartRound = ScenarioManager.CurrentScenarioState.RoundNumber;
		}
		CActiveBonus cActiveBonus = null;
		bool flag = false;
		if (CardType == ECardType.Item && actor.Class is CCharacterClass cCharacterClass && cCharacterClass.ActivatedCards.SingleOrDefault((CBaseCard s) => s.ID == ID && s is CItem) is CItem cItem)
		{
			if (bespokeStrength.HasValue)
			{
				ability.Strength = bespokeStrength.Value;
			}
			cActiveBonus = CActiveBonus.CreateActiveBonus(cItem, ability, actor, caster, activeBonusStartRound.Value, iD, remaining, isAugment, isSong, loadingItemBonus, isDoom);
			bool flag2 = false;
			foreach (CActiveBonus activeBonuse in cItem.ActiveBonuses)
			{
				if (CActiveBonus.AreTheSame(cActiveBonus, activeBonuse))
				{
					DLLDebug.LogError("Attempting to add a duplicate of an active bonus to the base card. " + ID);
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				cItem.ActiveBonuses.Add(cActiveBonus);
			}
			flag = true;
		}
		if (!flag)
		{
			if (actor?.Augments != null)
			{
				foreach (CAugment augment in actor.Augments)
				{
					if (this != augment.RegisteredBaseCard || augment.AugmentType != CAugment.EAugmentType.Bonus || augment.Abilities == null)
					{
						continue;
					}
					using List<CAbility>.Enumerator enumerator3 = augment.Abilities.GetEnumerator();
					if (enumerator3.MoveNext())
					{
						CAbility current3 = enumerator3.Current;
						cActiveBonus = CActiveBonus.CreateActiveBonus(augment.RegisteredBaseCard, current3, actor, caster, activeBonusStartRound.Value, iD, null, isAugment: true);
						ActiveBonuses.Add(cActiveBonus);
						if (actor.Class is CCharacterClass cCharacterClass2)
						{
							cCharacterClass2.ActivateCard(augment.RegisteredBaseCard);
						}
						return cActiveBonus;
					}
				}
			}
			if (bespokeStrength.HasValue)
			{
				ability.Strength = bespokeStrength.Value;
			}
			if (ability is CAbilityAddActiveBonus { AddAbility: not null } cAbilityAddActiveBonus)
			{
				if (cAbilityAddActiveBonus.AddAbility.AbilityBaseCard == null)
				{
					cAbilityAddActiveBonus.AddAbility.ParentAbilityBaseCard = cAbilityAddActiveBonus.AbilityBaseCard;
				}
				if (cAbilityAddActiveBonus.AddAbility is CAbilityChooseAbility { ChooseAbilities: not null } cAbilityChooseAbility)
				{
					foreach (CAbility chooseAbility in cAbilityChooseAbility.ChooseAbilities)
					{
						if (chooseAbility.AbilityBaseCard == null)
						{
							chooseAbility.ParentAbilityBaseCard = cAbilityAddActiveBonus.AbilityBaseCard;
						}
					}
				}
			}
			cActiveBonus = CActiveBonus.CreateActiveBonus(this, ability, actor, caster, activeBonusStartRound.Value, iD, remaining, isAugment, isSong, loadingItemBonus, isDoom);
			ActiveBonuses.Add(cActiveBonus);
		}
		if (cActiveBonus.Ability.ActiveBonusData.GiveAbilityCardToActor && this is CAbilityCard item && actor is CPlayerActor cPlayerActor && caster is CPlayerActor cPlayerActor2)
		{
			if (cPlayerActor2.IsTakingExtraTurn)
			{
				cPlayerActor2.CharacterClass.ExtraTurnCards.Remove(item);
			}
			else
			{
				cPlayerActor2.CharacterClass.RoundAbilityCards.Remove(item);
			}
			if (!cPlayerActor.CharacterClass.ActivatedCards.Contains(item))
			{
				cPlayerActor.CharacterClass.ActivatedCards.Add(item);
				CSupplyCardsGiven_MessageData cSupplyCardsGiven_MessageData = new CSupplyCardsGiven_MessageData(cPlayerActor2);
				cSupplyCardsGiven_MessageData.m_ActorGivenCards = cPlayerActor;
				cSupplyCardsGiven_MessageData.m_SupplyCardsGiven = new List<CAbilityCard> { item };
				ScenarioRuleClient.MessageHandler(cSupplyCardsGiven_MessageData);
			}
		}
		if (cActiveBonus.IsAura)
		{
			cActiveBonus.FindValidActorsInRangeOfAura();
		}
		if (bonusCheck)
		{
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
			}
		}
		return cActiveBonus;
	}

	public void AddActiveBonus(CActiveBonus activeBonus)
	{
		ActiveBonuses.Add(activeBonus);
		if (activeBonus.IsAura)
		{
			foreach (CActor item in activeBonus.ValidActorsInRangeOfAura)
			{
				item.CheckForCachedValuesAfterActiveBonusesUpdate();
			}
			return;
		}
		activeBonus.Actor.CheckForCachedValuesAfterActiveBonusesUpdate();
	}

	public CActiveBonus GetAugmentActiveBonus(CAugment augment)
	{
		CActiveBonus cActiveBonus = ActiveBonuses.SingleOrDefault((CActiveBonus s) => s.Ability.Augment == augment);
		if (cActiveBonus == null && augment.AugmentType == CAugment.EAugmentType.Bonus && augment.Abilities != null && augment.Abilities[0].Augment != null)
		{
			cActiveBonus = ActiveBonuses.SingleOrDefault((CActiveBonus s) => s.Ability.Augment == augment.Abilities[0].Augment);
		}
		return cActiveBonus;
	}

	public CActiveBonus GetSongActiveBonus(CSong song)
	{
		return ActiveBonuses.SingleOrDefault((CActiveBonus s) => s.Ability.Song == song);
	}

	public void RemoveAugmentActiveBonus(CAugment augment)
	{
		CActiveBonus item = ActiveBonuses.SingleOrDefault((CActiveBonus s) => s.Ability.Augment == augment);
		ActiveBonuses.Remove(item);
	}

	public void RemoveSongActiveBonus(CSong song)
	{
		CActiveBonus item = ActiveBonuses.SingleOrDefault((CActiveBonus s) => s.Ability.Song == song);
		ActiveBonuses.Remove(item);
	}

	public virtual void Reset()
	{
		CurrentCardPile = ECardPile.None;
		ActiveBonuses.Clear();
	}

	public virtual ActionType GetAbilityActionType(CAbility ability)
	{
		return ActionType.NA;
	}

	public static List<Tuple<int, string>> Compare(CBaseCard state1, CBaseCard state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			if (state1.Name != state2.Name)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2801, "CBaseCard Name does not match.", new List<string[]>
				{
					new string[3] { "Name", state1.Name, state2.Name },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"CardType",
						state1.CardType.ToString(),
						state2.CardType.ToString()
					}
				});
			}
			if (state1.ID != state2.ID)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2802, "CBaseCard ID does not match.", new List<string[]>
				{
					new string[3] { "Name", state1.Name, state2.Name },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"CardType",
						state1.CardType.ToString(),
						state2.CardType.ToString()
					}
				});
			}
			if (state1.CardType != state2.CardType)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2803, "CBaseCard CardType does not match.", new List<string[]>
				{
					new string[3] { "Name", state1.Name, state2.Name },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"CardType",
						state1.CardType.ToString(),
						state2.CardType.ToString()
					}
				});
			}
			if (state1.CurrentCardPile != state2.CurrentCardPile)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2804, "CBaseCard CurrentCardPile does not match.", new List<string[]>
				{
					new string[3] { "Name", state1.Name, state2.Name },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"CardType",
						state1.CardType.ToString(),
						state2.CardType.ToString()
					},
					new string[3]
					{
						"CurrentCardPile",
						state1.CurrentCardPile.ToString(),
						state2.CurrentCardPile.ToString()
					}
				});
			}
			if (state1.ActionHasHappened != state2.ActionHasHappened)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2805, "CBaseCard ActionHasHappened does not match.", new List<string[]>
				{
					new string[3] { "Name", state1.Name, state2.Name },
					new string[3]
					{
						"ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"CardType",
						state1.CardType.ToString(),
						state2.CardType.ToString()
					},
					new string[3]
					{
						"ActionHasHappened",
						state1.ActionHasHappened.ToString(),
						state2.ActionHasHappened.ToString()
					}
				});
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(2899, "Exception during CBaseCard compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}

	public CBaseCard()
	{
	}

	public CBaseCard(CBaseCard state, ReferenceDictionary references)
	{
		ID = state.ID;
		CardType = state.CardType;
		ActiveBonuses = references.Get(state.ActiveBonuses);
		if (ActiveBonuses == null && state.ActiveBonuses != null)
		{
			ActiveBonuses = new List<CActiveBonus>();
			for (int i = 0; i < state.ActiveBonuses.Count; i++)
			{
				CActiveBonus cActiveBonus = state.ActiveBonuses[i];
				CActiveBonus cActiveBonus2 = references.Get(cActiveBonus);
				if (cActiveBonus2 == null && cActiveBonus != null)
				{
					CActiveBonus cActiveBonus3 = ((cActiveBonus is CAddConditionActiveBonus state2) ? new CAddConditionActiveBonus(state2, references) : ((cActiveBonus is CAddHealActiveBonus state3) ? new CAddHealActiveBonus(state3, references) : ((cActiveBonus is CAddRangeActiveBonus state4) ? new CAddRangeActiveBonus(state4, references) : ((cActiveBonus is CAddTargetActiveBonus state5) ? new CAddTargetActiveBonus(state5, references) : ((cActiveBonus is CAdjustInitiativeActiveBonus state6) ? new CAdjustInitiativeActiveBonus(state6, references) : ((cActiveBonus is CAdvantageActiveBonus state7) ? new CAdvantageActiveBonus(state7, references) : ((cActiveBonus is CAttackActiveBonus state8) ? new CAttackActiveBonus(state8, references) : ((cActiveBonus is CAttackersGainDisadvantageActiveBonus state9) ? new CAttackersGainDisadvantageActiveBonus(state9, references) : ((cActiveBonus is CChangeCharacterModelActiveBonus state10) ? new CChangeCharacterModelActiveBonus(state10, references) : ((cActiveBonus is CChangeConditionActiveBonus state11) ? new CChangeConditionActiveBonus(state11, references) : ((cActiveBonus is CChangeModifierActiveBonus state12) ? new CChangeModifierActiveBonus(state12, references) : ((cActiveBonus is CChooseAbilityActiveBonus state13) ? new CChooseAbilityActiveBonus(state13, references) : ((cActiveBonus is CDamageActiveBonus state14) ? new CDamageActiveBonus(state14, references) : ((cActiveBonus is CDisableCardActionActiveBonus state15) ? new CDisableCardActionActiveBonus(state15, references) : ((cActiveBonus is CDuringActionAbilityActiveBonus state16) ? new CDuringActionAbilityActiveBonus(state16, references) : ((cActiveBonus is CDuringTurnAbilityActiveBonus state17) ? new CDuringTurnAbilityActiveBonus(state17, references) : ((cActiveBonus is CEndActionAbilityActiveBonus state18) ? new CEndActionAbilityActiveBonus(state18, references) : ((cActiveBonus is CEndRoundAbilityActiveBonus state19) ? new CEndRoundAbilityActiveBonus(state19, references) : ((cActiveBonus is CEndTurnAbilityActiveBonus state20) ? new CEndTurnAbilityActiveBonus(state20, references) : ((cActiveBonus is CForgoActionsForCompanionActiveBonus state21) ? new CForgoActionsForCompanionActiveBonus(state21, references) : ((cActiveBonus is CHealthReductionActiveBonus state22) ? new CHealthReductionActiveBonus(state22, references) : ((cActiveBonus is CImmunityActiveBonus state23) ? new CImmunityActiveBonus(state23, references) : ((cActiveBonus is CInfuseActiveBonus state24) ? new CInfuseActiveBonus(state24, references) : ((cActiveBonus is CInvulnerabilityActiveBonus state25) ? new CInvulnerabilityActiveBonus(state25, references) : ((cActiveBonus is CItemLockActiveBonus state26) ? new CItemLockActiveBonus(state26, references) : ((cActiveBonus is CLootActiveBonus state27) ? new CLootActiveBonus(state27, references) : ((cActiveBonus is CMoveActiveBonus state28) ? new CMoveActiveBonus(state28, references) : ((cActiveBonus is COverhealActiveBonus state29) ? new COverhealActiveBonus(state29, references) : ((cActiveBonus is COverrideAbilityTypeActiveBonus state30) ? new COverrideAbilityTypeActiveBonus(state30, references) : ((cActiveBonus is CPierceInvulnerabilityActiveBonus state31) ? new CPierceInvulnerabilityActiveBonus(state31, references) : ((cActiveBonus is CPreventDamageActiveBonus state32) ? new CPreventDamageActiveBonus(state32, references) : ((cActiveBonus is CRedirectActiveBonus state33) ? new CRedirectActiveBonus(state33, references) : ((cActiveBonus is CRetaliateActiveBonus state34) ? new CRetaliateActiveBonus(state34, references) : ((cActiveBonus is CShieldActiveBonus state35) ? new CShieldActiveBonus(state35, references) : ((cActiveBonus is CStartActionAbilityActiveBonus state36) ? new CStartActionAbilityActiveBonus(state36, references) : ((cActiveBonus is CStartRoundAbilityActiveBonus state37) ? new CStartRoundAbilityActiveBonus(state37, references) : ((cActiveBonus is CStartTurnAbilityActiveBonus state38) ? new CStartTurnAbilityActiveBonus(state38, references) : ((cActiveBonus is CSummonActiveBonus state39) ? new CSummonActiveBonus(state39, references) : ((!(cActiveBonus is CUntargetableActiveBonus state40)) ? new CActiveBonus(cActiveBonus, references) : new CUntargetableActiveBonus(state40, references))))))))))))))))))))))))))))))))))))))));
					cActiveBonus2 = cActiveBonus3;
					references.Add(cActiveBonus, cActiveBonus2);
				}
				ActiveBonuses.Add(cActiveBonus2);
			}
			references.Add(state.ActiveBonuses, ActiveBonuses);
		}
		CurrentCardPile = state.CurrentCardPile;
		ActionHasHappened = state.ActionHasHappened;
		m_StringID = state.m_StringID;
	}
}
