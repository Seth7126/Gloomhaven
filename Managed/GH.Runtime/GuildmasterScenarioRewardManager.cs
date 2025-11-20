using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

public class GuildmasterScenarioRewardManager : ScenarioRewardManager
{
	public override bool IsShown => Singleton<UIRewardsManager>.Instance.IsShown;

	public override void Show(CActor actor, List<Reward> rewards, Action onFinish)
	{
		Singleton<UIRewardsManager>.Instance.StartRewardsShowcase(rewards, hideBlackOverlayInstantly: false, networkProcessIfServer: true, onFinish, () => !FFSNetwork.IsOnline || actor.IsUnderMyControl);
	}
}
