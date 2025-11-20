using System;
using System.Collections.Generic;
using SRF.Service;

namespace SRDebugger.Internal;

[Service(typeof(InternalOptionsRegistry))]
public sealed class InternalOptionsRegistry
{
	private List<object> _registeredContainers = new List<object>();

	private Action<object> _handler;

	public void AddOptionContainer(object obj)
	{
		if (_handler != null)
		{
			_handler(obj);
		}
		else
		{
			_registeredContainers.Add(obj);
		}
	}

	public void SetHandler(Action<object> action)
	{
		_handler = action;
		foreach (object registeredContainer in _registeredContainers)
		{
			_handler(registeredContainer);
		}
		_registeredContainers = null;
	}
}
