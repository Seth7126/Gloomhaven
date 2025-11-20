namespace UnityEngine.UIElements;

public sealed class ClickEvent : PointerEventBase<ClickEvent>
{
	protected override void Init()
	{
		base.Init();
		LocalInit();
	}

	private void LocalInit()
	{
		base.propagation = EventPropagation.Bubbles | EventPropagation.TricklesDown | EventPropagation.Cancellable | EventPropagation.SkipDisabledElements;
	}

	public ClickEvent()
	{
		LocalInit();
	}

	internal static ClickEvent GetPooled(PointerUpEvent pointerEvent, int clickCount)
	{
		ClickEvent clickEvent = PointerEventBase<ClickEvent>.GetPooled(pointerEvent);
		clickEvent.clickCount = clickCount;
		return clickEvent;
	}
}
