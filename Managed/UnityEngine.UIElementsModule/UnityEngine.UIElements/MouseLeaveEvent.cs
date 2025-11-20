namespace UnityEngine.UIElements;

public class MouseLeaveEvent : MouseEventBase<MouseLeaveEvent>
{
	protected override void Init()
	{
		base.Init();
		LocalInit();
	}

	private void LocalInit()
	{
		base.propagation = EventPropagation.TricklesDown | EventPropagation.Cancellable | EventPropagation.IgnoreCompositeRoots;
	}

	public MouseLeaveEvent()
	{
		LocalInit();
	}
}
