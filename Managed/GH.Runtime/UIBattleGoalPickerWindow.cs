using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GLOO.Introduction;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using SM.Gamepad;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIBattleGoalPickerWindow : MonoBehaviour, IEscapable
{
	private UIWindow window;

	[SerializeField]
	private VerticalPointerUI verticalPointer;

	[SerializeField]
	private VerticalPointerUI arrow;

	[SerializeField]
	private List<UIBattleGoalPickerSlot> slotPool;

	[SerializeField]
	private RectTransform slotsContainer;

	[SerializeField]
	private UIIntroduce introduction;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private Hotkey _backHotKey;

	[SerializeField]
	private Hotkey _rightStickHotkey;

	[SerializeField]
	private GameObject _commonBackHotkey;

	[SerializeField]
	private PanelHotkeyContainer _hotkeyContainer;

	[SerializeField]
	private UiNavigationGroup _slotsUiNavigationGroup;

	[SerializeField]
	private bool _isAllowedToEscapeDuringSave;

	[SerializeField]
	private RectTransform _goalsViewport;

	private IBattleGoalService service;

	private ICharacterService characterData;

	private UIBattleGoalPickerSlot selectedSlot;

	private Action onHidden;

	private Vector2 slotContainerPosition;

	private Action<bool> onBattleGoalUpdated;

	private readonly Dictionary<string, UINavigationSelectable> _selectedSlots = new Dictionary<string, UINavigationSelectable>();

	private UIBattleGoalPickerSlot _hoveredSlot;

	public bool IsOpen => window.IsOpen;

	public bool SelectionAllowed { get; set; } = true;

	public bool IsAllowedToEscapeDuringSave => _isAllowedToEscapeDuringSave;

	private bool AllowPick
	{
		get
		{
			if (FFSNetwork.IsOnline)
			{
				if (FFSNetwork.IsOnline)
				{
					return characterData.IsUnderMyControl;
				}
				return false;
			}
			return true;
		}
	}

	private void Awake()
	{
		window = GetComponent<UIWindow>();
		window.onHidden.AddListener(OnHidden);
		slotContainerPosition = slotsContainer.anchoredPosition;
		controllerArea.OnFocusedArea.AddListener(OnFocused);
		controllerArea.OnUnfocusedArea.AddListener(OnUnfocused);
		if (InputManager.GamePadInUse)
		{
			_rightStickHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
	}

	private void OnEnable()
	{
		if (InputManager.GamePadInUse)
		{
			UIBattleGoalPickerSlot.SlotPicked += OnSlotPickedGamepad;
			_hotkeyContainer.SetActiveHotkey("Unselect", value: false);
			_hotkeyContainer.SetActiveHotkey("Select", value: false);
			_hotkeyContainer.SetActiveHotkey("Back", value: true, ignoreActiveInHierarchy: true);
			_rightStickHotkey.gameObject.SetActive(value: false);
			_hotkeyContainer.SetActive(value: true);
		}
	}

	private void OnDisable()
	{
		if (InputManager.GamePadInUse)
		{
			_hotkeyContainer.SetActive(value: false);
			UIBattleGoalPickerSlot.SlotPicked -= OnSlotPickedGamepad;
		}
	}

	private void UpdateSelectionHotkeysActivity()
	{
		if (InputManager.GamePadInUse && _hoveredSlot != null)
		{
			bool flag = _hoveredSlot == selectedSlot;
			if (FFSNetwork.IsOnline)
			{
				bool flag2 = characterData?.IsUnderMyControl ?? false;
				_hotkeyContainer.SetActiveHotkey("Unselect", flag2 && flag);
				_hotkeyContainer.SetActiveHotkey("Select", flag2 && !flag);
			}
			else
			{
				_hotkeyContainer.SetActiveHotkey("Unselect", flag);
				_hotkeyContainer.SetActiveHotkey("Select", !flag);
			}
		}
		else if (InputManager.GamePadInUse && FFSNetwork.IsOnline)
		{
			ICharacterService characterService = characterData;
			if (characterService == null || !characterService.IsUnderMyControl)
			{
				_hotkeyContainer.SetActiveHotkey("Unselect", value: false);
				_hotkeyContainer.SetActiveHotkey("Select", value: false);
			}
		}
	}

	public void Display(CQuestState quest, ICharacterService characterData, RectTransform position, Action<bool> onBattleGoalUpdated = null, Action onHidden = null)
	{
		Display(new BattleGoalService(quest), characterData, position, onBattleGoalUpdated, onHidden);
	}

	public void Display(IBattleGoalService service, ICharacterService characterData, RectTransform position, Action<bool> onBattleGoalUpdated = null, Action onHidden = null)
	{
		this.characterData = characterData;
		this.service = service;
		this.onBattleGoalUpdated = onBattleGoalUpdated;
		this.onHidden = onHidden;
		AnalyticsWrapper.LogScreenDisplay(AWScreenName.battle_goal_selection);
		CreateBattleGoalChoices(service.GetAvailableBattleGoals(characterData.CharacterID), service.GetChosenBattleGoal(characterData.CharacterID));
		verticalPointer.PointAt(position);
		arrow.PointAt(position);
		RefreshPosition();
		window.Show();
		controllerArea.Enable();
		ShowIntroduction();
		if (!InputManager.GamePadInUse)
		{
			StartCoroutine(RefreshPositionDelayed());
		}
		UpdateSelectionHotkeysActivity();
	}

	private void ShowIntroduction()
	{
		if (!service.HasShownBattleGoalIntro)
		{
			introduction.Show();
			service.SetBattleGoalIntroShown();
		}
	}

	private IEnumerator RefreshPositionDelayed()
	{
		yield return null;
		RefreshPosition();
	}

	private void RefreshPosition()
	{
		slotsContainer.anchoredPosition = slotContainerPosition;
		LayoutRebuilder.ForceRebuildLayoutImmediate(slotsContainer);
		slotsContainer.position += slotsContainer.DeltaWorldPositionToFitRectTransform(UIManager.Instance.UICamera, _goalsViewport);
	}

	public void Hide()
	{
		window.Hide();
	}

	private void OnHidden()
	{
		StopAllCoroutines();
		controllerArea.Destroy();
		onHidden?.Invoke();
	}

	private void CreateBattleGoalChoices(List<CBattleGoalState> battleGoals, CBattleGoalState selected)
	{
		selectedSlot = null;
		_hoveredSlot = null;
		HelperTools.NormalizePool(ref slotPool, slotPool[0].gameObject, slotPool[0].transform.parent, battleGoals.Count);
		for (int i = 0; i < battleGoals.Count; i++)
		{
			UIBattleGoalPickerSlot slot = slotPool[i];
			if (InputManager.GamePadInUse && AllowPick && _selectedSlots.ContainsKey(characterData.CharacterID))
			{
				_slotsUiNavigationGroup.SetDefaultElementToSelect(_selectedSlots[characterData.CharacterID]);
				Singleton<UINavigation>.Instance.NavigationManager.TrySelect(_selectedSlots[characterData.CharacterID]);
			}
			slot.SetBattleGoal(battleGoals[i], characterData, delegate(bool toggled, CBattleGoalState battleGoal)
			{
				OnToggled(toggled, slot, battleGoal);
			}, delegate(bool hovered)
			{
				OnHovered(hovered, slot);
			});
			if (selected?.ID == battleGoals[i].ID)
			{
				selectedSlot = slot;
			}
		}
		if (selectedSlot != null)
		{
			selectedSlot.Select();
		}
	}

	private void OnToggled(bool selected, UIBattleGoalPickerSlot slot, CBattleGoalState battleGoal)
	{
		if (selected)
		{
			if (selectedSlot != slot)
			{
				if (FFSNetwork.IsOnline)
				{
					NewPartyDisplayUI.PartyDisplay.BattleGoalWindow.SelectionAllowed = false;
				}
				selectedSlot?.Deselect();
				selectedSlot = slot;
				selectedSlot.HighlightAssign(delegate
				{
					onBattleGoalUpdated?.Invoke(obj: true);
				});
				service.ChooseBattleGoal(characterData, battleGoal);
				if (slot != null && !string.IsNullOrEmpty(slot.Id))
				{
					_selectedSlots[characterData.CharacterID] = slot.UINavigationSelectable;
				}
			}
			for (int num = 0; num < slotPool.Count && slotPool[num].gameObject.activeSelf; num++)
			{
				if (slotPool[num] != slot)
				{
					slotPool[num].SetFocused(focused: false);
				}
			}
			selectedSlot.SetFocused(focused: true);
			selectedSlot.Highlight(on: true);
		}
		else
		{
			if (selectedSlot == slot)
			{
				selectedSlot = null;
				for (int num2 = 0; num2 < slotPool.Count && slotPool[num2].gameObject.activeSelf; num2++)
				{
					slotPool[num2].SetFocused(focused: true);
				}
			}
			else
			{
				selectedSlot.SetFocused(focused: false);
			}
			service.RemoveBattleGoal(characterData, battleGoal);
			onBattleGoalUpdated?.Invoke(obj: false);
		}
		UpdateSelectionHotkeysActivity();
	}

	private void OnHovered(bool isHovered, UIBattleGoalPickerSlot slot)
	{
		_hoveredSlot = slot;
		if (isHovered)
		{
			for (int i = 0; i < slotPool.Count && slotPool[i].gameObject.activeSelf; i++)
			{
				if (slotPool[i] == slot)
				{
					slotPool[i].SetFocused(focused: true);
					slotPool[i].Highlight(on: true);
				}
				else
				{
					slotPool[i].Highlight(on: false);
					slotPool[i].SetFocused(selectedSlot == null || slotPool[i] == selectedSlot);
				}
			}
		}
		else
		{
			for (int j = 0; j < slotPool.Count && slotPool[j].gameObject.activeSelf; j++)
			{
				if (selectedSlot == null || slotPool[j] != selectedSlot)
				{
					slotPool[j].ClearHighlight();
				}
				else
				{
					slotPool[j].Highlight(on: true);
				}
				slotPool[j].SetFocused(selectedSlot == null || slotPool[j] == selectedSlot);
			}
		}
		UpdateSelectionHotkeysActivity();
	}

	private void OnSlotPickedGamepad()
	{
		onBattleGoalUpdated?.Invoke(obj: true);
	}

	private void ToggleCurrentHoveredSlot()
	{
		if (_hoveredSlot != null)
		{
			_hoveredSlot.Toggle();
		}
	}

	private void OnFocused()
	{
		if (Singleton<UINavigation>.Instance.StateMachine.CurrentState is BattleGoalPickerState battleGoalPickerState)
		{
			if (AllowPick)
			{
				battleGoalPickerState.FocusSlot((selectedSlot != null) ? selectedSlot : slotPool.First());
			}
		}
		else
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.BattleGoalPicker, AllowPick ? selectedSlot : null);
		}
		Singleton<UILoadoutManager>.Instance.OnShortPressed += ToggleCurrentHoveredSlot;
		for (int i = 0; i < slotPool.Count; i++)
		{
			UIBattleGoalPickerSlot uIBattleGoalPickerSlot = slotPool[i];
			if (!uIBattleGoalPickerSlot.gameObject.activeSelf)
			{
				break;
			}
			uIBattleGoalPickerSlot.OnFocused();
		}
		if (InputManager.GamePadInUse)
		{
			_hotkeyContainer.SetActive(value: true);
			if (_commonBackHotkey != null)
			{
				_commonBackHotkey.gameObject.SetActive(value: false);
			}
			slotPool.ForEach(delegate(UIBattleGoalPickerSlot slot)
			{
				slot.SetDimmer(isDimmer: false);
			});
			_rightStickHotkey.gameObject.SetActive(value: false);
		}
		UIWindowManager.RegisterEscapable(this);
	}

	private void OnUnfocused()
	{
		UIWindowManager.UnregisterEscapable(this);
		Singleton<UILoadoutManager>.Instance.OnShortPressed -= ToggleCurrentHoveredSlot;
		for (int i = 0; i < slotPool.Count && slotPool[i].gameObject.activeSelf; i++)
		{
		}
		if (InputManager.GamePadInUse)
		{
			_hotkeyContainer.SetActive(value: false);
			if (_commonBackHotkey != null)
			{
				_commonBackHotkey.gameObject.SetActive(value: true);
			}
			slotPool.ForEach(delegate(UIBattleGoalPickerSlot slot)
			{
				slot.SetDimmer(isDimmer: true);
			});
			_rightStickHotkey.gameObject.SetActive(value: true);
		}
	}

	public bool Escape()
	{
		Hide();
		return true;
	}

	public int Order()
	{
		return 0;
	}
}
