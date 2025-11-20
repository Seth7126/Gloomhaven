using System.Runtime.InteropServices;

namespace Sony.NP;

public struct MemoryPoolStats
{
	public struct HttpStats
	{
		public ulong poolSize;

		public ulong maxInUseSize;

		public ulong currentInUseSize;

		public int reserved;
	}

	public struct InGameMessageMemoryPoolStatistics
	{
		public ulong poolSize;

		public ulong maxInUseSize;

		public ulong currentInUseSize;

		public int reserved;
	}

	public struct Matching2MemoryInfo
	{
		public ulong totalMemSize;

		public ulong curMemUsage;

		public ulong maxMemUsage;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public byte[] reserved;
	}

	public struct NetMemoryPoolStats
	{
		public ulong poolSize;

		public ulong maxInUseSize;

		public ulong currentInUseSize;

		public int reserved;
	}

	public struct SslMemoryPoolStats
	{
		public ulong poolSize;

		public ulong maxInUseSize;

		public ulong currentInUseSize;

		public int reserved;
	}

	public struct WebApiMemoryPoolStats
	{
		public ulong poolSize;

		public ulong maxInUseSize;

		public ulong currentInUseSize;

		public int reserved;
	}

	public HttpStats httpPoolStats;

	public SslMemoryPoolStats sslPoolStats;

	public WebApiMemoryPoolStats webApiPoolStats;

	public NetMemoryPoolStats netPoolStats;

	public NpToolkitMemoryPoolStats npToolkitPoolStats;

	public JsonMemoryPoolStats jsonPoolStats;

	public Matching2MemoryInfo matchingPoolStats;

	public Matching2MemoryInfo matchingSslPoolStats;

	public InGameMessageMemoryPoolStatistics inGameMsgPoolStats;
}
