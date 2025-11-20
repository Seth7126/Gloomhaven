using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Utilities;

namespace AsmodeeNet.UserInterface;

public class DefaultUINavigationInput : UINavigationInput
{
	private bool _horizontalAxisInUse;

	private bool _verticalAxisInUse;

	private BasicControl _basicControl;

	public DefaultUINavigationInput(BasicControl basicControl)
	{
		_basicControl = basicControl;
	}

	public void ProcessInput(UINavigationManager navigationManager)
	{
		StandaloneInputModule component = EventSystem.current.GetComponent<StandaloneInputModule>();
		bool flag = false;
		if (InputSystemUtilities.GetMouseButtionDown(MouseButton.Left))
		{
			navigationManager.LoseFocus();
			flag = true;
		}
		if (InputSystemUtilities.GetKeyDown(Key.Tab))
		{
			if (InputSystemUtilities.GetKey(Key.LeftShift) || InputSystemUtilities.GetKey(Key.RightShift))
			{
				navigationManager.MoveFocus(UINavigationManager.Direction.Previous);
			}
			else
			{
				navigationManager.MoveFocus(UINavigationManager.Direction.Next);
			}
			flag = true;
		}
		if (InputSystemUtilities.GetKeyDown(Key.NumpadMinus))
		{
			navigationManager.MoveFocus(UINavigationManager.Direction.Previous);
			flag = true;
		}
		if (InputSystemUtilities.GetKeyDown(Key.NumpadPlus))
		{
			navigationManager.MoveFocus(UINavigationManager.Direction.Next);
			flag = true;
		}
		if (InputSystemUtilities.GetKeyDown(component.submitButton))
		{
			navigationManager.ValidateFocusable();
			flag = true;
		}
		if (InputSystemUtilities.GetKeyDown(component.cancelButton))
		{
			navigationManager.CancelCurrentContext();
			flag = true;
		}
		if (InputSystemUtilities.GetKeyDown(Key.Slash))
		{
			navigationManager.FindFocusableInputFieldAndEnterEditMode();
			flag = true;
		}
		float num = _basicControl.Mouse.MoveXRaw.ReadValue<float>();
		if ((num < -0.1f || num > 0.1f) && !_horizontalAxisInUse)
		{
			_horizontalAxisInUse = true;
			if (num > 0f)
			{
				navigationManager.MoveFocus(UINavigationManager.Direction.Right);
			}
			else
			{
				navigationManager.MoveFocus(UINavigationManager.Direction.Left);
			}
			flag = true;
		}
		if (num == 0f)
		{
			_horizontalAxisInUse = false;
		}
		float num2 = _basicControl.Mouse.MoveYRaw.ReadValue<float>();
		if ((num2 < -0.1f || num2 > 0.1f) && !_verticalAxisInUse)
		{
			_verticalAxisInUse = true;
			if (num2 > 0f)
			{
				navigationManager.MoveFocus(UINavigationManager.Direction.Up);
			}
			else
			{
				navigationManager.MoveFocus(UINavigationManager.Direction.Down);
			}
			flag = true;
		}
		if (num2 == 0f)
		{
			_verticalAxisInUse = false;
		}
		if (!flag && InputSystemUtilities.AnyKeyDown())
		{
			string text = InputSystemUtilities.InputKeyDown();
			if (!string.IsNullOrEmpty(text))
			{
				navigationManager.HandleInputString(text);
				flag = true;
			}
		}
	}
}
