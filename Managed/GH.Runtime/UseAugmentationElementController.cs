using ScenarioRuleLibrary;

public class UseAugmentationElementController : IElementHolder
{
	private UIUseAugmentationElement augmentUI;

	public ElementInfusionBoardManager.EElement RequiredElement { get; private set; }

	public ElementInfusionBoardManager.EElement? SelectedElement { get; private set; }

	public UseAugmentationElementController(UIUseAugmentationElement augmentUI, ElementInfusionBoardManager.EElement elementTarget)
	{
		this.augmentUI = augmentUI;
		RequiredElement = elementTarget;
		augmentUI.Init(UIInfoTools.Instance.GetElementUseSprite(elementTarget), elementTarget == ElementInfusionBoardManager.EElement.Any);
		SetSelectedElement(null);
	}

	public void SetSelectedElement(ElementInfusionBoardManager.EElement? element)
	{
		SelectedElement = element;
		augmentUI.SetSelected(element.HasValue ? UIInfoTools.Instance.GetElementPickerSprite(element.Value) : null);
	}

	public void ClearSelection()
	{
		SetSelectedElement(null);
	}
}
