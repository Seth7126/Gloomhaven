using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RenderHeads.Media.AVProMovieCapture;

public class NativePlugin
{
	public enum PixelFormat
	{
		RGBA32,
		BGRA32,
		YCbCr422_YUY2,
		YCbCr422_UYVY,
		YCbCr422_HDYC
	}

	public enum PluginEvent
	{
		CaptureFrameBuffer,
		FreeResources
	}

	public const int PluginID = 262340608;

	public const string ScriptVersion = "3.6.8";

	public const string ExpectedPluginVersion = "3.6.8";

	public const int MaxRenderWidth = 16384;

	public const int MaxRenderHeight = 16384;

	[DllImport("AVProMovieCapture")]
	public static extern IntPtr GetRenderEventFunc();

	[DllImport("AVProMovieCapture")]
	public static extern IntPtr GetFreeResourcesEventFunc();

	[DllImport("AVProMovieCapture")]
	public static extern bool Init();

	[DllImport("AVProMovieCapture")]
	public static extern void Deinit();

	public static string GetPluginVersionString()
	{
		return Marshal.PtrToStringAnsi(GetPluginVersion());
	}

	[DllImport("AVProMovieCapture")]
	public static extern bool IsTrialVersion();

	[DllImport("AVProMovieCapture")]
	public static extern int GetNumAVIVideoCodecs();

	[DllImport("AVProMovieCapture")]
	public static extern bool IsConfigureVideoCodecSupported(int index);

	[DllImport("AVProMovieCapture")]
	public static extern void ConfigureVideoCodec(int index);

	public static string GetAVIVideoCodecName(int index)
	{
		string result = "Invalid";
		StringBuilder stringBuilder = new StringBuilder(256);
		if (GetAVIVideoCodecName(index, stringBuilder, stringBuilder.Capacity))
		{
			result = stringBuilder.ToString();
		}
		return result;
	}

	[DllImport("AVProMovieCapture")]
	public static extern int GetNumAVIAudioCodecs();

	[DllImport("AVProMovieCapture")]
	public static extern bool IsConfigureAudioCodecSupported(int index);

	[DllImport("AVProMovieCapture")]
	public static extern void ConfigureAudioCodec(int index);

	public static string GetAVIAudioCodecName(int index)
	{
		string result = "Invalid";
		StringBuilder stringBuilder = new StringBuilder(256);
		if (GetAVIAudioCodecName(index, stringBuilder, stringBuilder.Capacity))
		{
			result = stringBuilder.ToString();
		}
		return result;
	}

	[DllImport("AVProMovieCapture")]
	public static extern int GetNumAVIAudioInputDevices();

	public static string GetAVIAudioInputDeviceName(int index)
	{
		string result = "Invalid";
		StringBuilder stringBuilder = new StringBuilder(256);
		if (GetAVIAudioInputDeviceName(index, stringBuilder, stringBuilder.Capacity))
		{
			result = stringBuilder.ToString();
		}
		return result;
	}

	[DllImport("AVProMovieCapture")]
	public static extern int CreateRecorderVideo([MarshalAs(UnmanagedType.LPWStr)] string filename, uint width, uint height, int frameRate, int format, bool isTopDown, int videoCodecIndex, bool hasAudio, int audioSampleRate, int audioChannelCount, int audioInputDeviceIndex, int audioCodecIndex, bool isRealTime, bool useMediaFoundation, bool supportAlpha, bool forceGpuFlush);

	[DllImport("AVProMovieCapture")]
	public static extern int CreateRecorderImages([MarshalAs(UnmanagedType.LPWStr)] string filename, uint width, uint height, int frameRate, int format, bool isTopDown, bool isRealTime, int imageFormatType, bool supportAlpha, bool forceGpuFlush, int startFrame);

	[DllImport("AVProMovieCapture")]
	public static extern int CreateRecorderPipe([MarshalAs(UnmanagedType.LPWStr)] string filename, uint width, uint height, int frameRate, int format, bool isTopDown, bool supportAlpha, bool forceGpuFlush);

	[DllImport("AVProMovieCapture")]
	public static extern bool Start(int handle);

	[DllImport("AVProMovieCapture")]
	public static extern bool IsNewFrameDue(int handle);

	[DllImport("AVProMovieCapture")]
	public static extern void EncodeFrame(int handle, IntPtr data);

	[DllImport("AVProMovieCapture")]
	public static extern void EncodeAudio(int handle, IntPtr data, uint length);

	[DllImport("AVProMovieCapture")]
	public static extern void EncodeFrameWithAudio(int handle, IntPtr videoData, IntPtr audioData, uint audioLength);

	[DllImport("AVProMovieCapture")]
	public static extern void Pause(int handle);

	[DllImport("AVProMovieCapture")]
	public static extern void Stop(int handle, bool skipPendingFrames);

	[DllImport("AVProMovieCapture")]
	public static extern void SetTexturePointer(int handle, IntPtr texture);

	[DllImport("AVProMovieCapture")]
	public static extern void FreeRecorder(int handle);

	[DllImport("AVProMovieCapture")]
	public static extern uint GetNumDroppedFrames(int handle);

	[DllImport("AVProMovieCapture")]
	public static extern uint GetNumDroppedEncoderFrames(int handle);

	[DllImport("AVProMovieCapture")]
	public static extern uint GetNumEncodedFrames(int handle);

	[DllImport("AVProMovieCapture")]
	public static extern uint GetEncodedSeconds(int handle);

	[DllImport("AVProMovieCapture")]
	private static extern IntPtr GetPluginVersion();

	[DllImport("AVProMovieCapture")]
	private static extern bool GetAVIVideoCodecName(int index, StringBuilder name, int nameBufferLength);

	[DllImport("AVProMovieCapture")]
	private static extern bool GetAVIAudioCodecName(int index, StringBuilder name, int nameBufferLength);

	[DllImport("AVProMovieCapture")]
	private static extern bool GetAVIAudioInputDeviceName(int index, StringBuilder name, int nameBufferLength);
}
