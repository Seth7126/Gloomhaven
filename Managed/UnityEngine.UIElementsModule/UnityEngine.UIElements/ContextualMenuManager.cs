namespace UnityEngine.UIElements;

public abstract class ContextualMenuManager
{
	internal bool displayMenuHandledOSX { get; set; }

	public abstract void DisplayMenuIfEventMatches(EventBase evt, IEventHandler eventHandler);

	public void DisplayMenu(EventBase triggerEvent, IEventHandler target)
	{
		DropdownMenu menu = new DropdownMenu();
		using (ContextualMenuPopulateEvent e = ContextualMenuPopulateEvent.GetPooled(triggerEvent, menu, target, this))
		{
			target?.SendEvent(e);
		}
		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
		{
			displayMenuHandledOSX = true;
		}
	}

	protected internal abstract void DoDisplayMenu(DropdownMenu menu, EventBase triggerEvent);
}
