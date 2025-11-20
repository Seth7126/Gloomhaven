using UnityEngine;

public static class Vector3ColliderExtensions
{
	public static bool IsInside(this Vector3 p_Point, BoxCollider p_Box)
	{
		p_Point = p_Box.transform.InverseTransformPoint(p_Point) - p_Box.center;
		float num = p_Box.size.x * 0.5f;
		float num2 = p_Box.size.y * 0.5f;
		float num3 = p_Box.size.z * 0.5f;
		if (p_Point.x < num && p_Point.x > 0f - num && p_Point.y < num2 && p_Point.y > 0f - num2 && p_Point.z < num3)
		{
			return p_Point.z > 0f - num3;
		}
		return false;
	}

	public static bool IsInsideXZ(this Vector3 p_Point, BoxCollider p_Box)
	{
		p_Point = p_Box.transform.InverseTransformPoint(p_Point) - p_Box.center;
		float num = p_Box.size.x * 0.5f;
		float num2 = p_Box.size.z * 0.5f;
		if (p_Point.x < num && p_Point.x > 0f - num && p_Point.z < num2)
		{
			return p_Point.z > 0f - num2;
		}
		return false;
	}
}
