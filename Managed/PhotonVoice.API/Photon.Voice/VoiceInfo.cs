using POpusCodec.Enums;

namespace Photon.Voice;

public struct VoiceInfo
{
	public Codec Codec { get; set; }

	public int SamplingRate { get; set; }

	public int Channels { get; set; }

	public int FrameDurationUs { get; set; }

	public int Bitrate { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public int FPS { get; set; }

	public int KeyFrameInt { get; set; }

	public object UserData { get; set; }

	public int FrameDurationSamples => (int)((long)SamplingRate * (long)FrameDurationUs / 1000000);

	public int FrameSize => FrameDurationSamples * Channels;

	public static VoiceInfo CreateAudioOpus(SamplingRate samplingRate, int channels, OpusCodec.FrameDuration frameDurationUs, int bitrate, object userdata = null)
	{
		return new VoiceInfo
		{
			Codec = Codec.AudioOpus,
			SamplingRate = (int)samplingRate,
			Channels = channels,
			FrameDurationUs = (int)frameDurationUs,
			Bitrate = bitrate,
			UserData = userdata
		};
	}

	public static VoiceInfo CreateAudio(Codec codec, int samplingRate, int channels, int frameDurationUs, object userdata = null)
	{
		return new VoiceInfo
		{
			Codec = codec,
			SamplingRate = samplingRate,
			Channels = channels,
			FrameDurationUs = frameDurationUs,
			UserData = userdata
		};
	}

	public override string ToString()
	{
		return "c=" + Codec.ToString() + " f=" + SamplingRate + " ch=" + Channels + " d=" + FrameDurationUs + " s=" + FrameSize + " b=" + Bitrate + " w=" + Width + " h=" + Height + " fps=" + FPS + " kfi=" + KeyFrameInt + " ud=" + UserData;
	}
}
