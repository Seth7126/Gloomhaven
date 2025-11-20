using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Utils.Extensions;
using Assets.Script.Misc;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class DistributeGoldBagProcess : DistributeRewardProcess
{
	private class DistributeGoldBagService : DistributeRewardService
	{
		public DistributeGoldBagService(List<CMapCharacter> characters, Reward reward)
			: base(characters, reward)
		{
			base.AvailablePoints = 1;
		}

		public override void Apply()
		{
			base.Reward.CharacterID = actors.Single((DistributeMapActor it) => GetAssignedPoints(it) != 0).Character.CharacterID;
			AdventureState.MapState.ApplyRewards(new List<Reward>
			{
				new Reward(ETreasureType.Gold, base.Reward.Amount, ETreasureDistributionType.None, base.Reward.CharacterID)
			}, null);
		}

		public override Sprite GetTitleIcon()
		{
			return null;
		}

		public override string GetTitleText()
		{
			if (base.Reward.Amount > 0)
			{
				return string.Format(LocalizationManager.GetTranslation("GUI_DISTRIBUTE_SELECT_GAIN"), string.Format("<sprite name=\"Gold_Icon_White\">{0} {1}", base.Reward.Amount, LocalizationManager.GetTranslation("Gold")));
			}
			string text = UIInfoTools.Instance.warningColor.ToHex();
			return string.Format(LocalizationManager.GetTranslation("GUI_DISTRIBUTE_SELECT_LOSE"), string.Format("<color=#{0}><sprite name=\"Gold_Icon_White\" color=#{1}>{2} {3}", text, text, -base.Reward.Amount, LocalizationManager.GetTranslation("Gold")));
		}

		public override bool CanRemovePointsFrom(IDistributePointsActor actor)
		{
			return ((DistributeMapActor)actor).AssignedPoints != 0;
		}

		public override bool CanAddPointsTo(IDistributePointsActor actor)
		{
			if (base.AvailablePoints <= 0)
			{
				return actors.Exists((DistributeMapActor it) => GetAssignedPoints(it) != 0);
			}
			return true;
		}

		public override void AddPoint(IDistributePointsActor actor)
		{
			if (base.AvailablePoints > 0)
			{
				base.AvailablePoints--;
				((DistributeMapActor)actor).AssignedPoints += Math.Abs(base.Reward.Amount);
				if (FFSNetwork.IsHost)
				{
					DistributeMapActor distributeMapActor = (DistributeMapActor)actor;
					int actorID = (AdventureState.MapState.IsCampaign ? distributeMapActor.Character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(distributeMapActor.Character.CharacterID));
					Synchronizer.SendGameAction(GameActionType.DistributeUIAddPoint, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorID, 5);
				}
				return;
			}
			DistributeMapActor distributeMapActor2 = actors.FirstOrDefault((DistributeMapActor it) => GetAssignedPoints(it) != 0);
			if (distributeMapActor2 == null)
			{
				return;
			}
			RemovePoint(distributeMapActor2);
			if (base.AvailablePoints > 0)
			{
				base.AvailablePoints--;
				((DistributeMapActor)actor).AssignedPoints += Math.Abs(base.Reward.Amount);
				if (FFSNetwork.IsHost)
				{
					DistributeMapActor distributeMapActor3 = (DistributeMapActor)actor;
					int actorID2 = (AdventureState.MapState.IsCampaign ? distributeMapActor3.Character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(distributeMapActor3.Character.CharacterID));
					Synchronizer.SendGameAction(GameActionType.DistributeUIAddPoint, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorID2, 5);
				}
			}
		}

		public override void RemovePoint(IDistributePointsActor actor)
		{
			if (GetAssignedPoints(actor) != 0)
			{
				DistributeMapActor distributeMapActor = (DistributeMapActor)actor;
				base.AvailablePoints++;
				distributeMapActor.AssignedPoints -= Math.Abs(base.Reward.Amount);
				if (FFSNetwork.IsHost)
				{
					int actorID = (AdventureState.MapState.IsCampaign ? distributeMapActor.Character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(distributeMapActor.Character.CharacterID));
					Synchronizer.SendGameAction(GameActionType.DistributeUIRemovePoint, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorID, 5);
				}
			}
		}

		public override int GetMaxPoints(IDistributePointsActor actor)
		{
			return base.AvailablePoints;
		}

		public override int GetCurrentPoints(IDistributePointsActor actor)
		{
			return 0;
		}
	}

	[SerializeField]
	private UIDistributeReward processUI;

	private DistributeGoldBagService m_Service;

	public override EDistributeRewardProcessType GetRewardType()
	{
		return EDistributeRewardProcessType.DistributeGoldBag;
	}

	public override ICallbackPromise Process(List<Reward> rewards)
	{
		List<CMapCharacter> characters = AdventureState.MapState.MapParty.SelectedCharacters.Where((CMapCharacter it) => it.PersonalQuest == null || !it.PersonalQuest.IsFinished).ToList();
		if (characters.Count == 0)
		{
			return CallbackPromise.Resolved();
		}
		List<Reward> list = rewards.FindAll(IsRewardToProcess);
		ICallbackPromise callbackPromise = CallbackPromise.Resolved();
		foreach (Reward goldReward in list)
		{
			callbackPromise = callbackPromise.Then(delegate
			{
				if (goldReward.Amount == 0)
				{
					return CallbackPromise.Resolved();
				}
				List<CMapCharacter> list2 = characters;
				if (goldReward.Amount < 0)
				{
					list2 = characters.FindAll((CMapCharacter it) => it.CharacterGold >= -goldReward.Amount);
					if (list2.Count == 0)
					{
						return CallbackPromise.Resolved();
					}
				}
				m_Service = new DistributeGoldBagService(list2, goldReward);
				return processUI.Distribute(m_Service, GetRewardType(), "GUI_CONFIRM", "GUI_SELECT");
			});
		}
		return callbackPromise;
	}

	public override bool IsRewardToProcess(Reward reward)
	{
		if (reward.Type == ETreasureType.Gold && reward.TreasureDistributionType == ETreasureDistributionType.Combined)
		{
			return reward.TreasureDistributionRestrictionType == ETreasureDistributionRestrictionType.AllAmount;
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
		if (m_Service != null)
		{
			m_Service.AddPoint(actor);
			if (processUI.PopUp.Slots.SingleOrDefault((UIDistributePointsSlot s) => s.Actor == actor).Counter is UIDistributePointsToggle uIDistributePointsToggle)
			{
				uIDistributePointsToggle.SetExtendedPoints(m_Service.GetAssignedPoints(actor));
			}
			if (Singleton<UIDistributePointsPopup>.Instance != null)
			{
				Singleton<UIDistributePointsPopup>.Instance.Refresh();
			}
			return;
		}
		throw new Exception("DistributeGoldBagProcess service is null");
	}

	public override void ProxyRemovePoint(IDistributePointsActor actor)
	{
		if (m_Service != null)
		{
			m_Service.RemovePoint(actor);
			if (processUI.PopUp.Slots.SingleOrDefault((UIDistributePointsSlot s) => s.Actor == actor).Counter is UIDistributePointsToggle uIDistributePointsToggle)
			{
				uIDistributePointsToggle.SetExtendedPoints(m_Service.GetAssignedPoints(actor));
			}
			if (Singleton<UIDistributePointsPopup>.Instance != null)
			{
				Singleton<UIDistributePointsPopup>.Instance.Refresh();
			}
			return;
		}
		throw new Exception("DistributeGoldBagProcess service is null");
	}
}
