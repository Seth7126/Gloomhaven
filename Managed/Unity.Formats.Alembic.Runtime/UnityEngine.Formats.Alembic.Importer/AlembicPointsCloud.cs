using System.Collections.Generic;
using UnityEngine.Formats.Alembic.Sdk;

namespace UnityEngine.Formats.Alembic.Importer;

[ExecuteInEditMode]
public class AlembicPointsCloud : MonoBehaviour
{
	private PinnedList<Vector3> m_points = new PinnedList<Vector3>();

	private PinnedList<Vector3> m_velocities = new PinnedList<Vector3>();

	private PinnedList<uint> m_ids = new PinnedList<uint>();

	internal AlembicPoints m_abc;

	[Tooltip("Sort points by distance from sortFrom object")]
	internal bool m_sort;

	internal Transform m_sortFrom;

	internal PinnedList<Vector3> pointsList => m_points;

	internal PinnedList<Vector3> velocitiesList => m_velocities;

	internal PinnedList<uint> idsList => m_ids;

	public List<Vector3> Positions => pointsList.List;

	public List<Vector3> Velocities => velocitiesList.List;

	public List<uint> Ids => idsList.List;

	public Vector3 BoundsCenter { get; internal set; }

	public Vector3 BoundsExtents { get; internal set; }

	private void Reset()
	{
		Camera main = Camera.main;
		if (main != null)
		{
			m_sortFrom = main.GetComponent<Transform>();
		}
	}

	private void OnDestroy()
	{
		if (m_points != null)
		{
			m_points.Dispose();
		}
		if (m_velocities != null)
		{
			m_velocities.Dispose();
		}
		if (m_ids != null)
		{
			m_ids.Dispose();
		}
	}
}
