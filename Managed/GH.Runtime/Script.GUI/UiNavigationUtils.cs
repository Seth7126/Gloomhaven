using System;
using System.Collections.Generic;
using SM.Gamepad;

namespace Script.GUI;

public static class UiNavigationUtils
{
	public static IUiNavigationSelectable FindFirstSelectable(List<IUiNavigationElement> elements)
	{
		foreach (IUiNavigationElement element in elements)
		{
			if (element is IUiNavigationSelectable result)
			{
				return result;
			}
			if (element is IUiNavigationNode uiNavigationNode)
			{
				return FindFirstSelectable(uiNavigationNode.Elements);
			}
		}
		return null;
	}

	public static IUiNavigationSelectable FindFirstSelectableFiltered(List<IUiNavigationElement> elements, Func<IUiNavigationSelectable, bool> filterFunction)
	{
		IUiNavigationSelectable uiNavigationSelectable = null;
		foreach (IUiNavigationElement element in elements)
		{
			if (element is IUiNavigationSelectable uiNavigationSelectable2)
			{
				if (filterFunction(uiNavigationSelectable2))
				{
					uiNavigationSelectable = uiNavigationSelectable2;
					break;
				}
			}
			else if (element is IUiNavigationNode uiNavigationNode)
			{
				uiNavigationSelectable = FindFirstSelectableFiltered(uiNavigationNode.Elements, filterFunction);
				if (uiNavigationSelectable != null)
				{
					break;
				}
			}
		}
		return uiNavigationSelectable;
	}

	public static IUiNavigationElement FindFirstElementFiltered(List<IUiNavigationElement> elements, Func<IUiNavigationElement, bool> filterFunction)
	{
		IUiNavigationElement uiNavigationElement = null;
		foreach (IUiNavigationElement element in elements)
		{
			if (element is IUiNavigationSelectable uiNavigationSelectable)
			{
				if (filterFunction(uiNavigationSelectable))
				{
					uiNavigationElement = uiNavigationSelectable;
					break;
				}
			}
			else if (element is IUiNavigationNode uiNavigationNode)
			{
				if (filterFunction(uiNavigationNode))
				{
					uiNavigationElement = uiNavigationNode;
					break;
				}
				uiNavigationElement = FindFirstElementFiltered(uiNavigationNode.Elements, filterFunction);
				if (uiNavigationElement != null)
				{
					break;
				}
			}
		}
		return uiNavigationElement;
	}

	public static bool TryGetComponentInParents<T>(this IUiNavigationElement element, out T component)
	{
		IUiNavigationElement uiNavigationElement = element;
		component = default(T);
		do
		{
			component = uiNavigationElement.GameObject.GetComponent<T>();
			uiNavigationElement = uiNavigationElement.Parent;
		}
		while (component == null && uiNavigationElement != null);
		return component != null;
	}
}
