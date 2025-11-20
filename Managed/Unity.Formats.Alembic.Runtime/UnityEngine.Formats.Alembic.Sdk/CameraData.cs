namespace UnityEngine.Formats.Alembic.Sdk;

internal struct CameraData
{
	public Bool visibility { get; set; }

	public float focalLength { get; set; }

	public Vector2 sensorSize { get; set; }

	public Vector2 lensShift { get; set; }

	public float nearClipPlane { get; set; }

	public float farClipPlane { get; set; }
}
