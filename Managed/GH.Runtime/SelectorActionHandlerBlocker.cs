using System;

public class SelectorActionHandlerBlocker : IKeyActionHandlerBlocker
{
	private readonly IKeyActionHandlerBlocker _firstBlocker;

	private readonly IKeyActionHandlerBlocker _secondBlocker;

	private readonly Func<bool> _condition;

	public bool IsBlock
	{
		get
		{
			if (!_condition())
			{
				return _secondBlocker.IsBlock;
			}
			return _firstBlocker.IsBlock;
		}
	}

	public event Action BlockStateChanged;

	public SelectorActionHandlerBlocker(IKeyActionHandlerBlocker firstBlocker, IKeyActionHandlerBlocker secondBlocker, Func<bool> condition)
	{
		_firstBlocker = firstBlocker;
		_secondBlocker = secondBlocker;
		_condition = condition;
		_firstBlocker.BlockStateChanged += OnBlockStateChanged;
		_secondBlocker.BlockStateChanged += OnBlockStateChanged;
	}

	private void OnBlockStateChanged()
	{
		this.BlockStateChanged?.Invoke();
	}

	public void Clear()
	{
		if (_firstBlocker != null)
		{
			_firstBlocker.BlockStateChanged -= OnBlockStateChanged;
			_firstBlocker.Clear();
		}
		if (_secondBlocker != null)
		{
			_secondBlocker.BlockStateChanged -= OnBlockStateChanged;
			_secondBlocker.Clear();
		}
	}
}
