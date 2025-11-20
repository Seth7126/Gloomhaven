using Script.GUI.Buttons;
using TMPro;
using UnityEngine;

namespace Script.GUI.PartyDisplay.BattleGoals;

public class UIBattleGoalSlotToggleViewBehaviour : MonoBehaviour
{
	[SerializeField]
	private ToggleView _toggleView;

	[SerializeField]
	private UIBattleGoalPickerSlot _slot;

	[SerializeField]
	private TextMeshProUGUI _text;

	private void OnEnable()
	{
		UpdateView();
		_slot.OnHover += _toggleView.Hover;
		_slot.OnSelect += OnSelect;
		_slot.OnFocus += UpdateView;
	}

	private void OnDisable()
	{
		_slot.OnHover -= _toggleView.Hover;
		_slot.OnSelect -= OnSelect;
		_slot.OnFocus -= UpdateView;
	}

	private void OnSelect(bool selected)
	{
		_toggleView.Select(selected);
		_text.enabled = selected;
	}

	private void UpdateView()
	{
		_toggleView.Hover(_slot.IsHovered);
		OnSelect(_slot.IsSelected);
	}
}
