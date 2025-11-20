using UnityEngine;

namespace SM.Gamepad;

public interface IUiNavigationPositionCalculator
{
	Vector2 CalculateNavigationPosition(IUiNavigationElement navigationElement);
}
