using System;
using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class CAbilityRequirements
{
	public enum EStartAbilityRequirementType
	{
		None,
		SubAbility,
		PreviousAbility,
		ThisAbility
	}

	public static EStartAbilityRequirementType[] StartAbilityRequirementTypes = (EStartAbilityRequirementType[])Enum.GetValues(typeof(EStartAbilityRequirementType));

	public EStartAbilityRequirementType StartAbilityRequirementType { get; set; }

	public int XP { get; set; }

	public int RequirementRange { get; set; }

	public int RoundIsInterval { get; set; }

	public int ActiveBonusInternalRoundInterval { get; set; }

	public CAbilityFilterContainer RequirementActorFilter { get; set; }

	public bool? AbilityMovementWasStraightLine { get; set; }

	public bool? OccupyingObstacleHex { get; set; }

	public bool? EnterHazardousTerrain { get; set; }

	public bool? EnterDifficultTerrain { get; set; }

	public bool? ElementsConsumedThisAction { get; set; }

	public bool? OnYourTurn { get; set; }

	public int? ActiveBonusAtTrackerIndex { get; set; }

	public bool? AbilityHasHappened { get; set; }

	public CEqualityFilter HP { get; set; }

	public CEqualityFilter HPPercentOfMax { get; set; }

	public CEqualityFilter HexesMovedThisTurn { get; set; }

	public CEqualityFilter ActorsTargetedByAbility { get; set; }

	public CEqualityFilter ActorsKilledByAbility { get; set; }

	public CEqualityFilter ActorsKilledThisTurn { get; set; }

	public CEqualityFilter ActorsKilledThisRound { get; set; }

	public CEqualityFilter ValidActorsInRangeOfAddedAbilityRange { get; set; }

	public CEqualityFilter ValidActorsInRangeOfRequirementRange { get; set; }

	public CEqualityFilter ActiveSpawnersInRange { get; set; }

	public CEqualityFilter InactiveSpawnersInRange { get; set; }

	public CEqualityFilter LootInRange { get; set; }

	public CEqualityFilter DoorsOpenedByAbility { get; set; }

	public CEqualityFilter WallsAdjacent { get; set; }

	public CEqualityFilter ActorHasPerformedValidAbilityTypesThisTurn { get; set; }

	public CEqualityFilter ActorHasPerformedValidAbilityTypesThisAction { get; set; }

	public List<ScenarioManager.ObjectImportType> ObjectImportTypes { get; set; }

	public List<ElementInfusionBoardManager.EElement> InertElements { get; set; }

	public List<ElementInfusionBoardManager.EElement> WaningElements { get; set; }

	public List<ElementInfusionBoardManager.EElement> StrongElements { get; set; }

	public List<CAbility.EAbilityType> AbilityTypes { get; set; }

	public bool? ForgoTopAction { get; set; }

	public bool? ForgoBottomAction { get; set; }

	public CAbilityRequirements()
	{
		RequirementActorFilter = CAbilityFilterContainer.CreateDefaultFilter();
	}

	public CAbilityRequirements Copy()
	{
		return new CAbilityRequirements
		{
			StartAbilityRequirementType = StartAbilityRequirementType,
			XP = XP,
			RequirementRange = RequirementRange,
			RoundIsInterval = RoundIsInterval,
			ActiveBonusInternalRoundInterval = ActiveBonusInternalRoundInterval,
			RequirementActorFilter = RequirementActorFilter.Copy(),
			AbilityMovementWasStraightLine = AbilityMovementWasStraightLine,
			OccupyingObstacleHex = OccupyingObstacleHex,
			EnterHazardousTerrain = EnterHazardousTerrain,
			EnterDifficultTerrain = EnterDifficultTerrain,
			ElementsConsumedThisAction = ElementsConsumedThisAction,
			OnYourTurn = OnYourTurn,
			ActiveBonusAtTrackerIndex = ActiveBonusAtTrackerIndex,
			AbilityHasHappened = AbilityHasHappened,
			HP = HP,
			HPPercentOfMax = HPPercentOfMax,
			HexesMovedThisTurn = HexesMovedThisTurn,
			ActorsTargetedByAbility = ActorsTargetedByAbility,
			ActorsKilledByAbility = ActorsKilledByAbility,
			ActorsKilledThisTurn = ActorsKilledThisTurn,
			ActorsKilledThisRound = ActorsKilledThisRound,
			ValidActorsInRangeOfAddedAbilityRange = ValidActorsInRangeOfAddedAbilityRange,
			ValidActorsInRangeOfRequirementRange = ValidActorsInRangeOfRequirementRange,
			ActiveSpawnersInRange = ActiveSpawnersInRange,
			InactiveSpawnersInRange = InactiveSpawnersInRange,
			LootInRange = LootInRange,
			DoorsOpenedByAbility = DoorsOpenedByAbility,
			WallsAdjacent = WallsAdjacent,
			ActorHasPerformedValidAbilityTypesThisTurn = ActorHasPerformedValidAbilityTypesThisTurn,
			ActorHasPerformedValidAbilityTypesThisAction = ActorHasPerformedValidAbilityTypesThisAction,
			ObjectImportTypes = ObjectImportTypes?.ToList(),
			InertElements = InertElements?.ToList(),
			WaningElements = WaningElements?.ToList(),
			StrongElements = StrongElements?.ToList(),
			AbilityTypes = AbilityTypes?.ToList(),
			ForgoTopAction = ForgoTopAction,
			ForgoBottomAction = ForgoBottomAction
		};
	}

	public bool MeetsAbilityRequirements(CActor actor, CAbility ability)
	{
		if (RoundIsInterval != 0 && ScenarioManager.CurrentScenarioState.RoundNumber % RoundIsInterval != 0)
		{
			return false;
		}
		if (AbilityMovementWasStraightLine.HasValue && AbilityMovementWasStraightLine.Value && ability != null && ability is CAbilityMove cAbilityMove)
		{
			List<Point> list = new List<Point>();
			list.Add(cAbilityMove.StartingPoint);
			list.AddRange(cAbilityMove.AllArrayIndexOnPathIncludingRepeatsCopy);
			if (list.Count > 2)
			{
				ScenarioManager.EAdjacentPosition adjacentPosition = ScenarioManager.GetAdjacentPosition(list[0].X, list[0].Y, list[1].X, list[1].Y);
				for (int i = 1; i < list.Count - 1; i++)
				{
					ScenarioManager.EAdjacentPosition adjacentPosition2 = ScenarioManager.GetAdjacentPosition(list[i].X, list[i].Y, list[i + 1].X, list[i + 1].Y);
					if (adjacentPosition != adjacentPosition2)
					{
						return false;
					}
				}
			}
			else if (cAbilityMove.AllArrayIndexOnPathIncludingRepeatsCopy.Count <= 0)
			{
				return false;
			}
		}
		if (actor != null && !MeetsAbilityRequirements(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]))
		{
			return false;
		}
		if (ElementsConsumedThisAction.HasValue && ElementsConsumedThisAction.Value && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.Action && !((CPhaseAction)PhaseManager.CurrentPhase).HasConsumedElements)
		{
			return false;
		}
		if (actor != null && OnYourTurn.HasValue && OnYourTurn.Value)
		{
			if (PhaseManager.CurrentPhase.Type < CPhase.PhaseType.StartTurn || PhaseManager.CurrentPhase.Type > CPhase.PhaseType.EndTurn)
			{
				return false;
			}
			if (GameState.TurnActor != actor)
			{
				return false;
			}
		}
		if (AbilityHasHappened.HasValue && ability != null && AbilityHasHappened.Value != ability.AbilityHasHappened)
		{
			return false;
		}
		if (ForgoTopAction.HasValue && ForgoTopAction.Value && actor != null && actor is CPlayerActor cPlayerActor && actor == GameState.InternalCurrentActor && (GameState.HasPlayedTopAction || cPlayerActor.CharacterClass.LongRest || cPlayerActor.CharacterClass.HasLongRested))
		{
			return false;
		}
		if (ForgoBottomAction.HasValue && ForgoBottomAction.Value && actor != null && actor is CPlayerActor cPlayerActor2 && actor == GameState.InternalCurrentActor && (GameState.HasPlayedBottomAction || cPlayerActor2.CharacterClass.LongRest || cPlayerActor2.CharacterClass.HasLongRested))
		{
			return false;
		}
		if (HP != null && !HP.Compare(actor.Health))
		{
			return false;
		}
		if (HPPercentOfMax != null && !HPPercentOfMax.Compare(actor.Health, actor.OriginalMaxHealth))
		{
			return false;
		}
		if (HexesMovedThisTurn != null && !HexesMovedThisTurn.Compare(GameState.HexesMovedThisTurn.Count))
		{
			return false;
		}
		if (ability != null && ActorsTargetedByAbility != null && !ActorsTargetedByAbility.Compare(ability.ActorsTargeted.Count((CActor x) => RequirementActorFilter.IsValidTarget(x, actor, isTargetedAbility: false, useTargetOriginalType: false, false))))
		{
			return false;
		}
		if (ability != null && ActorsKilledByAbility != null && !ActorsKilledByAbility.Compare(ability.ActorsTargeted.Count((CActor x) => x.IsDead && RequirementActorFilter.IsValidTarget(x, actor, isTargetedAbility: false, useTargetOriginalType: false, false))))
		{
			return false;
		}
		if (ActorsKilledThisTurn != null && !ActorsKilledThisTurn.Compare(GameState.ActorsKilledThisTurn.Count((CActor x) => x.KilledByActor == actor && RequirementActorFilter.IsValidTarget(x, actor, isTargetedAbility: false, useTargetOriginalType: false, false))))
		{
			return false;
		}
		if (ActorsKilledThisRound != null && !ActorsKilledThisRound.Compare(GameState.ActorsKilledThisRound.Count((CActor x) => x.KilledByActor == actor && RequirementActorFilter.IsValidTarget(x, actor, isTargetedAbility: false, useTargetOriginalType: false, false))))
		{
			return false;
		}
		if (ValidActorsInRangeOfAddedAbilityRange != null && ability != null && actor != null)
		{
			List<CActor> actorsInRange = GameState.GetActorsInRange(actor.ArrayIndex, actor, ability.Range, null, RequirementActorFilter, null, null, isTargetedAbility: false, null, false);
			if (!ValidActorsInRangeOfAddedAbilityRange.Compare(actorsInRange.Count))
			{
				return false;
			}
		}
		if (ValidActorsInRangeOfRequirementRange != null)
		{
			if (actor == null)
			{
				return false;
			}
			List<CActor> actorsInRange2 = GameState.GetActorsInRange(actor, actor, RequirementRange, null, RequirementActorFilter, null, null, isTargetedAbility: false, null, false);
			if (!ValidActorsInRangeOfRequirementRange.Compare(actorsInRange2.Count))
			{
				return false;
			}
		}
		if (ActiveSpawnersInRange != null && ability != null && actor != null)
		{
			List<CTile> tilesInRange = GameState.GetTilesInRange(actor, ability.Range, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			CTile item = ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y];
			if (!tilesInRange.Contains(item))
			{
				tilesInRange.Add(item);
			}
			tilesInRange = tilesInRange.Where((CTile x) => x.m_Spawners.Any((CSpawner y) => y is CInteractableSpawner && y.IsActive)).ToList();
			if (!ActiveSpawnersInRange.Compare(tilesInRange.Count))
			{
				return false;
			}
		}
		if (InactiveSpawnersInRange != null && ability != null && actor != null)
		{
			List<CTile> tilesInRange2 = GameState.GetTilesInRange(actor, ability.Range, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			CTile item2 = ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y];
			if (!tilesInRange2.Contains(item2))
			{
				tilesInRange2.Add(item2);
			}
			tilesInRange2 = tilesInRange2.Where((CTile x) => x.m_Spawners.Any((CSpawner y) => y is CInteractableSpawner && !y.IsActive)).ToList();
			if (!InactiveSpawnersInRange.Compare(tilesInRange2.Count))
			{
				return false;
			}
		}
		if (LootInRange != null && ability != null && actor != null)
		{
			List<CTile> tilesInRange3 = GameState.GetTilesInRange(actor, ability.Range, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			CTile item3 = ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y];
			if (!tilesInRange3.Contains(item3))
			{
				tilesInRange3.Add(item3);
			}
			List<CObjectProp> list2 = new List<CObjectProp>();
			foreach (CTile item4 in tilesInRange3)
			{
				list2.AddRange(item4.m_Props.FindAll((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.Chest || x.ObjectType == ScenarioManager.ObjectImportType.GoalChest || x.ObjectType == ScenarioManager.ObjectImportType.MoneyToken || x.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem || x.ObjectType == ScenarioManager.ObjectImportType.Resource));
			}
			if (ObjectImportTypes != null)
			{
				list2.RemoveAll((CObjectProp x) => !ObjectImportTypes.Contains(x.ObjectType));
			}
			if (!LootInRange.Compare(list2.Count))
			{
				return false;
			}
		}
		if (DoorsOpenedByAbility != null)
		{
			if (ability == null || !(ability is CAbilityMove cAbilityMove2))
			{
				return false;
			}
			if (!DoorsOpenedByAbility.Compare(cAbilityMove2.DoorsOpened))
			{
				return false;
			}
		}
		if (WallsAdjacent != null && actor != null)
		{
			CTile cTile = ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y];
			List<CTile> list3 = new List<CTile>();
			for (int num = 1; num < 7; num++)
			{
				ScenarioManager.EAdjacentPosition eposition = (ScenarioManager.EAdjacentPosition)num;
				CTile adjacentTile = ScenarioManager.GetAdjacentTile(actor.ArrayIndex.X, actor.ArrayIndex.Y, eposition);
				if (adjacentTile != null)
				{
					CNode cNode = ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
					if ((!cNode.Walkable && !cNode.IsBridge) || !cTile.IsMapShared(adjacentTile))
					{
						list3.Add(adjacentTile);
					}
				}
			}
			if (!WallsAdjacent.Compare(list3.Count))
			{
				return false;
			}
		}
		if (actor != null && ActorHasPerformedValidAbilityTypesThisTurn != null && AbilityTypes != null)
		{
			List<CAbility.EAbilityType> list4 = actor.AbilityTypesPerformedThisTurn.Intersect(AbilityTypes).ToList();
			if (!ActorHasPerformedValidAbilityTypesThisTurn.Compare(list4.Count))
			{
				return false;
			}
		}
		if (actor != null && ActorHasPerformedValidAbilityTypesThisAction != null && AbilityTypes != null)
		{
			List<CAbility.EAbilityType> list5 = actor.AbilityTypesPerformedThisAction.Intersect(AbilityTypes).ToList();
			if (!ActorHasPerformedValidAbilityTypesThisAction.Compare(list5.Count))
			{
				return false;
			}
		}
		if (InertElements != null)
		{
			foreach (ElementInfusionBoardManager.EElement inertElement in InertElements)
			{
				if (ElementInfusionBoardManager.ElementColumn(inertElement) != ElementInfusionBoardManager.EColumn.Inert)
				{
					return false;
				}
			}
		}
		if (WaningElements != null)
		{
			foreach (ElementInfusionBoardManager.EElement waningElement in WaningElements)
			{
				if (ElementInfusionBoardManager.ElementColumn(waningElement) != ElementInfusionBoardManager.EColumn.Waning)
				{
					return false;
				}
			}
		}
		if (StrongElements != null)
		{
			foreach (ElementInfusionBoardManager.EElement strongElement in StrongElements)
			{
				if (ElementInfusionBoardManager.ElementColumn(strongElement) != ElementInfusionBoardManager.EColumn.Strong)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool MeetsAbilityRequirements(CTile tile)
	{
		if (OccupyingObstacleHex.HasValue && OccupyingObstacleHex.Value)
		{
			CObjectObstacle cObjectObstacle = tile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
			if (cObjectObstacle == null)
			{
				cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(tile.m_ArrayIndex, ref tile);
			}
			if (cObjectObstacle == null)
			{
				CNode cNode = ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y];
				if (cNode != null && cNode.Blocked)
				{
					List<CTile> allAdjacentTiles = ScenarioManager.GetAllAdjacentTiles(tile);
					for (int i = 0; i < allAdjacentTiles.Count; i++)
					{
						if (allAdjacentTiles[i].m_Props.Count > 0 && allAdjacentTiles[i].FindProp(ScenarioManager.ObjectImportType.Obstacle) != null)
						{
							return true;
						}
					}
				}
				return false;
			}
		}
		return true;
	}

	public bool MeetsActiveBonusRequirements(CActiveBonus bonus)
	{
		if (ActiveBonusInternalRoundInterval != 0 && (bonus.InternalRoundNumber - 1) % ActiveBonusInternalRoundInterval != 0)
		{
			return false;
		}
		if (ActiveBonusAtTrackerIndex.HasValue && bonus.HasTracker && bonus.TrackerIndex != ActiveBonusAtTrackerIndex.Value)
		{
			return false;
		}
		return true;
	}

	public void PayRequirementCost()
	{
		if (ForgoTopAction.HasValue && ForgoTopAction.Value)
		{
			GameState.SetActionFlag(GameState.EActionSelectionFlag.TopActionPlayed);
			CForgoActionForActiveBonus_MessageData message = new CForgoActionForActiveBonus_MessageData();
			ScenarioRuleClient.MessageHandler(message);
		}
		if (ForgoBottomAction.HasValue && ForgoBottomAction.Value)
		{
			GameState.SetActionFlag(GameState.EActionSelectionFlag.BottomActionPlayed);
			CForgoActionForActiveBonus_MessageData message2 = new CForgoActionForActiveBonus_MessageData();
			ScenarioRuleClient.MessageHandler(message2);
		}
	}
}
