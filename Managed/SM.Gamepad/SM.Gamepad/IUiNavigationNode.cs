using System.Collections.Generic;

namespace SM.Gamepad;

public interface IUiNavigationNode : IUiNavigationElement
{
	List<IUiNavigationElement> Elements { get; }

	List<IUiNavigationElement> ElementsByPriorityDescending { get; }

	List<IUiNavigationElement> ElementsByPriorityAscending { get; }

	bool AutoSelectFirstElement { get; }

	IUiNavigationSelectable DefaultElementToSelect { get; }

	void SetDefaultElementToSelect(UINavigationSelectable defaultElement, bool navigateToElementNextTime = true);

	bool IsNavigationSourceAllowed(UIActionBaseEventData uiActionBaseEventData);

	bool TryToNavigate(HashSet<IUiNavigationNode> handledNodes, UINavigationDirection navigationDirection);
}
