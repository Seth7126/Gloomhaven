using System;

public class LongHotkeyOptionPresenter : OptionPresenter<LongHotkeyOptionView>
{
	private readonly Action _onPress;

	private readonly Action _onShortPress;

	public LongHotkeyOptionPresenter(LongHotkeyOptionView view, Action onPress, Action onShortPress)
		: base(view)
	{
		_onPress = onPress;
		_onShortPress = onShortPress;
	}

	public override void Enter()
	{
		_view.OnPressed += OnPressed;
		_view.OnShortPressed += OnShortPressed;
	}

	public override void Exit()
	{
		_view.OnPressed -= OnPressed;
		_view.OnShortPressed -= OnShortPressed;
	}

	private void OnPressed()
	{
		_onPress?.Invoke();
	}

	private void OnShortPressed()
	{
		_onShortPress?.Invoke();
	}
}
