namespace UnityEngine.UIElements;

public abstract class NavigationEventBase<T> : EventBase<T>, INavigationEvent where T : NavigationEventBase<T>, new()
{
	protected NavigationEventBase()
	{
		LocalInit();
	}

	protected override void Init()
	{
		base.Init();
		LocalInit();
	}

	private void LocalInit()
	{
		base.propagation = EventPropagation.Bubbles | EventPropagation.TricklesDown | EventPropagation.Cancellable | EventPropagation.SkipDisabledElements;
		base.propagateToIMGUI = false;
	}
}
