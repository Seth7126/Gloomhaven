using System.Collections.Generic;
using System.Linq;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class UiNavigationSimpleInnerFixedWithTag : UiNavigationSimpleInnerFixed
{
	[SerializeField]
	private string _navigationTag;

	protected override void CollectElementsByDirection(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode node, List<IUiNavigationElement> elements, UINavigationDirection navigationDirection, List<IUiNavigationElement> result)
	{
		List<IUiNavigationElement> elements2 = elements.Where((IUiNavigationElement uiNavigationElement) => uiNavigationElement.NavigationTags.Contains(_navigationTag)).ToList();
		base.CollectElementsByDirection(proceededNodes, node, elements2, navigationDirection, result);
	}
}
