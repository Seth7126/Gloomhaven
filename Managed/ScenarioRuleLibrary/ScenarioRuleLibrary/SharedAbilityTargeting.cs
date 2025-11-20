using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;

namespace ScenarioRuleLibrary;

public class SharedAbilityTargeting
{
	public static bool TileSelected(CAbility ability, CTile selectedTile, CAreaEffect areaEffect, CActor targetActor, CActor filterActor, int range, ref List<CActor> validActorsInRange, List<CActor> actorsToIgnore, CAbilityFilterContainer abilityFilter, int maxNumberTargets, ref int numberTargetsRemaining, List<CTile> validTilesInAreaAffectedIncludingBlocked, ref List<CActor> actorsToTarget, ref List<CTile> selectedTiles, bool isTargetedAbility)
	{
		if (areaEffect != null)
		{
			if (!selectedTiles.Contains(selectedTile))
			{
				selectedTiles.Add(selectedTile);
			}
			if (ability.MiscAbilityData?.IgnorePreviousAbilityTargets != null)
			{
				AbilityData.MiscAbilityData miscAbilityData = ability.MiscAbilityData;
				if (miscAbilityData != null && miscAbilityData.IgnorePreviousAbilityTargets.Count > 0)
				{
					range = 99;
				}
			}
			List<CTile> tilesInRange = GameState.GetTilesInRange(targetActor, range, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			if (validTilesInAreaAffectedIncludingBlocked.Count > 0 && tilesInRange.Any((CTile x) => validTilesInAreaAffectedIncludingBlocked.Any((CTile y) => y == x)))
			{
				validActorsInRange = GameState.GetActorsInRange(targetActor, filterActor, range, actorsToIgnore, abilityFilter, areaEffect, validTilesInAreaAffectedIncludingBlocked, isTargetedAbility, null, ability.MiscAbilityData?.CanTargetInvisible);
				ability.RemoveImmuneActorsFromList(ref validActorsInRange);
				if (validActorsInRange.Count > 0)
				{
					targetActor.Inventory.HighlightUsableItems(ability, validActorsInRange, CItem.EItemTrigger.SingleTarget, CItem.EItemTrigger.EntireAction);
					CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData = new CShowSingleTargetActiveBonus_MessageData(ability.TargetingActor);
					cShowSingleTargetActiveBonus_MessageData.m_ShowSingleTargetActiveBonus = true;
					cShowSingleTargetActiveBonus_MessageData.m_Ability = ability;
					ScenarioRuleClient.MessageHandler(cShowSingleTargetActiveBonus_MessageData);
					return true;
				}
			}
			validActorsInRange.Clear();
			return false;
		}
		foreach (CActor item in validActorsInRange)
		{
			if (!(item.ArrayIndex == selectedTile.m_ArrayIndex))
			{
				continue;
			}
			bool flag = actorsToTarget.Contains(item);
			if (!flag)
			{
				AbilityData.MiscAbilityData miscAbilityData2 = ability.MiscAbilityData;
				if (miscAbilityData2 != null && miscAbilityData2.TargetOneEnemyWithAllAttacks.HasValue)
				{
					AbilityData.MiscAbilityData miscAbilityData3 = ability.MiscAbilityData;
					if (miscAbilityData3 != null && miscAbilityData3.TargetOneEnemyWithAllAttacks.Value)
					{
						selectedTiles.Add(selectedTile);
						actorsToTarget = new List<CActor>(maxNumberTargets);
						for (int num = 0; num < maxNumberTargets; num++)
						{
							actorsToTarget.Add(item);
						}
						goto IL_01fa;
					}
				}
				if (actorsToTarget.Count < maxNumberTargets)
				{
					selectedTiles.Add(selectedTile);
					actorsToTarget.Add(item);
				}
			}
			goto IL_01fa;
			IL_01fa:
			numberTargetsRemaining = maxNumberTargets - actorsToTarget.Count;
			if (actorsToTarget.Count > 0)
			{
				targetActor.Inventory.HighlightUsableItems(ability, actorsToTarget, CItem.EItemTrigger.SingleTarget, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
				CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData2 = new CShowSingleTargetActiveBonus_MessageData(ability.TargetingActor);
				cShowSingleTargetActiveBonus_MessageData2.m_ShowSingleTargetActiveBonus = true;
				cShowSingleTargetActiveBonus_MessageData2.m_Ability = ability;
				ScenarioRuleClient.MessageHandler(cShowSingleTargetActiveBonus_MessageData2);
			}
			CActorSelectedAttackFocus_MessageData message = new CActorSelectedAttackFocus_MessageData(targetActor)
			{
				m_AttackingActor = targetActor,
				m_Ability = ability,
				m_AttackFocus = item,
				m_Adding = !flag
			};
			ScenarioRuleClient.MessageHandler(message);
			if (actorsToTarget.Count == maxNumberTargets)
			{
				return true;
			}
		}
		return false;
	}

	public static bool TileDeselected(CAbility ability, CTile selectedTile, int maxNumberTargets, ref int numberTargetsRemaining, ref List<CActor> actorsToTarget, ref List<CTile> selectedTiles)
	{
		if (ability.AreaEffect != null || ability.AreaEffectBackup != null)
		{
			if (ability.MiscAbilityData != null && ability.MiscAbilityData.ExactRange.HasValue && ability.MiscAbilityData.ExactRange.Value)
			{
				return false;
			}
			if (selectedTiles.Contains(selectedTile))
			{
				selectedTiles.Remove(selectedTile);
			}
			actorsToTarget.Clear();
			for (int num = ability.ActiveSingleTargetItems.Count - 1; num >= 0; num--)
			{
				CItem cItem = ability.ActiveSingleTargetItems[num];
				if (cItem.SingleTarget != null)
				{
					ability.TargetingActor.Inventory.DeselectItem(cItem);
					ability.ActiveSingleTargetItems.Remove(cItem);
				}
			}
			for (int num2 = ability.ActiveSingleTargetActiveBonuses.Count - 1; num2 >= 0; num2--)
			{
				CActiveBonus cActiveBonus = ability.ActiveSingleTargetActiveBonuses[num2];
				if (cActiveBonus.SingleTarget != null)
				{
					cActiveBonus.ToggleActiveBonus(null, ability.TargetingActor);
				}
			}
			return true;
		}
		if (selectedTiles.Contains(selectedTile))
		{
			CActor actorOnTile = ScenarioManager.Scenario.FindActorAt(selectedTile.m_ArrayIndex);
			if (actorOnTile != null)
			{
				if (actorsToTarget.Contains(actorOnTile))
				{
					selectedTiles.Remove(selectedTile);
					actorsToTarget.RemoveAll((CActor x) => x == actorOnTile);
					numberTargetsRemaining++;
				}
				for (int num3 = ability.ActiveSingleTargetItems.Count - 1; num3 >= 0; num3--)
				{
					CItem cItem2 = ability.ActiveSingleTargetItems[num3];
					if (cItem2.SingleTarget != null && cItem2.SingleTarget == actorOnTile)
					{
						ability.TargetingActor.Inventory.DeselectItem(cItem2);
						ability.ActiveSingleTargetItems.Remove(cItem2);
					}
				}
				for (int num4 = ability.ActiveSingleTargetActiveBonuses.Count - 1; num4 >= 0; num4--)
				{
					CActiveBonus cActiveBonus2 = ability.ActiveSingleTargetActiveBonuses[num4];
					if (cActiveBonus2.SingleTarget != null && cActiveBonus2.SingleTarget == actorOnTile)
					{
						cActiveBonus2.ToggleActiveBonus(null, ability.TargetingActor);
					}
				}
				if (actorsToTarget.Count > 0)
				{
					ability.TargetingActor.Inventory.HighlightUsableItems(ability, actorsToTarget, CItem.EItemTrigger.SingleTarget, CItem.EItemTrigger.EntireAction);
					CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData = new CShowSingleTargetActiveBonus_MessageData(ability.TargetingActor);
					cShowSingleTargetActiveBonus_MessageData.m_ShowSingleTargetActiveBonus = true;
					cShowSingleTargetActiveBonus_MessageData.m_Ability = ability;
					ScenarioRuleClient.MessageHandler(cShowSingleTargetActiveBonus_MessageData);
				}
				else
				{
					CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData2 = new CShowSingleTargetActiveBonus_MessageData(ability.TargetingActor);
					cShowSingleTargetActiveBonus_MessageData2.m_ShowSingleTargetActiveBonus = false;
					cShowSingleTargetActiveBonus_MessageData2.m_Ability = ability;
					ScenarioRuleClient.MessageHandler(cShowSingleTargetActiveBonus_MessageData2);
				}
				return true;
			}
		}
		return false;
	}

	public static void GetValidActorsInRange(CAbility ability)
	{
		ability.ValidActorsInRange = new List<CActor>();
		CTile cTile = null;
		if (ability.ParentAbility != null && (ability.MiscAbilityData == null || ability.MiscAbilityData.UseParentTiles != false))
		{
			cTile = ((ability.ParentAbility.TilesSelected != null && ability.ParentAbility.TilesSelected.Count > 0) ? ability.ParentAbility.TilesSelected[0] : null);
			if (cTile == null)
			{
				cTile = ((ability.ParentAbility.ActorsTargeted != null && ability.ParentAbility.ActorsTargeted.Count > 0) ? ScenarioManager.Tiles[ability.ParentAbility.ActorsTargeted[0].ArrayIndex.X, ability.ParentAbility.ActorsTargeted[0].ArrayIndex.Y] : null);
			}
		}
		else if (ability.TargetThisActorAutomatically != null)
		{
			cTile = ScenarioManager.Tiles[ability.TargetThisActorAutomatically.ArrayIndex.X, ability.TargetThisActorAutomatically.ArrayIndex.Y];
		}
		CTile cTile2 = ((cTile != null) ? cTile : ScenarioManager.Tiles[ability.TargetingActor.ArrayIndex.X, ability.TargetingActor.ArrayIndex.Y]);
		ability.TilesInRange = GameState.GetTilesInRange(cTile2.m_ArrayIndex, ability.Range, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ability.Targeting != CAbility.EAbilityTargeting.Range);
		if (ability.AreaEffect != null)
		{
			List<CTile> validTilesIncludingBlockedOut = new List<CTile>();
			CAreaEffect.GetAllPossibleTiles(ability.TargetingActor, ability.AreaEffect, ability.Range, ability.TilesInRange, getBlocked: true, ref validTilesIncludingBlockedOut);
			ability.AllPossibleTilesInAreaEffect = validTilesIncludingBlockedOut;
			ability.ValidActorsInRange = GameState.GetActorsInRange(ability.TargetingActor, ability.FilterActor, ability.Range, ability.ActorsToIgnore, ability.AbilityFilter, ability.AreaEffect, ability.AllPossibleTilesInAreaEffect, ability.IsTargetedAbility, cTile, ability.MiscAbilityData?.CanTargetInvisible);
			if (ability.UseSubAbilityTargeting && ability.ParentAbility?.TilesSelected != null && ability.ParentAbility.TilesSelected.Count > 1)
			{
				foreach (CTile item in ability.ParentAbility.TilesSelected.Skip(1))
				{
					CActor cActor = ScenarioManager.Scenario.FindActorAt(item.m_ArrayIndex);
					if (cActor != null && ability.AbilityFilter.IsValidTarget(cActor, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible))
					{
						ability.ValidActorsInRange.Add(cActor);
					}
				}
			}
		}
		else if (ability.AllTargetsOnMovePath)
		{
			foreach (CActor item2 in CAbilityMove.AllTargetActorsOnPath)
			{
				if (item2 != null)
				{
					CActor cActor2 = (CActor.AreActorsAllied(ability.TargetingActor.Type, item2.Type) ? null : item2);
					if (cActor2 != null && !cActor2.PhasedOut && ability.AbilityFilter.IsValidTarget(cActor2, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible))
					{
						ability.ValidActorsInRange.Add(cActor2);
					}
				}
			}
		}
		else if (ability.Targeting == CAbility.EAbilityTargeting.Range)
		{
			ability.ValidActorsInRange = GameState.GetActorsInRange(ability.TargetingActor, ability.FilterActor, ability.Range, ability.ActorsToIgnore, ability.AbilityFilter, null, null, ability.IsTargetedAbility, cTile, ability.MiscAbilityData?.CanTargetInvisible);
			if (ability.UseSubAbilityTargeting && ability.ParentAbility.TilesSelected != null && ability.ParentAbility.TilesSelected.Count > 1)
			{
				foreach (CTile item3 in ability.ParentAbility.TilesSelected.Skip(1))
				{
					CActor cActor3 = ScenarioManager.Scenario.FindActorAt(item3.m_ArrayIndex);
					if (cActor3 != null && ability.AbilityFilter.IsValidTarget(cActor3, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible))
					{
						ability.ValidActorsInRange.Add(cActor3);
					}
				}
			}
		}
		else if (ability.Targeting == CAbility.EAbilityTargeting.Room || ability.Targeting == CAbility.EAbilityTargeting.All || ability.Targeting == CAbility.EAbilityTargeting.AllConnectedRooms)
		{
			ability.ValidActorsInRange = new List<CActor>();
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				if (playerActor.PhasedOut || !ability.AbilityFilter.IsValidTarget(playerActor, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible))
				{
					continue;
				}
				bool flag = false;
				if (ability.Targeting == CAbility.EAbilityTargeting.All)
				{
					flag = true;
				}
				else if (ability.Targeting == CAbility.EAbilityTargeting.Room && IsArrayIndexInSameRoom(cTile2.m_ArrayIndex, playerActor.ArrayIndex))
				{
					flag = true;
				}
				else if (ability.Targeting == CAbility.EAbilityTargeting.AllConnectedRooms)
				{
					ScenarioManager.PathFinder.FindPath(playerActor.ArrayIndex, cTile2.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out var foundPath);
					if (foundPath)
					{
						flag = true;
					}
				}
				if (flag)
				{
					ability.ValidActorsInRange.Add(playerActor);
				}
			}
			foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
			{
				if (heroSummon.PhasedOut || !ability.AbilityFilter.IsValidTarget(heroSummon, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible))
				{
					continue;
				}
				bool flag2 = false;
				if (ability.Targeting == CAbility.EAbilityTargeting.All)
				{
					flag2 = true;
				}
				else if (ability.Targeting == CAbility.EAbilityTargeting.Room && IsArrayIndexInSameRoom(cTile2.m_ArrayIndex, heroSummon.ArrayIndex))
				{
					flag2 = true;
				}
				else if (ability.Targeting == CAbility.EAbilityTargeting.AllConnectedRooms)
				{
					ScenarioManager.PathFinder.FindPath(heroSummon.ArrayIndex, cTile2.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out var foundPath2);
					if (foundPath2)
					{
						flag2 = true;
					}
				}
				if (flag2)
				{
					ability.ValidActorsInRange.Add(heroSummon);
				}
			}
			foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
			{
				if (allAliveMonster.PhasedOut || !ability.AbilityFilter.IsValidTarget(allAliveMonster, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible))
				{
					continue;
				}
				bool flag3 = false;
				if (ability.Targeting == CAbility.EAbilityTargeting.All)
				{
					flag3 = true;
				}
				else if (ability.Targeting == CAbility.EAbilityTargeting.Room && IsArrayIndexInSameRoom(cTile2.m_ArrayIndex, allAliveMonster.ArrayIndex))
				{
					flag3 = true;
				}
				else if (ability.Targeting == CAbility.EAbilityTargeting.AllConnectedRooms)
				{
					ScenarioManager.PathFinder.FindPath(allAliveMonster.ArrayIndex, cTile2.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out var foundPath3);
					if (foundPath3)
					{
						flag3 = true;
					}
				}
				if (flag3)
				{
					ability.ValidActorsInRange.Add(allAliveMonster);
				}
			}
			foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
			{
				if (@object.PhasedOut || !ability.AbilityFilter.IsValidTarget(@object, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible))
				{
					continue;
				}
				bool flag4 = false;
				if (ability.Targeting == CAbility.EAbilityTargeting.All)
				{
					flag4 = true;
				}
				else if (ability.Targeting == CAbility.EAbilityTargeting.Room && IsArrayIndexInSameRoom(cTile2.m_ArrayIndex, @object.ArrayIndex))
				{
					flag4 = true;
				}
				else if (ability.Targeting == CAbility.EAbilityTargeting.AllConnectedRooms)
				{
					ScenarioManager.PathFinder.FindPath(@object.ArrayIndex, cTile2.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out var foundPath4);
					if (foundPath4)
					{
						flag4 = true;
					}
				}
				if (flag4)
				{
					ability.ValidActorsInRange.Add(@object);
				}
			}
		}
		else
		{
			DLLDebug.LogError("Invalid targeting type.  Unable to target.");
		}
		if (ability.ValidActorsInRange.Count > 0)
		{
			for (int num = ability.ValidActorsInRange.Count - 1; num >= 0; num--)
			{
				CActor cActor4 = ability.ValidActorsInRange[num];
				if (CAbility.ImmuneToAbility(cActor4, ability))
				{
					AbilityData.MiscAbilityData miscAbilityData = ability.MiscAbilityData;
					if (miscAbilityData == null || miscAbilityData.AlsoTargetSelf != true)
					{
						ability.ValidActorsInRange.Remove(cActor4);
					}
				}
			}
		}
		ability.ValidActorsInRange.RemoveAll((CActor a) => a.IsUsingOnDeathAbility);
	}

	public static bool IsActorInSameRoom(CActor target, CActor targetingActor)
	{
		CNode cNode = ScenarioManager.PathFinder.Nodes[targetingActor.ArrayIndex.X, targetingActor.ArrayIndex.Y];
		CNode cNode2 = ScenarioManager.PathFinder.Nodes[target.ArrayIndex.X, target.ArrayIndex.Y];
		if (!cNode.IsBridge && !cNode2.IsBridge)
		{
			if (ScenarioManager.Tiles[target.ArrayIndex.X, target.ArrayIndex.Y].m_HexMap != ScenarioManager.Tiles[targetingActor.ArrayIndex.X, targetingActor.ArrayIndex.Y].m_HexMap)
			{
				return ScenarioManager.Tiles[target.ArrayIndex.X, target.ArrayIndex.Y].m_Hex2Map == ScenarioManager.Tiles[targetingActor.ArrayIndex.X, targetingActor.ArrayIndex.Y].m_HexMap;
			}
			return true;
		}
		return false;
	}

	public static bool IsArrayIndexInSameRoom(Point point1, Point point2)
	{
		CNode cNode = ScenarioManager.PathFinder.Nodes[point1.X, point1.Y];
		CNode cNode2 = ScenarioManager.PathFinder.Nodes[point2.X, point2.Y];
		if (!cNode.IsBridge && !cNode2.IsBridge)
		{
			if (ScenarioManager.Tiles[point1.X, point1.Y].m_HexMap != ScenarioManager.Tiles[point2.X, point2.Y].m_HexMap)
			{
				return ScenarioManager.Tiles[point1.X, point1.Y].m_Hex2Map == ScenarioManager.Tiles[point2.X, point2.Y].m_HexMap;
			}
			return true;
		}
		return false;
	}

	public static int GetDistanceBetweenActorsInHexes(CActor target, CActor targetingActor)
	{
		int result = -1;
		bool foundPath;
		List<Point> list = ScenarioManager.PathFinder.FindPath(targetingActor.ArrayIndex, target.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
		if (foundPath)
		{
			result = list.Count;
		}
		return result;
	}
}
