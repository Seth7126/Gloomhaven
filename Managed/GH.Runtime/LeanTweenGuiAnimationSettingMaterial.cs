using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public abstract class LeanTweenGuiAnimationSettingMaterial<T> : LeanTweenGuiAnimationSetting<T, GUIAnimationMaterialType, Graphic>
{
	[Header("Material properties")]
	public string property;

	public bool applyToOriginalMaterial;

	public override void Initialize()
	{
		base.Initialize();
		if (!applyToOriginalMaterial)
		{
			Target.material = new Material(Target.material);
		}
	}

	protected LeanTweenGuiAnimationSettingMaterial(Graphic target, string property, float duration, GUIAnimationMaterialType type = GUIAnimationMaterialType.MaterialForRendering, float delay = 0f, LeanTweenType easing = LeanTweenType.notUsed, bool applyToOriginalMaterial = false)
		: base(target, type, duration, delay, easing)
	{
		this.applyToOriginalMaterial = applyToOriginalMaterial;
		this.property = property;
	}
}
