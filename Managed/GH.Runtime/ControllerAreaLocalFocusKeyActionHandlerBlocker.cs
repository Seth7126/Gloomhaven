using System;

public class ControllerAreaLocalFocusKeyActionHandlerBlocker : IKeyActionHandlerBlocker
{
	private readonly ControllerInputAreaLocal _area;

	public bool IsBlock => !_area.IsFocused;

	public event Action BlockStateChanged;

	public ControllerAreaLocalFocusKeyActionHandlerBlocker(ControllerInputAreaLocal area)
	{
		_area = area;
		_area.OnFocusedArea.AddListener(OnFocusChanged);
		_area.OnUnfocusedArea.AddListener(OnFocusChanged);
	}

	private void OnFocusChanged()
	{
		this.BlockStateChanged?.Invoke();
	}

	public void Clear()
	{
		this.BlockStateChanged = null;
		_area.OnFocusedArea.RemoveListener(OnFocusChanged);
		_area.OnUnfocusedArea.RemoveListener(OnFocusChanged);
	}
}
