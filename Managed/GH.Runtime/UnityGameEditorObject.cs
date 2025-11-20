using System.Collections.Generic;
using AStar;
using EPOOutline;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using Script.Controller;
using UnityEngine;

[ExecuteInEditMode]
public class UnityGameEditorObject : MonoBehaviour
{
	public bool m_ShouldSnapToHexSpacing;

	public bool m_InvisableAtRuntime;

	public ScenarioManager.ObjectImportType m_ObjectType;

	public string m_PrefabName;

	public bool m_ShouldSnapRotation;

	public bool m_ExcludeFromExport;

	[HideInInspector]
	public List<ScenarioManager.ObjectImportType> m_ProcGenGameObjectsOnTile = new List<ScenarioManager.ObjectImportType>();

	private List<Point> m_PathingBlockers = new List<Point>();

	private List<Outlinable> m_OutlinableList;

	private CObjectProp m_PropObject;

	public CObjectProp PropObject
	{
		get
		{
			return m_PropObject;
		}
		set
		{
			m_PropObject = value;
		}
	}

	[UsedImplicitly]
	private void OnDrawGizmos()
	{
		if (GetComponent<CharacterManager>() != null)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(base.transform.position, 0.5f);
		}
	}

	[UsedImplicitly]
	private void Start()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (m_ObjectType != ScenarioManager.ObjectImportType.Tile && m_ObjectType != ScenarioManager.ObjectImportType.EdgeTile && m_ObjectType != ScenarioManager.ObjectImportType.Coverage)
		{
			base.gameObject.layer = LayerMask.NameToLayer("Hovering");
		}
		if (!(GetComponent<Coverage>() == null) || ScenarioManager.Scenario == null || ScenarioManager.Scenario.PositiveSpaceOffset == null)
		{
			return;
		}
		Vector3Int vector3Int = new Vector3Int(ScenarioManager.Scenario.PositiveSpaceOffset.X, ScenarioManager.Scenario.PositiveSpaceOffset.Y, ScenarioManager.Scenario.PositiveSpaceOffset.Z);
		Coverage[] componentsInChildren = base.gameObject.GetComponentsInChildren<Coverage>();
		foreach (Coverage obj in componentsInChildren)
		{
			Vector3Int vector3Int2 = MF.GetTileIntegerSnapSpace(obj.gameObject.transform.position) + vector3Int;
			m_PathingBlockers.Add(new Point(vector3Int2.x, vector3Int2.z));
			Object.Destroy(obj.gameObject);
		}
		foreach (Point pathingBlocker in m_PathingBlockers)
		{
			ScenarioManager.PathFinder.Nodes[pathingBlocker.X, pathingBlocker.Y].Blocked = true;
		}
	}

	public void SetObject()
	{
	}

	public void ReleasePathing()
	{
		foreach (Point pathingBlocker in m_PathingBlockers)
		{
			ScenarioManager.PathFinder.Nodes[pathingBlocker.X, pathingBlocker.Y].Blocked = false;
		}
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		if (Application.isPlaying)
		{
			ReleasePathing();
		}
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		if (Singleton<ObjectCacheService>.Instance != null)
		{
			Singleton<ObjectCacheService>.Instance.AddUnityGameEditorObject(this);
		}
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		if (Singleton<ObjectCacheService>.Instance != null)
		{
			Singleton<ObjectCacheService>.Instance.RemoveUnityGameEditorObject(this);
		}
	}
}
