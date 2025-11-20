using System;
using Photon.Bolt.Internal;
using UdpKit;
using UdpKit.Platform;

namespace Photon.Bolt;

public static class BoltLauncher
{
	private static UdpPlatform TargetPlatform { get; set; }

	public static void StartSinglePlayer(BoltConfig config = null)
	{
		if (config == null)
		{
			config = BoltRuntimeSettings.instance.GetConfigCopy();
		}
		Initialize(BoltNetworkModes.None, UdpEndPoint.Any, config);
	}

	public static void StartServer(int port = -1)
	{
		if (port >= 0 && port <= 65535)
		{
			StartServer(new UdpEndPoint(UdpIPv4Address.Any, (ushort)port));
			return;
		}
		if (port == -1)
		{
			StartServer(UdpEndPoint.Any);
			return;
		}
		throw new ArgumentOutOfRangeException($"'port' must be >= 0 and <= {ushort.MaxValue}");
	}

	public static void StartServer(BoltConfig config, string scene = null)
	{
		StartServer(UdpEndPoint.Any, config, scene);
	}

	public static void StartServer(UdpEndPoint endpoint, string scene = null)
	{
		StartServer(endpoint, BoltRuntimeSettings.instance.GetConfigCopy(), scene);
	}

	public static void StartServer(UdpEndPoint endpoint, BoltConfig config, string scene = null)
	{
		Initialize(BoltNetworkModes.Server, endpoint, config, scene);
	}

	public static void StartClient(int port = -1)
	{
		if (port >= 0 && port <= 65535)
		{
			StartClient(new UdpEndPoint(UdpIPv4Address.Any, (ushort)port));
			return;
		}
		if (port == -1)
		{
			StartClient(UdpEndPoint.Any);
			return;
		}
		throw new ArgumentOutOfRangeException($"'port' must be >= 0 and <= {ushort.MaxValue}");
	}

	public static void StartClient(BoltConfig config)
	{
		StartClient(UdpEndPoint.Any, config);
	}

	public static void StartClient(UdpEndPoint endpoint, BoltConfig config = null)
	{
		if (config == null)
		{
			config = BoltRuntimeSettings.instance.GetConfigCopy();
		}
		Initialize(BoltNetworkModes.Client, endpoint, config);
	}

	private static void Initialize(BoltNetworkModes modes, UdpEndPoint endpoint, BoltConfig config, string scene = null)
	{
		BoltNetworkInternal.Initialize(modes, endpoint, config, TargetPlatform, scene);
	}

	public static void Shutdown()
	{
		BoltNetwork.Shutdown();
	}

	public static void SetUdpPlatform(UdpPlatform platform)
	{
		TargetPlatform = platform;
	}
}
