using System;
using System.Reflection;
using Photon.Bolt.Internal;
using Photon.Bolt.Tokens;
using UdpKit;
using UdpKit.Platform.Photon;

namespace Photon.Bolt;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class PhotonCloudGlobalBehavior : PhotonPoller
{
	internal class PhotonBoltBehavior : GlobalEventListenerBase
	{
		public override void BoltShutdownBegin(AddCallback registerDoneCallback, UdpConnectionDisconnectReason disconnectReason)
		{
			registerDoneCallback(delegate
			{
				PhotonPoller.ConnectFailed();
			});
		}

		public override void ConnectAttempt(UdpEndPoint endpoint, IProtocolToken token)
		{
			PhotonPoller.ConnectAttempt(endpoint);
		}

		public override void ConnectFailed(UdpEndPoint endpoint, IProtocolToken token)
		{
			PhotonPoller.ConnectFailed();
		}

		public override void ConnectRefused(UdpEndPoint endpoint, IProtocolToken token)
		{
			PhotonPoller.ConnectFailed(refused: true);
		}
	}

	public void Awake()
	{
		PhotonPoller.RegisterInstance<PhotonCloudGlobalBehavior>();
	}

	protected internal override void BoltConnectInternal(UdpEndPoint endPoint, object token)
	{
		byte[] bytes = (byte[])token;
		BoltCore.Connect(endPoint, bytes.ToToken());
	}

	protected internal override void BoltCancelConnectInternal(UdpEndPoint endPoint)
	{
		BoltCore.CancelConnect(endPoint, internalOnly: true);
	}

	protected internal override void BoltDisconnectInternal(UdpConnectionDisconnectReason disconnectedCause)
	{
		if (BoltNetwork.IsClient)
		{
			BoltCore.Shutdown(disconnectedCause);
		}
		else
		{
			if (!BoltNetwork.IsServer)
			{
				return;
			}
			BoltDisconnectToken token = new BoltDisconnectToken("Bolt Server disconnected from Photon Server");
			foreach (BoltConnection connection in BoltNetwork.Connections)
			{
				connection.Disconnect(token, disconnectedCause);
			}
			BoltCore.Shutdown(disconnectedCause);
		}
	}

	protected internal override UdpEndPoint BoltEndPointInternal()
	{
		return BoltNetwork.UdpSocket.LanEndPoint;
	}

	protected internal override bool BoltIsClientInternal()
	{
		return BoltNetwork.IsClient;
	}

	protected internal override void BoltUpdateSessionListInternal(Map<Guid, UdpSession> sessions)
	{
		BoltNetwork.UpdateSessionList(sessions);
	}
}
