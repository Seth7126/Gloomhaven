namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiConfig
{
	public NormalsMode normalsMode { get; set; }

	public TangentsMode tangentsMode { get; set; }

	public float scaleFactor { get; set; }

	public float aspectRatio { get; set; }

	public float vertexMotionScale { get; set; }

	public int splitUnit { get; set; }

	public Bool swapHandedness { get; set; }

	public Bool flipFaces { get; set; }

	public Bool interpolateSamples { get; set; }

	public Bool importPointPolygon { get; set; }

	public Bool importLinePolygon { get; set; }

	public Bool importTrianglePolygon { get; set; }

	public void SetDefaults()
	{
		normalsMode = NormalsMode.CalculateIfMissing;
		tangentsMode = TangentsMode.None;
		scaleFactor = 0.01f;
		aspectRatio = -1f;
		vertexMotionScale = 1f;
		splitUnit = int.MaxValue;
		swapHandedness = true;
		flipFaces = false;
		interpolateSamples = true;
		importPointPolygon = true;
		importLinePolygon = true;
		importTrianglePolygon = true;
	}
}
