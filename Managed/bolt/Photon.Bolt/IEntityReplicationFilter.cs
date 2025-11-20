namespace Photon.Bolt;

public interface IEntityReplicationFilter
{
	bool AllowReplicationTo(BoltConnection connection);
}
