using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;

public static class EnhancementUtils
{
	public static List<EEnhancement> GetEnhancementsCompatibleWithAbility(AbilityCardUI cardGO, CAbility ability)
	{
		return GetEnhancementButtons(cardGO, enhanced: false, ability).SelectMany(GetEnhancementsCompatibleWithEnhancementButton).Distinct().ToList();
	}

	public static List<EEnhancement> GetEnhancementsCompatibleWithEnhancementButton(this EnhancementButtonBase button)
	{
		if (!InputManager.GamePadInUse && (InputManager.GamePadInUse || button.Enhancement.Enhancement != EEnhancement.NoEnhancement))
		{
			return new List<EEnhancement>();
		}
		List<EEnhancement> list = CAbility.GetValidEnhancements(button.Enhancement.Ability.AbilityType, button.Enhancement.Ability.PositiveConditions, button.Enhancement.Ability.NegativeConditions, button.Enhancement.EnhancementLine);
		if (InputManager.GamePadInUse)
		{
			list = list.Where((EEnhancement enhancement) => enhancement != button.Enhancement.Enhancement).ToList();
		}
		return list;
	}

	public static List<EnhancementSlot> GetEnhancementsCompatibleWithEnhancementButtonLine(this EnhancementLine line)
	{
		IEnumerable<EnhancementSlot> source = line.EnhancementSlots.SelectMany((EnhancementButtonBase button) => from it in button.GetEnhancementsCompatibleWithEnhancementButton()
			select new EnhancementSlot(button, it, buyMode: true, line));
		if (InputManager.GamePadInUse)
		{
			source = from enhancement in source
				group enhancement by enhancement.enhancement into grouping
				select grouping.First();
		}
		return source.ToList();
	}

	public static EnhancementButtonBase GetFreeEnhancementButtonForEnhancement(AbilityCardUI cardGO, CAbility ability, EEnhancement enhancement)
	{
		foreach (EnhancementButtonBase item in from it in GetEnhancementButtons(cardGO, enhanced: false, ability)
			orderby it.EnhancementSlot
			select it)
		{
			if (item.GetEnhancementsCompatibleWithEnhancementButton().Contains(enhancement))
			{
				return item;
			}
		}
		return null;
	}

	public static List<EnhancementButtonBase> GetEnhancementButtons(AbilityCardUI cardGO, bool enhanced, CAbility ability, EEnhancementLine line)
	{
		return cardGO.EnhancementElements.All.Where((EnhancementButtonBase it) => it.EnhancementLine == line && it.CanBeEnhanced(enhanced) && (ability == null || it.Enhancement.Ability.ID == ability.ID)).ToList();
	}

	public static EnhancementButtonBase GetEnhancementButton(AbilityCardUI cardGO, bool enhanced, string abilityName, EEnhancementLine line, int slot)
	{
		return cardGO.EnhancementElements.All.FirstOrDefault((EnhancementButtonBase it) => it.EnhancementLine == line && it.EnhancementSlot == slot && it.CanBeEnhanced(enhanced) && (abilityName == null || it.Enhancement.Ability.Name == abilityName));
	}

	public static List<EnhancementButtonBase> GetEnhancementButtons(AbilityCardUI cardGO, bool enhanced, CAbility ability = null)
	{
		return (from it in cardGO.GetEnhancementButtons(enhanced)
			where ability == null || it.Enhancement.Ability.ID == ability.ID
			select it).ToList();
	}

	public static List<EnhancementLine> GetEnhancementLines(AbilityCardUI cardGO, bool enhanced)
	{
		List<EnhancementLine> list = new List<EnhancementLine>();
		if (cardGO == null)
		{
			return list;
		}
		foreach (var item in from it in cardGO.GetEnhancementButtons(enhanced)
			group it by new
			{
				it.EnhancementLine,
				it.Enhancement.Ability
			})
		{
			list.Add(new EnhancementLine(item.Key.EnhancementLine, item.Key.Ability, item.ToList()));
		}
		return list;
	}

	private static IEnumerable<EnhancementButtonBase> GetEnhancementButtons(this AbilityCardUI cardGO, bool enhanced)
	{
		IEnumerable<EnhancementButtonBase> enumerable = cardGO.EnhancementElements.All;
		if (!InputManager.GamePadInUse)
		{
			enumerable = enumerable.Where((EnhancementButtonBase it) => it.CanBeEnhanced(enhanced));
		}
		return enumerable;
	}

	public static EnhancementButtonBase GetEnhancementButton(AbilityCardUI cardGO, CAbility ability, EEnhancement enhancement)
	{
		return (from it in cardGO.EnhancementElements.All
			where it.Enhancement.Enhancement == enhancement && (ability == null || it.Enhancement.Ability.ID == ability.ID)
			orderby it.Enhancement.EnhancementSlot descending
			select it).FirstOrDefault();
	}

	public static EnhancementButtonBase GetEnhancementButton(List<EnhancementButtonBase> buttons, EEnhancement enhancement)
	{
		return (from it in buttons
			where it.Enhancement.Enhancement == enhancement
			orderby it.Enhancement.EnhancementSlot descending
			select it).FirstOrDefault();
	}

	public static List<CAbility> GetAbilitiesWithEnhancements(AbilityCardUI cardGO, bool enhanced)
	{
		return (from it in cardGO.GetEnhancementButtons(enhanced)
			select it.Enhancement.Ability).Distinct().ToList();
	}

	public static bool CanBeEnhanced(AbilityCardUI card)
	{
		return card.EnhancementElements.All.Count > 0;
	}

	public static bool HaveEnhancementButtons(AbilityCardUI card, bool enhanced)
	{
		return card.GetEnhancementButtons(enhanced).Any();
	}

	public static int GetEnhancedCount(this CardEnhancementElements enhancementElements)
	{
		return enhancementElements.All.Count((EnhancementButtonBase it) => it.Enhancement.Enhancement != EEnhancement.NoEnhancement);
	}

	public static bool CanBeEnhanced(this EnhancementButtonBase button, bool enhanced)
	{
		return enhanced ^ (button.Enhancement.Enhancement == EEnhancement.NoEnhancement);
	}
}
