namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiMeshSampleSummary
{
	public Bool visibility { get; set; }

	public int splitCount { get; set; }

	public int submeshCount { get; set; }

	public int vertexCount { get; set; }

	public int indexCount { get; set; }

	public Bool topologyChanged { get; set; }
}
