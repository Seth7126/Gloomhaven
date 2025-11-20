using System;
using System.Collections.Concurrent;
using Hydra.Api.Errors;
using Hydra.Sdk.Errors;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States.Core;

public class StateResolver
{
	private readonly ConcurrentDictionary<Type, object> _holders;

	public StateResolver()
	{
		_holders = new ConcurrentDictionary<Type, object>();
	}

	public void Register<T>(T defaultState) where T : IHydraSdkStateWrapper
	{
		Type typeFromHandle = typeof(T);
		if (_holders.TryGetValue(typeof(T), out var _))
		{
			throw new HydraSdkException(ErrorCode.SdkInternalError, "State with type '" + typeFromHandle.Name + "' is already registered");
		}
		_holders.TryAdd(typeFromHandle, new StateHolder<T>(defaultState));
	}

	private object Get<T>() where T : IHydraSdkStateWrapper
	{
		return _holders[typeof(T)];
	}

	public bool IsRegistered<T>() where T : IHydraSdkStateWrapper
	{
		if (!_holders.ContainsKey(typeof(T)))
		{
			return false;
		}
		return true;
	}

	public StateObserver<T> CreateLinkedObserver<T>() where T : IHydraSdkStateWrapper
	{
		_holders.TryAdd(typeof(T), new StateHolder<T>(default(T)));
		return (StateObserver<T>)Activator.CreateInstance(typeof(StateObserver<T>), Get<T>());
	}
}
