using System;
using UnityEngine;

public class LongHotkeyOptionView : HotkeyOptionView
{
	[SerializeField]
	private LongConfirmHandler _confirmHandler;

	public event Action OnShortPressed;

	protected override void OnHotkeyPressed()
	{
		_confirmHandler.Pressed(base.InvokeOnPressed, InvokeOnShortPressed);
	}

	private void InvokeOnShortPressed()
	{
		this.OnShortPressed?.Invoke();
	}
}
