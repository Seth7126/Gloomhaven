namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiSampleSelector
{
	public ulong requestedIndex { get; set; }

	public double requestedTime { get; set; }

	public int requestedTimeIndexType { get; set; }
}
