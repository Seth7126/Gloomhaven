using System;

public class ControllerAreaCustomFocusKeyActionHandlerBlocker : IKeyActionHandlerBlocker
{
	private readonly ControllerInputAreaCustom _area;

	public bool IsBlock => !_area.IsFocused;

	public event Action BlockStateChanged;

	public ControllerAreaCustomFocusKeyActionHandlerBlocker(ControllerInputAreaCustom area)
	{
		_area = area;
		_area.OnFocused += OnFocusChanged;
		_area.OnUnfocused += OnFocusChanged;
	}

	private void OnFocusChanged()
	{
		this.BlockStateChanged?.Invoke();
	}

	public void Clear()
	{
		this.BlockStateChanged = null;
		_area.OnFocused -= OnFocusChanged;
		_area.OnUnfocused -= OnFocusChanged;
	}
}
