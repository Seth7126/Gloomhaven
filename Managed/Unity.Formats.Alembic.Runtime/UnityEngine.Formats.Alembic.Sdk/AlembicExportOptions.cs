using System;
using System.Runtime.InteropServices;

namespace UnityEngine.Formats.Alembic.Sdk;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public class AlembicExportOptions
{
	[SerializeField]
	private TimeSamplingType timeSamplingType;

	[HideInInspector]
	[SerializeField]
	private float frameRate = 30f;

	[SerializeField]
	private TransformType xformType = TransformType.TRS;

	[SerializeField]
	private Bool swapHandedness = true;

	[SerializeField]
	private Bool swapFaces = false;

	[SerializeField]
	private float scaleFactor = 100f;

	public TimeSamplingType TimeSamplingType
	{
		get
		{
			return timeSamplingType;
		}
		set
		{
			timeSamplingType = value;
		}
	}

	public float FrameRate
	{
		get
		{
			return frameRate;
		}
		set
		{
			frameRate = Mathf.Max(value, Mathf.Epsilon);
		}
	}

	public TransformType TranformType
	{
		get
		{
			return xformType;
		}
		set
		{
			xformType = value;
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

	public bool SwapFaces
	{
		get
		{
			return swapFaces;
		}
		set
		{
			swapFaces = value;
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
}
