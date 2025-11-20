using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal static class RuntimeEventDispatcher
{
	public static EventDispatcher Create()
	{
		return EventDispatcher.CreateForRuntime(new List<IEventDispatchingStrategy>
		{
			new NavigationEventDispatchingStrategy(),
			new PointerCaptureDispatchingStrategy(),
			new KeyboardEventDispatchingStrategy(),
			new PointerEventDispatchingStrategy(),
			new MouseEventDispatchingStrategy(),
			new DefaultDispatchingStrategy()
		});
	}
}
