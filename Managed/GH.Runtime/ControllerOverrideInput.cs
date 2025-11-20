using UnityEngine;
using UnityEngine.EventSystems;

public class ControllerOverrideInput : BaseInput
{
	public override Vector2 mousePosition
	{
		get
		{
			if (!ControllerInputAreaManager.IsFocusedArea(EControllerInputAreaType.WorldMap))
			{
				return new Vector2(-1f, -1f);
			}
			return InputManager.CursorPosition;
		}
	}

	public override bool mousePresent
	{
		get
		{
			if (base.mousePresent && ControllerInputAreaManager.IsEnabled)
			{
				return ControllerInputAreaManager.IsFocusedArea(EControllerInputAreaType.WorldMap);
			}
			return false;
		}
	}
}
