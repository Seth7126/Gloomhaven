using InControl;
using UnityEngine;
using UnityEngine.EventSystems;

public class InControlInputModuleExtended : InControlInputModule, IInputModulePointer
{
	private static InControlInputModuleExtended _instance;

	private int _IdTap;

	private PointerEventData _data;

	public static InControlInputModuleExtended Instance => _instance;

	protected override void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			base.Awake();
		}
		else
		{
			Debug.LogError("InControlInputModuleExtended has already been created.");
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_instance = null;
	}

	protected override void Start()
	{
		base.Start();
		if (PlatformLayer.Setting.UseTouchpad)
		{
			allowTouchInput = true;
		}
		allowMouseInput = true;
		_IdTap = 0;
		_data = new PointerEventData(base.eventSystem);
	}

	public GameObject GameObjectUnderPointer(int pointerId = -1)
	{
		return GetLastPointerEventData(pointerId)?.pointerCurrentRaycast.gameObject;
	}

	public static MoveDirection CalculateMoveDirection(float x, float y)
	{
		return BaseInputModule.DetermineMoveDirection(x, y);
	}

	public static MoveDirection CalculateMoveDirection(Vector2 dir, float deadZone = 0f)
	{
		return CalculateMoveDirection(dir.x, dir.y, deadZone);
	}

	public static MoveDirection CalculateMoveDirection(float x, float y, float deadZone)
	{
		return BaseInputModule.DetermineMoveDirection(x, y, deadZone);
	}
}
