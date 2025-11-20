using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[DebuggerDisplay("{Name}")]
public class CAbilityCard : CBaseAbilityCard
{
	private CAction m_TopAction;

	private CAction m_DefaultAttackAction;

	private CAction m_DefaultMoveAction;

	private CAction m_BottomAction;

	private CAction m_SelectedAction;

	private CAction m_LastSelectedAction;

	private AbilityCardYMLData m_AbilityCardYML;

	public CAction TopAction => m_TopAction;

	public CAction DefaultAttackAction => m_DefaultAttackAction;

	public CAction DefaultMoveAction => m_DefaultMoveAction;

	public CAction BottomAction => m_BottomAction;

	public CAction SelectedAction => m_SelectedAction;

	public CAction LastSelectedAction => m_LastSelectedAction;

	public AbilityCardYMLData GetAbilityCardYML => m_AbilityCardYML;

	public int Level { get; private set; }

	public bool SupplyCard { get; private set; }

	public int CardInstanceID { get; private set; }

	public CAbilityCard(int initiative, int id, CAction defaultMoveAction, CAction defaultAttackAction, CAction topAction, CAction bottomAction, AbilityCardYMLData abilityCardYML, string classID, int? cardInstanceID = null)
		: base(initiative, id, classID, ECardType.CharacterAbility, id.ToString())
	{
		CAbilityCard cAbilityCard = this;
		m_DefaultMoveAction = defaultMoveAction;
		m_DefaultAttackAction = defaultAttackAction;
		m_TopAction = topAction;
		m_BottomAction = bottomAction;
		m_AbilityCardYML = abilityCardYML;
		if (int.TryParse(m_AbilityCardYML.Level, out var result))
		{
			Level = result;
		}
		else
		{
			Level = 1;
		}
		SupplyCard = m_AbilityCardYML.SupplyCard.Value;
		if (cardInstanceID.HasValue)
		{
			CardInstanceID = cardInstanceID.Value;
			return;
		}
		bool flag = false;
		int checkID = 0;
		int num = 0;
		while (!flag)
		{
			checkID = base.ID + num * 10000;
			num++;
			if (!CharacterClassManager.AllAbilityCardInstances.Any((CAbilityCard w) => w.ID == cAbilityCard.ID && w.CardInstanceID == checkID))
			{
				flag = true;
			}
		}
		CardInstanceID = checkID;
	}

	public CAbilityCard Copy(int? cardInstanceID = null)
	{
		return new CAbilityCard(base.Initiative, base.ID, DefaultMoveAction.Copy(), DefaultAttackAction.Copy(), TopAction.Copy(), BottomAction.Copy(), GetAbilityCardYML, base.ClassID, cardInstanceID);
	}

	public void SetSelectedAction(CAction action)
	{
		m_SelectedAction = action;
		if (action != null)
		{
			m_LastSelectedAction = m_SelectedAction;
		}
	}

	public override ActionType GetAbilityActionType(CAbility ability)
	{
		if (GetTopActionAbilities().Any((CAbility x) => x.ID == ability.ID))
		{
			return ActionType.TopAction;
		}
		if (GetBottomActionAbilities().Any((CAbility x) => x.ID == ability.ID))
		{
			return ActionType.BottomAction;
		}
		return ActionType.NA;
	}

	public CAction GetActionForType(ActionType type)
	{
		return type switch
		{
			ActionType.TopAction => m_TopAction, 
			ActionType.DefaultAttackAction => m_DefaultAttackAction, 
			ActionType.DefaultMoveAction => m_DefaultMoveAction, 
			ActionType.BottomAction => m_BottomAction, 
			_ => null, 
		};
	}

	public override void Reset()
	{
		base.Reset();
		TopAction.Reset();
		BottomAction.Reset();
		m_SelectedAction = null;
		m_LastSelectedAction = null;
	}

	public void ResetAbilitiesAndEnhancementsOnly()
	{
		TopAction.Reset();
		BottomAction.Reset();
	}

	public bool HasAbility(CAbility ability)
	{
		if (GetAllAbilities().Any((CAbility x) => x.HasID(ability.ID)))
		{
			return true;
		}
		if (TopAction.Augmentations.Any((CActionAugmentation x) => x.AugmentationOps.Any((CActionAugmentationOp y) => y.Ability != null && y.Ability.HasID(ability.ID))) || TopAction.Augmentations.Any((CActionAugmentation x) => x.AugmentationOps.Any((CActionAugmentationOp y) => y.AbilityOverride != null && y.AbilityOverride.SubAbilities != null && y.AbilityOverride.SubAbilities.Any((CAbility z) => z.HasID(ability.ID)))) || BottomAction.Augmentations.Any((CActionAugmentation x) => x.AugmentationOps.Any((CActionAugmentationOp y) => y.Ability != null && y.Ability.HasID(ability.ID))) || BottomAction.Augmentations.Any((CActionAugmentation x) => x.AugmentationOps.Any((CActionAugmentationOp y) => y.AbilityOverride != null && y.AbilityOverride.SubAbilities != null && y.AbilityOverride.SubAbilities.Any((CAbility z) => z.HasID(ability.ID)))))
		{
			return true;
		}
		return false;
	}

	public bool HasAbilityOfType(CAbility.EAbilityType abilityType)
	{
		return GetAllAbilities().Any((CAbility a) => a.AbilityType == abilityType);
	}

	public List<CAbility> GetAllAbilities()
	{
		return GetTopActionAbilities().Concat(GetBottomActionAbilities()).ToList();
	}

	public List<CAbility> GetTopActionAbilities()
	{
		return TopAction.Abilities.Concat(TopAction.Abilities.Where((CAbility w) => w is CAbilityControlActor).SelectMany((CAbility sm) => (sm as CAbilityControlActor).ControlActorData.ControlAbilities)).Concat(TopAction.Abilities.SelectMany((CAbility sm) => sm.SubAbilities).Concat(TopAction.Abilities.Where((CAbility w) => w.Augment != null && w.Augment.Abilities != null).SelectMany((CAbility sm) => sm.Augment.Abilities).Concat(TopAction.Abilities.Where((CAbility w) => w is CAbilityMerged).SelectMany((CAbility sm) => (sm as CAbilityMerged).MergedAbilities).Concat(TopAction.Abilities.Where((CAbility w) => w is CAbilityAddDoom).SelectMany((CAbility sm) => (sm as CAbilityAddDoom).Doom.DoomAbilities))))).ToList();
	}

	public List<CAbility> GetBottomActionAbilities()
	{
		return BottomAction.Abilities.Concat(BottomAction.Abilities.Where((CAbility w) => w is CAbilityControlActor).SelectMany((CAbility sm) => (sm as CAbilityControlActor).ControlActorData.ControlAbilities)).Concat(BottomAction.Abilities.SelectMany((CAbility sm) => sm.SubAbilities).Concat(BottomAction.Abilities.Where((CAbility w) => w.Augment != null && w.Augment.Abilities != null).SelectMany((CAbility sm) => sm.Augment.Abilities).Concat(BottomAction.Abilities.Where((CAbility w) => w is CAbilityMerged).SelectMany((CAbility sm) => (sm as CAbilityMerged).MergedAbilities).Concat(BottomAction.Abilities.Where((CAbility w) => w is CAbilityAddDoom).SelectMany((CAbility sm) => (sm as CAbilityAddDoom).Doom.DoomAbilities))))).ToList();
	}

	public CAction GetAction(CAbility ability)
	{
		if (GetTopActionAbilities().Any((CAbility a) => a.Name == ability.Name))
		{
			return TopAction;
		}
		if (GetBottomActionAbilities().Any((CAbility a) => a.Name == ability.Name))
		{
			return BottomAction;
		}
		return null;
	}

	private List<CAbility> GetRelatedAbilities(CAbility ability)
	{
		List<CAbility> list = ability.SubAbilities.ToList();
		if (ability is CAbilityControlActor cAbilityControlActor)
		{
			list.AddRange(cAbilityControlActor.ControlActorData.ControlAbilities);
		}
		if (ability.Augment?.Abilities != null)
		{
			list.AddRange(ability.Augment.Abilities);
		}
		if (ability is CAbilityMerged cAbilityMerged)
		{
			list.AddRange(cAbilityMerged.MergedAbilities);
		}
		return list;
	}

	public List<CAbility> GetActionAbilities(CAbility ability)
	{
		List<CAbility> list = new List<CAbility>();
		foreach (CAbility ability2 in GetAction(ability).Abilities)
		{
			list.Add(ability2);
			list.AddRange(GetRelatedAbilities(ability2));
		}
		return list;
	}

	public List<CEnhancement> GetAllEnhancements()
	{
		return TopAction.Abilities.SelectMany((CAbility sm) => sm.AbilityEnhancements).Concat(TopAction.Abilities.SelectMany((CAbility sm) => sm.SubAbilities).SelectMany((CAbility sm) => sm.AbilityEnhancements).Concat(BottomAction.Abilities.SelectMany((CAbility sm) => sm.AbilityEnhancements).Concat(BottomAction.Abilities.SelectMany((CAbility sm) => sm.SubAbilities).SelectMany((CAbility sm) => sm.AbilityEnhancements)))).ToList();
	}

	public CAbility FindAbilityOnCard(string name)
	{
		CAbility cAbility = TopAction.Abilities.SingleOrDefault((CAbility s) => s.Name == name);
		if (cAbility == null)
		{
			cAbility = BottomAction.Abilities.SingleOrDefault((CAbility s) => s.Name == name);
		}
		if (cAbility == null)
		{
			cAbility = TopAction.Abilities.SelectMany((CAbility sm) => sm.SubAbilities).SingleOrDefault((CAbility s) => s.Name == name);
		}
		if (cAbility == null)
		{
			cAbility = BottomAction.Abilities.SelectMany((CAbility sm) => sm.SubAbilities).SingleOrDefault((CAbility s) => s.Name == name);
		}
		if (cAbility == null)
		{
			cAbility = TopAction.Abilities.Where((CAbility w) => w.Augment != null && w.Augment.Abilities != null).SelectMany((CAbility sm) => sm.Augment.Abilities).SingleOrDefault((CAbility s) => s.Name == name);
		}
		if (cAbility == null)
		{
			cAbility = BottomAction.Abilities.Where((CAbility w) => w.Augment != null && w.Augment.Abilities != null).SelectMany((CAbility sm) => sm.Augment.Abilities).SingleOrDefault((CAbility s) => s.Name == name);
		}
		if (cAbility == null)
		{
			foreach (CAbility ability in TopAction.Abilities)
			{
				if (ability is CAbilityMerged cAbilityMerged)
				{
					cAbility = cAbilityMerged.MergedAbilities.SingleOrDefault((CAbility s) => s.Name == name);
				}
			}
		}
		if (cAbility == null)
		{
			foreach (CAbility ability2 in BottomAction.Abilities)
			{
				if (ability2 is CAbilityMerged cAbilityMerged2)
				{
					cAbility = cAbilityMerged2.MergedAbilities.SingleOrDefault((CAbility s) => s.Name == name);
				}
			}
		}
		if (cAbility == null)
		{
			foreach (CAbility ability3 in TopAction.Abilities)
			{
				if (ability3 is CAbilityControlActor cAbilityControlActor)
				{
					cAbility = cAbilityControlActor.ControlActorData.ControlAbilities.SingleOrDefault((CAbility s) => s.Name == name);
				}
			}
		}
		if (cAbility == null)
		{
			foreach (CAbility ability4 in BottomAction.Abilities)
			{
				if (ability4 is CAbilityControlActor cAbilityControlActor2)
				{
					cAbility = cAbilityControlActor2.ControlActorData.ControlAbilities.SingleOrDefault((CAbility s) => s.Name == name);
				}
			}
		}
		if (cAbility == null)
		{
			foreach (CAbility ability5 in TopAction.Abilities)
			{
				if (ability5 is CAbilityAddDoom cAbilityAddDoom)
				{
					cAbility = cAbilityAddDoom.Doom.DoomAbilities.SingleOrDefault((CAbility s) => s.Name == name);
				}
			}
		}
		if (cAbility == null)
		{
			foreach (CAbility ability6 in BottomAction.Abilities)
			{
				if (ability6 is CAbilityAddDoom cAbilityAddDoom2)
				{
					cAbility = cAbilityAddDoom2.Doom.DoomAbilities.SingleOrDefault((CAbility s) => s.Name == name);
				}
			}
		}
		return cAbility;
	}

	public List<CAbility> FindAbilityOfType(CAbility.EAbilityType abilityType)
	{
		return GetAllAbilities().FindAll((CAbility a) => a.AbilityType == abilityType);
	}

	public CAbilityCard()
	{
	}

	public CAbilityCard(CAbilityCard state, ReferenceDictionary references)
		: base(state, references)
	{
		Level = state.Level;
		SupplyCard = state.SupplyCard;
		CardInstanceID = state.CardInstanceID;
	}
}
