using System.Collections.Generic;

namespace UnityEngine.Formats.Alembic.Importer;

internal static class VPMatrices
{
	private static Dictionary<Camera, Matrix4x4> s_currentVPMatrix = new Dictionary<Camera, Matrix4x4>();

	private static Dictionary<Camera, Matrix4x4> s_previousVPMatrix = new Dictionary<Camera, Matrix4x4>();

	private static int s_frameCount;

	public static Matrix4x4 Get(Camera camera)
	{
		if (Time.frameCount != s_frameCount)
		{
			SwapMatrixMap();
		}
		if (!camera)
		{
			return default(Matrix4x4);
		}
		if (!s_currentVPMatrix.TryGetValue(camera, out var value))
		{
			value = camera.nonJitteredProjectionMatrix * camera.worldToCameraMatrix;
			s_currentVPMatrix.Add(camera, value);
		}
		return value;
	}

	public static Matrix4x4 GetPrevious(Camera camera)
	{
		if (Time.frameCount != s_frameCount)
		{
			SwapMatrixMap();
		}
		if (s_previousVPMatrix.TryGetValue(camera, out var value))
		{
			return value;
		}
		return Get(camera);
	}

	private static void SwapMatrixMap()
	{
		Dictionary<Camera, Matrix4x4> dictionary = s_previousVPMatrix;
		s_previousVPMatrix = s_currentVPMatrix;
		dictionary.Clear();
		s_currentVPMatrix = dictionary;
		s_frameCount = Time.frameCount;
	}
}
