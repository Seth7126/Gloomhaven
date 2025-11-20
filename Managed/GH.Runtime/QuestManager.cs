using System;
using System.Collections.Generic;
using System.Linq;
using GLOO.Introduction;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.Quest;
using UnityEngine;

public class QuestManager : Singleton<QuestManager>
{
	[SerializeField]
	private QuestLogManager questLog;

	[SerializeField]
	private UIQuestPopupManager questPopups;

	[SerializeField]
	private QuestTrackerManager questTracker;

	[SerializeField]
	private StoryHide storyHide;

	[SerializeField]
	private UIIntroduce questIntroduction;

	private HashSet<object> hideLogRequests = new HashSet<object>();

	private Dictionary<CQuestState, MapLocation> locations = new Dictionary<CQuestState, MapLocation>();

	private bool hideCompletedQuest;

	public bool IsHiddenCompletedQuests => hideCompletedQuest;

	protected override void Awake()
	{
		base.Awake();
		Singleton<MapMarkersManager>.Instance.OnAddedQuestMarker.AddListener(OnTrackQuest);
		Singleton<MapMarkersManager>.Instance.OnRemovedQuestMarker.AddListener(OnUntrackQuest);
		questLog.OnQuestHovered.AddListener(OnHoveredLogQuest);
		questLog.OnQuestUnhovered.AddListener(OnUnhoveredLogQuest);
		questLog.OnQuestSelected.AddListener(OnSelectedLogQuest);
		questTracker.OnClickedTracker.AddListener(FocusOnQuest);
		storyHide.Hide();
	}

	private void OnEnable()
	{
		NewPartyDisplayUI.PartyDisplay.OnOpenedPanel += OnPartyChanged;
	}

	private void OnDisable()
	{
		if (NewPartyDisplayUI.PartyDisplay != null)
		{
			NewPartyDisplayUI.PartyDisplay.OnOpenedPanel -= OnPartyChanged;
		}
	}

	private void OnPartyChanged(bool selected, NewPartyDisplayUI.DisplayType display)
	{
		if (!selected && display == NewPartyDisplayUI.DisplayType.LEVELUP)
		{
			questLog.RefreshQuestsState();
		}
	}

	public void OnMapLocationQuestHighlighted(CQuestState quest, bool highlighted)
	{
		if (highlighted)
		{
			OnHighlighted(quest);
			questLog.HighlightQuest(quest);
		}
		else
		{
			questLog.UnhighlightQuest(quest);
		}
		RefreshVisibilityCompletedQuest(quest);
	}

	public void OnHeadquartersMapLocationQuestHighlighted(bool highlighted)
	{
		if (AdventureState.MapState.IsCampaign)
		{
			questLog.HighlightQuestGroup(CampaignQuestLogService.ECampaignLogGroup.City, highlighted);
		}
	}

	public void AddCityEventQuest(Action onClicked)
	{
		questLog.AddCityEventQuest(onClicked);
	}

	public void HideCityEventQuest()
	{
		questLog.HideCityEventQuest();
	}

	private void OnTrackQuest(CQuestState quest, MapLocation location)
	{
		locations[quest] = location;
		questLog.AddQuest(quest);
		RefreshVisibilityCompletedQuest(quest);
	}

	private void OnUntrackQuest(CQuestState quest, MapLocation location)
	{
		locations.Remove(quest);
		questLog.RemoveQuest(quest);
	}

	private void OnHoveredLogQuest(CQuestState quest)
	{
		OnHighlighted(quest);
		locations[quest].ForceHighlight(force: true);
	}

	private void OnHighlighted(CQuestState quest)
	{
		if (quest.IsNew)
		{
			quest.IsNew = false;
			questLog.RefreshNotification(quest);
		}
	}

	private void OnUnhoveredLogQuest(CQuestState quest)
	{
		locations[quest].ForceHighlight(force: false);
		if (locations[quest].IsHighlighted)
		{
			questLog.HighlightQuest(quest);
		}
	}

	private void OnSelectedLogQuest(CQuestState quest)
	{
		Singleton<MapChoreographer>.Instance.SelectLocation(locations[quest]);
	}

	public void OnMapLocationQuestSelected(CQuestState quest, bool selected)
	{
		if (selected)
		{
			if (quest != null)
			{
				questPopups.ShowQuest(quest, autoFocus: true);
				questTracker.TrackQuest(quest, locations[quest]);
				HideLogScreen(this);
				ShowIntroduction();
			}
		}
		else
		{
			questTracker.RemoveTracker();
			if (quest == null)
			{
				questPopups.HideAll();
			}
			else
			{
				questPopups.Hide(quest);
			}
			ShowLogScreen(this);
		}
	}

	private void ShowIntroduction()
	{
		if (!AdventureState.MapState.MapParty.HasIntroduced(EIntroductionConcept.Quest.ToString()))
		{
			questIntroduction.Show();
			AdventureState.MapState.MapParty.MarkIntroDone(EIntroductionConcept.Quest.ToString());
		}
	}

	private void FocusOnQuest(CQuestState quest)
	{
		CameraController.s_CameraController.m_TargetFocalPoint = locations[quest].CenterPosition;
	}

	public void Show()
	{
		storyHide.Show();
	}

	public void OnPartyMove()
	{
		questPopups.HideAll();
		questLog.HideLogScreen();
	}

	public void RefreshLockedQuests()
	{
		questLog.RefreshQuestsState();
		Singleton<MapMarkersManager>.Instance.RefreshQuestsState();
		questPopups.RefreshQuestsState();
	}

	public void HideLogScreen(object request, bool instant = true)
	{
		hideLogRequests.Add(request);
		questLog.HideLogScreen(instant);
	}

	public void ShowLogScreen(object request, bool instant = true)
	{
		hideLogRequests.Remove(request);
		if (hideLogRequests.Count == 0)
		{
			questLog.ShowLogScreen(instant);
		}
	}

	public void HideCompletedQuests(bool hide)
	{
		if (hide == hideCompletedQuest)
		{
			return;
		}
		hideCompletedQuest = hide;
		foreach (KeyValuePair<CQuestState, MapLocation> item in locations.Where((KeyValuePair<CQuestState, MapLocation> it) => it.Key.QuestState >= CQuestState.EQuestState.Completed && (it.Key.Quest.HideTreasureWhenCompleted || !it.Value.HasPendingTreasures)))
		{
			RefreshVisibilityCompletedQuest(item.Key);
		}
	}

	private void RefreshVisibilityCompletedQuest(CQuestState quest)
	{
		if (quest.QuestState < CQuestState.EQuestState.Completed || (locations[quest].HasPendingTreasures && !quest.Quest.HideTreasureWhenCompleted))
		{
			return;
		}
		if (AdventureState.MapState.IsCampaign || quest.Quest.Type == EQuestType.Travel)
		{
			if (hideCompletedQuest && !locations[quest].IsHighlighted)
			{
				locations[quest].HideQuestMapMarker(this);
			}
			else
			{
				locations[quest].ShowQuestMapMarker(instant: true, this);
			}
		}
		else if (hideCompletedQuest && !locations[quest].IsHighlighted)
		{
			locations[quest].HideLocation(this);
		}
		else
		{
			locations[quest].ShowLocation(this);
		}
	}

	public void FocusQuestLog()
	{
		questLog.Focus();
	}

	public void SortQuestLogAlphabetically()
	{
		questLog.SortQuestLogAlphabetically();
	}
}
