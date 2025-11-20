using System.Runtime.InteropServices;
using UnityEngine;

namespace AsmodeeNet.Utils;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MathUtils
{
	public static bool Approximately(float value1, float value2, float epsilon)
	{
		float num = value1 - value2;
		if (num >= 0f)
		{
			return num <= epsilon;
		}
		return num >= 0f - epsilon;
	}

	public static bool Approximately(Vector2 value1, Vector2 value2, float epsilon)
	{
		if (Approximately(value1.x, value2.x, epsilon))
		{
			return Approximately(value1.y, value2.y, epsilon);
		}
		return false;
	}

	public static bool Approximately(Rect value1, Rect value2, float epsilon)
	{
		if (Approximately(value1.x, value2.x, epsilon) && Approximately(value1.y, value2.y, epsilon) && Approximately(value1.width, value2.width, epsilon))
		{
			return Approximately(value1.height, value2.height, epsilon);
		}
		return false;
	}
}
