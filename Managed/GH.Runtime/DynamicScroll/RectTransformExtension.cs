using UnityEngine;

namespace DynamicScroll;

public static class RectTransformExtension
{
	public static Vector2 GetSize(this RectTransform self)
	{
		return self.rect.size;
	}

	public static void SetSize(this RectTransform self, Vector2 newSize)
	{
		Vector2 pivot = self.pivot;
		Vector2 vector = newSize - self.rect.size;
		self.offsetMin -= new Vector2(vector.x * pivot.x, vector.y * pivot.y);
		self.offsetMax += new Vector2(vector.x * (1f - pivot.x), vector.y * (1f - pivot.y));
	}
}
