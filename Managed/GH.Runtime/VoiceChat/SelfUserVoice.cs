#define ENABLE_LOGS
using System;
using Photon.Voice.Unity;
using Photon.Voice.Unity.UtilityScripts;
using SM.Utils;
using UnityEngine;

namespace VoiceChat;

public class SelfUserVoice : IUserVoice
{
	private readonly MicAmplifier _amplifier;

	private readonly Recorder _recorder;

	private string _recordDevice;

	private bool _isRecording;

	private bool _hadRecordDevice;

	private float _time;

	public bool IsRecording
	{
		get
		{
			return _isRecording;
		}
		set
		{
			_hadRecordDevice = HasRecordDevice;
			_isRecording = value;
		}
	}

	public bool IsMuted => !_recorder.TransmitEnabled;

	public bool HasRecordDevice => _recorder.CheckIfThereIsAtLeastOneMic();

	public bool IsHost => FFSNetwork.IsHost;

	public float Volume
	{
		get
		{
			return _amplifier.AmplificationFactor;
		}
		set
		{
			_amplifier.AmplificationFactor = value;
		}
	}

	public string Name { get; }

	public string PlatformAccountID => PlatformLayer.UserData.PlatformNetworkAccountPlayerID;

	public string PlatformName => PlatformLayer.Instance.PlatformID;

	public bool IsSpeaking => _recorder.VoiceDetector.Detected;

	public event Action EventMuteChange;

	public void Mute()
	{
		if (HasRecordDevice)
		{
			IsRecording = false;
			_recorder.TransmitEnabled = false;
			this.EventMuteChange?.Invoke();
		}
	}

	public void UnMute()
	{
		if (HasRecordDevice)
		{
			IsRecording = true;
			_recorder.TransmitEnabled = true;
			this.EventMuteChange?.Invoke();
		}
	}

	public SelfUserVoice(MicAmplifier amplifier, Recorder recorder)
	{
		_amplifier = amplifier;
		_recorder = recorder;
		Name = null;
		IsRecording = true;
	}

	public void Update()
	{
		if (!(_time + 1f > Time.time))
		{
			_time = Time.time;
			bool hadRecordDevice = _hadRecordDevice;
			bool num = (_hadRecordDevice = HasRecordDevice);
			bool flag = !num && hadRecordDevice;
			if (num && !hadRecordDevice && IsRecording)
			{
				_recorder.StartRecording();
				this.EventMuteChange?.Invoke();
			}
			else if (flag && IsRecording)
			{
				LogUtils.Log("StopRecording");
				_recorder.StopRecording();
				this.EventMuteChange?.Invoke();
			}
		}
	}
}
