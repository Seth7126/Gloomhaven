namespace Code.State;

public interface IState
{
	void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null) where TPayload : class;

	void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null) where TPayload : class;
}
