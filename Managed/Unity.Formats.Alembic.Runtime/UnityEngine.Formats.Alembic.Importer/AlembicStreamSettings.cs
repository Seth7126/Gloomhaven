using System;
using UnityEngine.Formats.Alembic.Sdk;

namespace UnityEngine.Formats.Alembic.Importer;

[Serializable]
public class AlembicStreamSettings
{
	internal class AlembicStreamSettingsCopier : ScriptableObject
	{
		public AlembicStreamSettings abcSettings;
	}

	[SerializeField]
	private NormalsMode normals = NormalsMode.CalculateIfMissing;

	[SerializeField]
	private TangentsMode tangents = TangentsMode.Calculate;

	[SerializeField]
	private AspectRatioMode cameraAspectRatio = AspectRatioMode.CameraAperture;

	[SerializeField]
	private bool importVisibility = true;

	[SerializeField]
	private float scaleFactor = 0.01f;

	[SerializeField]
	private bool swapHandedness = true;

	[SerializeField]
	private bool flipFaces;

	[SerializeField]
	private bool interpolateSamples = true;

	[SerializeField]
	private bool importPointPolygon = true;

	[SerializeField]
	private bool importLinePolygon = true;

	[SerializeField]
	private bool importTrianglePolygon = true;

	[SerializeField]
	private bool importXform = true;

	[SerializeField]
	private bool importCameras = true;

	[SerializeField]
	private bool importMeshes = true;

	[SerializeField]
	private bool importPoints;

	[SerializeField]
	private bool importCurves = true;

	[SerializeField]
	private bool createCurveRenderers;

	public NormalsMode Normals
	{
		get
		{
			return normals;
		}
		set
		{
			normals = value;
		}
	}

	public TangentsMode Tangents
	{
		get
		{
			return tangents;
		}
		set
		{
			tangents = value;
		}
	}

	internal AspectRatioMode CameraAspectRatio
	{
		get
		{
			return cameraAspectRatio;
		}
		set
		{
			cameraAspectRatio = value;
		}
	}

	public bool ImportVisibility
	{
		get
		{
			return importVisibility;
		}
		set
		{
			importVisibility = value;
		}
	}

	public float ScaleFactor
	{
		get
		{
			return scaleFactor;
		}
		set
		{
			scaleFactor = value;
		}
	}

	public bool SwapHandedness
	{
		get
		{
			return swapHandedness;
		}
		set
		{
			swapHandedness = value;
		}
	}

	public bool FlipFaces
	{
		get
		{
			return flipFaces;
		}
		set
		{
			flipFaces = value;
		}
	}

	public bool InterpolateSamples
	{
		get
		{
			return interpolateSamples;
		}
		set
		{
			interpolateSamples = value;
		}
	}

	internal bool ImportPointPolygon
	{
		get
		{
			return importPointPolygon;
		}
		set
		{
			importPointPolygon = value;
		}
	}

	internal bool ImportLinePolygon
	{
		get
		{
			return importLinePolygon;
		}
		set
		{
			importLinePolygon = value;
		}
	}

	internal bool ImportTrianglePolygon
	{
		get
		{
			return importTrianglePolygon;
		}
		set
		{
			importTrianglePolygon = value;
		}
	}

	public bool ImportXform
	{
		get
		{
			return importXform;
		}
		set
		{
			importXform = value;
		}
	}

	public bool ImportCameras
	{
		get
		{
			return importCameras;
		}
		set
		{
			importCameras = value;
		}
	}

	public bool ImportMeshes
	{
		get
		{
			return importMeshes;
		}
		set
		{
			importMeshes = value;
		}
	}

	public bool ImportPoints
	{
		get
		{
			return importPoints;
		}
		set
		{
			importPoints = value;
		}
	}

	public bool ImportCurves
	{
		get
		{
			return importCurves;
		}
		set
		{
			importCurves = value;
		}
	}

	public bool CreateCurveRenderers
	{
		get
		{
			return createCurveRenderers;
		}
		set
		{
			createCurveRenderers = value;
		}
	}
}
