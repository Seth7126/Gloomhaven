using System;

public class ControllerInputAreaDialog : ControllerInputAreaCustom
{
	public ControllerInputAreaDialog(Action onFocused = null, Action onUnfocused = null)
		: base("Dialog", onFocused, onUnfocused, stackArea: false, EKeyActionTag.AreaShortcuts)
	{
	}
}
