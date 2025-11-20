#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using FFSNet;
using Photon.Bolt;
using ScenarioRuleLibrary;
using UnityEngine;

public class ActiveBonus : BaseActiveBonus<CActiveBonus>
{
	public ActiveBonus(CActiveBonus bonus, CActor actor)
		: base(bonus, actor)
	{
	}

	public override Sprite GetIcon()
	{
		if (bonus.BaseCard is CItem cItem)
		{
			return UIInfoTools.Instance.GetItemMiniSprite(cItem.YMLData.Art);
		}
		if (bonus.IsAura && bonus.Actor is CPlayerActor cPlayerActor)
		{
			return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(cPlayerActor.CharacterClass.ID, cPlayerActor.CharacterClass.CharacterYML.CustomCharacterConfig);
		}
		if (bonus.Caster is CPlayerActor cPlayerActor2)
		{
			return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(cPlayerActor2.Class.ID, cPlayerActor2.CharacterClass?.CharacterYML.CustomCharacterConfig);
		}
		if (actor is CPlayerActor cPlayerActor3)
		{
			return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(cPlayerActor3.Class.ID, cPlayerActor3?.CharacterClass?.CharacterYML.CustomCharacterConfig);
		}
		if (actor is CHeroSummonActor cHeroSummonActor)
		{
			return UIInfoTools.Instance.GetActiveAbilityIcon("Ability_" + cHeroSummonActor.HeroSummonClass.ID.ReplaceLastOccurrence("ID", string.Empty) + "_Icon");
		}
		return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(actor.Class.ID);
	}

	public override string GetSelectAudioItem()
	{
		if (bonus.BaseCard is CItem cItem)
		{
			return UIInfoTools.Instance.GetItemConfig(cItem.YMLData.Art).toggleAudioItem;
		}
		return base.GetSelectAudioItem();
	}

	public override void UntoggleActiveBonus(bool fromClick = false)
	{
		Debug.Log("Attempting to untoggle active bonus slot\n" + Environment.StackTrace);
		if (!IsToggled || IsToggleLocked)
		{
			return;
		}
		Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: false);
		if (fromClick)
		{
			bool flag = Choreographer.s_Choreographer.ActorOrHisSummonerIsUnderMyControl(actor);
			if (FFSNetwork.IsOnline && flag && ActionProcessor.CurrentPhase != ActionPhaseType.TakeDamageConfirmation)
			{
				Debug.Log("Sending untoggle active bonus slot click action\n" + Environment.StackTrace);
				ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
				int iD = actor.ID;
				IProtocolToken supplementaryDataToken = new ActiveBonusToken(bonus, selected: false);
				Synchronizer.SendGameAction(GameActionType.ClickActiveBonusSlot, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
			if (FFSNetwork.IsOnline && !actor.IsUnderMyControl)
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionProcessor.CurrentPhase, savePreviousState: true);
			}
		}
		Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
		Choreographer.s_Choreographer.m_SkipButton.SetInteractable(active: false);
		Choreographer.s_Choreographer.m_UndoButton.SetInteractable(active: false);
		ScenarioRuleClient.ToggleActiveBonus(bonus, actor, null, (int)ActionProcessor.CurrentPhase, null, GameState.WaitingForPlayerToSelectDamageResponse || GameState.WaitingForPlayerActorToAvoidDamageResponse, ScenarioRuleClient.ToggleState.Untoggle);
	}

	public void ToggleActiveBonus(ElementInfusionBoardManager.EElement? eElement, object extraOption, bool fromClick = false)
	{
		Debug.Log("Attempting to toggle active bonus slot\n" + Environment.StackTrace);
		if (IsToggled || IsToggleLocked)
		{
			return;
		}
		Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: false);
		if (PhaseManager.CurrentPhase is CPhaseAction { CurrentPhaseAbility: not null } cPhaseAction && cPhaseAction.CurrentPhaseAbility.m_Ability.IsWaitingForSingleTargetActiveBonus())
		{
			Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
		}
		if (fromClick)
		{
			bool flag = Choreographer.s_Choreographer.ActorOrHisSummonerIsUnderMyControl(actor);
			if (FFSNetwork.IsOnline && flag && ActionProcessor.CurrentPhase != ActionPhaseType.TakeDamageConfirmation)
			{
				Debug.Log("Sending toggle active bonus slot click action\n" + Environment.StackTrace);
				UIUseActiveBonus slotForActiveBonus = Singleton<UIActiveBonusBar>.Instance.GetSlotForActiveBonus(bonus);
				List<ElementInfusionBoardManager.EElement> selectedConsumes = slotForActiveBonus.GetSelectedConsumes();
				ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
				int iD = actor.ID;
				IProtocolToken supplementaryDataToken = new ActiveBonusToken(bonus, selected: true, selectedConsumes.IsNullOrEmpty() ? ((ElementInfusionBoardManager.EElement?)null) : new ElementInfusionBoardManager.EElement?(selectedConsumes[0]), slotForActiveBonus.SelectedOptions);
				Synchronizer.SendGameAction(GameActionType.ClickActiveBonusSlot, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
			if (FFSNetwork.IsOnline && !actor.IsUnderMyControl)
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionProcessor.CurrentPhase, savePreviousState: true);
			}
		}
		Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
		Choreographer.s_Choreographer.m_SkipButton.SetInteractable(active: false);
		Choreographer.s_Choreographer.m_UndoButton.SetInteractable(active: false);
		ScenarioRuleClient.ToggleActiveBonus(bonus, actor, eElement, (int)ActionProcessor.CurrentPhase, extraOption, GameState.WaitingForPlayerToSelectDamageResponse || GameState.WaitingForPlayerActorToAvoidDamageResponse, ScenarioRuleClient.ToggleState.Toggle);
	}

	public override void ToggleActiveBonus(ElementInfusionBoardManager.EElement? eElement, bool fromClick = false)
	{
		ToggleActiveBonus(eElement, null, fromClick);
	}
}
