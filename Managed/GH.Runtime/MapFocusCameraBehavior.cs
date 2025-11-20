using System;
using UnityEngine;

public class MapFocusCameraBehavior : MoveAndZoomCameraBehavior
{
	public const string FOCUS_ID = "MAP_FOCUS";

	public MapFocusCameraBehavior(float? zoomTo, Vector3? moveTo, float duration, Action onFinished = null, Action onCanceled = null)
		: base("MAP_FOCUS", zoomTo, moveTo, duration, UIInfoTools.Instance.focusAnimationEase, UIInfoTools.Instance.focusAnimationCurve, onFinished, onCanceled)
	{
	}

	public MapFocusCameraBehavior(float zoomTo, float duration, Action onFinished = null)
		: this(zoomTo, null, duration, onFinished)
	{
	}

	public MapFocusCameraBehavior(Vector3 moveTo, float duration, Action onFinished = null)
		: this(null, moveTo, duration, onFinished)
	{
	}
}
