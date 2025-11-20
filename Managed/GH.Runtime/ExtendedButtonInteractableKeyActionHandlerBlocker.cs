using System;

public class ExtendedButtonInteractableKeyActionHandlerBlocker : IKeyActionHandlerBlocker
{
	private readonly ExtendedButton _button;

	public bool IsBlock => !_button.interactable;

	public event Action BlockStateChanged;

	public ExtendedButtonInteractableKeyActionHandlerBlocker(ExtendedButton button)
	{
		_button = button;
		ExtendedButton button2 = _button;
		button2.InteractableChanged = (Action<bool>)Delegate.Combine(button2.InteractableChanged, new Action<bool>(InteractableChanged));
	}

	private void InteractableChanged(bool value)
	{
		this.BlockStateChanged?.Invoke();
	}

	public void Clear()
	{
		this.BlockStateChanged = null;
		ExtendedButton button = _button;
		button.InteractableChanged = (Action<bool>)Delegate.Remove(button.InteractableChanged, new Action<bool>(InteractableChanged));
	}
}
