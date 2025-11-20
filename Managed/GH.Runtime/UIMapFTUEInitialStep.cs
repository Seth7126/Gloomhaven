using System;
using AsmodeeNet.Foundation;
using Assets.Script.Misc;
using UnityEngine;
using UnityEngine.UI;

public class UIMapFTUEInitialStep : UIMapFTUEStep, IEscapable
{
	[SerializeField]
	private string introVideoPath = "CP_Intro/GH_CP_Intro";

	[SerializeField]
	private float fadeInDuration = 1f;

	private LTDescr fadeAnim;

	[SerializeField]
	private ClickTrackerExtended clickTrackerExtended;

	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	private void Awake()
	{
		clickTrackerExtended.enabled = false;
		StopFadeIn();
	}

	public override ICallbackPromise StartStep()
	{
		StopFadeIn();
		TransitionManager.s_Instance.SetFade(1f);
		Singleton<UIWindowManager>.Instance.AddSkipShowWindows(UIWindowID.ESCMenu);
		Singleton<ESCMenu>.Instance.Hide();
		NewPartyDisplayUI.PartyDisplay.Hide(this, instant: true);
		return base.StartStep();
	}

	public override void Show(Action onFinished = null)
	{
		clickTrackerExtended.enabled = true;
		clickTrackerExtended.onClick.AddListener(delegate
		{
			VideoCamera.s_This.Stop();
			FadeInShow(onFinished);
		});
		UIWindowManager.RegisterEscapable(this);
		AudioController.StopMusic();
		VideoCamera.s_This.PlayFullscreenVideo(introVideoPath, delegate
		{
			FadeInShow(onFinished);
		}, reenableDisabledObjects: true);
	}

	private void FadeInShow(Action onFinished)
	{
		AudioController.PlayMusicPlaylist();
		StopFadeIn();
		UIWindowManager.UnregisterEscapable(this);
		clickTrackerExtended.enabled = false;
		fadeAnim = LeanTween.value(base.gameObject, delegate(float val)
		{
			TransitionManager.s_Instance.SetFade(val);
		}, 1f, 0f, fadeInDuration).setOnComplete((Action)delegate
		{
			Singleton<UIWindowManager>.Instance.RemoveSkipShowWindows(UIWindowID.ESCMenu);
			fadeAnim = null;
			base.Show(onFinished);
		});
	}

	public override void FinishStep()
	{
		ResetState();
		base.FinishStep();
	}

	protected override void OnFinishedStep()
	{
		if (base.IsActive)
		{
			ResetState();
			base.OnFinishedStep();
		}
	}

	private void ResetState()
	{
		NewPartyDisplayUI.PartyDisplay.Show(this);
		UIWindowManager.UnregisterEscapable(this);
		Singleton<UIWindowManager>.Instance.RemoveSkipShowWindows(UIWindowID.ESCMenu);
		clickTrackerExtended.enabled = false;
		VideoCamera.s_This.Stop();
		StopFadeIn();
		TransitionManager.s_Instance.SetFade(0f);
	}

	private void StopFadeIn()
	{
		if (fadeAnim != null)
		{
			LeanTween.cancel(fadeAnim.id);
			fadeAnim = null;
		}
	}

	private void OnDestroy()
	{
		clickTrackerExtended.onClick.RemoveAllListeners();
		if (!CoreApplication.IsQuitting)
		{
			UIWindowManager.UnregisterEscapable(this);
			Singleton<UIWindowManager>.Instance.RemoveSkipShowWindows(UIWindowID.ESCMenu);
			StopFadeIn();
		}
	}

	public bool Escape()
	{
		if (VideoCamera.IsPlaying)
		{
			clickTrackerExtended.onClick.Invoke();
		}
		return true;
	}

	public int Order()
	{
		return 0;
	}
}
