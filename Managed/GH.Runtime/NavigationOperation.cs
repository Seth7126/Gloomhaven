using Script.GUI.SMNavigation;

public static class NavigationOperation
{
	public static INavigationOperation ToPreviousState { get; } = new ToPreviousStateOperation();

	public static INavigationOperation ToPreviousNonMenuState { get; } = new ToNonMenuPreviousStateOperation();

	public static INavigationOperation Empty { get; } = new EmptyNavigationOperation();

	public static void ExecuteForDefaultMachine(this INavigationOperation operation)
	{
		operation.Execute(Singleton<UINavigation>.Instance.StateMachine);
	}
}
