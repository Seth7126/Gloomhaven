using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class LeanTweenGUIAnimationSettingColor : LeanTweenGuiAnimationSetting<Color, GUIAnimationColorType, RectTransform>
{
	protected override LTDescr BuildTweenAction()
	{
		switch (Animation)
		{
		case GUIAnimationColorType.GRAPHIC:
			return LeanTween.color(Target, ToValue, Duration).setRecursive(useRecursion: false);
		case GUIAnimationColorType.TEXT:
		{
			TextMeshProUGUI text = Target.GetComponent<TextMeshProUGUI>();
			if (text != null)
			{
				return LeanTween.value(Target.gameObject, delegate(Color color)
				{
					text.color = color;
				}, text.color, ToValue, Duration);
			}
			Debug.LogErrorFormat("Error in LeanTweenGUIAnimationSettingColor: Target ({0}) doesn't have TextMeshProUGUI component", Target.name);
			return null;
		}
		default:
			Debug.LogError("Effect not supported :" + Animation);
			return null;
		}
	}

	protected override void SetValue(Color value)
	{
		switch (Animation)
		{
		case GUIAnimationColorType.GRAPHIC:
		{
			Graphic component2 = Target.GetComponent<Graphic>();
			if (component2 == null)
			{
				Debug.LogErrorFormat("Error in LeanTweenGUIAnimationSettingColor: Target ({0}) doesn't have Graphic component", Target.name);
			}
			else
			{
				component2.color = value;
			}
			break;
		}
		case GUIAnimationColorType.TEXT:
		{
			TextMeshProUGUI component = Target.GetComponent<TextMeshProUGUI>();
			if (component == null)
			{
				Debug.LogErrorFormat("Error in LeanTweenGUIAnimationSettingColor: Target ({0}) doesn't have TextMeshProUGUI component", Target.name);
			}
			else
			{
				component.color = value;
			}
			break;
		}
		default:
			Debug.LogError("Effect not supported :" + Animation);
			break;
		}
	}

	public LeanTweenGUIAnimationSettingColor(RectTransform target, GUIAnimationColorType type, float duration, float delay = 0f, LeanTweenType easing = LeanTweenType.notUsed)
		: base(target, type, duration, delay, easing)
	{
	}
}
