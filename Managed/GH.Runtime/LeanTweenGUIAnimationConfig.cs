using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class LeanTweenGUIAnimationConfig
{
	[Header("Scale")]
	[SerializeField]
	private List<LeanTweenGuiAnimationSettingScale> scaleEffects;

	[Header("Move")]
	[SerializeField]
	private List<LeanTweenGuiAnimationSettingMove> moveEffects;

	[Header("Fade")]
	[SerializeField]
	private List<LeanTweenGuiAnimationSettingFade> fadeEffects;

	[Header("Color")]
	[SerializeField]
	private List<LeanTweenGUIAnimationSettingColor> colorEffects;

	[Header("Material")]
	[SerializeField]
	private List<LeanTweenGuiAnimationSettingMaterialPropertyFloat> materialEffects;

	[Header("Custom")]
	[SerializeField]
	private List<CustomLeanTweenGuiAnimationSetting> customEffects;

	private List<LeanTweenGUIAnimationSetting> allSettings;

	public List<LeanTweenGUIAnimationSetting> GetSettings()
	{
		if (allSettings == null)
		{
			allSettings = ((IEnumerable<LeanTweenGUIAnimationSetting>)scaleEffects).Concat((IEnumerable<LeanTweenGUIAnimationSetting>)moveEffects).Concat(materialEffects).Concat(fadeEffects)
				.Concat(customEffects)
				.Concat(colorEffects)
				.ToList();
		}
		return allSettings;
	}
}
