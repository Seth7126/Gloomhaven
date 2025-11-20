using System.Collections.Generic;

namespace SM.Gamepad;

public static class UiNavigationElementsExtensions
{
	public static void InitializeNavigationElement(this IUiNavigationElement element, IUiNavigationManager uiNavigationManager, IUiNavigationRoot root, HashSet<IUiNavigationElement> knownElements, Dictionary<string, IUiNavigationElement> elementsByNameMap, Dictionary<IUiNavigationSelectable, List<IUiNavigationNode>> knownSelectablesMap)
	{
		if (!(element is UiNavigationRoot uiNavigationRoot))
		{
			if (!(element is UiNavigationGroup uiNavigationGroup))
			{
				if (element is UINavigationSelectable uINavigationSelectable)
				{
					uINavigationSelectable.InitializeSelectableElement(uiNavigationManager, root, knownElements, elementsByNameMap, knownSelectablesMap);
				}
			}
			else
			{
				uiNavigationGroup.InitializeNodeElement(uiNavigationManager, root, knownElements, elementsByNameMap, knownSelectablesMap);
			}
		}
		else
		{
			uiNavigationRoot.InitializeRootElement(uiNavigationManager, knownElements, elementsByNameMap, knownSelectablesMap);
		}
	}
}
