using System;

public class ExtendedButtonSelectKeyActionHandlerBlocker : IKeyActionHandlerBlocker
{
	private readonly ExtendedButton _button;

	public bool IsBlock => !_button.IsSelected;

	public event Action BlockStateChanged;

	public ExtendedButtonSelectKeyActionHandlerBlocker(ExtendedButton button)
	{
		_button = button;
		_button.onSelected.AddListener(OnSelectChanged);
		_button.onDeselected.AddListener(OnSelectChanged);
	}

	private void OnSelectChanged()
	{
		this.BlockStateChanged?.Invoke();
	}

	public void Clear()
	{
		this.BlockStateChanged = null;
		_button.onSelected.RemoveListener(OnSelectChanged);
		_button.onDeselected.RemoveListener(OnSelectChanged);
	}
}
