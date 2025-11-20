using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class LeanTweenGuiAnimationSettingFade : LeanTweenGuiAnimationSetting<float, GUIAnimationFadeType, RectTransform>
{
	protected override LTDescr BuildTweenAction()
	{
		switch (Animation)
		{
		case GUIAnimationFadeType.CANVAS_GROUP:
		{
			CanvasGroup component = Target.GetComponent<CanvasGroup>();
			if (component != null)
			{
				return LeanTween.alphaCanvas(component, ToValue, Duration);
			}
			Debug.LogErrorFormat("Error in LeanTweenGuiAnimationSettingAlpha: Target ({0}) doesn't have CanvasGroup component", Target.name);
			return null;
		}
		case GUIAnimationFadeType.IMAGE:
		case GUIAnimationFadeType.GRAPHIC:
			return LeanTween.alpha(Target, ToValue, Duration);
		case GUIAnimationFadeType.TEXT:
		{
			TextMeshProUGUI text = Target.GetComponent<TextMeshProUGUI>();
			if (text != null)
			{
				return LeanTween.value(Target.gameObject, delegate(float alpha)
				{
					text.SetAlpha(alpha);
				}, OriginalValue, ToValue, Duration);
			}
			Debug.LogErrorFormat("Error in LeanTweenGuiAnimationSettingAlpha: Target ({0}) doesn't have TextMeshProUGUI component", Target.name);
			return null;
		}
		default:
			Debug.LogError("Effect not supported :" + Animation);
			return null;
		}
	}

	protected override void SetValue(float value)
	{
		switch (Animation)
		{
		case GUIAnimationFadeType.CANVAS_GROUP:
		{
			CanvasGroup component3 = Target.GetComponent<CanvasGroup>();
			if (component3 != null)
			{
				component3.alpha = value;
				break;
			}
			Debug.LogErrorFormat("Error in LeanTweenGuiAnimationSettingAlpha: Target ({0}) doesn't have CanvasGroup component", Target.name);
			break;
		}
		case GUIAnimationFadeType.IMAGE:
		case GUIAnimationFadeType.GRAPHIC:
		{
			Graphic component2 = Target.GetComponent<Graphic>();
			if (component2 == null)
			{
				Debug.LogErrorFormat("Error in LeanTweenGuiAnimationSettingAlpha: Target ({0}) doesn't have Graphic component", Target.name);
			}
			else
			{
				Color color = component2.color;
				color.a = value;
				component2.color = color;
			}
			break;
		}
		case GUIAnimationFadeType.TEXT:
		{
			TextMeshProUGUI component = Target.GetComponent<TextMeshProUGUI>();
			if (component == null)
			{
				Debug.LogErrorFormat("Error in LeanTweenGuiAnimationSettingAlpha: Target ({0}) doesn't have TextMeshProUGUI component", Target.name);
			}
			else
			{
				component.SetAlpha(value);
			}
			break;
		}
		default:
			Debug.LogError("Effect not supported :" + Animation);
			break;
		}
	}

	public LeanTweenGuiAnimationSettingFade(CanvasGroup target, float duration, float delay = 0f, LeanTweenType easing = LeanTweenType.notUsed)
		: base(target.GetComponent<RectTransform>(), GUIAnimationFadeType.CANVAS_GROUP, duration, delay, easing)
	{
	}

	public LeanTweenGuiAnimationSettingFade(Graphic target, float duration, float delay = 0f, LeanTweenType easing = LeanTweenType.notUsed)
		: base(target.rectTransform, GUIAnimationFadeType.GRAPHIC, duration, delay, easing)
	{
	}
}
