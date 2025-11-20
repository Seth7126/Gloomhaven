using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyActionHandlerController : Singleton<KeyActionHandlerController>
{
	private List<KeyActionHandler> _handlers;

	public void Setup()
	{
		UnityEngine.Object.DontDestroyOnLoad(this);
		_handlers = new List<KeyActionHandler>();
	}

	public void ResetSceneHandlers()
	{
		for (int i = 0; i < _handlers.Count; i++)
		{
			if (!_handlers[i].IsPersistent)
			{
				_handlers[i].Clear();
				_handlers.Remove(_handlers[i]);
			}
		}
	}

	private KeyActionHandler FindHandler(KeyAction keyAction, Action action)
	{
		foreach (KeyActionHandler handler in _handlers)
		{
			if (handler.KeyAction == keyAction && handler.Action == action)
			{
				return handler;
			}
		}
		return null;
	}

	public void AddHandler(KeyActionHandler handler)
	{
		if (FindHandler(handler.KeyAction, handler.Action) != null)
		{
			handler.Clear();
			return;
		}
		handler.CheckBlockers();
		_handlers.Add(handler);
	}

	public void AddBlockerForHandler(KeyAction handlerKeyAction, Action handlerAction, IKeyActionHandlerBlocker keyActionHandlerBlocker)
	{
		KeyActionHandler keyActionHandler = FindHandler(handlerKeyAction, handlerAction);
		if (keyActionHandler != null)
		{
			keyActionHandler.AddBlocker(keyActionHandlerBlocker);
			keyActionHandler.CheckBlockers();
		}
	}

	public void RemoveBlockerForHandler(KeyAction handlerKeyAction, Action handlerAction, IKeyActionHandlerBlocker keyActionHandlerBlocker)
	{
		FindHandler(handlerKeyAction, handlerAction)?.RemoveBlocker(keyActionHandlerBlocker);
	}

	public void RemoveBlockerForHandler(KeyAction handlerKeyAction, Action handlerAction, Type blockerType)
	{
		FindHandler(handlerKeyAction, handlerAction)?.RemoveBlocker(blockerType);
	}

	public void RemoveHandler(KeyActionHandler handler)
	{
		foreach (KeyActionHandler handler2 in _handlers)
		{
			if (handler2 == handler)
			{
				handler2.Clear();
				_handlers.Remove(handler2);
				break;
			}
		}
	}

	public void RemoveHandler(KeyAction keyAction, Action action)
	{
		foreach (KeyActionHandler handler in _handlers)
		{
			if (handler.KeyAction == keyAction && handler.Action == action)
			{
				handler.Clear();
				_handlers.Remove(handler);
				break;
			}
		}
	}

	protected override void OnDestroy()
	{
		foreach (KeyActionHandler handler in _handlers)
		{
			handler.Clear();
		}
		_handlers.Clear();
		base.OnDestroy();
	}
}
