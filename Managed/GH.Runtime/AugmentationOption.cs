using ScenarioRuleLibrary;
using UnityEngine;

public class AugmentationOption : AugmentationHolder, IOption
{
	public AugmentationOption(CActionAugmentation augmentation)
		: base(augmentation.Name, augmentation)
	{
	}

	public Sprite GetPickerIcon()
	{
		return null;
	}

	public string GetPickerText()
	{
		return PreviewEffectGenerator.GenerateDescription(base.Augmentation);
	}

	public string GetSelectedText()
	{
		return PreviewEffectGenerator.GenerateDescription(base.Augmentation.CostAbility);
	}
}
