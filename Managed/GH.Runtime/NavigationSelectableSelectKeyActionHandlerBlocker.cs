using System;
using SM.Gamepad;

public class NavigationSelectableSelectKeyActionHandlerBlocker : IKeyActionHandlerBlocker
{
	private bool _isSelected;

	private readonly UINavigationSelectable _uiNavigationSelectable;

	public bool IsBlock => !_isSelected;

	public event Action BlockStateChanged;

	public NavigationSelectableSelectKeyActionHandlerBlocker(UINavigationSelectable uiNavigationSelectable)
	{
		_uiNavigationSelectable = uiNavigationSelectable;
		_uiNavigationSelectable.OnNavigationSelectedEvent += OnSelected;
		_uiNavigationSelectable.OnNavigationDeselectedEvent += OnDeselected;
	}

	public void Clear()
	{
		_uiNavigationSelectable.OnNavigationSelectedEvent -= OnSelected;
		_uiNavigationSelectable.OnNavigationDeselectedEvent -= OnDeselected;
	}

	private void OnDeselected(IUiNavigationSelectable selectable)
	{
		_isSelected = false;
		this.BlockStateChanged?.Invoke();
	}

	private void OnSelected(IUiNavigationSelectable selectable)
	{
		_isSelected = true;
		this.BlockStateChanged?.Invoke();
	}
}
