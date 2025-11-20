using UnityEngine;

namespace SM.Gamepad;

public static class ImageExtensions
{
	public static float GetMatchedWidth(float targetHeight, Sprite sprite)
	{
		return sprite.rect.width / (sprite.rect.height / targetHeight);
	}
}
