namespace UnityEngine.Formats.Alembic.Util;

internal static class Utils
{
	public static Matrix4x4 WorldNoScale(this Transform transform)
	{
		Quaternion rotation = transform.rotation;
		Vector3 position = transform.position;
		return Matrix4x4.TRS(Matrix4x4.Rotate(rotation).transpose.MultiplyPoint(-position), Quaternion.Inverse(rotation), new Vector3(1f, 1f, 1f));
	}
}
