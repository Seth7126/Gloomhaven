using System;
using UnityEngine;

namespace SM.Gamepad;

[Serializable]
public class HotkeyViewConfig
{
	public float _widthBound = 100f;

	public float _heightBound = 36f;

	public float _minHeight = 36f;

	public float _minWidth = 36f;

	public float _maxHeight = 36f;

	public float _maxWidth = 150f;

	public Vector2 GetOverridenSize(Vector2 targetSize)
	{
		return new Vector2(GetOverridenWidth(targetSize.x), GetOverridenHeight(targetSize.y));
	}

	public Vector2 GetOverridenSize(float width, float height)
	{
		return new Vector2(GetOverridenWidth(width), GetOverridenHeight(height));
	}

	public Vector2 GetOverridenSize(Rect rect)
	{
		return new Vector2(GetOverridenWidth(rect.width), GetOverridenHeight(rect.height));
	}

	public float GetOverridenWidth(float width)
	{
		if (!(width > _widthBound))
		{
			return _minWidth;
		}
		return Mathf.Min(width, _maxWidth);
	}

	public float GetOverridenHeight(float height)
	{
		if (!(height > _heightBound))
		{
			return _minHeight;
		}
		return Mathf.Min(height, _maxHeight);
	}
}
