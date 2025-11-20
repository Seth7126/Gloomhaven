using Photon.Bolt;

namespace FFSNet;

public sealed class NetworkStatsClient : NetworkStats
{
	public override float Ping => base.Connection.PingNetwork;

	public override float BytesPerSecondIn => (float)base.Connection.BitsPerSecondIn / 8f;

	public override float BytesPerSecondOut => (float)base.Connection.BitsPerSecondOut / 8f;

	public NetworkStatsClient(BoltConnection connection)
		: base(connection)
	{
	}
}
