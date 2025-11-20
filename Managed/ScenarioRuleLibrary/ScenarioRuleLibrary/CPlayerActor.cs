using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CPlayerActor : CActor
{
	private string m_CharacterName;

	private string m_CompanionGuid;

	private CHeroSummonActor m_CompanionCached;

	private List<int> m_characterHealthTable;

	public CCharacterClass CharacterClass => (CCharacterClass)m_Class;

	public string CharacterName
	{
		get
		{
			return m_CharacterName;
		}
		set
		{
			m_CharacterName = value;
		}
	}

	public bool SkipTopCardAction { get; set; }

	public bool SkipBottomCardAction { get; set; }

	public bool IsLongRestSelected { get; set; }

	public bool IsLongRestActionSelected { get; set; }

	public string OverrideCharacterModel { get; set; }

	public CAbilityExtraTurn.EExtraTurnType SelectingCardsForExtraTurnOfType { get; set; }

	public override bool IsDeadPlayer => base.IsDead;

	public override bool Flying
	{
		get
		{
			if (base.Tokens.HasKey(CCondition.ENegativeCondition.StopFlying))
			{
				return false;
			}
			foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.Move))
			{
				if (item.BespokeBehaviour != null && item.BespokeBehaviour is CMoveActiveBonus_BuffMove cMoveActiveBonus_BuffMove)
				{
					cMoveActiveBonus_BuffMove.CalculateBuffs(out var _, out var _, out var _, out var fly, out var _, out var _);
					if (fly.HasValue && fly.Value)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public override bool Invulnerable => CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.Invulnerability).Count > 0;

	public override bool PierceInvulnerability => CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.PierceInvulnerability).Count > 0;

	public override bool Untargetable => CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.Untargetable).Count > 0;

	public override List<CAbility.EAbilityType> Immunities
	{
		get
		{
			List<CAbility.EAbilityType> list = new List<CAbility.EAbilityType>();
			foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.ImmunityTo))
			{
				if (!(item.Ability is CAbilityImmunityTo cAbilityImmunityTo))
				{
					continue;
				}
				foreach (CAbility.EAbilityType immuneToAbilityType in cAbilityImmunityTo.ImmuneToAbilityTypes)
				{
					if (!list.Contains(immuneToAbilityType))
					{
						list.Add(immuneToAbilityType);
					}
				}
			}
			return list;
		}
	}

	public CHeroSummonActor CompanionSummon
	{
		get
		{
			if (m_CompanionCached != null)
			{
				return m_CompanionCached;
			}
			m_CompanionCached = ScenarioManager.Scenario.HeroSummons.SingleOrDefault((CHeroSummonActor s) => s.ActorGuid == m_CompanionGuid);
			return m_CompanionCached;
		}
	}

	public string PersonalQuestID { get; set; }

	public CPlayerActor()
	{
	}

	public CPlayerActor(CPlayerActor state, ReferenceDictionary references)
		: base(state, references)
	{
		SkipTopCardAction = state.SkipTopCardAction;
		SkipBottomCardAction = state.SkipBottomCardAction;
		IsLongRestSelected = state.IsLongRestSelected;
		IsLongRestActionSelected = state.IsLongRestActionSelected;
		OverrideCharacterModel = state.OverrideCharacterModel;
		SelectingCardsForExtraTurnOfType = state.SelectingCardsForExtraTurnOfType;
		m_CharacterName = state.m_CharacterName;
		m_CompanionGuid = state.m_CompanionGuid;
		m_characterHealthTable = references.Get(state.m_characterHealthTable);
		if (m_characterHealthTable == null && state.m_characterHealthTable != null)
		{
			m_characterHealthTable = new List<int>();
			for (int i = 0; i < state.m_characterHealthTable.Count; i++)
			{
				int item = state.m_characterHealthTable[i];
				m_characterHealthTable.Add(item);
			}
			references.Add(state.m_characterHealthTable, m_characterHealthTable);
		}
		PersonalQuestID = state.PersonalQuestID;
	}

	public CPlayerActor(Point startPosition, CCharacterClass characterClass, int currentHealth, int maxHealth, int level, string actorGuid, int chosenModelIndex, string characterName)
		: base(EType.Player, characterClass, startPosition, currentHealth, maxHealth, level, actorGuid, chosenModelIndex)
	{
		m_CharacterName = characterName;
		if (actorGuid == null && CharacterClass.InitialState)
		{
			CharacterClass.InitialState = false;
			CharacterClass.ResetAttackModifierDeck();
		}
		DLLDebug.Log("Created a new CPlayerActor with ModelInstanceID: " + characterClass.ModelInstanceID);
	}

	public override int Initiative()
	{
		int num = 0;
		if (CharacterClass.LongRest)
		{
			num = CharacterClass.InitiativeBonus;
			num += 99;
		}
		else if (SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater && CharacterClass.ExtraTurnInitiativeAbilityCard != null)
		{
			num += CharacterClass.ExtraTurnInitiativeAbilityCard.Initiative;
		}
		else if (base.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
		{
			if ((CharacterClass.ExtraTurnCardsSelectedInCardSelection.Count > 0 && CharacterClass.ExtraTurnCardsSelectedInCardSelection.Contains(CharacterClass.ExtraTurnInitiativeAbilityCard)) || (CharacterClass.NextPendingExtraTurnCards.Count > 0 && CharacterClass.NextPendingExtraTurnCards.Contains(CharacterClass.ExtraTurnInitiativeAbilityCard)))
			{
				num += CharacterClass.ExtraTurnInitiativeAbilityCard.Initiative;
			}
		}
		else
		{
			num = CharacterClass.InitiativeBonus;
			if (CharacterClass.RoundAbilityCards.Count > 0 && CharacterClass.RoundAbilityCards.Contains(CharacterClass.InitiativeAbilityCard))
			{
				num += CharacterClass.InitiativeAbilityCard.Initiative;
			}
			else if (base.HasPendingExtraTurn)
			{
				num += CharacterClass.ExtraTurnInitiativeAbilityCard.Initiative;
			}
			else if (CharacterClass.InitiativeAbilityCard != null)
			{
				num += CharacterClass.InitiativeAbilityCard.Initiative;
			}
		}
		if (CharacterClass.InitiativeBonus != 0)
		{
			return Math.Max(1, num);
		}
		return num;
	}

	public override int SubInitiative()
	{
		int num = 0;
		if (CharacterClass.LongRest)
		{
			num += 99;
		}
		else if (base.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater || SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
		{
			if (CharacterClass.ExtraTurnCardsSelectedInCardSelection.Count > 0 && CharacterClass.ExtraTurnCardsSelectedInCardSelection.Contains(CharacterClass.ExtraTurnInitiativeAbilityCard))
			{
				num += CharacterClass.ExtraTurnCardsSelectedInCardSelection.First((CAbilityCard f) => f != CharacterClass.ExtraTurnInitiativeAbilityCard).Initiative;
			}
			else if (CharacterClass.NextPendingExtraTurnCards.Count > 0 && CharacterClass.NextPendingExtraTurnCards.Contains(CharacterClass.ExtraTurnInitiativeAbilityCard))
			{
				num += CharacterClass.NextPendingExtraTurnCards.First((CAbilityCard f) => f != CharacterClass.ExtraTurnInitiativeAbilityCard).Initiative;
			}
		}
		else if (CharacterClass.RoundAbilityCards.Count > 1 && CharacterClass.RoundAbilityCards.Contains(CharacterClass.InitiativeAbilityCard))
		{
			num += CharacterClass.RoundAbilityCards.First((CAbilityCard f) => f != CharacterClass.InitiativeAbilityCard).Initiative;
		}
		else if (CharacterClass.NextPendingExtraTurnCards.Count > 0 && CharacterClass.NextPendingExtraTurnCards.Contains(CharacterClass.ExtraTurnInitiativeAbilityCard))
		{
			num += CharacterClass.NextPendingExtraTurnCards.First((CAbilityCard f) => f != CharacterClass.ExtraTurnInitiativeAbilityCard).Initiative;
		}
		else if (CharacterClass.SubInitiativeAbilityCard != null)
		{
			num += CharacterClass.SubInitiativeAbilityCard.Initiative;
		}
		return num;
	}

	public int OriginalInitiative()
	{
		int num = CharacterClass.InitiativeBonus;
		if (CharacterClass.LongRest)
		{
			num += 99;
		}
		else if (CharacterClass.RoundAbilityCards.Count > 0 && CharacterClass.RoundAbilityCards.Contains(CharacterClass.InitiativeAbilityCard))
		{
			num += CharacterClass.InitiativeAbilityCard.Initiative;
		}
		else if (SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater && CharacterClass.RoundCardsSelectedInCardSelection.Contains(CharacterClass.InitiativeAbilityCard))
		{
			num += CharacterClass.InitiativeAbilityCard.Initiative;
		}
		if (CharacterClass.InitiativeBonus != 0)
		{
			return Math.Max(1, num);
		}
		return num;
	}

	public override string ActorLocKey()
	{
		return CharacterClass.LocKey;
	}

	public override string GetPrefabName()
	{
		if (string.IsNullOrEmpty(OverrideCharacterModel))
		{
			return CharacterClass.CharacterModel.ToString();
		}
		return OverrideCharacterModel;
	}

	public override void Move(int maxMoveCount, bool jump, bool fly, int range, bool allowMove, bool ignoreDifficultTerrain = false, CAbilityAttack attack = null, bool firstMove = false, bool moveTest = false, bool carryOtherActors = false)
	{
		CActorStatic.AIMove(this, maxMoveCount, jump, fly, ignoreDifficultTerrain, base.Type == EType.Player || base.Type == EType.Ally, range, allowMove, attack, firstMove, moveTest, carryOtherActors);
	}

	public static List<Point> FindPlayerPath(CActor actorFindingPath, Point startLocation, Point endLocation, bool ignoreBlocked, bool ignoreMoveCost, out bool foundPath, bool avoidHazards = false, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false, bool carryOtherActors = false, EType carryType = EType.Unknown, bool logFailure = false)
	{
		List<Point> list = new List<Point>();
		if (ignoreBlocked && actorFindingPath.IgnoreActorCollision)
		{
			List<CObjectObstacle> list2;
			lock (ScenarioManager.CurrentScenarioState.Props)
			{
				list2 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectObstacle>().ToList();
			}
			foreach (CObjectObstacle item in list2)
			{
				foreach (TileIndex pathingBlocker in item.PathingBlockers)
				{
					list.Add(new Point(pathingBlocker));
				}
			}
			foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
			{
				list.Add(@object.ArrayIndex);
			}
		}
		if (!ignoreBlocked && (!actorFindingPath.IgnoreActorCollision || (actorFindingPath.IgnoreActorCollision && carryOtherActors && carryType != EType.Unknown)))
		{
			if (!CActor.AreActorsAllied(carryType, EType.Enemy))
			{
				foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
				{
					if (enemy.BlocksPathing)
					{
						list.Add(enemy.ArrayIndex);
					}
				}
			}
			if (!CActor.AreActorsAllied(carryType, EType.Enemy2))
			{
				foreach (CEnemyActor enemy2Monster in ScenarioManager.Scenario.Enemy2Monsters)
				{
					if (enemy2Monster.BlocksPathing)
					{
						list.Add(enemy2Monster.ArrayIndex);
					}
				}
			}
			if (!CActor.AreActorsAllied(carryType, EType.Neutral))
			{
				foreach (CEnemyActor neutralMonster in ScenarioManager.Scenario.NeutralMonsters)
				{
					if (neutralMonster.BlocksPathing)
					{
						list.Add(neutralMonster.ArrayIndex);
					}
				}
			}
			foreach (CObjectActor object2 in ScenarioManager.Scenario.Objects)
			{
				if (object2.BlocksPathing)
				{
					list.Add(object2.ArrayIndex);
				}
			}
			if (avoidHazards)
			{
				foreach (CObjectTrap item2 in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectTrap>())
				{
					if (item2.ArrayIndex.X != endLocation.X || item2.ArrayIndex.Y != endLocation.Y)
					{
						list.Add(new Point(item2.ArrayIndex));
					}
				}
				if (!ignoreHazardousTerrain)
				{
					foreach (CObjectHazardousTerrain item3 in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectHazardousTerrain>())
					{
						if (item3.ArrayIndex.X != endLocation.X || item3.ArrayIndex.Y != endLocation.Y)
						{
							list.Add(new Point(item3.ArrayIndex));
						}
					}
					ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == actorFindingPath.ActorGuid);
					foreach (CObjectDifficultTerrain item4 in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDifficultTerrain>())
					{
						if (item4.TreatAsTrap && item4.TreatAsTrapFilter.IsValidTarget(actorState) && (item4.ArrayIndex.X != endLocation.X || item4.ArrayIndex.Y != endLocation.Y))
						{
							list.Add(new Point(item4.ArrayIndex));
						}
					}
				}
			}
		}
		List<CObjectObstacle> list3;
		lock (ScenarioManager.CurrentScenarioState.Props)
		{
			list3 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectObstacle>().ToList();
		}
		List<Point> list4 = new List<Point>();
		foreach (CObjectObstacle item5 in list3)
		{
			foreach (TileIndex pathingBlocker2 in item5.PathingBlockers)
			{
				if (item5.IgnoresFlyAndJump)
				{
					list4.Add(new Point(pathingBlocker2));
				}
			}
		}
		ScenarioManager.PathFinder.QueuedTransientSuperBlockedLists.Add(list4);
		ScenarioManager.PathFinder.QueuedTransientBlockedLists.Add(list);
		return ScenarioManager.PathFinder.FindPath(startLocation, endLocation, ignoreBlocked, ignoreMoveCost, out foundPath, ignoreBridges: false, ignoreDifficultTerrain, logFailure);
	}

	public new CPlayerActor Clone()
	{
		CPlayerActor cPlayerActor = (CPlayerActor)MemberwiseClone();
		cPlayerActor.Inventory.InventoryActor = cPlayerActor;
		return cPlayerActor;
	}

	public void SetXP(int amount)
	{
		m_XP = amount;
	}

	public void SetPlayerLevel(int level)
	{
		m_Level = Math.Max(1, Math.Min(9, level));
	}

	public override void ActionSelection()
	{
		base.ActionSelection();
		GameState.InternalCurrentActor.Inventory.HighlightUsableItems(null, CItem.EItemTrigger.DuringOwnTurn);
	}

	public int GetMaxHealthForCharacterLevel()
	{
		return CharacterClass.HealthTable[Level];
	}

	public void SetCompanionGuid(string companionGuid)
	{
		m_CompanionGuid = companionGuid;
	}

	public void LoadPlayer(PlayerState playerState)
	{
		m_CharacterName = playerState.CharacterName;
		m_ArrayIndex = new Point(playerState.Location.X, playerState.Location.Y);
		m_Health = playerState.Health;
		base.MaxHealth = playerState.MaxHealth;
		m_Level = playerState.Level;
		m_Tokens = new CTokens(this, playerState.PositiveConditions, playerState.NegativeConditions);
		m_CarriedQuestProps = new List<CObjectProp>(playerState.CarriedQuestProps);
		m_PlayedThisRound = playerState.PlayedThisRound;
		base.CauseOfDeath = playerState.CauseOfDeath;
		base.KilledByActorGuid = playerState.KilledByActorGuid;
		base.PhasedOut = playerState.PhasedOut;
		base.Deactivated = playerState.Deactivated;
		if (base.PhasedOut)
		{
			CActorIsTeleporting_MessageData message = new CActorIsTeleporting_MessageData(this)
			{
				m_ActorTeleporting = this,
				m_TeleportAbility = null
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		if (m_Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
		{
			CSleep_MessageData message2 = new CSleep_MessageData("", null)
			{
				m_ActorToSleep = this
			};
			ScenarioRuleClient.MessageHandler(message2);
		}
		m_Gold = playerState.ScenarioGold;
		m_XP = playerState.ScenarioXP;
		(base.Class as CCharacterClass).LongRest = playerState.IsLongResting;
		(base.Class as CCharacterClass).ImprovedShortRest = playerState.ImprovedShortRestState;
		m_CompanionGuid = playerState.CompanionSummonGuid;
		foreach (CItem item in playerState.Items.OrderBy((CItem it) => (int)it.YMLData.Slot))
		{
			CItem.EItemSlotState slotState = item.SlotState;
			base.Inventory.AddItem(item, overrideExisting: true);
			switch (slotState)
			{
			case CItem.EItemSlotState.Consumed:
			case CItem.EItemSlotState.Spent:
				base.Inventory.UseItem(item);
				break;
			case CItem.EItemSlotState.Active:
				item.SlotState = CItem.EItemSlotState.Selected;
				base.Inventory.HandleActiveItemTriggered(item, ignoreConsume: true);
				break;
			}
		}
		m_CharacterResources = new List<CCharacterResource>();
		foreach (KeyValuePair<string, int> characterResource in playerState.CharacterResources)
		{
			m_CharacterResources.Add(new CCharacterResource(characterResource.Key, characterResource.Value));
		}
	}
}
