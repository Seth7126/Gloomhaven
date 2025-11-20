using System;

namespace Photon.Bolt;

[Serializable]
public sealed class BoltConfig
{
	public int framesPerSecond = 60;

	public int packetSize = 1200;

	public int packetMaxEventSize = 512;

	public int packetStreamSize = 1024;

	public int maxEntityPriority = 65536;

	public int maxPropertyPriority = 2048;

	public ScopeMode scopeMode = ScopeMode.Automatic;

	public BoltConfigLogTargets logTargets = BoltConfigLogTargets.Unity | BoltConfigLogTargets.Console;

	public bool disableDejitterBuffer;

	public int clientSendRate;

	public int clientDejitterDelay;

	public int clientDejitterDelayMin;

	public int clientDejitterDelayMax;

	public int serverSendRate;

	public int serverDejitterDelay;

	public int serverDejitterDelayMin;

	public int serverDejitterDelayMax;

	public int serverConnectionLimit;

	public BoltConnectionAcceptMode serverConnectionAcceptMode = BoltConnectionAcceptMode.Auto;

	public int commandQueueSize;

	public int commandRedundancy;

	public float commandPingMultiplier;

	public bool useNetworkSimulation = true;

	public float simulatedLoss;

	public int simulatedPingMean;

	public int simulatedPingJitter;

	public BoltRandomFunction simulatedRandomFunction = BoltRandomFunction.PerlinNoise;

	public int connectionTimeout = 10000;

	public int connectionRequestTimeout = 500;

	public int connectionRequestAttempts = 20;

	public int connectionLocalRequestAttempts = 5;

	internal bool connectionLocalRequestAttemptsOverride = false;

	public bool disableAutoSceneLoading;

	public bool EnableIPv6;

	internal int ProtocolTokenMaxSize => packetSize - 256;

	public int commandDejitterDelay { get; set; }

	internal int commandDejitterDelayMin => clientSendRate;

	internal int commandDejitterDelayMax => clientSendRate * 3;

	public BoltConfig()
	{
		serverSendRate = 3;
		clientSendRate = 3;
		clientDejitterDelay = serverSendRate * 2;
		clientDejitterDelayMin = clientDejitterDelay - serverSendRate;
		clientDejitterDelayMax = clientDejitterDelay + serverSendRate;
		serverDejitterDelay = clientSendRate * 2;
		serverDejitterDelayMin = serverDejitterDelay - clientSendRate;
		serverDejitterDelayMax = serverDejitterDelay + clientSendRate;
		serverConnectionLimit = 64;
		commandRedundancy = 6;
		commandPingMultiplier = 1.25f;
		commandQueueSize = 60;
		EnableIPv6 = false;
		disableAutoSceneLoading = false;
	}

	internal BoltConfig Clone()
	{
		return (BoltConfig)MemberwiseClone();
	}
}
