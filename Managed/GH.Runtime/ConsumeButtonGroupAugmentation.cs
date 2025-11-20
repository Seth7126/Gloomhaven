using System.Collections.Generic;
using GLOOM;
using ScenarioRuleLibrary;

public class ConsumeButtonGroupAugmentation : IAugmentation
{
	private ISingleOptionHolder optionHolder;

	private UIUsePreview previewEffect;

	private string groupId;

	public AugmentationHolder SelectedAugment => optionHolder.SelectedOption as AugmentationOption;

	public string ID => groupId;

	public ConsumeButtonGroupAugmentation(string groupId, List<CActionAugmentation> augmentations, ISingleOptionHolder optionHolder, UIUsePreview previewEffect)
	{
		this.groupId = groupId;
		this.optionHolder = optionHolder;
		this.previewEffect = previewEffect;
		previewEffect.SetDescription(LocalizationManager.GetTranslation($"UI_PREVIEW_EFFECT_AUGMENT_GROUP_{groupId}"));
	}

	public void ActiveAugment(List<ElementInfusionBoardManager.EElement> elements)
	{
		previewEffect.SetDescription(SelectedAugment.Augmentation);
		SelectedAugment.ActiveAugment(elements);
	}

	public bool CanBeDisactivated()
	{
		if (SelectedAugment != null)
		{
			return SelectedAugment.CanBeDisactivated();
		}
		return false;
	}

	public string GetSelectAudioItem()
	{
		return UIInfoTools.Instance.toggleUseCharacterSlotAudioItem;
	}

	public void DisactiveAugment()
	{
		previewEffect.SetDescription(LocalizationManager.GetTranslation($"UI_PREVIEW_EFFECT_AUGMENT_GROUP_{groupId}"));
		if (optionHolder.SelectedOption != null)
		{
			SelectedAugment.DisactiveAugment();
		}
	}
}
