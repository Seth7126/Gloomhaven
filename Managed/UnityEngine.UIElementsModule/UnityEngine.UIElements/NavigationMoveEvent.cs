namespace UnityEngine.UIElements;

public class NavigationMoveEvent : NavigationEventBase<NavigationMoveEvent>
{
	public enum Direction
	{
		None,
		Left,
		Up,
		Right,
		Down
	}

	public Direction direction { get; private set; }

	public Vector2 move { get; private set; }

	internal static Direction DetermineMoveDirection(float x, float y, float deadZone = 0.6f)
	{
		if (new Vector2(x, y).sqrMagnitude < deadZone * deadZone)
		{
			return Direction.None;
		}
		if (Mathf.Abs(x) > Mathf.Abs(y))
		{
			if (x > 0f)
			{
				return Direction.Right;
			}
			return Direction.Left;
		}
		if (y > 0f)
		{
			return Direction.Up;
		}
		return Direction.Down;
	}

	public static NavigationMoveEvent GetPooled(Vector2 moveVector)
	{
		NavigationMoveEvent navigationMoveEvent = EventBase<NavigationMoveEvent>.GetPooled();
		navigationMoveEvent.direction = DetermineMoveDirection(moveVector.x, moveVector.y);
		navigationMoveEvent.move = moveVector;
		return navigationMoveEvent;
	}

	protected override void Init()
	{
		base.Init();
		direction = Direction.None;
		move = Vector2.zero;
	}

	public NavigationMoveEvent()
	{
		Init();
	}
}
