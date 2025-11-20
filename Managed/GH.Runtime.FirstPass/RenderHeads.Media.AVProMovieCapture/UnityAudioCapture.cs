#define ENABLE_LOGS
using System;
using System.Runtime.InteropServices;
using SM.Utils;
using UnityEngine;

namespace RenderHeads.Media.AVProMovieCapture;

[AddComponentMenu("AVPro Movie Capture/Unity Audio Capture", 500)]
public class UnityAudioCapture : MonoBehaviour
{
	[SerializeField]
	private bool _debugLogging;

	[SerializeField]
	private bool _muteAudio;

	private const int BufferSize = 16;

	private float[] _buffer = new float[0];

	private float[] _readBuffer = new float[0];

	private int _bufferIndex;

	private GCHandle _bufferHandle;

	private int _numChannels;

	private int _overflowCount;

	private object _lockObject = new object();

	public float[] Buffer => _readBuffer;

	public int BufferLength => _bufferIndex;

	public int NumChannels => _numChannels;

	public IntPtr BufferPtr => _bufferHandle.AddrOfPinnedObject();

	public int OverflowCount => _overflowCount;

	private void OnEnable()
	{
		int bufferLength = 0;
		int numBuffers = 0;
		AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);
		_numChannels = GetNumChannels(AudioSettings.driverCapabilities);
		if (AudioSettings.speakerMode < AudioSettings.driverCapabilities)
		{
			_numChannels = GetNumChannels(AudioSettings.speakerMode);
		}
		if (_debugLogging)
		{
			LogUtils.Log($"[UnityAudioCapture] SampleRate: {AudioSettings.outputSampleRate}hz SpeakerMode: {AudioSettings.speakerMode.ToString()} BestDriverMode: {AudioSettings.driverCapabilities.ToString()} (DSP using {numBuffers} buffers of {bufferLength} bytes using {_numChannels} channels)");
		}
		_buffer = new float[bufferLength * _numChannels * numBuffers * 16];
		_readBuffer = new float[bufferLength * _numChannels * numBuffers * 16];
		_bufferIndex = 0;
		_bufferHandle = GCHandle.Alloc(_readBuffer, GCHandleType.Pinned);
		_overflowCount = 0;
	}

	private void OnDisable()
	{
		lock (_lockObject)
		{
			_bufferIndex = 0;
			if (_bufferHandle.IsAllocated)
			{
				_bufferHandle.Free();
			}
			_readBuffer = (_buffer = null);
		}
		_numChannels = 0;
	}

	public IntPtr ReadData(out int length)
	{
		lock (_lockObject)
		{
			Array.Copy(_buffer, 0, _readBuffer, 0, _bufferIndex);
			length = _bufferIndex;
			_bufferIndex = 0;
		}
		return _bufferHandle.AddrOfPinnedObject();
	}

	public void FlushBuffer()
	{
		lock (_lockObject)
		{
			_bufferIndex = 0;
			_overflowCount = 0;
		}
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		if (_buffer == null)
		{
			return;
		}
		lock (_lockObject)
		{
			int num = Mathf.Min(data.Length, _buffer.Length - _bufferIndex);
			if (!_muteAudio)
			{
				for (int i = 0; i < num; i++)
				{
					_buffer[i + _bufferIndex] = data[i];
				}
			}
			else
			{
				for (int j = 0; j < num; j++)
				{
					_buffer[j + _bufferIndex] = data[j];
					data[j] = 0f;
				}
			}
			_bufferIndex += num;
			if (num < data.Length)
			{
				_overflowCount++;
				LogUtils.LogWarning("[AVProMovieCapture] Audio buffer overflow, may cause sync issues.  Disable this component if not recording Unity audio.");
			}
		}
	}

	public static int GetNumChannels(AudioSpeakerMode mode)
	{
		int result = 0;
		switch (mode)
		{
		case AudioSpeakerMode.Mono:
			result = 1;
			break;
		case AudioSpeakerMode.Stereo:
			result = 2;
			break;
		case AudioSpeakerMode.Quad:
			result = 4;
			break;
		case AudioSpeakerMode.Surround:
			result = 5;
			break;
		case AudioSpeakerMode.Mode5point1:
			result = 6;
			break;
		case AudioSpeakerMode.Mode7point1:
			result = 8;
			break;
		case AudioSpeakerMode.Prologic:
			result = 2;
			break;
		}
		return result;
	}
}
