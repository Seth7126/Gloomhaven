using SM.Gamepad;

namespace Code.State;

public class SelectionStateData
{
	public IUiNavigationSelectable ConcreteSelectable { get; }

	public bool SelectFirst { get; }

	public bool SelectPrevious { get; }

	public SelectionStateData(IUiNavigationSelectable concreteSelectable = null, bool selectFirst = false, bool selectPrevious = false)
	{
		ConcreteSelectable = concreteSelectable;
		SelectFirst = selectFirst;
		SelectPrevious = selectPrevious;
	}
}
