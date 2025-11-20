using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityLoot : CAbility
{
	public enum EAdditionalLootEffect
	{
		None,
		ExtraGoldPerAdjacentEnemy
	}

	public enum ELootState
	{
		None,
		PreLootBuffTargeting,
		ApplyLootBuff,
		CheckForDelayedDrops,
		CheckToAllowTileSelection,
		SelectLootTiles,
		LootTiles,
		CheckForLootQuestItems,
		LootQuestItems,
		LootDone
	}

	public class LootData
	{
		public List<ScenarioManager.ObjectImportType> LootableObjectImportTypes;

		public EAdditionalLootEffect AdditionalLootEffect;

		public LootData()
		{
			LootableObjectImportTypes = new List<ScenarioManager.ObjectImportType>();
			AdditionalLootEffect = EAdditionalLootEffect.None;
		}

		public LootData Copy()
		{
			return new LootData
			{
				LootableObjectImportTypes = LootableObjectImportTypes.ToList(),
				AdditionalLootEffect = AdditionalLootEffect
			};
		}

		public static LootData CreateDefaultLootData()
		{
			return new LootData
			{
				LootableObjectImportTypes = new List<ScenarioManager.ObjectImportType>
				{
					ScenarioManager.ObjectImportType.MoneyToken,
					ScenarioManager.ObjectImportType.Chest,
					ScenarioManager.ObjectImportType.GoalChest,
					ScenarioManager.ObjectImportType.CarryableQuestItem,
					ScenarioManager.ObjectImportType.Resource
				}
			};
		}
	}

	public static EAdditionalLootEffect[] AdditionalLootEffects = (EAdditionalLootEffect[])Enum.GetValues(typeof(EAdditionalLootEffect));

	private ELootState m_State;

	private List<CObjectProp> m_PropsLooted;

	private int m_ExtraGoldFromAdditionalEffects;

	private List<CLootActiveBonus_LimitLoot> m_LimitLootActiveBonuses = new List<CLootActiveBonus_LimitLoot>();

	public LootData AbilityLootData { get; set; }

	public CAbilityLoot(LootData lootData)
	{
		AbilityLootData = lootData;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = ELootState.CheckForDelayedDrops;
		m_ExtraGoldFromAdditionalEffects = 0;
		if (base.ActiveBonusData != null && base.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
		{
			m_State = ELootState.PreLootBuffTargeting;
			m_ActorsToTarget.Clear();
			m_ActorsToTarget.Add(base.TargetingActor);
			m_ValidActorsInRange = new List<CActor>();
			m_ValidActorsInRange.Add(base.TargetingActor);
		}
		else if (base.AllTargetsOnMovePath)
		{
			for (int i = 0; i < CAbilityMove.AllArrayIndexOnPath.Count; i++)
			{
				if (base.MiscAbilityData?.MovePathIndexFilter == null || base.MiscAbilityData.MovePathIndexFilter.Compare(i))
				{
					Point point = CAbilityMove.AllArrayIndexOnPath[i];
					CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
					if (cTile != null)
					{
						base.TilesInRange.Add(cTile);
					}
				}
			}
		}
		else
		{
			GetBonuses();
			if (base.AreaEffect == null)
			{
				base.TilesInRange = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			}
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter);
			if (!base.TilesInRange.Contains(adjacentTile))
			{
				base.TilesInRange.Add(adjacentTile);
			}
		}
		if (m_NumberTargets == -1)
		{
			m_AllTargets = true;
			m_NumberTargets = base.TilesInRange.Count;
		}
		else
		{
			m_AllTargets = false;
		}
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_PropsLooted = new List<CObjectProp>();
		LogEvent(ESESubTypeAbility.AbilityStart);
		m_AbilityStartComplete = true;
	}

	public override bool Perform()
	{
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		LogEvent(ESESubTypeAbility.AbilityPerform);
		if (!base.ProcessIfDead)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0075;
			}
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0075;
				}
			}
		}
		if (m_CancelAbility)
		{
			PhaseManager.NextStep();
			return true;
		}
		switch (m_State)
		{
		case ELootState.PreLootBuffTargeting:
		{
			if (base.TargetingActor.Type != CActor.EType.Player || (base.MiscAbilityData.AutotriggerAbility.HasValue && base.MiscAbilityData.AutotriggerAbility.Value))
			{
				PhaseManager.StepComplete();
				break;
			}
			CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData2 = new CActorIsSelectingTargetingFocus_MessageData(base.TargetingActor);
			cActorIsSelectingTargetingFocus_MessageData2.m_TargetingAbility = this;
			cActorIsSelectingTargetingFocus_MessageData2.m_IsPositive = true;
			ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData2);
			break;
		}
		case ELootState.ApplyLootBuff:
			ScenarioRuleClient.FirstAbilityStarted();
			base.AbilityHasHappened = true;
			foreach (CActor item in m_ActorsToTarget)
			{
				ApplyToActor(item);
			}
			PhaseManager.NextStep();
			return true;
		case ELootState.CheckForDelayedDrops:
		{
			CWaitForDelayedDrops_MessageData message3 = new CWaitForDelayedDrops_MessageData();
			ScenarioRuleClient.MessageHandler(message3);
			return true;
		}
		case ELootState.CheckToAllowTileSelection:
		{
			bool flag = true;
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				base.TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
				List<CTile> list = new List<CTile>();
				int num = 0;
				foreach (CTile item2 in base.TilesInRange)
				{
					List<CObjectProp> list2 = item2.m_Props.FindAll((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.Chest || x.ObjectType == ScenarioManager.ObjectImportType.GoalChest || x.ObjectType == ScenarioManager.ObjectImportType.MoneyToken || x.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem || x.ObjectType == ScenarioManager.ObjectImportType.Resource);
					list2.RemoveAll((CObjectProp x) => !AbilityLootData.LootableObjectImportTypes.Contains(x.ObjectType) || !x.CanActorLoot(base.TargetingActor) || !m_LimitLootActiveBonuses.All((CLootActiveBonus_LimitLoot y) => y.CanLootObject(base.TargetingActor, x.ObjectType)));
					if (list2.Count > 0)
					{
						list.Add(item2);
						num += list2.Count;
						if (!m_AllTargets && num > m_NumberTargets)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag || m_AllTargets)
				{
					m_TilesSelected.AddRange(list);
				}
				if (list.Count <= 0 && base.TargetingActor.Type == CActor.EType.Player)
				{
					GetLootAlly();
					if (base.ValidActorsInRange.Count > 0)
					{
						m_State = ELootState.CheckForLootQuestItems;
					}
					else
					{
						m_State = ELootState.SelectLootTiles;
					}
				}
				else
				{
					m_State = ELootState.SelectLootTiles;
				}
			}
			else
			{
				m_TilesSelected = base.TilesInRange.ToList();
				m_State = ELootState.LootTiles;
			}
			Perform();
			return true;
		}
		case ELootState.SelectLootTiles:
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectingObjectPosition_MessageData message = new CPlayerSelectingObjectPosition_MessageData(base.TargetingActor)
				{
					m_SpawnType = ScenarioManager.ObjectImportType.None,
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.None },
					m_Ability = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			return true;
		case ELootState.LootTiles:
		{
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			int num2 = 0;
			int num3 = 0;
			base.AbilityHasHappened = true;
			CActor cActor = base.TargetingActor;
			if (base.TargetingActor is CHeroSummonActor cHeroSummonActor)
			{
				cActor = cHeroSummonActor.Summoner;
			}
			for (int num4 = 0; num4 < m_TilesSelected.Count; num4++)
			{
				if (num3 != 0)
				{
					break;
				}
				CTile cTile = m_TilesSelected[num4];
				List<CObjectProp> list3 = ((!(base.TargetingActor.Class is CCharacterClass) && !(base.TargetingActor is CHeroSummonActor { IsCompanionSummon: not false })) ? cTile.m_Props.FindAll((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.MoneyToken) : cTile.m_Props.FindAll((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.Chest || x.ObjectType == ScenarioManager.ObjectImportType.GoalChest || x.ObjectType == ScenarioManager.ObjectImportType.MoneyToken || x.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem || x.ObjectType == ScenarioManager.ObjectImportType.Resource));
				num2 += list3.Count;
				if (!m_AllTargets && num2 >= m_NumberTargets)
				{
					num3 = num4;
				}
				if (base.TargetingActor.LootTile(cTile))
				{
					m_PropsLooted.AddRange(list3);
				}
			}
			if (AbilityLootData.AdditionalLootEffect.Equals(EAdditionalLootEffect.ExtraGoldPerAdjacentEnemy))
			{
				m_ActorsToIgnore.Add(base.TargetingActor);
				List<CActor> actorsInRange = GameState.GetActorsInRange(base.TargetingActor.ArrayIndex, base.TargetingActor, 1, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible);
				int num5 = ((ScenarioManager.Scenario.SLTE == null) ? 1 : ScenarioManager.Scenario.SLTE.GoldConversion);
				m_ExtraGoldFromAdditionalEffects = num5 * actorsInRange.Count;
				cActor.AddGold(m_ExtraGoldFromAdditionalEffects);
			}
			if (ScenarioManager.Scenario.HasActor(base.TargetingActor))
			{
				CLooting_MessageData message2 = new CLooting_MessageData(base.AnimOverload, base.TargetingActor)
				{
					m_LootAbility = this,
					m_ActorLooting = base.TargetingActor,
					m_PropsLooted = m_PropsLooted,
					m_ExtraGoldFromAdditionalEffects = m_ExtraGoldFromAdditionalEffects
				};
				ScenarioRuleClient.MessageHandler(message2);
			}
			else
			{
				PhaseManager.StepComplete();
			}
			return true;
		}
		case ELootState.CheckForLootQuestItems:
			GetLootAlly();
			base.ActorsToTarget.AddRange(base.ValidActorsInRange);
			if (base.ActorsToTarget.Count > 0)
			{
				CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(base.TargetingActor);
				cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
				cActorIsSelectingTargetingFocus_MessageData.m_ObjectiveRelated = true;
				ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			}
			else
			{
				PhaseManager.NextStep();
			}
			return true;
		case ELootState.LootQuestItems:
			foreach (CActor item3 in base.ValidActorsInRange)
			{
				foreach (CObjectProp carriedQuestItem in item3.CarriedQuestItems)
				{
					base.TargetingActor.CarryQuestItem(carriedQuestItem);
					CActivateProp_MessageData cActivateProp_MessageData = new CActivateProp_MessageData(base.TargetingActor);
					cActivateProp_MessageData.m_Prop = carriedQuestItem;
					cActivateProp_MessageData.m_CarriedByActor = true;
					ScenarioRuleClient.MessageHandler(cActivateProp_MessageData);
				}
				item3.CarriedQuestItems.Clear();
			}
			PhaseManager.NextStep();
			return true;
		}
		return false;
		IL_0075:
		if (base.TargetingActor.Type == CActor.EType.Player)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message4 = new CPlayerIsSleeping_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message4);
			}
			else
			{
				CPlayerIsStunned_MessageData message5 = new CPlayerIsStunned_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message5);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public void GetLootAlly()
	{
		base.ValidActorsInRange.Clear();
		CAbilityFilterContainer cAbilityFilterContainer = CAbilityFilterContainer.CreateDefaultFilter();
		cAbilityFilterContainer.AbilityFilters[0].FilterTargetType = CAbilityFilter.EFilterTargetType.Ally;
		cAbilityFilterContainer.AbilityFilters[0].FilterActorType = CAbilityFilter.EFilterActorType.Player;
		cAbilityFilterContainer.AbilityFilters[0].FilterFlags = CAbilityFilter.EFilterFlags.CarryingQuestItem;
		base.ValidActorsInRange = GameState.GetActorsInRange(base.TargetingActor.ArrayIndex, base.TargetingActor, 1, null, cAbilityFilterContainer, null, null, isTargetedAbility: false, null, false);
		m_ActorsToTarget.Clear();
	}

	public void GetBonuses()
	{
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(base.TargetingActor, EAbilityType.Loot);
		if (list == null)
		{
			return;
		}
		foreach (CActiveBonus item2 in list.Where((CActiveBonus x) => x.BespokeBehaviour == null || !(x.BespokeBehaviour is CLootActiveBonus_LimitLoot)))
		{
			m_Range += item2.ReferenceStrength(this, base.TargetingActor);
		}
		foreach (CActiveBonus item3 in list.Where((CActiveBonus x) => x.BespokeBehaviour == null || !(x.BespokeBehaviour is CLootActiveBonus_LimitLoot)))
		{
			m_ModifiedStrength *= item3.ReferenceStrengthScalar(this, base.TargetingActor);
		}
		foreach (CActiveBonus item4 in list)
		{
			if (item4.BespokeBehaviour != null && item4.BespokeBehaviour is CLootActiveBonus_LimitLoot item)
			{
				m_LimitLootActiveBonuses.Add(item);
			}
		}
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (!base.TilesSelected.Contains(selectedTile) && m_State == ELootState.SelectLootTiles && m_NumberTargetsRemaining > 0 && base.TilesInRange.Contains(selectedTile))
		{
			List<CObjectProp> list = selectedTile.m_Props.FindAll((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.Chest || x.ObjectType == ScenarioManager.ObjectImportType.GoalChest || x.ObjectType == ScenarioManager.ObjectImportType.MoneyToken || x.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem || x.ObjectType == ScenarioManager.ObjectImportType.Resource);
			list.RemoveAll((CObjectProp x) => !AbilityLootData.LootableObjectImportTypes.Contains(x.ObjectType));
			if (list != null && list.Count > 0)
			{
				m_TilesSelected.Add(selectedTile);
				m_NumberTargetsRemaining--;
			}
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData message = new CPlayerSelectedTile_MessageData(base.TargetingActor)
				{
					m_Ability = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		if (false)
		{
			Perform();
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (!base.AllTargets && m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			m_NumberTargetsRemaining++;
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData message = new CPlayerSelectedTile_MessageData(base.TargetingActor)
				{
					m_Ability = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		if (false)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public bool HasPassedState(ELootState lootState)
	{
		return m_State > lootState;
	}

	public override bool CanClearTargets()
	{
		return m_State == ELootState.SelectLootTiles;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == ELootState.SelectLootTiles;
		}
		return false;
	}

	public override string GetDescription()
	{
		if (m_Range > 0)
		{
			return "Loot R: " + m_Range;
		}
		return "Loot";
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			CBaseCard cBaseCard = base.TargetingActor.FindCardWithAbility(this);
			if (base.ActiveBonusData.OverrideAsSong)
			{
				actor.AddAugmentOrSong(this, base.TargetingActor);
			}
			else if (cBaseCard != null)
			{
				cBaseCard.AddActiveBonus(this, actor, base.TargetingActor);
			}
			else
			{
				DLLDebug.LogError("Unable to find base ability card for ability " + base.Name);
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == ELootState.LootDone;
	}

	public override void Restart()
	{
		base.Restart();
		m_State = ELootState.CheckToAllowTileSelection;
		m_ExtraGoldFromAdditionalEffects = 0;
		if (base.AllTargetsOnMovePath)
		{
			for (int i = 0; i < CAbilityMove.AllArrayIndexOnPath.Count; i++)
			{
				if (base.MiscAbilityData?.MovePathIndexFilter == null || base.MiscAbilityData.MovePathIndexFilter.Compare(i))
				{
					Point point = CAbilityMove.AllArrayIndexOnPath[i];
					CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
					if (cTile != null)
					{
						base.TilesInRange.Add(cTile);
					}
				}
			}
		}
		else
		{
			GetBonuses();
			if (base.AreaEffect == null)
			{
				base.TilesInRange = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			}
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter);
			if (!base.TilesInRange.Contains(adjacentTile))
			{
				base.TilesInRange.Add(adjacentTile);
			}
		}
		if (m_AllTargets)
		{
			m_NumberTargets = base.TilesInRange.Count;
		}
		else
		{
			m_AllTargets = false;
		}
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_TilesSelected.Clear();
		m_PropsLooted = new List<CObjectProp>();
		LogEvent(ESESubTypeAbility.AbilityRestart);
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		bool isSummon = false;
		CActor targetingActor = base.TargetingActor;
		if (targetingActor != null && targetingActor.Type == CActor.EType.Enemy)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == base.TargetingActor.ActorGuid);
			if (cEnemyActor != null)
			{
				isSummon = cEnemyActor.IsSummon;
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityLoot(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, m_State, m_PropsLooted.Select((CObjectProp x) => x.PrefabName).ToList(), base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}

	public CAbilityLoot()
	{
	}

	public CAbilityLoot(CAbilityLoot state, ReferenceDictionary references)
		: base(state, references)
	{
		m_State = state.m_State;
		m_PropsLooted = references.Get(state.m_PropsLooted);
		if (m_PropsLooted == null && state.m_PropsLooted != null)
		{
			m_PropsLooted = new List<CObjectProp>();
			for (int i = 0; i < state.m_PropsLooted.Count; i++)
			{
				CObjectProp cObjectProp = state.m_PropsLooted[i];
				CObjectProp cObjectProp2 = references.Get(cObjectProp);
				if (cObjectProp2 == null && cObjectProp != null)
				{
					CObjectProp cObjectProp3 = ((cObjectProp is CObjectChest state2) ? new CObjectChest(state2, references) : ((cObjectProp is CObjectDifficultTerrain state3) ? new CObjectDifficultTerrain(state3, references) : ((cObjectProp is CObjectDoor state4) ? new CObjectDoor(state4, references) : ((cObjectProp is CObjectGoldPile state5) ? new CObjectGoldPile(state5, references) : ((cObjectProp is CObjectHazardousTerrain state6) ? new CObjectHazardousTerrain(state6, references) : ((cObjectProp is CObjectMonsterGrave state7) ? new CObjectMonsterGrave(state7, references) : ((cObjectProp is CObjectObstacle state8) ? new CObjectObstacle(state8, references) : ((cObjectProp is CObjectPortal state9) ? new CObjectPortal(state9, references) : ((cObjectProp is CObjectPressurePlate state10) ? new CObjectPressurePlate(state10, references) : ((cObjectProp is CObjectQuestItem state11) ? new CObjectQuestItem(state11, references) : ((cObjectProp is CObjectResource state12) ? new CObjectResource(state12, references) : ((cObjectProp is CObjectTerrainVisual state13) ? new CObjectTerrainVisual(state13, references) : ((!(cObjectProp is CObjectTrap state14)) ? new CObjectProp(cObjectProp, references) : new CObjectTrap(state14, references))))))))))))));
					cObjectProp2 = cObjectProp3;
					references.Add(cObjectProp, cObjectProp2);
				}
				m_PropsLooted.Add(cObjectProp2);
			}
			references.Add(state.m_PropsLooted, m_PropsLooted);
		}
		m_ExtraGoldFromAdditionalEffects = state.m_ExtraGoldFromAdditionalEffects;
		m_LimitLootActiveBonuses = references.Get(state.m_LimitLootActiveBonuses);
		if (m_LimitLootActiveBonuses != null || state.m_LimitLootActiveBonuses == null)
		{
			return;
		}
		m_LimitLootActiveBonuses = new List<CLootActiveBonus_LimitLoot>();
		for (int j = 0; j < state.m_LimitLootActiveBonuses.Count; j++)
		{
			CLootActiveBonus_LimitLoot cLootActiveBonus_LimitLoot = state.m_LimitLootActiveBonuses[j];
			CLootActiveBonus_LimitLoot cLootActiveBonus_LimitLoot2 = references.Get(cLootActiveBonus_LimitLoot);
			if (cLootActiveBonus_LimitLoot2 == null && cLootActiveBonus_LimitLoot != null)
			{
				cLootActiveBonus_LimitLoot2 = new CLootActiveBonus_LimitLoot(cLootActiveBonus_LimitLoot, references);
				references.Add(cLootActiveBonus_LimitLoot, cLootActiveBonus_LimitLoot2);
			}
			m_LimitLootActiveBonuses.Add(cLootActiveBonus_LimitLoot2);
		}
		references.Add(state.m_LimitLootActiveBonuses, m_LimitLootActiveBonuses);
	}
}
