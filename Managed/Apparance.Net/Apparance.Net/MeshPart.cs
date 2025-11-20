namespace Apparance.Net;

public struct MeshPart
{
	public int Material;

	public int BaseVertex;

	public Vector3 MinBounds;

	public Vector3 MaxBounds;

	public int NumIndices;

	public int[] Indices;
}
