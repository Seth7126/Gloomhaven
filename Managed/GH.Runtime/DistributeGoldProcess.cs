using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Misc;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class DistributeGoldProcess : DistributeRewardProcess
{
	private class DistributeGoldService : DistributeRewardService
	{
		public DistributeGoldService(List<CMapCharacter> characters, Reward reward)
			: base(characters, reward)
		{
			if (base.AvailablePoints > 0)
			{
				int num = Mathf.FloorToInt((float)reward.Amount / (float)actors.Count);
				if (num > 0)
				{
					base.AvailablePoints = reward.Amount % actors.Count;
				}
				else
				{
					base.AvailablePoints = reward.Amount;
				}
				{
					foreach (DistributeMapActor actor in actors)
					{
						actor.AssignedPoints = num;
						if (base.AvailablePoints > 0)
						{
							AddPoints(actor, 1);
						}
					}
					return;
				}
			}
			int num2 = actors.Sum((DistributeMapActor it) => it.Character.CharacterGold);
			int num3 = Math.Abs(reward.Amount);
			foreach (DistributeMapActor actor2 in actors)
			{
				AddPoints(actor2, Mathf.FloorToInt((float)actor2.Character.CharacterGold / (float)num2 * (float)num3));
			}
			if (base.AvailablePoints >= 0)
			{
				return;
			}
			foreach (DistributeMapActor item in actors.Where((DistributeMapActor it) => it.AssignedPoints < it.Character.CharacterGold))
			{
				AddPoints(item, 1);
				if (base.AvailablePoints == 0)
				{
					break;
				}
			}
		}

		public override void Apply()
		{
			AdventureState.MapState.ApplyRewards((from it in actors
				where GetAssignedPoints(it) > 0
				select new Reward(ETreasureType.Gold, (base.Reward.Amount > 0) ? GetAssignedPoints(it) : (-GetAssignedPoints(it)), ETreasureDistributionType.None, it.Character.CharacterID)).ToList(), null);
		}

		public override Sprite GetTitleIcon()
		{
			return null;
		}

		public override string GetTitleText()
		{
			return string.Format(LocalizationManager.GetTranslation((base.AvailablePoints == 0) ? "GUI_DISTRIBUTED_GOLD_POINTS" : "GUI_DISTRIBUTE_GOLD_POINTS"), base.AvailablePoints);
		}

		public override bool CanRemovePointsFrom(IDistributePointsActor actor)
		{
			return ((DistributeMapActor)actor).AssignedPoints > 0;
		}

		public override bool CanAddPointsTo(IDistributePointsActor actor)
		{
			if (base.Reward.Amount >= 0)
			{
				return base.AvailablePoints != 0;
			}
			if (base.AvailablePoints != 0)
			{
				return ((DistributeMapActor)actor).AssignedPoints < ((DistributeMapActor)actor).Character.CharacterGold;
			}
			return false;
		}

		public override void AddPoint(IDistributePointsActor actor)
		{
			if (AddPoints((DistributeMapActor)actor, 1) && FFSNetwork.IsHost)
			{
				DistributeMapActor distributeMapActor = (DistributeMapActor)actor;
				int actorID = (AdventureState.MapState.IsCampaign ? distributeMapActor.Character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(distributeMapActor.Character.CharacterID));
				Synchronizer.SendGameAction(GameActionType.DistributeUIAddPoint, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorID, 1);
			}
		}

		private bool AddPoints(DistributeMapActor actor, int points)
		{
			if (base.AvailablePoints == 0)
			{
				return false;
			}
			if (base.Reward.Amount > 0)
			{
				base.AvailablePoints -= points;
			}
			else
			{
				base.AvailablePoints += points;
			}
			actor.AssignedPoints += points;
			return true;
		}

		public override void RemovePoint(IDistributePointsActor actor)
		{
			if (GetAssignedPoints(actor) > 0)
			{
				if (base.Reward.Amount > 0)
				{
					base.AvailablePoints++;
				}
				else
				{
					base.AvailablePoints--;
				}
				((DistributeMapActor)actor).AssignedPoints--;
				if (FFSNetwork.IsHost)
				{
					DistributeMapActor distributeMapActor = (DistributeMapActor)actor;
					int actorID = (AdventureState.MapState.IsCampaign ? distributeMapActor.Character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(distributeMapActor.Character.CharacterID));
					Synchronizer.SendGameAction(GameActionType.DistributeUIRemovePoint, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorID, 1);
				}
			}
		}

		public override int GetMaxPoints(IDistributePointsActor actor)
		{
			if (base.Reward.Amount >= 0)
			{
				return base.AvailablePoints;
			}
			return Mathf.Min(Mathf.Abs(base.AvailablePoints), ((DistributeMapActor)actor).Character.CharacterGold);
		}

		public override int GetCurrentPoints(IDistributePointsActor actor)
		{
			return 0;
		}
	}

	[SerializeField]
	private UIDistributeReward processUI;

	private DistributeGoldService m_Service;

	public override EDistributeRewardProcessType GetRewardType()
	{
		return EDistributeRewardProcessType.DistributeGold;
	}

	public override ICallbackPromise Process(List<Reward> rewards)
	{
		List<Reward> goldRewards = rewards.FindAll(IsRewardToProcess);
		if (goldRewards.Count == 0)
		{
			return CallbackPromise.Resolved();
		}
		List<CMapCharacter> list;
		if (goldRewards.Exists((Reward it) => it.TreasureDistributionRestrictionType == ETreasureDistributionRestrictionType.ExcludePreviousSelectedCharacter))
		{
			List<string> excludedCharacters = (from it in rewards.Except(goldRewards)
				where !it.CharacterID.IsNullOrEmpty()
				select it.CharacterID).ToList();
			list = AdventureState.MapState.MapParty.SelectedCharacters.Where((CMapCharacter it) => (it.PersonalQuest == null || !it.PersonalQuest.IsFinished) && !excludedCharacters.Contains(it.CharacterID)).ToList();
		}
		else
		{
			list = AdventureState.MapState.MapParty.SelectedCharacters.Where((CMapCharacter it) => it.PersonalQuest == null || !it.PersonalQuest.IsFinished).ToList();
		}
		if (list.Count == 0)
		{
			return CallbackPromise.Resolved();
		}
		int num = goldRewards.Sum((Reward it) => it.Amount);
		if (num < 0)
		{
			num = -Math.Min(Math.Abs(num), list.Sum((CMapCharacter it) => it.CharacterGold));
		}
		if (num == 0)
		{
			return CallbackPromise.Resolved();
		}
		m_Service = new DistributeGoldService(list, new Reward(ETreasureType.Gold, num, ETreasureDistributionType.Combined, null));
		return processUI.Distribute(m_Service, EDistributeRewardProcessType.DistributeGold, "GUI_DISTRIBUTE_GOLD_CONFIRM").Then(delegate
		{
			rewards.RemoveAll((Reward it) => goldRewards.Contains(it));
		});
	}

	public override bool IsRewardToProcess(Reward reward)
	{
		if (reward.Type == ETreasureType.Gold && reward.TreasureDistributionType == ETreasureDistributionType.Combined)
		{
			return reward.TreasureDistributionRestrictionType != ETreasureDistributionRestrictionType.AllAmount;
		}
		return false;
	}

	public override IDistributePointsActor GetActor(int modelInstanceID)
	{
		return m_Service?.GetActor(modelInstanceID);
	}

	public override void ProxyConfirmClick()
	{
		processUI.ProxyConfirmClick();
	}

	public override void ProxyAddPoint(IDistributePointsActor actor)
	{
		if (m_Service != null && actor != null)
		{
			m_Service.AddPoint(actor);
			processUI.PopUp.Refresh();
			return;
		}
		throw new Exception("DistributeGoldProcess was sent null values");
	}

	public override void ProxyRemovePoint(IDistributePointsActor actor)
	{
		if (m_Service != null && actor != null)
		{
			m_Service.RemovePoint(actor);
			processUI.PopUp.Refresh();
			return;
		}
		throw new Exception("DistributeGoldProcess was sent null values");
	}
}
