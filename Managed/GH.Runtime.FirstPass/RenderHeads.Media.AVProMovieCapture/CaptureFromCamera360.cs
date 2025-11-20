#define ENABLE_LOGS
using System;
using SM.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderHeads.Media.AVProMovieCapture;

[AddComponentMenu("AVPro Movie Capture/Capture From Camera 360 (VR)", 100)]
public class CaptureFromCamera360 : CaptureBase
{
	private enum CubemapRenderMethod
	{
		Manual,
		Unity,
		Unity2018
	}

	[SerializeField]
	public int _cubemapResolution = 2048;

	[SerializeField]
	public int _cubemapDepth = 24;

	[SerializeField]
	public bool _supportGUI;

	[SerializeField]
	public bool _supportCameraRotation;

	[SerializeField]
	[Tooltip("Render 180 degree equirectangular instead of 360 degrees")]
	public bool _render180Degrees;

	[SerializeField]
	public StereoPacking _stereoRendering;

	[SerializeField]
	[Tooltip("Makes assumption that 1 Unity unit is 1m")]
	public float _ipd = 0.064f;

	[SerializeField]
	private Camera _camera;

	private RenderTexture _faceTarget;

	private Material _blitMaterial;

	private Material _cubemapToEquirectangularMaterial;

	private RenderTexture _cubeTarget;

	private RenderTexture _finalTarget;

	private IntPtr _targetNativePointer = IntPtr.Zero;

	private int _propFlipX;

	public CaptureFromCamera360()
	{
		_renderResolution = Resolution.POW2_2048x2048;
	}

	private CubemapRenderMethod GetCubemapRenderingMethod()
	{
		if (_supportGUI)
		{
			return CubemapRenderMethod.Manual;
		}
		if (_supportCameraRotation)
		{
			if (_stereoRendering != StereoPacking.None)
			{
				return CubemapRenderMethod.Unity2018;
			}
			return CubemapRenderMethod.Manual;
		}
		if (_stereoRendering == StereoPacking.None)
		{
			return CubemapRenderMethod.Unity;
		}
		return CubemapRenderMethod.Unity2018;
	}

	public void SetCamera(Camera camera)
	{
		_camera = camera;
	}

	public override void UpdateFrame()
	{
		TickFrameTimer();
		AccumulateMotionBlur();
		if (_capturing && !_paused && _cubeTarget != null && _camera != null)
		{
			bool flag = true;
			if (IsUsingMotionBlur())
			{
				flag = _motionBlur.IsFrameAccumulated;
			}
			if (flag && CanOutputFrame())
			{
				if (IsRecordingUnityAudio())
				{
					int length = 0;
					IntPtr data = _audioCapture.ReadData(out length);
					if (length > 0)
					{
						NativePlugin.EncodeAudio(_handle, data, (uint)length);
					}
				}
				RenderTexture renderTexture = _finalTarget;
				if (!IsUsingMotionBlur())
				{
					UpdateTexture();
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

	private static void ClearCubemap(RenderTexture texture, Color color)
	{
		bool clearColor = texture.depth != 0;
		Graphics.SetRenderTarget(texture, 0, CubemapFace.PositiveX);
		GL.Clear(clearDepth: true, clearColor, color);
		Graphics.SetRenderTarget(texture, 0, CubemapFace.PositiveY);
		GL.Clear(clearDepth: true, clearColor, color);
		Graphics.SetRenderTarget(texture, 0, CubemapFace.PositiveZ);
		GL.Clear(clearDepth: true, clearColor, color);
		Graphics.SetRenderTarget(texture, 0, CubemapFace.NegativeX);
		GL.Clear(clearDepth: true, clearColor, color);
		Graphics.SetRenderTarget(texture, 0, CubemapFace.NegativeY);
		GL.Clear(clearDepth: true, clearColor, color);
		Graphics.SetRenderTarget(texture, 0, CubemapFace.NegativeZ);
		GL.Clear(clearDepth: true, clearColor, color);
		Graphics.SetRenderTarget(null);
	}

	private void RenderCubemapToEquiRect(RenderTexture cubemap, RenderTexture target, bool supportRotation, Quaternion rotation, bool isEyeLeft)
	{
		if (_stereoRendering == StereoPacking.TopBottom)
		{
			if (isEyeLeft)
			{
				_cubemapToEquirectangularMaterial.DisableKeyword("STEREOPACK_BOTTOM");
				_cubemapToEquirectangularMaterial.EnableKeyword("STEREOPACK_TOP");
			}
			else
			{
				_cubemapToEquirectangularMaterial.DisableKeyword("STEREOPACK_TOP");
				_cubemapToEquirectangularMaterial.EnableKeyword("STEREOPACK_BOTTOM");
			}
		}
		else if (_stereoRendering == StereoPacking.LeftRight)
		{
			if (isEyeLeft)
			{
				_cubemapToEquirectangularMaterial.DisableKeyword("STEREOPACK_RIGHT");
				_cubemapToEquirectangularMaterial.EnableKeyword("STEREOPACK_LEFT");
			}
			else
			{
				_cubemapToEquirectangularMaterial.DisableKeyword("STEREOPACK_LEFT");
				_cubemapToEquirectangularMaterial.EnableKeyword("STEREOPACK_RIGHT");
			}
		}
		Graphics.Blit(cubemap, target, _cubemapToEquirectangularMaterial);
	}

	private void UpdateTexture()
	{
		Camera camera = _camera;
		_cubeTarget.DiscardContents();
		_finalTarget.DiscardContents();
		CubemapRenderMethod cubemapRenderingMethod = GetCubemapRenderingMethod();
		if (_stereoRendering == StereoPacking.None)
		{
			switch (cubemapRenderingMethod)
			{
			case CubemapRenderMethod.Unity:
				camera.RenderToCubemap(_cubeTarget, 63);
				break;
			case CubemapRenderMethod.Manual:
				RenderCameraToCubemap(camera, _cubeTarget);
				break;
			}
			RenderCubemapToEquiRect(_cubeTarget, _finalTarget, supportRotation: false, Quaternion.identity, isEyeLeft: true);
			return;
		}
		switch (cubemapRenderingMethod)
		{
		case CubemapRenderMethod.Unity2018:
			camera.stereoSeparation = _ipd;
			camera.RenderToCubemap(_cubeTarget, 63, Camera.MonoOrStereoscopicEye.Left);
			RenderCubemapToEquiRect(_cubeTarget, _finalTarget, supportRotation: false, camera.transform.rotation, isEyeLeft: true);
			_cubeTarget.DiscardContents();
			camera.RenderToCubemap(_cubeTarget, 63, Camera.MonoOrStereoscopicEye.Right);
			RenderCubemapToEquiRect(_cubeTarget, _finalTarget, supportRotation: false, camera.transform.rotation, isEyeLeft: false);
			break;
		case CubemapRenderMethod.Manual:
		{
			Vector3 localPosition = camera.transform.localPosition;
			camera.transform.Translate(new Vector3((0f - _ipd) / 2f, 0f, 0f), Space.Self);
			RenderCameraToCubemap(camera, _cubeTarget);
			RenderCubemapToEquiRect(_cubeTarget, _finalTarget, supportRotation: false, Quaternion.identity, isEyeLeft: true);
			camera.transform.localPosition = localPosition;
			camera.transform.Translate(new Vector3(_ipd / 2f, 0f, 0f), Space.Self);
			RenderCameraToCubemap(camera, _cubeTarget);
			RenderCubemapToEquiRect(_cubeTarget, _finalTarget, supportRotation: false, Quaternion.identity, isEyeLeft: false);
			camera.transform.localPosition = localPosition;
			break;
		}
		}
	}

	private void RenderCameraToCubemap(Camera camera, RenderTexture cubemapTarget)
	{
		float fieldOfView = camera.fieldOfView;
		RenderTexture targetTexture = camera.targetTexture;
		Quaternion rotation = camera.transform.rotation;
		Quaternion quaternion = camera.transform.rotation;
		if (!_supportCameraRotation)
		{
			quaternion = Quaternion.identity;
		}
		camera.targetTexture = _faceTarget;
		camera.fieldOfView = 90f;
		camera.transform.rotation = quaternion * Quaternion.LookRotation(Vector3.forward, Vector3.down);
		_faceTarget.DiscardContents();
		camera.Render();
		Graphics.SetRenderTarget(cubemapTarget, 0, CubemapFace.PositiveZ);
		Graphics.Blit(_faceTarget, _blitMaterial);
		camera.transform.rotation = quaternion * Quaternion.LookRotation(Vector3.back, Vector3.down);
		_faceTarget.DiscardContents();
		camera.Render();
		Graphics.SetRenderTarget(cubemapTarget, 0, CubemapFace.NegativeZ);
		Graphics.Blit(_faceTarget, _blitMaterial);
		camera.transform.rotation = quaternion * Quaternion.LookRotation(Vector3.right, Vector3.down);
		_faceTarget.DiscardContents();
		camera.Render();
		Graphics.SetRenderTarget(cubemapTarget, 0, CubemapFace.NegativeX);
		Graphics.Blit(_faceTarget, _blitMaterial);
		camera.transform.rotation = quaternion * Quaternion.LookRotation(Vector3.left, Vector3.down);
		_faceTarget.DiscardContents();
		camera.Render();
		Graphics.SetRenderTarget(cubemapTarget, 0, CubemapFace.PositiveX);
		Graphics.Blit(_faceTarget, _blitMaterial);
		camera.transform.rotation = quaternion * Quaternion.LookRotation(Vector3.up, Vector3.forward);
		_faceTarget.DiscardContents();
		camera.Render();
		Graphics.SetRenderTarget(cubemapTarget, 0, CubemapFace.PositiveY);
		Graphics.Blit(_faceTarget, _blitMaterial);
		camera.transform.rotation = quaternion * Quaternion.LookRotation(Vector3.down, Vector3.back);
		_faceTarget.DiscardContents();
		camera.Render();
		Graphics.SetRenderTarget(cubemapTarget, 0, CubemapFace.NegativeY);
		Graphics.Blit(_faceTarget, _blitMaterial);
		Graphics.SetRenderTarget(null);
		camera.transform.rotation = rotation;
		camera.fieldOfView = fieldOfView;
		camera.targetTexture = targetTexture;
	}

	private void AccumulateMotionBlur()
	{
		if (_motionBlur != null && _capturing && !_paused && _camera != null && _handle >= 0)
		{
			UpdateTexture();
			_motionBlur.Accumulate(_finalTarget);
		}
	}

	public override bool PrepareCapture()
	{
		if (_capturing)
		{
			return false;
		}
		if (SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL") && !SystemInfo.graphicsDeviceVersion.Contains("emulated"))
		{
			LogUtils.LogError("[AVProMovieCapture] OpenGL not yet supported for CaptureFromCamera360 component, please use Direct3D11 instead. You may need to switch your build platform to Windows.");
			return false;
		}
		_pixelFormat = NativePlugin.PixelFormat.RGBA32;
		_isTopDown = true;
		if (_camera == null)
		{
			_camera = GetComponent<Camera>();
		}
		if (_camera == null)
		{
			LogUtils.LogError("[AVProMovieCapture] No camera assigned to CaptureFromCamera360");
			return false;
		}
		int width = Mathf.FloorToInt(_camera.pixelRect.width);
		int height = Mathf.FloorToInt(_camera.pixelRect.height);
		if (_renderResolution == Resolution.Custom)
		{
			width = (int)_renderSize.x;
			height = (int)_renderSize.y;
		}
		else if (_renderResolution != Resolution.Original)
		{
			CaptureBase.GetResolution(_renderResolution, ref width, ref height);
		}
		int cameraAntiAliasingLevel = GetCameraAntiAliasingLevel(_camera);
		CubemapRenderMethod cubemapRenderingMethod = GetCubemapRenderingMethod();
		LogUtils.Log("[AVProMovieCapture] Using cubemap render method: " + cubemapRenderingMethod);
		if (!Mathf.IsPowerOfTwo(_cubemapResolution))
		{
			_cubemapResolution = Mathf.ClosestPowerOfTwo(_cubemapResolution);
			LogUtils.LogWarning("[AVProMovieCapture] Cubemap must be power-of-2 dimensions, resizing to closest = " + _cubemapResolution);
		}
		_targetNativePointer = IntPtr.Zero;
		if (_finalTarget != null)
		{
			_finalTarget.DiscardContents();
			if (_finalTarget.width != width || _finalTarget.height != height)
			{
				RenderTexture.ReleaseTemporary(_finalTarget);
				_finalTarget = null;
			}
		}
		if (_finalTarget == null)
		{
			_finalTarget = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1);
			_finalTarget.name = "[AVProMovieCapture] 360 Final Target";
		}
		if (_faceTarget != null)
		{
			_faceTarget.DiscardContents();
			if (_faceTarget.width != _cubemapResolution || _faceTarget.height != _cubemapResolution || _faceTarget.depth != _cubemapDepth || cameraAntiAliasingLevel != _faceTarget.antiAliasing)
			{
				UnityEngine.Object.Destroy(_faceTarget);
				_faceTarget = null;
			}
		}
		if (cubemapRenderingMethod == CubemapRenderMethod.Manual)
		{
			if (_faceTarget == null)
			{
				_faceTarget = new RenderTexture(_cubemapResolution, _cubemapResolution, _cubemapDepth, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
				_faceTarget.name = "[AVProMovieCapture] 360 Face Target";
				_faceTarget.isPowerOfTwo = true;
				_faceTarget.wrapMode = TextureWrapMode.Clamp;
				_faceTarget.filterMode = FilterMode.Bilinear;
				_faceTarget.autoGenerateMips = false;
				_faceTarget.antiAliasing = cameraAntiAliasingLevel;
			}
			_cubemapToEquirectangularMaterial.SetFloat(_propFlipX, 0f);
		}
		else
		{
			_cubemapToEquirectangularMaterial.SetFloat(_propFlipX, 1f);
		}
		_cubemapToEquirectangularMaterial.DisableKeyword("USE_ROTATION");
		_cubemapToEquirectangularMaterial.DisableKeyword("STEREOPACK_TOP");
		_cubemapToEquirectangularMaterial.DisableKeyword("STEREOPACK_BOTTOM");
		_cubemapToEquirectangularMaterial.DisableKeyword("STEREOPACK_LEFT");
		_cubemapToEquirectangularMaterial.DisableKeyword("STEREOPACK_RIGHT");
		if (_render180Degrees)
		{
			_cubemapToEquirectangularMaterial.DisableKeyword("LAYOUT_EQUIRECT360");
			_cubemapToEquirectangularMaterial.EnableKeyword("LAYOUT_EQUIRECT180");
		}
		else
		{
			_cubemapToEquirectangularMaterial.DisableKeyword("LAYOUT_EQUIRECT180");
			_cubemapToEquirectangularMaterial.EnableKeyword("LAYOUT_EQUIRECT360");
		}
		int num = 0;
		if (cubemapRenderingMethod != CubemapRenderMethod.Manual)
		{
			num = _cubemapDepth;
		}
		int num2 = 1;
		if (cubemapRenderingMethod != CubemapRenderMethod.Manual)
		{
			num2 = cameraAntiAliasingLevel;
		}
		if (_cubeTarget != null)
		{
			_cubeTarget.DiscardContents();
			if (_cubeTarget.width != _cubemapResolution || _cubeTarget.height != _cubemapResolution || _cubeTarget.depth != num || num2 != _cubeTarget.antiAliasing)
			{
				UnityEngine.Object.Destroy(_cubeTarget);
				_cubeTarget = null;
			}
		}
		if (_cubeTarget == null)
		{
			_cubeTarget = new RenderTexture(_cubemapResolution, _cubemapResolution, num, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
			_cubeTarget.name = "[AVProMovieCapture] 360 Cube Target";
			_cubeTarget.isPowerOfTwo = true;
			_cubeTarget.dimension = TextureDimension.Cube;
			_cubeTarget.useMipMap = false;
			_cubeTarget.autoGenerateMips = false;
			_cubeTarget.antiAliasing = num2;
			_cubeTarget.wrapMode = TextureWrapMode.Clamp;
			_cubeTarget.filterMode = FilterMode.Bilinear;
		}
		if (_useMotionBlur)
		{
			_motionBlurCameras = new Camera[1];
			_motionBlurCameras[0] = _camera;
		}
		SelectRecordingResolution(width, height);
		GenerateFilename();
		return base.PrepareCapture();
	}

	public override Texture GetPreviewTexture()
	{
		if (IsUsingMotionBlur())
		{
			return _motionBlur.FinalTexture;
		}
		return _finalTarget;
	}

	public override void Start()
	{
		Shader shader = Resources.Load<Shader>("CubemapToEquirectangular");
		if (shader != null)
		{
			_cubemapToEquirectangularMaterial = new Material(shader);
		}
		else
		{
			LogUtils.LogError("[AVProMovieCapture] Can't find CubemapToEquirectangular shader");
		}
		Shader shader2 = Shader.Find("Hidden/BlitCopy");
		if (shader2 != null)
		{
			_blitMaterial = new Material(shader2);
		}
		else
		{
			LogUtils.LogError("[AVProMovieCapture] Can't find Hidden/BlitCopy shader");
		}
		_propFlipX = Shader.PropertyToID("_FlipX");
		base.Start();
	}

	public override void OnDestroy()
	{
		_targetNativePointer = IntPtr.Zero;
		if (_blitMaterial != null)
		{
			UnityEngine.Object.Destroy(_blitMaterial);
			_blitMaterial = null;
		}
		if (_faceTarget != null)
		{
			UnityEngine.Object.Destroy(_faceTarget);
			_faceTarget = null;
		}
		if (_cubeTarget != null)
		{
			UnityEngine.Object.Destroy(_cubeTarget);
			_cubeTarget = null;
		}
		if (_finalTarget != null)
		{
			RenderTexture.ReleaseTemporary(_finalTarget);
			_finalTarget = null;
		}
		base.OnDestroy();
	}
}
