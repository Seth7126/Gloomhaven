using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary.YML;

public class AttackModifierCard : CBaseCard
{
	public const string FONT_ATTACKMODSDOUBLE = "<b><color=#D92727>{0}</color></b>";

	public const string FONT_ATTACKMODSPOSITIVE = "<b><color=#FFA414>{0}</color></b>";

	public const string FONT_ATTACKMODSNEGATIVE = "<b><color=#A050E0>{0}</color></b>";

	private AttackModifierYMLData m_YMLData;

	public AttackModifierYMLData YMLData
	{
		get
		{
			if (m_YMLData == null)
			{
				m_YMLData = ScenarioRuleClient.SRLYML.AttackModifiers.FirstOrDefault((AttackModifierYMLData s) => s.Name == base.Name).Copy();
			}
			return m_YMLData;
		}
	}

	public bool Infuse
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityInfuse);
			}
			return false;
		}
	}

	public List<ElementInfusionBoardManager.EElement> InfuseElements
	{
		get
		{
			if (YMLData.Abilities == null)
			{
				return new List<ElementInfusionBoardManager.EElement>();
			}
			return YMLData.Abilities.Where((CAbility w) => w is CAbilityInfuse).SelectMany((CAbility s) => (s as CAbilityInfuse).ElementsToInfuse).ToList();
		}
	}

	public bool Shield
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityShield);
			}
			return false;
		}
	}

	public bool Pierce
	{
		get
		{
			if (YMLData.Overrides != null)
			{
				return YMLData.Overrides.Any((CAbilityOverride a) => a.Pierce.HasValue && a.Pierce > 0);
			}
			return false;
		}
	}

	public bool Heal
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityHeal && !a.AbilityFilter.FilterAlly());
			}
			return false;
		}
	}

	public bool HealAlly
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityHeal && a.AbilityFilter.FilterAlly());
			}
			return false;
		}
	}

	public int HealAmount
	{
		get
		{
			if (YMLData.Abilities == null)
			{
				return 0;
			}
			return YMLData.Abilities.Where((CAbility a) => a is CAbilityHeal).Sum((CAbility a) => a.Strength);
		}
	}

	public bool Damage
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityDamage);
			}
			return false;
		}
	}

	public int DamageAmount
	{
		get
		{
			if (YMLData.Abilities == null)
			{
				return 0;
			}
			return YMLData.Abilities.Where((CAbility a) => a is CAbilityDamage).Sum((CAbility a) => a.Strength);
		}
	}

	public bool Push
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityPush);
			}
			return false;
		}
	}

	public bool Push2
	{
		get
		{
			if (YMLData.Abilities != null && YMLData.Abilities.Any((CAbility a) => a is CAbilityPush))
			{
				return YMLData.Abilities.Where((CAbility a) => a is CAbilityPush).Sum((CAbility a) => a.Strength) > 1;
			}
			return false;
		}
	}

	public bool Pull
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityPull);
			}
			return false;
		}
	}

	public List<CCondition.ENegativeCondition> NegativeConditions
	{
		get
		{
			if (YMLData.Overrides == null)
			{
				return new List<CCondition.ENegativeCondition>();
			}
			return YMLData.Overrides.Where((CAbilityOverride w) => w.NegativeConditions != null).SelectMany((CAbilityOverride s) => s.NegativeConditions).ToList();
		}
	}

	public List<CCondition.EPositiveCondition> PositiveConditions
	{
		get
		{
			if (YMLData.Overrides == null)
			{
				return new List<CCondition.EPositiveCondition>();
			}
			return YMLData.Overrides.Where((CAbilityOverride w) => w.PositiveConditions != null).SelectMany((CAbilityOverride s) => s.PositiveConditions).ToList();
		}
	}

	public bool HasAdditionalEffect
	{
		get
		{
			if ((YMLData.Abilities == null || YMLData.Abilities.Count <= 0) && (YMLData.Overrides == null || YMLData.Overrides.Count <= 0))
			{
				return YMLData.AddTarget;
			}
			return true;
		}
	}

	public AttackModifierCard(string name)
		: base(name.GetHashCode(), ECardType.AttackModifier, name)
	{
	}

	public string AttackModifierLogString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		bool flag2 = false;
		if (YMLData.Overrides.Any((CAbilityOverride a) => a.NegativeConditions != null && a.NegativeConditions.Count > 0))
		{
			foreach (CCondition.ENegativeCondition item in YMLData.Overrides.Where((CAbilityOverride w) => w.NegativeConditions != null).SelectMany((CAbilityOverride s) => s.NegativeConditions))
			{
				stringBuilder.Append("<sprite name=" + item.ToString() + "> ");
			}
			flag = true;
		}
		if (YMLData.Overrides.Any((CAbilityOverride a) => a.PositiveConditions != null && a.PositiveConditions.Count > 0))
		{
			foreach (CCondition.EPositiveCondition item2 in YMLData.Overrides.Where((CAbilityOverride w) => w.PositiveConditions != null).SelectMany((CAbilityOverride s) => s.PositiveConditions))
			{
				stringBuilder.Append("<sprite name=" + item2.ToString() + "> ");
			}
			flag = true;
		}
		if (YMLData.IsBless)
		{
			stringBuilder.Append("<sprite name=Bless> ");
		}
		if (YMLData.IsCurse)
		{
			stringBuilder.Append("<sprite name=Curse> ");
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityInfuse))
		{
			foreach (ElementInfusionBoardManager.EElement item3 in YMLData.Abilities.Where((CAbility w) => w is CAbilityInfuse).SelectMany((CAbility s) => (s as CAbilityInfuse).ElementsToInfuse))
			{
				stringBuilder.Append("<sprite name=" + item3.ToString() + "> ");
			}
			flag = true;
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityHeal && a.AbilityFilter.FilterAlly()))
		{
			stringBuilder.Append(YMLData.MathModifier + " " + YMLData.Abilities.Where((CAbility a) => a is CAbilityHeal).Sum((CAbility a) => a.Strength) + "<sprite name=Heal>");
			flag = true;
			flag2 = true;
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityHeal && !a.AbilityFilter.FilterAlly()))
		{
			stringBuilder.Append("<sprite name=Heal> ");
			flag = true;
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityDamage))
		{
			stringBuilder.Append(YMLData.MathModifier + " " + YMLData.Abilities.Where((CAbility a) => a is CAbilityDamage).Sum((CAbility a) => a.Strength) + "<sprite name=Damage> ");
			flag = true;
			flag2 = true;
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityShield))
		{
			stringBuilder.Append("<sprite name=Shield> ");
			flag = true;
		}
		if (YMLData.Overrides.Any((CAbilityOverride a) => a.Pierce.HasValue && a.Pierce > 0))
		{
			stringBuilder.Append("<sprite name=Pierce> ");
			flag = true;
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityPush))
		{
			stringBuilder.Append("<sprite name=Push> ");
			flag = true;
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityPull))
		{
			stringBuilder.Append("<sprite name=Pull> ");
			flag = true;
		}
		if (YMLData.AddTarget)
		{
			stringBuilder.Append("<sprite name=AddTarget> ");
			flag = true;
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityRefreshItemCards))
		{
			stringBuilder.Append("<sprite name=Refresh> ");
			flag = true;
		}
		if (YMLData.MathModifier == "+0")
		{
			if (!flag && !flag2)
			{
				stringBuilder.Append(ApplyModifierFont(YMLData.MathModifier));
			}
		}
		else if (!flag2)
		{
			stringBuilder.Append(ApplyModifierFont(YMLData.MathModifier));
		}
		if (YMLData.Rolling)
		{
			stringBuilder.Append(" <sprite name=Rolling>");
			flag = true;
		}
		if (YMLData.Shuffle)
		{
			stringBuilder.Append(" <sprite name=Shuffle>");
			flag = true;
		}
		return stringBuilder.ToString();
	}

	private string ApplyModifierFont(string modifier)
	{
		switch (modifier)
		{
		case "*0":
			return string.Format("<b><color=#A050E0>{0}</color></b>", "x0");
		case "-2":
		case "-1":
			return $"<b><color=#A050E0>{modifier}</color></b>";
		case "+0":
		case "+1":
		case "+2":
		case "+3":
		case "+4":
			return $"<b><color=#FFA414>{modifier}</color></b>";
		case "*2":
			return string.Format("<b><color=#D92727>{0}</color></b>", "x2");
		default:
			DLLDebug.Log("Unexpected modifier type for combat log: " + modifier);
			return $"<b><color=#FFA414>{modifier}</color></b>";
		}
	}

	public AttackModifierCard()
	{
	}

	public AttackModifierCard(AttackModifierCard state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
