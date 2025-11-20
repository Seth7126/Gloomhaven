using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Utilities;

public static class InputSystemUtilities
{
	private static BasicControl _control = new BasicControl();

	private static VirtualMouseUtilities _currentVirtualMouse;

	public static void InitVirtualMouse(VirtualMouseUtilities mouse)
	{
		mouse.OnTryStart += TryEnableVirtualMouse;
		mouse.OnStop += DisableVirtualMouse;
	}

	private static void TryEnableVirtualMouse(VirtualMouseUtilities mouse)
	{
		if (_currentVirtualMouse == null)
		{
			_currentVirtualMouse = mouse;
		}
	}

	private static void DisableVirtualMouse(VirtualMouseUtilities mouse)
	{
		if (mouse == _currentVirtualMouse)
		{
			_currentVirtualMouse = null;
		}
	}

	public static VirtualMouseUtilities GetCurrentVirtualMouse()
	{
		return _currentVirtualMouse;
	}

	public static void EnableTouchpadInputSystem()
	{
		_control.Touchpad.Enable();
	}

	public static void EnableMouseInputSystem()
	{
		_control.Mouse.Enable();
	}

	public static void DisableTouchpadInputSystem()
	{
		_control.Touchpad.Disable();
	}

	public static void DisableMouseInputSystem()
	{
		_control.Mouse.Disable();
	}

	public static void EnableMouseCursor()
	{
		Cursor.visible = true;
	}

	public static void DisableMouseCursor()
	{
		Cursor.visible = false;
	}

	public static bool GetKey(Key code)
	{
		return Keyboard.current?[code].isPressed ?? false;
	}

	public static bool GetKeyDown(Key code)
	{
		return Keyboard.current?[code].wasPressedThisFrame ?? false;
	}

	public static bool GetKeyDown(string code)
	{
		return Keyboard.current?[code].IsPressed() ?? false;
	}

	public static bool GetKeyUp(Key code)
	{
		return Keyboard.current?[code].wasReleasedThisFrame ?? false;
	}

	private static bool CheckUseVirtualMouse()
	{
		return _currentVirtualMouse != null;
	}

	public static bool GetMouseButtionDown(MouseButton button)
	{
		if (CheckUseVirtualMouse())
		{
			if (button == MouseButton.Left && _currentVirtualMouse.IsWasPressed())
			{
				return true;
			}
			return false;
		}
		Mouse current = Mouse.current;
		if (current == null)
		{
			return false;
		}
		return button switch
		{
			MouseButton.Left => current.leftButton.wasPressedThisFrame, 
			MouseButton.Right => current.rightButton.wasPressedThisFrame, 
			MouseButton.Middle => current.middleButton.wasPressedThisFrame, 
			MouseButton.Forward => current.forwardButton.wasPressedThisFrame, 
			MouseButton.Back => current.backButton.wasPressedThisFrame, 
			_ => false, 
		};
	}

	public static bool GetMouseButtionUp(MouseButton button)
	{
		if (CheckUseVirtualMouse())
		{
			if (button == MouseButton.Left && _currentVirtualMouse.IsWasReleased())
			{
				return true;
			}
			return false;
		}
		Mouse current = Mouse.current;
		if (current == null)
		{
			return false;
		}
		return button switch
		{
			MouseButton.Left => current.leftButton.wasReleasedThisFrame, 
			MouseButton.Right => current.rightButton.wasReleasedThisFrame, 
			MouseButton.Middle => current.middleButton.wasReleasedThisFrame, 
			MouseButton.Forward => current.forwardButton.wasReleasedThisFrame, 
			MouseButton.Back => current.backButton.wasReleasedThisFrame, 
			_ => false, 
		};
	}

	public static bool GetMouseButtion(MouseButton button)
	{
		if (CheckUseVirtualMouse())
		{
			if (button == MouseButton.Left && _currentVirtualMouse.IsPress())
			{
				return true;
			}
			return false;
		}
		Mouse current = Mouse.current;
		if (current == null)
		{
			return false;
		}
		return button switch
		{
			MouseButton.Left => current.leftButton.isPressed, 
			MouseButton.Right => current.rightButton.isPressed, 
			MouseButton.Middle => current.middleButton.isPressed, 
			MouseButton.Forward => current.forwardButton.isPressed, 
			MouseButton.Back => current.backButton.isPressed, 
			_ => false, 
		};
	}

	public static Vector3 GetMousePosition()
	{
		if (CheckUseVirtualMouse())
		{
			return _currentVirtualMouse.GetPosition();
		}
		Mouse current = Mouse.current;
		if (current == null)
		{
			return Vector3.zero;
		}
		Vector2 vector = current.position.ReadValue();
		return new Vector3(vector.x, vector.y, 0f);
	}

	public static Vector2 GetMouseScroll()
	{
		if (CheckUseVirtualMouse())
		{
			return _currentVirtualMouse.GetScrollWheel();
		}
		return _control.Mouse.ScrollVector2.ReadValue<Vector2>().normalized;
	}

	public static MouseButton GetMouseButtonWithID(int id)
	{
		return id switch
		{
			0 => MouseButton.Left, 
			1 => MouseButton.Right, 
			2 => MouseButton.Middle, 
			3 => MouseButton.Forward, 
			4 => MouseButton.Back, 
			_ => MouseButton.Left, 
		};
	}

	public static string[] GetJoystickNames()
	{
		return Joystick.all.Select((Joystick x) => x.name).ToArray();
	}

	public static bool AnyKeyDown()
	{
		return Keyboard.current?.anyKey.wasPressedThisFrame ?? false;
	}

	public static bool AnyMouseButtonDown()
	{
		Mouse current = Mouse.current;
		if (current == null)
		{
			return false;
		}
		if (!current.leftButton.wasPressedThisFrame)
		{
			return current.rightButton.wasPressedThisFrame;
		}
		return true;
	}

	public static string InputKeyDown()
	{
		Keyboard current = Keyboard.current;
		if (current == null)
		{
			return string.Empty;
		}
		return current.anyKey.name;
	}

	public static int GetCountTouch()
	{
		return 3;
	}

	public static TouchState[] GetTouches()
	{
		return new TouchState[3]
		{
			_control.Touchpad.Touch0.ReadValue<TouchState>(),
			_control.Touchpad.Touch1.ReadValue<TouchState>(),
			_control.Touchpad.Touch2.ReadValue<TouchState>()
		};
	}
}
