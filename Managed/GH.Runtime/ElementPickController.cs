using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine.UI;

public abstract class ElementPickController : PickController
{
	protected UIElementPicker picker;

	protected List<ElementInfusionBoardManager.EElement> excludedElements;

	protected ElementPickController(UIElementPicker picker)
	{
		this.picker = picker;
	}

	public ElementPickController SetExcludedElements(List<ElementInfusionBoardManager.EElement> excludedElements)
	{
		this.excludedElements = excludedElements;
		return this;
	}

	public override bool IsSelecting()
	{
		return picker.IsOpen;
	}

	public override void ClosePicker()
	{
		UIWindowManager.UnregisterEscapable(this);
		picker.Hide();
	}

	public void OnDestroy()
	{
		UIWindowManager.UnregisterEscapable(this);
	}
}
