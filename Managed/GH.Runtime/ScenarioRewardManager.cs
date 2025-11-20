using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

public abstract class ScenarioRewardManager : Singleton<ScenarioRewardManager>
{
	public abstract bool IsShown { get; }

	public abstract void Show(CActor actor, List<Reward> rewards, Action onFinish);
}
