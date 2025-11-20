#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using FFSNet;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.PopupStates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDistributePointsPopup : Singleton<UIDistributePointsPopup>
{
	[SerializeField]
	private GameObject window;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private Image titleImage;

	[SerializeField]
	private UIDistributePointsSlot slotPrefab;

	[SerializeField]
	private RectTransform slotsContainer;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private PopupStateTag stateTag;

	[SerializeField]
	private List<UIDistributePointsSlot> slots = new List<UIDistributePointsSlot>();

	private Dictionary<IDistributePointsActor, UIDistributePointsSlot> assignedSlots = new Dictionary<IDistributePointsActor, UIDistributePointsSlot>();

	private IDistributePointsService service;

	private Action<IDistributePointsActor, int> onUpdatedPoints;

	private Action<IDistributePointsActor, bool> onHovered;

	private Action<bool> _onSelected;

	private bool ShowHotkeys
	{
		get
		{
			if (InputManager.GamePadInUse && stateTag != PopupStateTag.DistributeGoldRewards)
			{
				if (FFSNetwork.IsOnline)
				{
					return FFSNetwork.IsHost;
				}
				return true;
			}
			return false;
		}
	}

	public List<UIDistributePointsSlot> Slots => slots;

	public UIDistributePointsSlot CurrentHoveredSlot { get; private set; }

	public bool IsFocused => controllerArea.IsFocused;

	private PanelHotkeyContainer Hotkeys
	{
		get
		{
			if (SaveData.Instance.Global.CurrentGameState != EGameState.Map)
			{
				return Singleton<UIScenarioDistributePointsManager>.Instance.HotkeyContainer;
			}
			return Singleton<UIDistributeRewardManager>.Instance.HotkeyContainer;
		}
	}

	public bool IsShown => window.activeSelf;

	protected override void Awake()
	{
		base.Awake();
		controllerArea.OnFocusedArea.AddListener(OnControllerAreaFocused);
		controllerArea.OnUnfocusedArea.AddListener(OnControllerAreaUnfocused);
		controllerArea.OnEnabledArea.AddListener(OnControllerAreaEnabled);
		controllerArea.OnDisabledArea.AddListener(OnControllerAreaDisabled);
	}

	public void Show(IDistributePointsService service, Action<IDistributePointsActor, int> onUpdatedPoints = null, Action<IDistributePointsActor, bool> onHovered = null, string optionsText = null, bool isRewards = false, Action<bool> onSelected = null)
	{
		this.service = service;
		this.onUpdatedPoints = onUpdatedPoints;
		this.onHovered = onHovered;
		_onSelected = onSelected;
		titleImage.sprite = service.GetTitleIcon();
		List<IDistributePointsActor> actors = service.GetActors();
		HelperTools.NormalizePool(ref slots, (slotPrefab == null) ? slots[0].gameObject : slotPrefab.gameObject, slotsContainer, actors.Count);
		assignedSlots.Clear();
		for (int i = 0; i < actors.Count; i++)
		{
			UIDistributePointsSlot uIDistributePointsSlot = slots[i];
			IDistributePointsActor actor = actors[i];
			uIDistributePointsSlot.Subscribe();
			uIDistributePointsSlot.Init(actor, service.GetMaxPoints(actor), service.GetCurrentPoints(actor), delegate
			{
				AddPoint(actor);
			}, delegate
			{
				RemovePoint(actor);
			}, delegate(bool hovered)
			{
				OnHoveredSlot(actor, hovered);
			}, i, isRewards, optionsText);
			uIDistributePointsSlot.StateTag = stateTag;
			assignedSlots[actor] = uIDistributePointsSlot;
		}
		RefreshAssignedPoints();
		window.SetActive(value: true);
		controllerArea.Enable();
		EnableHotkeys();
	}

	public void Hide()
	{
		DisableHotkeys();
		foreach (UIDistributePointsSlot slot in slots)
		{
			slot.Release();
			slot.Unsubscribe();
		}
		controllerArea.Destroy();
		service = null;
		if (window != null)
		{
			window.SetActive(value: false);
		}
	}

	private void DisableHotkeys()
	{
		if (ShowHotkeys)
		{
			Hotkeys?.SetActiveHotkey("Select", value: false, ignoreActiveInHierarchy: true);
			Hotkeys?.SetActiveHotkey("Unselect", value: false, ignoreActiveInHierarchy: true);
		}
	}

	private void EnableHotkeys()
	{
		if (ShowHotkeys)
		{
			bool flag = CurrentHoveredSlot != null && CurrentHoveredSlot.IsSelected;
			Hotkeys?.SetActiveHotkey("Select", !flag, ignoreActiveInHierarchy: true);
			Hotkeys?.SetActiveHotkey("Unselect", flag, ignoreActiveInHierarchy: true);
		}
	}

	private void OnHoveredSlot(IDistributePointsActor actor, bool hovered)
	{
		if (IsShown)
		{
			CurrentHoveredSlot = assignedSlots[actor];
			onHovered?.Invoke(actor, hovered);
			if (InputManager.GamePadInUse)
			{
				RefreshHotkeys(actor);
			}
		}
	}

	private void RemovePoint(IDistributePointsActor actor)
	{
		service.RemovePoint(actor);
		Refresh();
		RefreshHotkeys(actor);
		onUpdatedPoints?.Invoke(actor, service.GetAssignedPoints(actor));
	}

	private void AddPoint(IDistributePointsActor actor)
	{
		service.AddPoint(actor);
		Refresh();
		RefreshHotkeys(actor);
		onUpdatedPoints?.Invoke(actor, service.GetAssignedPoints(actor));
	}

	public void Refresh()
	{
		if (IsShown)
		{
			RefreshAssignedPoints();
		}
	}

	private void RefreshHotkeys(IDistributePointsActor actor)
	{
		if (ShowHotkeys)
		{
			bool isSelected = assignedSlots[actor].IsSelected;
			Hotkeys?.SetActiveHotkey("Select", !isSelected, ignoreActiveInHierarchy: true);
			Hotkeys?.SetActiveHotkey("Unselect", isSelected, ignoreActiveInHierarchy: true);
			_onSelected?.Invoke(isSelected);
		}
	}

	private void RefreshAssignedPoints()
	{
		titleText.text = service.GetTitleText();
		int num = assignedSlots.Keys.Sum((IDistributePointsActor actor) => service.GetAssignedPoints(actor));
		foreach (KeyValuePair<IDistributePointsActor, UIDistributePointsSlot> assignedSlot in assignedSlots)
		{
			int assignedPoints = service.GetAssignedPoints(assignedSlot.Key);
			bool flag = service.CanAddPointsTo(assignedSlot.Key);
			bool flag2 = service.CanRemovePointsFrom(assignedSlot.Key);
			bool isHighlighted = num == 0 || assignedPoints > 0;
			Debug.Log("[DistributePopup] Refresh assigned points. " + $"Assigned points: {assignedPoints}; " + $"Can add points: {flag}; " + $"Can remove points: {flag2}");
			assignedSlot.Value.RefreshAssignedPoints(assignedPoints);
			assignedSlot.Value.EnableAddPoints(flag);
			assignedSlot.Value.EnableRemovePoints(flag2);
			assignedSlot.Value.RefreshGrayHighlight(isHighlighted);
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			controllerArea.Destroy();
		}
	}

	private void OnControllerAreaFocused()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(stateTag);
	}

	private void OnControllerAreaUnfocused()
	{
	}

	private void OnControllerAreaEnabled()
	{
		List<KeyAction> list = InputManager.GetKeyActionsAssociated(EKeyActionTag.AreaShortcuts).ToList();
		list.Remove(KeyAction.CONTROL_PARTY_PANEL);
		list.Add(KeyAction.UI_PREVIOUS_TAB);
		list.Add(KeyAction.UI_NEXT_TAB);
		InputManager.RequestDisableInput(this, list.ToArray());
	}

	private void OnControllerAreaDisabled()
	{
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
	}

	public void ProxyRedistributeHealth(GameAction gameAction)
	{
		IDistributePointsActor actor = service.GetActor(gameAction);
		if (actor != null && assignedSlots.ContainsKey(actor))
		{
			if (gameAction.SupplementaryDataBoolean)
			{
				AddPoint(actor);
			}
			else
			{
				RemovePoint(actor);
			}
			return;
		}
		throw new Exception("Error redistributing damage. Actor returns null (Actor ID: " + gameAction.ActorID + ").");
	}
}
