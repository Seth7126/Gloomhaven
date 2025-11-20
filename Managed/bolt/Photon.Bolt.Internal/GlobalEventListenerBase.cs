using System;
using Photon.Bolt.Collections;
using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt.Internal;

[Documentation(Alias = "Photon.Bolt.GlobalEventListener")]
public abstract class GlobalEventListenerBase : MonoBehaviour, IBoltListNode<GlobalEventListenerBase>
{
	internal static readonly BoltDoubleList<GlobalEventListenerBase> callbacks = new BoltDoubleList<GlobalEventListenerBase>();

	GlobalEventListenerBase IBoltListNode<GlobalEventListenerBase>.prev { get; set; }

	GlobalEventListenerBase IBoltListNode<GlobalEventListenerBase>.next { get; set; }

	object IBoltListNode<GlobalEventListenerBase>.list { get; set; }

	internal static void OnPacketSendInvoke(BoltConnection connection, Packet packet)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				if (callback is IExtraConnectionStats extraConnectionStats)
				{
					extraConnectionStats.OnPacketSend(connection, packet);
				}
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	protected void OnEnable()
	{
		BoltCore._globalEventDispatcher.Add(this);
		callbacks.AddLast(this);
	}

	protected void OnDisable()
	{
		BoltCore._globalEventDispatcher.Remove(this);
		callbacks.Remove(this);
	}

	public virtual bool PersistBetweenStartupAndShutdown()
	{
		return false;
	}

	public virtual void BoltShutdownBegin(AddCallback registerDoneCallback, UdpConnectionDisconnectReason disconnectReason)
	{
	}

	internal static void BoltShutdownBeginInvoke(AddCallback registerDoneCallback, UdpConnectionDisconnectReason disconnectReason)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.BoltShutdownBegin(registerDoneCallback, disconnectReason);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void BoltStartBegin()
	{
	}

	internal static void BoltStartBeginInvoke()
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.BoltStartBegin();
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void BoltStartDone()
	{
	}

	internal static void BoltStartDoneInvoke()
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.BoltStartDone();
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void BoltStartFailed(UdpConnectionDisconnectReason disconnectReason)
	{
	}

	internal static void BoltStartFailedInvoke(UdpConnectionDisconnectReason disconnectReason)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.BoltStartFailed(disconnectReason);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void StreamDataStarted(BoltConnection connection, UdpChannelName channel, ulong streamID)
	{
	}

	internal static void StreamDataStartedInvoke(BoltConnection connection, UdpChannelName channel, ulong streamID)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.StreamDataStarted(connection, channel, streamID);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void StreamDataAborted(BoltConnection connection, UdpChannelName channel, ulong streamID)
	{
	}

	internal static void StreamDataAbortedInvoke(BoltConnection connection, UdpChannelName channel, ulong streamID)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.StreamDataAborted(connection, channel, streamID);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void StreamDataProgress(BoltConnection connection, UdpChannelName channel, ulong streamID, float progress)
	{
	}

	internal static void StreamDataProgressInvoke(BoltConnection connection, UdpChannelName channel, ulong streamID, float progress)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.StreamDataProgress(connection, channel, streamID, progress);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void StreamDataReceived(BoltConnection connection, UdpStreamData data)
	{
	}

	internal static void StreamDataReceivedInvoke(BoltConnection connection, UdpStreamData data)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.StreamDataReceived(connection, data);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void SceneLoadLocalBegin(string scene, IProtocolToken token)
	{
	}

	internal static void SceneLoadLocalBeginInvoke(string scene, IProtocolToken token)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.SceneLoadLocalBegin(scene, token);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void SceneLoadLocalDone(string scene, IProtocolToken token)
	{
	}

	internal static void SceneLoadLocalDoneInvoke(string scene, IProtocolToken token)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.SceneLoadLocalDone(scene, token);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void SceneLoadRemoteDone(BoltConnection connection, IProtocolToken token)
	{
	}

	internal static void SceneLoadRemoteDoneInvoke(BoltConnection connection, IProtocolToken token)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.SceneLoadRemoteDone(connection, token);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void Connected(BoltConnection connection)
	{
	}

	internal static void ConnectedInvoke(BoltConnection connection)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.Connected(connection);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void ConnectFailed(UdpEndPoint endpoint, IProtocolToken token)
	{
	}

	internal static void ConnectFailedInvoke(UdpEndPoint endpoint, IProtocolToken token)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.ConnectFailed(endpoint, token);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void ConnectRequest(UdpEndPoint endpoint, IProtocolToken token)
	{
	}

	internal static void ConnectRequestInvoke(UdpEndPoint endpoint, IProtocolToken token)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.ConnectRequest(endpoint, token);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void ConnectRefused(UdpEndPoint endpoint, IProtocolToken token)
	{
	}

	internal static void ConnectRefusedInvoke(UdpEndPoint endpoint, IProtocolToken token)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.ConnectRefused(endpoint, token);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void ConnectAttempt(UdpEndPoint endpoint, IProtocolToken token)
	{
	}

	internal static void ConnectAttemptInvoke(UdpEndPoint endpoint, IProtocolToken token)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.ConnectAttempt(endpoint, token);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void Disconnected(BoltConnection connection)
	{
	}

	internal static void DisconnectedInvoke(BoltConnection connection)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.Disconnected(connection);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void ControlOfEntityLost(BoltEntity entity)
	{
	}

	internal static void ControlOfEntityLostInvoke(BoltEntity entity)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.ControlOfEntityLost(entity);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void ControlOfEntityGained(BoltEntity entity)
	{
	}

	internal static void ControlOfEntityGainedInvoke(BoltEntity entity)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.ControlOfEntityGained(entity);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void EntityAttached(BoltEntity entity)
	{
	}

	internal static void EntityAttachedInvoke(BoltEntity entity)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.EntityAttached(entity);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void EntityDetached(BoltEntity entity)
	{
	}

	internal static void EntityDetachedInvoke(BoltEntity entity)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.EntityDetached(entity);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void EntityReceived(BoltEntity entity)
	{
	}

	internal static void EntityReceivedInvoke(BoltEntity entity)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.EntityReceived(entity);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void EntityFrozen(BoltEntity entity)
	{
	}

	internal static void EntityFrozenInvoke(BoltEntity entity)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.EntityFrozen(entity);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void EntityThawed(BoltEntity entity)
	{
	}

	internal static void EntityThawedInvoke(BoltEntity entity)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.EntityThawed(entity);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void SessionListUpdated(Map<Guid, UdpSession> sessionList)
	{
	}

	internal static void SessionListUpdatedInvoke(Map<Guid, UdpSession> sessionList)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.SessionListUpdated(sessionList);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void SessionConnected(UdpSession session, IProtocolToken token)
	{
	}

	internal static void SessionConnectedInvoke(UdpSession session, IProtocolToken token)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.SessionConnected(session, token);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void SessionConnectFailed(UdpSession session, IProtocolToken token, UdpSessionError errorReason)
	{
	}

	internal static void SessionConnectFailedInvoke(UdpSession session, IProtocolToken token, UdpSessionError errorReason)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.SessionConnectFailed(session, token, errorReason);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void SessionCreatedOrUpdated(UdpSession session)
	{
	}

	internal static void SessionCreatedOrUpdatedInvoke(UdpSession session)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.SessionCreatedOrUpdated(session);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}

	public virtual void SessionCreationFailed(UdpSession session, UdpSessionError errorReason)
	{
	}

	internal static void SessionCreationFailedInvoke(UdpSession session, UdpSessionError errorReason)
	{
		foreach (GlobalEventListenerBase callback in callbacks)
		{
			try
			{
				callback.SessionCreationFailed(session, errorReason);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}
}
