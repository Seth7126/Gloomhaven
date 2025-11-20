using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using Photon.Bolt;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using Script.GUI.SMNavigation.States.PopupStates;
using UnityEngine;

public class ItemRewardLosePicker : Singleton<ItemRewardLosePicker>
{
	[SerializeField]
	private ItemCardPicker picker;

	private Color defaultColorConfirmText;

	private List<RewardGroup> rewardGroupsToCheck;

	private List<Reward> rewardsWithItems;

	private List<CItem> itemsToChooseFrom;

	public bool CanSelect
	{
		get
		{
			if (FFSNetwork.IsOnline)
			{
				return FFSNetwork.IsHost;
			}
			return true;
		}
	}

	public void Show(int rewardsToLose = 1)
	{
		rewardGroupsToCheck = new List<RewardGroup>();
		rewardsWithItems = new List<Reward>();
		foreach (Tuple<string, RewardGroup> goalChestReward in ScenarioManager.CurrentScenarioState.GoalChestRewards)
		{
			rewardGroupsToCheck.Add(goalChestReward.Item2);
			foreach (Reward reward in goalChestReward.Item2.Rewards)
			{
				if (reward.Item != null)
				{
					rewardsWithItems.Add(reward);
				}
			}
		}
		itemsToChooseFrom = rewardsWithItems.Select((Reward x) => x.Item).ToList();
		var (hintTitle, hintMessage) = GetHintMessage();
		picker.Show(PopupStateTag.PickItemToLose, itemsToChooseFrom, ConfirmSelectedRewardItems, CanSelect, rewardsToLose, hintTitle, hintMessage);
	}

	private (string, string) GetHintMessage()
	{
		if (CanSelect)
		{
			return (null, LocalizationManager.GetTranslation("GUI_CHOOSE_ITEM_TO_LOSE"));
		}
		return (LocalizationManager.GetTranslation("Consoles/GUI_MULTIPLAYER_TIP_TITLE"), LocalizationManager.GetTranslation("Consoles/GUI_WAIT_FOR_HOST_TIP"));
	}

	private void ConfirmSelectedRewardItems()
	{
		picker.Hide();
		List<int> list = new List<int>();
		foreach (CItem currentSelectedItem in picker.GetCurrentSelectedItems())
		{
			int num = itemsToChooseFrom.IndexOf(currentSelectedItem);
			list.Add(num);
			Reward item = rewardsWithItems[num];
			foreach (RewardGroup item2 in rewardGroupsToCheck)
			{
				item2.Rewards.Remove(item);
			}
		}
		if (FFSNetwork.IsOnline)
		{
			IProtocolToken supplementaryDataToken = new IndexToken(list);
			Synchronizer.SendGameAction(GameActionType.LoseItemReward, ActionPhaseType.LosingItemRewards, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
		}
		Finish();
	}

	private void Finish()
	{
		if (Choreographer.s_Choreographer.m_WaitState.m_State == Choreographer.ChoreographerStateType.WaitingForLoseGoalChestRewardSelection)
		{
			Choreographer.s_Choreographer.SetChoreographerState(Choreographer.ChoreographerStateType.Play, 0, null);
			ScenarioRuleClient.StepComplete();
		}
	}

	public void ProxyItemRewardLose(GameAction action)
	{
		picker.Hide();
		Choreographer.s_Choreographer.FindPlayerActor(action.ActorID);
		int[] indexes = ((IndexToken)action.SupplementaryDataToken).Indexes;
		foreach (int index in indexes)
		{
			CItem item = itemsToChooseFrom[index];
			int index2 = itemsToChooseFrom.IndexOf(item);
			Reward item2 = rewardsWithItems[index2];
			foreach (RewardGroup item3 in rewardGroupsToCheck)
			{
				item3.Rewards.Remove(item2);
			}
		}
		Finish();
	}
}
