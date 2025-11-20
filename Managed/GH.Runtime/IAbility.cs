using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;

public interface IAbility
{
	string ID { get; }

	Sprite Icon { get; }

	List<CAbility> Abilities { get; }

	List<IInfuseElement> Infusions { get; }

	CItem DescriptionItem { get; }

	string DescriptionTitle { get; }

	string DescriptionText { get; }

	Tuple<IOptionHolder, List<IOption>> GenerateOption(UIUseOption optionUI);

	string GetSelectAudioItem();
}
