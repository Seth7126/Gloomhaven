#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectedAnimation : MonoBehaviour
{
	[SerializeField]
	private List<HoverEffect.HoverEffectSettings> _effects;

	private List<HoverEffect.HoverEffectSettings> _undoEffects;

	private readonly List<LTDescr> _animationsInProcess = new List<LTDescr>();

	private void Awake()
	{
		_undoEffects = new List<HoverEffect.HoverEffectSettings>();
		CalculateUndoEffects();
	}

	private void CalculateUndoEffects()
	{
		foreach (HoverEffect.HoverEffectSettings effect in _effects)
		{
			switch (effect.Animation)
			{
			case TweenAction.SCALE:
				_undoEffects.Add(new HoverEffect.HoverEffectSettings(effect, effect.Transform.localScale));
				break;
			case TweenAction.MOVE:
				_undoEffects.Add(new HoverEffect.HoverEffectSettings(effect, effect.Transform.anchoredPosition3D));
				break;
			case TweenAction.MOVE_LOCAL:
				_undoEffects.Add(new HoverEffect.HoverEffectSettings(effect, effect.Transform.localPosition));
				break;
			case TweenAction.CANVASGROUP_ALPHA:
				_undoEffects.Add(new HoverEffect.HoverEffectSettings(effect, effect.Transform.GetComponent<CanvasGroup>().alpha));
				break;
			default:
				Debug.LogError("Effect not supported :" + effect.Animation);
				break;
			}
		}
	}

	public void Animate(bool toTargetValue)
	{
		Cancel();
		foreach (HoverEffect.HoverEffectSettings item in toTargetValue ? _effects : _undoEffects)
		{
			LTDescr anim = null;
			switch (item.Animation)
			{
			case TweenAction.SCALE:
				anim = LeanTween.scale(item.Transform, item.ToValue, item.Duration).setEase(item.AnimationType);
				break;
			case TweenAction.MOVE:
				anim = LeanTween.move(item.Transform, item.ToValue, item.Duration).setEase(item.AnimationType);
				break;
			case TweenAction.MOVE_LOCAL:
				anim = LeanTween.moveLocal(item.Transform.gameObject, item.ToValue, item.Duration).setEase(item.AnimationType);
				break;
			case TweenAction.CANVASGROUP_ALPHA:
				anim = LeanTween.alphaCanvas(item.Transform.GetComponent<CanvasGroup>(), item.ToUniqueValue, item.Duration).setEase(item.AnimationType);
				break;
			default:
				Debug.LogError("Effect not supported :" + item.Animation);
				break;
			}
			if (anim != null)
			{
				anim.setOnComplete((Action)delegate
				{
					_animationsInProcess.Remove(anim);
				});
				_animationsInProcess.Add(anim);
			}
		}
	}

	public void Cancel()
	{
		if (_animationsInProcess == null)
		{
			return;
		}
		foreach (LTDescr item in _animationsInProcess)
		{
			if (item.debug)
			{
				Debug.Log($"HoverEffect cancels {item.id} {base.gameObject.name}");
			}
			LeanTween.cancel(item.id);
		}
		_animationsInProcess.Clear();
	}
}
