using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityNullHex : CAbility
{
	public enum ENullHexState
	{
		SelectHexPositions,
		AnimateCharacter,
		NullHexDone
	}

	private ENullHexState m_State;

	public CAbilityNullHex()
	{
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = ENullHexState.SelectHexPositions;
		if (base.AreaEffect != null)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
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
		case ENullHexState.SelectHexPositions:
		{
			if (base.AreaEffect == null)
			{
				base.TilesInRange = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
				List<CTile> list = new List<CTile>();
				foreach (CTile item in base.TilesInRange)
				{
					if (CAbilityFilter.IsValidTile(item, base.TileFilter))
					{
						list.Add(item);
					}
				}
				base.TilesInRange = list.ToList();
			}
			CPlayerSelectingObjectPosition_MessageData message2 = new CPlayerSelectingObjectPosition_MessageData(base.TargetingActor)
			{
				m_SpawnType = ScenarioManager.ObjectImportType.None,
				m_TileFilter = new List<CAbilityFilter.EFilterTile> { base.TileFilter },
				m_Ability = this
			};
			ScenarioRuleClient.MessageHandler(message2);
			return true;
		}
		case ENullHexState.AnimateCharacter:
			base.AbilityHasHappened = true;
			if (m_TilesSelected.Count != 0)
			{
				CActorSelectedNullHexes_MessageData message = new CActorSelectedNullHexes_MessageData(base.AnimOverload, base.TargetingActor)
				{
					m_Ability = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			else if (!base.IsMergedAbility)
			{
				PhaseManager.NextStep();
			}
			break;
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
		if (!base.TilesSelected.Contains(selectedTile) && m_State == ENullHexState.SelectHexPositions && !base.UseSubAbilityTargeting)
		{
			if (base.AreaEffect != null)
			{
				if (!m_AreaEffectLocked && m_ValidTilesInAreaEffect.Count == base.ValidTilesInAreaEffectedIncludingBlocked.Count && m_ValidTilesInAreaEffect.Count == base.AreaEffect.AllHexes.Count)
				{
					foreach (CTile item in m_ValidTilesInAreaEffect)
					{
						if (!CAbilityFilter.IsValidTile(item, base.TileFilter))
						{
							if (flag)
							{
								Perform();
							}
							base.TileSelected(selectedTile, optionalTileList);
							LogEvent(ESESubTypeAbility.AbilityTileSelected);
							return;
						}
					}
					m_TilesSelected.AddRange(m_ValidTilesInAreaEffect);
					m_AreaEffectLocked = true;
					if (base.TargetingActor.Type == CActor.EType.Player)
					{
						CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(base.TargetingActor);
						cPlayerSelectedTile_MessageData.m_Ability = this;
						ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
					}
				}
			}
			else if (base.TilesInRange.Contains(selectedTile) && m_TilesSelected.Count < m_NumberTargets)
			{
				if (!m_TilesSelected.Contains(selectedTile))
				{
					m_TilesSelected.Add(selectedTile);
					flag = true;
				}
				if (base.TargetingActor.Type == CActor.EType.Player)
				{
					CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData2 = new CPlayerSelectedTile_MessageData(base.TargetingActor);
					cPlayerSelectedTile_MessageData2.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData2);
				}
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
		bool flag2 = false;
		if (base.AreaEffect != null && m_AreaEffectLocked)
		{
			m_AreaEffectLocked = false;
			m_TilesSelected.Clear();
			flag2 = true;
		}
		else
		{
			if (m_TilesSelected.Contains(selectedTile))
			{
				m_TilesSelected.Remove(selectedTile);
			}
			flag2 = true;
		}
		if (flag2)
		{
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(base.TargetingActor);
				cPlayerSelectedTile_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
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

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == ENullHexState.SelectHexPositions)
		{
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		return m_State == ENullHexState.SelectHexPositions;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == ENullHexState.SelectHexPositions;
		}
		return false;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == ENullHexState.NullHexDone;
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		CActor targetingActor = base.TargetingActor;
		if (targetingActor != null && targetingActor.Type == CActor.EType.Enemy)
		{
			_ = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == base.TargetingActor.ActorGuid)?.IsSummon;
		}
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityNullHex(CAbilityNullHex state, ReferenceDictionary references)
		: base(state, references)
	{
		m_State = state.m_State;
	}
}
