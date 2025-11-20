using System;
using TMPro;

public class InputFieldSelectKeyActionHandlerBlocker : IKeyActionHandlerBlocker
{
	private readonly TMP_InputField _inputField;

	public bool IsBlock { get; private set; }

	public event Action BlockStateChanged;

	public InputFieldSelectKeyActionHandlerBlocker(TMP_InputField inputField, bool isBlock)
	{
		_inputField = inputField;
		_inputField.onSelect.AddListener(OnSelect);
		_inputField.onDeselect.AddListener(OnDeselect);
		IsBlock = isBlock;
	}

	private void OnSelect(string value)
	{
		IsBlock = false;
		this.BlockStateChanged?.Invoke();
	}

	private void OnDeselect(string value)
	{
		IsBlock = true;
		this.BlockStateChanged?.Invoke();
	}

	public void Clear()
	{
		this.BlockStateChanged = null;
		_inputField.onSelect.RemoveListener(OnSelect);
		_inputField.onDeselect.RemoveListener(OnDeselect);
	}
}
