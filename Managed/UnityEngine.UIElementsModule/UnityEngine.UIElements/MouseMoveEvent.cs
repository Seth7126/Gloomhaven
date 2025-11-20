namespace UnityEngine.UIElements;

public class MouseMoveEvent : MouseEventBase<MouseMoveEvent>
{
	protected override void Init()
	{
		base.Init();
		LocalInit();
	}

	private void LocalInit()
	{
		base.propagation = EventPropagation.Bubbles | EventPropagation.TricklesDown | EventPropagation.Cancellable;
	}

	public MouseMoveEvent()
	{
		LocalInit();
	}

	public new static MouseMoveEvent GetPooled(Event systemEvent)
	{
		MouseMoveEvent mouseMoveEvent = MouseEventBase<MouseMoveEvent>.GetPooled(systemEvent);
		mouseMoveEvent.button = 0;
		return mouseMoveEvent;
	}

	internal static MouseMoveEvent GetPooled(PointerMoveEvent pointerEvent)
	{
		return MouseEventBase<MouseMoveEvent>.GetPooled(pointerEvent);
	}
}
