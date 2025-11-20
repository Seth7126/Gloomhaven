using System;

public class HotkeyOptionPresenter : OptionPresenter<HotkeyOptionView>
{
	private readonly Action _onPress;

	public HotkeyOptionPresenter(HotkeyOptionView view, Action onPress)
		: base(view)
	{
		_onPress = onPress;
	}

	public override void Enter()
	{
		_view.OnPressed += OnPressed;
	}

	public override void Exit()
	{
		_view.OnPressed -= OnPressed;
	}

	private void OnPressed()
	{
		_onPress?.Invoke();
	}
}
