using System;
using System.Collections.Generic;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class UICampaignAdventureRewardsManager : UIAdventureRewardsManager
{
	[SerializeField]
	private CampaignRewardsManager manager;

	private bool isShowingRewards;

	public override bool IsShowingRewards => isShowingRewards;

	public override void ShowRewards(List<Reward> rewards, string title, string closeKey, Action onClosed = null)
	{
		isShowingRewards = true;
		manager.ShowRewards(rewards, title, closeKey, delegate
		{
			isShowingRewards = false;
			onClosed?.Invoke();
		});
	}
}
