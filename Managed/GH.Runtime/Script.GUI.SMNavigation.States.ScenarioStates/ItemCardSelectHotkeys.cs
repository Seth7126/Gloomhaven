using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class ItemCardSelectHotkeys
{
	private readonly SelectHotkeys _selectHotkeys = new SelectHotkeys();

	public bool UpdateOnHover { get; set; }

	public bool CheckSelectable { get; set; }

	public ItemCardSelectHotkeys(bool updateOnHover = true, bool checkSelectable = true)
	{
		UpdateOnHover = updateOnHover;
		CheckSelectable = checkSelectable;
	}

	public void Enter(IHotkeySession session)
	{
		_selectHotkeys.Enter(session);
		if (UpdateOnHover)
		{
			ItemCardPickerSlot.ItemCardHoveringStateChanged += AbilityCardUIOnCardHoveringStateChanged;
		}
		ItemCardPickerSlot.ItemCardSelectionStateChanged += AbilityCardUIOnCardSelectionStateChanged;
	}

	public void Exit()
	{
		ItemCardPickerSlot.ItemCardHoveringStateChanged -= AbilityCardUIOnCardHoveringStateChanged;
		ItemCardPickerSlot.ItemCardSelectionStateChanged -= AbilityCardUIOnCardSelectionStateChanged;
	}

	private void AbilityCardUIOnCardSelectionStateChanged(ItemCardPickerSlot item, bool _)
	{
		SetSelectHotkeys(item);
	}

	private void AbilityCardUIOnCardHoveringStateChanged(ItemCardPickerSlot item, bool _)
	{
		SetSelectHotkeys(item);
	}

	public void SetSelectHotkeys(ItemCardPickerSlot item)
	{
		bool flag = !CheckSelectable || item.IsSelectable;
		_selectHotkeys.SetShown(flag && !item.Selected, flag && item.Selected);
	}
}
