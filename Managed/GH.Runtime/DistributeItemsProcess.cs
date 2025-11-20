using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Utils.Extensions;
using Assets.Script.Misc;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using Photon.Bolt;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class DistributeItemsProcess : DistributeRewardProcess
{
	public class DistributeItemService : DistributeRewardService
	{
		public DistributeItemService(List<CMapCharacter> characters, Reward reward)
			: base(characters, reward)
		{
		}

		public override void Apply()
		{
			AdventureState.MapState.ApplyRewards((from it in actors
				where GetAssignedPoints(it) > 0
				select new Reward(base.Reward.ItemID, GetAssignedPoints(it), base.Reward.Type, it.Character.CharacterID, EGiveToCharacterType.Give)).ToList(), null);
			if (!FFSNetwork.IsOnline || !FFSNetwork.IsHost)
			{
				return;
			}
			foreach (CMapCharacter checkCharacter in SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty.CheckCharacters)
			{
				int controllableID = (AdventureState.MapState.IsCampaign ? checkCharacter.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(checkCharacter.CharacterID));
				ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
				IProtocolToken supplementaryDataToken = new ItemInventoryToken(checkCharacter, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.GoldMode);
				Synchronizer.ReplicateControllableStateChange(GameActionType.ModifyItemInventory, currentPhase, controllableID, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
		}

		public override Sprite GetTitleIcon()
		{
			return null;
		}

		public override string GetTitleText()
		{
			string text = string.Format(LocalizationManager.GetTranslation($"GUI_DISTRIBUTE_{base.Reward.Type}"), base.AvailablePoints);
			if (base.Reward.IsNegative())
			{
				return "<color=#" + UIInfoTools.Instance.warningColor.ToHex() + ">" + text + "</color>";
			}
			return text;
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
					Synchronizer.SendGameAction(GameActionType.DistributeUIAddPoint, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorID, 2);
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
					Synchronizer.SendGameAction(GameActionType.DistributeUIAddPoint, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorID2, 2);
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
				if (FFSNetwork.IsHost)
				{
					DistributeMapActor distributeMapActor = (DistributeMapActor)actor;
					int actorID = (AdventureState.MapState.IsCampaign ? distributeMapActor.Character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(distributeMapActor.Character.CharacterID));
					Synchronizer.SendGameAction(GameActionType.DistributeUIRemovePoint, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorID, 2);
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

	public class DistributeConsumItemService : DistributeItemService
	{
		public DistributeConsumItemService(List<CMapCharacter> characters, Reward reward)
			: base(characters, reward)
		{
			if (base.AvailablePoints == 0)
			{
				base.AvailablePoints = 1;
			}
		}

		public override void Apply()
		{
			foreach (DistributeMapActor item in actors.Where((DistributeMapActor it) => GetAssignedPoints(it) > 0))
			{
				AdventureState.MapState.ApplyRewards(new List<Reward> { base.Reward }, item.Character.CharacterID);
			}
		}

		public override string GetTitleText()
		{
			return string.Format(LocalizationManager.GetTranslation("GUI_DISTRIBUTE_ConsumeItem"), LocalizationManager.GetTranslation("GUI_ITEM_SLOT_" + base.Reward.ConsumeSlot));
		}
	}

	[SerializeField]
	private UIDistributeReward processUI;

	private DistributeItemService m_Service;

	public override EDistributeRewardProcessType GetRewardType()
	{
		return EDistributeRewardProcessType.DistributeItems;
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
		foreach (Reward item in list)
		{
			callbackPromise = callbackPromise.Then(delegate
			{
				List<CMapCharacter> list2 = FilterPossibleCharacters(selectedCharacters, item);
				return (list2.Count == 0) ? CallbackPromise.Resolved() : processUI.Distribute(m_Service = BuildService(list2, item), EDistributeRewardProcessType.DistributeItems, "GUI_CONFIRM", (item.Type == ETreasureType.Item) ? "GUI_BIND_ITEM" : "GUI_SELECT");
			}).Then(delegate
			{
				rewards.Remove(item);
			});
		}
		return callbackPromise;
	}

	private DistributeItemService BuildService(List<CMapCharacter> characters, Reward reward)
	{
		if (reward.Type == ETreasureType.ConsumeItem)
		{
			return new DistributeConsumItemService(characters, reward);
		}
		_ = reward.Type;
		_ = 34;
		return new DistributeItemService(characters, reward);
	}

	private List<CMapCharacter> FilterPossibleCharacters(IEnumerable<CMapCharacter> characters, Reward reward)
	{
		if (reward.Type == ETreasureType.ConsumeItem)
		{
			CItem.EItemSlot slot = (CItem.EItemSlot)Enum.Parse(typeof(CItem.EItemSlot), reward.ConsumeSlot);
			return characters.Where((CMapCharacter it) => it.CheckEquippedItems.Any((CItem item) => item.YMLData.Slot == slot)).ToList();
		}
		if (reward.Type == ETreasureType.LoseItem)
		{
			return characters.Where((CMapCharacter it) => it.CheckBoundItems.Exists((CItem item) => item.ID == reward.ItemID) || it.CheckEquippedItems.Exists((CItem item) => item.ID == reward.ItemID)).ToList();
		}
		return characters.ToList();
	}

	public override bool IsRewardToProcess(Reward reward)
	{
		if (reward.Type != ETreasureType.Item || reward.GiveToCharacterRequirement != EGiveToCharacterRequirement.None || reward.TreasureDistributionType == ETreasureDistributionType.PerMercenaryInParty || !reward.GiveToCharacterID.IsNullOrEmpty())
		{
			if (reward.Type == ETreasureType.LoseItem || reward.Type == ETreasureType.ConsumeItem)
			{
				return reward.TreasureDistributionType == ETreasureDistributionType.Combined;
			}
			return false;
		}
		return true;
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
		throw new Exception("DistributeItemsProcess service is null");
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
