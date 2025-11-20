using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;

public class EnhancementLineFilter
{
	private EnhancementLine current;

	private List<EnhancementLine> filters;

	private Action<EnhancementLine> OnChangedFilter;

	public EnhancementLine CurrentFilter => current;

	public EnhancementLineFilter(Action<EnhancementLine> onChangedFilter)
	{
		OnChangedFilter = onChangedFilter;
	}

	public void SetFilters(List<EnhancementLine> filters, EnhancementLine current)
	{
		this.filters = filters;
		this.current = current;
		OnChangedFilter?.Invoke(current);
	}

	public void SelectFilter(CAbility ability, EEnhancementLine line)
	{
		EnhancementLine enhancementLine = filters.First((EnhancementLine it) => it.Ability == ability && it.Line == line);
		if (current != enhancementLine)
		{
			current = enhancementLine;
			OnChangedFilter?.Invoke(current);
		}
	}
}
