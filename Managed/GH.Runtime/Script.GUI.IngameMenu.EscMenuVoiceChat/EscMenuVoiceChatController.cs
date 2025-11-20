using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using VoiceChat;

namespace Script.GUI.IngameMenu.EscMenuVoiceChat;

public class EscMenuVoiceChatController : MonoBehaviour
{
	[SerializeField]
	[UsedImplicitly]
	private List<EscMenuVoiceChatRow> _rows;

	private BoltVoiceChatService _service;

	private bool _connected;

	private bool _isSubcribed;

	private void Start()
	{
		OnShow();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		OnHideInner();
		Unbind();
	}

	public void OnShow()
	{
		_service = Singleton<BoltVoiceChatService>.Instance;
		if (!_isSubcribed)
		{
			_service.EventJoinRoom += ServiceOnEventJoinRoom;
			_service.EventLeftRoom += ServiceOnEventLeftRoom;
			_isSubcribed = true;
		}
		DisplayStateStatus(_service.IsVoiceChatConnected);
	}

	public void OnHide()
	{
		if (!(_service != null) || !_service.IsVoiceChatConnected)
		{
			OnHideInner();
		}
	}

	public void ChangePosition(Vector2 pos)
	{
		RectTransform rectTransform = base.transform as RectTransform;
		if (!(rectTransform == null))
		{
			rectTransform.anchoredPosition = pos;
		}
	}

	private void OnHideInner()
	{
		if (_service != null)
		{
			_service.EventJoinRoom -= ServiceOnEventJoinRoom;
			_service.EventLeftRoom -= ServiceOnEventLeftRoom;
			_isSubcribed = false;
			if (_connected)
			{
				DisplayStateStatus(_service.IsVoiceChatConnected);
			}
		}
	}

	private void ServiceOnEventJoinRoom()
	{
		DisplayStateStatus(connected: true);
	}

	private void ServiceOnEventLeftRoom()
	{
		DisplayStateStatus(connected: false);
	}

	private void Bind()
	{
		_connected = true;
		_service.EventNewUserConnected += VoiceBridgeOnEventAddIncomingVoiceUser;
		_service.EventUserDisconnected += VoiceBridgeOnEventRemoveIncomingVoiceUser;
		_rows[0].Show(_service.SelfUserVoice);
		for (int i = 1; i < _rows.Count; i++)
		{
			EscMenuVoiceChatRow escMenuVoiceChatRow = _rows[i];
			if (i <= _service.PlayerVoices.Count)
			{
				escMenuVoiceChatRow.Show(_service.PlayerVoices[i - 1]);
			}
			else
			{
				escMenuVoiceChatRow.Hide();
			}
		}
	}

	private void Unbind()
	{
		_connected = false;
		_service.EventNewUserConnected -= VoiceBridgeOnEventAddIncomingVoiceUser;
		_service.EventUserDisconnected -= VoiceBridgeOnEventRemoveIncomingVoiceUser;
		foreach (EscMenuVoiceChatRow row in _rows)
		{
			row.Hide();
		}
	}

	private void DisplayStateStatus(bool connected)
	{
		base.gameObject.SetActive(connected);
		if (connected && !_connected)
		{
			Bind();
		}
		else if (_connected && !connected)
		{
			Unbind();
		}
	}

	private void VoiceBridgeOnEventRemoveIncomingVoiceUser(ConnectedUserVoice connectedUserVoice, int index)
	{
		Unbind();
		Bind();
	}

	private void VoiceBridgeOnEventAddIncomingVoiceUser(ConnectedUserVoice connectedUserVoice, int index)
	{
		_rows[index + 1].Show(connectedUserVoice);
	}
}
