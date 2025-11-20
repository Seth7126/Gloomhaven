using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

namespace SRDebugger.VirtualMouse;

public class DefaultVirtualMouse : IVirtualMouse
{
	private Mouse _virtualMouse;

	private Vector2 _position;

	private Vector2 _scrollWheel;

	public void Stop()
	{
		InputSystem.RemoveDevice(_virtualMouse);
		_virtualMouse = null;
	}

	public bool TryStart()
	{
		if (UnityEngine.InputSystem.Gamepad.current != null)
		{
			_virtualMouse = InputSystem.AddDevice<Mouse>("VirtualMouse");
			InputUser? inputUser = InputUser.FindUserPairedToDevice(UnityEngine.InputSystem.Gamepad.current);
			InputUser.PerformPairingWithDevice(_virtualMouse, inputUser.GetValueOrDefault());
			return true;
		}
		return false;
	}

	public void Press()
	{
		_virtualMouse.CopyState<MouseState>(out var state);
		state.WithButton(MouseButton.Left);
		InputState.Change(_virtualMouse, state);
	}

	public void Release()
	{
		_virtualMouse.CopyState<MouseState>(out var state);
		state.WithButton(MouseButton.Left, state: false);
		InputState.Change(_virtualMouse, state);
	}

	public void SetPosition(Vector2 position)
	{
		Vector2 state = position - _position;
		_position = position;
		InputState.Change(_virtualMouse.delta, state);
		InputState.Change(_virtualMouse.position, _position);
	}

	public void SetScrollWheel(Vector2 scrollWheel)
	{
		_scrollWheel = scrollWheel;
		InputState.Change(_virtualMouse.scroll, _scrollWheel);
	}
}
