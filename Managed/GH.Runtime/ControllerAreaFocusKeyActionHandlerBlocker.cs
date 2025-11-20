using System;

public class ControllerAreaFocusKeyActionHandlerBlocker : IKeyActionHandlerBlocker
{
	private readonly ControllerInputArea _area;

	public bool IsBlock => !_area.IsFocused;

	public event Action BlockStateChanged;

	public ControllerAreaFocusKeyActionHandlerBlocker(ControllerInputArea area)
	{
		_area = area;
		_area.OnFocused.AddListener(OnFocusChanged);
		_area.OnUnfocused.AddListener(OnFocusChanged);
	}

	private void OnFocusChanged()
	{
		this.BlockStateChanged?.Invoke();
	}

	public void Clear()
	{
		this.BlockStateChanged = null;
		_area.OnFocused.RemoveListener(OnFocusChanged);
		_area.OnUnfocused.RemoveListener(OnFocusChanged);
	}
}
