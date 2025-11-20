using System.Collections.Generic;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using UnityEngine;

public class ApparanceLayer : MonoBehaviour
{
	[Header("Doors")]
	public GameObject ThinDoor;

	public GameObject ThickDoor;

	public GameObject ThinNarrowDoor;

	public GameObject ThickNarrowDoor;

	[Header("Entrance doors")]
	public GameObject EntranceThinDoor;

	public GameObject EntranceThickDoor;

	[Header("Exit doors")]
	public GameObject ExitThinDoor;

	public GameObject ExitThickDoor;

	public static ApparanceLayer Instance;

	public List<GameObject> DoorInstances;

	[UsedImplicitly]
	private void Start()
	{
		Instance = this;
		DoorInstances = new List<GameObject>();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Instance = null;
	}

	public void Create(CObjectDoor door, GameObject procDoor)
	{
		GameObject gameObject = Object.Instantiate(GetDoorPrefab(door), base.transform);
		DoorInstances.Add(gameObject);
		gameObject.transform.localPosition = GloomUtility.CVToV(door.Position);
		gameObject.transform.localEulerAngles = new Vector3(0f, door.Angle, 0f);
		UnityGameEditorDoorProp component = procDoor.GetComponent<UnityGameEditorDoorProp>();
		if (!(component != null))
		{
			return;
		}
		if (component.m_IsDungeonEntrance || component.m_IsDungeonExit)
		{
			gameObject.transform.SetParent(procDoor.transform.parent);
			gameObject.name = procDoor.name;
			ScenarioManager.PathFinder.Nodes[door.ArrayIndex.X, door.ArrayIndex.Y].Blocked = true;
			if (component.m_IsDungeonEntrance)
			{
				ClientScenarioManager.s_ClientScenarioManager.ApparanceDungeonEntrance = (Door: gameObject, DoorNormal: component.ForwardVector());
			}
			Object.Destroy(procDoor);
			return;
		}
		UnityGameEditorObject component2 = procDoor.GetComponent<UnityGameEditorObject>();
		if (component2 != null)
		{
			component2.PropObject = door;
		}
		gameObject.transform.SetParent(procDoor.transform);
		component.CreateHoverCapsule(gameObject);
		if (0 == 0 && !component.m_InitiallyVisable)
		{
			Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
	}

	public void CreateLevelEditor(CObjectDoor door, GameObject procDoor)
	{
		GameObject gameObject = Object.Instantiate(GetDoorPrefab(door), base.transform);
		DoorInstances.Add(gameObject);
		gameObject.transform.localPosition = GloomUtility.CVToV(door.Position);
		gameObject.transform.SetParent(procDoor.transform);
		gameObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		UnityGameEditorDoorProp component = procDoor.GetComponent<UnityGameEditorDoorProp>();
		if (!(component != null))
		{
			return;
		}
		if (component.m_IsDungeonEntrance || component.m_IsDungeonExit)
		{
			gameObject.transform.SetParent(procDoor.transform.parent);
			gameObject.name = procDoor.name;
			ScenarioManager.PathFinder.Nodes[door.ArrayIndex.X, door.ArrayIndex.Y].Blocked = true;
			if (component.m_IsDungeonEntrance)
			{
				ClientScenarioManager.s_ClientScenarioManager.ApparanceDungeonEntrance = (Door: gameObject, DoorNormal: component.ForwardVector());
			}
			Object.Destroy(procDoor);
			return;
		}
		UnityGameEditorObject component2 = procDoor.GetComponent<UnityGameEditorObject>();
		if (component2 != null)
		{
			component2.PropObject = door;
		}
		component.CreateHoverCapsule(gameObject);
		if (0 == 0 && !component.m_InitiallyVisable)
		{
			Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
	}

	public void DestroyDoors()
	{
		foreach (GameObject doorInstance in DoorInstances)
		{
			Object.Destroy(doorInstance);
		}
	}

	public GameObject GetDoorPrefab(CObjectDoor doorObject)
	{
		switch (doorObject.DoorType)
		{
		case CObjectDoor.EDoorType.ThickDoor:
			if (doorObject.IsDungeonEntrance)
			{
				return EntranceThickDoor;
			}
			if (doorObject.IsDungeonExit)
			{
				return ExitThickDoor;
			}
			return ThickDoor;
		case CObjectDoor.EDoorType.ThinDoor:
			if (doorObject.IsDungeonEntrance)
			{
				return EntranceThinDoor;
			}
			if (doorObject.IsDungeonExit)
			{
				return ExitThinDoor;
			}
			return ThinDoor;
		case CObjectDoor.EDoorType.ThickNarrowDoor:
			return ThickNarrowDoor;
		case CObjectDoor.EDoorType.ThinNarrowDoor:
			return ThinNarrowDoor;
		default:
			Debug.LogError("Invalid door type " + doorObject.DoorType);
			return null;
		}
	}

	public static bool IsThinDoor(CObjectDoor.EDoorType doorType)
	{
		switch (doorType)
		{
		case CObjectDoor.EDoorType.ThinDoor:
		case CObjectDoor.EDoorType.ThinNarrowDoor:
			return true;
		case CObjectDoor.EDoorType.ThickDoor:
		case CObjectDoor.EDoorType.ThickNarrowDoor:
			return false;
		default:
			Debug.LogError("Invalid door type " + doorType);
			return false;
		}
	}
}
