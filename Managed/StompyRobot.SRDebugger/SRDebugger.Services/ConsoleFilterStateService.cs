using System;
using UnityEngine;

namespace SRDebugger.Services;

public sealed class ConsoleFilterStateService
{
	private bool[] _states;

	public event ConsoleStateChangedEventHandler FilterStateChange;

	public ConsoleFilterStateService()
	{
		_states = new bool[Enum.GetValues(typeof(LogType)).Length];
		for (int i = 0; i < _states.Length; i++)
		{
			_states[i] = true;
		}
	}

	public void SetState(LogType type, bool newState)
	{
		type = GetType(type);
		if (_states[(int)type] != newState)
		{
			_states[(int)type] = newState;
			this.FilterStateChange?.Invoke(type, newState);
		}
	}

	public bool GetState(LogType type)
	{
		type = GetType(type);
		return _states[(int)type];
	}

	private static LogType GetType(LogType type)
	{
		if ((uint)type <= 1u || type == LogType.Exception)
		{
			return LogType.Error;
		}
		return type;
	}
}
