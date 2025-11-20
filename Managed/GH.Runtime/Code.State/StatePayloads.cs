using System;
using System.Collections.Generic;

namespace Code.State;

public class StatePayloads
{
	private Dictionary<Type, Dictionary<Type, object>> _payloads = new Dictionary<Type, Dictionary<Type, object>>();

	public void Set<TState, TPayload>(TPayload payload) where TState : IState where TPayload : class
	{
		Type typeFromHandle = typeof(TState);
		Type typeFromHandle2 = typeof(TPayload);
		if (!_payloads.ContainsKey(typeFromHandle))
		{
			_payloads[typeFromHandle] = new Dictionary<Type, object>();
		}
		_payloads[typeFromHandle][typeFromHandle2] = payload;
	}
}
