using SM.Gamepad;

namespace Code.State;

public class MainStateData
{
	public UiNavigationRoot NavigationRoot { get; set; }

	public MainStateData(UiNavigationRoot navigationRoot)
	{
		NavigationRoot = navigationRoot;
	}
}
