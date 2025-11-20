#define ENABLE_LOGS
using System;
using SM.Utils;
using UnityEngine;
using UnityEngine.Profiling;

namespace RenderHeads.Media.AVProMovieCapture;

[AddComponentMenu("AVPro Movie Capture/Capture From Camera 360 Stereo ODS (VR)", 101)]
public class CaptureFromCamera360ODS : CaptureBase
{
	[Serializable]
	public class Settings
	{
		[SerializeField]
		public Camera camera;

		[SerializeField]
		[Tooltip("Render 180 degree equirectangular instead of 360 degrees.  Also faster rendering")]
		public bool render180Degrees;

		[SerializeField]
		[Tooltip("Makes assumption that 1 Unity unit is 1m")]
		public float ipd = 0.064f;

		[SerializeField]
		[Tooltip("Higher value meant less slices to render, but can affect quality.")]
		public int pixelSliceSize = 1;

		[SerializeField]
		[Range(1f, 31f)]
		[Tooltip("May need to be increased to work with some post image effects. Value is in pixels.")]
		public int paddingSize = 1;

		[SerializeField]
		public CameraClearFlags cameraClearMode = CameraClearFlags.Color;

		[SerializeField]
		public Color cameraClearColor = Color.black;

		[SerializeField]
		public Behaviour[] cameraImageEffects;
	}

	[SerializeField]
	private Settings _settings = new Settings();

	private int _eyeWidth = 1920;

	private int _eyeHeight = 1080;

	private Transform _cameraGroup;

	private Camera _leftCameraTop;

	private Camera _leftCameraBot;

	private Camera _rightCameraTop;

	private Camera _rightCameraBot;

	private RenderTexture _final;

	private IntPtr _targetNativePointer = IntPtr.Zero;

	private Material _finalMaterial;

	private int _propSliceCenter;

	public Settings Setup => _settings;

	public CaptureFromCamera360ODS()
	{
		_isRealTime = false;
		_renderResolution = Resolution.POW2_4096x4096;
	}

	public void SetCamera(Camera camera)
	{
		_settings.camera = camera;
	}

	public override void Start()
	{
		Shader shader = Shader.Find("Hidden/AVProMovieCapture/ODSMerge");
		if (shader != null)
		{
			_finalMaterial = new Material(shader);
		}
		else
		{
			LogUtils.LogError("[AVProMovieCapture] Can't find Hidden/AVProMovieCapture/ODSMerge shader");
		}
		_propSliceCenter = Shader.PropertyToID("_sliceCenter");
		base.Start();
	}

	private Camera CreateEye(Camera camera, string gameObjectName, float yRot, float xOffset, int cameraTargetHeight, int cullingMask, float fov, float aspect, int aalevel)
	{
		bool flag = false;
		if (camera == null)
		{
			GameObject obj = new GameObject(gameObjectName);
			obj.transform.parent = _cameraGroup;
			obj.transform.rotation = Quaternion.AngleAxis(0f - yRot, Vector3.right);
			obj.transform.localPosition = new Vector3(xOffset, 0f, 0f);
			obj.hideFlags = base.gameObject.hideFlags;
			camera = obj.AddComponent<Camera>();
			flag = true;
		}
		camera.fieldOfView = fov;
		camera.aspect = aspect;
		camera.clearFlags = _settings.cameraClearMode;
		camera.backgroundColor = _settings.cameraClearColor;
		camera.cullingMask = cullingMask;
		camera.useOcclusionCulling = _settings.camera.useOcclusionCulling;
		camera.renderingPath = _settings.camera.renderingPath;
		camera.allowHDR = _settings.camera.allowHDR;
		camera.allowMSAA = _settings.camera.allowMSAA;
		if (camera.renderingPath == RenderingPath.DeferredShading)
		{
			camera.allowMSAA = false;
		}
		int num = _settings.pixelSliceSize + 2 * _settings.paddingSize;
		if (camera.targetTexture != null)
		{
			camera.targetTexture.DiscardContents();
			if (camera.targetTexture.width != num || camera.targetTexture.height != cameraTargetHeight || camera.targetTexture.antiAliasing != aalevel)
			{
				RenderTexture.ReleaseTemporary(camera.targetTexture);
				camera.targetTexture = null;
			}
		}
		if (camera.targetTexture == null)
		{
			camera.targetTexture = RenderTexture.GetTemporary(num, cameraTargetHeight, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, aalevel);
		}
		camera.enabled = false;
		if (flag && _settings.cameraImageEffects != null)
		{
			for (int i = 0; i < _settings.cameraImageEffects.Length; i++)
			{
				Behaviour behaviour = _settings.cameraImageEffects[i];
				if (behaviour != null)
				{
					if (!behaviour.enabled)
					{
					}
				}
				else
				{
					LogUtils.LogWarning("[AVProMovieCapture] Image effect is null");
				}
			}
		}
		return camera;
	}

	public override void UpdateFrame()
	{
		TickFrameTimer();
		AccumulateMotionBlur();
		if (_capturing && !_paused && _settings.camera != null && _handle >= 0)
		{
			bool flag = true;
			if (IsUsingMotionBlur())
			{
				flag = _motionBlur.IsFrameAccumulated;
			}
			if (flag && CanOutputFrame())
			{
				RenderTexture renderTexture = null;
				if (!IsUsingMotionBlur())
				{
					RenderFrame();
					renderTexture = _final;
				}
				else
				{
					renderTexture = _motionBlur.FinalTexture;
				}
				if (_targetNativePointer == IntPtr.Zero || _supportTextureRecreate)
				{
					_targetNativePointer = renderTexture.GetNativeTexturePtr();
				}
				NativePlugin.SetTexturePointer(_handle, _targetNativePointer);
				RenderThreadEvent(NativePlugin.PluginEvent.CaptureFrameBuffer);
				GL.InvalidateState();
				UpdateFPS();
			}
		}
		base.UpdateFrame();
		RenormTimer();
	}

	private void AccumulateMotionBlur()
	{
		if (_motionBlur != null && _capturing && !_paused && _settings.camera != null && _handle >= 0)
		{
			RenderFrame();
			_motionBlur.Accumulate(_final);
		}
	}

	private void RenderFrame()
	{
		_cameraGroup.position = _settings.camera.transform.position;
		Quaternion quaternion = _settings.camera.transform.rotation * Quaternion.AngleAxis(180f, Vector3.up);
		float num = 360f / (float)_eyeWidth;
		int num2 = 0;
		int num3 = _eyeWidth / _settings.pixelSliceSize;
		int num4 = num3;
		if (_settings.render180Degrees)
		{
			num2 = num4 / 4;
			num4 -= num2;
			num2 = Mathf.Max(0, num2 - 2);
			num4 = Mathf.Min(num3, num4 + 2);
		}
		_final.DiscardContents();
		for (int i = num2; i < num4; i++)
		{
			int num5 = i * _settings.pixelSliceSize;
			float angle = (float)num5 * num;
			_cameraGroup.rotation = quaternion * Quaternion.AngleAxis(angle, Vector3.up);
			_leftCameraTop.targetTexture.DiscardContents();
			_leftCameraBot.targetTexture.DiscardContents();
			_rightCameraTop.targetTexture.DiscardContents();
			_rightCameraBot.targetTexture.DiscardContents();
			_leftCameraTop.Render();
			_leftCameraBot.Render();
			_rightCameraTop.Render();
			_rightCameraBot.Render();
			_finalMaterial.SetFloat(_propSliceCenter, (float)num5 + (float)_settings.pixelSliceSize / 2f);
			Graphics.Blit(null, _final, _finalMaterial);
		}
	}

	public override Texture GetPreviewTexture()
	{
		if (IsUsingMotionBlur())
		{
			return _motionBlur.FinalTexture;
		}
		return _final;
	}

	public override bool PrepareCapture()
	{
		if (_capturing)
		{
			return false;
		}
		if (SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL") && !SystemInfo.graphicsDeviceVersion.Contains("emulated"))
		{
			LogUtils.LogError("[AVProMovieCapture] OpenGL not yet supported for CaptureFromCamera360ODS component, please use Direct3D11 instead. You may need to switch your build platform to Windows.");
			return false;
		}
		if (Profiler.enabled)
		{
			LogUtils.LogWarning("[AVProMovieCapture] Having the Unity profiler enabled while using the CaptureFromCamera360ODS component is not recommended. Too many samples are generated which can make the system run out of memory. Disable the profiler, close the window and remove the tab. A Unity restart may be required after disabling the profiler recording");
		}
		_pixelFormat = NativePlugin.PixelFormat.RGBA32;
		_isTopDown = true;
		if (_settings.camera == null)
		{
			_settings.camera = GetComponent<Camera>();
		}
		if (_settings.camera == null)
		{
			LogUtils.LogError("[AVProMovieCapture] No camera assigned to CaptureFromCamera360ODS");
			return false;
		}
		int width = Mathf.FloorToInt(_settings.camera.pixelRect.width);
		int height = Mathf.FloorToInt(_settings.camera.pixelRect.height);
		if (_renderResolution == Resolution.Custom)
		{
			width = (int)_renderSize.x;
			height = (int)_renderSize.y;
		}
		else if (_renderResolution != Resolution.Original)
		{
			CaptureBase.GetResolution(_renderResolution, ref width, ref height);
		}
		_eyeWidth = Mathf.Clamp(width, 1, 8192);
		_eyeHeight = Mathf.Clamp(height / 2, 1, 4096);
		_eyeHeight -= _eyeHeight & 1;
		width = _eyeWidth;
		height = _eyeHeight * 2;
		int cameraAntiAliasingLevel = GetCameraAntiAliasingLevel(_settings.camera);
		_settings.pixelSliceSize = Mathf.Clamp(_settings.pixelSliceSize, 1, _eyeWidth);
		_settings.pixelSliceSize -= _eyeWidth % _settings.pixelSliceSize;
		_settings.paddingSize = Mathf.Clamp(_settings.paddingSize, 0, 31);
		float num = _settings.ipd / 2f;
		float aspect = (float)_settings.pixelSliceSize * 2f / (float)_eyeHeight;
		if (_cameraGroup == null)
		{
			GameObject gameObject = new GameObject("OdsCameraGroup");
			gameObject.transform.parent = base.gameObject.transform;
			gameObject.hideFlags = base.gameObject.hideFlags;
			_cameraGroup = gameObject.transform;
		}
		_leftCameraTop = CreateEye(_leftCameraTop, "LeftEyeTop", 45f, 0f - num, _eyeHeight / 2, _settings.camera.cullingMask, 90f, aspect, cameraAntiAliasingLevel);
		_leftCameraBot = CreateEye(_leftCameraBot, "LeftEyeBot", -45f, 0f - num, _eyeHeight / 2, _settings.camera.cullingMask, 90f, aspect, cameraAntiAliasingLevel);
		_rightCameraTop = CreateEye(_rightCameraTop, "RightEyeTop", 45f, num, _eyeHeight / 2, _settings.camera.cullingMask, 90f, aspect, cameraAntiAliasingLevel);
		_rightCameraBot = CreateEye(_rightCameraBot, "RightEyeBot", -45f, num, _eyeHeight / 2, _settings.camera.cullingMask, 90f, aspect, cameraAntiAliasingLevel);
		_targetNativePointer = IntPtr.Zero;
		if (_final != null)
		{
			_final.DiscardContents();
			if (_final.width != width || _final.height != height || _final.antiAliasing != cameraAntiAliasingLevel)
			{
				RenderTexture.ReleaseTemporary(_final);
				_final = null;
			}
			_final = null;
		}
		if (_final == null)
		{
			_final = RenderTexture.GetTemporary(width, height, 0);
		}
		_finalMaterial.SetTexture("_leftTopTex", _leftCameraTop.targetTexture);
		_finalMaterial.SetTexture("_leftBotTex", _leftCameraBot.targetTexture);
		_finalMaterial.SetTexture("_rightTopTex", _rightCameraTop.targetTexture);
		_finalMaterial.SetTexture("_rightBotTex", _rightCameraBot.targetTexture);
		_finalMaterial.SetFloat("_pixelSliceSize", _settings.pixelSliceSize);
		_finalMaterial.SetInt("_paddingSize", _settings.paddingSize);
		_finalMaterial.SetFloat("_targetXTexelSize", 1f / (float)width);
		if (_settings.render180Degrees)
		{
			_finalMaterial.DisableKeyword("LAYOUT_EQUIRECT360");
			_finalMaterial.EnableKeyword("LAYOUT_EQUIRECT180");
		}
		else
		{
			_finalMaterial.DisableKeyword("LAYOUT_EQUIRECT180");
			_finalMaterial.EnableKeyword("LAYOUT_EQUIRECT360");
		}
		SelectRecordingResolution(width, height);
		GenerateFilename();
		return base.PrepareCapture();
	}

	private static void DestroyEye(Camera camera)
	{
		if (camera != null)
		{
			RenderTexture.ReleaseTemporary(camera.targetTexture);
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(camera.gameObject);
			}
		}
	}

	public override void OnDestroy()
	{
		_targetNativePointer = IntPtr.Zero;
		if (_final != null)
		{
			RenderTexture.ReleaseTemporary(_final);
			_final = null;
		}
		DestroyEye(_leftCameraTop);
		DestroyEye(_leftCameraBot);
		DestroyEye(_rightCameraTop);
		DestroyEye(_rightCameraBot);
		_leftCameraTop = null;
		_leftCameraBot = null;
		_rightCameraTop = null;
		_rightCameraBot = null;
		if (_cameraGroup != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(_cameraGroup.gameObject);
			}
			_cameraGroup = null;
		}
		if ((bool)_finalMaterial)
		{
			UnityEngine.Object.Destroy(_finalMaterial);
			_finalMaterial = null;
		}
		base.OnDestroy();
	}
}
