#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverEffect : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[Serializable]
	public struct HoverEffectSettings
	{
		public RectTransform Transform;

		public TweenAction Animation;

		public Vector3 ToValue;

		public float ToUniqueValue;

		public float Duration;

		public LeanTweenType AnimationType;

		public HoverEffectSettings(RectTransform transform, TweenAction animation, Vector3 toValue, float duration, LeanTweenType AnimationType, float uniquevalue = 0f)
		{
			Transform = transform;
			Animation = animation;
			ToValue = toValue;
			Duration = duration;
			this.AnimationType = AnimationType;
			ToUniqueValue = uniquevalue;
		}

		public HoverEffectSettings(HoverEffectSettings effect, Vector3 originalValue)
			: this(effect.Transform, effect.Animation, originalValue, effect.Duration, effect.AnimationType)
		{
		}

		public HoverEffectSettings(HoverEffectSettings effect, float originalValue)
			: this(effect.Transform, effect.Animation, Vector3.zero, effect.Duration, effect.AnimationType, originalValue)
		{
		}
	}

	[SerializeField]
	private List<HoverEffectSettings> effects;

	private List<HoverEffectSettings> undoEffects;

	private readonly List<LTDescr> animationsInProcess = new List<LTDescr>();

	private void Start()
	{
		undoEffects = new List<HoverEffectSettings>();
		CalculateUndoEffects();
	}

	private void CalculateUndoEffects()
	{
		foreach (HoverEffectSettings effect in effects)
		{
			switch (effect.Animation)
			{
			case TweenAction.SCALE:
				undoEffects.Add(new HoverEffectSettings(effect, effect.Transform.localScale));
				break;
			case TweenAction.MOVE:
				undoEffects.Add(new HoverEffectSettings(effect, effect.Transform.anchoredPosition3D));
				break;
			case TweenAction.MOVE_LOCAL:
				undoEffects.Add(new HoverEffectSettings(effect, effect.Transform.localPosition));
				break;
			case TweenAction.CANVASGROUP_ALPHA:
				undoEffects.Add(new HoverEffectSettings(effect, effect.Transform.GetComponent<CanvasGroup>().alpha));
				break;
			default:
				Debug.LogError("Effect not supported :" + effect.Animation);
				break;
			}
		}
	}

	public void Reset()
	{
		undoEffects.Clear();
		CalculateUndoEffects();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Cancel();
		Animate(effects);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Cancel();
		Animate(undoEffects);
	}

	public void OnSelect(BaseEventData eventData)
	{
		Cancel();
		Animate(effects);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		Cancel();
		Animate(undoEffects);
	}

	private void Animate(IEnumerable<HoverEffectSettings> effects)
	{
		foreach (HoverEffectSettings effect in effects)
		{
			LTDescr anim = null;
			switch (effect.Animation)
			{
			case TweenAction.SCALE:
				anim = LeanTween.scale(effect.Transform, effect.ToValue, effect.Duration).setEase(effect.AnimationType);
				break;
			case TweenAction.MOVE:
				anim = LeanTween.move(effect.Transform, effect.ToValue, effect.Duration).setEase(effect.AnimationType);
				break;
			case TweenAction.MOVE_LOCAL:
				anim = LeanTween.moveLocal(effect.Transform.gameObject, effect.ToValue, effect.Duration).setEase(effect.AnimationType);
				break;
			case TweenAction.CANVASGROUP_ALPHA:
				anim = LeanTween.alphaCanvas(effect.Transform.GetComponent<CanvasGroup>(), effect.ToUniqueValue, effect.Duration).setEase(effect.AnimationType);
				break;
			default:
				Debug.LogError("Effect not supported :" + effect.Animation);
				break;
			}
			if (anim != null)
			{
				anim.setOnComplete((Action)delegate
				{
					animationsInProcess.Remove(anim);
				});
				animationsInProcess.Add(anim);
			}
		}
	}

	public void Cancel()
	{
		if (animationsInProcess == null)
		{
			return;
		}
		foreach (LTDescr item in animationsInProcess)
		{
			if (item.debug)
			{
				Debug.Log($"HoverEffect cancels {item.id} {base.gameObject.name}");
			}
			LeanTween.cancel(item.id);
		}
		animationsInProcess.Clear();
	}
}
