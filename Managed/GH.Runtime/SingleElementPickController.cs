using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine.UI;

public class SingleElementPickController : ElementPickController
{
	private Action<ElementInfusionBoardManager.EElement> onSelected;

	private IElementHolder elementUI;

	public SingleElementPickController(UIElementPicker picker)
		: base(picker)
	{
	}

	public SingleElementPickController SetOnSelectedElement(Action<ElementInfusionBoardManager.EElement> onSelected)
	{
		this.onSelected = onSelected;
		return this;
	}

	public void Setup(IElementHolder elementUI)
	{
		this.elementUI = elementUI;
		if (elementUI.RequiredElement == ElementInfusionBoardManager.EElement.Any)
		{
			picker.Init(OnSelected, excludedElements, isInfusion: false, 1, null, onOpenPicker, onClosePicker);
		}
	}

	private void OnSelected(List<ElementInfusionBoardManager.EElement> elements)
	{
		elementUI.SetSelectedElement(elements[0]);
		picker.Hide();
		UIWindowManager.UnregisterEscapable(this);
		onSelected?.Invoke(elements[0]);
	}

	public override bool Pick()
	{
		if (elementUI.RequiredElement == ElementInfusionBoardManager.EElement.Any)
		{
			if (!elementUI.SelectedElement.HasValue)
			{
				List<ElementInfusionBoardManager.EElement> availableElements = InfusionBoardUI.Instance.GetAvailableElements();
				if (availableElements.Count == 1)
				{
					elementUI.SetSelectedElement(availableElements[0]);
					return true;
				}
				UIWindowManager.RegisterEscapable(this);
				picker.Show();
				return false;
			}
			return true;
		}
		if (ElementInfusionBoardManager.ElementColumn(elementUI.RequiredElement) == ElementInfusionBoardManager.EColumn.Inert || InfusionBoardUI.Instance.IsElementReserved(elementUI.RequiredElement))
		{
			Debug.LogError("An active bonus with consume element is enabled when missing its infusion " + elementUI.RequiredElement);
			return false;
		}
		elementUI.SetSelectedElement(elementUI.RequiredElement);
		return true;
	}
}
