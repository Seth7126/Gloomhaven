using UnityEngine.EventSystems;

namespace SM.Gamepad;

public class SelectableHotkey : Hotkey, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	private bool _isSelected;

	private void Awake()
	{
		DisplayHotkey(active: false);
	}

	public void OnSelect(BaseEventData eventData)
	{
		_isSelected = true;
		DisplayHotkey(active: true);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		_isSelected = false;
		DisplayHotkey(active: false);
	}

	protected override void OnInputEvent(string eventName)
	{
		if (_isSelected)
		{
			base.OnInputEvent(eventName);
		}
	}
}
