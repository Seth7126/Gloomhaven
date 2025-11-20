using MapRuleLibrary.Adventure;
using UnityEngine;

namespace GLOO.Introduction;

public class GuildmasterFTUEIntroductionConditionConfigUI : IntroductionConditionConfigUI
{
	[SerializeField]
	private bool completed;

	public override bool IsValid()
	{
		return AdventureState.MapState.IntroCompleted == completed;
	}
}
