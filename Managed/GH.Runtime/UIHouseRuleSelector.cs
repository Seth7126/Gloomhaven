using System;
using System.Linq;
using AsmodeeNet.Foundation;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHouseRuleSelector : UIModeSelector<StateShared.EHouseRulesFlag>, IMoveHandler, IEventSystemHandler, ISubmitHandler
{
	[SerializeField]
	private ExtendedButton m_SelectableContainer;

	[SerializeField]
	private UITextTooltipTarget[] m_Tooltips;

	[SerializeField]
	private UIHouseRuleSelectorElement[] _selectorElements;

	private int focused = -1;

	protected override void Awake()
	{
		base.Awake();
		if (m_SelectableContainer != null)
		{
			m_SelectableContainer.onSelected.AddListener(FocusOnSelected);
			m_SelectableContainer.onDeselected.AddListener(delegate
			{
				Focus(-1);
			});
		}
	}

	private void OnEnable()
	{
		if (InputManager.GamePadInUse)
		{
			TooltipsVisibilityHelper instance = TooltipsVisibilityHelper.Instance;
			instance.HideTooltipsEvent = (Action)Delegate.Combine(instance.HideTooltipsEvent, new Action(OnHideTooltips));
			TooltipsVisibilityHelper instance2 = TooltipsVisibilityHelper.Instance;
			instance2.ShowTooltipsEvent = (Action)Delegate.Combine(instance2.ShowTooltipsEvent, new Action(OnShowTooltips));
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			DisableNavigation();
			if (InputManager.GamePadInUse)
			{
				TooltipsVisibilityHelper instance = TooltipsVisibilityHelper.Instance;
				instance.HideTooltipsEvent = (Action)Delegate.Remove(instance.HideTooltipsEvent, new Action(OnHideTooltips));
				TooltipsVisibilityHelper instance2 = TooltipsVisibilityHelper.Instance;
				instance2.ShowTooltipsEvent = (Action)Delegate.Remove(instance2.ShowTooltipsEvent, new Action(OnShowTooltips));
			}
		}
	}

	private void OnHideTooltips()
	{
		for (int i = 0; i < _selectorElements.Length; i++)
		{
			_selectorElements[i].ShouldDisplayTooltips = false;
			_selectorElements[i].OnHideTooltips();
		}
	}

	private void OnShowTooltips()
	{
		for (int i = 0; i < _selectorElements.Length; i++)
		{
			_selectorElements[i].ShouldDisplayTooltips = true;
			_selectorElements[i].OnShowTooltips();
		}
	}

	private void FocusOnSelected()
	{
		Focus(Math.Max(0, m_Modes.IndexOf(selectedMode)));
	}

	private void Focus(int index)
	{
		if (focused != index)
		{
			if (focused >= 0)
			{
				ExecuteEvents.Execute(m_Modes[focused].toggle.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
				focused = -1;
			}
			focused = index;
			if (focused >= 0)
			{
				ExecuteEvents.Execute(m_Modes[focused].toggle.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
			}
		}
	}

	public override void SetMode(StateShared.EHouseRulesFlag mode)
	{
		ModeConfig modeConfig = m_Modes.FirstOrDefault((ModeConfig it) => it.Value != StateShared.EHouseRulesFlag.None && StateShared.HasHouseRuleFlag(mode, it.Value));
		SetMode(modeConfig ?? m_Modes.First((ModeConfig it) => it.Value == StateShared.EHouseRulesFlag.None));
	}

	public void SetInteractable(bool interactable)
	{
		for (int i = 0; i < m_Modes.Count; i++)
		{
			m_Modes[i].toggle.interactable = interactable;
		}
	}

	public void EnableNavigation()
	{
		m_SelectableContainer.SetNavigation(Navigation.Mode.Vertical);
	}

	public void DisableNavigation()
	{
		Focus(-1);
		if (m_SelectableContainer != null)
		{
			m_SelectableContainer.DisableNavigation();
		}
	}

	public void OnMove(AxisEventData eventData)
	{
		switch (eventData.moveDir)
		{
		case MoveDirection.Left:
		{
			int previousToggleIndex = GetPreviousToggleIndex(focused);
			if (previousToggleIndex >= 0)
			{
				Focus(previousToggleIndex);
			}
			break;
		}
		case MoveDirection.Right:
		{
			int nextToggleIndex = GetNextToggleIndex(focused);
			if (nextToggleIndex >= 0)
			{
				Focus(nextToggleIndex);
			}
			break;
		}
		}
	}

	private int GetNextToggleIndex(int currentIndex)
	{
		currentIndex = Math.Max(currentIndex, 0);
		for (int i = 0; i < m_Modes.Count; i++)
		{
			int num = (currentIndex + 1) % m_Modes.Count;
			if (m_Modes[num].toggle.interactable)
			{
				return num;
			}
		}
		return -1;
	}

	private int GetPreviousToggleIndex(int currentIndex)
	{
		currentIndex = Math.Max(currentIndex, 0);
		for (int num = currentIndex - 1; num >= 0; num--)
		{
			if (m_Modes[num].toggle.interactable)
			{
				return num;
			}
		}
		for (int num2 = m_Modes.Count - 1; num2 >= currentIndex; num2--)
		{
			if (m_Modes[num2].toggle.interactable)
			{
				return num2;
			}
		}
		return -1;
	}

	public void OnSubmit(BaseEventData eventData)
	{
		if (focused >= 0)
		{
			Toggle toggle = m_Modes[focused].toggle;
			if (toggle.IsInteractable())
			{
				toggle.isOn = true;
			}
		}
	}

	public void SetTooltip(string tooltip)
	{
		if (tooltip.IsNullOrEmpty())
		{
			for (int i = 0; i < m_Tooltips.Length; i++)
			{
				m_Tooltips[i].enabled = false;
			}
			return;
		}
		for (int j = 0; j < m_Tooltips.Length; j++)
		{
			m_Tooltips[j].SetText(tooltip);
			m_Tooltips[j].enabled = true;
		}
	}
}
