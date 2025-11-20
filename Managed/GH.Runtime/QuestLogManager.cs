using System;
using System.Collections.Generic;
using System.Linq;
using Code.State;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using SM.Gamepad;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class QuestLogManager : MonoBehaviour
{
	[SerializeField]
	private UIQuestLogGroup fakeGroup;

	[SerializeField]
	private UIQuestLogSlot fakeQuest;

	[SerializeField]
	private List<UIQuestLogGroup> groups;

	[SerializeField]
	private List<UIQuestLogSlot> slotsPool;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private ScrollSizeFitter scrollSizeFitter;

	[SerializeField]
	private float scrollingOffset;

	[SerializeField]
	private ControllerInputArea controllerArea;

	[SerializeField]
	private TextLocalizedListener controllerActionText;

	[SerializeField]
	private MonoHotkeySession hotkeySession;

	[SerializeField]
	private Hotkey scrollbarHotkey;

	public QuestEvent OnQuestHovered;

	public QuestEvent OnQuestUnhovered;

	public QuestEvent OnQuestSelected;

	private IQuestLogService service;

	private UIWindow window;

	private Dictionary<Enum, UIQuestLogGroup> assignedGroups;

	private Dictionary<CQuestState, UIQuestLogSlot> assignedSlots;

	private void Awake()
	{
		window = GetComponent<UIWindow>();
		assignedSlots = new Dictionary<CQuestState, UIQuestLogSlot>();
		if (AdventureState.MapState.IsCampaign)
		{
			service = new CampaignQuestLogService();
		}
		else
		{
			service = new GuildmasterQuestLogService();
		}
		CreateGroups();
		controllerArea.OnFocused.AddListener(OnFocused);
		controllerArea.OnUnfocused.AddListener(OnUnfocused);
		if (InputManager.GamePadInUse)
		{
			scrollbarHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			scrollbarHotkey.DisplayHotkey(active: false);
		}
		scrollRect.verticalScrollbar.interactable = false;
	}

	private void OnDestroy()
	{
		OnQuestHovered.RemoveAllListeners();
		OnQuestUnhovered.RemoveAllListeners();
		OnQuestSelected.RemoveAllListeners();
		if (InputManager.GamePadInUse)
		{
			scrollbarHotkey.Deinitialize();
		}
	}

	private void CreateGroups()
	{
		List<QuestLogGroup> list = service.GetGroups();
		HelperTools.NormalizePool(ref groups, groups[0].gameObject, groups[0].transform.parent, list.Count);
		assignedGroups = new Dictionary<Enum, UIQuestLogGroup>();
		for (int i = 0; i < list.Count; i++)
		{
			Enum id = list[i].Id;
			assignedGroups[id] = groups[i];
			groups[i].SetQuestGroup(list[i], delegate(bool hovered)
			{
				OnHoveredGroup(hovered, id);
			}, OnExpandedGroup);
		}
		OnContentChanged();
	}

	public void SetQuests(List<CQuestState> quests)
	{
		List<CQuestState> list = quests.OrderBy((CQuestState it) => LocalizationManager.GetTranslation(it.Quest.LocalisedNameKey)).ToList();
		for (int num = 0; num < list.Count; num++)
		{
			AddQuest(list[num]);
		}
	}

	public void AddCityEventQuest(Action onClicked)
	{
		if (fakeGroup.Slots.Count == 0)
		{
			UIQuestLogSlot uIQuestLogSlot = fakeQuest;
			uIQuestLogSlot.SetFakeQuest(delegate
			{
				fakeGroup.RemoveFirstSlot();
				onClicked();
			});
			fakeGroup.AddFakeSlot(uIQuestLogSlot);
			OnContentChanged();
		}
	}

	public void HideCityEventQuest()
	{
		fakeGroup.RemoveSlot(fakeQuest);
		OnContentChanged();
	}

	public void AddQuest(CQuestState quest)
	{
		UIQuestLogGroup uIQuestLogGroup = assignedGroups[service.GetGroup(quest)];
		UIQuestLogSlot slot = GetSlot(uIQuestLogGroup);
		slot.SetQuest(quest, OnHoveredQuest, OnQuestSelected.Invoke);
		assignedSlots[quest] = slot;
		uIQuestLogGroup.AddSlot(slot);
		OnContentChanged();
	}

	public void SortQuestLogAlphabetically()
	{
		foreach (UIQuestLogGroup value in assignedGroups.Values)
		{
			foreach (UIQuestLogSlot item in value.Slots.OrderBy((UIQuestLogSlot it) => LocalizationManager.GetTranslation(it.Quest.Quest.LocalisedNameKey)))
			{
				item.transform.SetAsLastSibling();
			}
		}
	}

	public void RemoveQuest(CQuestState quest)
	{
		UIQuestLogSlot uIQuestLogSlot = assignedSlots[quest];
		uIQuestLogSlot.gameObject.SetActive(value: false);
		slotsPool.Remove(uIQuestLogSlot);
		slotsPool.Add(uIQuestLogSlot);
		assignedSlots.Remove(quest);
		assignedGroups[service.GetGroup(quest)].RemoveSlot(uIQuestLogSlot);
		OnContentChanged();
	}

	private void OnContentChanged()
	{
		if (scrollSizeFitter != null)
		{
			scrollSizeFitter.Fit();
		}
	}

	private void ScrollTo(RectTransform rectTransform)
	{
		scrollRect.ScrollToFit(rectTransform);
	}

	private void OnHoveredQuest(CQuestState quest, bool hovered)
	{
		if (hovered)
		{
			if (InputManager.GamePadInUse)
			{
				RectTransform target = assignedSlots[quest].transform as RectTransform;
				if (!scrollRect.IsFullyVisibleInViewport(target))
				{
					ScrollTo(assignedSlots[quest].transform as RectTransform);
				}
			}
			controllerActionText.SetTextKey("GUI_SELECT_QUEST");
			OnQuestHovered.Invoke(quest);
		}
		else
		{
			OnQuestUnhovered.Invoke(quest);
		}
	}

	private void OnHoveredGroup(bool hovered, Enum groupId)
	{
		if (!hovered)
		{
			return;
		}
		if (InputManager.GamePadInUse)
		{
			RectTransform rectTransform = assignedGroups[groupId].transform.GetChild(0) as RectTransform;
			if (!scrollRect.IsFullyVisibleInViewport(rectTransform))
			{
				ScrollTo(rectTransform);
			}
		}
		controllerActionText.SetTextKey(assignedGroups[groupId].IsExpanded ? "GUI_CONTROLLER_CLOSE_GROUP" : "GUI_CONTROLLER_EXPAND_GROUP");
	}

	private void OnExpandedGroup(bool expanded)
	{
		controllerActionText.SetTextKey(expanded ? "GUI_CONTROLLER_CLOSE_GROUP" : "GUI_CONTROLLER_EXPAND_GROUP");
		OnContentChanged();
	}

	public void ShowQuestSelected(CQuestState quest, bool selected)
	{
		UIQuestLogSlot uIQuestLogSlot = assignedSlots[quest];
		uIQuestLogSlot.SetSelected(selected);
		if (!assignedGroups[service.GetGroup(quest)].IsExpanded)
		{
			uIQuestLogSlot.gameObject.SetActive(uIQuestLogSlot.IsFocused);
		}
	}

	public void HighlightQuest(CQuestState quest)
	{
		UIQuestLogSlot uIQuestLogSlot = assignedSlots[quest];
		if (!uIQuestLogSlot.IsHighlighted)
		{
			uIQuestLogSlot.Highlight();
			if (!assignedGroups[service.GetGroup(quest)].IsExpanded)
			{
				uIQuestLogSlot.gameObject.SetActive(value: true);
				OnContentChanged();
			}
			RectTransform rectTransform = uIQuestLogSlot.transform as RectTransform;
			if (!rectTransform.FitsInRectTransform(UIManager.Instance.UICamera, scrollRect.viewport))
			{
				ScrollTo(rectTransform);
			}
		}
	}

	public void UnhighlightQuest(CQuestState quest)
	{
		UIQuestLogSlot uIQuestLogSlot = assignedSlots[quest];
		uIQuestLogSlot.UnHighlight();
		if (!uIQuestLogSlot.IsFocused && !assignedGroups[service.GetGroup(quest)].IsExpanded)
		{
			uIQuestLogSlot.gameObject.SetActive(value: false);
			OnContentChanged();
		}
	}

	private UIQuestLogSlot GetSlot(UIQuestLogGroup group)
	{
		if (slotsPool.Count <= assignedSlots.Count)
		{
			UIQuestLogSlot uIQuestLogSlot = UnityEngine.Object.Instantiate(slotsPool[0], group.questContainer);
			slotsPool.Add(uIQuestLogSlot);
			uIQuestLogSlot.gameObject.SetActive(value: true);
			return uIQuestLogSlot;
		}
		UIQuestLogSlot uIQuestLogSlot2 = slotsPool[assignedSlots.Count];
		uIQuestLogSlot2.transform.SetParent(group.questContainer);
		uIQuestLogSlot2.gameObject.SetActive(value: true);
		return uIQuestLogSlot2;
	}

	public void RefreshNotification(CQuestState quest)
	{
		assignedSlots[quest].RefreshNotification();
	}

	public void RefreshQuestsState()
	{
		foreach (KeyValuePair<CQuestState, UIQuestLogSlot> item in assignedSlots.ToList())
		{
			CQuestState key = item.Key;
			UIQuestLogSlot value = item.Value;
			value.RefreshLockState();
			if (!assignedGroups[service.GetGroup(key)].HasSlot(value))
			{
				RemoveQuest(key);
				AddQuest(key);
			}
		}
		Singleton<QuestManager>.Instance.SortQuestLogAlphabetically();
	}

	public void ShowLogScreen(bool instant = false)
	{
		window.Show(instant);
	}

	public void HideLogScreen(bool instant = false)
	{
		window.Hide(instant);
	}

	public void HighlightQuestGroup(Enum groupId, bool highlight)
	{
		if (assignedGroups.ContainsKey(groupId))
		{
			assignedGroups[groupId].SetHighlighted(highlight);
		}
	}

	public void Focus()
	{
		controllerArea.Focus();
	}

	private void OnFocused()
	{
		if (hotkeySession != null)
		{
			hotkeySession.Show();
		}
		scrollbarHotkey.DisplayHotkey(active: true);
		CampaignMapStateTag latestState = ((IStateProvider)Singleton<UINavigation>.Instance.StateMachine).GetLatestState(new CampaignMapStateTag[2]
		{
			CampaignMapStateTag.MapEvent,
			CampaignMapStateTag.WorldMap
		});
		if (latestState != CampaignMapStateTag.WorldMap && latestState == CampaignMapStateTag.MapEvent)
		{
			Singleton<UIWindowManager>.Instance.Escape();
		}
		else
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.QuestLog);
		}
		scrollRect.verticalScrollbar.interactable = true;
	}

	private void OnUnfocused()
	{
		if (hotkeySession != null)
		{
			hotkeySession.Hide();
		}
		scrollbarHotkey.DisplayHotkey(active: false);
		if (Singleton<UINavigation>.Instance.StateMachine.PreviousState is QuestLogState)
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.WorldMap);
		}
		else
		{
			Singleton<UINavigation>.Instance.StateMachine.ToPreviousState();
		}
		scrollRect.verticalScrollbar.interactable = false;
	}
}
