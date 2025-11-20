using System;

namespace UnityEngine.UIElements;

public class KeyboardNavigationManipulator : Manipulator
{
	private readonly Action<KeyboardNavigationOperation, EventBase> m_Action;

	public KeyboardNavigationManipulator(Action<KeyboardNavigationOperation, EventBase> action)
	{
		m_Action = action;
	}

	protected override void RegisterCallbacksOnTarget()
	{
		base.target.RegisterCallback<NavigationMoveEvent>(OnNavigationMove);
		base.target.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
		base.target.RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);
		base.target.RegisterCallback<KeyDownEvent>(OnKeyDown);
	}

	protected override void UnregisterCallbacksFromTarget()
	{
		base.target.UnregisterCallback<NavigationMoveEvent>(OnNavigationMove);
		base.target.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
		base.target.UnregisterCallback<NavigationCancelEvent>(OnNavigationCancel);
		base.target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
	}

	internal void OnKeyDown(KeyDownEvent evt)
	{
		IPanel panel = base.target.panel;
		if (panel != null && panel.contextType == ContextType.Editor)
		{
			OnEditorKeyDown(evt);
		}
		else
		{
			OnRuntimeKeyDown(evt);
		}
	}

	private void OnRuntimeKeyDown(KeyDownEvent evt)
	{
		Invoke(GetOperation(), evt);
		KeyboardNavigationOperation GetOperation()
		{
			switch (evt.keyCode)
			{
			case KeyCode.A:
				if (evt.actionKey)
				{
					return KeyboardNavigationOperation.SelectAll;
				}
				break;
			case KeyCode.Home:
				return KeyboardNavigationOperation.Begin;
			case KeyCode.End:
				return KeyboardNavigationOperation.End;
			case KeyCode.PageUp:
				return KeyboardNavigationOperation.PageUp;
			case KeyCode.PageDown:
				return KeyboardNavigationOperation.PageDown;
			}
			return KeyboardNavigationOperation.None;
		}
	}

	private void OnEditorKeyDown(KeyDownEvent evt)
	{
		Invoke(GetOperation(), evt);
		KeyboardNavigationOperation GetOperation()
		{
			switch (evt.keyCode)
			{
			case KeyCode.A:
				if (evt.actionKey)
				{
					return KeyboardNavigationOperation.SelectAll;
				}
				break;
			case KeyCode.Escape:
				return KeyboardNavigationOperation.Cancel;
			case KeyCode.Return:
			case KeyCode.KeypadEnter:
				return KeyboardNavigationOperation.Submit;
			case KeyCode.UpArrow:
				return KeyboardNavigationOperation.Previous;
			case KeyCode.DownArrow:
				return KeyboardNavigationOperation.Next;
			case KeyCode.Home:
				return KeyboardNavigationOperation.Begin;
			case KeyCode.End:
				return KeyboardNavigationOperation.End;
			case KeyCode.PageUp:
				return KeyboardNavigationOperation.PageUp;
			case KeyCode.PageDown:
				return KeyboardNavigationOperation.PageDown;
			}
			return KeyboardNavigationOperation.None;
		}
	}

	private void OnNavigationCancel(NavigationCancelEvent evt)
	{
		Invoke(KeyboardNavigationOperation.Cancel, evt);
	}

	private void OnNavigationSubmit(NavigationSubmitEvent evt)
	{
		Invoke(KeyboardNavigationOperation.Submit, evt);
	}

	private void OnNavigationMove(NavigationMoveEvent evt)
	{
		switch (evt.direction)
		{
		case NavigationMoveEvent.Direction.Up:
			Invoke(KeyboardNavigationOperation.Previous, evt);
			break;
		case NavigationMoveEvent.Direction.Down:
			Invoke(KeyboardNavigationOperation.Next, evt);
			break;
		}
	}

	private void Invoke(KeyboardNavigationOperation operation, EventBase evt)
	{
		if (operation != KeyboardNavigationOperation.None)
		{
			m_Action?.Invoke(operation, evt);
		}
	}
}
