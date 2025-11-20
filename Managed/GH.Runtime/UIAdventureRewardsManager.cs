using System;
using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

public abstract class UIAdventureRewardsManager : Singleton<UIAdventureRewardsManager>
{
	public abstract bool IsShowingRewards { get; }

	public abstract void ShowRewards(List<Reward> rewards, string title, string closeKey, Action onClosed = null);
}
