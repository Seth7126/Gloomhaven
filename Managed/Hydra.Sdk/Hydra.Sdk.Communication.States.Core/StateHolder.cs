using System.Collections.Generic;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States.Core;

public class StateHolder<T> where T : IHydraSdkStateWrapper
{
	private List<StateObserver<T>> _observers = new List<StateObserver<T>>();

	public T State { get; private set; }

	public StateHolder(T state)
	{
		State = state;
	}

	public void Attach(StateObserver<T> observer)
	{
		_observers.Add(observer);
	}

	public void Detach(StateObserver<T> observer)
	{
		_observers.Remove(observer);
	}

	internal void Update(T state)
	{
		State = state;
		_observers.ForEach(delegate(StateObserver<T> o)
		{
			o.UpdateFromSource();
		});
	}
}
