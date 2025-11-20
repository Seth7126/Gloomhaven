using UnityEngine;

namespace Script.Extensions;

public static class Vector3Extensions
{
	public static void Clamp(this ref Vector3 vector, in Vector3 min, in Vector3 max)
	{
		vector.x = Mathf.Clamp(vector.x, min.x, max.x);
		vector.y = Mathf.Clamp(vector.y, min.y, max.y);
		vector.z = Mathf.Clamp(vector.z, min.z, max.z);
	}
}
