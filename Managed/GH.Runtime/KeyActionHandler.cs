using System;
using System.Collections.Generic;
using System.Linq;

public class KeyActionHandler
{
	public enum RegisterType
	{
		Press,
		Release,
		Click
	}

	private readonly List<IKeyActionHandlerBlocker> _blockers;

	private Action _onUnblock;

	private Action _onBlock;

	private bool _pressed;

	private readonly RegisterType _registerType;

	private readonly bool _isPersistent;

	public KeyAction KeyAction { get; }

	public Action Action { get; }

	public bool IsPersistent => _isPersistent;

	public bool HasBlockers => _blockers.Count > 0;

	public KeyActionHandler(KeyAction keyAction, Action action, Action onUnblock = null, Action onBlock = null, bool isPersistent = false, RegisterType registerType = RegisterType.Press)
	{
		_blockers = new List<IKeyActionHandlerBlocker>();
		KeyAction = keyAction;
		Action = action;
		_onUnblock = onUnblock;
		_onBlock = onBlock;
		_isPersistent = isPersistent;
		_registerType = registerType;
	}

	private void Register()
	{
		switch (_registerType)
		{
		case RegisterType.Press:
			InputManager.RegisterToOnPressed(KeyAction, Action);
			break;
		case RegisterType.Release:
			InputManager.RegisterToOnReleased(KeyAction, Action);
			break;
		case RegisterType.Click:
			InputManager.RegisterToOnPressed(KeyAction, ClickActionPress);
			InputManager.RegisterToOnReleased(KeyAction, ClickActionReleased);
			break;
		}
	}

	private void Unregister()
	{
		switch (_registerType)
		{
		case RegisterType.Press:
			InputManager.UnregisterToOnPressed(KeyAction, Action);
			break;
		case RegisterType.Release:
			InputManager.UnregisterToOnReleased(KeyAction, Action);
			break;
		case RegisterType.Click:
			_pressed = false;
			InputManager.UnregisterToOnPressed(KeyAction, ClickActionPress);
			InputManager.UnregisterToOnReleased(KeyAction, ClickActionReleased);
			break;
		}
	}

	private void ClickActionPress()
	{
		_pressed = true;
	}

	private void ClickActionReleased()
	{
		if (_pressed)
		{
			Action?.Invoke();
		}
		_pressed = false;
	}

	public void Clear()
	{
		Unregister();
		_onUnblock = null;
		_onBlock = null;
		foreach (IKeyActionHandlerBlocker blocker in _blockers)
		{
			blocker.Clear();
		}
		_blockers.Clear();
	}

	public KeyActionHandler AddBlocker(IKeyActionHandlerBlocker keyActionHandlerBlocker)
	{
		_blockers.Add(keyActionHandlerBlocker);
		keyActionHandlerBlocker.BlockStateChanged += CheckBlockers;
		return this;
	}

	public void RemoveBlocker(IKeyActionHandlerBlocker keyActionHandlerBlocker)
	{
		if (_blockers.Contains(keyActionHandlerBlocker))
		{
			keyActionHandlerBlocker.Clear();
			_blockers.Remove(keyActionHandlerBlocker);
			CheckBlockers();
		}
	}

	public void RemoveBlocker(Type blockerType)
	{
		IKeyActionHandlerBlocker keyActionHandlerBlocker = _blockers.Find((IKeyActionHandlerBlocker blocker) => blocker.GetType() == blockerType);
		if (keyActionHandlerBlocker != null)
		{
			keyActionHandlerBlocker.Clear();
			_blockers.Remove(keyActionHandlerBlocker);
			CheckBlockers();
		}
	}

	public void CheckBlockers()
	{
		if (!_blockers.Any((IKeyActionHandlerBlocker blocker) => blocker.IsBlock))
		{
			Register();
			_onUnblock?.Invoke();
		}
		else
		{
			Unregister();
			_onBlock?.Invoke();
		}
	}
}
