using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using UnityEngine;

namespace GLOO.Introduction;

public class DLLModeIntroductionConditionConfigUI : IntroductionConditionConfigUI
{
	[SerializeField]
	private ScenarioManager.EDLLMode mode;

	public override bool IsValid()
	{
		return AdventureState.MapState.DLLMode == mode;
	}
}
