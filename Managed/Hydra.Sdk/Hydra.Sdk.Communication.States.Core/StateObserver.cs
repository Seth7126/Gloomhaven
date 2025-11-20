using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States.Core;

public class StateObserver<T> where T : IHydraSdkStateWrapper
{
	public delegate void StateUpdate(T oldState, T newState);

	private StateHolder<T> _holder;

	public StateUpdate OnStateUpdate;

	public T State { get; private set; }

	public StateObserver(StateHolder<T> holder)
	{
		_holder = holder;
		State = _holder.State;
		_holder.Attach(this);
	}

	public void UpdateFromSource()
	{
		OnStateUpdate?.Invoke(State, _holder.State);
		State = _holder.State;
	}

	public void Update(T state)
	{
		_holder.Update(state);
	}
}
