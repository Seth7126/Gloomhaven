using System;
using System.Collections.Generic;

namespace Code.State;

public class StatePayloadsContainer
{
	private readonly Dictionary<Type, object> _payloads = new Dictionary<Type, object>();

	private Type _latestType;

	public void Set<TPayload>(TPayload payload) where TPayload : class
	{
		Type typeFromHandle = typeof(TPayload);
		_payloads[typeFromHandle] = payload;
		_latestType = typeFromHandle;
	}
}
