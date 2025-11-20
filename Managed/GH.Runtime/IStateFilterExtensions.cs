using Code.State;
using Script.GUI.SMNavigation;

public static class IStateFilterExtensions
{
	public static bool IsCurrentStateValid(this IStateFilter filter)
	{
		if (filter != null && Singleton<UINavigation>.Instance.StateMachine.CurrentState != null)
		{
			return filter.IsValid(Singleton<UINavigation>.Instance.StateMachine.CurrentState);
		}
		return false;
	}
}
