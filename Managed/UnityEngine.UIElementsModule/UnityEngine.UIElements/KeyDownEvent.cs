namespace UnityEngine.UIElements;

public class KeyDownEvent : KeyboardEventBase<KeyDownEvent>
{
	internal void GetEquivalentImguiEvent(Event outImguiEvent)
	{
		if (base.imguiEvent != null)
		{
			outImguiEvent.CopyFrom(base.imguiEvent);
			return;
		}
		outImguiEvent.type = EventType.KeyDown;
		outImguiEvent.modifiers = base.modifiers;
		outImguiEvent.character = base.character;
		outImguiEvent.keyCode = base.keyCode;
	}
}
