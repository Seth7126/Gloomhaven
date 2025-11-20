using UnityEngine;
using UnityEngine.EventSystems;

namespace Script.GUI.SMNavigation.Input;

public class StickInputListener : InputListener, IMoveHandler, IEventSystemHandler
{
	private enum Direction
	{
		Horizontal,
		Vertical
	}

	[SerializeField]
	private Direction _direction;

	private bool _registered;

	public void OnMove(AxisEventData eventData)
	{
		switch (eventData.moveDir)
		{
		case MoveDirection.Left:
			NextPrevious(nextPrev: false, horizontalVertical: true);
			break;
		case MoveDirection.Right:
			NextPrevious(nextPrev: true, horizontalVertical: true);
			break;
		case MoveDirection.Down:
			NextPrevious(nextPrev: false, horizontalVertical: false);
			break;
		case MoveDirection.Up:
			NextPrevious(nextPrev: true, horizontalVertical: false);
			break;
		}
	}

	private void NextPrevious(bool nextPrev, bool horizontalVertical)
	{
		if (_registered && horizontalVertical == (_direction == Direction.Horizontal))
		{
			if (nextPrev)
			{
				Next();
			}
			else
			{
				Previous();
			}
		}
	}

	public override void Register()
	{
		_registered = true;
	}

	public override void UnRegister()
	{
		_registered = false;
	}
}
