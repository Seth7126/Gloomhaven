using UnityEngine;

namespace Photon.Voice.Unity;

public class UnityAudioOut : AudioOutDelayControl<float>
{
	protected readonly AudioSource source;

	protected AudioClip clip;

	public override int OutPos => source.timeSamples;

	public UnityAudioOut(AudioSource audioSource, PlayDelayConfig playDelayConfig, ILogger logger, string logPrefix, bool debugInfo)
		: base(processInService: true, playDelayConfig, logger, "[PV] [Unity] AudioOut" + ((logPrefix == "") ? "" : (" " + logPrefix)), debugInfo)
	{
		source = audioSource;
	}

	public override void OutCreate(int frequency, int channels, int bufferSamples)
	{
		source.loop = true;
		clip = AudioClip.Create("UnityAudioOut", bufferSamples, channels, frequency, stream: false);
		source.clip = clip;
	}

	public override void OutStart()
	{
		source.Play();
	}

	public override void OutWrite(float[] data, int offsetSamples)
	{
		clip.SetData(data, offsetSamples);
	}

	public override void Stop()
	{
		base.Stop();
		source.Stop();
		if (source != null)
		{
			source.clip = null;
			clip = null;
		}
	}
}
