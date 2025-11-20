using System;
using Photon.Voice.Unity;
using UnityEngine;

namespace VoiceChat;

public class ConnectedUserVoice : IUserVoice
{
	private readonly AudioSource _audioSource;

	private readonly Speaker _speaker;

	public bool IsMuted => !_speaker.PlaybackStarted;

	public bool HasRecordDevice => true;

	public bool IsLinked => _speaker.IsLinked;

	public float Volume
	{
		get
		{
			return _audioSource.volume;
		}
		set
		{
			_audioSource.volume = value;
		}
	}

	public string Name { get; private set; }

	public string PlatformAccountID { get; }

	public string PlatformName { get; }

	public int VoiceChatUserId { get; }

	public bool IsHost { get; }

	public bool IsSpeaking => _speaker.IsPlaying;

	public event Action EventMuteChange;

	public ConnectedUserVoice(AudioSource audioSource, Speaker speaker, string name, string platformAccountID, string platformName, int voiceChatUserId, bool isHost)
	{
		_audioSource = audioSource;
		_speaker = speaker;
		Name = name;
		PlatformAccountID = platformAccountID;
		PlatformName = platformName;
		VoiceChatUserId = voiceChatUserId;
		IsHost = isHost;
	}

	public void MaskBadWordsInUsername(Action onCallback)
	{
		if (((IUserVoice)this).IsPCUser)
		{
			Name.GetCensoredStringAsync(delegate(string censoredName)
			{
				Name = censoredName;
				onCallback?.Invoke();
			});
		}
		else
		{
			Name = Name;
			onCallback?.Invoke();
		}
	}

	public void Mute()
	{
		_speaker.StopPlayback();
		this.EventMuteChange?.Invoke();
	}

	public void UnMute()
	{
		_speaker.StartPlayback();
		this.EventMuteChange?.Invoke();
	}
}
