using System;
using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class CAction
{
	private List<CAbility> m_Abilities = new List<CAbility>();

	private CBaseCard.ECardPile m_CardPile;

	private List<ElementInfusionBoardManager.EElement> m_Infusions = new List<ElementInfusionBoardManager.EElement>();

	private List<CActionAugmentation> m_Augmentations = new List<CActionAugmentation>();

	public Guid ID { get; private set; }

	public List<CAbility> Abilities => m_Abilities;

	public CBaseCard.ECardPile CardPile => m_CardPile;

	public List<ElementInfusionBoardManager.EElement> Infusions => m_Infusions;

	public List<CActionAugmentation> Augmentations => m_Augmentations;

	public int ActionXP { get; private set; }

	public CAction()
	{
	}

	public CAction(List<ElementInfusionBoardManager.EElement> infusions, List<CActionAugmentation> augmentations, int actionXP, CBaseCard.ECardPile cardPile = CBaseCard.ECardPile.Discarded)
	{
		ActionXP = actionXP;
		m_Infusions = infusions;
		m_Augmentations = augmentations;
		m_CardPile = cardPile;
		AssignID();
	}

	public CAction(List<ElementInfusionBoardManager.EElement> infusions, List<CActionAugmentation> augmentations, CAbility ability, int actionXP, CBaseCard.ECardPile cardPile = CBaseCard.ECardPile.Discarded)
	{
		ActionXP = actionXP;
		m_Infusions = infusions;
		m_Augmentations = augmentations;
		if (ability != null)
		{
			AddAbility(ability);
		}
		m_CardPile = cardPile;
		AssignID();
	}

	public CAction(List<ElementInfusionBoardManager.EElement> infusions, List<CActionAugmentation> augmentations, List<CAbility> abilities, int actionXP, CBaseCard.ECardPile cardPile = CBaseCard.ECardPile.Discarded)
	{
		ActionXP = actionXP;
		m_Infusions = infusions;
		m_Augmentations = augmentations;
		if (abilities != null)
		{
			m_Abilities = abilities;
		}
		m_CardPile = cardPile;
		AssignID();
	}

	public CAbility FindType(CAbility.EAbilityType type)
	{
		return Abilities.Find((CAbility x) => x.AbilityType == type);
	}

	public void AddAbility(CAbility ability)
	{
		m_Abilities.Add(ability);
	}

	public string GetDescription()
	{
		string text = "";
		foreach (CAbility ability in m_Abilities)
		{
			text += ability.GetDescription();
			text += "\n";
		}
		return text;
	}

	public CAction Copy()
	{
		return new CAction
		{
			ID = ID,
			m_Abilities = m_Abilities.Select((CAbility s) => CAbility.CopyAbility(s, generateNewID: false)).ToList(),
			m_CardPile = m_CardPile,
			m_Infusions = m_Infusions?.ToList(),
			m_Augmentations = m_Augmentations?.Select((CActionAugmentation s) => s.Copy()).ToList(),
			ActionXP = ActionXP
		};
	}

	private void AssignID()
	{
		ID = Guid.NewGuid();
		if (m_Augmentations == null)
		{
			return;
		}
		foreach (CActionAugmentation augmentation in m_Augmentations)
		{
			augmentation.ActionID = ID;
		}
	}

	public void Reset()
	{
		foreach (CAbility ability in m_Abilities)
		{
			ability.Reset();
		}
	}

	public bool Validate()
	{
		if (Abilities == null)
		{
			return false;
		}
		foreach (CAbility ability in Abilities)
		{
			if (!ability.Validate())
			{
				return false;
			}
		}
		return true;
	}
}
