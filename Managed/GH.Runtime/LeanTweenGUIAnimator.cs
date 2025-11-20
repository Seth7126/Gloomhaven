#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeanTweenGUIAnimator : GUIAnimator
{
	[SerializeField]
	protected LeanTweenGUIAnimationConfig animations;

	[SerializeField]
	protected bool ignoreScaleTime;

	[SerializeField]
	private bool loop;

	protected List<LTDescr> animationsInProcess = new List<LTDescr>();

	private bool initialized;

	private void Awake()
	{
		Initialize();
	}

	private void Initialize()
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		foreach (LeanTweenGUIAnimationSetting setting in GetSettings())
		{
			setting.Initialize();
		}
	}

	protected override void Animate()
	{
		LTDescr lastAnimation = null;
		foreach (LeanTweenGUIAnimationSetting effect in GetSettings())
		{
			LTDescr anim = effect.BuildTween()?.setIgnoreTimeScale(ignoreScaleTime);
			if (anim == null)
			{
				continue;
			}
			anim.debug = enableDebug;
			animationsInProcess.Add(anim);
			if (loop)
			{
				anim.setLoopClamp();
				anim.setOnCompleteOnRepeat(isOn: true);
				anim.setOnComplete((Action)delegate
				{
					anim.setDelay(effect.Delay);
				});
				continue;
			}
			anim.setOnComplete((Action)delegate
			{
				animationsInProcess.Remove(anim);
				if (enableDebug)
				{
					Debug.Log($"[Animation] Completed tween {anim.trans.name}: type {anim.type} id {anim.id} for {base.gameObject.name}. Remaining: {animationsInProcess.Count}");
				}
				anim = null;
			});
			if (lastAnimation == null || effect.Delay + effect.Duration >= lastAnimation.time + lastAnimation.delay)
			{
				lastAnimation = anim;
			}
			if (enableDebug)
			{
				Debug.Log($"[Animation] Create tween {anim.trans.name}: type {anim.type} duration {anim.time} delay {anim.delay} id {anim.id} for {base.gameObject.name}");
			}
		}
		if (lastAnimation == null)
		{
			return;
		}
		lastAnimation.setOnComplete((Action)delegate
		{
			animationsInProcess.Remove(lastAnimation);
			if (enableDebug)
			{
				Debug.Log($"[Animation] Completed last tween {lastAnimation.trans.name}: type {lastAnimation.type} id {lastAnimation.id} for {base.gameObject.name}. Remaining: {animationsInProcess.Count}");
			}
			lastAnimation = null;
			FinishAnimation();
		});
	}

	protected override void Clear()
	{
		base.Clear();
		foreach (LTDescr item in animationsInProcess)
		{
			if (item.debug)
			{
				Debug.Log($"Lean tween anim cancels {item.id} {base.gameObject.name}");
			}
			LeanTween.cancel(item.id);
		}
		animationsInProcess.Clear();
	}

	protected override void ResetInitialState()
	{
		Initialize();
		foreach (LeanTweenGUIAnimationSetting item in from it in animations.GetSettings()
			orderby it.Delay descending
			select it)
		{
			item.RestorOriginalValue();
		}
		if (enableDebug)
		{
			Debug.Log("[Animation] ResetInitialState " + base.gameObject.name);
		}
	}

	protected override void ResetFinishState()
	{
		Initialize();
		foreach (LeanTweenGUIAnimationSetting item in from it in animations.GetSettings()
			orderby it.Delay + it.Duration
			select it)
		{
			item.SetFinalValue();
		}
		if (enableDebug)
		{
			Debug.Log("[Animation] ResetFinishState " + base.gameObject.name);
		}
	}

	private void OnDisable()
	{
		Stop();
	}

	public List<LeanTweenGUIAnimationSetting> GetSettings()
	{
		return animations.GetSettings();
	}
}
