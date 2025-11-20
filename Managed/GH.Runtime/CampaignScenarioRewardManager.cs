#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Utils.Extensions;
using FFSNet;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class CampaignScenarioRewardManager : ScenarioRewardManager
{
	[SerializeField]
	private CampaignRewardsManager manager;

	private bool isShown;

	public override bool IsShown => isShown;

	public override void Show(CActor actor, List<Reward> rewards, Action onFinish)
	{
		if (rewards.Count == 0)
		{
			onFinish?.Invoke();
			return;
		}
		ActionPhaseType previousPhase = ActionProcessor.CurrentPhase;
		isShown = true;
		manager.ShowRewards(rewards, rewards.All((Reward it) => it.IsNegative()) ? ("<color=#" + UIInfoTools.Instance.warningColor.ToHex() + ">" + LocalizationManager.GetTranslation("GUI_REWARD_TRAP") + "</color>") : LocalizationManager.GetTranslation("GUI_REWARD_TREASURE"), "GUI_CONTINUE", delegate
		{
			isShown = false;
			onFinish?.Invoke();
			if (FFSNetwork.IsOnline)
			{
				if (actor.IsUnderMyControl)
				{
					Synchronizer.SendGameAction(GameActionType.ConfirmReward, ActionPhaseType.ScenarioReward);
				}
				ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, previousPhase);
			}
		}, showAppliedEffects: false, FFSNetwork.IsOnline ? ((Func<bool>)(() => !FFSNetwork.IsOnline || actor.IsUnderMyControl)) : null);
		if (FFSNetwork.IsOnline)
		{
			ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.ScenarioReward);
		}
	}

	public void ProxyConfirm(GameAction gameAction)
	{
		Debug.Log("ProxyConfirmRewards " + IsShown);
		if (isShown)
		{
			manager.Confirm();
		}
	}
}
