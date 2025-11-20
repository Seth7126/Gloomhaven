using System;
using Photon.Bolt.Channel;
using Photon.Bolt.Internal;
using UdpKit;

namespace Photon.Bolt.SceneManagement;

internal class SceneLoadChannel : BoltChannel
{
	private readonly bool autoSceneLoadEnabled;

	private bool forceSync;

	public SceneLoadChannel()
	{
		autoSceneLoadEnabled = !BoltCore._config.disableAutoSceneLoading;
		forceSync = false;
	}

	internal void ForceSceneSync()
	{
		forceSync = true;
	}

	public override void Pack(Packet packet)
	{
		UdpPacket udpPacket = packet.UdpPacket;
		udpPacket.WriteBool(BoltCore._canReceiveEntities);
		SceneLoadState localSceneLoading = BoltCore._localSceneLoading;
		SceneLoadState remoteSceneLoading = base.connection._remoteSceneLoading;
		udpPacket.WriteInt(localSceneLoading.State, 2);
		udpPacket.WriteInt(localSceneLoading.Scene.Index, 8);
		udpPacket.WriteInt(localSceneLoading.Scene.Sequence, 8);
		if (BoltNetwork.IsServer && udpPacket.WriteBool(localSceneLoading.Scene != remoteSceneLoading.Scene))
		{
			udpPacket.WriteToken(localSceneLoading.Token);
		}
	}

	public override void Read(Packet packet)
	{
		UdpPacket udpPacket = packet.UdpPacket;
		base.connection._canReceiveEntities = udpPacket.ReadBool();
		SceneLoadState sceneLoadState = new SceneLoadState
		{
			State = udpPacket.ReadInt(2),
			Scene = new Scene(udpPacket.ReadInt(8), udpPacket.ReadInt(8))
		};
		if (BoltNetwork.IsClient && udpPacket.ReadBool())
		{
			sceneLoadState.Token = udpPacket.ReadToken();
		}
		if (base.connection._remoteSceneLoading.Scene == sceneLoadState.Scene)
		{
			sceneLoadState.State = Math.Max(base.connection._remoteSceneLoading.State, sceneLoadState.State);
		}
		base.connection._remoteSceneLoading = sceneLoadState;
		if (BoltCore.isClient && (autoSceneLoadEnabled || forceSync))
		{
			forceSync = false;
			if (sceneLoadState.Scene != BoltCore._localSceneLoading.Scene)
			{
				sceneLoadState.State = 1;
				BoltCore.LoadSceneInternal(sceneLoadState);
			}
		}
	}
}
