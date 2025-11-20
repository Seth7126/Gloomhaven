using System;
using AsmodeeNet.Foundation;
using UnityEngine;

public class ControllerInputElement : MonoBehaviour
{
	protected bool isEnabled;

	protected virtual void OnEnable()
	{
		InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Remove(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnControllerTypeChanged));
		InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Combine(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnControllerTypeChanged));
		if (InputManager.GamePadInUse)
		{
			OnEnabledControllerControl();
		}
		else
		{
			OnDisabledControllerControl();
		}
	}

	protected virtual void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Remove(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnControllerTypeChanged));
			OnDisabledControllerControl();
		}
	}

	protected virtual void OnControllerTypeChanged(ControllerType type)
	{
		if (!InputManager.GamePadInUse)
		{
			OnDisabledControllerControl();
		}
		else
		{
			OnEnabledControllerControl();
		}
	}

	protected virtual void OnEnabledControllerControl()
	{
		isEnabled = true;
	}

	protected virtual void OnDisabledControllerControl()
	{
		isEnabled = false;
	}
}
