using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine.UI;

public class InfuseElementPickController : ElementPickController
{
	private Action<List<ElementInfusionBoardManager.EElement>> onSelected;

	private List<IElementHolder> elementsUI;

	private List<IElementHolder> anyElementUI;

	private Action<int, ElementInfusionBoardManager.EElement> onSelectedElement;

	private Func<bool> cancelCheck;

	public InfuseElementPickController(UIElementPicker picker)
		: base(picker)
	{
	}

	public InfuseElementPickController SetOnSelectedAllElements(Action<List<ElementInfusionBoardManager.EElement>> onSelected)
	{
		this.onSelected = onSelected;
		return this;
	}

	public InfuseElementPickController SetOnSelectedElement(Action<int, ElementInfusionBoardManager.EElement> onSelectedElement)
	{
		this.onSelectedElement = onSelectedElement;
		return this;
	}

	public InfuseElementPickController SetOnCancelCondition(Func<bool> cancelCheck)
	{
		this.cancelCheck = cancelCheck;
		return this;
	}

	public void Setup(List<IElementHolder> elementUI)
	{
		elementsUI = elementUI;
		anyElementUI = elementUI.FindAll((IElementHolder it) => it.RequiredElement == ElementInfusionBoardManager.EElement.Any);
	}

	private void OnClosePicker()
	{
		UIWindowManager.UnregisterEscapable(this);
		if (!AreAllAnySelected() || (cancelCheck != null && cancelCheck()))
		{
			Cancel();
		}
		onClosePicker?.Invoke();
	}

	private void OnSelectedElement(ElementInfusionBoardManager.EElement element, bool selected)
	{
		if (selected)
		{
			int num = anyElementUI.FindIndex((IElementHolder it) => !it.SelectedElement.HasValue);
			if (num == -1)
			{
				num = anyElementUI.FindIndex((IElementHolder it) => it.SelectedElement == element);
			}
			anyElementUI[num].SetSelectedElement(element);
			onSelectedElement?.Invoke(num, element);
		}
		else
		{
			int index = anyElementUI.FindIndex((IElementHolder it) => it.SelectedElement == element);
			anyElementUI[index].SetSelectedElement(null);
		}
	}

	public void OnSelectedAll(List<ElementInfusionBoardManager.EElement> elements)
	{
		onSelected?.Invoke(elementsUI.Select((IElementHolder it) => it.SelectedElement.Value).ToList());
		ClosePicker();
	}

	private void Cancel()
	{
		foreach (IElementHolder item in elementsUI)
		{
			item.SetSelectedElement(null);
		}
	}

	public override bool Pick()
	{
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		foreach (IElementHolder item in elementsUI)
		{
			if (item.SelectedElement.HasValue)
			{
				list.Add(item.SelectedElement.Value);
			}
		}
		foreach (IElementHolder item2 in elementsUI)
		{
			if (item2.RequiredElement == ElementInfusionBoardManager.EElement.Any)
			{
				picker.Init(OnSelectedAll, excludedElements, isInfusion: true, anyElementUI.Count, OnSelectedElement, onOpenPicker, OnClosePicker, list);
				picker.Show();
				UIWindowManager.RegisterEscapable(this);
				return false;
			}
		}
		foreach (IElementHolder item3 in elementsUI)
		{
			if (item3.RequiredElement != ElementInfusionBoardManager.EElement.Any)
			{
				item3.SetSelectedElement(item3.RequiredElement);
			}
		}
		return true;
	}

	public bool AreAllAnySelected()
	{
		return !anyElementUI.Exists((IElementHolder it) => !it.SelectedElement.HasValue);
	}
}
