using System.Collections.Generic;
using System.Linq;
using SM.Utils;

namespace SM.Gamepad;

public static class UiNavigationElementsUtils
{
	private static List<IUiNavigationNode> _pathRevertBuffer = new List<IUiNavigationNode>();

	public static void InitElements(List<IUiNavigationElement> elements, IUiNavigationManager uiNavigationManager, IUiNavigationRoot root, HashSet<IUiNavigationElement> knownElements, Dictionary<string, IUiNavigationElement> elementsByNameMap, Dictionary<IUiNavigationSelectable, List<IUiNavigationNode>> knownSelectablesMap)
	{
		foreach (IUiNavigationElement element in elements)
		{
			element.InitializeNavigationElement(uiNavigationManager, root, knownElements, elementsByNameMap, knownSelectablesMap);
		}
	}

	public static void CollectAllSelectables(IUiNavigationNode navigationNode, List<IUiNavigationSelectable> result)
	{
		result.Clear();
		FindAllSelectablesRecursively(navigationNode, result);
		static void FindAllSelectablesRecursively(IUiNavigationNode currentNode, List<IUiNavigationSelectable> resultBuffer)
		{
			foreach (IUiNavigationElement element in currentNode.Elements)
			{
				if (element is IUiNavigationSelectable item)
				{
					resultBuffer.Add(item);
				}
				if (element is IUiNavigationNode currentNode2)
				{
					FindAllSelectablesRecursively(currentNode2, resultBuffer);
				}
			}
		}
	}

	public static void MarkNameOnMap(IUiNavigationElement element, Dictionary<string, IUiNavigationElement> map)
	{
		if (!string.IsNullOrEmpty(element.NavigationName) && !map.TryAdd(element.NavigationName, element))
		{
			LogUtils.LogError("Failed to add " + element.NavigationName + " " + element.GameObject.name + " " + map[element.NavigationName].GameObject.name);
		}
	}

	public static void MarkSelectableOnMap(IUiNavigationElement element, Dictionary<IUiNavigationSelectable, List<IUiNavigationNode>> map)
	{
		if (element is IUiNavigationSelectable uiNavigationSelectable)
		{
			List<IUiNavigationNode> value = FindNavigationPathTo(uiNavigationSelectable);
			map.Add(uiNavigationSelectable, value);
		}
	}

	internal static void InsertElementsBySiblingIndex<T>(List<T> elements, T newElement) where T : IUiNavigationElement
	{
		int i;
		for (i = 0; i < elements.Count && elements[i].RectTransform.GetSiblingIndex() <= newElement.RectTransform.GetSiblingIndex(); i++)
		{
		}
		elements.Insert(i, newElement);
	}

	public static void InsertElementsByPriorityDescending<T>(List<T> elements, T newElement) where T : IUiNavigationElement
	{
		int i;
		for (i = 0; i < elements.Count && elements[i].NavigationPriority >= newElement.NavigationPriority; i++)
		{
		}
		elements.Insert(i, newElement);
	}

	public static void InsertElementsByPriorityAscending<T>(List<T> elements, T newElement) where T : IUiNavigationElement
	{
		int i;
		for (i = 0; i < elements.Count && elements[i].NavigationPriority <= newElement.NavigationPriority; i++)
		{
		}
		elements.Insert(i, newElement);
	}

	public static void SortElementsBySiblingIndex<T>(List<T> elements) where T : IUiNavigationElement
	{
		elements.Sort((T x, T y) => x.RectTransform.GetSiblingIndex().CompareTo(y.RectTransform.GetSiblingIndex()));
	}

	public static void SortElementsByPriorityDescending<T>(List<T> elements) where T : IUiNavigationElement
	{
		elements.Sort((T x, T y) => -1 * x.NavigationPriority.CompareTo(y.NavigationPriority));
	}

	public static void SortElementsByPriorityAscending<T>(List<T> elements) where T : IUiNavigationElement
	{
		elements.Sort((T x, T y) => x.NavigationPriority.CompareTo(y.NavigationPriority));
	}

	public static IUiNavigationElement GetNextWithHigherPriority(IUiNavigationNode inNode, int thenPriority)
	{
		return inNode.ElementsByPriorityAscending.FirstOrDefault((IUiNavigationElement element) => element.NavigationPriority > thenPriority);
	}

	public static IUiNavigationElement GetNextWithLowerPriority(IUiNavigationNode inNode, int thenPriority)
	{
		return inNode.ElementsByPriorityDescending.FirstOrDefault((IUiNavigationElement element) => element.NavigationPriority < thenPriority);
	}

	private static List<IUiNavigationNode> FindNavigationPathTo(IUiNavigationSelectable selectable)
	{
		_pathRevertBuffer.Clear();
		for (IUiNavigationNode parent = selectable.Parent; parent != null; parent = parent.Parent)
		{
			_pathRevertBuffer.Add(parent);
		}
		List<IUiNavigationNode> list = new List<IUiNavigationNode>();
		for (int num = _pathRevertBuffer.Count - 1; num >= 0; num--)
		{
			list.Add(_pathRevertBuffer[num]);
		}
		return list;
	}

	public static UiNavigationGroup FindGroupForObjectSlow(UiNavigationBase navigationBase)
	{
		if (navigationBase.transform.parent == null)
		{
			return null;
		}
		return navigationBase.transform.parent.GetComponentInParent<UiNavigationGroup>(includeInactive: true);
	}
}
