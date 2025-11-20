#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Misc;
using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.State;
using ScenarioRuleLibrary.YML;
using Script.GUI;
using UnityEngine;

public class UIDistributeRewardManager : Singleton<UIDistributeRewardManager>
{
	[SerializeField]
	private List<DistributeRewardProcess> processes;

	[SerializeField]
	private PanelHotkeyContainer _hotkeyContainer;

	public PanelHotkeyContainer HotkeyContainer => _hotkeyContainer;

	public bool IsDistributing { get; private set; }

	public void Process(List<Reward> rewards, Action onFinished, Action onStartDistribution = null, bool readyAllPlayers = true)
	{
		if (rewards.IsNullOrEmpty())
		{
			IsDistributing = false;
			onFinished?.Invoke();
			return;
		}
		IsDistributing = true;
		if (FFSNetwork.IsOnline && readyAllPlayers)
		{
			Debug.LogGUI("Initialize ready up rewards " + Singleton<UIReadyToggle>.Instance.PlayersReady.Count);
			Singleton<UIMapMultiplayerController>.Instance.ShowRewardsMultiplayer(delegate
			{
				ProcessSP(rewards, onFinished, onStartDistribution);
			});
		}
		else
		{
			ProcessSP(rewards, onFinished, onStartDistribution);
		}
	}

	private void ProcessSP(List<Reward> rewards, Action onFinished, Action onStartDistribution = null)
	{
		if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
		{
			List<Reward> rewardsIndividual = rewards.FindAll((Reward it) => !processes.Exists((DistributeRewardProcess process) => process.IsRewardToProcess(it)));
			AdventureState.MapState.ApplyRewards(rewardsIndividual, "party");
			rewards.RemoveAll((Reward it) => rewardsIndividual.Contains(it));
			if (rewards.Count == 0)
			{
				IsDistributing = false;
				onFinished?.Invoke();
				return;
			}
			onStartDistribution?.Invoke();
			Distribute(rewards).Done(delegate
			{
				IsDistributing = false;
				onFinished?.Invoke();
			});
		}
		else
		{
			AdventureState.MapState.ApplyRewards(rewards, "party");
			IsDistributing = false;
			onFinished?.Invoke();
		}
	}

	private ICallbackPromise Distribute(List<Reward> rewards)
	{
		if (rewards.IsNullOrEmpty())
		{
			return CallbackPromise.Resolved();
		}
		NewPartyDisplayUI.PartyDisplay.EnableCharacterSelection(enable: false, this);
		NewPartyDisplayUI.PartyDisplay.DisableEditStats(this, closeWindowsOnDisable: true);
		Singleton<UIGuildmasterHUD>.Instance.DisableEditGoldMode(this);
		NewPartyDisplayUI.PartyDisplay.Hide(this, instant: true);
		Singleton<UIGuildmasterHUD>.Instance.Hide(this);
		Singleton<QuestManager>.Instance.HideLogScreen(this);
		Singleton<AdventureMapUIManager>.Instance.LockOptionsInteraction(locked: true, this, blur: true);
		ICallbackPromise callbackPromise = CallbackPromise.Resolved();
		foreach (DistributeRewardProcess process in GetOrderProcesses(rewards))
		{
			callbackPromise = callbackPromise.Then(() => process.Process(rewards));
		}
		return callbackPromise.Then(delegate
		{
			rewards.Clear();
			Singleton<UIGuildmasterHUD>.Instance.EnableEditGoldMode(this);
			NewPartyDisplayUI.PartyDisplay.EnableCharacterSelection(enable: true, this);
			Singleton<AdventureMapUIManager>.Instance.LockOptionsInteraction(locked: false, this);
			NewPartyDisplayUI.PartyDisplay.EnableEditStats(this, closeWindowsOnEnable: true);
			NewPartyDisplayUI.PartyDisplay.Show(this);
			Singleton<UIGuildmasterHUD>.Instance.Show(this);
			Singleton<QuestManager>.Instance.ShowLogScreen(this);
		});
	}

	private IEnumerable<DistributeRewardProcess> GetOrderProcesses(List<Reward> rewards)
	{
		List<Reward> lastRewards = rewards.FindAll((Reward it) => it.TreasureDistributionRestrictionType == ETreasureDistributionRestrictionType.ExcludePreviousSelectedCharacter);
		if (lastRewards.IsNullOrEmpty())
		{
			return processes;
		}
		List<DistributeRewardProcess> lastprocess = processes.FindAll((DistributeRewardProcess it) => lastRewards.Exists(it.IsRewardToProcess));
		return processes.OrderBy((DistributeRewardProcess it) => lastprocess.Contains(it) ? 1 : 0);
	}

	public void ProxyConfirmClick(GameAction action)
	{
		try
		{
			DistributeRewardProcess.EDistributeRewardProcessType type = (DistributeRewardProcess.EDistributeRewardProcessType)action.SupplementaryDataIDMin;
			processes.Single((DistributeRewardProcess s) => s.GetRewardType() == type).ProxyConfirmClick();
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to process UIDistributeRewardsManager.ProxyConfirmClick.  SupplementaryDataIDMin: " + action.SupplementaryDataIDMin + "\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void ProxyAddPoints(GameAction action)
	{
		try
		{
			DistributeRewardProcess.EDistributeRewardProcessType type = (DistributeRewardProcess.EDistributeRewardProcessType)action.SupplementaryDataIDMin;
			DistributeRewardProcess distributeRewardProcess = processes.Single((DistributeRewardProcess s) => s.GetRewardType() == type);
			distributeRewardProcess.ProxyAddPoint(distributeRewardProcess.GetActor(action.ActorID));
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to process UIDistributeRewardsManager.ProxyAddPoints.  SupplementaryDataIDMin: " + action.SupplementaryDataIDMin + "\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void ProxyRemovePoints(GameAction action)
	{
		try
		{
			DistributeRewardProcess.EDistributeRewardProcessType type = (DistributeRewardProcess.EDistributeRewardProcessType)action.SupplementaryDataIDMin;
			DistributeRewardProcess distributeRewardProcess = processes.Single((DistributeRewardProcess s) => s.GetRewardType() == type);
			distributeRewardProcess.ProxyRemovePoint(distributeRewardProcess.GetActor(action.ActorID));
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to process UIDistributeRewardsManager.ProxyRemovePoints.  SupplementaryDataIDMin: " + action.SupplementaryDataIDMin + " ActorID:" + action.ActorID + "\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}
}
