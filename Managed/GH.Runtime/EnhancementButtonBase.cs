using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

public class EnhancementButtonBase : MonoBehaviour
{
	public EEnhancement EnhancementType;

	public EEnhancementLine EnhancementLine;

	public int AbilityCardID;

	public string AbilityName;

	public int EnhancementSlot;

	public string AbilityNameLookupText;

	public RectTransform ParentContainer;

	public CAbilityCard AbilityCard
	{
		get
		{
			foreach (CCharacterClass @class in CharacterClassManager.Classes)
			{
				CAbilityCard cAbilityCard = @class.FindCardWithID(AbilityCardID);
				if (cAbilityCard != null)
				{
					return cAbilityCard;
				}
			}
			return null;
		}
	}

	public CAbility Ability => AbilityCard.FindAbilityOnCard(AbilityName);

	public EEnhancement EnhancedType => Enhancement?.Enhancement ?? EEnhancement.NoEnhancement;

	public bool Enhanced => EnhancedType != EEnhancement.NoEnhancement;

	public CEnhancement Enhancement => Ability.AbilityEnhancements.SingleOrDefault((CEnhancement s) => s.EnhancementLine == EnhancementLine && s.EnhancementSlot == EnhancementSlot);

	public virtual void Init(CEnhancement enhancement, string abilityNameLookupText, RectTransform parentContainer)
	{
		EnhancementType = enhancement.Enhancement;
		EnhancementLine = enhancement.EnhancementLine;
		AbilityCardID = enhancement.AbilityCardID;
		AbilityName = enhancement.AbilityName;
		EnhancementSlot = enhancement.EnhancementSlot;
		AbilityNameLookupText = abilityNameLookupText;
		ParentContainer = parentContainer;
	}
}
