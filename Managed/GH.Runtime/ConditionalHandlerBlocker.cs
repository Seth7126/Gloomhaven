using System;

public class ConditionalHandlerBlocker : IKeyActionHandlerBlocker
{
	private Func<bool> _blockFunc;

	public bool IsBlock => CheckBlock();

	public event Action BlockStateChanged;

	public ConditionalHandlerBlocker(Func<bool> blockFunc)
	{
		_blockFunc = blockFunc;
	}

	public void StateChangedHandler()
	{
		this.BlockStateChanged?.Invoke();
	}

	public bool CheckBlock()
	{
		return _blockFunc();
	}

	public void Clear()
	{
		this.BlockStateChanged = null;
		_blockFunc = null;
	}
}
