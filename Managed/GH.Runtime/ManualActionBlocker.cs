using System;

public class ManualActionBlocker : IKeyActionHandlerBlocker
{
	public bool IsBlock { get; private set; } = true;

	public event Action BlockStateChanged;

	public void UnBlock()
	{
		IsBlock = false;
		this.BlockStateChanged?.Invoke();
	}

	public void Clear()
	{
		this.BlockStateChanged = null;
	}
}
