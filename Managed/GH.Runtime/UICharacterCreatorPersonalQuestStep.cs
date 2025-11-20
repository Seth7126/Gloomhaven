using System;
using System.Collections.Generic;
using Assets.Script.Misc;
using GLOO.Introduction;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UICharacterCreatorPersonalQuestStep : UICharacterCreatorStep<CPersonalQuestState>
{
	[SerializeField]
	private GameObject confirmWarning;

	[SerializeField]
	private HelpBoxLine helpText;

	[SerializeField]
	private List<UICharacterCreatorPersonalQuestChoice> personalQuestsPool;

	[SerializeField]
	private UIIntroduce introduction;

	[SerializeField]
	private UIMapFTUEStep ftuePQ;

	[SerializeField]
	private PanelHotkeyContainer panelHotkeyContainer;

	private UICharacterCreatorPersonalQuestChoice selectedQuest;

	private ManualActionBlocker _manualActionBlocker;

	private UICharacterCreatorPersonalQuestChoice _currentHoveredQuest;

	private SimpleKeyActionHandlerBlocker _personalQuestSelectionBlocker;

	protected override Action ShortPressCallback => ToggleFromShortRest;

	protected override void AddBlockers(KeyActionHandler keyActionHandler)
	{
		base.AddBlockers(keyActionHandler);
		_manualActionBlocker = new ManualActionBlocker();
		_personalQuestSelectionBlocker = new SimpleKeyActionHandlerBlocker();
		keyActionHandler.AddBlocker(_manualActionBlocker);
		keyActionHandler.AddBlocker(_personalQuestSelectionBlocker);
	}

	private bool IsQuestSelected()
	{
		return selectedQuest != null;
	}

	public ICallbackPromise<CPersonalQuestState> Show(List<CPersonalQuestState> possiblePersonalQuests, bool instant = false)
	{
		selectedQuest = null;
		helpText.Show(null, "GUI_CREATE_CHARACTER_PERSONAL_QUEST_HELP_TITLE");
		HelperTools.NormalizePool(ref personalQuestsPool, personalQuestsPool[0].gameObject, personalQuestsPool[0].transform.parent, possiblePersonalQuests.Count);
		for (int i = 0; i < possiblePersonalQuests.Count; i++)
		{
			personalQuestsPool[i].SetPersonalQuest(possiblePersonalQuests[i], OnPersonalQuestSelected, OnPersonalQuestHovered);
			if (InputManager.GamePadInUse)
			{
				personalQuestsPool[i].IsQuestSelectedCallback = IsQuestSelected;
			}
		}
		bool flag = Singleton<MapFTUEManager>.Instance != null && !Singleton<MapFTUEManager>.Instance.HasCompletedStep(EMapFTUEStep.CreatedFirstCharacter);
		if (flag)
		{
			window.escapeKeyAction = UIWindow.EscapeKeyAction.Skip;
		}
		else
		{
			window.escapeKeyAction = UIWindow.EscapeKeyAction.HideOnlyThis;
		}
		CallbackPromise<CPersonalQuestState> result = ProcessStep(instant);
		if (selectedQuest != null)
		{
			selectedQuest.Select();
		}
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.PersonalQuestChoice);
		ShowIntroduction();
		if (InputManager.GamePadInUse && panelHotkeyContainer != null)
		{
			panelHotkeyContainer.SetActiveHotkey("Back", !flag, ignoreActiveInHierarchy: true);
		}
		OnPersonalQuestSelected(selected: false, null);
		EnableConfirmationButton(enable: false);
		return result;
	}

	private void ShowIntroduction()
	{
		if (AdventureState.MapState.MapParty.HasIntroduced(EIntroductionConcept.PersonalQuest.ToString()))
		{
			ShowFTUE();
			return;
		}
		AdventureState.MapState.MapParty.MarkIntroDone(EIntroductionConcept.PersonalQuest.ToString());
		introduction.Show(ShowFTUE);
	}

	private void ShowFTUE()
	{
		_manualActionBlocker.UnBlock();
	}

	protected override CPersonalQuestState GetSelectedValue()
	{
		return selectedQuest?.PersonalQuest;
	}

	protected override void EnableConfirmationButton(bool enable)
	{
		base.EnableConfirmationButton(enable);
		confirmWarning.SetActive(value: false);
		helpText.Show(null, "GUI_CREATE_CHARACTER_PERSONAL_QUEST_HELP_TITLE");
		if (InputManager.GamePadInUse)
		{
			longConfirmHandler.gameObject.SetActive(enable && IsQuestSelected());
		}
	}

	private void OnPersonalQuestSelected(bool selected, UICharacterCreatorPersonalQuestChoice slot)
	{
		if (selected)
		{
			if (selectedQuest != null)
			{
				selectedQuest.Deselect();
			}
			selectedQuest = slot;
			for (int i = 0; i < personalQuestsPool.Count && personalQuestsPool[i].gameObject.activeSelf; i++)
			{
				if (personalQuestsPool[i] != slot)
				{
					personalQuestsPool[i].Focus(focused: false);
				}
			}
			selectedQuest.Focus(focused: true);
			selectedQuest.Highlight(highlight: true);
			EnableConfirmationButton(enable: true);
			if (!InputManager.GamePadInUse)
			{
				confirmButton.gameObject.SetActive(value: true);
			}
		}
		else if (selectedQuest == slot && selectedQuest != null)
		{
			selectedQuest.Focus(focused: true);
			selectedQuest = null;
			EnableConfirmationButton(enable: false);
			if (!InputManager.GamePadInUse)
			{
				confirmButton.gameObject.SetActive(value: false);
			}
		}
		else if (selectedQuest != null)
		{
			selectedQuest.Focus(focused: false);
		}
		if (selectedQuest == null)
		{
			foreach (UICharacterCreatorPersonalQuestChoice item in personalQuestsPool)
			{
				if (!item.gameObject.activeSelf)
				{
					break;
				}
				item.Focus(focused: true);
			}
		}
		_personalQuestSelectionBlocker.SetBlock(!IsQuestSelected());
		UpdateSelectionHotkeysActivity();
	}

	protected override void Validate(Action<bool> callback)
	{
		if (selectedQuest == null)
		{
			confirmWarning.SetActive(value: true);
			helpText.ShowWarning(null, "GUI_CREATE_CHARACTER_PERSONAL_QUEST_HELP_TITLE");
			helpText.HighlightWarning();
			callback(obj: false);
		}
		else
		{
			base.Validate(callback);
		}
	}

	protected override void OnHidden()
	{
		window.escapeKeyAction = UIWindow.EscapeKeyAction.None;
		base.OnHidden();
	}

	private void ToggleFromShortRest()
	{
		if (_currentHoveredQuest != null)
		{
			_currentHoveredQuest.Toggle();
		}
	}

	private void OnPersonalQuestHovered(bool isHovered, UICharacterCreatorPersonalQuestChoice slot)
	{
		_currentHoveredQuest = (isHovered ? slot : null);
		if (isHovered)
		{
			for (int i = 0; i < personalQuestsPool.Count && personalQuestsPool[i].gameObject.activeSelf; i++)
			{
				if (personalQuestsPool[i] == slot)
				{
					personalQuestsPool[i].Highlight(highlight: true);
					personalQuestsPool[i].Focus(focused: true);
				}
				else
				{
					personalQuestsPool[i].Highlight(highlight: false);
					personalQuestsPool[i].Focus(selectedQuest == null || personalQuestsPool[i] == selectedQuest);
				}
			}
		}
		else
		{
			for (int j = 0; j < personalQuestsPool.Count && personalQuestsPool[j].gameObject.activeSelf; j++)
			{
				personalQuestsPool[j].Highlight(personalQuestsPool[j] == selectedQuest);
				personalQuestsPool[j].Focus(selectedQuest == null || personalQuestsPool[j] == selectedQuest);
			}
		}
		UpdateSelectionHotkeysActivity();
	}

	private void UpdateSelectionHotkeysActivity()
	{
		if (InputManager.GamePadInUse && _currentHoveredQuest != null)
		{
			bool flag = _currentHoveredQuest == selectedQuest;
			panelHotkeyContainer.SetActiveHotkey("Unselect", flag);
			panelHotkeyContainer.SetActiveHotkey("Select", !flag);
		}
	}

	protected override void OnControllerAreaUnfocused()
	{
		if (InputManager.GamePadInUse)
		{
			base.OnControllerAreaUnfocused();
			for (int i = 0; i < personalQuestsPool.Count && personalQuestsPool[i].gameObject.activeSelf; i++)
			{
				personalQuestsPool[i].DisableNavigation();
			}
			panelHotkeyContainer.gameObject.SetActive(value: false);
		}
	}

	protected override void OnControllerAreaFocused()
	{
		if (InputManager.GamePadInUse)
		{
			base.OnControllerAreaFocused();
			_currentHoveredQuest?.EnableNavigation();
			panelHotkeyContainer.gameObject.SetActive(value: true);
		}
	}
}
