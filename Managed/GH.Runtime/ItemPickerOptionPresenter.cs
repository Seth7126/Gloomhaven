using System;

public class ItemPickerOptionPresenter
{
	private readonly IOptionPresenter _optionPresenter;

	private readonly Action<bool> _confirmAvailableChanged;

	public ItemPickerOptionPresenter(IOptionPresenter optionPresenter, Action<bool> confirmAvailableChanged = null)
	{
		_optionPresenter = optionPresenter;
		_confirmAvailableChanged = confirmAvailableChanged;
	}

	public void Enter()
	{
		_optionPresenter.Enter();
		_optionPresenter.SetShown(shown: true);
	}

	public void Exit()
	{
		_optionPresenter.SetShown(shown: false);
		_optionPresenter.Exit();
	}

	public void Hide()
	{
		_optionPresenter.SetShown(shown: false);
	}

	public void SetConfirmAvailable(bool available)
	{
		_optionPresenter.SetInteractable(available);
		_confirmAvailableChanged?.Invoke(available);
	}
}
