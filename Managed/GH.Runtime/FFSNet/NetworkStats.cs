using Photon.Bolt;

namespace FFSNet;

public abstract class NetworkStats
{
	public BoltConnection Connection { get; protected set; }

	public abstract float Ping { get; }

	public int PingAsMilliseconds => (int)(Ping * 1000f);

	public float Latency => Ping / 2f;

	public abstract float BytesPerSecondIn { get; }

	public abstract float BytesPerSecondOut { get; }

	public float KBytesPerSecondIn => BytesPerSecondIn / 1024f;

	public float KBytesPerSecondOut => BytesPerSecondOut / 1024f;

	public NetworkStats(BoltConnection connection)
	{
		Connection = connection;
	}

	public override string ToString()
	{
		return $"Ping: {PingAsMilliseconds} ms\nDownload: {KBytesPerSecondIn:0.00} KB/sec\nUpload: {KBytesPerSecondOut:0.00} KB/sec";
	}
}
