#define ENABLE_LOGS
using System.Collections.Generic;
using System.Linq;
using AStar;
using MapRuleLibrary.Client;
using SM.Utils;
using ScenarioRuleLibrary;
using Script.Controller;
using UnityEngine;
using UnityEngine.Rendering;

public class ClientScenarioManager : MonoBehaviour
{
	public GameObject m_TilePrefab;

	public GameObject m_Board;

	public Material m_PathRendererMaterial;

	public Material m_DarkPathRendererMaterial;

	public static ClientScenarioManager s_ClientScenarioManager;

	private const float k_MovementScalar = 26f;

	private bool m_AStarDisplay;

	private bool m_AIPathDisplay;

	private bool m_LevelFlowDisplay;

	private bool m_LOSDisplay;

	private CClientTile m_LOSSourceTile;

	private CClientTile m_LOSTargetTile;

	private CClientTile[,] m_ClientTileArray;

	private List<GameObject> m_AStarLineRendererGameObjects = new List<GameObject>();

	private List<GameObject> m_AIPathGameObjects = new List<GameObject>();

	private List<GameObject> m_LevelFlowGameObjects = new List<GameObject>();

	private List<GameObject> m_LOSGameObjects = new List<GameObject>();

	private GameObject m_DungeonEntrance;

	private GameObject m_DungeonExit;

	private List<List<CClientTile>> m_PossibleStartingTiles = new List<List<CClientTile>>();

	private List<CClientTile> m_DungeonExitTiles = new List<CClientTile>();

	private Vector3 m_LOSDebugShift;

	private CActor m_CurrentAIPathActor;

	public CClientTile[,] ClientTileArray => m_ClientTileArray;

	public GameObject DungeonEntrance => m_DungeonEntrance;

	public (GameObject Door, Vector3 DoorNormal) ApparanceDungeonEntrance { get; set; }

	public GameObject DungeonExit => m_DungeonExit;

	public List<List<CClientTile>> PossibleStartingTiles => m_PossibleStartingTiles;

	public List<CClientTile> DungeonExitTiles => m_DungeonExitTiles;

	public ScenarioState CurrentState { get; private set; }

	public bool LOSDisplay => m_LOSDisplay;

	public List<CClientTile> AllPossibleStartingTiles
	{
		get
		{
			List<CClientTile> list = new List<CClientTile>();
			foreach (List<CClientTile> possibleStartingTile in m_PossibleStartingTiles)
			{
				list.AddRange(possibleStartingTile);
			}
			return list;
		}
	}

	private void Awake()
	{
		s_ClientScenarioManager = this;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		s_ClientScenarioManager = null;
	}

	public static void Create(ScenarioState scenarioState, bool firstLoad)
	{
		ScenarioManager.Load(scenarioState, MapRuleLibraryClient.MRLYML.Headquarters.SLT, firstLoad);
		s_ClientScenarioManager.CurrentState = scenarioState;
		s_ClientScenarioManager.m_ClientTileArray = new CClientTile[ScenarioManager.Width, ScenarioManager.Height];
		for (int i = 0; i < ScenarioManager.Height; i++)
		{
			for (int j = 0; j < ScenarioManager.Width; j++)
			{
				CTile cTile = ScenarioManager.Tiles[j, i];
				if (cTile != null)
				{
					GameObject tile = Singleton<ObjectCacheService>.Instance.GetTile(cTile.m_Hex);
					if (tile != null)
					{
						CClientTile cClientTile = new CClientTile();
						s_ClientScenarioManager.m_ClientTileArray[j, i] = cClientTile;
						cClientTile.m_GameObject = tile;
						cClientTile.m_TileBehaviour = tile.GetComponent<TileBehaviour>();
						cClientTile.m_Tile = cTile;
						cClientTile.m_TileBehaviour.m_ClientTile = cClientTile;
						if (cTile.m_Hex2 != null)
						{
							cClientTile.m_TileBehaviour1 = (cClientTile.m_GameObject1 = Singleton<ObjectCacheService>.Instance.GetTile(cTile.m_Hex2)).GetComponent<TileBehaviour>();
							cClientTile.m_TileBehaviour1.m_ClientTile = cClientTile;
						}
						if (DebugMenu.DebugMenuNotNull)
						{
							cClientTile.m_TileBehaviour.CreateWorldspaceTileBehaviourUI();
						}
					}
				}
				else
				{
					ScenarioManager.Tiles[j, i] = null;
				}
			}
		}
		bool flag = false;
		bool flag2 = false;
		s_ClientScenarioManager.m_PossibleStartingTiles.Clear();
		s_ClientScenarioManager.m_DungeonExitTiles.Clear();
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject allMap in Choreographer.s_Choreographer.m_AllMaps)
		{
			list.AddRange(UnityGameEditorRuntime.FindUnityGameObjects(allMap, ScenarioManager.ObjectImportType.Tile));
		}
		list = list.Distinct().ToList();
		UnityGameEditorDoorProp[] array = (UnityGameEditorDoorProp[])Resources.FindObjectsOfTypeAll(typeof(UnityGameEditorDoorProp));
		if (SaveData.Instance.Global.CurrentCustomLevelData != null && SaveData.Instance.Global.CurrentCustomLevelData.StartingTileIndexes.Count > 0)
		{
			Dictionary<CMap, List<CClientTile>> dictionary = new Dictionary<CMap, List<CClientTile>>();
			foreach (TileIndex startingTileIndex in SaveData.Instance.Global.CurrentCustomLevelData.StartingTileIndexes)
			{
				CTile cTile2 = ScenarioManager.Tiles[startingTileIndex.X, startingTileIndex.Y];
				if (cTile2 != null)
				{
					CClientTile item = s_ClientScenarioManager.m_ClientTileArray[cTile2.m_ArrayIndex.X, cTile2.m_ArrayIndex.Y];
					if (dictionary.TryGetValue(cTile2.m_HexMap, out var value))
					{
						value.Add(item);
						continue;
					}
					dictionary.Add(cTile2.m_HexMap, new List<CClientTile> { item });
				}
			}
			foreach (List<CClientTile> value2 in dictionary.Values)
			{
				List<CClientTile> list2 = value2.ToList();
				list2.RemoveAll((CClientTile t) => scenarioState.Props.Any((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.Obstacle && p.GetConfigForPartySize(scenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden && p.ArrayIndex.X == t.m_Tile.m_ArrayIndex.X && p.ArrayIndex.Y == t.m_Tile.m_ArrayIndex.Y) || scenarioState.ActivatedProps.Any((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.Obstacle && p.GetConfigForPartySize(scenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden && p.ArrayIndex.X == t.m_Tile.m_ArrayIndex.X && p.ArrayIndex.Y == t.m_Tile.m_ArrayIndex.Y));
				if (list2.Count > 0)
				{
					s_ClientScenarioManager.m_PossibleStartingTiles.Add(list2);
					flag = true;
				}
			}
			foreach (CObjective_ActorReachPosition item2 in scenarioState.WinObjectives.FindAll((CObjective o) => o.ObjectiveType == EObjectiveType.ActorReachPosition))
			{
				foreach (TileIndex actorTargetPosition in item2.ActorTargetPositions)
				{
					if (ScenarioManager.Scenario.FindActorAt(new Point(actorTargetPosition)) == null)
					{
						CClientTile cClientTile2 = s_ClientScenarioManager.m_ClientTileArray[actorTargetPosition.X, actorTargetPosition.Y];
						CNode cNode = ScenarioManager.PathFinder.Nodes[actorTargetPosition.X, actorTargetPosition.Y];
						if (cClientTile2.m_Tile.m_Props.Count == 0 && cNode.Walkable && !cNode.Blocked)
						{
							s_ClientScenarioManager.m_DungeonExitTiles.Add(cClientTile2);
							flag2 = true;
						}
					}
				}
			}
			foreach (CObjective_ActorsEscaped item3 in scenarioState.WinObjectives.FindAll((CObjective o) => o.ObjectiveType == EObjectiveType.ActorsEscaped))
			{
				foreach (TileIndex escapePosition in item3.EscapePositions)
				{
					if (ScenarioManager.Scenario.FindActorAt(new Point(escapePosition)) != null)
					{
						continue;
					}
					CClientTile cClientTile3 = s_ClientScenarioManager.m_ClientTileArray[escapePosition.X, escapePosition.Y];
					if (!s_ClientScenarioManager.m_DungeonExitTiles.Contains(cClientTile3))
					{
						CNode cNode2 = ScenarioManager.PathFinder.Nodes[escapePosition.X, escapePosition.Y];
						if (cClientTile3.m_Tile.m_Props.Count == 0 && cNode2.Walkable && !cNode2.Blocked)
						{
							s_ClientScenarioManager.m_DungeonExitTiles.Add(cClientTile3);
							flag2 = true;
						}
					}
				}
			}
		}
		UnityGameEditorDoorProp[] array2 = array;
		foreach (UnityGameEditorDoorProp unityGameEditorDoorProp in array2)
		{
			string objName = unityGameEditorDoorProp.gameObject.name;
			CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.DoorProps.SingleOrDefault((CObjectProp x) => x.InstanceName == objName);
			if (cObjectProp != null)
			{
				Singleton<ObjectCacheService>.Instance.AddProp(cObjectProp, unityGameEditorDoorProp.gameObject);
			}
			else
			{
				LogUtils.LogWarning("Failed to find door " + objName);
			}
			int num2 = 0;
			List<CClientTile> list3 = new List<CClientTile>();
			if (!unityGameEditorDoorProp.m_IsDungeonEntrance)
			{
				continue;
			}
			s_ClientScenarioManager.m_DungeonEntrance = unityGameEditorDoorProp.gameObject;
			if (flag)
			{
				continue;
			}
			List<GameObject> list4 = new List<GameObject>();
			list4.AddRange(UnityGameEditorRuntime.FindUnityGameObjects(unityGameEditorDoorProp.transform.parent.gameObject, ScenarioManager.ObjectImportType.Tile));
			foreach (GameObject item4 in Choreographer.FindValidStartingPositions(list4, s_ClientScenarioManager.m_DungeonEntrance, null))
			{
				CClientTile clientTile = item4.GetComponent<TileBehaviour>().m_ClientTile;
				CActor cActor = ScenarioManager.Scenario.FindActorAt(clientTile.m_Tile.m_ArrayIndex);
				if (cActor == null || cActor.Type == CActor.EType.Player)
				{
					CNode cNode3 = ScenarioManager.PathFinder.Nodes[clientTile.m_Tile.m_ArrayIndex.X, clientTile.m_Tile.m_ArrayIndex.Y];
					if (clientTile.m_Tile.m_Props.Count == 0 && cNode3.Walkable && !cNode3.Blocked)
					{
						list3.Add(clientTile);
						num2++;
					}
				}
				if (num2 >= 5)
				{
					s_ClientScenarioManager.m_PossibleStartingTiles.Add(list3);
					break;
				}
			}
		}
		ScenarioManager.SetStartingTiles(s_ClientScenarioManager.AllPossibleStartingTiles.Select((CClientTile x) => x.m_Tile).ToList());
		array2 = array;
		foreach (UnityGameEditorDoorProp unityGameEditorDoorProp2 in array2)
		{
			if (!unityGameEditorDoorProp2.m_IsDungeonExit)
			{
				continue;
			}
			s_ClientScenarioManager.m_DungeonExit = unityGameEditorDoorProp2.gameObject;
			GameObject gameObject = null;
			foreach (GameObject allMap2 in Choreographer.s_Choreographer.m_AllMaps)
			{
				if (UnityGameEditorRuntime.FindUnityGameObjects(allMap2, ScenarioManager.ObjectImportType.Door).Contains(unityGameEditorDoorProp2.gameObject))
				{
					gameObject = allMap2;
					break;
				}
			}
			if (flag2 || !(gameObject != null))
			{
				continue;
			}
			foreach (GameObject item5 in Choreographer.FindValidStartingPositions(UnityGameEditorRuntime.FindUnityGameObjects(gameObject, ScenarioManager.ObjectImportType.Tile), s_ClientScenarioManager.m_DungeonExit, null))
			{
				CClientTile clientTile2 = item5.GetComponent<TileBehaviour>().m_ClientTile;
				if (ScenarioManager.Scenario.FindActorAt(clientTile2.m_Tile.m_ArrayIndex) == null)
				{
					CNode cNode4 = ScenarioManager.PathFinder.Nodes[clientTile2.m_Tile.m_ArrayIndex.X, clientTile2.m_Tile.m_ArrayIndex.Y];
					if (clientTile2.m_Tile.m_Props.Count == 0 && cNode4.Walkable && !cNode4.Blocked)
					{
						s_ClientScenarioManager.m_DungeonExitTiles.Add(clientTile2);
					}
				}
				if (s_ClientScenarioManager.m_DungeonExitTiles.Count >= 5)
				{
					break;
				}
			}
		}
		if (firstLoad && !flag2 && s_ClientScenarioManager.m_DungeonExitTiles.Count > 0)
		{
			List<TileIndex> list5 = new List<TileIndex>();
			foreach (CClientTile dungeonExitTile in s_ClientScenarioManager.m_DungeonExitTiles)
			{
				list5.Add(new TileIndex(dungeonExitTile.m_Tile.m_ArrayIndex));
			}
			foreach (CObjective_ActorReachPosition item6 in scenarioState.WinObjectives.FindAll((CObjective o) => o.ObjectiveType == EObjectiveType.ActorReachPosition))
			{
				item6.AddExitDungeonPositions(list5);
			}
			foreach (CObjective_ActorReachPosition item7 in scenarioState.LoseObjectives.FindAll((CObjective o) => o.ObjectiveType == EObjectiveType.ActorReachPosition))
			{
				item7.AddExitDungeonPositions(list5);
			}
		}
		s_ClientScenarioManager.enabled = true;
	}

	public void CreateTilesUI()
	{
		if (m_ClientTileArray == null)
		{
			return;
		}
		CClientTile[,] clientTileArray = m_ClientTileArray;
		foreach (CClientTile cClientTile in clientTileArray)
		{
			if (cClientTile != null && cClientTile.m_TileBehaviour != null)
			{
				cClientTile.m_TileBehaviour.CreateWorldspaceTileBehaviourUI();
			}
		}
	}

	public void ToggleLevelFlowDisplay()
	{
		m_LevelFlowDisplay = !m_LevelFlowDisplay;
		if (m_LevelFlowDisplay)
		{
			if (m_LevelFlowGameObjects.Count != 0)
			{
				return;
			}
			{
				foreach (CMap map in CurrentState.Maps)
				{
					ApparanceMap component = Singleton<ObjectCacheService>.Instance.GetMap(map).GetComponent<ApparanceMap>();
					GameObject gameObject = new GameObject("LevelFlowRenderer");
					m_LevelFlowGameObjects.Add(gameObject);
					LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
					lineRenderer.material = m_PathRendererMaterial;
					lineRenderer.widthMultiplier = 0.025f;
					lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
					lineRenderer.receiveShadows = false;
					lineRenderer.allowOcclusionWhenDynamic = false;
					lineRenderer.positionCount = 0;
					List<Vector3> list = new List<Vector3>();
					Vector3 vector = ((component.EntranceDoor != null) ? component.EntranceDoor.transform.position : new Vector3(map.Centre.X, map.Centre.Y, map.Centre.Z));
					Vector3 vector2 = new Vector3(map.DefaultLevelFlowNormal.X, map.DefaultLevelFlowNormal.Y, map.DefaultLevelFlowNormal.Z);
					list.Add(vector + new Vector3(0f, 0.05f, 0f));
					list.Add(vector + vector2 * 3f + new Vector3(0f, 0.05f, 0f));
					list.Add(vector + vector2 * 2f + Vector3.Cross(Vector3.up, vector2) + new Vector3(0f, 0.05f, 0f));
					list.Add(vector + vector2 * 3f + new Vector3(0f, 0.05f, 0f));
					list.Add(vector + vector2 * 2f - Vector3.Cross(Vector3.up, vector2) + new Vector3(0f, 0.05f, 0f));
					lineRenderer.enabled = m_LevelFlowDisplay;
					lineRenderer.positionCount = list.Count;
					lineRenderer.SetPositions(list.ToArray());
					GameObject gameObject2 = new GameObject("LevelCenterRenderer");
					m_LevelFlowGameObjects.Add(gameObject2);
					LineRenderer lineRenderer2 = gameObject2.AddComponent<LineRenderer>();
					lineRenderer2.material = m_PathRendererMaterial;
					lineRenderer2.widthMultiplier = 1f;
					lineRenderer2.shadowCastingMode = ShadowCastingMode.Off;
					lineRenderer2.receiveShadows = false;
					lineRenderer2.allowOcclusionWhenDynamic = false;
					lineRenderer2.positionCount = 0;
					list = new List<Vector3>();
					vector = new Vector3(map.Centre.X, map.Centre.Y, map.Centre.Z);
					list.Add(vector + new Vector3(0f, 0.05f, 0f) + new Vector3(0.5f, 0f, 0f));
					list.Add(vector + new Vector3(0f, 0.05f, 0f) + new Vector3(-1f, 0f, 0f));
					lineRenderer2.enabled = m_LevelFlowDisplay;
					lineRenderer2.positionCount = list.Count;
					lineRenderer2.SetPositions(list.ToArray());
				}
				return;
			}
		}
		foreach (GameObject levelFlowGameObject in m_LevelFlowGameObjects)
		{
			Object.Destroy(levelFlowGameObject);
		}
		m_LevelFlowGameObjects.Clear();
	}

	private void LOSDebugPrimitive(Vector3 vector1, Vector3 vector2, Material material, string primitive)
	{
		GameObject gameObject = new GameObject("LOSRenderer");
		m_LOSGameObjects.Add(gameObject);
		LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.material = material;
		lineRenderer.widthMultiplier = 0.025f;
		lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
		lineRenderer.receiveShadows = false;
		lineRenderer.allowOcclusionWhenDynamic = false;
		lineRenderer.positionCount = 0;
		lineRenderer.loop = true;
		List<Vector3> list = new List<Vector3>();
		if (primitive == "square")
		{
			list.Add(vector1 + new Vector3(0.1f, 0f, 0f) + new Vector3(0f, 0.05f, 0f));
			list.Add(vector1 + new Vector3(0f, 0f, 0.1f) + new Vector3(0f, 0.05f, 0f));
			list.Add(vector1 + new Vector3(-0.1f, 0f, 0f) + new Vector3(0f, 0.05f, 0f));
			list.Add(vector1 + new Vector3(0f, 0f, -0.1f) + new Vector3(0f, 0.05f, 0f));
		}
		else
		{
			list.Add(vector1 + new Vector3(0f, 0.05f, 0f));
			list.Add(vector2 + new Vector3(0f, 0.05f, 0f));
		}
		lineRenderer.enabled = true;
		lineRenderer.positionCount = list.Count;
		lineRenderer.SetPositions(list.ToArray());
	}

	private void DebugLOSDisplay(Vector sourcePosition, Vector targetPosition, bool rayBlocked, string primitive = "line")
	{
		LOSDebugPrimitive(new Vector3(sourcePosition.X, 0f, sourcePosition.Y) - m_LOSDebugShift, new Vector3(targetPosition.X, 0f, targetPosition.Y) - m_LOSDebugShift, rayBlocked ? m_PathRendererMaterial : m_DarkPathRendererMaterial, primitive);
	}

	public void ClearAIPathVisuals(CActor actor)
	{
		if (m_CurrentAIPathActor == actor)
		{
			return;
		}
		foreach (GameObject aIPathGameObject in m_AIPathGameObjects)
		{
			Object.Destroy(aIPathGameObject);
		}
		m_AIPathGameObjects.Clear();
		m_CurrentAIPathActor = null;
		CActorStatic.s_LastCalculatedPathActor = null;
	}

	public void Update()
	{
		if (m_LOSGameObjects.Count > 0)
		{
			foreach (GameObject lOSGameObject in m_LOSGameObjects)
			{
				Object.Destroy(lOSGameObject);
			}
			m_LOSGameObjects.Clear();
		}
		if (m_LOSDisplay || m_LOSSourceTile != null)
		{
			CClientTile cClientTile = ((!m_LOSDisplay) ? m_LOSSourceTile : ((Choreographer.s_Choreographer.m_CurrentActor == null) ? null : s_ClientScenarioManager.m_ClientTileArray[Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex.X, Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex.Y]));
			CInteractable cInteractable = MF.FindInteractableAtMousePosition(ignoreinteractableviaguiflag: false, Controller.Instance.m_HexSelectionRaycastLayer);
			CClientTile cClientTile2 = null;
			if (m_LOSTargetTile != null)
			{
				cClientTile2 = m_LOSTargetTile;
			}
			else if (cInteractable != null && cInteractable.GetComponent<TileBehaviour>() != null)
			{
				cClientTile2 = cInteractable.GetComponent<TileBehaviour>().m_ClientTile;
			}
			CActor.SetLOSTileScalar(UnityGameEditorRuntime.s_TileSize.x, UnityGameEditorRuntime.s_TileSize.z);
			if (cClientTile != null && cClientTile2 != null)
			{
				ScenarioRuleLibrary.MF.ArrayIndexToCartesianCoord(cClientTile.m_Tile.m_ArrayIndex, UnityGameEditorRuntime.s_TileSize.x, UnityGameEditorRuntime.s_TileSize.z, out m_LOSDebugShift.x, out m_LOSDebugShift.z);
				m_LOSDebugShift -= cClientTile.m_GameObject.transform.position;
				CActor.HaveLOS(cClientTile.m_Tile, cClientTile2.m_Tile, DebugLOSDisplay);
			}
		}
		if (m_AIPathDisplay)
		{
			if (CActorStatic.s_LastCalculatedPathActor == null || CActorStatic.s_LastCalculatedPathActor == m_CurrentAIPathActor || CActorStatic.s_LastCalculatedPath == null || CActorStatic.s_LastCalculatedPath.m_ArrayIndices.Count <= 0)
			{
				return;
			}
			m_CurrentAIPathActor = CActorStatic.s_LastCalculatedPathActor;
			foreach (GameObject aIPathGameObject in m_AIPathGameObjects)
			{
				Object.Destroy(aIPathGameObject);
			}
			m_AIPathGameObjects.Clear();
			if (CActorStatic.s_LastCalculatedPath.m_ArrayIndicesBeforeCull != null)
			{
				GameObject gameObject = new GameObject("AIPathRendererFull");
				m_AIPathGameObjects.Add(gameObject);
				LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
				lineRenderer.material = m_PathRendererMaterial;
				lineRenderer.widthMultiplier = 0.1f;
				lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
				lineRenderer.receiveShadows = false;
				lineRenderer.allowOcclusionWhenDynamic = false;
				lineRenderer.positionCount = 0;
				lineRenderer.numCapVertices = 8;
				lineRenderer.numCornerVertices = 8;
				List<Vector3> list = new List<Vector3>();
				list.Add(Choreographer.s_Choreographer.FindClientActorGameObject(m_CurrentAIPathActor).transform.position + new Vector3(0f, 0.25f, 0f));
				foreach (Point item in CActorStatic.s_LastCalculatedPath.m_ArrayIndicesBeforeCull)
				{
					list.Add(m_ClientTileArray[item.X, item.Y].m_GameObject.transform.position + new Vector3(0f, 0.1f, 0f));
				}
				lineRenderer.enabled = m_AIPathDisplay;
				lineRenderer.positionCount = list.Count;
				lineRenderer.SetPositions(list.ToArray());
			}
			GameObject gameObject2 = new GameObject("AIPathRenderer");
			m_AIPathGameObjects.Add(gameObject2);
			LineRenderer lineRenderer2 = gameObject2.AddComponent<LineRenderer>();
			lineRenderer2.material = m_DarkPathRendererMaterial;
			lineRenderer2.widthMultiplier = 0.1f;
			lineRenderer2.shadowCastingMode = ShadowCastingMode.Off;
			lineRenderer2.receiveShadows = false;
			lineRenderer2.allowOcclusionWhenDynamic = false;
			lineRenderer2.positionCount = 0;
			lineRenderer2.numCapVertices = 8;
			lineRenderer2.numCornerVertices = 8;
			List<Vector3> list2 = new List<Vector3>();
			list2.Add(Choreographer.s_Choreographer.FindClientActorGameObject(m_CurrentAIPathActor).transform.position + new Vector3(0f, 0.25f, 0f));
			foreach (Point arrayIndex in CActorStatic.s_LastCalculatedPath.m_ArrayIndices)
			{
				list2.Add(m_ClientTileArray[arrayIndex.X, arrayIndex.Y].m_GameObject.transform.position + new Vector3(0f, 0.25f, 0f));
			}
			lineRenderer2.enabled = m_AIPathDisplay;
			lineRenderer2.positionCount = list2.Count;
			lineRenderer2.SetPositions(list2.ToArray());
			return;
		}
		foreach (GameObject aIPathGameObject2 in m_AIPathGameObjects)
		{
			Object.Destroy(aIPathGameObject2);
		}
		m_AIPathGameObjects.Clear();
	}

	public void ToggleLOSDisplay(bool? newState = null)
	{
		m_LOSDisplay = (newState.HasValue ? newState.Value : (!m_LOSDisplay));
	}

	public void EnableUserLOSDisplay(bool isOn)
	{
		if (!isOn)
		{
			m_LOSSourceTile = null;
		}
		else if (Controller.Instance != null)
		{
			CInteractable cInteractable = MF.FindInteractableAtMousePosition(ignoreinteractableviaguiflag: false, Controller.Instance.m_HexSelectionRaycastLayer);
			m_LOSSourceTile = ((cInteractable == null || cInteractable.GetComponent<TileBehaviour>() == null) ? null : cInteractable.GetComponent<TileBehaviour>().m_ClientTile);
		}
	}

	public void SetSpecificSourceAndTargetTile(CClientTile sourceTile, CClientTile targetTile)
	{
		m_LOSSourceTile = sourceTile;
		m_LOSTargetTile = targetTile;
	}

	public void ToggleAStarDisplay()
	{
		m_AStarDisplay = !m_AStarDisplay;
		if (m_AStarDisplay)
		{
			if (m_AStarLineRendererGameObjects.Count != 0)
			{
				return;
			}
			for (int i = 0; i < ScenarioManager.Height; i++)
			{
				GameObject gameObject = new GameObject("PathRenderer");
				m_AStarLineRendererGameObjects.Add(gameObject);
				LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
				lineRenderer.material = m_PathRendererMaterial;
				lineRenderer.widthMultiplier = 0.025f;
				lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
				lineRenderer.receiveShadows = false;
				lineRenderer.allowOcclusionWhenDynamic = false;
				lineRenderer.positionCount = 0;
				List<Vector3> list = new List<Vector3>();
				for (int j = 0; j < ScenarioManager.Width; j++)
				{
					CClientTile cClientTile = s_ClientScenarioManager.m_ClientTileArray[j, i];
					if (cClientTile == null || ScenarioManager.PathFinder.Nodes[j, i] == null || !ScenarioManager.PathFinder.Nodes[j, i].Walkable || ScenarioManager.PathFinder.Nodes[j, i].Blocked)
					{
						continue;
					}
					for (ScenarioManager.EAdjacentPosition eAdjacentPosition = ScenarioManager.EAdjacentPosition.EBottomRight; eAdjacentPosition >= ScenarioManager.EAdjacentPosition.ERight; eAdjacentPosition--)
					{
						CTile adjacentTile = ScenarioManager.GetAdjacentTile(j, i, eAdjacentPosition);
						if (adjacentTile != null && ScenarioManager.PathFinder.Nodes[j, i].NavTo(ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y], ignoreBlocked: false))
						{
							list.Add(cClientTile.m_GameObject.transform.position + new Vector3(0f, 0.1f, 0f));
							list.Add(m_ClientTileArray[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].m_GameObject.transform.position + new Vector3(0f, 0.1f, 0f));
							list.Add(cClientTile.m_GameObject.transform.position + new Vector3(0f, 0.1f, 0f));
						}
						else if (eAdjacentPosition == ScenarioManager.EAdjacentPosition.ERight)
						{
							lineRenderer.enabled = m_AStarDisplay;
							lineRenderer.positionCount = list.Count;
							lineRenderer.SetPositions(list.ToArray());
							gameObject = new GameObject("PathRenderer");
							m_AStarLineRendererGameObjects.Add(gameObject);
							lineRenderer = gameObject.AddComponent<LineRenderer>();
							lineRenderer.material = m_PathRendererMaterial;
							lineRenderer.widthMultiplier = 0.025f;
							lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
							lineRenderer.receiveShadows = false;
							lineRenderer.allowOcclusionWhenDynamic = false;
							lineRenderer.positionCount = 0;
							list = new List<Vector3>();
						}
					}
				}
				lineRenderer.enabled = m_AStarDisplay;
				lineRenderer.positionCount = list.Count;
				lineRenderer.SetPositions(list.ToArray());
			}
			return;
		}
		foreach (GameObject aStarLineRendererGameObject in m_AStarLineRendererGameObjects)
		{
			Object.Destroy(aStarLineRendererGameObject);
		}
		m_AStarLineRendererGameObjects.Clear();
	}

	public void ToggleAIPathDisplay()
	{
		m_AIPathDisplay = !m_AIPathDisplay;
	}
}
