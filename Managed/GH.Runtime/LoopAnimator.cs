#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoopAnimator : MonoBehaviour
{
	[SerializeField]
	private List<AnimationSetting> effects;

	[SerializeField]
	private bool autoStart;

	[SerializeField]
	private bool ignoreTimeScale;

	private List<LTDescr> currentAnim = new List<LTDescr>();

	private bool looping;

	private Dictionary<AnimationSetting, AnimationSetting> undoEffects;

	private void Awake()
	{
		CreateUndo();
	}

	private void OnEnable()
	{
		if (autoStart)
		{
			StartLoop();
		}
	}

	[ContextMenu("Start")]
	public void StartLoop(bool resetToInitial = false)
	{
		if (!looping && base.gameObject.activeSelf)
		{
			CreateUndo();
			if (resetToInitial)
			{
				ResetToInitialState();
			}
			looping = true;
			effects.ForEach(delegate(AnimationSetting it)
			{
				Loop(it);
			});
		}
	}

	[ContextMenu("Stop")]
	public void StopLoop(bool resetToInitial = false)
	{
		if (!looping)
		{
			return;
		}
		looping = false;
		foreach (LTDescr item in currentAnim)
		{
			if (item.debug)
			{
				Debug.Log($"Loop animator cancels {item.id} {base.gameObject.name}");
			}
			LeanTween.cancel(item.uniqueId);
		}
		currentAnim.Clear();
		if (resetToInitial)
		{
			ResetToInitialState();
		}
	}

	public void ResetToInitialState()
	{
		foreach (AnimationSetting value in undoEffects.Values)
		{
			switch (value.Animation)
			{
			case TweenAction.SCALE:
				value.Transform.localScale = value.ToValue;
				break;
			case TweenAction.MOVE:
				value.Transform.anchoredPosition = value.ToValue;
				break;
			case TweenAction.MOVE_LOCAL:
				value.Transform.localPosition = value.ToValue;
				break;
			case TweenAction.CANVASGROUP_ALPHA:
				value.Transform.GetComponent<CanvasGroup>().alpha = value.ToUniqueValue;
				break;
			case TweenAction.CANVAS_ALPHA:
			{
				Graphic component = value.Transform.GetComponent<Graphic>();
				Color color = component.color;
				color.a = value.ToUniqueValue;
				component.color = color;
				break;
			}
			default:
				Debug.LogError("Effect not supported :" + value.Animation);
				break;
			}
		}
	}

	private void OnDisable()
	{
		StopLoop();
	}

	private void CreateUndo()
	{
		if (undoEffects != null)
		{
			return;
		}
		undoEffects = new Dictionary<AnimationSetting, AnimationSetting>();
		foreach (AnimationSetting effect in effects)
		{
			switch (effect.Animation)
			{
			case TweenAction.SCALE:
				undoEffects[effect] = new AnimationSetting(effect.Transform, effect.Animation, effect.Transform.localScale, effect.Duration, effect.AnimationType);
				break;
			case TweenAction.MOVE:
				undoEffects[effect] = new AnimationSetting(effect.Transform, effect.Animation, effect.Transform.anchoredPosition, effect.Duration, effect.AnimationType);
				break;
			case TweenAction.MOVE_LOCAL:
				undoEffects[effect] = new AnimationSetting(effect.Transform, effect.Animation, effect.Transform.localPosition, effect.Duration, effect.AnimationType);
				break;
			case TweenAction.CANVASGROUP_ALPHA:
				undoEffects[effect] = new AnimationSetting(effect.Transform, effect.Animation, effect.UseSpecificFromValue ? effect.FromUniqueValue : effect.Transform.GetComponent<CanvasGroup>().alpha, effect.Duration, effect.AnimationType);
				break;
			case TweenAction.CANVAS_ALPHA:
				undoEffects[effect] = new AnimationSetting(effect.Transform, effect.Animation, effect.UseSpecificFromValue ? effect.FromUniqueValue : effect.Transform.GetComponent<Graphic>().color.a, effect.Duration, effect.AnimationType);
				break;
			default:
				Debug.LogError("Effect not supported :" + effect.Animation);
				break;
			}
		}
	}

	private LTDescr Animate(AnimationSetting effect)
	{
		LTDescr lTDescr = null;
		switch (effect.Animation)
		{
		case TweenAction.SCALE:
			lTDescr = LeanTween.scale(effect.Transform, effect.ToValue, effect.Duration).setEase(effect.AnimationType);
			break;
		case TweenAction.MOVE:
			lTDescr = LeanTween.move(effect.Transform, effect.ToValue, effect.Duration).setEase(effect.AnimationType);
			break;
		case TweenAction.MOVE_LOCAL:
			lTDescr = LeanTween.moveLocal(effect.Transform.gameObject, effect.ToValue, effect.Duration).setEase(effect.AnimationType);
			break;
		case TweenAction.CANVASGROUP_ALPHA:
			lTDescr = LeanTween.alphaCanvas(effect.Transform.GetComponent<CanvasGroup>(), effect.ToUniqueValue, effect.Duration).setEase(effect.AnimationType);
			break;
		case TweenAction.CANVAS_ALPHA:
			lTDescr = LeanTween.alpha(effect.Transform, effect.ToUniqueValue, effect.Duration).setEase(effect.AnimationType);
			break;
		default:
			Debug.LogError("Effect not supported :" + effect.Animation);
			break;
		}
		lTDescr?.setIgnoreTimeScale(ignoreTimeScale);
		return lTDescr;
	}

	private void Loop(AnimationSetting effect)
	{
		PlayEffect(effect, delegate
		{
			PlayEffect(undoEffects[effect], delegate
			{
				Loop(effect);
			});
		});
	}

	private void PlayEffect(AnimationSetting effect, Action onComplete)
	{
		if (looping)
		{
			LTDescr anim = Animate(effect);
			_ = anim.uniqueId;
			anim.setOnComplete((Action)delegate
			{
				currentAnim.Remove(anim);
				onComplete?.Invoke();
			});
			currentAnim.Add(anim);
		}
	}
}
