#define DEBUG
using System;
using Photon.Bolt.Exceptions;
using Photon.Bolt.Utils;
using UdpKit;
using UdpKit.Platform;

namespace Photon.Bolt.Internal;

internal class ControlCommandStart : ControlCommand
{
	public BoltConfig Config;

	public BoltNetworkModes Mode;

	public UdpPlatform Platform;

	public UdpEndPoint EndPoint;

	private UdpPlatform DefaultPlatform => new PhotonPlatform();

	public override void Run()
	{
		if (BoltNetwork.IsRunning)
		{
			throw new BoltException("Bolt is already running, you must call BoltLauncher.Shutdown() before starting a new instance of Bolt.");
		}
		try
		{
			if (Mode == BoltNetworkModes.None)
			{
				Mode = BoltNetworkModes.Server;
				Platform = new NullPlatform();
			}
			if (!EndPoint.IPv6 && EndPoint.Address.IsLocalHost)
			{
				BoltLog.Info("Switching platform to DotNetPlatform when using LocalHost endpoint.");
				Platform = Platform ?? new DotNetPlatform();
			}
			if (Platform == null)
			{
				Platform = DefaultPlatform;
				BoltLog.Info("Platform not set, using default platform: {0}", Platform);
			}
		}
		catch (Exception)
		{
			throw;
		}
		BoltLog.Info("Starting Photon Bolt using platform: {0}", Platform);
		BoltCore.BeginStart(this);
	}

	public override void Done()
	{
	}
}
