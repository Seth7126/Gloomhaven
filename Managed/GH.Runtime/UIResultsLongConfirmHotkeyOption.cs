using UnityEngine;

public class UIResultsLongConfirmHotkeyOption : UIResultsHotkeyOption
{
	[SerializeField]
	private LongConfirmHandler _confirmHandler;

	protected override void HotkeyAction()
	{
		_confirmHandler.Pressed(base.InvokeAction);
	}
}
