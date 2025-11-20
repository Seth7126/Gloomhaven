using System.Collections.Generic;
using InControl;
using UnityEngine;

public abstract class PlayerActionControlsProvider<T> where T : struct
{
	private readonly PlayerActionControls _playerActionControls;

	private Dictionary<T, float> _handledControls = new Dictionary<T, float>();

	protected PlayerActionControlsProvider(PlayerActionControls playerActionControls)
	{
		_playerActionControls = playerActionControls;
	}

	public bool TryGetControl(PlayerAction action, out T control)
	{
		return TryGetControlFromBindings(_playerActionControls.GetPlayerActionBindings(action), out control);
	}

	protected abstract bool TryGetControlFromBindings(PlayerActionBindingSources bindings, out T control);

	public void MarkControlAsHandled(T control, float handleDuration = 0f)
	{
		_handledControls[control] = GetHandleEndTime(handleDuration);
	}

	public bool ControlIsHandled(PlayerActionBindingSources bindings)
	{
		if (TryGetControlFromBindings(bindings, out var control) && _handledControls.TryGetValue(control, out var value))
		{
			if (!IsHandleEnded(value))
			{
				return true;
			}
			_handledControls.Remove(control);
			return false;
		}
		return false;
	}

	private bool IsHandleEnded(float handleEndTime)
	{
		return GetTime() > handleEndTime;
	}

	private float GetHandleEndTime(float handleDuration)
	{
		return GetTime() + handleDuration;
	}

	private float GetTime()
	{
		return Time.time;
	}
}
