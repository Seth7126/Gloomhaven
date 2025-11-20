using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class UiNavigationJumpToFirstTagged : CustomizableNavigationHandlerBehaviour
{
	[SerializeField]
	private string _navigationTag;

	[SerializeField]
	private BaseNavigationFilter _additionalFilter;

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
		_resultBuffer.Clear();
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
