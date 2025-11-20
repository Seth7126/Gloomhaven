using System.Collections.Generic;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityMergedCreateAttack : CAbilityMerged
{
	public enum MergedCreateAttackState
	{
		CreateAbilitySelectObstaclePositions,
		AttackAbilitySelectAttackFocus,
		PreActorIsAttacking,
		StartAttackAndCreateObstacle,
		FinishAttack,
		MergedDone
	}

	private CAbilityCreate m_MergedCreateAbility;

	private CAbilityAttack m_MergedAttackAbility;

	private CAbilityCreate m_MergedCreateAbilityCopy;

	private CAbilityAttack m_MergedAttackAbilityCopy;

	private MergedCreateAttackState m_State;

	public CAbilityCreate MergedCreateAbility => m_MergedCreateAbility;

	public CAbilityAttack MergedAttackAbility => m_MergedAttackAbility;

	public CAbilityCreate MergedCreateAbilityCopy => m_MergedCreateAbilityCopy;

	public CAbilityAttack MergedAttackAbilityCopy => m_MergedAttackAbilityCopy;

	public CAbilityMergedCreateAttack(CAbilityCreate createAbility, CAbilityAttack attackAbility)
		: base(createAbility, attackAbility)
	{
		m_MergedCreateAbility = createAbility;
		m_MergedAttackAbility = attackAbility;
	}

	public override void CopyMergedAbilities()
	{
		base.CopyMergedAbilities();
		m_MergedCreateAbilityCopy = (CAbilityCreate)CAbility.CopyAbility(m_MergedCreateAbility, generateNewID: false);
		m_MergedAttackAbilityCopy = (CAbilityAttack)CAbility.CopyAbility(m_MergedAttackAbility, generateNewID: false);
		m_MergedCreateAbilityCopy.ParentAbility = this;
		m_MergedAttackAbilityCopy.ParentAbility = this;
		m_CopiedMergedAbilities.Add(m_MergedCreateAbilityCopy);
		m_CopiedMergedAbilities.Add(m_MergedAttackAbilityCopy);
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = MergedCreateAttackState.CreateAbilitySelectObstaclePositions;
		m_MergedCreateAbilityCopy.Start(targetActor, filterActor, controllingActor);
		m_ActiveAbility = m_MergedCreateAbilityCopy;
	}

	public override bool Perform()
	{
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		if (!base.ProcessIfDead)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_006e;
			}
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_006e;
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
		case MergedCreateAttackState.CreateAbilitySelectObstaclePositions:
			if (m_ActiveAbility != m_MergedCreateAbilityCopy)
			{
				m_ActiveAbility = m_MergedCreateAbilityCopy;
			}
			if (!m_MergedCreateAbilityCopy.HasPassedState(CAbilityCreate.CreateState.SelectObstaclePositions) && !m_MergedCreateAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedCreateAbilityCopy.Perform();
				break;
			}
			m_State++;
			Perform();
			break;
		case MergedCreateAttackState.AttackAbilitySelectAttackFocus:
			if (m_ActiveAbility != m_MergedAttackAbilityCopy)
			{
				m_MergedAttackAbilityCopy.Start(m_StartTargetActor, m_StartFilterActor);
				m_ActiveAbility = m_MergedAttackAbilityCopy;
				if (!m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
				{
					CStartSecondMergedAbility_MessageData message = new CStartSecondMergedAbility_MessageData(base.TargetingActor)
					{
						m_Ability = this
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
			if (!m_MergedAttackAbilityCopy.HasPassedState(CAbilityAttack.EAttackState.SelectAttackFocus) && !m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedAttackAbilityCopy.Perform();
				break;
			}
			m_State++;
			Perform();
			break;
		case MergedCreateAttackState.PreActorIsAttacking:
			if (m_ActiveAbility != m_MergedAttackAbilityCopy)
			{
				m_ActiveAbility = m_MergedAttackAbilityCopy;
			}
			if (!m_MergedAttackAbilityCopy.HasPassedState(CAbilityAttack.EAttackState.UpdateAttackFocusAfterAttackEffectInlineSubAbility) && !m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedAttackAbilityCopy.Perform();
				break;
			}
			m_State++;
			Perform();
			break;
		case MergedCreateAttackState.StartAttackAndCreateObstacle:
			if (m_ActiveAbility != m_MergedAttackAbilityCopy)
			{
				m_ActiveAbility = m_MergedAttackAbilityCopy;
			}
			m_State++;
			if (!m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedAttackAbilityCopy.Perform();
			}
			if (!m_MergedCreateAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedCreateAbilityCopy.Perform();
			}
			break;
		case MergedCreateAttackState.FinishAttack:
			base.AbilityHasHappened = true;
			if (m_ActiveAbility != m_MergedAttackAbilityCopy)
			{
				m_ActiveAbility = m_MergedAttackAbilityCopy;
			}
			if (!m_MergedAttackAbilityCopy.HasPassedState(CAbilityAttack.EAttackState.FinaliseAttack) && !m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedAttackAbilityCopy.Perform();
				break;
			}
			m_State++;
			PhaseManager.StepComplete();
			break;
		}
		return false;
		IL_006e:
		if (base.TargetingActor.Type == CActor.EType.Player)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message2 = new CPlayerIsSleeping_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message2);
			}
			else
			{
				CPlayerIsStunned_MessageData message3 = new CPlayerIsStunned_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message3);
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
		if (m_State == MergedCreateAttackState.AttackAbilitySelectAttackFocus || (m_State == MergedCreateAttackState.PreActorIsAttacking && m_MergedAttackAbilityCopy.CanReceiveTileSelection()))
		{
			m_MergedAttackAbilityCopy.TileSelected(selectedTile, optionalTileList);
		}
		else if (m_State == MergedCreateAttackState.CreateAbilitySelectObstaclePositions)
		{
			m_MergedCreateAbilityCopy.TileSelected(selectedTile, optionalTileList);
		}
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (m_State == MergedCreateAttackState.AttackAbilitySelectAttackFocus || (m_State == MergedCreateAttackState.PreActorIsAttacking && m_MergedAttackAbilityCopy.CanReceiveTileSelection()))
		{
			m_MergedAttackAbilityCopy.TileDeselected(selectedTile, optionalTileList);
		}
		else if (m_State == MergedCreateAttackState.CreateAbilitySelectObstaclePositions)
		{
			m_MergedCreateAbilityCopy.TileDeselected(selectedTile, optionalTileList);
		}
	}

	public override bool CanClearTargets()
	{
		if (m_State != MergedCreateAttackState.AttackAbilitySelectAttackFocus)
		{
			return m_State == MergedCreateAttackState.CreateAbilitySelectObstaclePositions;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (m_State != MergedCreateAttackState.AttackAbilitySelectAttackFocus && m_State != MergedCreateAttackState.CreateAbilitySelectObstaclePositions)
		{
			if (m_State == MergedCreateAttackState.PreActorIsAttacking)
			{
				return m_MergedAttackAbilityCopy.CanReceiveTileSelection();
			}
			return false;
		}
		return true;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		base.ActiveAbility.AbilityComplete(dontMoveState, out fullAbilityRestart);
		return m_State == MergedCreateAttackState.MergedDone;
	}

	public override void AbilityEnded()
	{
		base.AbilityEnded();
		if (m_MergedAttackAbilityCopy.TargetingActor != null)
		{
			m_MergedAttackAbilityCopy.AbilityEnded();
		}
		if (m_MergedCreateAbilityCopy.TargetingActor != null)
		{
			m_MergedCreateAbilityCopy.AbilityEnded();
		}
	}

	public override string GetDescription()
	{
		return "MeredCreateAttack R: " + m_Range + " N: " + m_NumberTargets;
	}

	public override CAbility GetMergedWithAbility(CAbility ability)
	{
		if (!m_MergedCreateAbilityCopy.Equals(ability))
		{
			return m_MergedCreateAbilityCopy;
		}
		return m_MergedAttackAbilityCopy;
	}

	public CAbilityMergedCreateAttack()
	{
	}

	public CAbilityMergedCreateAttack(CAbilityMergedCreateAttack state, ReferenceDictionary references)
		: base(state, references)
	{
		m_State = state.m_State;
	}
}
