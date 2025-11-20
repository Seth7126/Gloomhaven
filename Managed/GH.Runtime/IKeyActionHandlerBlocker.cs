using System;

public interface IKeyActionHandlerBlocker
{
	bool IsBlock { get; }

	event Action BlockStateChanged;

	void Clear();
}
