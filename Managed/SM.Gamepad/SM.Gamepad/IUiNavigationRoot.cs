using System;

namespace SM.Gamepad;

public interface IUiNavigationRoot : IUiNavigationNode, IUiNavigationElement
{
	event Action<IUiNavigationRoot> OnRootElementEnabledEvent;

	event Action<IUiNavigationRoot> OnRootElementDisabledEvent;
}
