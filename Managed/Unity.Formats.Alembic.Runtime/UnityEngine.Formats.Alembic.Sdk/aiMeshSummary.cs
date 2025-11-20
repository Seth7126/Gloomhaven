namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiMeshSummary
{
	public aiTopologyVariance topologyVariance { get; set; }

	public Bool hasCounts { get; set; }

	public Bool hasIndsices { get; set; }

	public Bool hasPoints { get; set; }

	public Bool hasVelocities { get; set; }

	public Bool hasNormals { get; set; }

	public Bool hasTangents { get; set; }

	public Bool hasUV0 { get; set; }

	public Bool hasUV1 { get; set; }

	public Bool hasRgba { get; set; }

	public Bool hasRgb { get; set; }

	public Bool constantPoints { get; set; }

	public Bool constantVelocities { get; set; }

	public Bool constantNormals { get; set; }

	public Bool constantTangents { get; set; }

	public Bool constantUV0 { get; set; }

	public Bool constantUV1 { get; set; }

	public Bool constantRgba { get; set; }

	public Bool constantRgb { get; set; }
}
