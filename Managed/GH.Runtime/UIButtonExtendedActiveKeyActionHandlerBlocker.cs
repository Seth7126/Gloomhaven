using System;
using UnityEngine.UI;

public class UIButtonExtendedActiveKeyActionHandlerBlocker : IKeyActionHandlerBlocker
{
	private readonly UIButtonExtended _button;

	public bool IsBlock => !_button.gameObject.activeInHierarchy;

	public event Action BlockStateChanged;

	public UIButtonExtendedActiveKeyActionHandlerBlocker(UIButtonExtended button)
	{
		_button = button;
		_button.onStateChange.AddListener(Button_ActiveChanged);
	}

	private void Button_ActiveChanged(UIButtonExtended.VisualState visualState, bool value)
	{
		this.BlockStateChanged?.Invoke();
	}

	public void Clear()
	{
		this.BlockStateChanged = null;
		_button.onStateChange.RemoveListener(Button_ActiveChanged);
	}
}
