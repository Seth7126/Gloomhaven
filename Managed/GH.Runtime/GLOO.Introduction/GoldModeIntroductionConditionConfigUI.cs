using MapRuleLibrary.Adventure;
using MapRuleLibrary.State;
using UnityEngine;

namespace GLOO.Introduction;

public class GoldModeIntroductionConditionConfigUI : IntroductionConditionConfigUI
{
	[SerializeField]
	private EGoldMode mode;

	public override bool IsValid()
	{
		return AdventureState.MapState.GoldMode == mode;
	}
}
