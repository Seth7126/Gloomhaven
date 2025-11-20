#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;
using UdpKit;
using UdpKit.Platform;
using UdpKit.Platform.Photon;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VoiceChat;

public class BoltVoiceMenu : GlobalEventListener
{
	private bool _showGui = true;

	private Coroutine _timerRoutine;

	private bool inputIsBlocked;

	private void Awake()
	{
		BoltLauncher.SetUdpPlatform(new PhotonPlatform());
	}

	private void OnGUI()
	{
		if (_showGui && !inputIsBlocked)
		{
			GUILayout.BeginArea(new Rect(50f, 50f, Screen.width - 100, Screen.height - 100));
			if (GUILayout.Button("Start Server", GUILayout.ExpandWidth(expand: true), GUILayout.ExpandHeight(expand: true)) || Gamepad.current.bButton.isPressed)
			{
				inputIsBlocked = true;
				BoltLauncher.StartServer();
			}
			if (GUILayout.Button("Start Client", GUILayout.ExpandWidth(expand: true), GUILayout.ExpandHeight(expand: true)) || Gamepad.current.xButton.isPressed)
			{
				inputIsBlocked = true;
				BoltLauncher.StartClient();
			}
			GUILayout.EndArea();
		}
	}

	public override void BoltStartBegin()
	{
		_showGui = false;
		BoltNetwork.RegisterTokenClass<PhotonRoomProperties>();
	}

	public override void BoltStartFailed(UdpConnectionDisconnectReason disconnectReason)
	{
		_showGui = true;
		Debug.LogError("BoltStartFailed");
	}

	public override void BoltStartDone()
	{
		if (BoltNetwork.IsServer)
		{
			BoltMatchmaking.CreateSession(Guid.NewGuid().ToString(), null, "Voice_Meeting");
		}
		if (BoltNetwork.IsClient)
		{
			_timerRoutine = StartCoroutine(ShutdownAndStartServer());
		}
	}

	public override void BoltShutdownBegin(AddCallback registerDoneCallback, UdpConnectionDisconnectReason disconnectReason)
	{
		registerDoneCallback(delegate
		{
			BoltLauncher.StartServer();
		});
	}

	public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
	{
		if (_timerRoutine != null)
		{
			StopCoroutine(_timerRoutine);
			_timerRoutine = null;
		}
		Debug.LogFormat("Session list updated: {0} total sessions", sessionList.Count);
		foreach (KeyValuePair<Guid, UdpSession> session in sessionList)
		{
			if (session.Value is PhotonSession { Source: UdpSessionSource.Photon } photonSession)
			{
				photonSession.Properties.TryGetValue("type", out var value);
				photonSession.Properties.TryGetValue("map", out value);
				BoltMatchmaking.JoinSession(photonSession);
			}
		}
	}

	private static IEnumerator ShutdownAndStartServer(int timeout = 10)
	{
		yield return new WaitForSeconds(timeout);
		BoltNetwork.ShutdownImmediate();
	}
}
