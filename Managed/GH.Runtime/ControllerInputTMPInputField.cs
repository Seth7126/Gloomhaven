using UnityEngine.EventSystems;

public class ControllerInputTMPInputField : ControllerInputElement, IUpdateSelectedHandler, IEventSystemHandler
{
	public void OnUpdateSelected(BaseEventData eventData)
	{
		if (isEnabled)
		{
			eventData.Reset();
		}
	}
}
