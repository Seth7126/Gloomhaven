using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class UiNavigationJumpToClosestTagged : CustomizableNavigationHandlerBehaviour
{
	[SerializeField]
	private string _navigationTag;

	[SerializeField]
	private BaseNavigationFilter _additionalFilter;

	[SerializeField]
	private UiNavigationBase _closestToElement;

	private readonly List<IUiNavigationElement> _resultBuffer = new List<IUiNavigationElement>();

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		IUiNavigationManager uiNavigationManager = inNode.UiNavigationManager;
		uiNavigationManager.GetTaggedElements(_navigationTag, _resultBuffer, _additionalFilter.IsTrue);
		if (_resultBuffer.Count == 0)
		{
			return false;
		}
		IUiNavigationElement uiNavigationElement = _resultBuffer[0];
		float num = (uiNavigationElement.NavigationPosition - _closestToElement.NavigationPosition).magnitude;
		foreach (IUiNavigationElement item in _resultBuffer)
		{
			float magnitude = (item.NavigationPosition - _closestToElement.NavigationPosition).magnitude;
			if (magnitude < num)
			{
				uiNavigationElement = item;
				num = magnitude;
			}
		}
		if (!(uiNavigationElement is IUiNavigationSelectable uiNavigationSelectable))
		{
			if (uiNavigationElement is IUiNavigationNode navigationNode)
			{
				return uiNavigationManager.TrySelectFirstIn(navigationNode);
			}
			return false;
		}
		return uiNavigationManager.TrySelect(uiNavigationSelectable);
	}
}
