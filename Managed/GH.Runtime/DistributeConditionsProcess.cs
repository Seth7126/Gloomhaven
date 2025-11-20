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

public class DistributeConditionsProcess : DistributeRewardProcess
{
	public class DistributeConditionService : DistributeRewardService
	{
		public DistributeConditionService(List<CMapCharacter> characters, Reward reward)
			: base(characters, reward)
		{
			base.AvailablePoints = 1;
		}

		public override void Apply()
		{
			AdventureState.MapState.ApplyRewards(new List<Reward> { base.Reward }, actors.Single((DistributeMapActor it) => GetAssignedPoints(it) > 0).Character.CharacterID);
		}

		public override Sprite GetTitleIcon()
		{
			return null;
		}

		public override string GetTitleText()
		{
			return string.Format(LocalizationManager.GetTranslation("GUI_DISTRIBUTE_SELECT_GAIN"), string.Join(", ", base.Reward.Conditions.Select((RewardCondition it) => LocalizationManager.GetTranslation((it.Type == RewardCondition.EConditionType.Negative) ? it.NegativeCondition.ToString() : it.PositiveCondition.ToString()))));
		}

		public override bool CanAddPointsTo(IDistributePointsActor actor)
		{
			if (base.AvailablePoints <= 0)
			{
				return actors.Exists((DistributeMapActor it) => GetAssignedPoints(it) > 0);
			}
			return true;
		}

		public override void AddPoint(IDistributePointsActor actor)
		{
			if (base.AvailablePoints > 0)
			{
				base.AvailablePoints--;
				((DistributeMapActor)actor).AssignedPoints++;
				if (FFSNetwork.IsHost)
				{
					DistributeMapActor distributeMapActor = (DistributeMapActor)actor;
					int actorID = (AdventureState.MapState.IsCampaign ? distributeMapActor.Character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(distributeMapActor.Character.CharacterID));
					Synchronizer.SendGameAction(GameActionType.DistributeUIAddPoint, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorID, 4);
				}
				return;
			}
			DistributeMapActor distributeMapActor2 = actors.FirstOrDefault((DistributeMapActor it) => GetAssignedPoints(it) > 0);
			if (distributeMapActor2 == null)
			{
				return;
			}
			RemovePoint(distributeMapActor2);
			if (base.AvailablePoints > 0)
			{
				base.AvailablePoints--;
				((DistributeMapActor)actor).AssignedPoints++;
				if (FFSNetwork.IsHost)
				{
					DistributeMapActor distributeMapActor3 = (DistributeMapActor)actor;
					int actorID2 = (AdventureState.MapState.IsCampaign ? distributeMapActor3.Character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(distributeMapActor3.Character.CharacterID));
					Synchronizer.SendGameAction(GameActionType.DistributeUIAddPoint, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorID2, 4);
				}
			}
		}

		public override bool CanRemovePointsFrom(IDistributePointsActor actor)
		{
			return ((DistributeMapActor)actor).AssignedPoints > 0;
		}

		public override void RemovePoint(IDistributePointsActor actor)
		{
			if (GetAssignedPoints(actor) > 0)
			{
				base.AvailablePoints++;
				((DistributeMapActor)actor).AssignedPoints--;
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

	private DistributeConditionService m_Service;

	public override EDistributeRewardProcessType GetRewardType()
	{
		return EDistributeRewardProcessType.DistributeConditions;
	}

	public override ICallbackPromise Process(List<Reward> rewards)
	{
		List<CMapCharacter> selectedCharacters = AdventureState.MapState.MapParty.SelectedCharacters.Where((CMapCharacter it) => it.PersonalQuest == null || !it.PersonalQuest.IsFinished).ToList();
		if (selectedCharacters.Count == 0)
		{
			return CallbackPromise.Resolved();
		}
		List<Reward> list = rewards.FindAll(IsRewardToProcess);
		if (list.Count == 0)
		{
			return CallbackPromise.Resolved();
		}
		ICallbackPromise callbackPromise = CallbackPromise.Resolved();
		foreach (Reward condition in list)
		{
			callbackPromise = callbackPromise.Then(() => processUI.Distribute(m_Service = new DistributeConditionService(selectedCharacters, condition), GetRewardType(), "GUI_CONFIRM", "GUI_SELECT")).Then(delegate
			{
				rewards.Remove(condition);
			});
		}
		return callbackPromise;
	}

	public override bool IsRewardToProcess(Reward reward)
	{
		if (reward.Type == ETreasureType.Condition)
		{
			return reward.TreasureDistributionType == ETreasureDistributionType.Combined;
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
		throw new Exception("DistributeAttackModifierProcess service is null");
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
		throw new Exception("DistributeItemsProcess service is null");
	}
}
