using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Utilities;

public class VirtualMouseUtilities
{
	protected Vector2 _position;

	protected Vector2 _scrollWheel;

	protected bool _isPressed;

	protected bool _isPress;

	protected bool _isReleased;

	private bool _isListener;

	public event Action<VirtualMouseUtilities> OnTryStart;

	public event Action<VirtualMouseUtilities> OnStop;

	public VirtualMouseUtilities()
	{
		_position = new Vector2(0f, 0f);
		InputSystemUtilities.InitVirtualMouse(this);
	}

	public bool TryStart()
	{
		this.OnTryStart(this);
		bool num = InputSystemUtilities.GetCurrentVirtualMouse() == this;
		if (num)
		{
			InputSystem.onAfterUpdate += Update;
			_isListener = true;
		}
		return num;
	}

	public void Stop()
	{
		this.OnStop(this);
		if (_isListener)
		{
			_isListener = false;
			InputSystem.onAfterUpdate -= Update;
		}
	}

	public void Press()
	{
		_isPressed = true;
	}

	public void Release()
	{
		_isPressed = false;
		_isPress = false;
		_isReleased = true;
	}

	public void SetPosition(Vector2 position)
	{
		_position = position;
	}

	public Vector2 GetPosition()
	{
		return _position;
	}

	public void SetScrollWheel(Vector2 scrollWheel)
	{
		_scrollWheel = scrollWheel;
	}

	public Vector2 GetScrollWheel()
	{
		return _scrollWheel;
	}

	public bool IsWasPressed()
	{
		return _isPressed;
	}

	public bool IsPress()
	{
		return _isPress;
	}

	public bool IsWasReleased()
	{
		return _isReleased;
	}

	public void Update()
	{
		_isReleased = false;
		_isPressed = false;
	}
}
