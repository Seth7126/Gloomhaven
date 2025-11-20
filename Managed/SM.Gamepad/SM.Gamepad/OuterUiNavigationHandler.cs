using System.Collections.Generic;

namespace SM.Gamepad;

public class OuterUiNavigationHandler : UiNavigationHandler
{
	public OuterUiNavigationHandler(UiNavigationRoot root, UiNavigationGroup group)
		: base(root, group)
	{
	}

	public override bool TryPerformNavigation(HashSet<IUiNavigationNode> handledNodes, UINavigationDirection navigationDirection, IUiNavigationSelectable uiNavigationSelectable = null)
	{
		IUiNavigationNode uiNavigationNode = Group;
		bool flag = false;
		while (!flag)
		{
			uiNavigationNode = uiNavigationNode.Parent;
			if (uiNavigationNode == null)
			{
				return false;
			}
			if (handledNodes.Contains(uiNavigationNode))
			{
				return false;
			}
			switch (Group.OuterNavigationHandleType)
			{
			case NavigationHandleType.PositionBasedNavigation:
				flag = TryPerformDistanceBasedNavigation(handledNodes, uiNavigationNode as UiNavigationGroup, navigationDirection);
				break;
			case NavigationHandleType.DirectionBasedNavigation:
				flag = TryPerformDirectionBasedNavigation(handledNodes, uiNavigationNode as UiNavigationGroup, navigationDirection);
				break;
			case NavigationHandleType.PriorityBasedNavigation:
				flag = TryPerformPriorityBasedNavigation(handledNodes, uiNavigationNode as UiNavigationGroup, navigationDirection);
				break;
			}
			if (!flag)
			{
				handledNodes.Add(Group);
			}
		}
		return true;
	}
}
