using System.Collections.Generic;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityRecoverResources : CAbility
{
	public enum ERecoverResourcesState
	{
		None,
		SelectResourcesToRecover,
		RecoveringResources,
		RecoverResourcesDone
	}

	private ERecoverResourcesState m_State;

	public CAbilityRecoverResources()
	{
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = ERecoverResourcesState.SelectResourcesToRecover;
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
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
		else if (base.AreaEffect == null)
		{
			base.TilesInRange = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: false, null, ignorePathLength: true, ignoreBlockedWithActor: false, ignoreLOS: true);
			if (m_Range > 1)
			{
				base.TilesInRange.Add(ScenarioManager.GetAdjacentTile(base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter));
			}
		}
		if (m_NumberTargets == -1 || base.Targeting == EAbilityTargeting.All || base.Targeting == EAbilityTargeting.AllConnectedRooms)
		{
			m_AllTargets = true;
		}
		else
		{
			m_AllTargets = false;
		}
		if (base.UseSubAbilityTargeting)
		{
			if (base.IsInlineSubAbility && base.InlineSubAbilityTiles != null && base.InlineSubAbilityTiles.Count > 0)
			{
				foreach (CTile inlineSubAbilityTile in base.InlineSubAbilityTiles)
				{
					TileSelected(inlineSubAbilityTile, null);
				}
			}
			else if (base.ParentAbility != null && base.ParentAbility.TilesSelected != null && base.ParentAbility.TilesSelected.Count > 0)
			{
				foreach (CTile item in base.ParentAbility.TilesSelected)
				{
					TileSelected(item, null);
				}
			}
			m_State = ERecoverResourcesState.RecoveringResources;
		}
		else
		{
			if (base.TargetingActor.Type != CActor.EType.Player)
			{
				DLLDebug.LogError("CAbilityRecoverResources does not support non-player actors currently");
				return;
			}
			if (m_AllTargets)
			{
				m_TilesSelected.Clear();
				foreach (CTile item2 in base.TilesInRange)
				{
					if (item2.FindProp(ScenarioManager.ObjectImportType.Resource) is CObjectResource cObjectResource && base.ResourcesToTakeFromTargets.ContainsKey(cObjectResource.ResourceData.ID) && !m_TilesSelected.Contains(item2))
					{
						m_TilesSelected.Add(item2);
					}
					CActor actorOnTile = GameState.GetActorOnTile(item2, base.FilterActor, base.AbilityFilter, base.ActorsToIgnore, base.IsTargetedAbility, false);
					if (actorOnTile != null && base.AbilityFilter.IsValidTarget(actorOnTile, base.TargetingActor, base.IsTargetedAbility, useTargetOriginalType: false, false) && !m_TilesSelected.Contains(item2))
					{
						m_TilesSelected.Add(item2);
						m_ActorsToTarget.Add(actorOnTile);
					}
				}
				if (m_TilesSelected.Count <= 0)
				{
					m_CancelAbility = true;
				}
			}
		}
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
		case ERecoverResourcesState.SelectResourcesToRecover:
		{
			CPlayerSelectingObjectPosition_MessageData message2 = new CPlayerSelectingObjectPosition_MessageData(base.TargetingActor)
			{
				m_SpawnType = ScenarioManager.ObjectImportType.Resource,
				m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.None },
				m_Ability = this
			};
			ScenarioRuleClient.MessageHandler(message2);
			break;
		}
		case ERecoverResourcesState.RecoveringResources:
		{
			m_AbilityHasHappened = true;
			foreach (CTile item in base.TilesSelected)
			{
				if (item.FindProp(ScenarioManager.ObjectImportType.Resource) is CObjectResource cObjectResource && base.ResourcesToTakeFromTargets.ContainsKey(cObjectResource.ResourceData.ID))
				{
					base.TargetingActor.LootTile(item, asPartOfAbility: true, ScenarioManager.ObjectImportType.Resource);
				}
			}
			foreach (CActor item2 in base.ActorsToTarget)
			{
				foreach (string key in base.ResourcesToTakeFromTargets.Keys)
				{
					if (item2.CharacterHasResource(key, base.ResourcesToTakeFromTargets[key]))
					{
						item2.RemoveCharacterResource(key, base.ResourcesToTakeFromTargets[key]);
						base.TargetingActor.AddCharacterResource(key, base.ResourcesToTakeFromTargets[key]);
					}
				}
			}
			CActorIsRecoveringResources_MessageData message = new CActorIsRecoveringResources_MessageData(base.TargetingActor)
			{
				m_ActorRecovering = base.TargetingActor,
				m_RecoverAbility = this
			};
			ScenarioRuleClient.MessageHandler(message);
			PhaseManager.NextStep();
			break;
		}
		}
		return false;
		IL_0075:
		if (base.TargetingActor.Type == CActor.EType.Player)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message3 = new CPlayerIsSleeping_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message3);
			}
			else
			{
				CPlayerIsStunned_MessageData message4 = new CPlayerIsStunned_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message4);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		bool flag = false;
		if (!base.AllTargets && m_State == ERecoverResourcesState.SelectResourcesToRecover && !m_TilesSelected.Contains(selectedTile))
		{
			if (selectedTile.FindProp(ScenarioManager.ObjectImportType.Resource) is CObjectResource cObjectResource && base.ResourcesToTakeFromTargets.ContainsKey(cObjectResource.ResourceData.ID) && !m_TilesSelected.Contains(selectedTile))
			{
				m_TilesSelected.Add(selectedTile);
				flag = true;
			}
			CActor actorOnTile = GameState.GetActorOnTile(selectedTile, base.FilterActor, base.AbilityFilter, base.ActorsToIgnore, base.IsTargetedAbility, false);
			if (actorOnTile != null && base.AbilityFilter.IsValidTarget(actorOnTile, base.TargetingActor, base.IsTargetedAbility, useTargetOriginalType: false, false))
			{
				if (!m_TilesSelected.Contains(selectedTile))
				{
					m_TilesSelected.Add(selectedTile);
				}
				m_ActorsToTarget.Add(actorOnTile);
				flag = true;
			}
		}
		if (flag)
		{
			Perform();
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		bool flag = false;
		if (!base.AllTargets && m_State == ERecoverResourcesState.SelectResourcesToRecover && m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			CActor actorOnTile = GameState.GetActorOnTile(selectedTile, base.FilterActor, base.AbilityFilter, base.ActorsToIgnore, base.IsTargetedAbility, false);
			if (actorOnTile != null && m_ActorsToTarget.Contains(actorOnTile))
			{
				m_ActorsToTarget.Remove(actorOnTile);
			}
			flag = true;
		}
		if (flag)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override void Restart()
	{
		base.Restart();
	}

	public override bool CanClearTargets()
	{
		return m_State == ERecoverResourcesState.SelectResourcesToRecover;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == ERecoverResourcesState.SelectResourcesToRecover;
		}
		return false;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == ERecoverResourcesState.RecoverResourcesDone;
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		CActor targetingActor = base.TargetingActor;
		if (targetingActor != null && targetingActor.Type == CActor.EType.Enemy)
		{
			_ = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == base.TargetingActor.ActorGuid)?.IsSummon;
		}
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityRecoverResources(CAbilityRecoverResources state, ReferenceDictionary references)
		: base(state, references)
	{
		m_State = state.m_State;
	}
}
