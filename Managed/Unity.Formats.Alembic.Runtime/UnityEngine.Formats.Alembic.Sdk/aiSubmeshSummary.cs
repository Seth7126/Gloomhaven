namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiSubmeshSummary
{
	public int splitIndex { get; set; }

	public int submeshIndex { get; set; }

	public int indexCount { get; set; }

	public aiTopology topology { get; set; }
}
