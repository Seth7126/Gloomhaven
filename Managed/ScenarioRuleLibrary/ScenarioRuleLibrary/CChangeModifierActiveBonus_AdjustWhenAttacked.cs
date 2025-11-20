using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CChangeModifierActiveBonus_AdjustWhenAttacked : CBespokeBehaviour
{
	public CBaseCard BaseCard;

	public CChangeModifierActiveBonus_AdjustWhenAttacked(CActor actor, CBaseCard baseCard, CAbilityChangeModifier ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnDrawModifier(ref List<AttackModifierYMLData> modifiers, CActor actor, CActor target)
	{
		if (target != m_Actor)
		{
			return;
		}
		CAbilityChangeModifier thisAbility = m_Ability as CAbilityChangeModifier;
		if (thisAbility.MiscAbilityData.ReplaceModifiers == null || thisAbility.MiscAbilityData.ReplaceWithModifier == null)
		{
			return;
		}
		for (int i = 0; i < modifiers.Count; i++)
		{
			if (thisAbility.MiscAbilityData.ReplaceModifiers.Contains(modifiers[i].MathModifier))
			{
				AttackModifierYMLData attackModifierYMLData = ScenarioRuleClient.SRLYML.AttackModifiers.FirstOrDefault((AttackModifierYMLData s) => s.Name == thisAbility.MiscAbilityData.ReplaceWithModifier);
				if (attackModifierYMLData != null)
				{
					modifiers[i].OriginalModifier = modifiers[i].MathModifier;
					modifiers[i].MathModifier = attackModifierYMLData.MathModifier;
					modifiers[i].NewCard = attackModifierYMLData.Card;
					m_ActiveBonus.RestrictActiveBonus(actor);
					CChangeModifier_MessageData message = new CChangeModifier_MessageData(GameState.InternalCurrentActor)
					{
						m_BaseCard = BaseCard,
						m_OriginalModifier = modifiers[i].OriginalModifier,
						m_ReplaceModifier = modifiers[i].MathModifier
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
		}
	}

	public CChangeModifierActiveBonus_AdjustWhenAttacked()
	{
	}

	public CChangeModifierActiveBonus_AdjustWhenAttacked(CChangeModifierActiveBonus_AdjustWhenAttacked state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
