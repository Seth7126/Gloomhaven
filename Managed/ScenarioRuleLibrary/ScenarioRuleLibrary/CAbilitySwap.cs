using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilitySwap : CAbility
{
	public enum ESwapState
	{
		None,
		SelectTargets,
		CastSwap,
		FinishSwap,
		SwapDone
	}

	private ESwapState m_State;

	private Point m_FirstTargetStartLocation;

	private Point m_SecondTargetStartLocation;

	private List<CActor> m_FirstTargetValidActorsInRange = new List<CActor>();

	private List<CActor> m_SecondTargetValidActorsInRange = new List<CActor>();

	public CAbilityFilterContainer FirstTargetFilter { get; set; }

	public CAbilityFilterContainer SecondTargetFilter { get; set; }

	public CAbilitySwap(CAbilityFilterContainer firstTargetFilter = null, CAbilityFilterContainer secondTargetFilter = null)
	{
		FirstTargetFilter = firstTargetFilter;
		SecondTargetFilter = secondTargetFilter;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = ESwapState.SelectTargets;
		CAbilityFilterContainer abilityFilter = base.AbilityFilter;
		if (FirstTargetFilter == null)
		{
			FirstTargetFilter = base.AbilityFilter;
		}
		if (SecondTargetFilter == null)
		{
			SecondTargetFilter = base.AbilityFilter;
		}
		base.AbilityFilter = FirstTargetFilter;
		SharedAbilityTargeting.GetValidActorsInRange(this);
		RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
		m_FirstTargetValidActorsInRange = m_ValidActorsInRange.ToList();
		base.AbilityFilter = SecondTargetFilter;
		SharedAbilityTargeting.GetValidActorsInRange(this);
		RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
		m_SecondTargetValidActorsInRange = m_ValidActorsInRange.ToList();
		if (FirstTargetFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			m_ActorsToTarget.Add(base.TargetingActor);
			base.ValidActorsInRange = m_SecondTargetValidActorsInRange.ToList();
			FilterTargetsForFlying(base.TargetingActor, ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y]);
		}
		else if (SecondTargetFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			m_ActorsToTarget.Add(base.TargetingActor);
			base.ValidActorsInRange = m_FirstTargetValidActorsInRange.ToList();
			FilterTargetsForFlying(base.TargetingActor, ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y]);
		}
		base.AbilityFilter = abilityFilter;
		m_ValidActorsInRange = m_FirstTargetValidActorsInRange.Concat(m_SecondTargetValidActorsInRange).Distinct().ToList();
		if (m_NumberTargets == -1 && m_ValidActorsInRange.Count == 2)
		{
			m_ActorsToTarget = m_ValidActorsInRange;
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
		case ESwapState.SelectTargets:
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(base.TargetingActor);
				cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
				cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = true;
				ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			}
			break;
		case ESwapState.CastSwap:
		{
			base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
			ScenarioRuleClient.FirstAbilityStarted();
			base.AbilityHasHappened = true;
			m_CanUndo = false;
			m_FirstTargetStartLocation = m_ActorsToTarget[0].ArrayIndex;
			m_SecondTargetStartLocation = m_ActorsToTarget[1].ArrayIndex;
			CActorIsCastingSwap_MessageData message2 = new CActorIsCastingSwap_MessageData(base.TargetingActor)
			{
				m_ActorCasting = base.TargetingActor,
				m_FirstTarget = m_ActorsToTarget[0],
				m_SecondTarget = m_ActorsToTarget[1],
				m_SwapAbility = this
			};
			ScenarioRuleClient.MessageHandler(message2);
			break;
		}
		case ESwapState.FinishSwap:
		{
			CActorsAreSwapping_MessageData message = new CActorsAreSwapping_MessageData(base.TargetingActor)
			{
				m_FirstTarget = m_ActorsToTarget[0],
				m_SecondTarget = m_ActorsToTarget[1],
				m_SwapAbility = this
			};
			GameState.LostAdjacency(m_ActorsToTarget[0], m_ActorsToTarget[0].ArrayIndex, m_ActorsToTarget[1].ArrayIndex);
			GameState.LostAdjacency(m_ActorsToTarget[1], m_ActorsToTarget[1].ArrayIndex, m_ActorsToTarget[0].ArrayIndex);
			Point arrayIndex = m_ActorsToTarget[0].ArrayIndex;
			m_ActorsToTarget[0].ArrayIndex = m_ActorsToTarget[1].ArrayIndex;
			m_ActorsToTarget[1].ArrayIndex = arrayIndex;
			ScenarioRuleClient.MessageHandler(message);
			PhaseManager.StepComplete();
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
		CActor actorOnTile = GameState.GetActorOnTile(selectedTile, base.FilterActor, base.AbilityFilter, new List<CActor>(), base.IsTargetedAbility, base.MiscAbilityData?.CanTargetInvisible);
		if (actorOnTile != null && base.ValidActorsInRange.Contains(actorOnTile) && m_State == ESwapState.SelectTargets && m_ActorsToTarget.Count < 2 && !m_ActorsToTarget.Contains(actorOnTile))
		{
			m_ActorsToTarget.Add(actorOnTile);
			if (m_ActorsToTarget.Count < 2)
			{
				if (m_FirstTargetValidActorsInRange.Contains(actorOnTile))
				{
					base.ValidActorsInRange = m_SecondTargetValidActorsInRange.ToList();
				}
				else
				{
					base.ValidActorsInRange = m_FirstTargetValidActorsInRange.ToList();
				}
				FilterTargetsForFlying(actorOnTile, selectedTile);
			}
			else
			{
				base.ValidActorsInRange.Clear();
			}
			flag = true;
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
		CActor actorOnTile = GameState.GetActorOnTile(selectedTile, base.FilterActor, base.AbilityFilter, new List<CActor>(), base.IsTargetedAbility, base.MiscAbilityData?.CanTargetInvisible);
		if (m_ActorsToTarget.Contains(actorOnTile))
		{
			m_ActorsToTarget.Remove(actorOnTile);
			if (m_ActorsToTarget.Count > 0)
			{
				if (m_FirstTargetValidActorsInRange.Contains(m_ActorsToTarget[0]))
				{
					base.ValidActorsInRange = m_SecondTargetValidActorsInRange.ToList();
				}
				else
				{
					base.ValidActorsInRange = m_FirstTargetValidActorsInRange.ToList();
				}
			}
			else
			{
				m_ValidActorsInRange = m_FirstTargetValidActorsInRange.Concat(m_SecondTargetValidActorsInRange).Distinct().ToList();
			}
		}
		if (false)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public void FilterTargetsForFlying(CActor selectedActor, CTile selectedTile)
	{
		if (selectedTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) != null)
		{
			for (int num = base.ValidActorsInRange.Count - 1; num >= 0; num--)
			{
				CActor cActor = base.ValidActorsInRange[num];
				if (!cActor.Flying)
				{
					base.ValidActorsInRange.Remove(cActor);
				}
			}
		}
		else
		{
			if (selectedActor.Flying)
			{
				return;
			}
			for (int num2 = base.ValidActorsInRange.Count - 1; num2 >= 0; num2--)
			{
				CActor cActor2 = base.ValidActorsInRange[num2];
				if (ScenarioManager.Tiles[cActor2.ArrayIndex.X, cActor2.ArrayIndex.Y].FindProp(ScenarioManager.ObjectImportType.Obstacle) != null)
				{
					base.ValidActorsInRange.Remove(cActor2);
				}
			}
		}
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		CAbilityFilterContainer abilityFilter = base.AbilityFilter;
		base.AbilityFilter = FirstTargetFilter;
		SharedAbilityTargeting.GetValidActorsInRange(this);
		m_FirstTargetValidActorsInRange = m_ValidActorsInRange.ToList();
		base.AbilityFilter = SecondTargetFilter;
		SharedAbilityTargeting.GetValidActorsInRange(this);
		m_SecondTargetValidActorsInRange = m_ValidActorsInRange.ToList();
		if (FirstTargetFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			m_ActorsToTarget.Add(base.TargetingActor);
			base.ValidActorsInRange = m_SecondTargetValidActorsInRange.ToList();
			FilterTargetsForFlying(base.TargetingActor, ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y]);
		}
		else if (SecondTargetFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			m_ActorsToTarget.Add(base.TargetingActor);
			base.ValidActorsInRange = m_FirstTargetValidActorsInRange.ToList();
			FilterTargetsForFlying(base.TargetingActor, ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y]);
		}
		base.AbilityFilter = abilityFilter;
		m_ValidActorsInRange = m_FirstTargetValidActorsInRange.Concat(m_SecondTargetValidActorsInRange).Distinct().ToList();
		if (m_State == ESwapState.SelectTargets)
		{
			Perform();
		}
	}

	public override void Restart()
	{
		base.Restart();
		m_State = ESwapState.SelectTargets;
		m_ActorsToTarget.Clear();
		CAbilityFilterContainer abilityFilter = base.AbilityFilter;
		base.AbilityFilter = FirstTargetFilter;
		SharedAbilityTargeting.GetValidActorsInRange(this);
		m_FirstTargetValidActorsInRange = m_ValidActorsInRange.ToList();
		base.AbilityFilter = SecondTargetFilter;
		SharedAbilityTargeting.GetValidActorsInRange(this);
		m_SecondTargetValidActorsInRange = m_ValidActorsInRange.ToList();
		base.AbilityFilter = abilityFilter;
		m_ValidActorsInRange = m_FirstTargetValidActorsInRange.Concat(m_SecondTargetValidActorsInRange).Distinct().ToList();
	}

	public override bool CanClearTargets()
	{
		return m_State == ESwapState.SelectTargets;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == ESwapState.SelectTargets;
		}
		return false;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_ActorsToTarget.Count == 2;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == ESwapState.SwapDone;
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilitySwap(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, m_State, (m_ActorsToTarget.Count > 0) ? m_ActorsToTarget[0].GetPrefabName() : null, (m_ActorsToTarget.Count > 1) ? m_ActorsToTarget[1].GetPrefabName() : null, new TileIndex(m_FirstTargetStartLocation), new TileIndex(m_SecondTargetStartLocation), base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
		for (int i = 0; i < m_ActorsToTarget.Count; i++)
		{
			CActor cActor = m_ActorsToTarget[i];
			CTile cTile = ScenarioManager.Tiles[cActor.ArrayIndex.X, cActor.ArrayIndex.Y];
			for (int num = cTile.m_Props.Count - 1; num >= 0; num--)
			{
				if (cTile.m_Props[num].ObjectType != ScenarioManager.ObjectImportType.PressurePlate)
				{
					cTile.m_Props[num].AutomaticActivate(cActor);
				}
			}
		}
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilitySwap(CAbilitySwap state, ReferenceDictionary references)
		: base(state, references)
	{
		FirstTargetFilter = references.Get(state.FirstTargetFilter);
		if (FirstTargetFilter == null && state.FirstTargetFilter != null)
		{
			FirstTargetFilter = new CAbilityFilterContainer(state.FirstTargetFilter, references);
			references.Add(state.FirstTargetFilter, FirstTargetFilter);
		}
		SecondTargetFilter = references.Get(state.SecondTargetFilter);
		if (SecondTargetFilter == null && state.SecondTargetFilter != null)
		{
			SecondTargetFilter = new CAbilityFilterContainer(state.SecondTargetFilter, references);
			references.Add(state.SecondTargetFilter, SecondTargetFilter);
		}
		m_State = state.m_State;
		m_FirstTargetValidActorsInRange = references.Get(state.m_FirstTargetValidActorsInRange);
		if (m_FirstTargetValidActorsInRange == null && state.m_FirstTargetValidActorsInRange != null)
		{
			m_FirstTargetValidActorsInRange = new List<CActor>();
			for (int i = 0; i < state.m_FirstTargetValidActorsInRange.Count; i++)
			{
				CActor cActor = state.m_FirstTargetValidActorsInRange[i];
				CActor cActor2 = references.Get(cActor);
				if (cActor2 == null && cActor != null)
				{
					CActor cActor3 = ((cActor is CObjectActor state2) ? new CObjectActor(state2, references) : ((cActor is CEnemyActor state3) ? new CEnemyActor(state3, references) : ((cActor is CHeroSummonActor state4) ? new CHeroSummonActor(state4, references) : ((!(cActor is CPlayerActor state5)) ? new CActor(cActor, references) : new CPlayerActor(state5, references)))));
					cActor2 = cActor3;
					references.Add(cActor, cActor2);
				}
				m_FirstTargetValidActorsInRange.Add(cActor2);
			}
			references.Add(state.m_FirstTargetValidActorsInRange, m_FirstTargetValidActorsInRange);
		}
		m_SecondTargetValidActorsInRange = references.Get(state.m_SecondTargetValidActorsInRange);
		if (m_SecondTargetValidActorsInRange != null || state.m_SecondTargetValidActorsInRange == null)
		{
			return;
		}
		m_SecondTargetValidActorsInRange = new List<CActor>();
		for (int j = 0; j < state.m_SecondTargetValidActorsInRange.Count; j++)
		{
			CActor cActor4 = state.m_SecondTargetValidActorsInRange[j];
			CActor cActor5 = references.Get(cActor4);
			if (cActor5 == null && cActor4 != null)
			{
				CActor cActor3 = ((cActor4 is CObjectActor state6) ? new CObjectActor(state6, references) : ((cActor4 is CEnemyActor state7) ? new CEnemyActor(state7, references) : ((cActor4 is CHeroSummonActor state8) ? new CHeroSummonActor(state8, references) : ((!(cActor4 is CPlayerActor state9)) ? new CActor(cActor4, references) : new CPlayerActor(state9, references)))));
				cActor5 = cActor3;
				references.Add(cActor4, cActor5);
			}
			m_SecondTargetValidActorsInRange.Add(cActor5);
		}
		references.Add(state.m_SecondTargetValidActorsInRange, m_SecondTargetValidActorsInRange);
	}
}
