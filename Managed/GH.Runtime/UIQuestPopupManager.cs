using System;
using Assets.Script.GUI.Quest;
using MapRuleLibrary.MapState;
using UnityEngine;

public class UIQuestPopupManager : Singleton<UIQuestPopupManager>
{
	[SerializeField]
	private UIQuestPopup selectedQuestPopup;

	[SerializeField]
	private UIQuestPreviewPopup questPreviewPopup;

	[SerializeField]
	private UIQuestPopup multiplayerQuestPopup;

	[SerializeField]
	private RectTransform previewWorldQuestHolder;

	private Action onQuestPreviewFinished;

	private IQuest selectedQuest;

	private IQuest previewdQuest;

	private CQuestState clientSelectedQuest;

	public bool IsQuestShown => selectedQuest != null;

	public void ShowQuest(CQuestState questState, bool autoFocus = false)
	{
		ShowQuest(new Quest(questState), autoFocus);
	}

	public void ShowQuest(IQuest questState, bool autoFocus = false)
	{
		HidePreview();
		if (!object.Equals(questState, selectedQuest))
		{
			selectedQuest = questState;
			selectedQuestPopup.ShowQuest(questState, autoFocus);
		}
	}

	public void TryFocusQuest()
	{
		if (IsQuestShown)
		{
			selectedQuestPopup.Focus();
		}
	}

	public void TryUnfocusQuest()
	{
		if (IsQuestShown)
		{
			selectedQuestPopup.Unfocus();
		}
	}

	public void SetSwitchNavigation(bool canSwitch)
	{
		selectedQuestPopup.SetSwitchNavigation(canSwitch);
	}

	public void PreviewQuest(CQuestState questState, MapLocation location, Vector3 offset, Transform position = null)
	{
		PreviewQuest(new Quest(questState), location, offset, position);
	}

	public void PreviewQuest(IQuest questState, MapLocation location, Vector3 offset, Transform position = null)
	{
		if (location.IsSelected)
		{
			ShowQuest(questState);
		}
		else if (selectedQuest == null)
		{
			onQuestPreviewFinished = null;
			previewdQuest = questState;
			questPreviewPopup.ShowQuest(questState, (position == null) ? location.transform : position, offset, FFSNetwork.IsOnline && FFSNetwork.IsClient && Singleton<AdventureMapUIManager>.Instance.LocationToTravel != null);
			Singleton<QuestManager>.Instance.HideCompletedQuests(!questState.IsCompleted());
		}
	}

	public void ResetQuestPopups()
	{
		if (selectedQuest != null)
		{
			selectedQuestPopup.SetQuest(selectedQuest, forceRefresh: true);
		}
		if (clientSelectedQuest != null)
		{
			multiplayerQuestPopup.SetQuest(new Quest(clientSelectedQuest), forceRefresh: true);
		}
	}

	public void UnfocusQuests()
	{
		questPreviewPopup.Focus(focus: false);
	}

	public void HidePreview(CQuestState quest)
	{
		HidePreview(new Quest(quest));
	}

	public void HidePreview(IQuest quest)
	{
		if (object.Equals(quest, previewdQuest))
		{
			HidePreview();
		}
	}

	private void HidePreview(bool instant = false)
	{
		questPreviewPopup.Hide(instant);
		previewdQuest = null;
		if (onQuestPreviewFinished != null)
		{
			Action action = onQuestPreviewFinished;
			onQuestPreviewFinished = null;
			action();
		}
	}

	public void Hide(CQuestState quest)
	{
		if (quest != null)
		{
			Hide(new Quest(quest));
		}
	}

	public void Hide(IQuest quest)
	{
		if (object.Equals(quest, selectedQuest))
		{
			selectedQuestPopup.Hide();
			selectedQuest = null;
		}
		HidePreview(quest);
	}

	public void HideAll(bool instant = false)
	{
		selectedQuestPopup.Hide(instant);
		selectedQuest = null;
		HidePreview(instant);
		HideMultiplayerPreview(instant);
	}

	public void RefreshQuestsState()
	{
		if (selectedQuest != null)
		{
			selectedQuestPopup.RefreshLocked();
		}
		if (previewdQuest != null)
		{
			questPreviewPopup.RefreshLocked();
		}
		if (clientSelectedQuest != null)
		{
			multiplayerQuestPopup.RefreshLocked();
		}
	}

	public void HideMultiplayerPreview(bool instant = false)
	{
		if (clientSelectedQuest != null)
		{
			clientSelectedQuest = null;
			multiplayerQuestPopup.Hide(instant);
		}
	}

	public void ShowMultiplayerPreview(CQuestState quest)
	{
		if (clientSelectedQuest != quest)
		{
			clientSelectedQuest = quest;
			multiplayerQuestPopup.gameObject.SetActive(value: true);
			multiplayerQuestPopup.ShowQuest(new Quest(quest));
		}
	}

	public void PreviewWorldQuestFromCity(CQuestState quest, MapLocation mapLocation)
	{
		PreviewQuest(quest, mapLocation, Vector3.up, previewWorldQuestHolder);
		onQuestPreviewFinished = delegate
		{
			Singleton<UIGuildmasterHUD>.Instance.HighlightWorldMode(show: false);
		};
		Singleton<UIGuildmasterHUD>.Instance.HighlightWorldMode(show: true);
	}
}
