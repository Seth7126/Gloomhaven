namespace Photon.Bolt;

public interface IPriorityCalculator
{
	bool Always { get; }

	float CalculateStatePriority(BoltConnection connection, int skipped);

	float CalculateEventPriority(BoltConnection connection, Event evnt);
}
