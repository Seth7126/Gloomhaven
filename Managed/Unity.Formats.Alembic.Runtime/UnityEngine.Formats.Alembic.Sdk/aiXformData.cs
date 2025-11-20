namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiXformData
{
	public Bool visibility { get; set; }

	public Vector3 translation { get; set; }

	public Quaternion rotation { get; set; }

	public Vector3 scale { get; set; }

	public Bool inherits { get; set; }
}
