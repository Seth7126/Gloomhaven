using System.Linq;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class COverrideAbilityTypeActiveBonus_OverrideAbility : CBespokeBehaviour
{
	private int originalStrength;

	public COverrideAbilityTypeActiveBonus_OverrideAbility(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnAbilityStarted(CAbility startedAbility)
	{
		ActivateNonToggleOverrides(startedAbility);
	}

	private void ActivateNonToggleOverrides(CAbility ability)
	{
		if (!m_ActiveBonusData.IsToggleBonus && IsValidAbilityType(ability) && IsValidAttackType(ability) && ability.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)
		{
			OnActiveBonusToggled(ability, toggledOn: true);
		}
	}

	public override void OnAbilityEnded(CAbility endedAbility)
	{
		if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(endedAbility.TargetingActor))
		{
			m_ActiveBonus.RestrictActiveBonus(endedAbility.TargetingActor);
			OnBehaviourTriggered();
		}
	}

	public override void OnActiveBonusToggled(CAbility currentAbility, bool toggledOn)
	{
		if (IsValidAbilityType(currentAbility) && IsValidAttackType(currentAbility) && ValidTargetTypeFilters(currentAbility) && toggledOn)
		{
			foreach (CItem item in currentAbility.ActiveSingleTargetItems.Concat(currentAbility.ActiveOverrideItems))
			{
				currentAbility.TargetingActor.Inventory.DeselectItem(item);
			}
			for (int num = currentAbility.CurrentOverrides.Count - 1; num >= 0; num--)
			{
				CAbilityOverride abilityOverride = currentAbility.CurrentOverrides[num];
				currentAbility.UndoOverride(abilityOverride, perform: false);
			}
			if (m_ActiveBonusData.ActiveBonusAbilityOverrides.Count <= 0)
			{
				return;
			}
			originalStrength = currentAbility.Strength;
			{
				foreach (CAbilityOverride activeBonusAbilityOverride in m_ActiveBonusData.ActiveBonusAbilityOverrides)
				{
					CAbility.EAbilityType abilityType = currentAbility.AbilityType;
					currentAbility.OverrideAbilityValues(activeBonusAbilityOverride, perform: false);
					if (activeBonusAbilityOverride.AbilityType.HasValue && activeBonusAbilityOverride.AbilityType != abilityType && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
					{
						if (!(currentAbility is CAbilityAttack))
						{
							currentAbility.Strength = currentAbility.ModifiedStrength();
						}
						cPhaseAction.FinalizeOverrideAbilityType(currentAbility, activeBonusAbilityOverride);
					}
				}
				return;
			}
		}
		if (toggledOn || m_ActiveBonusData.ActiveBonusAbilityOverrides.Count <= 0)
		{
			return;
		}
		foreach (CAbilityOverride activeBonusAbilityOverride2 in m_ActiveBonusData.ActiveBonusAbilityOverrides)
		{
			bool flag = activeBonusAbilityOverride2.AbilityType.HasValue && activeBonusAbilityOverride2.AbilityType != activeBonusAbilityOverride2.OriginalAbility.AbilityType;
			foreach (CItem item2 in currentAbility.ActiveSingleTargetItems.Concat(currentAbility.ActiveOverrideItems))
			{
				currentAbility.TargetingActor.Inventory.DeselectItem(item2);
			}
			for (int num2 = currentAbility.CurrentOverrides.Count - 1; num2 >= 0; num2--)
			{
				CAbilityOverride abilityOverride2 = currentAbility.CurrentOverrides[num2];
				currentAbility.UndoOverride(abilityOverride2, perform: false);
			}
			if (flag && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction2)
			{
				currentAbility.Strength = originalStrength;
				cPhaseAction2.FinalizeOverrideAbilityType(currentAbility);
			}
		}
	}

	public COverrideAbilityTypeActiveBonus_OverrideAbility()
	{
	}

	public COverrideAbilityTypeActiveBonus_OverrideAbility(COverrideAbilityTypeActiveBonus_OverrideAbility state, ReferenceDictionary references)
		: base(state, references)
	{
		originalStrength = state.originalStrength;
	}
}
