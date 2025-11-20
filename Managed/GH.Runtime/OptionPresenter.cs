public abstract class OptionPresenter<T> : IOptionPresenter where T : IOptionView
{
	protected readonly T _view;

	public OptionPresenter(T view)
	{
		_view = view;
	}

	public abstract void Enter();

	public abstract void Exit();

	public void SetInteractable(bool interactable)
	{
		_view.SetInteractable(interactable);
	}

	public void SetShown(bool shown)
	{
		_view.SetShown(shown);
	}
}
