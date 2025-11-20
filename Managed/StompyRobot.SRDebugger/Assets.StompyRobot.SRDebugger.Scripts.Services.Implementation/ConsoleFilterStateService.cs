using System;
using SRDebugger.Services;
using SRF.Service;
using UnityEngine;

namespace Assets.StompyRobot.SRDebugger.Scripts.Services.Implementation;

[Service(typeof(IConsoleFilterState))]
public sealed class ConsoleFilterStateService : IConsoleFilterState
{
	private readonly bool[] _states;

	public event ConsoleStateChangedEventHandler FilterStateChange;

	public ConsoleFilterStateService()
	{
		_states = new bool[Enum.GetValues(typeof(LogType)).Length];
		for (int i = 0; i < _states.Length; i++)
		{
			_states[i] = true;
		}
	}

	public void SetConsoleFilterState(LogType type, bool newState)
	{
		type = GetType(type);
		if (_states[(int)type] != newState)
		{
			_states[(int)type] = newState;
			this.FilterStateChange?.Invoke(type, newState);
		}
	}

	public bool GetConsoleFilterState(LogType type)
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
