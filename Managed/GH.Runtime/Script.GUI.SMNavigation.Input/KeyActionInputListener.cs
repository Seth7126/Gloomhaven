using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Script.GUI.SMNavigation.Input;

public class KeyActionInputListener : InputListener
{
	[FormerlySerializedAs("_nextTab")]
	[SerializeField]
	private KeyAction _nextKey;

	[FormerlySerializedAs("_previousTab")]
	[SerializeField]
	private KeyAction _previousKey;

	[SerializeField]
	private bool _listenToPressedOrReleased = true;

	public bool IsRegistered { get; private set; }

	public override void Register()
	{
		RegisterAction(_nextKey, Next);
		RegisterAction(_previousKey, Previous);
		IsRegistered = true;
	}

	public override void UnRegister()
	{
		UnregisterAction(_nextKey, Next);
		UnregisterAction(_previousKey, Previous);
		IsRegistered = false;
	}

	protected void RegisterAction(KeyAction key, Action action)
	{
		if (key != KeyAction.None)
		{
			if (_listenToPressedOrReleased)
			{
				InputManager.RegisterToOnPressed(key, action);
			}
			else
			{
				InputManager.RegisterToOnReleased(key, action);
			}
		}
	}

	protected void UnregisterAction(KeyAction key, Action action)
	{
		if (key != KeyAction.None)
		{
			if (_listenToPressedOrReleased)
			{
				InputManager.UnregisterToOnPressed(key, action);
			}
			else
			{
				InputManager.UnregisterToOnReleased(key, action);
			}
		}
	}
}
