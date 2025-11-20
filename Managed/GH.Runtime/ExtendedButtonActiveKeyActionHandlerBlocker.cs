using System;

public class ExtendedButtonActiveKeyActionHandlerBlocker : IKeyActionHandlerBlocker
{
	private readonly ExtendedButton _button;

	public bool IsBlock => !_button.gameObject.activeInHierarchy;

	public event Action BlockStateChanged;

	public ExtendedButtonActiveKeyActionHandlerBlocker(ExtendedButton button)
	{
		_button = button;
		ExtendedButton button2 = _button;
		button2.ActiveChanged = (Action<bool>)Delegate.Combine(button2.ActiveChanged, new Action<bool>(Button_ActiveChanged));
	}

	private void Button_ActiveChanged(bool value)
	{
		this.BlockStateChanged?.Invoke();
	}

	public void Clear()
	{
		this.BlockStateChanged = null;
		ExtendedButton button = _button;
		button.ActiveChanged = (Action<bool>)Delegate.Remove(button.ActiveChanged, new Action<bool>(Button_ActiveChanged));
	}
}
