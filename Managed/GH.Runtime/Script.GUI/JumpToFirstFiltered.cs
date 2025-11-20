using System.Collections.Generic;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using UnityEngine;

namespace Script.GUI;

public class JumpToFirstFiltered : CustomizableNavigationHandlerBehaviour
{
	[SerializeField]
	private UiNavigationGroup _navigationGroup;

	[SerializeField]
	private BaseNavigationFilter _filter;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		UiNavigationManager navigationManager = Singleton<UINavigation>.Instance.NavigationManager;
		IUiNavigationSelectable uiNavigationSelectable = ((!(_navigationGroup != null)) ? UiNavigationUtils.FindFirstSelectableFiltered(navigationManager.CurrentNavigationRoot.Elements, _filter.IsTrue) : UiNavigationUtils.FindFirstSelectableFiltered(_navigationGroup.Elements, _filter.IsTrue));
		return navigationManager.TrySelect(uiNavigationSelectable);
	}
}
