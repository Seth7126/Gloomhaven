using System;
using UnityEngine.UI;

[Serializable]
public class LeanTweenGuiAnimationSettingMaterialPropertyFloat : LeanTweenGuiAnimationSettingMaterial<float>
{
	protected override LTDescr BuildTweenAction()
	{
		if (!Target.material.HasProperty(property))
		{
			return null;
		}
		return LeanTween.value(Target.gameObject, Target.material.GetFloat(property), ToValue, Duration).setOnUpdate(SetValue);
	}

	protected override void SetValue(float value)
	{
		switch (Animation)
		{
		case GUIAnimationMaterialType.Material:
			if (Target.material.HasProperty(property))
			{
				Target.material.SetFloat(property, value);
			}
			break;
		case GUIAnimationMaterialType.MaterialForRendering:
			if (Target.materialForRendering.HasProperty(property))
			{
				Target.materialForRendering.SetFloat(property, value);
			}
			break;
		default:
			Debug.LogError("Effect not supported :" + Animation);
			break;
		}
	}

	public LeanTweenGuiAnimationSettingMaterialPropertyFloat(Graphic target, string property, float duration, GUIAnimationMaterialType type = GUIAnimationMaterialType.MaterialForRendering, float delay = 0f, LeanTweenType easing = LeanTweenType.notUsed, bool applyToOriginalMaterial = false)
		: base(target, property, duration, type, delay, easing, applyToOriginalMaterial)
	{
	}
}
