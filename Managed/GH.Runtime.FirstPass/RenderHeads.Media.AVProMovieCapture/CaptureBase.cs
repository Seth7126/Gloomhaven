#define ENABLE_LOGS
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using SM.Utils;
using UnityEngine;

namespace RenderHeads.Media.AVProMovieCapture;

public class CaptureBase : MonoBehaviour
{
	public enum FrameRate
	{
		One = 1,
		Ten = 10,
		Fifteen = 15,
		TwentyFour = 24,
		TwentyFive = 25,
		Thirty = 30,
		Fifty = 50,
		Sixty = 60,
		SeventyFive = 75,
		Ninety = 90,
		OneTwenty = 120
	}

	public enum Resolution
	{
		POW2_8192x8192,
		POW2_8192x4096,
		POW2_4096x4096,
		POW2_4096x2048,
		POW2_2048x4096,
		UHD_3840x2160,
		UHD_3840x2048,
		UHD_3840x1920,
		POW2_2048x2048,
		POW2_2048x1024,
		HD_1920x1080,
		HD_1280x720,
		SD_1024x768,
		SD_800x600,
		SD_800x450,
		SD_640x480,
		SD_640x360,
		SD_320x240,
		Original,
		Custom
	}

	public enum CubemapDepth
	{
		Depth_24 = 24,
		Depth_16 = 16,
		Depth_Zero = 0
	}

	public enum CubemapResolution
	{
		POW2_4096 = 4096,
		POW2_2048 = 2048,
		POW2_1024 = 1024,
		POW2_512 = 512,
		POW2_256 = 256
	}

	public enum AntiAliasingLevel
	{
		UseCurrent,
		ForceNone,
		ForceSample2,
		ForceSample4,
		ForceSample8
	}

	public enum DownScale
	{
		Original = 1,
		Half = 2,
		Quarter = 4,
		Eighth = 8,
		Sixteenth = 16,
		Custom = 100
	}

	public enum OutputPath
	{
		RelativeToProject,
		RelativeToPeristentData,
		Absolute
	}

	public enum OutputExtension
	{
		AVI = 0,
		MP4 = 1,
		PNG = 2,
		Custom = 100
	}

	public enum OutputType
	{
		VideoFile,
		ImageSequence,
		NamedPipe
	}

	[Serializable]
	public class PostCaptureSettings
	{
		[SerializeField]
		[Tooltip("Move the 'moov' atom in the MP4 file from the end to the start of the file to make streaming start fast.  Also called 'Fast Start' in some encoders")]
		public bool writeFastStartStreamingForMp4 = true;
	}

	[SerializeField]
	private PostCaptureSettings _postCaptureSettings = new PostCaptureSettings();

	public KeyCode _captureKey;

	public bool _captureOnStart;

	public bool _startPaused;

	public bool _listVideoCodecsOnStart;

	public bool _isRealTime = true;

	[SerializeField]
	private bool _persistAcrossSceneLoads;

	public StopMode _stopMode;

	public int _stopFrames;

	public float _stopSeconds;

	public bool _useMediaFoundationH264;

	[SerializeField]
	private string[] _videoCodecPriority = new string[3] { "Lagarith Lossless Codec", "x264vfw - H.264/MPEG-4 AVC codec", "Xvid MPEG-4 Codec" };

	public FrameRate _frameRate = FrameRate.Thirty;

	[SerializeField]
	[Tooltip("Timelapse scale makes the frame capture run at a fraction of the target frame rate.  Default value is 1")]
	private int _timelapseScale = 1;

	public DownScale _downScale = DownScale.Original;

	public Vector2 _maxVideoSize = Vector2.zero;

	public int _forceVideoCodecIndex = -1;

	public bool _flipVertically;

	public bool _supportAlpha;

	[Tooltip("Flushing the GPU during each capture results in less latency, but can slow down rendering performance for complex scenes.")]
	[SerializeField]
	private bool _forceGpuFlush;

	public bool _noAudio = true;

	public string[] _audioCodecPriority = new string[0];

	public int _forceAudioCodecIndex = -1;

	public int _forceAudioDeviceIndex = -1;

	public UnityAudioCapture _audioCapture;

	public bool _autoGenerateFilename = true;

	public OutputPath _outputFolderType;

	public string _outputFolderPath = "Captures";

	public string _autoFilenamePrefix = "MovieCapture";

	public string _autoFilenameExtension = "avi";

	public string _forceFilename = "movie.avi";

	public int _imageSequenceStartFrame;

	[Range(2f, 12f)]
	public int _imageSequenceZeroDigits = 6;

	public OutputType _outputType;

	public ImageSequenceFormat _imageSequenceFormat;

	public Resolution _renderResolution = Resolution.Original;

	public Vector2 _renderSize = Vector2.one;

	public int _renderAntiAliasing = -1;

	public bool _useMotionBlur;

	[Range(0f, 64f)]
	public int _motionBlurSamples = 16;

	public Camera[] _motionBlurCameras;

	protected MotionBlur _motionBlur;

	public bool _allowVSyncDisable = true;

	[SerializeField]
	protected bool _supportTextureRecreate;

	public bool _captureMouseCursor;

	public MouseCursor _mouseCursor;

	[NonSerialized]
	public string _codecName = "uncompressed";

	[NonSerialized]
	public int _codecIndex = -1;

	[NonSerialized]
	public string _audioCodecName = "uncompressed";

	[NonSerialized]
	public int _audioCodecIndex = -1;

	[NonSerialized]
	public string _audioDeviceName = "Unity";

	[NonSerialized]
	public int _audioDeviceIndex = -1;

	[NonSerialized]
	public int _unityAudioSampleRate = -1;

	[NonSerialized]
	public int _unityAudioChannelCount = -1;

	protected Texture2D _texture;

	protected int _handle = -1;

	protected int _targetWidth;

	protected int _targetHeight;

	protected bool _capturing;

	protected bool _paused;

	protected string _filePath;

	protected FileInfo _fileInfo;

	protected NativePlugin.PixelFormat _pixelFormat = NativePlugin.PixelFormat.YCbCr422_YUY2;

	private int _oldVSyncCount;

	private float _oldFixedDeltaTime;

	protected bool _isTopDown = true;

	protected bool _isDirectX11;

	private bool _queuedStartCapture;

	private bool _queuedStopCapture;

	private float _captureStartTime;

	private float _timeSinceLastFrame;

	public int _minimumDiskSpaceMB = -1;

	private long _freeDiskSpaceMB;

	private uint _numDroppedFrames;

	private uint _numDroppedEncoderFrames;

	private uint _numEncodedFrames;

	private uint _totalEncodedSeconds;

	private static bool _isInitialised;

	protected IntPtr _renderEventFunction = IntPtr.Zero;

	protected IntPtr _freeEventFunction = IntPtr.Zero;

	private float _fps;

	private int _frameTotal;

	private int _frameCount;

	private float _startFrameTime;

	public string LastFilePath => _filePath;

	public uint NumDroppedFrames => _numDroppedFrames;

	public uint NumDroppedEncoderFrames => _numDroppedEncoderFrames;

	public uint NumEncodedFrames => _numEncodedFrames;

	public uint TotalEncodedSeconds => _totalEncodedSeconds;

	public string[] VideoCodecPriority
	{
		get
		{
			return _videoCodecPriority;
		}
		set
		{
			_videoCodecPriority = value;
			SelectCodec(listCodecs: false);
		}
	}

	public int TimelapseScale
	{
		get
		{
			return _timelapseScale;
		}
		set
		{
			_timelapseScale = value;
		}
	}

	public PostCaptureSettings PostCapture => _postCaptureSettings;

	public static string LastFileSaved
	{
		get
		{
			return PlayerPrefs.GetString("AVProMovieCapture-LastSavedFile", string.Empty);
		}
		set
		{
			PlayerPrefs.SetString("AVProMovieCapture-LastSavedFile", value);
		}
	}

	public float FPS => _fps;

	public float FramesTotal => _frameTotal;

	protected virtual void Awake()
	{
		if (!_isInitialised)
		{
			try
			{
				string pluginVersionString = NativePlugin.GetPluginVersionString();
				if (!pluginVersionString.StartsWith("3.6.8"))
				{
					LogUtils.LogWarning("[AVProMovieCapture] Plugin version number " + pluginVersionString + " doesn't match the expected version number 3.6.8.  It looks like the plugin didn't upgrade correctly.  To resolve this please restart Unity and try to upgrade the package again.");
				}
				if (NativePlugin.Init())
				{
					LogUtils.Log("[AVProMovieCapture] Init plugin version: " + pluginVersionString + " (script v3.6.8) with GPU " + SystemInfo.graphicsDeviceName + " " + SystemInfo.graphicsDeviceVersion);
					SetupRenderFunctions();
					_isInitialised = true;
				}
				else
				{
					LogUtils.LogError("[AVProMovieCapture] Failed to initialise plugin version: " + pluginVersionString + " (script v3.6.8) with GPU " + SystemInfo.graphicsDeviceName + " " + SystemInfo.graphicsDeviceVersion);
				}
			}
			catch (DllNotFoundException ex)
			{
				string empty = string.Empty;
				empty = "Unity couldn't find the plugin DLL. Please select the native plugin files in 'Plugins/RenderHeads/AVProMovieCapture/Plugins' folder and select the correct platform in the Inspector.";
				LogUtils.LogError("[AVProMovieCapture] " + empty);
				throw ex;
			}
		}
		else
		{
			SetupRenderFunctions();
		}
		_isDirectX11 = SystemInfo.graphicsDeviceVersion.StartsWith("Direct3D 11");
		SelectCodec(_listVideoCodecsOnStart);
		SelectAudioCodec(_listVideoCodecsOnStart);
		SelectAudioDevice(_listVideoCodecsOnStart);
		if (_persistAcrossSceneLoads)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	protected void SetupRenderFunctions()
	{
		_renderEventFunction = NativePlugin.GetRenderEventFunc();
		_freeEventFunction = NativePlugin.GetFreeResourcesEventFunc();
		LogUtils.Assert(_renderEventFunction != IntPtr.Zero, string.Empty);
		LogUtils.Assert(_freeEventFunction != IntPtr.Zero, string.Empty);
	}

	public virtual void Start()
	{
		Application.runInBackground = true;
		if (_captureOnStart)
		{
			StartCapture();
		}
	}

	public void SelectCodec(bool listCodecs)
	{
		int numAVIVideoCodecs = NativePlugin.GetNumAVIVideoCodecs();
		if (listCodecs)
		{
			for (int i = 0; i < numAVIVideoCodecs; i++)
			{
				LogUtils.Log("VideoCodec " + i + ": " + NativePlugin.GetAVIVideoCodecName(i));
			}
		}
		_codecIndex = -1;
		_codecName = "Uncompressed";
		if (_useMediaFoundationH264)
		{
			_codecName = "Media Foundation H.264(MP4)";
		}
		else if (_forceVideoCodecIndex >= 0)
		{
			if (_forceVideoCodecIndex < numAVIVideoCodecs)
			{
				_codecName = NativePlugin.GetAVIVideoCodecName(_forceVideoCodecIndex);
				_codecIndex = _forceVideoCodecIndex;
			}
			if (_codecIndex < 0)
			{
				_codecName = "Uncompressed";
				LogUtils.LogWarning("[AVProMovieCapture] Video codec not found.  Video will be uncompressed.");
			}
		}
		else
		{
			if (_videoCodecPriority == null || _videoCodecPriority.Length == 0)
			{
				return;
			}
			string[] videoCodecPriority = _videoCodecPriority;
			for (int j = 0; j < videoCodecPriority.Length; j++)
			{
				string text = videoCodecPriority[j].Trim();
				if (string.IsNullOrEmpty(text))
				{
					_codecName = "Uncompressed";
					break;
				}
				for (int k = 0; k < numAVIVideoCodecs; k++)
				{
					if (text == NativePlugin.GetAVIVideoCodecName(k))
					{
						_codecName = text;
						_codecIndex = k;
						break;
					}
				}
				if (_codecIndex >= 0)
				{
					break;
				}
			}
			if (_codecIndex < 0)
			{
				_codecName = "Uncompressed";
				LogUtils.LogWarning("[AVProMovieCapture] Video codec not found.  Video will be uncompressed.");
			}
		}
	}

	public void SelectAudioCodec(bool listCodecs)
	{
		int numAVIAudioCodecs = NativePlugin.GetNumAVIAudioCodecs();
		if (listCodecs)
		{
			for (int i = 0; i < numAVIAudioCodecs; i++)
			{
				LogUtils.Log("AudioCodec " + i + ": " + NativePlugin.GetAVIAudioCodecName(i));
			}
		}
		_audioCodecIndex = -1;
		if (_forceAudioCodecIndex >= 0)
		{
			if (_forceAudioCodecIndex < numAVIAudioCodecs)
			{
				_audioCodecName = NativePlugin.GetAVIAudioCodecName(_forceAudioCodecIndex);
				_audioCodecIndex = _forceAudioCodecIndex;
			}
			if (_audioCodecIndex < 0)
			{
				_audioCodecName = "Uncompressed";
				LogUtils.LogWarning("[AVProMovieCapture] Audio codec not found.  Audio will be uncompressed.");
			}
		}
		else
		{
			if (_audioCodecPriority == null || _audioCodecPriority.Length == 0)
			{
				return;
			}
			string[] audioCodecPriority = _audioCodecPriority;
			for (int j = 0; j < audioCodecPriority.Length; j++)
			{
				string text = audioCodecPriority[j].Trim();
				if (string.IsNullOrEmpty(text))
				{
					break;
				}
				for (int k = 0; k < numAVIAudioCodecs; k++)
				{
					if (text == NativePlugin.GetAVIAudioCodecName(k))
					{
						_audioCodecName = text;
						_audioCodecIndex = k;
						break;
					}
				}
				if (_audioCodecIndex >= 0)
				{
					break;
				}
			}
			if (_audioCodecIndex < 0)
			{
				_audioCodecName = "Uncompressed";
				LogUtils.LogWarning("[AVProMovieCapture] Codec not found.  Audio will be uncompressed.");
			}
		}
	}

	public void SelectAudioDevice(bool display)
	{
		int numAVIAudioInputDevices = NativePlugin.GetNumAVIAudioInputDevices();
		if (display)
		{
			for (int i = 0; i < numAVIAudioInputDevices; i++)
			{
				LogUtils.Log("AudioDevice " + i + ": " + NativePlugin.GetAVIAudioInputDeviceName(i));
			}
		}
		if (_forceAudioDeviceIndex >= 0)
		{
			if (_forceAudioDeviceIndex < numAVIAudioInputDevices)
			{
				_audioDeviceName = NativePlugin.GetAVIAudioInputDeviceName(_forceAudioDeviceIndex);
				_audioDeviceIndex = _forceAudioDeviceIndex;
			}
		}
		else
		{
			_audioDeviceName = "Unity";
			_audioDeviceIndex = -1;
		}
	}

	public static Vector2 GetRecordingResolution(int width, int height, DownScale downscale, Vector2 maxVideoSize)
	{
		int num = width;
		int num2 = height;
		if (downscale != DownScale.Custom)
		{
			num /= (int)downscale;
			num2 /= (int)downscale;
		}
		else if (maxVideoSize.x >= 1f && maxVideoSize.y >= 1f)
		{
			num = Mathf.FloorToInt(maxVideoSize.x);
			num2 = Mathf.FloorToInt(maxVideoSize.y);
		}
		num = NextMultipleOf4(num);
		num2 = NextMultipleOf4(num2);
		return new Vector2(num, num2);
	}

	public void SelectRecordingResolution(int width, int height)
	{
		_targetWidth = width;
		_targetHeight = height;
		if (_downScale != DownScale.Custom)
		{
			_targetWidth /= (int)_downScale;
			_targetHeight /= (int)_downScale;
		}
		else if (_maxVideoSize.x >= 1f && _maxVideoSize.y >= 1f)
		{
			_targetWidth = Mathf.FloorToInt(_maxVideoSize.x);
			_targetHeight = Mathf.FloorToInt(_maxVideoSize.y);
		}
		_targetWidth = NextMultipleOf4(_targetWidth);
		_targetHeight = NextMultipleOf4(_targetHeight);
	}

	public virtual void OnDestroy()
	{
		StopCapture();
	}

	private void OnApplicationQuit()
	{
		StopCapture();
		if (_isInitialised)
		{
			NativePlugin.Deinit();
			_isInitialised = false;
		}
	}

	protected void EncodeTexture(Texture2D texture)
	{
		GCHandle gCHandle = GCHandle.Alloc(texture.GetPixels32(), GCHandleType.Pinned);
		EncodePointer(gCHandle.AddrOfPinnedObject());
		if (gCHandle.IsAllocated)
		{
			gCHandle.Free();
		}
	}

	protected bool IsUsingUnityAudio()
	{
		if (_isRealTime && !_noAudio && _audioDeviceIndex < 0)
		{
			return _audioCapture != null;
		}
		return false;
	}

	protected bool IsRecordingUnityAudio()
	{
		if (IsUsingUnityAudio())
		{
			return _audioCapture.isActiveAndEnabled;
		}
		return false;
	}

	protected bool IsUsingMotionBlur()
	{
		if (_useMotionBlur && !_isRealTime)
		{
			return _motionBlur != null;
		}
		return false;
	}

	public virtual void EncodePointer(IntPtr ptr)
	{
		if (!IsRecordingUnityAudio())
		{
			NativePlugin.EncodeFrame(_handle, ptr);
			return;
		}
		int length = 0;
		IntPtr audioData = _audioCapture.ReadData(out length);
		if (length > 0)
		{
			NativePlugin.EncodeFrameWithAudio(_handle, ptr, audioData, (uint)length);
		}
		else
		{
			NativePlugin.EncodeFrame(_handle, ptr);
		}
	}

	public bool IsCapturing()
	{
		return _capturing;
	}

	public bool IsPaused()
	{
		return _paused;
	}

	public int GetRecordingWidth()
	{
		return _targetWidth;
	}

	public int GetRecordingHeight()
	{
		return _targetHeight;
	}

	protected virtual string GenerateTimestampedFilename(string filenamePrefix, string filenameExtension)
	{
		TimeSpan timeSpan = DateTime.Now - DateTime.Now.Date;
		return string.Format("{0}-{1}-{2}-{3}-{4}s-{5}x{6}.{7}", filenamePrefix, DateTime.Now.Year, DateTime.Now.Month.ToString("D2"), DateTime.Now.Day.ToString("D2"), ((int)timeSpan.TotalSeconds).ToString(), _targetWidth, _targetHeight, filenameExtension);
	}

	private static string GetFolder(OutputPath outputPathType, string path)
	{
		string result = string.Empty;
		switch (outputPathType)
		{
		case OutputPath.RelativeToProject:
			result = Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..")), path);
			break;
		case OutputPath.RelativeToPeristentData:
			result = Path.Combine(Path.GetFullPath(Application.persistentDataPath), path);
			break;
		case OutputPath.Absolute:
			result = path;
			break;
		}
		return result;
	}

	private static string AutoGenerateFilename(OutputPath outputPathType, string path, string filename)
	{
		return Path.Combine(GetFolder(outputPathType, path), filename);
	}

	private static string ManualGenerateFilename(OutputPath outputPathType, string path, string filename)
	{
		string result = filename;
		if (!Path.IsPathRooted(filename))
		{
			result = GetFolder(outputPathType, path);
			result = Path.Combine(result, filename);
		}
		return result;
	}

	protected static bool HasExtension(string path, string extension)
	{
		return path.ToLower().EndsWith(extension, StringComparison.OrdinalIgnoreCase);
	}

	protected void GenerateFilename()
	{
		if (_outputType == OutputType.VideoFile)
		{
			if (_autoGenerateFilename || string.IsNullOrEmpty(_forceFilename))
			{
				string filename = GenerateTimestampedFilename(_autoFilenamePrefix, _autoFilenameExtension);
				_filePath = AutoGenerateFilename(_outputFolderType, _outputFolderPath, filename);
			}
			else
			{
				_filePath = ManualGenerateFilename(_outputFolderType, _outputFolderPath, _forceFilename);
			}
			if (_useMediaFoundationH264 && !HasExtension(_filePath, ".mp4"))
			{
				LogUtils.LogWarning("[AVProMovieCapture] Media Foundation H.264 MP4 Encoder selected but file extension is not set to 'mp4' - replacing extension");
				_filePath += ".mp4";
			}
			string directoryName = Path.GetDirectoryName(_filePath);
			if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
		}
		else if (_outputType == OutputType.ImageSequence)
		{
			string filename2 = _autoFilenamePrefix + "/" + _autoFilenamePrefix + $"-%0{_imageSequenceZeroDigits}d." + "png";
			_filePath = ManualGenerateFilename(_outputFolderType, _outputFolderPath, filename2);
			string directoryName2 = Path.GetDirectoryName(_filePath);
			if (!string.IsNullOrEmpty(directoryName2) && !Directory.Exists(directoryName2))
			{
				Directory.CreateDirectory(directoryName2);
			}
		}
		else
		{
			_filePath = _forceFilename;
		}
	}

	public virtual bool PrepareCapture()
	{
		if (_outputType == OutputType.VideoFile && File.Exists(_filePath))
		{
			File.Delete(_filePath);
		}
		_numDroppedFrames = 0u;
		_numDroppedEncoderFrames = 0u;
		_numEncodedFrames = 0u;
		_totalEncodedSeconds = 0u;
		if (_minimumDiskSpaceMB > 0 && _outputType == OutputType.VideoFile)
		{
			ulong freespace = 0uL;
			if (Utils.DriveFreeBytes(Path.GetPathRoot(_filePath), out freespace))
			{
				_freeDiskSpaceMB = (long)(freespace / 1048576);
			}
			if (!IsEnoughDiskSpace())
			{
				LogUtils.LogError("[AVProMovieCapture] Not enough free space to start capture.  Stopping capture.");
				return false;
			}
		}
		if (!_isRealTime)
		{
			if (_allowVSyncDisable && !Screen.fullScreen && QualitySettings.vSyncCount > 0)
			{
				_oldVSyncCount = QualitySettings.vSyncCount;
				QualitySettings.vSyncCount = 0;
			}
			if (_useMotionBlur && _motionBlurSamples > 1)
			{
				Time.captureFramerate = _motionBlurSamples * (int)_frameRate;
				if (this is CaptureFromTexture || this is CaptureFromCamera360 || this is CaptureFromCamera360ODS)
				{
					if (_motionBlur == null)
					{
						_motionBlur = GetComponent<MotionBlur>();
					}
					if (_motionBlur == null)
					{
						_motionBlur = base.gameObject.AddComponent<MotionBlur>();
					}
					if (_motionBlur != null)
					{
						_motionBlur.NumSamples = _motionBlurSamples;
						_motionBlur.SetTargetSize(_targetWidth, _targetHeight);
						_motionBlur.enabled = false;
					}
				}
				else if (_motionBlurCameras.Length != 0)
				{
					Camera[] motionBlurCameras = _motionBlurCameras;
					foreach (Camera camera in motionBlurCameras)
					{
						MotionBlur motionBlur = camera.GetComponent<MotionBlur>();
						if (motionBlur == null)
						{
							motionBlur = camera.gameObject.AddComponent<MotionBlur>();
						}
						if (motionBlur != null)
						{
							motionBlur.NumSamples = _motionBlurSamples;
							motionBlur.enabled = true;
							_motionBlur = motionBlur;
						}
					}
				}
			}
			else
			{
				Time.captureFramerate = (int)_frameRate;
			}
			_oldFixedDeltaTime = Time.fixedDeltaTime;
			Time.fixedDeltaTime = 1f / (float)Time.captureFramerate;
		}
		int audioInputDeviceIndex = _audioDeviceIndex;
		int audioCodecIndex = _audioCodecIndex;
		bool flag = _noAudio;
		if (!_isRealTime)
		{
			flag = true;
		}
		if (_mouseCursor != null)
		{
			_mouseCursor.enabled = _captureMouseCursor;
		}
		if (!flag && _audioCapture == null && _audioDeviceIndex < 0)
		{
			_audioCapture = GetComponent<UnityAudioCapture>();
			if (_audioCapture == null)
			{
				_audioCapture = UnityEngine.Object.FindObjectOfType<UnityAudioCapture>();
			}
			if (_audioCapture == null)
			{
				AudioListener audioListener = GetComponent<AudioListener>();
				if (audioListener == null)
				{
					audioListener = UnityEngine.Object.FindObjectOfType<AudioListener>();
				}
				if (audioListener != null)
				{
					_audioCapture = audioListener.gameObject.AddComponent<UnityAudioCapture>();
					LogUtils.LogWarning("[AVProMovieCapture] Capturing audio from Unity without an UnityAudioCapture assigned so we had to create one manually (very slow).  Consider adding a UnityAudioCapture component to your scene and assigned it to this MovieCapture component.");
				}
				else
				{
					flag = true;
					LogUtils.LogWarning("[AVProMovieCapture] No audio listener found in scene.  Unable to capture audio from Untiy.");
				}
			}
			else
			{
				LogUtils.LogWarning("[AVProMovieCapture] Capturing audio from Unity without an UnityAudioCapture assigned so we had to search for one manually (very slow)");
			}
		}
		if (flag || (_audioCapture == null && _audioDeviceIndex < 0))
		{
			audioCodecIndex = (audioInputDeviceIndex = -1);
			_audioDeviceName = "none";
			flag = true;
		}
		_unityAudioSampleRate = -1;
		_unityAudioChannelCount = -1;
		if (IsUsingUnityAudio())
		{
			if (!_audioCapture.enabled)
			{
				_audioCapture.enabled = true;
			}
			_unityAudioSampleRate = AudioSettings.outputSampleRate;
			_unityAudioChannelCount = _audioCapture.NumChannels;
		}
		object[] obj = new object[4] { _targetWidth, _targetHeight, null, null };
		int i = (int)_frameRate;
		obj[2] = i.ToString();
		obj[3] = _pixelFormat.ToString();
		string text = string.Format("{0}x{1} @ {2}fps [{3}]", obj);
		if (_outputType == OutputType.VideoFile)
		{
			text = (_useMediaFoundationH264 ? (text + " vcodec:'Media Foundation H.264'") : (text + $" vcodec:'{_codecName}'"));
			if (!flag)
			{
				text = ((_audioDeviceIndex < 0) ? (text + $" audio device:'Unity' {_unityAudioSampleRate}hz {_unityAudioChannelCount} channels") : (text + $" audio device:'{_audioDeviceName}'"));
				text += $" acodec:'{_audioCodecName}'";
			}
			text += $" to file: '{_filePath}'";
		}
		else if (_outputType == OutputType.ImageSequence)
		{
			text += $" to file: '{_filePath}'";
		}
		else if (_outputType == OutputType.NamedPipe)
		{
			text += $" to pipe: '{_filePath}'";
		}
		if (_flipVertically)
		{
			_isTopDown = !_isTopDown;
		}
		if (_outputType == OutputType.VideoFile)
		{
			LogUtils.Log("[AVProMovieCapture] Start File Capture: " + text);
			bool isRealTime = _isRealTime && _timelapseScale <= 1;
			_handle = NativePlugin.CreateRecorderVideo(_filePath, (uint)_targetWidth, (uint)_targetHeight, (int)_frameRate, (int)_pixelFormat, _isTopDown, _codecIndex, !flag, _unityAudioSampleRate, _unityAudioChannelCount, audioInputDeviceIndex, audioCodecIndex, isRealTime, _useMediaFoundationH264, _supportAlpha, _forceGpuFlush);
		}
		else if (_outputType == OutputType.ImageSequence)
		{
			LogUtils.Log("[AVProMovieCapture] Start Images Capture: " + text);
			bool isRealTime2 = _isRealTime && _timelapseScale <= 1;
			_handle = NativePlugin.CreateRecorderImages(_filePath, (uint)_targetWidth, (uint)_targetHeight, (int)_frameRate, (int)_pixelFormat, _isTopDown, isRealTime2, (int)_imageSequenceFormat, _supportAlpha, _forceGpuFlush, _imageSequenceStartFrame);
		}
		else if (_outputType == OutputType.NamedPipe)
		{
			LogUtils.Log("[AVProMovieCapture] Start Pipe Capture: " + text);
			_handle = NativePlugin.CreateRecorderPipe(_filePath, (uint)_targetWidth, (uint)_targetHeight, (int)_frameRate, (int)_pixelFormat, _isTopDown, _supportAlpha, _forceGpuFlush);
		}
		if (_handle < 0)
		{
			LogUtils.LogError("[AVProMovieCapture] Failed to create recorder");
			if (!HasExtension(_filePath, ".mp4") && _useMediaFoundationH264)
			{
				LogUtils.LogError("[AVProMovieCapture] When using MF H.264 codec the MP4 extension must be used");
			}
			else if (_useMediaFoundationH264 && _targetWidth * _targetHeight >= 9360000)
			{
				LogUtils.LogError("[AVProMovieCapture] Resolution is possibly too high for the MF H.264 codec");
			}
			if ((HasExtension(_filePath, ".mp4") && !_useMediaFoundationH264) || _codecIndex == 0)
			{
				LogUtils.LogError("[AVProMovieCapture] Uncompressed video codec not supported with MP4 extension, use AVI instead for uncompressed");
			}
			StopCapture();
		}
		return _handle >= 0;
	}

	public void QueueStartCapture()
	{
		_queuedStartCapture = true;
	}

	public bool StartCapture()
	{
		if (_capturing)
		{
			return false;
		}
		if (_handle < 0 && !PrepareCapture())
		{
			return false;
		}
		if (_handle >= 0)
		{
			if (IsUsingUnityAudio())
			{
				_audioCapture.FlushBuffer();
			}
			if (!NativePlugin.Start(_handle))
			{
				StopCapture(skipPendingFrames: true);
				LogUtils.LogError("[AVProMovieCapture] Failed to start recorder");
				return false;
			}
			ResetFPS();
			_captureStartTime = Time.realtimeSinceStartup;
			_timeSinceLastFrame = GetSecondsPerCaptureFrame();
			_capturing = true;
			_paused = false;
		}
		if (_startPaused)
		{
			PauseCapture();
		}
		return _capturing;
	}

	public void PauseCapture()
	{
		if (_capturing && !_paused)
		{
			if (IsUsingUnityAudio())
			{
				_audioCapture.enabled = false;
			}
			NativePlugin.Pause(_handle);
			if (!_isRealTime)
			{
				Time.timeScale = 0f;
			}
			_paused = true;
			ResetFPS();
		}
	}

	public void ResumeCapture()
	{
		if (_capturing && _paused)
		{
			if (IsUsingUnityAudio())
			{
				_audioCapture.FlushBuffer();
				_audioCapture.enabled = true;
			}
			NativePlugin.Start(_handle);
			if (!_isRealTime)
			{
				Time.timeScale = 1f;
			}
			_paused = false;
			if (_startPaused)
			{
				_captureStartTime = Time.realtimeSinceStartup;
				_startPaused = false;
			}
		}
	}

	public void CancelCapture()
	{
		StopCapture(skipPendingFrames: true);
		if (_outputType == OutputType.VideoFile && File.Exists(_filePath))
		{
			File.Delete(_filePath);
		}
	}

	public virtual void UnprepareCapture()
	{
		if (_mouseCursor != null)
		{
			_mouseCursor.enabled = false;
		}
	}

	protected void RenderThreadEvent(NativePlugin.PluginEvent renderEvent)
	{
		if (_renderEventFunction == IntPtr.Zero)
		{
			SetupRenderFunctions();
		}
		switch (renderEvent)
		{
		case NativePlugin.PluginEvent.CaptureFrameBuffer:
			GL.IssuePluginEvent(_renderEventFunction, (int)((NativePlugin.PluginEvent)262340608 | renderEvent) | _handle);
			break;
		case NativePlugin.PluginEvent.FreeResources:
			GL.IssuePluginEvent(_freeEventFunction, (int)((NativePlugin.PluginEvent)262340608 | renderEvent));
			break;
		}
	}

	public virtual void StopCapture(bool skipPendingFrames = false)
	{
		UnprepareCapture();
		if (_capturing)
		{
			LogUtils.Log("[AVProMovieCapture] Stopping capture " + _handle);
			_capturing = false;
		}
		bool flag = false;
		if (_handle >= 0)
		{
			NativePlugin.FreeRecorder(_handle);
			_handle = -1;
			if (!skipPendingFrames && !string.IsNullOrEmpty(_filePath))
			{
				if (_outputType == OutputType.VideoFile)
				{
					LastFileSaved = _filePath;
					flag = true;
				}
				else if (_outputType == OutputType.ImageSequence)
				{
					LastFileSaved = Path.GetDirectoryName(_filePath);
				}
			}
		}
		RenderThreadEvent(NativePlugin.PluginEvent.FreeResources);
		_fileInfo = null;
		if ((bool)_audioCapture)
		{
			_audioCapture.enabled = false;
		}
		if ((bool)_motionBlur)
		{
			_motionBlur.enabled = false;
		}
		Time.captureFramerate = 0;
		if (_oldFixedDeltaTime > 0f)
		{
			Time.fixedDeltaTime = _oldFixedDeltaTime;
		}
		_oldFixedDeltaTime = 0f;
		if (_oldVSyncCount > 0)
		{
			QualitySettings.vSyncCount = _oldVSyncCount;
			_oldVSyncCount = 0;
		}
		_motionBlur = null;
		if (_texture != null)
		{
			UnityEngine.Object.Destroy(_texture);
			_texture = null;
		}
		if (flag)
		{
			ApplyPostOperations(_filePath);
		}
	}

	protected void ApplyPostOperations(string path)
	{
		if (!_postCaptureSettings.writeFastStartStreamingForMp4 || !HasExtension(path, ".mp4"))
		{
			return;
		}
		try
		{
			if (MP4FileProcessing.ApplyFastStart(path, keepBackup: false))
			{
				LogUtils.Log("[AVProMovieCapture] moved atom 'moov' to start of file for fast streaming");
			}
		}
		catch (Exception exception)
		{
			LogUtils.LogException(exception);
		}
	}

	private void ToggleCapture()
	{
		if (_capturing)
		{
			StopCapture();
		}
		else
		{
			StartCapture();
		}
	}

	private bool IsEnoughDiskSpace()
	{
		bool result = true;
		long num = GetCaptureFileSize() / 1048576;
		if (_freeDiskSpaceMB - num < _minimumDiskSpaceMB)
		{
			result = false;
		}
		return result;
	}

	private void LateUpdate()
	{
		if (_handle >= 0 && !_paused)
		{
			CheckFreeDiskSpace();
		}
		UpdateFrame();
	}

	private void CheckFreeDiskSpace()
	{
		if (_minimumDiskSpaceMB > 0 && !IsEnoughDiskSpace())
		{
			LogUtils.LogWarning("[AVProMovieCapture] Free disk space getting too low.  Stopping capture.");
			StopCapture(skipPendingFrames: true);
		}
	}

	protected bool IsProgressComplete()
	{
		bool result = false;
		if (_stopMode != StopMode.None)
		{
			switch (_stopMode)
			{
			case StopMode.FramesEncoded:
				result = _numEncodedFrames >= _stopFrames;
				break;
			case StopMode.SecondsEncoded:
				result = (float)_totalEncodedSeconds >= _stopSeconds;
				break;
			case StopMode.SecondsElapsed:
				if (!_startPaused && !_paused)
				{
					result = Time.realtimeSinceStartup - _captureStartTime >= _stopSeconds;
				}
				break;
			}
		}
		return result;
	}

	public float GetProgress()
	{
		float result = 0f;
		if (_stopMode != StopMode.None)
		{
			switch (_stopMode)
			{
			case StopMode.FramesEncoded:
				result = (float)_numEncodedFrames / (float)_stopFrames;
				break;
			case StopMode.SecondsEncoded:
				result = (float)_numEncodedFrames / (float)_frameRate / _stopSeconds;
				break;
			case StopMode.SecondsElapsed:
				if (!_startPaused && !_paused)
				{
					result = (Time.realtimeSinceStartup - _captureStartTime) / _stopSeconds;
				}
				break;
			}
		}
		return result;
	}

	protected float GetSecondsPerCaptureFrame()
	{
		float num = _timelapseScale;
		if (!_isRealTime)
		{
			num = 1f;
		}
		float num2 = (float)_frameRate / num;
		return 1f / num2;
	}

	protected bool CanOutputFrame()
	{
		bool result = false;
		if (_handle >= 0)
		{
			if (_isRealTime)
			{
				if (NativePlugin.IsNewFrameDue(_handle))
				{
					result = _timeSinceLastFrame >= GetSecondsPerCaptureFrame();
				}
			}
			else
			{
				int num = 0;
				if (_outputType != OutputType.NamedPipe)
				{
					while (_handle >= 0 && !NativePlugin.IsNewFrameDue(_handle) && num < 100)
					{
						Thread.Sleep(1);
						num++;
					}
				}
				result = _handle >= 0 && num < 100;
			}
		}
		return result;
	}

	protected void TickFrameTimer()
	{
		_timeSinceLastFrame += Time.unscaledDeltaTime;
	}

	protected void RenormTimer()
	{
		float secondsPerCaptureFrame = GetSecondsPerCaptureFrame();
		if (_timeSinceLastFrame >= secondsPerCaptureFrame)
		{
			_timeSinceLastFrame -= secondsPerCaptureFrame;
		}
	}

	public virtual Texture GetPreviewTexture()
	{
		return null;
	}

	public virtual void UpdateFrame()
	{
		if (Input.GetKeyDown(_captureKey))
		{
			ToggleCapture();
		}
		if (_handle >= 0 && !_paused)
		{
			_numDroppedFrames = NativePlugin.GetNumDroppedFrames(_handle);
			_numDroppedEncoderFrames = NativePlugin.GetNumDroppedEncoderFrames(_handle);
			_numEncodedFrames = NativePlugin.GetNumEncodedFrames(_handle);
			_totalEncodedSeconds = NativePlugin.GetEncodedSeconds(_handle);
			if (IsProgressComplete())
			{
				_queuedStopCapture = true;
			}
		}
		if (_queuedStopCapture)
		{
			_queuedStopCapture = false;
			_queuedStartCapture = false;
			StopCapture();
		}
		if (_queuedStartCapture)
		{
			_queuedStartCapture = false;
			StartCapture();
		}
	}

	protected void ResetFPS()
	{
		_frameCount = 0;
		_frameTotal = 0;
		_fps = 0f;
		_startFrameTime = 0f;
	}

	public void UpdateFPS()
	{
		_frameCount++;
		_frameTotal++;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = realtimeSinceStartup - _startFrameTime;
		if (num >= 1f)
		{
			_fps = (float)_frameCount / num;
			_frameCount = 0;
			_startFrameTime = realtimeSinceStartup;
		}
	}

	protected int GetCameraAntiAliasingLevel(Camera camera)
	{
		int num = QualitySettings.antiAliasing;
		if (num == 0)
		{
			num = 1;
		}
		if (_renderAntiAliasing > 0)
		{
			num = _renderAntiAliasing;
		}
		if (num != 1 && num != 2 && num != 4 && num != 8)
		{
			LogUtils.LogError("[AVProMovieCapture] Invalid antialiasing value, must be 1, 2, 4 or 8.  Defaulting to 1. >> " + num);
			num = 1;
		}
		if (num != 1 && (camera.actualRenderingPath == RenderingPath.DeferredLighting || camera.actualRenderingPath == RenderingPath.DeferredShading))
		{
			LogUtils.LogWarning("[AVProMovieCapture] Not using antialiasing because MSAA is not supported by camera render path " + camera.actualRenderingPath);
			num = 1;
		}
		return num;
	}

	private void ConfigureCodec()
	{
		NativePlugin.Init();
		SelectCodec(listCodecs: false);
		if (_codecIndex >= 0)
		{
			NativePlugin.ConfigureVideoCodec(_codecIndex);
		}
	}

	public long GetCaptureFileSize()
	{
		long result = 0L;
		if (_handle >= 0 && _outputType == OutputType.VideoFile)
		{
			if (_fileInfo == null && File.Exists(_filePath))
			{
				_fileInfo = new FileInfo(_filePath);
			}
			if (_fileInfo != null)
			{
				_fileInfo.Refresh();
				result = _fileInfo.Length;
			}
		}
		return result;
	}

	public static void GetResolution(Resolution res, ref int width, ref int height)
	{
		switch (res)
		{
		case Resolution.POW2_8192x8192:
			width = 8192;
			height = 8192;
			break;
		case Resolution.POW2_8192x4096:
			width = 8192;
			height = 4096;
			break;
		case Resolution.POW2_4096x4096:
			width = 4096;
			height = 4096;
			break;
		case Resolution.POW2_4096x2048:
			width = 4096;
			height = 2048;
			break;
		case Resolution.POW2_2048x4096:
			width = 2048;
			height = 4096;
			break;
		case Resolution.UHD_3840x2160:
			width = 3840;
			height = 2160;
			break;
		case Resolution.UHD_3840x2048:
			width = 3840;
			height = 2048;
			break;
		case Resolution.UHD_3840x1920:
			width = 3840;
			height = 1920;
			break;
		case Resolution.POW2_2048x2048:
			width = 2048;
			height = 2048;
			break;
		case Resolution.POW2_2048x1024:
			width = 2048;
			height = 1024;
			break;
		case Resolution.HD_1920x1080:
			width = 1920;
			height = 1080;
			break;
		case Resolution.HD_1280x720:
			width = 1280;
			height = 720;
			break;
		case Resolution.SD_1024x768:
			width = 1024;
			height = 768;
			break;
		case Resolution.SD_800x600:
			width = 800;
			height = 600;
			break;
		case Resolution.SD_800x450:
			width = 800;
			height = 450;
			break;
		case Resolution.SD_640x480:
			width = 640;
			height = 480;
			break;
		case Resolution.SD_640x360:
			width = 640;
			height = 360;
			break;
		case Resolution.SD_320x240:
			width = 320;
			height = 240;
			break;
		}
	}

	protected static int NextMultipleOf4(int value)
	{
		return (value + 3) & -4;
	}
}
