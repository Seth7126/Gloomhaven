using System;
using Code.State;

namespace Script.GUI.SMNavigation.States;

public abstract class NavigationState<TStateTag> : IState where TStateTag : Enum
{
	public abstract TStateTag StateTag { get; }

	public abstract void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null) where TPayload : class;

	public abstract void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null) where TPayload : class;
}
