using System;
using UnityEngine.UI;

public class UIWindowOpenKeyActionBlocker : IKeyActionHandlerBlocker
{
	private readonly UIWindow _window;

	public bool IsBlock { get; private set; }

	public event Action BlockStateChanged;

	public UIWindowOpenKeyActionBlocker(UIWindow window)
	{
		_window = window;
		UIWindow window2 = _window;
		window2.OnShow = (Action)Delegate.Combine(window2.OnShow, new Action(OnShow));
		UIWindow window3 = _window;
		window3.OnHide = (Action)Delegate.Combine(window3.OnHide, new Action(OnHide));
		IsBlock = !_window.IsOpen;
	}

	private void OnShow()
	{
		IsBlock = false;
		this.BlockStateChanged?.Invoke();
	}

	private void OnHide()
	{
		IsBlock = true;
		this.BlockStateChanged?.Invoke();
	}

	public void Clear()
	{
		this.BlockStateChanged = null;
		UIWindow window = _window;
		window.OnShow = (Action)Delegate.Remove(window.OnShow, new Action(OnShow));
		UIWindow window2 = _window;
		window2.OnHide = (Action)Delegate.Remove(window2.OnHide, new Action(OnHide));
	}
}
