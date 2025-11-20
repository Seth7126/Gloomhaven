using System;

public class UnifiedOptionPresenter<T> where T : class
{
	private readonly T _keyboardOption;

	private readonly T _gamepadOption;

	private readonly Action<T> _onOptionEnter;

	private readonly Action<T> _onOptionExit;

	private T _current;

	public T Current => _current;

	public UnifiedOptionPresenter(T keyboardOption, T gamepadOption, Action<T> onOptionEnter, Action<T> onOptionExit)
	{
		_keyboardOption = keyboardOption;
		_gamepadOption = gamepadOption;
		_onOptionEnter = onOptionEnter;
		_onOptionExit = onOptionExit;
	}

	public void Enter()
	{
		SwitchOption(GetCurrentOption());
	}

	public void Exit()
	{
		SwitchOption(null);
	}

	private void SwitchOption(T option)
	{
		if (_current != null)
		{
			_onOptionExit?.Invoke(_current);
		}
		_current = option;
		if (_current != null)
		{
			_onOptionEnter?.Invoke(_current);
		}
	}

	private T GetCurrentOption()
	{
		if (InputManager.GamePadInUse)
		{
			return _gamepadOption;
		}
		return _keyboardOption;
	}
}
