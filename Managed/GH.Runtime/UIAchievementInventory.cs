using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Utils.Extensions;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Achievements;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAchievementInventory : MonoBehaviour, IMoveHandler, IEventSystemHandler
{
	public delegate void AchieventEvent(CPartyAchievement achievement);

	[SerializeField]
	private ScrollRect scroll;

	[SerializeField]
	private UIAchievementSlot slotPrefab;

	[SerializeField]
	private VerticalLayoutGroup _verticalLayoutGroup;

	[SerializeField]
	private int initialPool = 10;

	[SerializeField]
	private List<GameObject> interactionMasks;

	[SerializeField]
	private List<UIAchievementFilter> filters;

	[SerializeField]
	private UIAchievementFilter defaultFilter;

	[SerializeField]
	private ControllerInputAreaLocal controllerInputArea;

	[Space(5f)]
	[Header("Gamepad")]
	[SerializeField]
	private List<Hotkey> _hotkeys;

	[SerializeField]
	private UiNavigationRoot navRoot;

	private EAchievementType currentFilter;

	private List<UIAchievementSlot> achievementsPool = new List<UIAchievementSlot>();

	private Dictionary<EAchievementType, List<UIAchievementSlot>> slotsType = new Dictionary<EAchievementType, List<UIAchievementSlot>>();

	public event AchieventEvent OnClaimedAchievement;

	public event AchieventEvent OnHoveredAchievement;

	private void Awake()
	{
		for (int i = 0; i < filters.Count; i++)
		{
			UIAchievementFilter filter = filters[i];
			filter.tab.onValueChanged.AddListener(delegate(bool active)
			{
				if (active)
				{
					FilterBy(filter.filter);
				}
			});
		}
		if (defaultFilter == null)
		{
			defaultFilter = filters[0];
		}
		controllerInputArea.OnFocusedArea.AddListener(EnableNavigation);
		controllerInputArea.OnUnfocusedArea.AddListener(DisableNavigation);
		InitializeHotKeysGamepad();
		SubscribeOnKeyActionHandlerEvents();
	}

	private void OnDestroy()
	{
		for (int i = 0; i < filters.Count; i++)
		{
			filters[i].tab.onValueChanged.RemoveAllListeners();
		}
		UnsubscribeOnKeyActionHandlerEvents();
	}

	public void Preload()
	{
		HelperTools.NormalizePool(ref achievementsPool, slotPrefab.gameObject, scroll.content, initialPool);
	}

	public void Show(List<CPartyAchievement> achievements, bool selectDefaultFilter = true)
	{
		foreach (KeyValuePair<EAchievementType, List<UIAchievementSlot>> item in slotsType)
		{
			item.Value.Clear();
		}
		slotsType.Clear();
		scroll.verticalNormalizedPosition = 1f;
		List<CPartyAchievement> list = achievements.OrderByDescending(delegate(CPartyAchievement it)
		{
			if (it.State == EAchievementState.Completed)
			{
				return 2f;
			}
			return (it.State == EAchievementState.RewardsClaimed) ? (-1f) : ((float)it.AchievementConditionState.CurrentProgress / (float)it.AchievementConditionState.TotalConditionsAndTargets);
		}).ThenBy(delegate(CPartyAchievement it)
		{
			AchievementYMLData achievement = it.Achievement;
			return achievement.AchievementOrderId ?? achievement.ID;
		}).ToList();
		HelperTools.NormalizePool(ref achievementsPool, slotPrefab.gameObject, scroll.content, achievements.Count);
		for (int num = 0; num < list.Count; num++)
		{
			UIAchievementSlot uIAchievementSlot = achievementsPool[num];
			if (!InputManager.GamePadInUse)
			{
				uIAchievementSlot.SetAchievement(list[num], OnClaimReward, OnHovered, delegate(UIAchievementSlot _, MoveDirection dir)
				{
					MoveToFilter(dir);
				});
			}
			else
			{
				uIAchievementSlot.SetAchievement(list[num], OnClaimReward, OnHovered, null);
			}
			if (controllerInputArea.IsEnabled)
			{
				uIAchievementSlot.EnableNavigation();
			}
			if (!slotsType.ContainsKey(uIAchievementSlot.Type))
			{
				slotsType[uIAchievementSlot.Type] = new List<UIAchievementSlot>();
			}
			slotsType[uIAchievementSlot.Type].Add(uIAchievementSlot);
		}
		int num2 = 0;
		bool flag = false;
		for (int num3 = 0; num3 < filters.Count; num3++)
		{
			UIAchievementFilter uIAchievementFilter = filters[num3];
			if (uIAchievementFilter.filter == EAchievementType.None)
			{
				continue;
			}
			if (!slotsType.ContainsKey(uIAchievementFilter.filter))
			{
				uIAchievementFilter.Init(0, 0);
				continue;
			}
			List<UIAchievementSlot> list2 = slotsType[uIAchievementFilter.filter];
			int num4 = list2.Count((UIAchievementSlot it) => it.Achievement.State == EAchievementState.RewardsClaimed);
			bool flag2 = list2.Exists((UIAchievementSlot it) => it.Achievement.IsNew);
			uIAchievementFilter.Init(list2.Count, num4, flag2);
			num2 += num4;
			flag = flag || flag2;
		}
		if (selectDefaultFilter)
		{
			currentFilter = EAchievementType.None;
			if (defaultFilter.tab.isOn)
			{
				FilterBy(defaultFilter.filter);
			}
			else
			{
				defaultFilter.tab.isOn = true;
			}
		}
		else
		{
			EAchievementType newFilter = currentFilter;
			currentFilter = EAchievementType.None;
			FilterBy(newFilter);
		}
		if (!controllerInputArea.IsEnabled)
		{
			controllerInputArea.Enable();
		}
	}

	private void InitializeHotKeysGamepad()
	{
		if (!InputManager.GamePadInUse)
		{
			return;
		}
		foreach (Hotkey hotkey in _hotkeys)
		{
			hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
	}

	private void OnHovered(UIAchievementSlot slot, bool hovered)
	{
		if (!hovered)
		{
			return;
		}
		bool isNew = slot.Achievement.IsNew;
		this.OnHoveredAchievement?.Invoke(slot.Achievement);
		if (InputManager.GamePadInUse)
		{
			scroll.ScrollToFit(slot.transform as RectTransform);
		}
		if (slot.Achievement.IsNew != isNew)
		{
			filters.First((UIAchievementFilter it) => it.filter == slot.Type).ShowNewNotification(slotsType[slot.Type].Exists((UIAchievementSlot it) => it.Achievement.IsNew));
		}
	}

	private void OnClaimReward(UIAchievementSlot slot)
	{
		this.OnClaimedAchievement?.Invoke(slot.Achievement);
	}

	public void FilterBy(EAchievementType newFilter)
	{
		StopAllCoroutines();
		if (currentFilter == newFilter)
		{
			return;
		}
		if (newFilter == EAchievementType.None)
		{
			foreach (KeyValuePair<EAchievementType, List<UIAchievementSlot>> item in slotsType)
			{
				if (item.Key == currentFilter)
				{
					continue;
				}
				foreach (UIAchievementSlot item2 in item.Value)
				{
					item2.SetVisibility(value: true);
				}
			}
		}
		else if (currentFilter == EAchievementType.None)
		{
			foreach (KeyValuePair<EAchievementType, List<UIAchievementSlot>> item3 in slotsType)
			{
				bool visibility = item3.Key == newFilter;
				foreach (UIAchievementSlot item4 in item3.Value)
				{
					item4.SetVisibility(visibility);
				}
			}
		}
		else
		{
			if (slotsType.ContainsKey(currentFilter))
			{
				foreach (UIAchievementSlot item5 in slotsType[currentFilter])
				{
					item5.SetVisibility(value: false);
				}
			}
			if (slotsType.ContainsKey(newFilter))
			{
				foreach (UIAchievementSlot item6 in slotsType[newFilter])
				{
					item6.SetVisibility(value: true);
				}
			}
		}
		currentFilter = newFilter;
		scroll.verticalNormalizedPosition = 1f;
		if (controllerInputArea.IsFocused)
		{
			EnableNavigation();
		}
	}

	public void SetInteractable(bool interactable)
	{
		for (int i = 0; i < filters.Count; i++)
		{
			filters[i].SetInteractable(interactable);
		}
		for (int j = 0; j < interactionMasks.Count; j++)
		{
			interactionMasks[j].gameObject.SetActive(!interactable);
		}
		for (int k = 0; k < achievementsPool.Count && achievementsPool[k].gameObject.activeSelf; k++)
		{
			achievementsPool[k].SetInteractable(interactable);
		}
	}

	public void Hide()
	{
		if (slotsType.ContainsKey(currentFilter))
		{
			List<UIAchievementSlot> list = slotsType[currentFilter];
			for (int i = 0; i < list.Count; i++)
			{
				list[i].StopAnimations();
			}
		}
		StopAllCoroutines();
		controllerInputArea.Destroy();
	}

	private void SubscribeOnKeyActionHandlerEvents()
	{
		if (InputManager.GamePadInUse)
		{
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_NEXT_TAB, MoveToNextTab).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerInputArea)));
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_PREVIOUS_TAB, MoveToPreviousTab).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerInputArea)));
		}
	}

	private void UnsubscribeOnKeyActionHandlerEvents()
	{
		if (InputManager.GamePadInUse)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_PREVIOUS_TAB, MoveToPreviousTab);
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_NEXT_TAB, MoveToNextTab);
		}
	}

	private void MoveToNextTab()
	{
		MoveToFilter(MoveDirection.Right);
	}

	private void MoveToPreviousTab()
	{
		MoveToFilter(MoveDirection.Left);
	}

	public void OnMove(AxisEventData eventData)
	{
		MoveToFilter(eventData.moveDir);
	}

	private void MoveToFilter(MoveDirection direction)
	{
		if (direction != MoveDirection.Left && direction != MoveDirection.Right)
		{
			return;
		}
		List<UIAchievementFilter> list = new List<UIAchievementFilter>();
		foreach (UIAchievementFilter filter in filters)
		{
			if (filter.tab.IsInteractable)
			{
				list.Add(filter);
			}
		}
		list = list.OrderBy((UIAchievementFilter it) => it.transform.GetSiblingIndex()).ToList();
		int num = list.FindIndex((UIAchievementFilter it) => it.filter == currentFilter);
		switch (direction)
		{
		case MoveDirection.Left:
			((num == 0) ? list.Last().tab : list[num - 1].tab).isOn = true;
			break;
		case MoveDirection.Right:
			list[(num + 1) % list.Count].tab.isOn = true;
			break;
		}
	}

	private void DisableNavigation()
	{
		if (currentFilter == EAchievementType.None)
		{
			foreach (UIAchievementSlot item in slotsType.SelectMany((KeyValuePair<EAchievementType, List<UIAchievementSlot>> it) => it.Value))
			{
				item.DisableNavigation();
			}
			return;
		}
		if (!slotsType.ContainsKey(currentFilter))
		{
			return;
		}
		foreach (UIAchievementSlot item2 in slotsType[currentFilter])
		{
			item2.DisableNavigation();
		}
	}

	public void EnableNavigation()
	{
		if (currentFilter == EAchievementType.None)
		{
			foreach (UIAchievementSlot item in slotsType.SelectMany((KeyValuePair<EAchievementType, List<UIAchievementSlot>> it) => it.Value))
			{
				item.EnableNavigation();
			}
		}
		else if (slotsType.ContainsKey(currentFilter))
		{
			foreach (UIAchievementSlot item2 in slotsType[currentFilter])
			{
				item2.EnableNavigation();
			}
		}
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.GuildmasterTrainer);
		SelectFirstElement();
	}

	private void SelectFirstElement()
	{
		if (!controllerInputArea.IsFocused)
		{
			return;
		}
		if (InputManager.GamePadInUse)
		{
			Singleton<UINavigation>.Instance.NavigationManager.TrySelectFirstIn(navRoot);
			return;
		}
		UIAchievementSlot uIAchievementSlot = (slotsType.ContainsKey(currentFilter) ? slotsType[currentFilter].FirstOrDefault((UIAchievementSlot it) => it.gameObject.activeInHierarchy) : null);
		if (uIAchievementSlot != null)
		{
			Select(uIAchievementSlot.gameObject);
		}
		else
		{
			Select(base.gameObject);
		}
	}

	private void Select(GameObject slotGameObject)
	{
		if (InputManager.GamePadInUse)
		{
			Singleton<UINavigation>.Instance.NavigationManager.TrySelect(slotGameObject.GetComponent<IUiNavigationSelectable>());
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(slotGameObject);
		}
	}
}
