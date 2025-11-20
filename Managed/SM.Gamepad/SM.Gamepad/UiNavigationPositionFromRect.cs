using UnityEngine;

namespace SM.Gamepad;

public class UiNavigationPositionFromRect : IUiNavigationPositionCalculator
{
	private readonly RectTransform _rect;

	public UiNavigationPositionFromRect(RectTransform rect)
	{
		_rect = rect;
	}

	public Vector2 CalculateNavigationPosition(IUiNavigationElement navigationElement)
	{
		return _rect.TransformPoint(_rect.rect.center);
	}
}
