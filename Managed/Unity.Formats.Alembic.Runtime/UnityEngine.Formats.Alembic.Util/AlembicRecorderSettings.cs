using System;
using UnityEngine.Formats.Alembic.Sdk;

namespace UnityEngine.Formats.Alembic.Util;

[Serializable]
public class AlembicRecorderSettings
{
	internal delegate void SetTargetBranch(GameObject go);

	internal delegate GameObject GetTargetBranch();

	[SerializeField]
	private string outputPath = "Output/Output.abc";

	[SerializeField]
	private AlembicExportOptions conf = new AlembicExportOptions();

	[SerializeField]
	private ExportScope scope;

	[SerializeField]
	private GameObject targetBranch;

	internal SetTargetBranch setTargetBranch;

	internal GetTargetBranch getTargetBranch;

	[SerializeField]
	[HideInInspector]
	private bool fixDeltaTime = true;

	[SerializeField]
	[Tooltip("Assume only GameObjects with a SkinnedMeshRenderer component change over time.")]
	private bool assumeNonSkinnedMeshesAreConstant = true;

	[SerializeField]
	private bool captureMeshRenderer = true;

	[SerializeField]
	private bool captureSkinnedMeshRenderer = true;

	[SerializeField]
	private bool captureParticleSystem;

	[SerializeField]
	private bool captureCamera = true;

	[SerializeField]
	private bool meshNormals = true;

	[SerializeField]
	private bool meshUV0 = true;

	[SerializeField]
	private bool meshUV1 = true;

	[SerializeField]
	private bool meshColors = true;

	[SerializeField]
	private bool meshSubmeshes = true;

	[SerializeField]
	private bool detailedLog;

	public string OutputPath
	{
		get
		{
			return outputPath;
		}
		set
		{
			outputPath = value;
		}
	}

	public AlembicExportOptions ExportOptions => conf;

	public ExportScope Scope
	{
		get
		{
			return scope;
		}
		set
		{
			scope = value;
		}
	}

	public GameObject TargetBranch
	{
		get
		{
			if (getTargetBranch == null)
			{
				return targetBranch;
			}
			return getTargetBranch();
		}
		set
		{
			if (setTargetBranch != null)
			{
				setTargetBranch(value);
			}
			else
			{
				targetBranch = value;
			}
		}
	}

	public bool FixDeltaTime
	{
		get
		{
			return fixDeltaTime;
		}
		set
		{
			fixDeltaTime = value;
		}
	}

	public bool AssumeNonSkinnedMeshesAreConstant
	{
		get
		{
			return assumeNonSkinnedMeshesAreConstant;
		}
		set
		{
			assumeNonSkinnedMeshesAreConstant = value;
		}
	}

	public bool CaptureMeshRenderer
	{
		get
		{
			return captureMeshRenderer;
		}
		set
		{
			captureMeshRenderer = value;
		}
	}

	public bool CaptureSkinnedMeshRenderer
	{
		get
		{
			return captureSkinnedMeshRenderer;
		}
		set
		{
			captureSkinnedMeshRenderer = value;
		}
	}

	internal bool CaptureParticleSystem
	{
		get
		{
			return captureParticleSystem;
		}
		set
		{
			captureParticleSystem = value;
		}
	}

	public bool CaptureCamera
	{
		get
		{
			return captureCamera;
		}
		set
		{
			captureCamera = value;
		}
	}

	public bool MeshNormals
	{
		get
		{
			return meshNormals;
		}
		set
		{
			meshNormals = value;
		}
	}

	public bool MeshUV0
	{
		get
		{
			return meshUV0;
		}
		set
		{
			meshUV0 = value;
		}
	}

	public bool MeshUV1
	{
		get
		{
			return meshUV1;
		}
		set
		{
			meshUV1 = value;
		}
	}

	public bool MeshColors
	{
		get
		{
			return meshColors;
		}
		set
		{
			meshColors = value;
		}
	}

	public bool MeshSubmeshes
	{
		get
		{
			return meshSubmeshes;
		}
		set
		{
			meshSubmeshes = value;
		}
	}

	public bool DetailedLog
	{
		get
		{
			return detailedLog;
		}
		set
		{
			detailedLog = value;
		}
	}
}
