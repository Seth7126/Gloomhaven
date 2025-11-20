using SM.Gamepad;
using UnityEngine;

namespace Script.Extensions;

public static class NavigationExtensions
{
	public static UINavigationDirection ToNavigationDirection(this Vector2 vector)
	{
		if (vector.x > 0f)
		{
			return UINavigationDirection.Right;
		}
		if (vector.x < 0f)
		{
			return UINavigationDirection.Left;
		}
		if (vector.y > 0f)
		{
			return UINavigationDirection.Up;
		}
		return UINavigationDirection.Down;
	}
}
