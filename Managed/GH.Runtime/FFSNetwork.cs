#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using FFSNet;
using MEC;
using Photon.Bolt;
using SM.Utils;
using UnityEngine.Events;

public static class FFSNetwork
{
	public enum SessionReceivedResult
	{
		None,
		MultipleCall,
		Success
	}

	private static Action<SessionReceivedResult, string> _sessionReceivedCompleted = null;

	public static NetworkManager Manager { get; private set; }

	public static NetworkBehaviour Behaviour { get; private set; }

	public static bool IsOnline
	{
		get
		{
			if (BoltNetwork.IsRunning)
			{
				return !IsShuttingDown;
			}
			return false;
		}
	}

	public static bool IsHost => BoltNetwork.IsServer;

	public static bool IsClient => BoltNetwork.IsClient;

	public static bool IsStartingUp { get; set; }

	public static bool IsShuttingDown { get; set; }

	public static bool HasDesynchronized { get; set; }

	public static bool IsKickedState { get; set; }

	public static bool IsUGCEnabled { get; set; } = true;

	public static DesyncDetectedEvent OnDesyncDetected { get; set; }

	public static void Initialize(NetworkManager manager, NetworkBehaviour behaviour)
	{
		Debug.Log("Initializing FFSNetwork");
		Manager = manager;
		Behaviour = behaviour;
	}

	public static void StartUp()
	{
		Debug.Log("Starting up FFSNetwork");
		IsStartingUp = true;
		HasDesynchronized = false;
	}

	public static void HandleDesync(Exception ex)
	{
		if (IsOnline && !HasDesynchronized)
		{
			if (IsClient)
			{
				Synchronizer.SendSideAction(GameActionType.ClientDesync, null, canBeUnreliable: false, sendToHostOnly: false, 0, PlayerRegistry.MyPlayer.PlayerID);
			}
			HasDesynchronized = true;
			FFSNet.Console.LogError("ERROR_MULTIPLAYER_00022", "Desynchronization occurred.\n" + ex.Message, ex.StackTrace);
			OnDesyncDetected?.Invoke(ex);
			Manager.OnDisconnected?.Invoke(DisconnectionErrorCode.Desynchronization);
			if (Manager.AutoShutdownUponDesynchronization)
			{
				Shutdown();
			}
		}
	}

	public static void Shutdown(IProtocolToken clientDisconnectionToken = null, UnityAction onShutdownCompleted = null)
	{
		if (IsOnline && !IsShuttingDown)
		{
			FFSNet.Console.Log("Shutting down FFSNetwork");
			if (IsHost)
			{
				if (HasDesynchronized)
				{
					clientDisconnectionToken = new DisconnectionErrorToken(DisconnectionErrorCode.Desynchronization);
				}
				if (clientDisconnectionToken != null)
				{
					foreach (BoltConnection connection in BoltNetwork.Connections)
					{
						connection.Disconnect(clientDisconnectionToken);
					}
				}
			}
			IsShuttingDown = true;
			BoltNetwork.Shutdown();
			PlatformLayer.Networking.LeaveSession(null);
		}
		if (onShutdownCompleted != null)
		{
			Timing.RunCoroutine(WaitUntilShutdownCompleted(onShutdownCompleted));
		}
		IsUGCEnabled = true;
	}

	private static IEnumerator<float> WaitUntilShutdownCompleted(UnityAction onShutdownCompleted)
	{
		while (IsOnline || IsShuttingDown)
		{
			yield return 0f;
		}
		onShutdownCompleted?.Invoke();
	}

	public static void StartPSPlayerSessionNegotiation(Action<SessionReceivedResult, string> callbackAction)
	{
		if (_sessionReceivedCompleted != null)
		{
			_sessionReceivedCompleted(SessionReceivedResult.MultipleCall, null);
		}
		_sessionReceivedCompleted = callbackAction;
		LogUtils.Log("StartPSPlayerSessionNegotiation");
		NetworkUtils.SendSessionCommunicationEvent(SessionMessageType.ClientSessionRequest, string.Empty, BoltNetwork.Server);
	}

	public static void OnSessionReceivedFromHost(string session)
	{
		LogUtils.Log("OnSessionReceivedFromHost " + session);
		Action<SessionReceivedResult, string> sessionReceivedCompleted = _sessionReceivedCompleted;
		_sessionReceivedCompleted = null;
		sessionReceivedCompleted?.Invoke(SessionReceivedResult.Success, session);
	}
}
