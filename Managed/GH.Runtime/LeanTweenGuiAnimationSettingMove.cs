using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class LeanTweenGuiAnimationSettingMove : LeanTweenGuiAnimationSetting<Vector3, GUIAnimationMoveType, RectTransform>
{
	protected override LTDescr BuildTweenAction()
	{
		LTDescr result = null;
		switch (Animation)
		{
		case GUIAnimationMoveType.MOVE:
			result = LeanTween.move(Target, ToValue, Duration);
			break;
		case GUIAnimationMoveType.MOVE_LOCAL:
			result = LeanTween.moveLocal(Target.gameObject, ToValue, Duration);
			break;
		case GUIAnimationMoveType.SIZE_DELTA:
			result = LeanTween.size(Target, ToValue, Duration);
			break;
		case GUIAnimationMoveType.UV:
		{
			RawImage image = Target.GetComponent<RawImage>();
			if (image == null)
			{
				Debug.LogErrorFormat("Error in LeanTweenGuiAnimationSettingMove: Target ({0}) doesn't have a RawImage component", Target.name);
				return null;
			}
			Rect rect = image.uvRect;
			result = LeanTween.value(Target.gameObject, (Vector3)rect.position, ToValue, Duration).setOnUpdateVector3(delegate(Vector3 val)
			{
				rect.position = val;
				image.uvRect = rect;
			});
			break;
		}
		default:
			Debug.LogError("Effect not supported :" + Animation);
			break;
		}
		return result;
	}

	protected override void SetValue(Vector3 value)
	{
		switch (Animation)
		{
		case GUIAnimationMoveType.MOVE:
			Target.anchoredPosition = value;
			break;
		case GUIAnimationMoveType.MOVE_LOCAL:
			Target.localPosition = value;
			break;
		case GUIAnimationMoveType.SIZE_DELTA:
			Target.sizeDelta = value;
			break;
		case GUIAnimationMoveType.UV:
		{
			RawImage component = Target.GetComponent<RawImage>();
			if (component == null)
			{
				Debug.LogErrorFormat("Error in LeanTweenGuiAnimationSettingMove: Target ({0}) doesn't have a RawImage component", Target.name);
			}
			else
			{
				Rect uvRect = component.uvRect;
				uvRect.position = value;
				component.uvRect = uvRect;
			}
			break;
		}
		default:
			Debug.LogError("Effect not supported :" + Animation);
			break;
		}
	}

	public LeanTweenGuiAnimationSettingMove(RectTransform target, GUIAnimationMoveType type, float duration, float delay = 0f, LeanTweenType easing = LeanTweenType.notUsed)
		: base(target, type, duration, delay, easing)
	{
	}

	public LeanTweenGuiAnimationSettingMove(Graphic target, float duration, float delay = 0f, LeanTweenType easing = LeanTweenType.notUsed)
		: base(target.rectTransform, GUIAnimationMoveType.UV, duration, delay, easing)
	{
	}
}
