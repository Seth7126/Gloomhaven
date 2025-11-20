using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine.UI;

public class MultiElementPickController : ElementPickController
{
	private Action<List<ElementInfusionBoardManager.EElement>> onSelected;

	private List<IElementHolder> elementUI;

	private List<IElementHolder> anyElementUI;

	private Action<int, ElementInfusionBoardManager.EElement> onSelectedElement;

	private Action onCancelled;

	public MultiElementPickController(UIElementPicker picker)
		: base(picker)
	{
	}

	public MultiElementPickController SetOnSelectedAllElements(Action<List<ElementInfusionBoardManager.EElement>> onSelected)
	{
		this.onSelected = onSelected;
		return this;
	}

	public MultiElementPickController SetOnSelectedElement(Action<int, ElementInfusionBoardManager.EElement> onSelectedElement)
	{
		this.onSelectedElement = onSelectedElement;
		return this;
	}

	public MultiElementPickController SetOnCancelled(Action onCancelled)
	{
		this.onCancelled = onCancelled;
		return this;
	}

	public void Setup(List<IElementHolder> elementUI)
	{
		this.elementUI = elementUI;
		anyElementUI = elementUI.FindAll((IElementHolder it) => it.RequiredElement == ElementInfusionBoardManager.EElement.Any);
	}

	private void OnClosePicker()
	{
		UIWindowManager.UnregisterEscapable(this);
		if (!AreAllAnySelected())
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
			anyElementUI[num].SetSelectedElement(element);
			onSelectedElement?.Invoke(num, element);
		}
		else
		{
			int index = anyElementUI.FindIndex((IElementHolder it) => it.SelectedElement == element);
			anyElementUI[index].SetSelectedElement(null);
		}
	}

	private void OnSelectedAll(List<ElementInfusionBoardManager.EElement> elements)
	{
		for (int i = 0; i < anyElementUI.Count; i++)
		{
			anyElementUI[i].SetSelectedElement(elements[i]);
		}
		picker.Hide();
		UIWindowManager.UnregisterEscapable(this);
		if (Pick())
		{
			onSelected?.Invoke(elementUI.Select((IElementHolder it) => it.SelectedElement.Value).ToList());
		}
	}

	public void Cancel()
	{
		bool flag = false;
		foreach (IElementHolder item in elementUI)
		{
			flag |= item.SelectedElement.HasValue;
			item.SetSelectedElement(null);
		}
		if (flag)
		{
			onCancelled?.Invoke();
		}
	}

	public bool AreAllAnySelected()
	{
		return !anyElementUI.Exists((IElementHolder it) => !it.SelectedElement.HasValue);
	}

	public override bool Pick()
	{
		foreach (IElementHolder item in elementUI)
		{
			if (item.RequiredElement == ElementInfusionBoardManager.EElement.Any)
			{
				if (!item.SelectedElement.HasValue)
				{
					picker.Init(OnSelectedAll, excludedElements, isInfusion: false, anyElementUI.Count, OnSelectedElement, onOpenPicker, OnClosePicker);
					UIWindowManager.RegisterEscapable(this);
					picker.Show();
					return false;
				}
			}
			else if (ElementInfusionBoardManager.ElementColumn(item.RequiredElement) == ElementInfusionBoardManager.EColumn.Inert || InfusionBoardUI.Instance.IsElementReserved(item.RequiredElement))
			{
				Debug.LogError("An active bonus with consume element is enabled when missing its infusion " + item.RequiredElement);
				return false;
			}
		}
		foreach (IElementHolder item2 in elementUI)
		{
			if (item2.RequiredElement != ElementInfusionBoardManager.EElement.Any)
			{
				item2.SetSelectedElement(item2.RequiredElement);
			}
		}
		return true;
	}
}
