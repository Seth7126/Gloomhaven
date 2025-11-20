#define ENABLE_LOGS
using System;
using SM.Utils;
using UnityEngine;

namespace RenderHeads.Media.AVProMovieCapture;

[AddComponentMenu("AVPro Movie Capture/Capture From Texture", 3)]
public class CaptureFromTexture : CaptureBase
{
	[Tooltip("If enabled the method the encoder will only process frames each time UpdateSourceTexture() is called. This is useful if the texture is updating at a different rate compared to Unity, eg for webcam capture.")]
	[SerializeField]
	private bool _manualUpdate;

	private Texture _sourceTexture;

	private RenderTexture _renderTexture;

	private IntPtr _targetNativePointer = IntPtr.Zero;

	public bool _isSourceTextureChanged;

	public void SetSourceTexture(Texture texture)
	{
		_sourceTexture = texture;
		if (texture is Texture2D)
		{
			if (((Texture2D)texture).format != TextureFormat.ARGB32 && ((Texture2D)texture).format != TextureFormat.RGBA32 && ((Texture2D)texture).format != TextureFormat.BGRA32)
			{
				LogUtils.LogWarning("[AVProMovieCapture] texture format may not be supported: " + ((Texture2D)texture).format);
			}
		}
		else if (texture is RenderTexture && ((RenderTexture)texture).format != RenderTextureFormat.ARGB32 && ((RenderTexture)texture).format != RenderTextureFormat.Default && ((RenderTexture)texture).format != RenderTextureFormat.BGRA32)
		{
			LogUtils.LogWarning("[AVProMovieCapture] texture format may not be supported: " + ((RenderTexture)texture).format);
		}
	}

	public void UpdateSourceTexture()
	{
		_isSourceTextureChanged = true;
	}

	private bool ShouldCaptureFrame()
	{
		if (_capturing && !_paused)
		{
			return _sourceTexture != null;
		}
		return false;
	}

	private bool HasSourceTextureChanged()
	{
		if (_manualUpdate)
		{
			if (_manualUpdate)
			{
				return _isSourceTextureChanged;
			}
			return false;
		}
		return true;
	}

	public override void UpdateFrame()
	{
		TickFrameTimer();
		AccumulateMotionBlur();
		if (ShouldCaptureFrame())
		{
			bool flag = HasSourceTextureChanged();
			if (IsUsingMotionBlur())
			{
				flag = _motionBlur.IsFrameAccumulated;
			}
			_isSourceTextureChanged = false;
			if (flag && (_manualUpdate || CanOutputFrame()))
			{
				Texture texture = _sourceTexture;
				if (IsUsingMotionBlur())
				{
					texture = _motionBlur.FinalTexture;
				}
				if (!(texture is RenderTexture))
				{
					_renderTexture.DiscardContents();
					Graphics.Blit(texture, _renderTexture);
					texture = _renderTexture;
				}
				if (_targetNativePointer == IntPtr.Zero || _supportTextureRecreate)
				{
					_targetNativePointer = texture.GetNativeTexturePtr();
				}
				NativePlugin.SetTexturePointer(_handle, _targetNativePointer);
				RenderThreadEvent(NativePlugin.PluginEvent.CaptureFrameBuffer);
				if (!IsUsingMotionBlur())
				{
					_isSourceTextureChanged = false;
				}
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

	private void AccumulateMotionBlur()
	{
		if (_motionBlur != null && ShouldCaptureFrame() && HasSourceTextureChanged())
		{
			_motionBlur.Accumulate(_sourceTexture);
			_isSourceTextureChanged = false;
		}
	}

	public override Texture GetPreviewTexture()
	{
		if (IsUsingMotionBlur())
		{
			return _motionBlur.FinalTexture;
		}
		if (_sourceTexture is RenderTexture)
		{
			return _sourceTexture;
		}
		return _renderTexture;
	}

	public override bool PrepareCapture()
	{
		if (_capturing)
		{
			return false;
		}
		if (_sourceTexture == null)
		{
			LogUtils.LogError("[AVProMovieCapture] No texture set to capture");
			return false;
		}
		if (SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL") && !SystemInfo.graphicsDeviceVersion.Contains("emulated"))
		{
			LogUtils.LogError("[AVProMovieCapture] OpenGL not yet supported for CaptureFromTexture component, please use Direct3D11 instead. You may need to switch your build platform to Windows.");
			return false;
		}
		_pixelFormat = NativePlugin.PixelFormat.RGBA32;
		_isSourceTextureChanged = false;
		SelectRecordingResolution(_sourceTexture.width, _sourceTexture.height);
		_renderTexture = RenderTexture.GetTemporary(_targetWidth, _targetHeight, 0, RenderTextureFormat.ARGB32);
		_renderTexture.Create();
		GenerateFilename();
		return base.PrepareCapture();
	}

	public override void UnprepareCapture()
	{
		_targetNativePointer = IntPtr.Zero;
		NativePlugin.SetTexturePointer(_handle, IntPtr.Zero);
		if (_renderTexture != null)
		{
			RenderTexture.ReleaseTemporary(_renderTexture);
			_renderTexture = null;
		}
		base.UnprepareCapture();
	}
}
