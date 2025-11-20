using System.Collections;
using Photon.Bolt.Internal;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt;

public class BoltDebugStart : GlobalEventListenerBase
{
	private void Start()
	{
		BoltRuntimeSettings instance = BoltRuntimeSettings.instance;
		BoltConfig configCopy = instance.GetConfigCopy();
		configCopy.connectionTimeout = 60000000;
		configCopy.connectionRequestTimeout = 500;
		configCopy.connectionRequestAttempts = 1000;
		if (!string.IsNullOrEmpty(instance.debugStartMapName))
		{
			if (BoltDebugStartSettings.DebugStartIsServer)
			{
				BoltLauncher.StartServer(new UdpEndPoint(UdpIPv4Address.Localhost, (ushort)instance.debugStartPort), configCopy);
			}
			else if (BoltDebugStartSettings.DebugStartIsClient)
			{
				BoltLauncher.StartClient(new UdpEndPoint(UdpIPv4Address.Localhost, 0), configCopy);
			}
			else if (BoltDebugStartSettings.DebugStartIsSinglePlayer)
			{
				BoltLauncher.StartSinglePlayer(configCopy);
			}
			BoltDebugStartSettings.PositionWindow();
		}
	}

	public override void BoltStartFailed(UdpConnectionDisconnectReason disconnectReason)
	{
	}

	public override void BoltStartDone()
	{
		if (BoltNetwork.IsServer || BoltNetwork.IsSinglePlayer)
		{
			BoltNetwork.LoadScene(BoltRuntimeSettings.instance.debugStartMapName);
		}
		else if (BoltNetwork.IsClient)
		{
			StartCoroutine(DelayClientConnect());
		}
	}

	private IEnumerator DelayClientConnect()
	{
		for (int i = 0; i < 5; i++)
		{
			yield return new WaitForSeconds(1f);
		}
		BoltNetwork.Connect((ushort)BoltRuntimeSettings.instance.debugStartPort);
	}
}
