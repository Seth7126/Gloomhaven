using System;
using SRDebugger.Gamepad;
using SRDebugger.VirtualMouse;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SRDebugger;

public class GamepadPointerController : MonoBehaviour
{
	private class Pointer : IDisposable
	{
		private Vector2 _position;

		private Vector2 _delta;

		private Vector2 _scrollWheel;

		private RectTransform _rect;

		private Image _image;

		private IVirtualMouse _virtualMouse;

		private Canvas _parentCanvas;

		private RectTransform _parentCanvasRectTransform;

		public RectTransform Rect => _rect;

		public Image Image => _image;

		public IVirtualMouse VirtualMouse => _virtualMouse;

		public bool IsValid => _virtualMouse != null;

		public Vector2 Position
		{
			get
			{
				return _position;
			}
			set
			{
				if (IsValid)
				{
					_position = value;
					_rect.position = _position;
					_virtualMouse.SetPosition(_position);
				}
			}
		}

		public Vector2 ScrollWheel
		{
			get
			{
				return _scrollWheel;
			}
			set
			{
				if (IsValid)
				{
					_scrollWheel = value;
					_virtualMouse.SetScrollWheel(_scrollWheel);
				}
			}
		}

		public Pointer(RectTransform pointerPrefab, Canvas pointerCanvas)
		{
			if (pointerPrefab == null)
			{
				Debug.LogError("Pointer prefab can not be NULL");
				return;
			}
			if (pointerCanvas == null)
			{
				Debug.LogError("Parent canvas can not be NULL");
				return;
			}
			_parentCanvas = pointerCanvas;
			_parentCanvasRectTransform = _parentCanvas.transform as RectTransform;
			_rect = UnityEngine.Object.Instantiate(pointerPrefab, _parentCanvasRectTransform);
			_image = _rect.GetComponent<Image>();
			_position = _rect.anchoredPosition;
			_virtualMouse = SRDebug.Instance.VirtualMouse;
			if (_virtualMouse == null || !_virtualMouse.TryStart())
			{
				Debug.LogWarning("Vitual mouse is null or has errors.");
				_virtualMouse = null;
			}
		}

		public void Dispose()
		{
			UnityEngine.Object.Destroy(_rect.gameObject);
			_image = null;
			if (IsValid)
			{
				_virtualMouse.Stop();
			}
		}

		public void LeftPressed()
		{
			if (IsValid)
			{
				_virtualMouse.Press();
			}
		}

		public void LeftReleased()
		{
			if (IsValid)
			{
				_virtualMouse.Release();
			}
		}
	}

	[SerializeField]
	[Range(0f, 1f)]
	private float inputDeadZone;

	[SerializeField]
	private float minSpeed;

	[SerializeField]
	private float maxSpeed;

	[SerializeField]
	private float toFullSpeedTime;

	[SerializeField]
	private AnimationCurve accelerationCurve;

	[SerializeField]
	private RectTransform pointerPrefab;

	[SerializeField]
	private Canvas parentCanvas;

	[SerializeField]
	private Color minSpeedColor;

	[SerializeField]
	private Color maxSpeedColor;

	private RectTransform _parentCanvasRectTransform;

	private float _movingTime;

	private EventSystem _eventSystem;

	private bool _defaultsendNavigationEvents;

	private Pointer _pointer;

	private Vector2? _previousPointerPosition;

	private IGamepadButton _buttonSouth;

	private void OnEnable()
	{
		_parentCanvasRectTransform = parentCanvas.transform as RectTransform;
		_eventSystem = EventSystem.current;
		_defaultsendNavigationEvents = _eventSystem.sendNavigationEvents;
		_eventSystem.sendNavigationEvents = false;
		if (_pointer == null)
		{
			_pointer = new Pointer(pointerPrefab, parentCanvas);
		}
		_pointer.Position = _previousPointerPosition ?? (_parentCanvasRectTransform.anchoredPosition + _parentCanvasRectTransform.rect.center);
		InputSystem.onAfterUpdate += OnUpdate;
	}

	private void OnDisable()
	{
		InputSystem.onAfterUpdate -= OnUpdate;
		_eventSystem.sendNavigationEvents = _defaultsendNavigationEvents;
		if (_pointer != null)
		{
			_previousPointerPosition = _pointer.Position;
			_pointer.Dispose();
			_pointer = null;
		}
	}

	private void OnUpdate()
	{
		if (_pointer == null)
		{
			return;
		}
		IGamepad gamepad = SRDebug.Instance.Gamepad;
		if (gamepad != null && gamepad.IsValid())
		{
			if (_buttonSouth == null)
			{
				_buttonSouth = gamepad.GetGamepadButton("buttonSouth");
			}
			Vector2 input = UpdateRawMoveInput(gamepad);
			input = ApplyDeadZone(input);
			_movingTime = UpdateMovingTime(_movingTime, input);
			Vector2 vector = CalculateMoveOffset(input, _movingTime);
			Vector2 position = ClampPositionToRectTransform(_pointer.Position + vector, _parentCanvasRectTransform);
			_pointer.Position = position;
			_pointer.ScrollWheel = gamepad.GetRightStickValue();
			if (_buttonSouth.IsWasPressed())
			{
				_pointer.LeftPressed();
			}
			if (_buttonSouth.IsWasReleased())
			{
				_pointer.LeftReleased();
			}
		}
	}

	private Vector2 UpdateRawMoveInput(IGamepad gamepad)
	{
		return gamepad?.GetLeftStickValue() ?? Vector2.zero;
	}

	private Vector2 ApplyDeadZone(Vector2 input)
	{
		if (Mathf.Abs(input.x) <= inputDeadZone)
		{
			input.x = 0f;
		}
		if (Mathf.Abs(input.y) <= inputDeadZone)
		{
			input.y = 0f;
		}
		return input;
	}

	private float UpdateMovingTime(float movingTime, Vector2 input)
	{
		movingTime += Time.deltaTime;
		if (input.sqrMagnitude <= inputDeadZone * inputDeadZone)
		{
			movingTime = 0f;
		}
		return movingTime;
	}

	private Vector2 CalculateMoveOffset(Vector2 input, float movingTime)
	{
		float t = accelerationCurve.Evaluate(movingTime / toFullSpeedTime);
		float num = Mathf.Lerp(minSpeed, maxSpeed, t);
		_pointer.Image.color = Color.Lerp(minSpeedColor, maxSpeedColor, t);
		return input * num * Time.deltaTime;
	}

	private Vector2 ClampPositionToRectTransform(Vector2 worldPosition, RectTransform rectTransform)
	{
		Rect rect = rectTransform.rect;
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		Vector2 vector = anchoredPosition + rect.max;
		Vector2 vector2 = anchoredPosition + rect.min;
		worldPosition.x = Mathf.Clamp(worldPosition.x, vector2.x, vector.x);
		worldPosition.y = Mathf.Clamp(worldPosition.y, vector2.y, vector.y);
		return worldPosition;
	}

	private Vector2 ClampPositionToScreen(Vector2 worldPosition)
	{
		worldPosition.x = Mathf.Clamp(worldPosition.x, 0f, Screen.width);
		worldPosition.y = Mathf.Clamp(worldPosition.y, 0f, Screen.height);
		return worldPosition;
	}
}
