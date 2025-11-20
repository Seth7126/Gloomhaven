using System;
using System.Collections;
using SM.Utils;
using UnityEngine;

namespace RenderHeads.Media.AVProMovieCapture;

[AddComponentMenu("AVPro Movie Capture/Capture From Screen", 0)]
public class CaptureFromScreen : CaptureBase
{
	private YieldInstruction _waitForEndOfFrame;

	public override bool PrepareCapture()
	{
		if (_capturing)
		{
			return false;
		}
		if (SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL") && !SystemInfo.graphicsDeviceVersion.Contains("emulated"))
		{
			LogUtils.LogError("[AVProMovieCapture] OpenGL not yet supported for CaptureFromScreen component, please use Direct3D11 instead. You may need to switch your build platform to Windows.");
			return false;
		}
		SelectRecordingResolution(Screen.width, Screen.height);
		_pixelFormat = NativePlugin.PixelFormat.RGBA32;
		if (SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL") && !SystemInfo.graphicsDeviceVersion.Contains("emulated"))
		{
			_pixelFormat = NativePlugin.PixelFormat.BGRA32;
			_isTopDown = true;
		}
		else
		{
			_isTopDown = false;
			if (_isDirectX11)
			{
				_isTopDown = false;
			}
		}
		GenerateFilename();
		_waitForEndOfFrame = new WaitForEndOfFrame();
		return base.PrepareCapture();
	}

	public override void UnprepareCapture()
	{
		_waitForEndOfFrame = null;
		base.UnprepareCapture();
	}

	private IEnumerator FinalRenderCapture()
	{
		yield return _waitForEndOfFrame;
		TickFrameTimer();
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
			RenderThreadEvent(NativePlugin.PluginEvent.CaptureFrameBuffer);
			GL.InvalidateState();
			UpdateFPS();
		}
		RenormTimer();
	}

	public override void UpdateFrame()
	{
		if (_capturing && !_paused)
		{
			StartCoroutine(FinalRenderCapture());
		}
		base.UpdateFrame();
	}
}
