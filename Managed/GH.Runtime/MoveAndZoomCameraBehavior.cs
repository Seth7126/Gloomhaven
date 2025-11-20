#define ENABLE_LOGS
using System;
using UnityEngine;

public class MoveAndZoomCameraBehavior : CameraBehavior
{
	private float? zoomTo;

	private Vector3? moveTo;

	private float duration;

	private AnimationCurve animationCurve;

	private Action onFinished;

	private Action onCanceled;

	private LeanTweenType easeType;

	private LTDescr animDescr;

	public bool DisableCancelCallbackIfFinished { get; set; }

	public MoveAndZoomCameraBehavior(string id, float? zoomTo, Vector3? moveTo, float duration, LeanTweenType easeType = LeanTweenType.notUsed, AnimationCurve animationCurve = null, Action onFinished = null, Action onCanceled = null)
		: base(id)
	{
		this.zoomTo = zoomTo;
		this.moveTo = moveTo;
		this.animationCurve = animationCurve;
		this.duration = duration;
		this.onFinished = onFinished;
		this.easeType = easeType;
		this.onCanceled = onCanceled;
	}

	public MoveAndZoomCameraBehavior(string id, float zoomTo, float duration, Action onFinished = null, LeanTweenType easeType = LeanTweenType.notUsed, AnimationCurve animationCurve = null)
		: this(id, zoomTo, null, duration, easeType, animationCurve, onFinished)
	{
	}

	public MoveAndZoomCameraBehavior(string id, Vector3 moveTo, float duration, Action onFinished = null, LeanTweenType easeType = LeanTweenType.notUsed, AnimationCurve animationCurve = null)
		: this(id, null, moveTo, duration, easeType, animationCurve, onFinished)
	{
	}

	private void Cancel()
	{
		if (animDescr != null)
		{
			Debug.LogGUI("Cancel  Move and Zoom Camera Behavior animation");
			LeanTween.cancel(animDescr.id);
			animDescr = null;
		}
	}

	public override void Disable(CameraController camera)
	{
		Debug.LogGUI($"Disable Move and Zoom Camera Behavior (enabled {base.IsEnabled} animating {animDescr != null})");
		if (base.IsEnabled && animDescr != null)
		{
			camera.FreeDisableCameraInput(this);
			if (moveTo.HasValue)
			{
				float? num = camera.CalculateZoomForY(camera.m_Camera.transform.position.y);
				if (num.HasValue)
				{
					camera.SetZoom(num.Value);
				}
			}
		}
		Cancel();
		onCanceled?.Invoke();
		base.Disable(camera);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Cancel();
	}

	public override bool IsType(ECameraBehaviorType type)
	{
		if (type == ECameraBehaviorType.Move && moveTo.HasValue)
		{
			return true;
		}
		if (type == ECameraBehaviorType.Zoom && zoomTo.HasValue)
		{
			return true;
		}
		return false;
	}

	public override void Enable(CameraController camera)
	{
		base.Enable(camera);
		if (moveTo.HasValue)
		{
			moveTo = new Vector3(Mathf.Max(camera.m_FocalBounds.min.x, Mathf.Min(camera.m_FocalBounds.max.x, moveTo.Value.x)), moveTo.Value.y, Mathf.Max(camera.m_FocalBounds.min.z, Mathf.Min(camera.m_FocalBounds.max.z, moveTo.Value.z)));
		}
		Cancel();
		if (duration <= 0f)
		{
			Debug.LogGUI("Insta Move and Zoom Camera Behavior");
			OnFinished(camera);
			return;
		}
		Vector3 startFocal = camera.FocusPoint;
		float startFOV = camera.m_Camera.fieldOfView;
		Vector3 startPos = camera.m_Camera.transform.position;
		float yDist = 0f;
		if (zoomTo.HasValue)
		{
			yDist = camera.CalculateYForZoom(zoomTo.Value) - startPos.y;
		}
		camera.RequestDisableCameraInput(this);
		animDescr = LeanTween.value(camera.gameObject, delegate(float tSmooth)
		{
			float? y = null;
			if (zoomTo.HasValue)
			{
				camera.SetZoom(Mathf.Lerp(startFOV, zoomTo.Value, tSmooth));
			}
			if (moveTo.HasValue)
			{
				camera.m_TargetFocalPoint = Vector3.Lerp(startFocal, moveTo.Value, tSmooth);
				y = startPos.y + yDist * tSmooth;
			}
			camera.RefreshPositionToFocusOnTargetPoint(y);
		}, 0f, 1f, duration).setIgnoreTimeScale(useUnScaledTime: true).setOnComplete((Action)delegate
		{
			Debug.LogGUI("Finish Move and Zoom Camera Behavior");
			animDescr = null;
			camera.FreeDisableCameraInput(this);
			OnFinished(camera);
		});
		Debug.LogGUI("Create Move and Zoom Camera animation " + animDescr.id + " " + animDescr.trans.name);
		if (animationCurve != null)
		{
			animDescr.setEase(animationCurve);
		}
		else
		{
			animDescr.setEase(easeType);
		}
	}

	private void OnFinished(CameraController camera)
	{
		if (zoomTo.HasValue)
		{
			camera.SetZoom(zoomTo.Value);
		}
		if (moveTo.HasValue)
		{
			camera.m_TargetFocalPoint = moveTo.Value;
		}
		if (DisableCancelCallbackIfFinished)
		{
			onCanceled = null;
		}
		camera.ResetPositionToFocusOnTargetPoint();
		onFinished?.Invoke();
	}
}
