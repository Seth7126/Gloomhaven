using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Logger;

namespace ScenarioRuleLibrary;

public class CardLayoutRow
{
	public const char PropertyKeySymbol = '*';

	public const char EnhancementSlotsKeySymbol = '@';

	public const char AreaEffectKeySymbol = '£';

	public const char LookupKeySymbol = '$';

	public const char ActivePropertyKeySymbol = '^';

	public List<CAbility> linkedAbilities;

	public List<CAbilityOverride> linkedOverrides;

	public string originalRowText;

	private string localisedRowText;

	private string cardName;

	public List<CEnhancement> AbilityEnhancements { get; private set; }

	public EEnhancementLine EnhancementLine { get; private set; }

	public bool IsArea { get; private set; }

	public string AbilityName { get; private set; }

	public string AbilityNameLookupText { get; private set; }

	public CardLayoutRow(string rowText, string cardName, List<CAbility> linked, List<CAbilityOverride> overrides = null)
	{
		originalRowText = rowText;
		this.cardName = cardName;
		linkedAbilities = linked;
		linkedOverrides = overrides;
		AbilityEnhancements = new List<CEnhancement>();
		if (rowText.Contains('£'))
		{
			IsArea = true;
		}
		else
		{
			IsArea = false;
		}
		localisedRowText = string.Empty;
		EnhancementLine = EEnhancementLine.Mainline;
	}

	public void SetLocalisedText(string localisedText)
	{
		localisedRowText = localisedText;
	}

	private string GetLinkedAbilityAreaEffectString(CAbility ability)
	{
		AbilityName = ability.Name;
		if (ability.AreaEffectLayoutOverrideYMLString != null && ability.AreaEffectLayoutOverrideYMLString.Length > 0)
		{
			if (AbilityEnhancements.Count == 0)
			{
				AbilityEnhancements.AddRange(ability.AbilityEnhancements);
			}
			return ability.AreaEffectLayoutOverrideYMLString;
		}
		if (ability.AreaEffectYMLString != null && ability.AreaEffectYMLString.Length > 0)
		{
			if (AbilityEnhancements.Count == 0)
			{
				AbilityEnhancements.AddRange(ability.AbilityEnhancements);
			}
			return ability.AreaEffectYMLString;
		}
		return null;
	}

	public string Text()
	{
		string empty = string.Empty;
		empty = ((localisedRowText.Length <= 0) ? originalRowText : localisedRowText);
		if (IsArea)
		{
			AbilityNameLookupText = "ATTACK_AREA";
			EnhancementLine = EEnhancementLine.AreaHex;
			string text = null;
			if (linkedAbilities != null)
			{
				foreach (CAbility linkedAbility in linkedAbilities)
				{
					text = GetLinkedAbilityAreaEffectString(linkedAbility);
					if (text != null)
					{
						break;
					}
					if (!(linkedAbility is CAbilityControlActor cAbilityControlActor))
					{
						continue;
					}
					foreach (CAbility controlAbility in cAbilityControlActor.ControlActorData.ControlAbilities)
					{
						text = GetLinkedAbilityAreaEffectString(controlAbility);
						if (text != null)
						{
							break;
						}
					}
				}
			}
			if (linkedOverrides != null)
			{
				foreach (CAbilityOverride linkedOverride in linkedOverrides)
				{
					if (linkedOverride.AreaEffectLayoutOverrideYMLString != null && linkedOverride.AreaEffectLayoutOverrideYMLString.Length > 0)
					{
						text = linkedOverride.AreaEffectLayoutOverrideYMLString;
					}
					else if (linkedOverride.AreaEffectYMLString != null && linkedOverride.AreaEffectYMLString.Length > 0)
					{
						text = linkedOverride.AreaEffectYMLString;
					}
					if (text != null)
					{
						break;
					}
				}
			}
			if (text == null)
			{
				GetKey(empty, '£');
			}
			return text;
		}
		if (empty.Contains('$'))
		{
			AbilityNameLookupText = GetKey(empty, '$');
		}
		empty = PropertyLookup(empty, linkedAbilities);
		if (empty.Contains('@'))
		{
			empty = ReplaceKey(empty, '@', "");
		}
		return empty;
	}

	public string PropertyLookup(string text, List<CAbility> linkedAbilities)
	{
		while (text.Contains('*'.ToString()))
		{
			string text2 = GetKey(text, '*');
			string defaultValue = "0";
			string subtype = string.Empty;
			if (text2.Contains(","))
			{
				string[] array = text2.Split(',');
				text2 = array[0];
				defaultValue = array[1];
			}
			if (text2.Contains("."))
			{
				string[] array2 = text2.Split('.');
				text2 = array2[0];
				subtype = array2[1];
			}
			text = ReplaceKey(text, '*', GetAbilityAmount(text2, FindAbilityByName(text2, linkedAbilities, cardName), defaultValue, subtype));
		}
		return text;
	}

	public static CAbility FindAbilityByName(string abilityName, List<CAbility> linkedAbilities, string cardName = "")
	{
		foreach (CAbility linkedAbility in linkedAbilities)
		{
			CAbility cAbility = linkedAbility.FindAbility(abilityName);
			if (cAbility != null)
			{
				return cAbility;
			}
		}
		DLLDebug.LogError(cardName + ": Unable to find ability " + abilityName);
		return null;
	}

	public void SetEnhancements()
	{
		if (AbilityEnhancements.Count != 0)
		{
			return;
		}
		string text = PropertyLookup(originalRowText, linkedAbilities);
		if (text.Contains('@'.ToString()))
		{
			string text2 = GetKey(text, '@');
			if (text2.Contains("."))
			{
				text2 = text2.Split('.')[0];
			}
			CAbility cAbility = FindAbilityByName(text2, linkedAbilities);
			if (cAbility != null)
			{
				AbilityEnhancements.AddRange(cAbility.AbilityEnhancements);
			}
			else
			{
				DLLDebug.LogError(cardName + ": Unable to find ability with name " + text2 + " for enhancements");
			}
		}
	}

	private string GetAbilityAmount(string abilityName, CAbility ability, string defaultValue, string subtype)
	{
		if (ability != null)
		{
			if (ability.AbilityType == CAbility.EAbilityType.AddActiveBonus)
			{
				ability = (ability as CAbilityAddActiveBonus).AddAbility;
			}
			if (ability.AbilityType == CAbility.EAbilityType.Loot)
			{
				return ability.Range.ToString();
			}
			if (ability.AbilityType == CAbility.EAbilityType.Push && ability.IsSubAbility)
			{
				EnhancementLine = EEnhancementLine.Push;
				int num = 0;
				if (ability is CAbilityPush cAbilityPush)
				{
					num = cAbilityPush.Strength;
				}
				return num.ToString();
			}
			if (ability.AbilityType == CAbility.EAbilityType.Pull && ability.IsSubAbility)
			{
				EnhancementLine = EEnhancementLine.Pull;
				int num2 = 0;
				if (ability is CAbilityPull cAbilityPull)
				{
					num2 = cAbilityPull.Strength;
				}
				return num2.ToString();
			}
			int num3 = 0;
			switch (subtype)
			{
			case "Range":
				num3 = ability.Range;
				EnhancementLine = EEnhancementLine.Range;
				break;
			case "Target":
				num3 = ability.NumberTargets;
				EnhancementLine = EEnhancementLine.Targets;
				break;
			case "Pierce":
				EnhancementLine = EEnhancementLine.Pierce;
				if (ability is CAbilityAttack cAbilityAttack)
				{
					num3 = cAbilityAttack.Pierce;
				}
				break;
			case "RetaliateRange":
				EnhancementLine = EEnhancementLine.RetaliateRange;
				if (ability is CAbilityRetaliate cAbilityRetaliate)
				{
					num3 = cAbilityRetaliate.RetaliateRange;
				}
				break;
			default:
				EnhancementLine = EEnhancementLine.Mainline;
				num3 = ability.Strength;
				break;
			}
			return num3.ToString();
		}
		DLLDebug.LogError(cardName + ": Unable to find ability " + abilityName + ".  Using the default value to set the strength for this ability");
		return defaultValue;
	}

	private string GetEnhancementIcons(string abilityName, CAbility ability, string subtype)
	{
		string text = string.Empty;
		AbilityName = abilityName;
		if (ability != null)
		{
			switch (subtype)
			{
			case "Range":
				foreach (EEnhancement item in from s in ability.AbilityEnhancements
					where s.EnhancementLine == EEnhancementLine.Range
					select s.Enhancement)
				{
					text = text + "<sprite name=" + item.ToString() + "> ";
				}
				break;
			case "Target":
				foreach (EEnhancement item2 in from s in ability.AbilityEnhancements
					where s.EnhancementLine == EEnhancementLine.Targets
					select s.Enhancement)
				{
					text = text + "<sprite name=" + item2.ToString() + "> ";
				}
				break;
			case "Pierce":
				foreach (EEnhancement item3 in from s in ability.AbilityEnhancements
					where s.EnhancementLine == EEnhancementLine.Pierce
					select s.Enhancement)
				{
					text = text + "<sprite name=" + item3.ToString() + "> ";
				}
				break;
			default:
				foreach (EEnhancement item4 in from s in ability.AbilityEnhancements
					where s.EnhancementLine == EEnhancementLine.Mainline
					select s.Enhancement)
				{
					text = text + "<sprite name=" + item4.ToString() + "> ";
				}
				break;
			}
			return text.TrimEnd();
		}
		DLLDebug.LogError(cardName + ": Unable to find ability " + abilityName + ".  Unable to set enhancement icons.");
		return string.Empty;
	}

	public static string GetKey(string word, char symbol)
	{
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < word.Length; i++)
		{
			if (word[i] == symbol)
			{
				if (num >= 0)
				{
					num2 = i;
					break;
				}
				num = i;
			}
		}
		num++;
		return word.Substring(num, num2 - num);
	}

	public static string ReplaceKey(string text, char symbol, string replaceWith)
	{
		int num = text.IndexOf(symbol);
		return text.Remove(num, text.Substring(num + 1).IndexOf(symbol) + 2).Insert(num, replaceWith);
	}

	private static string RemoveKey(string word, char symbol)
	{
		return word.Remove(word.IndexOf(symbol), word.Substring(word.IndexOf(symbol) + 1).IndexOf(symbol) + 2);
	}
}
