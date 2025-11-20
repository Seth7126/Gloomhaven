using System;
using UnityEngine;

public static class RectTransformExtensions
{
	public static void SetLeft(this RectTransform rt, float left)
	{
		rt.offsetMin = new Vector2(left, rt.offsetMin.y);
	}

	public static void SetRight(this RectTransform rt, float right)
	{
		rt.offsetMax = new Vector2(0f - right, rt.offsetMax.y);
	}

	public static void SetTop(this RectTransform rt, float top)
	{
		rt.offsetMax = new Vector2(rt.offsetMax.x, 0f - top);
	}

	public static void SetBottom(this RectTransform rt, float bottom)
	{
		rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
	}

	public static Vector3 DeltaPositionToFitTheScreen(this RectTransform rectTransform, float margin = 0f)
	{
		Vector2 vector = new Vector2((float)Screen.width - margin, (float)Screen.height - margin);
		Vector2 vector2 = new Vector2(margin, margin);
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		if (array[0].x < vector2.x || array[0].y < vector2.y)
		{
			return new Vector3(Mathf.Max(0f, vector2.x - array[0].x), Mathf.Max(0f, vector2.y - array[0].y));
		}
		if (array[1].x < vector2.x || array[1].y > vector.y)
		{
			return new Vector3(Mathf.Max(0f, vector2.x - array[1].x), vector.y - array[1].y);
		}
		if (array[2].x > vector.x || array[2].y > vector.y)
		{
			return new Vector3(Mathf.Min(0f, vector.x - array[2].x), Mathf.Min(0f, vector.y - array[2].y));
		}
		if (array[3].x > vector.x || array[3].y < vector2.y)
		{
			return new Vector3(Mathf.Min(0f, vector.x - array[3].x), Mathf.Max(0f, vector2.y - array[3].y));
		}
		return Vector3.zero;
	}

	public static Vector3 DeltaPositionToFitTheScreen(this RectTransform rectTransform, Camera camera, float margin = 0f)
	{
		Vector2 vector = new Vector2((float)Screen.width - margin, (float)Screen.height - margin);
		Vector2 vector2 = new Vector2(margin, margin);
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		if (camera != null)
		{
			for (int i = 0; i < 4; i++)
			{
				array[i] = camera.WorldToScreenPoint(array[i]);
			}
		}
		if (array[0].x < vector2.x || array[0].y < vector2.y)
		{
			return new Vector3(Mathf.Max(0f, vector2.x - array[0].x), Mathf.Max(0f, vector2.y - array[0].y));
		}
		if (array[1].x < vector2.x || array[1].y > vector.y)
		{
			return new Vector3(Mathf.Max(0f, vector2.x - array[1].x), vector.y - array[1].y);
		}
		if (array[2].x > vector.x || array[2].y > vector.y)
		{
			return new Vector3(Mathf.Min(0f, vector.x - array[2].x), Mathf.Min(0f, vector.y - array[2].y));
		}
		if (array[3].x > vector.x || array[3].y < vector2.y)
		{
			return new Vector3(Mathf.Min(0f, vector.x - array[3].x), Mathf.Max(0f, vector2.y - array[3].y));
		}
		return Vector3.zero;
	}

	public static Vector3 DeltaWorldPositionToFitTheScreen(this RectTransform rectTransform, Camera camera, float marginX, float marginY)
	{
		Vector2 screenMax = new Vector2((float)Screen.width - marginX, (float)Screen.height - marginY);
		Vector2 screenMin = new Vector2(marginX, marginY);
		Vector3[] worldCorners = new Vector3[4];
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(worldCorners);
		Vector3 screenMaxWorld = camera.ScreenToWorldPoint(screenMax);
		Vector3 screenMinWorld = camera.ScreenToWorldPoint(screenMin);
		for (int i = 0; i < 4; i++)
		{
			array[i] = camera.WorldToScreenPoint(worldCorners[i]);
		}
		Vector3 result = Vector3.zero;
		GetDelta(array[0].x, array[0].y, 0, 3, 0, 1);
		GetDelta(array[1].x, array[1].y, 1, 2, 0, 1);
		GetDelta(array[2].x, array[2].y, 1, 2, 3, 1);
		GetDelta(array[3].x, array[3].y, 0, 3, 3, 2);
		return result;
		void GetDelta(float x, float y, int minXWorldCorner, int maxXWorldCorner, int minYWorldCorner, int maxYWorldCorner)
		{
			if (x < screenMin.x)
			{
				result.x = screenMinWorld.x - worldCorners[minXWorldCorner].x;
			}
			else if (x > screenMax.x)
			{
				result.x = screenMaxWorld.x - worldCorners[maxXWorldCorner].x;
			}
			if (y < screenMin.y)
			{
				result.y = screenMinWorld.y - worldCorners[minYWorldCorner].y;
			}
			else if (y > screenMax.y)
			{
				result.y = screenMaxWorld.y - worldCorners[maxYWorldCorner].y;
			}
		}
	}

	public static Vector3 DeltaWorldPositionToFitTheScreen(this RectTransform rectTransform, Camera camera, float margin = 0f)
	{
		Vector2 vector = new Vector2((float)Screen.width - margin, (float)Screen.height - margin);
		Vector2 vector2 = new Vector2(margin, margin);
		Vector3[] array = new Vector3[4];
		Vector3[] array2 = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Vector3 vector3 = camera.ScreenToWorldPoint(vector);
		Vector3 vector4 = camera.ScreenToWorldPoint(vector2);
		for (int i = 0; i < 4; i++)
		{
			array2[i] = camera.WorldToScreenPoint(array[i]);
		}
		if (array2[0].x < vector2.x || array2[0].y < vector2.y)
		{
			return new Vector3(Mathf.Max(0f, vector4.x - array[0].x), Mathf.Max(0f, vector4.y - array[0].y));
		}
		if (array2[1].x < vector2.x || array2[1].y > vector.y)
		{
			return new Vector3(Mathf.Max(0f, vector4.x - array[1].x), Mathf.Min(0f, vector3.y - array[1].y));
		}
		if (array2[2].x > vector.x || array2[2].y > vector.y)
		{
			return new Vector3(Mathf.Min(0f, vector3.x - array[2].x), Mathf.Min(0f, vector3.y - array[2].y));
		}
		if (array2[3].x > vector.x || array2[3].y < vector2.y)
		{
			return new Vector3(Mathf.Min(0f, vector3.x - array[3].x), Mathf.Max(0f, vector4.y - array[3].y));
		}
		return Vector3.zero;
	}

	public static Vector3 DeltaWorldPositionToFitRectTransform(this RectTransform rectTransform, Camera camera, RectTransform areaToFit, bool checkBothAxies = false)
	{
		Vector3[] array = new Vector3[4];
		Vector3[] array2 = new Vector3[4];
		Vector3[] array3 = new Vector3[4];
		rectTransform.GetWorldCorners(array2);
		areaToFit.GetWorldCorners(array);
		Vector2 vector = camera.WorldToScreenPoint(array[2]);
		Vector2 vector2 = camera.WorldToScreenPoint(array[0]);
		for (int i = 0; i < 4; i++)
		{
			array3[i] = camera.WorldToScreenPoint(array2[i]);
		}
		Vector3 result = Vector3.zero;
		if (array3[0].x < vector2.x || array3[0].y < vector2.y)
		{
			result = new Vector3(Mathf.Max(0f, array[0].x - array2[0].x), Mathf.Max(0f, array[0].y - array2[0].y));
			if (!checkBothAxies)
			{
				return result;
			}
		}
		if (array3[1].x < vector2.x || array3[1].y > vector.y)
		{
			Vector3 result2 = new Vector3(Mathf.Max(0f, array[0].x - array2[1].x), Mathf.Min(0f, array[2].y - array2[1].y));
			if (!checkBothAxies)
			{
				return result2;
			}
			result.x = ((Math.Abs(result.x) > Math.Abs(result2.x)) ? result.x : result2.x);
			result.y = ((Math.Abs(result.y) > Math.Abs(result2.y)) ? result.y : result2.y);
		}
		if (array3[2].x > vector.x || array3[2].y > vector.y)
		{
			Vector3 result3 = new Vector3(Mathf.Min(0f, array[2].x - array2[2].x), Mathf.Min(0f, array[2].y - array2[2].y));
			if (!checkBothAxies)
			{
				return result3;
			}
			result.x = ((Math.Abs(result.x) > Math.Abs(result3.x)) ? result.x : result3.x);
			result.y = ((Math.Abs(result.y) > Math.Abs(result3.y)) ? result.y : result3.y);
		}
		if (array3[3].x > vector.x || array3[3].y < vector2.y)
		{
			Vector3 result4 = new Vector3(Mathf.Min(0f, array[2].x - array2[3].x), Mathf.Max(0f, array[0].y - array2[3].y));
			if (!checkBothAxies)
			{
				return result4;
			}
			result.x = ((Math.Abs(result.x) > Math.Abs(result4.x)) ? result.x : result4.x);
			result.y = ((Math.Abs(result.y) > Math.Abs(result4.y)) ? result.y : result4.y);
		}
		return result;
	}

	public static bool FitsInRectTransform(this RectTransform rectTransform, Camera camera, RectTransform areaToFit)
	{
		Vector3[] array = new Vector3[4];
		Vector3[] array2 = new Vector3[4];
		Vector3[] array3 = new Vector3[4];
		rectTransform.GetWorldCorners(array2);
		areaToFit.GetWorldCorners(array);
		Vector2 vector = camera.WorldToScreenPoint(array[2]);
		Vector2 vector2 = camera.WorldToScreenPoint(array[0]);
		for (int i = 0; i < 4; i++)
		{
			array3[i] = camera.WorldToScreenPoint(array2[i]);
		}
		if (array3[0].x < vector2.x || array3[0].y < vector2.y)
		{
			return false;
		}
		if (array3[1].x < vector2.x || array3[1].y > vector.y)
		{
			return false;
		}
		if (array3[2].x > vector.x || array3[2].y > vector.y)
		{
			return false;
		}
		if (array3[3].x > vector.x || array3[3].y < vector2.y)
		{
			return false;
		}
		return true;
	}

	public static bool IsWidthInRectTransform(this RectTransform rectTransform, Camera camera, RectTransform areaToFit)
	{
		Vector3[] array = new Vector3[4];
		Vector3[] array2 = new Vector3[4];
		Vector3[] array3 = new Vector3[4];
		rectTransform.GetWorldCorners(array2);
		areaToFit.GetWorldCorners(array);
		Vector2 vector = camera.WorldToScreenPoint(array[2]);
		Vector2 vector2 = camera.WorldToScreenPoint(array[0]);
		for (int i = 0; i < 4; i++)
		{
			array3[i] = camera.WorldToScreenPoint(array2[i]);
		}
		if (array3[0].x < vector2.x || array3[2].x > vector.x)
		{
			return false;
		}
		return true;
	}
}
