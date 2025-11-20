namespace SM.Gamepad;

public interface IUiNavigationTransitMarkableNode : IUiNavigationNode, IUiNavigationElement
{
	void OnNavigationTransitMarked(IUiNavigationSelectable selectedChild);

	void OnNavigationTransitUnmarked(IUiNavigationSelectable deselectedChild);
}
