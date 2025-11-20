using System.Collections.Generic;
using AStar;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CPreventDamageActiveBonus_PreventAndRetaliate : CBespokeBehaviour
{
	public CPreventDamageActiveBonus_PreventAndRetaliate(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnPreventDamageTriggered(int preventedDamage, CActor damageSource, CActor actorDamaged, CAbility damagingAbility)
	{
		CAbilityRetaliate obj = m_Ability.ActiveBonusData.AbilityData as CAbilityRetaliate;
		int strength = obj.Strength;
		int retaliateRange = obj.RetaliateRange;
		if (damageSource != null && damagingAbility != null && strength > 0 && retaliateRange > 0)
		{
			bool foundPath;
			List<Point> list = ScenarioManager.PathFinder.FindPath(damageSource.ArrayIndex, actorDamaged.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
			if (foundPath && list.Count <= retaliateRange)
			{
				CAbilityRetaliate retaliateAbility = new CAbilityRetaliate(retaliateRange)
				{
					Strength = strength
				};
				CRetaliate_MessageData message = new CRetaliate_MessageData("", actorDamaged)
				{
					m_RetaliateAbility = retaliateAbility,
					m_ActorAppliedTo = actorDamaged
				};
				ScenarioRuleClient.MessageHandler(message);
				damageSource.ApplyRetaliateToAttack(actorDamaged, damagingAbility, strength);
			}
		}
		OnBehaviourTriggered();
	}

	public CPreventDamageActiveBonus_PreventAndRetaliate()
	{
	}

	public CPreventDamageActiveBonus_PreventAndRetaliate(CPreventDamageActiveBonus_PreventAndRetaliate state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
