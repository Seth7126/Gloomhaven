namespace UnityEngine.UIElements;

public class MouseEnterEvent : MouseEventBase<MouseEnterEvent>
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

	public MouseEnterEvent()
	{
		LocalInit();
	}
}
