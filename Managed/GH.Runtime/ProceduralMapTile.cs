#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using Apparance.Net;
using UnityEngine;

[RequireComponent(typeof(ApparanceEntity))]
[RequireComponent(typeof(ProceduralStyle))]
public class ProceduralMapTile : ProceduralTileObserver, IProceduralContentMonitor
{
	public enum Visibility
	{
		Hidden,
		Preview,
		PreviewWithDoors,
		All
	}

	public Visibility visibility = Visibility.All;

	[Tooltip("Test mode to show wall ends and successful corner joins")]
	public bool showWallConnections;

	private Visibility previousVisibility = Visibility.All;

	private ProceduralWallAnalyser wallAnalyser;

	public static bool contentGenerationActive;

	private DynamicAmbience m_Ambience;

	private BoxCollider m_boxCollider;

	private IEnumerable<GameObject> AllWalls
	{
		get
		{
			GameObject gameObject = base.gameObject.FindInChildren("Walls", includeInactive: true);
			if (!(gameObject != null))
			{
				yield break;
			}
			foreach (ProceduralWall item in gameObject.FindInImmediateChildren<ProceduralWall>(includeInactive: true))
			{
				yield return item.gameObject;
			}
		}
	}

	private IEnumerable<GameObject> AllDoors
	{
		get
		{
			foreach (UnityGameEditorDoorProp door in base.gameObject.FindInImmediateChildren<UnityGameEditorDoorProp>(includeInactive: true))
			{
				yield return door.gameObject;
				foreach (ProceduralDoorway item in door.gameObject.FindInImmediateChildren<ProceduralDoorway>(includeInactive: true))
				{
					yield return item.gameObject;
				}
			}
		}
	}

	public DynamicAmbience Ambience
	{
		get
		{
			return m_Ambience;
		}
		set
		{
			m_Ambience = value;
		}
	}

	public BoxCollider BoxCollider
	{
		get
		{
			if (m_boxCollider == null)
			{
				m_boxCollider = GetComponent<BoxCollider>();
			}
			return m_boxCollider;
		}
	}

	public IEnumerable<GameObject> ConnectedDoors
	{
		get
		{
			foreach (GameObject doorway in GetScenario().Doorways)
			{
				ProceduralDoorway component = doorway.GetComponent<ProceduralDoorway>();
				if (component.GetFrontMapTile() == this || component.GetBackMapTile() == this)
				{
					yield return component.gameObject;
				}
			}
		}
	}

	public static event Action contentGenerationStarted;

	protected override void Update()
	{
		base.Update();
		if (visibility != previousVisibility)
		{
			previousVisibility = visibility;
			ApplyVisibility();
		}
		if (showWallConnections != (wallAnalyser != null))
		{
			if (showWallConnections)
			{
				wallAnalyser = new ProceduralWallAnalyser();
			}
			else
			{
				wallAnalyser = null;
			}
		}
	}

	private void ApplyVisibility()
	{
		bool show_preview = visibility == Visibility.Preview || visibility == Visibility.PreviewWithDoors;
		if (visibility == Visibility.All)
		{
			_ = 1;
		}
		else
			_ = visibility == Visibility.PreviewWithDoors;
		bool show_full = visibility == Visibility.All;
		ShowContent(base.gameObject, show_full, show_preview);
		foreach (GameObject allWall in AllWalls)
		{
			ShowContent(allWall, show_full);
		}
		foreach (GameObject allDoor in AllDoors)
		{
			allDoor.GetComponent<ProceduralDoorway>()?.SetVisibility(visibility);
		}
	}

	public static void ShowContent(GameObject o, bool show_full, bool show_preview = false)
	{
		if (!(o != null))
		{
			return;
		}
		GameObject gameObject = o.FindInChildren("Generated Content", includeInactive: true);
		if (!(gameObject != null))
		{
			return;
		}
		GameObject gameObject2 = gameObject.FindInChildren("Preview", includeInactive: true);
		if (gameObject2 != null)
		{
			gameObject.SetActive(value: true);
			gameObject2.SetActive(show_preview);
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				GameObject gameObject3 = gameObject.transform.GetChild(i).gameObject;
				if (gameObject3 != gameObject2)
				{
					gameObject3.SetActive(show_full);
				}
			}
		}
		else
		{
			gameObject.SetActive(show_full);
		}
	}

	public override void NotifyContentPlacementComplete()
	{
		base.NotifyContentPlacementComplete();
		GameObject gameObject = base.gameObject.FindInChildren("Generated Content", includeInactive: true);
		if (gameObject != null)
		{
			gameObject.layer = 10;
			foreach (Transform item in gameObject.transform)
			{
				item.gameObject.layer = 10;
			}
		}
		if (!contentGenerationActive)
		{
			Debug.Log("[Apparance] Content generation active.");
			contentGenerationActive = true;
			if (ProceduralMapTile.contentGenerationStarted != null)
			{
				ProceduralMapTile.contentGenerationStarted();
			}
		}
		ApplyVisibility();
	}

	public void ProceduralContentChanged(GameObject changed_child_object)
	{
		ApplyVisibility();
	}

	public override void WriteExtraParameters(ParameterCollection parameters, int next_id)
	{
		ProceduralStyle proceduralStyle = GloomUtility.EnsureComponent<ProceduralStyle>(base.gameObject);
		ParameterCollection style_struct = parameters.WriteListBegin(next_id++);
		proceduralStyle.WriteStyleParameters(style_struct, GetScenarioSeed(), proceduralStyle.Seed, proceduralStyle.Seed);
		parameters.WriteListEnd();
		int integer_value = ((PlatformLayer.Setting != null) ? PlatformLayer.Setting.GetApparenceSettingByCurrentLevel()._qualityLevel : 0);
		parameters.WriteInteger(integer_value, next_id);
	}

	private List<ProceduralWallAnalyser.WallInfo> GatherAnalysisWallInfo()
	{
		List<ProceduralWallAnalyser.WallInfo> list = new List<ProceduralWallAnalyser.WallInfo>();
		foreach (GameObject allWall in AllWalls)
		{
			Transform transform = allWall.transform;
			BoxCollider component = allWall.GetComponent<BoxCollider>();
			if (component != null)
			{
				Transform transform2 = component.transform;
				Matrix4x4 localToWorld = transform.transform.parent.localToWorldMatrix * transform2.localToWorldMatrix;
				list.Add(new ProceduralWallAnalyser.WallInfo
				{
					wallObject = allWall,
					boxBounds = component,
					boxParent = transform,
					localToWorld = localToWorld
				});
			}
		}
		return list;
	}

	private void OnDrawGizmos()
	{
		if (wallAnalyser != null)
		{
			List<ProceduralWallAnalyser.WallInfo> walls = GatherAnalysisWallInfo();
			wallAnalyser.SetWalls(walls);
			wallAnalyser.RunDiags();
			wallAnalyser.DrawConnections();
		}
	}
}
