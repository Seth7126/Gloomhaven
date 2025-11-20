#define ENABLE_LOGS
using System;
using SM.Utils;
using UnityEngine;

namespace RenderHeads.Media.AVProMovieCapture;

[AddComponentMenu("AVPro Movie Capture/Capture From Camera", 1)]
public class CaptureFromCamera : CaptureBase
{
	[SerializeField]
	private Camera _lastCamera;

	[SerializeField]
	private Camera[] _contribCameras;

	[SerializeField]
	private bool _useContributingCameras = true;

	private RenderTexture _target;

	private IntPtr _targetNativePointer = IntPtr.Zero;

	public bool UseContributingCameras
	{
		get
		{
			return _useContributingCameras;
		}
		set
		{
			_useContributingCameras = value;
		}
	}

	public void SetCamera(Camera mainCamera, bool useContributingCameras = true)
	{
		_lastCamera = mainCamera;
		_contribCameras = null;
		_useContributingCameras = useContributingCameras;
		if (_useContributingCameras && _lastCamera != null && Utils.HasContributingCameras(_lastCamera))
		{
			_contribCameras = Utils.FindContributingCameras(mainCamera);
		}
	}

	public void SetCamera(Camera mainCamera, Camera[] contributingCameras)
	{
		_lastCamera = mainCamera;
		_contribCameras = contributingCameras;
	}

	private bool HasCamera()
	{
		return _lastCamera != null;
	}

	private bool HasContributingCameras()
	{
		if (_useContributingCameras && _contribCameras != null)
		{
			return _contribCameras.Length != 0;
		}
		return false;
	}

	public override void UpdateFrame()
	{
		TickFrameTimer();
		if (_capturing && !_paused && HasCamera())
		{
			bool flag = true;
			if (IsUsingMotionBlur())
			{
				flag = _motionBlur.IsFrameAccumulated;
			}
			if (flag && _handle >= 0 && CanOutputFrame())
			{
				if (!IsUsingMotionBlur())
				{
					UpdateTexture();
				}
				else
				{
					_target.DiscardContents();
					Graphics.Blit(_motionBlur.FinalTexture, _target);
				}
				if (_supportTextureRecreate)
				{
					_targetNativePointer = _target.GetNativeTexturePtr();
				}
				NativePlugin.SetTexturePointer(_handle, _targetNativePointer);
				RenderThreadEvent(NativePlugin.PluginEvent.CaptureFrameBuffer);
				if (IsRecordingUnityAudio())
				{
					int length = 0;
					IntPtr data = _audioCapture.ReadData(out length);
					if (length > 0)
					{
						NativePlugin.EncodeAudio(_handle, data, (uint)length);
					}
				}
				UpdateFPS();
			}
		}
		base.UpdateFrame();
		RenormTimer();
	}

	private void UpdateTexture()
	{
		if (!HasContributingCameras())
		{
			RenderTexture targetTexture = _lastCamera.targetTexture;
			Rect rect = _lastCamera.rect;
			CameraClearFlags clearFlags = _lastCamera.clearFlags;
			Color backgroundColor = _lastCamera.backgroundColor;
			bool flag = false;
			if (_lastCamera.clearFlags == CameraClearFlags.Nothing || _lastCamera.clearFlags == CameraClearFlags.Depth)
			{
				flag = true;
				_lastCamera.clearFlags = CameraClearFlags.Color;
				if (!_supportAlpha)
				{
					_lastCamera.backgroundColor = Color.black;
				}
				else
				{
					_lastCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
				}
			}
			_lastCamera.rect = new Rect(0f, 0f, 1f, 1f);
			_lastCamera.targetTexture = _target;
			_lastCamera.Render();
			_lastCamera.rect = rect;
			if (flag)
			{
				_lastCamera.clearFlags = clearFlags;
				_lastCamera.backgroundColor = backgroundColor;
			}
			_lastCamera.targetTexture = targetTexture;
			return;
		}
		for (int i = 0; i < _contribCameras.Length; i++)
		{
			Camera camera = _contribCameras[i];
			if (camera != null && camera.isActiveAndEnabled)
			{
				RenderTexture targetTexture2 = camera.targetTexture;
				camera.targetTexture = _target;
				camera.Render();
				camera.targetTexture = targetTexture2;
			}
		}
		if (_lastCamera != null)
		{
			RenderTexture targetTexture3 = _lastCamera.targetTexture;
			_lastCamera.targetTexture = _target;
			_lastCamera.Render();
			_lastCamera.targetTexture = targetTexture3;
		}
	}

	public override void UnprepareCapture()
	{
		NativePlugin.SetTexturePointer(_handle, IntPtr.Zero);
		if (_target != null)
		{
			_target.DiscardContents();
		}
		base.UnprepareCapture();
	}

	public override Texture GetPreviewTexture()
	{
		return _target;
	}

	public override bool PrepareCapture()
	{
		if (_capturing)
		{
			return false;
		}
		if (SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL") && !SystemInfo.graphicsDeviceVersion.Contains("emulated"))
		{
			LogUtils.LogError("[AVProMovieCapture] OpenGL not yet supported for CaptureFromCamera component, please use Direct3D11 instead. You may need to switch your build platform to Windows.");
			return false;
		}
		_pixelFormat = NativePlugin.PixelFormat.RGBA32;
		_isTopDown = true;
		if (!HasCamera())
		{
			_lastCamera = GetComponent<Camera>();
			if (!HasCamera())
			{
				_lastCamera = Camera.main;
			}
			if (!HasCamera())
			{
				LogUtils.LogError("[AVProMovieCapture] No camera assigned to CaptureFromCamera");
				return false;
			}
		}
		if (!HasContributingCameras() && (_lastCamera.clearFlags == CameraClearFlags.Depth || _lastCamera.clearFlags == CameraClearFlags.Nothing))
		{
			LogUtils.LogWarning("[AVProMovieCapture] This camera doesn't clear, consider setting contributing cameras");
		}
		int width = Mathf.FloorToInt(_lastCamera.pixelRect.width);
		int height = Mathf.FloorToInt(_lastCamera.pixelRect.height);
		if (_renderResolution == Resolution.Custom)
		{
			width = (int)_renderSize.x;
			height = (int)_renderSize.y;
		}
		else if (_renderResolution != Resolution.Original)
		{
			CaptureBase.GetResolution(_renderResolution, ref width, ref height);
		}
		int cameraAntiAliasingLevel = GetCameraAntiAliasingLevel(_lastCamera);
		if (_target != null)
		{
			_target.DiscardContents();
			if (_target.width != width || _target.height != height || _target.antiAliasing != cameraAntiAliasingLevel)
			{
				_targetNativePointer = IntPtr.Zero;
				RenderTexture.ReleaseTemporary(_target);
				_target = null;
			}
		}
		if (_target == null)
		{
			_target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, cameraAntiAliasingLevel);
			_target.name = "[AVProMovieCapture] Camera Target";
			_target.Create();
			_targetNativePointer = _target.GetNativeTexturePtr();
		}
		if (_useMotionBlur)
		{
			_motionBlurCameras = new Camera[1];
			_motionBlurCameras[0] = _lastCamera;
		}
		SelectRecordingResolution(width, height);
		GenerateFilename();
		return base.PrepareCapture();
	}

	public override void OnDestroy()
	{
		if (_target != null)
		{
			_targetNativePointer = IntPtr.Zero;
			RenderTexture.ReleaseTemporary(_target);
			_target = null;
		}
		base.OnDestroy();
	}
}
