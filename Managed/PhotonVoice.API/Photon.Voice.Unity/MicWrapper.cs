using System;
using System.Linq;
using UnityEngine;

namespace Photon.Voice.Unity;

public class MicWrapper : IAudioReader<float>, IDataReader<float>, IDisposable, IAudioDesc
{
	private AudioClip mic;

	private string device;

	private ILogger logger;

	private int micPrevPos;

	private int micLoopCnt;

	private int readAbsPos;

	public int SamplingRate
	{
		get
		{
			if (Error != null)
			{
				return 0;
			}
			return mic.frequency;
		}
	}

	public int Channels
	{
		get
		{
			if (Error != null)
			{
				return 0;
			}
			return mic.channels;
		}
	}

	public string Error { get; private set; }

	public MicWrapper(string device, int suggestedFrequency, ILogger logger)
	{
		try
		{
			this.device = device;
			this.logger = logger;
			if (UnityMicrophone.devices.Length < 1)
			{
				Error = "No microphones found (UnityMicrophone.devices is empty)";
				logger.LogError("[PV] MicWrapper: " + Error);
				return;
			}
			if (!string.IsNullOrEmpty(device) && !UnityMicrophone.devices.Contains(device))
			{
				logger.LogError($"[PV] MicWrapper: \"{device}\" is not a valid Unity microphone device, falling back to default one");
				device = null;
			}
			logger.LogInfo("[PV] MicWrapper: initializing microphone '{0}', suggested frequency = {1}).", device, suggestedFrequency);
			UnityMicrophone.GetDeviceCaps(device, out var minFreq, out var maxFreq);
			int frequency = suggestedFrequency;
			if (suggestedFrequency < minFreq || (maxFreq != 0 && suggestedFrequency > maxFreq))
			{
				logger.LogWarning("[PV] MicWrapper does not support suggested frequency {0} (min: {1}, max: {2}). Setting to {2}", suggestedFrequency, minFreq, maxFreq);
				frequency = maxFreq;
			}
			mic = UnityMicrophone.Start(device, loop: true, 1, frequency);
			if (mic == null)
			{
				logger.LogError("[PV] MicWrapper: Was not able to start mic. mic is null!");
				return;
			}
			logger.LogInfo("[PV] MicWrapper: microphone '{0}' initialized, frequency = {1}, channels = {2}.", device, mic.frequency, mic.channels);
		}
		catch (Exception ex)
		{
			Error = ex.ToString();
			if (Error == null)
			{
				Error = "Exception in MicWrapper constructor";
			}
			logger.LogError("[PV] MicWrapper: " + Error);
		}
	}

	public void Dispose()
	{
		UnityMicrophone.End(device);
	}

	public bool Read(float[] buffer)
	{
		if (Error != null)
		{
			return false;
		}
		int position = UnityMicrophone.GetPosition(device);
		if (position < micPrevPos)
		{
			micLoopCnt++;
		}
		micPrevPos = position;
		int num = micLoopCnt * mic.samples + position;
		if (mic.channels == 0)
		{
			Error = "Number of channels is 0 in Read()";
			logger.LogError("[PV] MicWrapper: " + Error);
			return false;
		}
		int num2 = buffer.Length / mic.channels;
		int num3 = readAbsPos + num2;
		if (num3 < num)
		{
			mic.GetData(buffer, readAbsPos % mic.samples);
			readAbsPos = num3;
			return true;
		}
		return false;
	}
}
