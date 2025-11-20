using System;

public class SimpleKeyActionHandlerBlocker : IKeyActionHandlerBlocker
{
	public bool IsBlock { get; private set; }

	public event Action BlockStateChanged;

	public SimpleKeyActionHandlerBlocker(bool isBlock = false)
	{
		IsBlock = isBlock;
	}

	public void Clear()
	{
		this.BlockStateChanged = null;
	}

	public void SetBlock(bool value)
	{
		IsBlock = value;
		this.BlockStateChanged?.Invoke();
	}
}
