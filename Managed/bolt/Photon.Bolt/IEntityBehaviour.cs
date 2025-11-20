namespace Photon.Bolt;

public interface IEntityBehaviour
{
	bool invoke { get; }

	BoltEntity entity { get; set; }

	void Initialized();

	void Attached();

	void Detached();

	void SimulateOwner();

	void SimulateController();

	void ControlLost();

	void ControlGained();

	void MissingCommand(Command previous);

	void ExecuteCommand(Command command, bool resetState);

	bool LocalAndRemoteResultEqual(Command command);
}
public interface IEntityBehaviour<TState> : IEntityBehaviour where TState : IState
{
	TState state { get; }
}
