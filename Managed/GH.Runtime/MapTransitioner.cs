using System;
using UnityEngine;

public class MapTransitioner : MonoBehaviour
{
	[SerializeField]
	private float oldMapFadeOut;

	[SerializeField]
	private float newMapFadeIn;

	private LTSeq transition;

	public void TransitionTo(float zoomAmountExit, float zoomAmountEnter, Action onLoadNewMap, Action onFinished = null)
	{
		CancelAnimation();
		Singleton<AdventureMapUIManager>.Instance.LockOptionsInteraction(locked: true, this);
		Singleton<UIGuildmasterHUD>.Instance.LockInteraction(this, locked: true);
		InputManager.RequestDisableInput(this, EKeyActionTag.All);
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
		float num = CameraController.s_CameraController.Zoom + zoomAmountExit;
		if (num < CameraController.s_CameraController.m_MinimumFOV)
		{
			CameraController.s_CameraController.m_ExtraMinimumFOV = num - CameraController.s_CameraController.m_MinimumFOV;
		}
		else if (num > CameraController.s_CameraController.m_DefaultFOV)
		{
			CameraController.s_CameraController.m_DefaultFOV = num;
		}
		CameraController.s_CameraController.ZoomFOV(zoomAmountExit, oldMapFadeOut);
		transition = LeanTween.sequence().append(LeanTween.value(base.gameObject, delegate(float val)
		{
			TransitionManager.s_Instance.SetFade(val);
		}, 0f, 1f, oldMapFadeOut)).append(delegate
		{
			CameraController.s_CameraController.m_ExtraMinimumFOV = 0f;
			onLoadNewMap?.Invoke();
			CameraController.s_CameraController.ZoomFOV(zoomAmountEnter, newMapFadeIn);
		})
			.append(LeanTween.value(base.gameObject, delegate(float val)
			{
				TransitionManager.s_Instance.SetFade(val);
			}, 1f, 0f, newMapFadeIn))
			.append(delegate
			{
				transition = null;
				Singleton<AdventureMapUIManager>.Instance.LockOptionsInteraction(locked: false, this);
				Singleton<UIGuildmasterHUD>.Instance.LockInteraction(this, locked: false);
				InputManager.RequestEnableInput(this, EKeyActionTag.All);
				CameraController.s_CameraController?.FreeDisableCameraInput(this);
				onFinished?.Invoke();
			});
	}

	private void CancelAnimation()
	{
		if (transition != null)
		{
			LeanTween.cancel(transition.id);
		}
		transition = null;
	}

	private void OnDestroy()
	{
		CancelAnimation();
	}
}
