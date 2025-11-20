namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aeCameraData
{
	public Bool visibility;

	public float nearClippingPlane;

	public float farClippingPlane;

	public float fieldOfView;

	public float aspectRatio;

	public float focusDistance;

	public float focalLength;

	public float aperture;

	public static aeCameraData defaultValue => new aeCameraData
	{
		nearClippingPlane = 0.3f,
		farClippingPlane = 1000f,
		fieldOfView = 60f,
		aspectRatio = 1.7777778f,
		focusDistance = 5f,
		focalLength = 0f,
		aperture = 2.4f
	};
}
