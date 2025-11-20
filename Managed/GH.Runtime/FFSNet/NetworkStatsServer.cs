using System.Linq;
using Photon.Bolt;

namespace FFSNet;

public sealed class NetworkStatsServer : NetworkStats
{
	public override float Ping => 0f;

	public override float BytesPerSecondIn => PlayerRegistry.AllPlayers.Where((NetworkPlayer player) => player.IsClient).Sum((NetworkPlayer player) => player.NetStats.BytesPerSecondIn);

	public override float BytesPerSecondOut => PlayerRegistry.AllPlayers.Where((NetworkPlayer player) => player.IsClient).Sum((NetworkPlayer player) => player.NetStats.BytesPerSecondOut);

	public NetworkStatsServer(BoltConnection connection)
		: base(connection)
	{
	}
}
