using System;
using UnityEngine.Formats.Alembic.Sdk;

namespace UnityEngine.Formats.Alembic.Importer;

[DisallowMultipleComponent]
public class AlembicCurves : MonoBehaviour
{
	public delegate void OnUpdateDataHandler(AlembicCurves curves);

	private OnUpdateDataHandler update;

	public Vector3[] Positions => positionsList.GetArray();

	public int[] CurveOffsets => curveOffsets.GetArray();

	public Vector2[] UVs => uvs.GetArray();

	public float[] Widths => widths.GetArray();

	public Vector3[] Velocities => velocitiesList.GetArray();

	internal PinnedList<Vector3> positionsList { get; } = new PinnedList<Vector3>();

	internal PinnedList<int> curveOffsets { get; } = new PinnedList<int>();

	internal PinnedList<Vector2> uvs { get; } = new PinnedList<Vector2>();

	internal PinnedList<float> widths { get; } = new PinnedList<float>();

	internal PinnedList<Vector3> velocitiesList { get; } = new PinnedList<Vector3>();

	public event OnUpdateDataHandler OnUpdateData
	{
		add
		{
			update = (OnUpdateDataHandler)Delegate.Combine(update, value);
		}
		remove
		{
			update = (OnUpdateDataHandler)Delegate.Remove(update, value);
		}
	}

	internal void InvokeOnUpdate(AlembicCurves curves)
	{
		update?.Invoke(curves);
	}
}
