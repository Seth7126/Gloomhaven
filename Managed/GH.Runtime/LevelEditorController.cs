#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using AStar;
using Chronos;
using GLOOM;
using GLOOM.MainMenu;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using Script.Controller;
using SharedLibrary;
using SharedLibrary.Client;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class LevelEditorController : MonoBehaviour
{
	public enum ECaptureTileFunction
	{
		None = 0,
		SpawnUnit = 1,
		MoveUnit = 2,
		RecordTile = 4,
		SpawnInMultipleTiles = 5
	}

	public enum ELevelEditorState
	{
		Idle,
		PreviewingLoadOwnParty,
		PreviewingFixedPartyLevel,
		Editing
	}

	public enum ELevelEditingState
	{
		None,
		RoomPlacement,
		DoorPlacement,
		PropActorPlacement
	}

	public enum ERoomPlacementState
	{
		None,
		PlacingRoom,
		PlacingDoor
	}

	public static LevelEditorController s_Instance;

	[SerializeField]
	private AssetReference _referenceLevelEditorUI;

	public ScenarioManager.EDLLMode LastLoadedRuleset;

	public LevelEditorUI m_LevelEditorUIInstance;

	[SerializeField]
	private AsyncOperationHandle<GameObject> _handleLevelEditorUI;

	[HideInInspector]
	public bool AutoTestNeedsSaving;

	private List<string> m_EnemyStateGuidsToSetHidden;

	private int m_CurrentId = 1;

	private const string c_RoomNamePrefix = "Room_";

	private SharedLibrary.Random m_Random;

	public GameObject ThinDoorPrefab;

	public GameObject ThickDoorPrefab;

	public GameObject ThinNarrowDoorPrefab;

	public GameObject ThickNarrowDoorPrefab;

	public GameObject LocationIndicatorPrefab;

	private GameObject m_CurrentLocationIndicator;

	private IEnumerator m_LocationIndicatorRoutine;

	private ELevelEditingState m_CurrentLevelEditingState = ELevelEditingState.RoomPlacement;

	private GameObject m_CurrentMovingObject;

	private GameObject m_Plane;

	private GameObject m_DirectionalLight;

	public Dictionary<string, bool> WallVisibilityOverrides = new Dictionary<string, bool>();

	public bool BulkLevelProcessingInProgress;

	private bool m_EndBulkRun;

	private IEnumerator m_BulkLevelProcessingRoutine;

	public List<string> LoggedErrors = new List<string>();

	public List<string> LoggedWarnings = new List<string>();

	private string m_ToPlaceObjectIdentifier;

	private string m_ToMoveObjectGuid;

	private ECaptureTileFunction m_CaptureTileFunction;

	private Action<CClientTile> m_TileSelectedAction;

	public List<GameObject> AllMaps { get; private set; }

	public GameObject MapSceneRoot { get; private set; }

	public ELevelEditorState CurrentState { get; private set; }

	public ELevelEditingState CurrentLevelEditingState
	{
		get
		{
			return m_CurrentLevelEditingState;
		}
		set
		{
			if (m_CurrentLevelEditingState != value)
			{
				m_CurrentLevelEditingState = value;
				if (m_LevelEditorUIInstance != null && m_LevelEditorUIInstance.TabTogglesController != null)
				{
					m_LevelEditorUIInstance.TabTogglesController.RefreshAvailableToggles(m_CurrentLevelEditingState);
				}
			}
		}
	}

	public ERoomPlacementState CurrentPlacementState { get; private set; }

	public bool IsEditing => CurrentState >= ELevelEditorState.Editing;

	public bool IsPreviewingLevel
	{
		get
		{
			if (CurrentState != ELevelEditorState.PreviewingFixedPartyLevel)
			{
				return CurrentState == ELevelEditorState.PreviewingLoadOwnParty;
			}
			return true;
		}
	}

	public CCustomLevelData LoadedLevel { get; private set; }

	public string LoadedLevelFileName { get; private set; }

	public string LoadedAutoTestFileName { get; private set; }

	public bool BulkProcessingErrorOccurred { get; set; }

	public CAreaEffect CurrentAreaEffectSelection { get; set; }

	private bool CaptureTile { get; set; }

	private LevelEditorUnitsPanel.UnitType CaptureTileUnitType { get; set; }

	private int m_CaptureTileExpectedCount { get; set; }

	private List<CClientTile> m_CurrentlyCapturedTiles { get; set; }

	private void Awake()
	{
		s_Instance = this;
		WallVisibilityOverrides = new Dictionary<string, bool>();
		if (PlatformLayer.Instance.IsConsole)
		{
			_referenceLevelEditorUI = null;
			LocationIndicatorPrefab = null;
		}
	}

	private void Update()
	{
		if (CurrentState != ELevelEditorState.Editing || (CurrentPlacementState != ERoomPlacementState.PlacingRoom && CurrentPlacementState != ERoomPlacementState.PlacingDoor) || !(Camera.main != null))
		{
			return;
		}
		Ray ray = Camera.main.ScreenPointToRay(InputManager.CursorPosition);
		int mask = LayerMask.GetMask("LevelEditorPlane");
		if (Physics.Raycast(ray, out var hitInfo, 50000f, mask))
		{
			m_CurrentMovingObject.transform.position = hitInfo.point;
			SnapGameObjectToHexes(m_CurrentMovingObject);
		}
		if (InputManager.GetWasPressed(KeyAction.ROTATE_TARGET))
		{
			m_CurrentMovingObject.transform.Rotate(0f, 60f, 0f, Space.Self);
		}
		if (Singleton<InputManager>.Instance.PlayerControl.MouseClickRight.WasPressed)
		{
			if (CurrentPlacementState == ERoomPlacementState.PlacingRoom)
			{
				CMap item = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap x) => x.MapInstanceName == m_CurrentMovingObject.name);
				ScenarioManager.CurrentScenarioState.Maps.Remove(item);
				AllMaps.Remove(m_CurrentMovingObject);
				m_LevelEditorUIInstance.RoomPanel.RefreshRoomUIWithCurrentState();
			}
			UnityEngine.Object.Destroy(m_CurrentMovingObject);
			CurrentPlacementState = ERoomPlacementState.None;
			m_CurrentMovingObject = null;
			s_Instance.m_LevelEditorUIInstance.FinishedAction();
		}
		if (Singleton<InputManager>.Instance.PlayerControl.MouseClickLeft.WasPressed)
		{
			bool flag = false;
			switch (CurrentPlacementState)
			{
			case ERoomPlacementState.PlacingRoom:
				flag = LockRoomPlacement();
				break;
			case ERoomPlacementState.PlacingDoor:
				flag = LockDoorPlacement();
				break;
			}
			if (flag)
			{
				m_LevelEditorUIInstance.RoomPanel.RefreshRoomUIWithCurrentState();
				m_LevelEditorUIInstance.RoomPanel.RefreshDoorUIWithCurrentState();
				CurrentPlacementState = ERoomPlacementState.None;
				m_CurrentMovingObject = null;
				s_Instance.m_LevelEditorUIInstance.FinishedAction();
			}
		}
	}

	public void StartLevelEditor()
	{
		AllMaps = new List<GameObject>();
		if (m_Plane == null)
		{
			m_Plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
			m_Plane.transform.SetPositionAndRotation(new Vector3(m_Plane.transform.position.x, 0f, m_Plane.transform.position.z), m_Plane.transform.rotation);
			m_Plane.transform.localScale = new Vector3(10f, 1f, 10f);
			m_Plane.layer = 31;
		}
		if (m_DirectionalLight == null)
		{
			m_DirectionalLight = new GameObject("Light");
			m_DirectionalLight.AddComponent<Light>().type = LightType.Directional;
		}
		SetLevelEditorState(ELevelEditorState.Editing);
		if (SaveData.Instance.Global.CurrentEditorLevelData.ScenarioState != null)
		{
			UnityGameEditorRuntime.LoadScenario(SaveData.Instance.Global.CurrentEditorLevelData.ScenarioState);
			return;
		}
		Debug.LogError("You are loading a file that doesn't have a valid ScenarioState. It probably means that you are loading a deprecated file. Please, verify.");
		SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SAVEDATA_00016", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
	}

	public void QuitLevelEditor()
	{
		SetLevelEditorState(ELevelEditorState.Idle);
		if (!GloomUtility.IsGUIVisible())
		{
			GloomUtility.ToggleGUI();
		}
		SaveData.Instance.Global.CurrentEditorAutoTestData = null;
		SaveData.Instance.Global.CurrentEditorLevelData = null;
		if (m_LevelEditorUIInstance != null)
		{
			AssetBundleManager.ReleaseHandle(_handleLevelEditorUI, releaseInstance: true);
			_handleLevelEditorUI = default(AsyncOperationHandle<GameObject>);
		}
		if (m_DirectionalLight != null)
		{
			UnityEngine.Object.Destroy(m_DirectionalLight);
		}
		if (m_Plane != null)
		{
			UnityEngine.Object.Destroy(m_Plane);
		}
	}

	public void SetLevelEditorState(ELevelEditorState stateToSet)
	{
		if (CurrentState != stateToSet)
		{
			WallVisibilityOverrides.Clear();
			CurrentState = stateToSet;
		}
	}

	public void LoadLevelEditorScene(ScenarioState state, bool isFirstLoad)
	{
		AllMaps = new List<GameObject>();
		m_EnemyStateGuidsToSetHidden = (from a in state.Monsters
			where !a.IsRevealed
			select a.ActorGuid).ToList();
		Debug.Log("Level editor generating level from state with map count: " + state.Maps.Count);
		m_Random = new SharedLibrary.Random(state.Seed);
		MapSceneRoot = RoomVisibilityManager.s_Instance.Maps;
		Choreographer.s_Choreographer.m_MapSceneRoot = MapSceneRoot;
		if (state.Maps.Count > 0)
		{
			CurrentLevelEditingState = ELevelEditingState.PropActorPlacement;
			if (m_DirectionalLight != null)
			{
				UnityEngine.Object.Destroy(m_DirectionalLight);
			}
			foreach (CMap map in state.Maps)
			{
				LoadMaps(map, state, null, isFirstLoad);
			}
			if (AllMaps.Count < state.Maps.Count)
			{
				foreach (CMap item in state.Maps.Where((CMap w) => !AllMaps.Any((GameObject a) => a.name == w.MapInstanceName)).ToList())
				{
					state.MapFailedToLoad(item);
				}
			}
			if (AllMaps.Count != state.Maps.Count)
			{
				throw new Exception("Invalid map layout.  Map counts not equal.");
			}
			UnityGameEditorRuntime.InitialiseScenario(state);
		}
		else
		{
			CurrentLevelEditingState = ELevelEditingState.RoomPlacement;
		}
		ClientScenarioManager.Create(state, isFirstLoad);
		if (state.Maps.Count > 0)
		{
			SetupDoorsFromFullLoad(state, isFirstLoad);
		}
	}

	private void LoadMaps(CMap map, ScenarioState state, GameObject connectingDoor, bool generateNewMap)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>("misc_mapsleveleditor", "Map " + map.MapType, "mapsleveleditor"), MapSceneRoot.transform);
		gameObject.name = map.MapInstanceName;
		ApparanceMap component = gameObject.GetComponent<ApparanceMap>();
		component.MapGuid = map.MapGuid;
		component.RoomName = map.RoomName;
		component.Tiles = UnityGameEditorRuntime.FindUnityGameObjects(gameObject, ScenarioManager.ObjectImportType.Tile);
		gameObject.transform.eulerAngles = Vector3.zero;
		int num = 0;
		Vector3 zero = Vector3.zero;
		foreach (GameObject tile in component.Tiles)
		{
			zero += tile.transform.position;
			num++;
		}
		zero /= (float)num;
		map.Centre = new CVector3(zero.x, zero.y, zero.z);
		GameObject gameObject2 = null;
		float num2 = float.MaxValue;
		foreach (GameObject tile2 in component.Tiles)
		{
			if ((tile2.transform.position - zero).magnitude < num2)
			{
				gameObject2 = tile2;
				num2 = (tile2.transform.position - zero).magnitude;
			}
		}
		map.ClosestTileIdentityPosition = new CVector3(gameObject2.transform.position.x, gameObject2.transform.position.y, gameObject2.transform.position.z);
		if (generateNewMap)
		{
			return;
		}
		gameObject.transform.position = new Vector3(map.Position.X, map.Position.Y, map.Position.Z);
		gameObject.transform.eulerAngles = new Vector3(0f, map.Angle, 0f);
		foreach (CObjectDoor item in state.DoorProps.Where((CObjectProp x) => x.StartingMapGuid == map.MapGuid))
		{
			GameObject obj = UnityEngine.Object.Instantiate(GetDoorPrefab(item.DoorType), gameObject.transform);
			obj.name = item.InstanceName;
			obj.transform.position = new Vector3(item.Position.X, item.Position.Y, item.Position.Z);
			obj.transform.localEulerAngles = new Vector3(0f, item.Angle, 0f);
			UnityGameEditorDoorProp component2 = obj.GetComponent<UnityGameEditorDoorProp>();
			if (component2 != null)
			{
				component2.m_InitiallyVisable = true;
				component2.m_IsDungeonEntrance = item.IsDungeonEntrance;
				component2.m_IsDungeonExit = item.IsDungeonExit;
			}
		}
		InitNewMap(gameObject, map);
	}

	private void InitNewMap(GameObject newMapRootGameObject, CMap map)
	{
		ProceduralStyle component = newMapRootGameObject.GetComponent<ProceduralStyle>();
		if (map.SelectedPossibleRoom != null)
		{
			component.Biome = map.SelectedPossibleRoom.Biome;
			component.SubBiome = map.SelectedPossibleRoom.SubBiome;
			component.Theme = map.SelectedPossibleRoom.Theme;
			component.SubTheme = map.SelectedPossibleRoom.SubTheme;
			component.Tone = map.SelectedPossibleRoom.Tone;
		}
		newMapRootGameObject.SetActive(value: false);
		newMapRootGameObject.SetActive(value: true);
		map.Position = GloomUtility.VToCV(newMapRootGameObject.transform.position);
		map.Rotation = GloomUtility.VToCV(newMapRootGameObject.transform.eulerAngles);
		map.Angle = newMapRootGameObject.transform.eulerAngles.y;
		AllMaps.Add(newMapRootGameObject);
	}

	private void SetupDoorsFromFullLoad(ScenarioState state, bool firstLoad)
	{
		foreach (CObjectDoor item in state.Props.OfType<CObjectDoor>())
		{
			GameObject propObject = Singleton<ObjectCacheService>.Instance.GetPropObject(item);
			if (propObject != null)
			{
				ApparanceLayer.Instance.CreateLevelEditor(item, propObject);
			}
		}
	}

	public void OnProcGenLoadCompleteCallback(Scene gameScene)
	{
		if (m_LevelEditorUIInstance == null)
		{
			AssetBundleManager.ReleaseHandle(_handleLevelEditorUI, releaseInstance: true);
			_handleLevelEditorUI = Addressables.InstantiateAsync(_referenceLevelEditorUI.RuntimeKey, base.transform, instantiateInWorldSpace: false, trackHandle: false);
			m_LevelEditorUIInstance = _handleLevelEditorUI.WaitForCompletion().GetComponent<LevelEditorUI>();
			m_LevelEditorUIInstance.TabTogglesController.RefreshAvailableToggles(m_CurrentLevelEditingState);
			m_LevelEditorUIInstance.transform.SetParent(null);
			SceneManager.MoveGameObjectToScene(m_LevelEditorUIInstance.gameObject, gameScene);
		}
		foreach (string enemyGuid in m_EnemyStateGuidsToSetHidden)
		{
			EnemyState enemyState = ScenarioManager.CurrentScenarioState.Monsters.FirstOrDefault((EnemyState a) => a.ActorGuid == enemyGuid);
			if (enemyState != null)
			{
				enemyState.IsRevealed = false;
			}
		}
		m_EnemyStateGuidsToSetHidden.Clear();
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			foreach (PlayerState item in map.Players.Where((PlayerState p) => !p.IsRevealed && !ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor pl) => pl.ActorGuid == p.ActorGuid)))
			{
				RevealHiddenCharacterInState(item, forLevelEditor: true);
			}
			foreach (EnemyState item2 in map.Monsters.Where((EnemyState e) => !e.IsRevealed))
			{
				item2.Load();
			}
		}
		EnsureApparanceOverrideExistsForGuidAndStyle("ScenarioApparanceOverrideGUID", MapSceneRoot.GetComponent<ProceduralStyle>());
		if (AllMaps.Count > 0)
		{
			CameraController.s_CameraController.m_TargetFocalPoint = AllMaps[0].transform.position;
		}
	}

	public bool AddRoomOfType(EMapType mapType)
	{
		string roomName = "Room_" + m_CurrentId;
		m_CurrentId++;
		CMap cMap = new CMap(roomName, mapType, string.Empty, new ScenarioPossibleRoom("Room_Room_" + m_CurrentId, string.Empty, ScenarioPossibleRoom.EBiome.Inherit, ScenarioPossibleRoom.ESubBiome.Inherit, ScenarioPossibleRoom.ETheme.Inherit, ScenarioPossibleRoom.ESubTheme.Inherit, ScenarioPossibleRoom.ETone.Inherit, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null), null)
		{
			Revealed = true
		};
		GameObject gameObject = TryCreateNewMap(cMap);
		if (gameObject == null)
		{
			Debug.Log("Failed to find valid position for new room at door");
			return false;
		}
		Singleton<ObjectCacheService>.Instance.AddMap(cMap, gameObject);
		ScenarioManager.CurrentScenarioState.Maps.Add(cMap);
		CameraController.s_CameraController.m_TargetFocalPoint = gameObject.transform.position;
		m_LevelEditorUIInstance.RoomPanel.RefreshRoomUIWithCurrentState();
		m_CurrentMovingObject = gameObject;
		CurrentPlacementState = ERoomPlacementState.PlacingRoom;
		s_Instance.m_LevelEditorUIInstance.RoomBeingPlaced(mapType.ToString());
		return true;
	}

	public bool AddDoorOfType(CObjectDoor.EDoorType doorType)
	{
		GameObject doorPrefab = GetDoorPrefab(doorType);
		if (doorPrefab == null)
		{
			return false;
		}
		if (CurrentPlacementState == ERoomPlacementState.PlacingDoor)
		{
			UnityEngine.Object.Destroy(m_CurrentMovingObject);
			m_CurrentMovingObject = null;
		}
		m_CurrentMovingObject = UnityEngine.Object.Instantiate(doorPrefab, MapSceneRoot.transform);
		m_LevelEditorUIInstance.RoomPanel.RefreshDoorUIWithCurrentState();
		CurrentPlacementState = ERoomPlacementState.PlacingDoor;
		s_Instance.m_LevelEditorUIInstance.DoorBeingPlaced(doorType.ToString());
		return true;
	}

	public bool LockRoomPlacement()
	{
		foreach (Transform item in MapSceneRoot.transform)
		{
			if (item == m_CurrentMovingObject.transform)
			{
				continue;
			}
			foreach (Transform item2 in item.transform)
			{
				foreach (Transform item3 in m_CurrentMovingObject.transform)
				{
					if (Vector3.Distance(item2.position, item3.position) < 0.1f)
					{
						UnityGameEditorObject component = item2.gameObject.GetComponent<UnityGameEditorObject>();
						UnityGameEditorObject component2 = item3.gameObject.GetComponent<UnityGameEditorObject>();
						if (!(component == null) && !(component2 == null) && (!(component != null) || !(component2 != null) || component.m_ObjectType != ScenarioManager.ObjectImportType.EdgeTile || component2.m_ObjectType != ScenarioManager.ObjectImportType.EdgeTile))
						{
							return false;
						}
					}
				}
			}
		}
		CMap cMap = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap x) => x.MapInstanceName == m_CurrentMovingObject.name);
		cMap.Position = GloomUtility.VToCV(m_CurrentMovingObject.transform.position);
		cMap.Rotation = GloomUtility.VToCV(m_CurrentMovingObject.transform.eulerAngles);
		cMap.Angle = m_CurrentMovingObject.transform.eulerAngles.y;
		return true;
	}

	public bool LockDoorPlacement()
	{
		BoxCollider component = m_CurrentMovingObject.GetComponent<BoxCollider>();
		Transform parent1 = null;
		Transform parent2 = null;
		foreach (Transform item in MapSceneRoot.transform)
		{
			bool flag = false;
			if (item == m_CurrentMovingObject.transform || !(component != null))
			{
				continue;
			}
			foreach (Transform item2 in item.transform)
			{
				UnityGameEditorObject component2 = item2.gameObject.GetComponent<UnityGameEditorObject>();
				if (!(component2 != null) || (component2.m_ObjectType != ScenarioManager.ObjectImportType.EdgeTile && component2.m_ObjectType != ScenarioManager.ObjectImportType.Tile))
				{
					continue;
				}
				Collider component3 = component2.GetComponent<Collider>();
				if (component3 != null && component.bounds.Intersects(component3.bounds))
				{
					if (parent1 == null)
					{
						parent1 = item;
						break;
					}
					if (parent2 == null)
					{
						parent2 = item;
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				break;
			}
		}
		if (parent1 != null)
		{
			CObjectDoor cObjectDoor = null;
			CMap cMap = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap x) => x.MapInstanceName == parent1.name);
			CMap cMap2 = null;
			if (parent2 != null)
			{
				cMap2 = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap x) => x.MapInstanceName == parent2.name);
			}
			if (cMap.EntranceDoor == null)
			{
				m_CurrentMovingObject.transform.SetParent(parent1);
				UnityGameEditorDoorProp component4 = m_CurrentMovingObject.GetComponent<UnityGameEditorDoorProp>();
				if (cMap2 == null)
				{
					component4.m_IsDungeonEntrance = true;
					if (!ApparanceLayer.IsThinDoor(component4.m_DoorType))
					{
						if (Vector3.Dot(m_CurrentMovingObject.transform.forward, (parent1.position - m_CurrentMovingObject.transform.position).normalized) < 0f)
						{
							m_CurrentMovingObject.transform.Rotate(0f, 180f, 0f, Space.Self);
						}
					}
					else if (Vector3.Dot(m_CurrentMovingObject.transform.right, (parent1.position - m_CurrentMovingObject.transform.position).normalized) < 0f)
					{
						m_CurrentMovingObject.transform.Rotate(0f, 180f, 0f, Space.Self);
					}
				}
				cObjectDoor = UnityGameEditorRuntime.MakeDoorLevelEditor(m_CurrentMovingObject, ScenarioManager.CurrentScenarioState);
				cMap.EntranceDoor = cObjectDoor.InstanceName;
				cMap2?.ExitDoors.Add(cObjectDoor.InstanceName);
			}
			else
			{
				bool flag2 = false;
				if (cMap2 != null && cMap2.EntranceDoor == null)
				{
					flag2 = true;
				}
				m_CurrentMovingObject.transform.SetParent(flag2 ? parent2 : parent1);
				cObjectDoor = UnityGameEditorRuntime.MakeDoorLevelEditor(m_CurrentMovingObject, ScenarioManager.CurrentScenarioState);
				cMap.ExitDoors.Add(cObjectDoor.InstanceName);
				if (cMap2 != null)
				{
					if (flag2)
					{
						cMap2.EntranceDoor = cObjectDoor.InstanceName;
					}
					else
					{
						cMap2.ExitDoors.Add(cObjectDoor.InstanceName);
					}
				}
			}
			if (parent2 == null)
			{
				cMap.DungeonEntranceDoor = cObjectDoor.InstanceName;
			}
			return true;
		}
		return false;
	}

	public void OnFinishedPlacingRooms()
	{
		CurrentLevelEditingState = ELevelEditingState.DoorPlacement;
	}

	public void OnBuildTileMap()
	{
		try
		{
			RefreshTileMapForCurrentState();
			RefreshPropPositionsForNewTileMap();
			ClientScenarioManager.Create(ScenarioManager.CurrentScenarioState, firstLoad: true);
			foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
			{
				GameObject gameObject = AllMaps.SingleOrDefault((GameObject x) => x.name == map.MapInstanceName);
				gameObject.GetComponent<ProceduralMapTile>().visibility = ProceduralMapTile.Visibility.All;
				gameObject.FindInChildren("Walls", includeInactive: true).SetActive(value: true);
				gameObject.GetComponent<RoomVisibilityTracker>().enabled = true;
				SetupDoorsForNewRoom(ScenarioManager.CurrentScenarioState, map, gameObject);
			}
			try
			{
				RefreshApparance();
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Refreshing Apparance threw error <IGNORE ME> msg:\n" + ex.Message + "\n" + ex.StackTrace);
			}
			CurrentLevelEditingState = ELevelEditingState.PropActorPlacement;
			if (m_DirectionalLight != null)
			{
				UnityEngine.Object.Destroy(m_DirectionalLight);
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError("Exception occurred in OnBuildTileMap: /n" + ex2.Message + "/n" + ex2.StackTrace);
		}
	}

	public GameObject TryCreateNewMap(CMap mapToLoad)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>("misc_mapsleveleditor", "Map " + mapToLoad.MapType, "mapsleveleditor"), MapSceneRoot.transform);
		gameObject.name = mapToLoad.MapInstanceName;
		ApparanceMap component = gameObject.GetComponent<ApparanceMap>();
		component.MapGuid = mapToLoad.MapGuid;
		component.RoomName = mapToLoad.RoomName;
		component.Tiles = UnityGameEditorRuntime.FindUnityGameObjects(gameObject, ScenarioManager.ObjectImportType.Tile);
		gameObject.GetComponent<RoomVisibilityTracker>().enabled = false;
		gameObject.GetComponent<ProceduralMapTile>().visibility = ProceduralMapTile.Visibility.Hidden;
		gameObject.FindInChildren("Walls").SetActive(value: false);
		gameObject.transform.eulerAngles = Vector3.zero;
		int num = 0;
		Vector3 zero = Vector3.zero;
		foreach (GameObject tile in component.Tiles)
		{
			zero += tile.transform.position;
			num++;
		}
		zero /= (float)num;
		mapToLoad.Centre = new CVector3(zero.x, zero.y, zero.z);
		GameObject gameObject2 = null;
		float num2 = float.MaxValue;
		foreach (GameObject tile2 in component.Tiles)
		{
			if ((tile2.transform.position - zero).magnitude < num2)
			{
				gameObject2 = tile2;
				num2 = (tile2.transform.position - zero).magnitude;
			}
		}
		mapToLoad.ClosestTileIdentityPosition = new CVector3(gameObject2.transform.position.x, gameObject2.transform.position.y, gameObject2.transform.position.z);
		mapToLoad.Angle = gameObject.transform.eulerAngles.y;
		InitNewMap(gameObject, mapToLoad);
		return gameObject;
	}

	public void TryDeleteRoom(CMap mapToRemove)
	{
		try
		{
			GameObject map = Singleton<ObjectCacheService>.Instance.GetMap(mapToRemove);
			if (ScenarioManager.CurrentScenarioState.Maps.Any((CMap cMap2) => cMap2.RoomName == mapToRemove.ParentName))
			{
				CMap cMap = ScenarioManager.CurrentScenarioState.Maps.Single((CMap cMap2) => cMap2.RoomName == mapToRemove.ParentName);
				cMap.Children.Remove(mapToRemove.MapGuid);
				cMap.ExitDoors.Remove(mapToRemove.EntranceDoor);
			}
			ScenarioManager.CurrentScenarioState.Props.RemoveAll((CObjectProp prop) => mapToRemove.Props.Contains(prop));
			ScenarioManager.CurrentScenarioState.Maps.RemoveAll((CMap cMap2) => cMap2.MapGuid == mapToRemove.MapGuid);
			Singleton<ObjectCacheService>.Instance.RemoveMap(mapToRemove);
			AllMaps.Remove(map);
			UnityEngine.Object.DestroyImmediate(map);
			RefreshTileMapForCurrentState();
			RefreshPropPositionsForNewTileMap();
			ClientScenarioManager.Create(ScenarioManager.CurrentScenarioState, firstLoad: true);
			m_LevelEditorUIInstance.RoomPanel.RefreshRoomUIWithCurrentState();
		}
		catch (Exception ex)
		{
			Debug.LogError("Error trying to delete map with Guid " + mapToRemove.MapGuid + "\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void TryDeleteDoor(CObjectDoor doorPropToRemove)
	{
		UnityEngine.Object.DestroyImmediate(Singleton<ObjectCacheService>.Instance.GetPropObject(doorPropToRemove));
		CMap cMap = ScenarioManager.CurrentScenarioState.Maps.Single((CMap map) => map.MapGuid == doorPropToRemove.StartingMapGuid);
		if (cMap.EntranceDoor == doorPropToRemove.InstanceName)
		{
			cMap.EntranceDoor = null;
		}
		if (cMap.ExitDoors.Contains(doorPropToRemove.InstanceName))
		{
			cMap.ExitDoors.Remove(doorPropToRemove.InstanceName);
		}
		if (cMap.DungeonExitDoor == doorPropToRemove.InstanceName)
		{
			cMap.DungeonExitDoor = null;
		}
		if (cMap.DungeonEntranceDoor == doorPropToRemove.InstanceName)
		{
			cMap.DungeonEntranceDoor = null;
		}
		ScenarioManager.CurrentScenarioState.Props.Remove(doorPropToRemove);
		m_LevelEditorUIInstance.RoomPanel.RefreshDoorUIWithCurrentState();
	}

	public void TryRotateDoor(CObjectDoor doorPropToRotate)
	{
		GameObject propObject = Singleton<ObjectCacheService>.Instance.GetPropObject(doorPropToRotate);
		propObject.transform.Rotate(0f, 60f, 0f, Space.Self);
		Vector3Int vector3Int = MF.GetTileIntegerSnapSpace(propObject.transform.position) + new Vector3Int(ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.X, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Y, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Z);
		doorPropToRotate.SetLocation(new TileIndex(vector3Int.x, vector3Int.z), new CVector3(propObject.transform.position.x, propObject.transform.position.y, propObject.transform.position.z), new CVector3(propObject.transform.localEulerAngles.x, propObject.transform.localEulerAngles.y, propObject.transform.localEulerAngles.z));
	}

	public void TryRotateProp(CObjectProp propToRotate)
	{
		GameObject propObject = Singleton<ObjectCacheService>.Instance.GetPropObject(propToRotate);
		propObject.transform.Rotate(0f, 30f, 0f, Space.Self);
		Vector3Int vector3Int = MF.GetTileIntegerSnapSpace(propObject.transform.position) + new Vector3Int(ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.X, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Y, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Z);
		propToRotate.SetLocation(new TileIndex(vector3Int.x, vector3Int.z), new CVector3(propObject.transform.position.x, propObject.transform.position.y, propObject.transform.position.z), new CVector3(propObject.transform.eulerAngles.x, propObject.transform.eulerAngles.y, propObject.transform.eulerAngles.z));
	}

	public string CanRoomBeDeleted(CMap mapToTryDelete)
	{
		string text = "";
		if (ScenarioManager.CurrentScenarioState.Props.Any((CObjectProp prop) => prop.ObjectType != ScenarioManager.ObjectImportType.Door))
		{
			text = text + (string.IsNullOrEmpty(text) ? "" : ", ") + "props in level";
		}
		if (ScenarioManager.Scenario.PlayerActors.Count > 0)
		{
			text = text + (string.IsNullOrEmpty(text) ? "" : ", ") + "players in level";
		}
		if (ScenarioManager.Scenario.Enemies.Count > 0)
		{
			text = text + (string.IsNullOrEmpty(text) ? "" : ", ") + "enemies in level";
		}
		if (mapToTryDelete.Children != null && mapToTryDelete.Children.Count > 0)
		{
			text = text + (string.IsNullOrEmpty(text) ? "" : ", ") + "child room" + ((mapToTryDelete.Children.Count > 1) ? "s" : "");
		}
		return text;
	}

	public bool CanRoomBeAdded()
	{
		if (CurrentLevelEditingState != ELevelEditingState.RoomPlacement)
		{
			return false;
		}
		return true;
	}

	public bool CanDoorBeDeleted()
	{
		if (CurrentLevelEditingState != ELevelEditingState.DoorPlacement)
		{
			return false;
		}
		return true;
	}

	public bool CanDoorBeAdded()
	{
		if (CurrentLevelEditingState != ELevelEditingState.DoorPlacement)
		{
			return false;
		}
		return true;
	}

	public void SetBiomeForRoom(CMap mapToSet, ScenarioPossibleRoom.EBiome biome)
	{
		if (mapToSet.SelectedPossibleRoom == null)
		{
			return;
		}
		mapToSet.SelectedPossibleRoom.SetBiome(biome);
		GameObject map = Singleton<ObjectCacheService>.Instance.GetMap(mapToSet);
		if (!map)
		{
			return;
		}
		ProceduralStyle[] componentsInChildren = map.GetComponentsInChildren<ProceduralStyle>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			ProceduralStyle[] array = componentsInChildren;
			foreach (ProceduralStyle obj in array)
			{
				obj.Biome = biome;
				obj.ForceValidate();
			}
		}
	}

	public void SetSubBiomeForRoom(CMap mapToSet, ScenarioPossibleRoom.ESubBiome subBiome)
	{
		if (mapToSet.SelectedPossibleRoom == null)
		{
			return;
		}
		mapToSet.SelectedPossibleRoom.SetSubBiome(subBiome);
		GameObject map = Singleton<ObjectCacheService>.Instance.GetMap(mapToSet);
		if (!map)
		{
			return;
		}
		ProceduralStyle[] componentsInChildren = map.GetComponentsInChildren<ProceduralStyle>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			ProceduralStyle[] array = componentsInChildren;
			foreach (ProceduralStyle obj in array)
			{
				obj.SubBiome = subBiome;
				obj.ForceValidate();
			}
		}
	}

	public void SetThemeForRoom(CMap mapToSet, ScenarioPossibleRoom.ETheme theme)
	{
		if (mapToSet.SelectedPossibleRoom == null)
		{
			return;
		}
		mapToSet.SelectedPossibleRoom.SetTheme(theme);
		GameObject map = Singleton<ObjectCacheService>.Instance.GetMap(mapToSet);
		if (!map)
		{
			return;
		}
		ProceduralStyle[] componentsInChildren = map.GetComponentsInChildren<ProceduralStyle>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			ProceduralStyle[] array = componentsInChildren;
			foreach (ProceduralStyle obj in array)
			{
				obj.Theme = theme;
				obj.ForceValidate();
			}
		}
	}

	public void SetSubThemeForRoom(CMap mapToSet, ScenarioPossibleRoom.ESubTheme subTheme)
	{
		if (mapToSet.SelectedPossibleRoom == null)
		{
			return;
		}
		mapToSet.SelectedPossibleRoom.SetSubTheme(subTheme);
		GameObject map = Singleton<ObjectCacheService>.Instance.GetMap(mapToSet);
		if (!map)
		{
			return;
		}
		ProceduralStyle[] componentsInChildren = map.GetComponentsInChildren<ProceduralStyle>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			ProceduralStyle[] array = componentsInChildren;
			foreach (ProceduralStyle obj in array)
			{
				obj.SubTheme = subTheme;
				obj.ForceValidate();
			}
		}
	}

	public void SetToneForRoom(CMap mapToSet, ScenarioPossibleRoom.ETone tone)
	{
		if (mapToSet.SelectedPossibleRoom == null)
		{
			return;
		}
		mapToSet.SelectedPossibleRoom.SetTone(tone);
		GameObject map = Singleton<ObjectCacheService>.Instance.GetMap(mapToSet);
		if (!map)
		{
			return;
		}
		ProceduralStyle[] componentsInChildren = map.GetComponentsInChildren<ProceduralStyle>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			ProceduralStyle[] array = componentsInChildren;
			foreach (ProceduralStyle obj in array)
			{
				obj.Tone = tone;
				obj.ForceValidate();
			}
		}
	}

	private void SetupDoorsForNewRoom(ScenarioState state, CMap newMap, GameObject newMapObject)
	{
		foreach (CObjectDoor item in from door in state.Props.OfType<CObjectDoor>()
			where door.StartingMapGuid == newMap.MapGuid
			select door)
		{
			GameObject propObject = Singleton<ObjectCacheService>.Instance.GetPropObject(item);
			if (propObject != null)
			{
				ApparanceLayer.Instance.CreateLevelEditor(item, propObject);
			}
		}
	}

	private void RefreshTileMapForCurrentState()
	{
		Vector3Int vector3Int = new Vector3Int(int.MaxValue, 0, int.MaxValue);
		Vector3Int vector3Int2 = new Vector3Int(int.MinValue, 0, int.MinValue);
		Vector3Int zero = Vector3Int.zero;
		Vector3Int zero2 = Vector3Int.zero;
		bool flag = false;
		foreach (Transform item in MapSceneRoot.transform)
		{
			foreach (Transform item2 in item.transform)
			{
				UnityGameEditorObject component = item2.gameObject.GetComponent<UnityGameEditorObject>();
				if (component != null && (component.m_ObjectType == ScenarioManager.ObjectImportType.EdgeTile || component.m_ObjectType == ScenarioManager.ObjectImportType.Tile))
				{
					Vector3Int tileIntegerSnapSpace = MF.GetTileIntegerSnapSpace(item2.position);
					if (vector3Int.z > tileIntegerSnapSpace.z)
					{
						flag = (Mathf.Abs(tileIntegerSnapSpace.z) & 1) == 1;
					}
					vector3Int.x = ((vector3Int.x > tileIntegerSnapSpace.x) ? tileIntegerSnapSpace.x : vector3Int.x);
					vector3Int.z = ((vector3Int.z > tileIntegerSnapSpace.z) ? tileIntegerSnapSpace.z : vector3Int.z);
					vector3Int2.x = ((vector3Int2.x < tileIntegerSnapSpace.x) ? tileIntegerSnapSpace.x : vector3Int2.x);
					vector3Int2.z = ((vector3Int2.z < tileIntegerSnapSpace.z) ? tileIntegerSnapSpace.z : vector3Int2.z);
				}
			}
		}
		zero2 = new Vector3Int(-vector3Int.x + 1, 0, -vector3Int.z);
		if (flag)
		{
			zero2.z++;
		}
		zero = new Vector3Int(vector3Int2.x - vector3Int.x + 1, 0, vector3Int2.z - vector3Int.z + 1);
		int num = 0;
		foreach (Transform item3 in MapSceneRoot.transform)
		{
			ApparanceMap mapInfo = item3.gameObject.GetComponent<ApparanceMap>();
			CMap cMap = ScenarioManager.CurrentScenarioState.Maps.Single((CMap s) => s.MapGuid == mapInfo.MapGuid);
			item3.name = cMap.MapInstanceName;
			cMap.MapTiles.Clear();
			foreach (Transform item4 in item3.transform)
			{
				UnityGameEditorObject component2 = item4.gameObject.GetComponent<UnityGameEditorObject>();
				if (component2 != null && (component2.m_ObjectType == ScenarioManager.ObjectImportType.EdgeTile || component2.m_ObjectType == ScenarioManager.ObjectImportType.Tile))
				{
					Vector3Int vector3Int3 = MF.GetTileIntegerSnapSpace(item4.position) + zero2;
					CMapTile cMapTile = new CMapTile(component2.m_ObjectType == ScenarioManager.ObjectImportType.EdgeTile, new TileIndex(vector3Int3.x, vector3Int3.z), GloomUtility.VToCV(item4.position), SharedClient.GlobalRNG);
					item4.name = cMapTile.TileGuid;
					Singleton<ObjectCacheService>.Instance.AddTile(cMapTile, item4.gameObject);
					cMap.MapTiles.Add(cMapTile);
				}
			}
			num++;
		}
		CVectorInt3 positiveSpaceOffset = new CVectorInt3(zero2.x, zero2.y, zero2.z);
		ScenarioManager.CurrentScenarioState.InitScenarioState(zero.x, zero.z, positiveSpaceOffset);
	}

	private void RefreshPropPositionsForNewTileMap()
	{
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			GameObject propObject = Singleton<ObjectCacheService>.Instance.GetPropObject(prop);
			Vector3Int vector3Int = MF.GetTileIntegerSnapSpace(propObject.transform.position) + new Vector3Int(ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.X, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Y, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Z);
			prop.SetLocation(new TileIndex(vector3Int.x, vector3Int.z), new CVector3(propObject.transform.position.x, propObject.transform.position.y, propObject.transform.position.z), new CVector3(propObject.transform.localEulerAngles.x, propObject.transform.localEulerAngles.y, propObject.transform.localEulerAngles.z));
		}
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			Vector3Int vector3Int2 = MF.GetTileIntegerSnapSpace(Choreographer.s_Choreographer.FindClientActorGameObject(playerActor).transform.position) + new Vector3Int(ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.X, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Y, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Z);
			playerActor.ArrayIndex = new Point(vector3Int2.x, vector3Int2.z);
		}
		foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
		{
			Vector3Int vector3Int3 = MF.GetTileIntegerSnapSpace(Choreographer.s_Choreographer.FindClientActorGameObject(enemy).transform.position) + new Vector3Int(ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.X, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Y, ScenarioManager.CurrentScenarioState.PositiveSpaceOffset.Z);
			enemy.ArrayIndex = new Point(vector3Int3.x, vector3Int3.z);
		}
	}

	[ContextMenu("RefreshApparance")]
	private void RefreshApparance()
	{
		GameObject[] array = UnityEngine.Object.FindObjectsOfType<GameObject>();
		foreach (GameObject gameObject in array)
		{
			if (!gameObject.activeInHierarchy)
			{
				continue;
			}
			UnityGameEditorObject component = gameObject.GetComponent<UnityGameEditorObject>();
			if (component != null && component.m_InvisableAtRuntime)
			{
				MeshRenderer component2 = gameObject.GetComponent<MeshRenderer>();
				if (component2 != null)
				{
					component2.enabled = false;
				}
			}
		}
		MapSceneRoot.GetComponent<ProceduralScenario>().SetupWalls();
	}

	public void ToggleRoomVisibility(CMap map)
	{
		if (!WallVisibilityOverrides.ContainsKey(map.MapGuid))
		{
			WallVisibilityOverrides.Add(map.MapGuid, value: true);
		}
		WallVisibilityOverrides[map.MapGuid] = !WallVisibilityOverrides[map.MapGuid];
		GameObject map2 = Singleton<ObjectCacheService>.Instance.GetMap(map);
		if (map2 != null)
		{
			RoomVisibilityTracker[] componentsInChildren = map2.GetComponentsInChildren<RoomVisibilityTracker>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].ShowMaptile(WallVisibilityOverrides[map.MapGuid]);
			}
			TilesOcclusionVolume componentInChildren = map2.GetComponentInChildren<TilesOcclusionVolume>();
			TilesOcclusionGenerator.s_Instance.AddVolume(componentInChildren);
		}
		TilesOcclusionGenerator.s_Instance?.UpdateAwaitingVolumes(removeNotVisibleRenderers: true);
	}

	public GameObject GetDoorPrefab(CObjectDoor.EDoorType doorType)
	{
		switch (doorType)
		{
		case CObjectDoor.EDoorType.ThinDoor:
			return ThinDoorPrefab;
		case CObjectDoor.EDoorType.ThickDoor:
			return ThickDoorPrefab;
		case CObjectDoor.EDoorType.ThinNarrowDoor:
			return ThinNarrowDoorPrefab;
		case CObjectDoor.EDoorType.ThickNarrowDoor:
			return ThickNarrowDoorPrefab;
		default:
			Debug.LogError("Trying to add door not supported by level editor");
			return null;
		}
	}

	public bool GetRoomVisibilityOverride(CMap forMap)
	{
		if (forMap == null)
		{
			return true;
		}
		bool result = true;
		if (WallVisibilityOverrides.ContainsKey(forMap.MapGuid))
		{
			result = WallVisibilityOverrides[forMap.MapGuid];
		}
		return result;
	}

	public void SetRoomRevealed(CMap mapToToggle, bool revealed)
	{
		mapToToggle.Revealed = revealed;
		if (mapToToggle.Props.FirstOrDefault((CObjectProp s) => s.InstanceName == mapToToggle.EntranceDoor) is CObjectDoor { IsDungeonEntrance: false } cObjectDoor)
		{
			SetDoorOpen(revealed, cObjectDoor);
		}
		foreach (PlayerState player in mapToToggle.Players)
		{
			player.IsRevealed = revealed;
		}
		foreach (EnemyState monster in mapToToggle.Monsters)
		{
			monster.IsRevealed = revealed;
		}
		foreach (EnemyState allyMonster in mapToToggle.AllyMonsters)
		{
			allyMonster.IsRevealed = revealed;
		}
		foreach (EnemyState enemy2Monster in mapToToggle.Enemy2Monsters)
		{
			enemy2Monster.IsRevealed = revealed;
		}
		foreach (EnemyState neutralMonster in mapToToggle.NeutralMonsters)
		{
			neutralMonster.IsRevealed = revealed;
		}
		foreach (ObjectState @object in mapToToggle.Objects)
		{
			@object.IsRevealed = revealed;
		}
	}

	public void SetPlayerRevealed(PlayerState playerStateToReveal, bool revealed)
	{
		playerStateToReveal.IsRevealed = revealed;
		playerStateToReveal.HiddenAtStart = !playerStateToReveal.IsRevealed;
	}

	public void SetEnemyRevealed(EnemyState enemyStateToReveal, bool revealed)
	{
		enemyStateToReveal.IsRevealed = revealed;
	}

	public void SetDoorOpen(bool shouldOpen, CObjectDoor door)
	{
		if (!shouldOpen && door.Activated)
		{
			ScenarioManager.PathFinder.Nodes[door.ArrayIndex.X, door.ArrayIndex.Y].IsBridgeOpen = false;
			MF.GameObjectAnimatorPlay(Singleton<ObjectCacheService>.Instance.GetPropObject(door), "Idle");
			door.SetActivatedFromLevelEditor(activate: false);
		}
		else if (shouldOpen && !door.Activated)
		{
			ScenarioManager.PathFinder.Nodes[door.ArrayIndex.X, door.ArrayIndex.Y].IsBridgeOpen = true;
			MF.GameObjectAnimatorPlay(Singleton<ObjectCacheService>.Instance.GetPropObject(door), "Open");
			door.SetActivatedFromLevelEditor(activate: true);
		}
	}

	public void SetDoorType(CObjectDoor doorProp, CObjectDoor.EDoorType typeToSet)
	{
		CObjectDoor.EDoorType doorType = doorProp.DoorType;
		doorProp.SetDoorType(typeToSet);
		if (doorType != doorProp.DoorType)
		{
			GameObject map = Singleton<ObjectCacheService>.Instance.GetMap(doorProp.HexMap);
			GameObject propObject = Singleton<ObjectCacheService>.Instance.GetPropObject(doorProp);
			GameObject gameObject = UnityEngine.Object.Instantiate(GetDoorPrefab(doorProp.DoorType), map.transform);
			gameObject.name = propObject.name;
			gameObject.transform.position = propObject.transform.position;
			gameObject.transform.rotation = propObject.transform.rotation;
			gameObject.transform.SetSiblingIndex(propObject.transform.GetSiblingIndex());
			UnityEngine.Object.DestroyImmediate(propObject);
			UnityGameEditorDoorProp component = gameObject.GetComponent<UnityGameEditorDoorProp>();
			if (component != null)
			{
				component.m_InitiallyVisable = true;
				component.m_IsDungeonEntrance = doorProp.IsDungeonEntrance;
				component.m_IsDungeonExit = doorProp.IsDungeonExit;
			}
			ApparanceLayer.Instance.CreateLevelEditor(doorProp, gameObject);
			if (WallVisibilityOverrides.ContainsKey(doorProp.HexMap.MapGuid) && !WallVisibilityOverrides[doorProp.HexMap.MapGuid])
			{
				ToggleRoomVisibility(doorProp.HexMap);
			}
		}
	}

	public static void RegenerateApparance()
	{
		GameObject maps = RoomVisibilityManager.s_Instance.Maps;
		int seed = SharedClient.GlobalRNG.Next();
		if (maps != null)
		{
			ProceduralStyle component = maps.GetComponent<ProceduralStyle>();
			if (component != null)
			{
				component.Seed = seed;
				component.ForceValidate();
			}
		}
	}

	public void EnsureApparanceOverrideExistsForGuidAndStyle(string guid, ProceduralStyle style, bool overrideExistingWithCurrent = false)
	{
		bool flag = false;
		CApparanceOverrideDetails cApparanceOverrideDetails = SaveData.Instance.Global.CurrentEditorLevelData.ApparanceOverrideList.FirstOrDefault((CApparanceOverrideDetails o) => o.GUID == guid);
		if (cApparanceOverrideDetails == null)
		{
			cApparanceOverrideDetails = new CApparanceOverrideDetails(guid);
			flag = true;
		}
		if ((flag || overrideExistingWithCurrent) && style != null)
		{
			cApparanceOverrideDetails.OverrideSeed = style.Seed;
			cApparanceOverrideDetails.OverrideBiome = style.Biome;
			cApparanceOverrideDetails.OverrideSubBiome = style.SubBiome;
			cApparanceOverrideDetails.OverrideTheme = style.Theme;
			cApparanceOverrideDetails.OverrideSubTheme = style.SubTheme;
			cApparanceOverrideDetails.OverrideTone = style.Tone;
			style.FetchObjectHierarchyValues(out var objectIndex, out var _);
			cApparanceOverrideDetails.OverrideSiblingIndex = objectIndex;
		}
		AddOrReplaceApparanceOverride(cApparanceOverrideDetails);
	}

	public void AddOrReplaceApparanceOverride(CApparanceOverrideDetails overrideToAdd)
	{
		SaveData.Instance.Global.CurrentEditorLevelData.ApparanceOverrideList.RemoveAll((CApparanceOverrideDetails o) => o.GUID == overrideToAdd.GUID);
		SaveData.Instance.Global.CurrentEditorLevelData.ApparanceOverrideList.Add(overrideToAdd);
	}

	public static void SetBiome(ScenarioPossibleRoom.EBiome biome, bool saveToState = false)
	{
		try
		{
			if (saveToState)
			{
				foreach (CMap map in ScenarioManager.Scenario.Maps)
				{
					if (map.SelectedPossibleRoom == null)
					{
						Debug.LogWarning("Cannot set map.SelectedPossibleRoom details for map with guid [" + map.MapGuid + "]");
					}
					else
					{
						map.SelectedPossibleRoom.SetBiome(biome);
					}
				}
			}
			GameObject maps = RoomVisibilityManager.s_Instance.Maps;
			if (!(maps != null))
			{
				return;
			}
			ProceduralStyle[] componentsInChildren = maps.GetComponentsInChildren<ProceduralStyle>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				ProceduralStyle[] array = componentsInChildren;
				foreach (ProceduralStyle obj in array)
				{
					obj.Biome = biome;
					obj.ForceValidate();
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception trying to set Apparance Settings:" + ex.Message);
		}
	}

	public static void SetSubBiome(ScenarioPossibleRoom.ESubBiome subbiome, bool saveToState = false)
	{
		try
		{
			if (saveToState)
			{
				foreach (CMap map in ScenarioManager.Scenario.Maps)
				{
					if (map.SelectedPossibleRoom == null)
					{
						Debug.LogWarning("Cannot set map.SelectedPossibleRoom details for map with guid [" + map.MapGuid + "]");
					}
					else
					{
						map.SelectedPossibleRoom.SetSubBiome(subbiome);
					}
				}
			}
			GameObject maps = RoomVisibilityManager.s_Instance.Maps;
			if (!(maps != null))
			{
				return;
			}
			ProceduralStyle[] componentsInChildren = maps.GetComponentsInChildren<ProceduralStyle>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				ProceduralStyle[] array = componentsInChildren;
				foreach (ProceduralStyle obj in array)
				{
					obj.SubBiome = subbiome;
					obj.ForceValidate();
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception trying to set Apparance Settings:" + ex.Message);
		}
	}

	public static void SetTheme(ScenarioPossibleRoom.ETheme theme, bool saveToState = false)
	{
		try
		{
			if (saveToState)
			{
				foreach (CMap map in ScenarioManager.Scenario.Maps)
				{
					if (map.SelectedPossibleRoom == null)
					{
						Debug.LogWarning("Cannot set map.SelectedPossibleRoom details for map with guid [" + map.MapGuid + "]");
					}
					else
					{
						map.SelectedPossibleRoom.SetTheme(theme);
					}
				}
			}
			GameObject maps = RoomVisibilityManager.s_Instance.Maps;
			if (!(maps != null))
			{
				return;
			}
			ProceduralStyle[] componentsInChildren = maps.GetComponentsInChildren<ProceduralStyle>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				ProceduralStyle[] array = componentsInChildren;
				foreach (ProceduralStyle obj in array)
				{
					obj.Theme = theme;
					obj.ForceValidate();
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception trying to set Apparance Settings:" + ex.Message);
		}
	}

	public static void SetSubTheme(ScenarioPossibleRoom.ESubTheme subtheme, bool saveToState = false)
	{
		try
		{
			if (saveToState)
			{
				foreach (CMap map in ScenarioManager.Scenario.Maps)
				{
					if (map.SelectedPossibleRoom == null)
					{
						Debug.LogWarning("Cannot set map.SelectedPossibleRoom details for map with guid [" + map.MapGuid + "]");
					}
					else
					{
						map.SelectedPossibleRoom.SetSubTheme(subtheme);
					}
				}
			}
			GameObject maps = RoomVisibilityManager.s_Instance.Maps;
			if (!(maps != null))
			{
				return;
			}
			ProceduralStyle[] componentsInChildren = maps.GetComponentsInChildren<ProceduralStyle>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				ProceduralStyle[] array = componentsInChildren;
				foreach (ProceduralStyle obj in array)
				{
					obj.SubTheme = subtheme;
					obj.ForceValidate();
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception trying to set Apparance Settings:" + ex.Message);
		}
	}

	public static void SetTone(ScenarioPossibleRoom.ETone tone, bool saveToState = false)
	{
		try
		{
			if (saveToState)
			{
				foreach (CMap map in ScenarioManager.Scenario.Maps)
				{
					if (map.SelectedPossibleRoom == null)
					{
						Debug.LogWarning("Cannot set map.SelectedPossibleRoom details for map with guid [" + map.MapGuid + "]");
					}
					else
					{
						map.SelectedPossibleRoom.SetTone(tone);
					}
				}
			}
			GameObject maps = RoomVisibilityManager.s_Instance.Maps;
			if (!(maps != null))
			{
				return;
			}
			ProceduralStyle[] componentsInChildren = maps.GetComponentsInChildren<ProceduralStyle>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				ProceduralStyle[] array = componentsInChildren;
				foreach (ProceduralStyle obj in array)
				{
					obj.Tone = tone;
					obj.ForceValidate();
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception trying to set Apparance Settings:" + ex.Message);
		}
	}

	public static void BeginPlacingUnit(string unitID, LevelEditorUnitsPanel.UnitType unitType, int numberOfTileNeeded, CAreaEffect areaEffect = null)
	{
		s_Instance.m_ToPlaceObjectIdentifier = unitID;
		s_Instance.CaptureTile = true;
		s_Instance.CaptureTileUnitType = unitType;
		s_Instance.m_CaptureTileFunction = ECaptureTileFunction.SpawnUnit;
		s_Instance.m_CaptureTileExpectedCount = numberOfTileNeeded;
		s_Instance.m_CurrentlyCapturedTiles = new List<CClientTile>();
		s_Instance.CurrentAreaEffectSelection = areaEffect;
		if (areaEffect != null)
		{
			WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.LevelEditorSpawning;
		}
		if (unitType == LevelEditorUnitsPanel.UnitType.Prop)
		{
			s_Instance.m_LevelEditorUIInstance.ObjectBeingPlaced(unitID, s_Instance.m_CurrentlyCapturedTiles.Count + 1, s_Instance.m_CaptureTileExpectedCount);
		}
		else
		{
			s_Instance.m_LevelEditorUIInstance.ObjectBeingPlaced(unitID);
		}
	}

	public static void BeginMovingUnit(string unitGuid, LevelEditorUnitsPanel.UnitType unitType)
	{
		s_Instance.m_ToMoveObjectGuid = unitGuid;
		s_Instance.CaptureTile = true;
		s_Instance.CaptureTileUnitType = unitType;
		s_Instance.m_CaptureTileFunction = ECaptureTileFunction.MoveUnit;
		s_Instance.m_CaptureTileExpectedCount = 1;
		s_Instance.m_CurrentlyCapturedTiles = new List<CClientTile>();
		s_Instance.m_LevelEditorUIInstance.ObjectBeingPlaced(unitGuid);
	}

	public static void SelectTile(Action<CClientTile> tileSelectedAction)
	{
		s_Instance.CaptureTile = true;
		s_Instance.m_CaptureTileFunction = ECaptureTileFunction.RecordTile;
		s_Instance.m_TileSelectedAction = tileSelectedAction;
		s_Instance.m_CaptureTileExpectedCount = 1;
		s_Instance.m_CurrentlyCapturedTiles = new List<CClientTile>();
		s_Instance.m_LevelEditorUIInstance.TileBeingSelected();
	}

	public static bool InterceptTileAction(CClientTile clientTile)
	{
		if (s_Instance != null && s_Instance.CurrentState == ELevelEditorState.Editing && s_Instance.CaptureTile)
		{
			s_Instance.m_CurrentlyCapturedTiles.Add(clientTile);
			if (s_Instance.m_CurrentlyCapturedTiles.Count >= s_Instance.m_CaptureTileExpectedCount)
			{
				if (s_Instance.CurrentAreaEffectSelection != null)
				{
					List<CTile> list = new List<CTile>();
					int length = ScenarioManager.Tiles.GetLength(1);
					int length2 = ScenarioManager.Tiles.GetLength(0);
					for (int i = 0; i < length2; i++)
					{
						for (int j = 0; j < length; j++)
						{
							list.Add(ScenarioManager.Tiles[i, j]);
						}
					}
					List<CTile> list2 = new List<CTile>();
					List<CTile> validTilesIncludingBlockedOut = null;
					list2.AddRange(CAreaEffect.GetValidTiles(clientTile.m_Tile, clientTile.m_Tile, s_Instance.CurrentAreaEffectSelection, WorldspaceStarHexDisplay.Instance.AreaEffectAngle, list, getBlocked: false, ref validTilesIncludingBlockedOut));
					foreach (CTile item2 in list2)
					{
						CClientTile item = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y];
						if (!s_Instance.m_CurrentlyCapturedTiles.Contains(item))
						{
							s_Instance.m_CurrentlyCapturedTiles.Add(item);
						}
					}
				}
				switch (s_Instance.m_CaptureTileFunction)
				{
				case ECaptureTileFunction.SpawnUnit:
				{
					CNode cNode = ScenarioManager.PathFinder.Nodes[clientTile.m_Tile.m_ArrayIndex.X, clientTile.m_Tile.m_ArrayIndex.Y];
					if (clientTile.m_Tile.m_Props.Count != 0 || !cNode.Walkable || cNode.Blocked)
					{
						s_Instance.m_CurrentlyCapturedTiles.Remove(clientTile);
						return false;
					}
					switch (s_Instance.CaptureTileUnitType)
					{
					case LevelEditorUnitsPanel.UnitType.Enemy:
						if (SpawnMonsterAtLocation(s_Instance.m_CurrentlyCapturedTiles[0], s_Instance.m_ToPlaceObjectIdentifier))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedPlacingUnit(s_Instance.m_ToPlaceObjectIdentifier, s_Instance.m_CurrentlyCapturedTiles[0], s_Instance.CaptureTileUnitType);
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					case LevelEditorUnitsPanel.UnitType.Objects:
						if (SpawnObjectAtLocation(s_Instance.m_CurrentlyCapturedTiles[0], s_Instance.m_ToPlaceObjectIdentifier))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedPlacingUnit(s_Instance.m_ToPlaceObjectIdentifier, s_Instance.m_CurrentlyCapturedTiles[0], s_Instance.CaptureTileUnitType);
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					case LevelEditorUnitsPanel.UnitType.Player:
						if (SpawnCharacterAtLocation(s_Instance.m_CurrentlyCapturedTiles[0], s_Instance.m_ToPlaceObjectIdentifier))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedPlacingUnit(s_Instance.m_ToPlaceObjectIdentifier, s_Instance.m_CurrentlyCapturedTiles[0], s_Instance.CaptureTileUnitType);
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					case LevelEditorUnitsPanel.UnitType.Spawner:
					{
						string text = Guid.NewGuid().ToString();
						if (SpawnSpawnerAtLocation(s_Instance.m_CurrentlyCapturedTiles, s_Instance.m_ToPlaceObjectIdentifier, text))
						{
							s_Instance.m_ToPlaceObjectIdentifier = text;
							s_Instance.m_LevelEditorUIInstance.FinishedPlacingUnit(s_Instance.m_ToPlaceObjectIdentifier, s_Instance.m_CurrentlyCapturedTiles[0], s_Instance.CaptureTileUnitType);
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					}
					case LevelEditorUnitsPanel.UnitType.Prop:
						if (SpawnPropAtLocation(s_Instance.m_CurrentlyCapturedTiles, s_Instance.m_ToPlaceObjectIdentifier))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedPlacingUnit(s_Instance.m_ToPlaceObjectIdentifier, s_Instance.m_CurrentlyCapturedTiles[0], s_Instance.CaptureTileUnitType);
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					}
					break;
				}
				case ECaptureTileFunction.MoveUnit:
					switch (s_Instance.CaptureTileUnitType)
					{
					case LevelEditorUnitsPanel.UnitType.Enemy:
						if (MoveMonsterToLocation(clientTile, s_Instance.m_ToMoveObjectGuid))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedMovingUnit();
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					case LevelEditorUnitsPanel.UnitType.Objects:
						if (MoveObjectToLocation(clientTile, s_Instance.m_ToMoveObjectGuid))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedMovingUnit();
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					case LevelEditorUnitsPanel.UnitType.Player:
						if (MoveCharacterToLocation(clientTile, s_Instance.m_ToMoveObjectGuid))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedMovingUnit();
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					case LevelEditorUnitsPanel.UnitType.Prop:
						if (MovePropToLocation(clientTile, s_Instance.m_ToMoveObjectGuid))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedMovingUnit();
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					case LevelEditorUnitsPanel.UnitType.Spawner:
						if (MoveSpawnerToLocation(clientTile, s_Instance.m_ToMoveObjectGuid))
						{
							s_Instance.m_LevelEditorUIInstance.FinishedMovingUnit();
						}
						else
						{
							s_Instance.m_LevelEditorUIInstance.FailedToEditObject(s_Instance.CaptureTileUnitType);
						}
						break;
					default:
						Debug.LogError($"MoveUnit not implemented yet for unit:{s_Instance.CaptureTileUnitType}. Please, implement it.");
						break;
					}
					break;
				case ECaptureTileFunction.RecordTile:
					s_Instance.m_TileSelectedAction?.Invoke(clientTile);
					s_Instance.m_TileSelectedAction = null;
					s_Instance.m_LevelEditorUIInstance.FinishedAction();
					break;
				}
				s_Instance.CaptureTile = false;
				s_Instance.m_CaptureTileExpectedCount = 0;
				s_Instance.m_CurrentlyCapturedTiles = new List<CClientTile>();
				WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone;
			}
			else
			{
				s_Instance.m_LevelEditorUIInstance.ObjectBeingPlaced(s_Instance.m_ToMoveObjectGuid, s_Instance.m_CurrentlyCapturedTiles.Count + 1, s_Instance.m_CaptureTileExpectedCount);
			}
			return true;
		}
		return false;
	}

	public void ShowLocationIndicator(Vector3 locationToShowAt)
	{
		if (m_LocationIndicatorRoutine != null)
		{
			StopCoroutine(m_LocationIndicatorRoutine);
			m_LocationIndicatorRoutine = null;
		}
		if (m_CurrentLocationIndicator != null)
		{
			UnityEngine.Object.Destroy(m_CurrentLocationIndicator);
		}
		m_LocationIndicatorRoutine = ShowLocationIndicatorForATime(locationToShowAt);
		StartCoroutine(m_LocationIndicatorRoutine);
	}

	private IEnumerator ShowLocationIndicatorForATime(Vector3 atlocation)
	{
		m_CurrentLocationIndicator = UnityEngine.Object.Instantiate(LocationIndicatorPrefab, atlocation, Quaternion.identity);
		yield return Timekeeper.instance.WaitForSeconds(2f);
		if (m_CurrentLocationIndicator != null)
		{
			UnityEngine.Object.Destroy(m_CurrentLocationIndicator);
		}
	}

	public static int AddScenarioModifierToScenario(CScenarioModifier scenarioModifierToAdd)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null)
		{
			currentScenarioState.ScenarioModifiers.Add(scenarioModifierToAdd);
			return currentScenarioState.ScenarioModifiers.Count - 1;
		}
		return -1;
	}

	public static void ReplaceScenarioModifierInScenarioAtIndex(int scenarioModifierIndex, CScenarioModifier scenarioModifierToReplaceWith)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null && scenarioModifierIndex < currentScenarioState.ScenarioModifiers.Count)
		{
			currentScenarioState.ScenarioModifiers[scenarioModifierIndex] = scenarioModifierToReplaceWith;
		}
	}

	public static void DeleteScenarioModifierFromScenario(int scenarioModifierIndex)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null && scenarioModifierIndex < currentScenarioState.ScenarioModifiers.Count)
		{
			currentScenarioState.ScenarioModifiers.RemoveAt(scenarioModifierIndex);
		}
	}

	public static int AddWinObjectiveToScenario(CObjective objectiveToAdd)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null)
		{
			currentScenarioState.WinObjectives.Add(objectiveToAdd);
			return currentScenarioState.WinObjectives.Count - 1;
		}
		return -1;
	}

	public static int AddLoseObjectiveToScenario(CObjective objectiveToAdd)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null)
		{
			currentScenarioState.LoseObjectives.Add(objectiveToAdd);
			return currentScenarioState.LoseObjectives.Count - 1;
		}
		return -1;
	}

	public static void ReplaceWinObjectiveInScenarioAtIndex(int objectiveIndex, CObjective objectiveToReplaceWith)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null && objectiveIndex < currentScenarioState.WinObjectives.Count)
		{
			currentScenarioState.WinObjectives[objectiveIndex] = objectiveToReplaceWith;
		}
	}

	public static void ReplaceLoseObjectiveInScenarioAtIndex(int objectiveIndex, CObjective objectiveToReplaceWith)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null && objectiveIndex < currentScenarioState.LoseObjectives.Count)
		{
			currentScenarioState.LoseObjectives[objectiveIndex] = objectiveToReplaceWith;
		}
	}

	public static void DeleteWinObjectiveFromScenario(int objectiveIndex)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null && objectiveIndex < currentScenarioState.WinObjectives.Count)
		{
			currentScenarioState.WinObjectives.RemoveAt(objectiveIndex);
		}
	}

	public static void DeleteLoseObjectiveFromScenario(int objectiveIndex)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null && objectiveIndex < currentScenarioState.LoseObjectives.Count)
		{
			currentScenarioState.LoseObjectives.RemoveAt(objectiveIndex);
		}
	}

	public void ExitToMainMenuThroughUI()
	{
		if (IsEditing)
		{
			MainMenuUIManager.SetLoadingCompleteCallback(OnExitedToMainMenuFromLevelEditorCallback);
		}
		QuitLevelEditor();
	}

	public void OnExitedToMainMenuFromLevelEditorCallback()
	{
		MainMenuUIManager.Instance.ModeSelectionScreen.ShowForLevelEditor(autoSelectRuleset: true, LastLoadedRuleset);
	}

	public static bool SpawnSpawnerAtLocation(List<CClientTile> tiles, string spawnerType, string newSpawnerGuid)
	{
		if (tiles == null || tiles.Count < 1)
		{
			Debug.Log("Failed to spawn prop - Incorrect tile count");
			return false;
		}
		foreach (CClientTile tile in tiles)
		{
			if ((!CAbilityFilter.IsValidTile(tile.m_Tile, CAbilityFilter.EFilterTile.EmptyHex) && tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.MoneyToken) == null) || tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) != null)
			{
				Debug.LogError("Failed to spawn prop - One of the tiles chosen was invalid");
				return false;
			}
			foreach (CClientTile item2 in tiles.Where((CClientTile t) => t != tile).ToList())
			{
				if (!ScenarioManager.IsTileAdjacent(tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y, item2.m_Tile.m_ArrayIndex.X, item2.m_Tile.m_ArrayIndex.Y))
				{
					Debug.LogError("Failed to spawn prop - Tiles chosen were not adjacent");
					return false;
				}
			}
			Debug.Log("Currently Captured tile| X:" + tile.m_Tile.m_ArrayIndex.X + " Y:" + tile.m_Tile.m_ArrayIndex.Y);
		}
		if (!ScenarioManager.CurrentScenarioState.Spawners.Any((CSpawner p) => p.SpawnerGuid == newSpawnerGuid))
		{
			switch (spawnerType)
			{
			case "Spawner":
			{
				CSpawner item = CSpawnerExtensions.CreateDefaultSpawner(new TileIndex(tiles[0].m_Tile.m_ArrayIndex), tiles[0].m_Tile.m_HexMap.MapGuid, newSpawnerGuid);
				ScenarioManager.CurrentScenarioState.Spawners.Add(item);
				tiles[0].m_Tile.m_Spawners.Add(item);
				Debug.Log($"Spawned {spawnerType} on tile ({tiles[0].m_Tile.m_ArrayIndex.X}, {tiles[0].m_Tile.m_ArrayIndex.Y})");
				return true;
			}
			case "SewerPipeSpawner":
			{
				CInteractableSpawner cInteractableSpawner3 = CSpawnerExtensions.CreateDefaultInteractableSpawner(spawnerType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), tiles[0].m_Tile.m_HexMap.MapGuid, newSpawnerGuid);
				ScenarioManager.CurrentScenarioState.Spawners.Add(cInteractableSpawner3);
				tiles[0].m_Tile.m_Spawners.Add(cInteractableSpawner3);
				cInteractableSpawner3.CreateSpawnerProp();
				Debug.Log($"Spawned {spawnerType} on tile ({tiles[0].m_Tile.m_ArrayIndex.X}, {tiles[0].m_Tile.m_ArrayIndex.Y})");
				return true;
			}
			case "GraveSingleSpawner":
			{
				CInteractableSpawner cInteractableSpawner2 = CSpawnerExtensions.CreateDefaultInteractableSpawner(spawnerType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), tiles[0].m_Tile.m_HexMap.MapGuid, newSpawnerGuid);
				ScenarioManager.CurrentScenarioState.Spawners.Add(cInteractableSpawner2);
				tiles[0].m_Tile.m_Spawners.Add(cInteractableSpawner2);
				cInteractableSpawner2.CreateSpawnerProp();
				Debug.Log($"Spawned {spawnerType} on tile ({tiles[0].m_Tile.m_ArrayIndex.X}, {tiles[0].m_Tile.m_ArrayIndex.Y})");
				return true;
			}
			case "GraveDoubleSpawner":
			{
				List<TileIndex> pathingBlockers = tiles.Select((CClientTile t) => new TileIndex(t.m_Tile.m_ArrayIndex)).ToList();
				Quaternion quaternion = Quaternion.identity;
				int num = 0;
				if (tiles.Count == 2)
				{
					quaternion = Quaternion.LookRotation(tiles[0].m_GameObject.transform.position - tiles[1].m_GameObject.transform.position, Vector3.up);
					num = 90;
				}
				Vector3 eulerAngles = quaternion.eulerAngles;
				eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y + (float)num, eulerAngles.z);
				CVector3 rotation = GloomUtility.VToCV(eulerAngles);
				CInteractableSpawner cInteractableSpawner = CSpawnerExtensions.CreateDefaultInteractableSpawner(spawnerType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), tiles[0].m_Tile.m_HexMap.MapGuid, newSpawnerGuid);
				ScenarioManager.CurrentScenarioState.Spawners.Add(cInteractableSpawner);
				tiles[0].m_Tile.m_Spawners.Add(cInteractableSpawner);
				cInteractableSpawner.CreateSpawnerProp(pathingBlockers, rotation);
				Debug.Log($"Spawned {spawnerType} on tile ({tiles[0].m_Tile.m_ArrayIndex.X}, {tiles[0].m_Tile.m_ArrayIndex.Y})");
				return true;
			}
			}
		}
		else
		{
			Debug.LogError("Data for Spawner:" + newSpawnerGuid + " already exists in the party, so a new one was not added");
		}
		return false;
	}

	public static bool SpawnMonsterAtLocation(CClientTile tile, string monsterID, int level = 1, CActor.EType type = CActor.EType.Enemy, bool playSummonedAnim = false)
	{
		CActor cActor = ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex);
		if ((cActor == null || cActor.IsDead) && (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) == null || ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y].IsBridgeOpen))
		{
			CMonsterClass cMonsterClass = MonsterClassManager.Find(monsterID);
			cMonsterClass.SetMonsterStatLevel(level);
			if (cMonsterClass != null)
			{
				int num = -1;
				if (s_Instance.IsEditing)
				{
					int checkID = 1;
					while (num == -1)
					{
						if (!ScenarioManager.Scenario.AllMonsters.Any((CEnemyActor x) => x.MonsterClass.ID == monsterID && x.StandeeID == checkID))
						{
							num = checkID;
							cMonsterClass.RemoveAvailableID(num);
							break;
						}
						checkID++;
					}
				}
				else
				{
					num = cMonsterClass.GetNextID();
				}
				if (num == -1)
				{
					Debug.LogError("Maximum enemies of this type are already added to the scenario");
					return false;
				}
				int num2;
				for (num2 = num; num2 > cMonsterClass.Models.Count - 1; num2 -= cMonsterClass.Models.Count)
				{
				}
				EnemyState enemyState = new EnemyState(cMonsterClass.ID, num2, null, tile.m_Tile.m_HexMap.MapGuid, new TileIndex(tile.m_Tile.m_ArrayIndex), cMonsterClass.Health(), cMonsterClass.Health(), level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, isSummon: false, null, 1, type);
				enemyState.ID = num;
				switch (type)
				{
				case CActor.EType.Ally:
					if (ScenarioManager.Scenario.AddAllyMonster(enemyState, initial: false) == null)
					{
						Debug.LogError("Maximum enemies of this type are already added to the scenario");
						return false;
					}
					ScenarioManager.CurrentScenarioState.AllyMonsters.Add(enemyState);
					Choreographer.s_Choreographer.m_ClientAllyMonsters.Add(Choreographer.s_Choreographer.CreateCharacterActor(tile, ScenarioManager.Scenario.AllyMonsters.Last()));
					break;
				case CActor.EType.Enemy2:
					if (ScenarioManager.Scenario.AddEnemy2Monster(enemyState, initial: false) == null)
					{
						Debug.LogError("Maximum enemies of this type are already added to the scenario");
						return false;
					}
					ScenarioManager.CurrentScenarioState.Enemy2Monsters.Add(enemyState);
					Choreographer.s_Choreographer.m_ClientEnemy2Monsters.Add(Choreographer.s_Choreographer.CreateCharacterActor(tile, ScenarioManager.Scenario.Enemy2Monsters.Last()));
					break;
				case CActor.EType.Neutral:
					if (ScenarioManager.Scenario.AddNeutralMonster(enemyState, initial: false) == null)
					{
						Debug.LogError("Maximum enemies of this type are already added to the scenario");
						return false;
					}
					ScenarioManager.CurrentScenarioState.NeutralMonsters.Add(enemyState);
					Choreographer.s_Choreographer.m_ClientNeutralMonsters.Add(Choreographer.s_Choreographer.CreateCharacterActor(tile, ScenarioManager.Scenario.NeutralMonsters.Last()));
					break;
				default:
					if (ScenarioManager.Scenario.AddEnemy(enemyState, initial: false) == null)
					{
						Debug.LogError("Maximum enemies of this type are already added to the scenario");
						return false;
					}
					ScenarioManager.CurrentScenarioState.Monsters.Add(enemyState);
					Choreographer.s_Choreographer.m_ClientEnemies.Add(Choreographer.s_Choreographer.CreateCharacterActor(tile, ScenarioManager.Scenario.Enemies.Last(), playSummonedAnim));
					break;
				}
				enemyState.IsRevealed = tile.m_Tile.m_HexMap.Revealed;
				Choreographer.s_Choreographer.SetCharacterPositions();
				Debug.Log("Spawned " + LocalizationManager.GetTranslation(cMonsterClass.LocKey) + " on tile (" + tile.m_Tile.m_ArrayIndex.X + ", " + tile.m_Tile.m_ArrayIndex.Y + ")");
				ScenarioManager.CurrentScenarioState.Update();
				return true;
			}
			Debug.LogError("Unable to find monster " + monsterID);
		}
		else
		{
			Debug.LogError("Invalid location selected for monster to spawn.");
		}
		return false;
	}

	public static bool SpawnObjectAtLocation(CClientTile tile, string objectID, int level = 1)
	{
		if (ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex) == null && (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) == null || ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y].IsBridgeOpen))
		{
			CObjectClass cObjectClass = MonsterClassManager.FindObjectClass(objectID);
			cObjectClass.SetMonsterStatLevel(level);
			if (cObjectClass != null)
			{
				int nextID = cObjectClass.GetNextID();
				if (nextID == -1)
				{
					Debug.LogError("Maximum enemies of this type are already added to the scenario");
					return false;
				}
				int num;
				for (num = nextID; num > cObjectClass.Models.Count - 1; num -= cObjectClass.Models.Count)
				{
				}
				ObjectState objectState = new ObjectState(cObjectClass.ID, num, null, tile.m_Tile.m_HexMap.MapGuid, new TileIndex(tile.m_Tile.m_ArrayIndex), cObjectClass.Health(), cObjectClass.Health(), level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, isSummon: false, null, 1, CActor.EType.Enemy);
				objectState.ID = nextID;
				if (ScenarioManager.Scenario.AddObject(objectState, initial: false) == null)
				{
					Debug.LogError("Maximum enemies of this type are already added to the scenario");
					return false;
				}
				ScenarioManager.CurrentScenarioState.Objects.Add(objectState);
				objectState.IsRevealed = tile.m_Tile.m_HexMap.Revealed;
				Choreographer.s_Choreographer.m_ClientObjects.Add(Choreographer.s_Choreographer.CreateCharacterActor(tile, ScenarioManager.Scenario.Objects.Last()));
				Choreographer.s_Choreographer.SetCharacterPositions();
				Debug.Log("Spawned " + LocalizationManager.GetTranslation(cObjectClass.LocKey) + " on tile (" + tile.m_Tile.m_ArrayIndex.X + ", " + tile.m_Tile.m_ArrayIndex.Y + ")");
				ScenarioManager.CurrentScenarioState.Update();
				return true;
			}
			Debug.LogError("Unable to find monster " + objectID);
		}
		else
		{
			Debug.LogError("Invalid location selected for monster to spawn.");
		}
		return false;
	}

	public static bool SpawnCharacterAtLocation(CClientTile tile, string characterID)
	{
		if (ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex) == null && tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) == null)
		{
			CCharacterClass characterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == characterID);
			if (characterClass != null)
			{
				if (!ScenarioManager.CurrentScenarioState.Players.Any((PlayerState p) => p.ClassID == characterID))
				{
					List<CAbilityCard> list = new List<CAbilityCard>();
					list = CharacterClassManager.AllAbilityCards.Where((CAbilityCard x) => x.ClassID == characterClass.ID).ToList();
					List<Tuple<int, int>> selectedCardIDs = (from c in (from c in list.GetRange(0, list.Count)
							select new { c.ID, c.CardInstanceID }).AsEnumerable()
						select new Tuple<int, int>(c.ID, c.CardInstanceID)).ToList();
					PlayerState playerState = new PlayerState(characterID, 0, null, tile.m_Tile.m_HexMap.MapGuid, new TileIndex(tile.m_Tile.m_ArrayIndex), characterClass.Health(), characterClass.Health(), 1, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: false, CActor.ECauseOfDeath.StillAlive, 1, null, 0, 0, isLongResting: false, new AbilityDeckState(selectedCardIDs), new AttackModifierDeckState(characterClass), new List<CItem>())
					{
						IsRevealed = true
					};
					ScenarioManager.CurrentScenarioState.Players.Add(playerState);
					ScenarioManager.Scenario.AddPlayer(playerState, initial: false);
					playerState.IsRevealed = tile.m_Tile.m_HexMap.Revealed;
					playerState.HiddenAtStart = !playerState.IsRevealed;
					Choreographer.s_Choreographer.m_ClientPlayers.Add(Choreographer.s_Choreographer.CreateCharacterActor(tile, ScenarioManager.Scenario.PlayerActors.Last()));
					Choreographer.s_Choreographer.SetCharacterPositions();
					Debug.Log("Spawned " + LocalizationManager.GetTranslation(characterClass.LocKey) + " on tile (" + tile.m_Tile.m_ArrayIndex.X + ", " + tile.m_Tile.m_ArrayIndex.Y + ")");
					return true;
				}
				Debug.LogError("Data for Character " + characterID + " already exists in the party, so a new one was not added");
			}
			else
			{
				Debug.LogError("Unable to find Character " + characterID);
			}
		}
		else
		{
			Debug.LogError("Invalid location selected for character to spawn.");
		}
		return false;
	}

	public static bool SpawnPropAtLocation(List<CClientTile> tiles, string propName)
	{
		if (tiles == null || tiles.Count < 1)
		{
			Debug.Log("Failed to spawn prop - Incorrect tile count");
			return false;
		}
		ScenarioManager.ObjectImportType objectImportType = GlobalSettings.GetObjectImportType(propName);
		foreach (CClientTile tile in tiles)
		{
			if (!CObjectProp.IsLootableObjectImportType(objectImportType))
			{
				if (objectImportType == ScenarioManager.ObjectImportType.Tile || objectImportType == ScenarioManager.ObjectImportType.TerrainHotCoals || objectImportType == ScenarioManager.ObjectImportType.TerrainRubble || objectImportType == ScenarioManager.ObjectImportType.TerrainThorns || objectImportType == ScenarioManager.ObjectImportType.TerrainWater || objectImportType == ScenarioManager.ObjectImportType.TerrainVisualEffect)
				{
					List<ScenarioManager.ObjectImportType> typesToCheckFor = new List<ScenarioManager.ObjectImportType>
					{
						ScenarioManager.ObjectImportType.TerrainHotCoals,
						ScenarioManager.ObjectImportType.TerrainRubble,
						ScenarioManager.ObjectImportType.TerrainThorns,
						ScenarioManager.ObjectImportType.TerrainWater,
						ScenarioManager.ObjectImportType.TerrainVisualEffect,
						ScenarioManager.ObjectImportType.Obstacle,
						ScenarioManager.ObjectImportType.PressurePlate
					};
					if (tile.m_Tile.CheckForPropTypes(typesToCheckFor))
					{
						return false;
					}
				}
				else if ((!CAbilityFilter.IsValidTile(tile.m_Tile, CAbilityFilter.EFilterTile.EmptyHex) && tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.MoneyToken) == null) || tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) != null)
				{
					Debug.LogError("Failed to spawn prop - One of the tiles chosen was invalid");
					return false;
				}
			}
			Debug.Log("Currently Captured tile| X:" + tile.m_Tile.m_ArrayIndex.X + " Y:" + tile.m_Tile.m_ArrayIndex.Y);
		}
		switch (objectImportType)
		{
		case ScenarioManager.ObjectImportType.GenericProp:
		{
			CObjectProp prop14 = new CObjectProp(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop14);
			Debug.Log("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.MoneyToken:
		{
			CObjectGoldPile prop12 = new CObjectGoldPile(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop12);
			Debug.Log("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.Chest:
		{
			CObjectChest prop13 = new CObjectChest(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop13);
			Debug.Log("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.Obstacle:
		{
			List<TileIndex> pathingBlockers = tiles.Select((CClientTile t) => new TileIndex(t.m_Tile.m_ArrayIndex)).ToList();
			Quaternion quaternion = Quaternion.identity;
			int num = 0;
			if (tiles.Count == 2)
			{
				quaternion = Quaternion.LookRotation(tiles[1].m_GameObject.transform.position - tiles[0].m_GameObject.transform.position, Vector3.up);
				num = 90;
			}
			else if (tiles.Count == 3)
			{
				if (propName == "ThreeHexCurvedObstacle")
				{
					Vector3 vector = tiles[0].m_GameObject.transform.position - tiles[1].m_GameObject.transform.position;
					Vector3 vector2 = tiles[0].m_GameObject.transform.position - tiles[2].m_GameObject.transform.position;
					quaternion = Quaternion.LookRotation(vector - vector2, Vector3.up);
					num = 60;
				}
				else if (propName == "ThreeHexStraightObstacle")
				{
					quaternion = Quaternion.LookRotation(tiles[2].m_GameObject.transform.position - tiles[0].m_GameObject.transform.position, Vector3.up);
					num = 90;
				}
				else
				{
					Vector3 vector3 = tiles[1].m_GameObject.transform.position - tiles[0].m_GameObject.transform.position;
					Vector3 vector4 = tiles[2].m_GameObject.transform.position - tiles[0].m_GameObject.transform.position;
					quaternion = Quaternion.LookRotation((Vector3.Cross(vector3, vector4).y < 0f) ? vector3 : vector4, Vector3.up);
					num = 150;
				}
			}
			Vector3 eulerAngles = quaternion.eulerAngles;
			eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y + (float)num, eulerAngles.z);
			CVector3 rotation = GloomUtility.VToCV(eulerAngles);
			CObjectObstacle prop11 = new CObjectObstacle(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, rotation, pathingBlockers, null, tiles[0].m_Tile.m_HexMap.MapGuid, ignoresFlyAndJump: false, tiles.Count > 1);
			tiles[0].m_Tile.SpawnProp(prop11);
			Debug.Log("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.Trap:
		{
			CObjectTrap prop10 = new CObjectTrap(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid, new List<CCondition.ENegativeCondition>(), damage: true, 1, 0, 0, new List<CCondition.ENegativeCondition>(), null, 0);
			tiles[0].m_Tile.SpawnProp(prop10);
			Debug.Log("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.GoalChest:
		{
			CObjectChest prop9 = new CObjectChest(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop9);
			Debug.Log("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.PressurePlate:
		{
			CObjectPressurePlate prop8 = new CObjectPressurePlate(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop8);
			Debug.Log("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.TerrainHotCoals:
		case ScenarioManager.ObjectImportType.TerrainThorns:
		{
			CObjectHazardousTerrain prop7 = new CObjectHazardousTerrain(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop7);
			Debug.Log("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.TerrainWater:
		case ScenarioManager.ObjectImportType.TerrainRubble:
		{
			CObjectDifficultTerrain prop6 = new CObjectDifficultTerrain(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid, treatAsTrap: false, new CObjectiveFilter());
			tiles[0].m_Tile.SpawnProp(prop6);
			Debug.Log("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.Portal:
		{
			CObjectPortal prop5 = new CObjectPortal(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid, new CObjectiveFilter());
			tiles[0].m_Tile.SpawnProp(prop5);
			Debug.Log("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.CarryableQuestItem:
		{
			CObjectQuestItem prop4 = new CObjectQuestItem(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop4);
			Debug.Log("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.TerrainVisualEffect:
		{
			CObjectTerrainVisual prop3 = new CObjectTerrainVisual(propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop3);
			Debug.Log("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.Resource:
		{
			CObjectResource prop2 = new CObjectResource(null, propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop2);
			Debug.Log("Spawned prop " + propName);
			break;
		}
		case ScenarioManager.ObjectImportType.MonsterGrave:
		{
			CObjectMonsterGrave prop = new CObjectMonsterGrave(isEliteGrave: false, propName, objectImportType, new TileIndex(tiles[0].m_Tile.m_ArrayIndex), null, null, null, tiles[0].m_Tile.m_HexMap.MapGuid);
			tiles[0].m_Tile.SpawnProp(prop);
			Debug.Log("Spawned prop " + propName);
			break;
		}
		default:
			Debug.LogError("Unsupported prop type!");
			break;
		}
		return true;
	}

	public static bool SpawnSpawnerAtLocation(List<CClientTile> tiles)
	{
		if (tiles == null || tiles.Count < 1)
		{
			Debug.Log("Failed to spawn spawner - Incorrect tile count");
			return false;
		}
		SpawnRoundEntry item = new SpawnRoundEntry(new List<string> { "", "Bandit Archer", "Bandit Guard", "Bandit Guard" });
		Dictionary<string, List<SpawnRoundEntry>> dictionary = new Dictionary<string, List<SpawnRoundEntry>>();
		dictionary.Add("Default", new List<SpawnRoundEntry> { item });
		List<int> spawnRoundInterval = new List<int> { 1, 1, 1, 1 };
		SpawnerData spawnerData = new SpawnerData(SpawnerData.ESpawnerTriggerType.StartRound, SpawnerData.ESpawnerActivationType.ScenarioStart, 2, loopSpawnPattern: true, shouldCountTowardsKillAllEnemies: false, spawnRoundInterval, dictionary, "");
		List<CMapTile> list = new List<CMapTile>();
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			foreach (CClientTile tile in tiles)
			{
				if (tile.m_Tile.m_HexMap == map || tile.m_Tile.m_Hex2Map == map)
				{
					list.Add(map.MapTiles.SingleOrDefault((CMapTile m) => m.ArrayIndex.X == tile.m_Tile.m_ArrayIndex.X && m.ArrayIndex.Y == tile.m_Tile.m_ArrayIndex.Y));
				}
			}
		}
		CSpawner spawner = new CSpawner(spawnerData, list[0].ArrayIndex, tiles[0].m_Tile.m_HexMap.MapGuid);
		tiles[0].m_Tile.SpawnSpawner(spawner);
		return true;
	}

	public static bool RevealHiddenCharacterInState(PlayerState stateToReveal, bool forLevelEditor = false)
	{
		try
		{
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.AddPlayer(stateToReveal, initial: false);
			stateToReveal.Load(forceLoad: true);
			stateToReveal.LoadAbiltyDeck(forceLoad: true);
			if (!forLevelEditor)
			{
				stateToReveal.IsRevealed = true;
			}
			Choreographer.s_Choreographer.m_ClientPlayers.Add(Choreographer.s_Choreographer.CreateCharacterActor(CAutoTileClick.TileIndexToClientTile(new TileIndex(cPlayerActor.ArrayIndex)), cPlayerActor));
			Choreographer.s_Choreographer.SetCharacterPositions();
			Debug.Log("Revealed " + LocalizationManager.GetTranslation(cPlayerActor.ActorLocKey()));
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to reveal hidden character\nException:" + ex.Message + "\nStack:" + ex.StackTrace);
			return false;
		}
	}

	public static bool RemoveMonster(CEnemyActor enemyToRemove)
	{
		try
		{
			enemyToRemove.MonsterClass.RecycleID(enemyToRemove.ID);
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(enemyToRemove);
			ScenarioManager.Scenario.RemoveEnemy(enemyToRemove);
			ScenarioManager.Scenario.DeadEnemies.Remove(enemyToRemove);
			ScenarioManager.Scenario.RemoveAllyMonsters(enemyToRemove);
			ScenarioManager.Scenario.DeadAllyMonsters.Remove(enemyToRemove);
			ScenarioManager.Scenario.RemoveEnemy2Monster(enemyToRemove);
			ScenarioManager.Scenario.DeadEnemy2Monsters.Remove(enemyToRemove);
			ScenarioManager.Scenario.RemoveNeutralMonster(enemyToRemove);
			ScenarioManager.Scenario.DeadNeutralMonsters.Remove(enemyToRemove);
			ScenarioManager.CurrentScenarioState.Monsters.RemoveAll((EnemyState ec) => ec.ActorGuid == enemyToRemove.ActorGuid);
			ScenarioManager.CurrentScenarioState.AllyMonsters.RemoveAll((EnemyState ec) => ec.ActorGuid == enemyToRemove.ActorGuid);
			ScenarioManager.CurrentScenarioState.Enemy2Monsters.RemoveAll((EnemyState ec) => ec.ActorGuid == enemyToRemove.ActorGuid);
			ScenarioManager.CurrentScenarioState.NeutralMonsters.RemoveAll((EnemyState ec) => ec.ActorGuid == enemyToRemove.ActorGuid);
			ScenarioManager.CurrentScenarioState.ResetEnemyClassManager();
			ScenarioManager.CurrentScenarioState.Update();
			Choreographer.s_Choreographer.m_ClientEnemies.Remove(gameObject);
			Choreographer.s_Choreographer.m_ClientAllyMonsters.Remove(gameObject);
			Choreographer.s_Choreographer.m_ClientEnemy2Monsters.Remove(gameObject);
			Choreographer.s_Choreographer.m_ClientNeutralMonsters.Remove(gameObject);
			ActorBehaviour.GetActorBehaviour(gameObject).m_WorldspacePanelUI.Destroy();
			UnityEngine.Object.Destroy(gameObject);
			return true;
		}
		catch
		{
			Debug.LogError("Failed to remove Enemy: " + LocalizationManager.GetTranslation(enemyToRemove.ActorLocKey()));
			return false;
		}
	}

	public static bool RemoveObject(CObjectActor objectToRemove)
	{
		try
		{
			objectToRemove.MonsterClass.RecycleID(objectToRemove.ID);
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(objectToRemove);
			ScenarioManager.Scenario.RemoveObjectActor(objectToRemove);
			ScenarioManager.Scenario.DeadObjects.Remove(objectToRemove);
			ScenarioManager.CurrentScenarioState.Objects.RemoveAll((ObjectState ec) => ec.ActorGuid == objectToRemove.ActorGuid);
			Choreographer.s_Choreographer.m_ClientObjects.Remove(gameObject);
			ActorBehaviour.GetActorBehaviour(gameObject).m_WorldspacePanelUI.Destroy();
			UnityEngine.Object.Destroy(gameObject);
			return true;
		}
		catch
		{
			Debug.LogError("Failed to remove Obect: " + LocalizationManager.GetTranslation(objectToRemove.ActorLocKey()));
			return false;
		}
	}

	public static bool RemovePlayer(CPlayerActor playerToRemove)
	{
		try
		{
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(playerToRemove);
			ScenarioManager.Scenario.RemovePlayer(playerToRemove);
			ScenarioManager.Scenario.ExhaustedPlayers.Remove(playerToRemove);
			ScenarioManager.CurrentScenarioState.Players.RemoveAll((PlayerState pc) => pc.ActorGuid == playerToRemove.ActorGuid);
			Choreographer.s_Choreographer.m_ClientPlayers.Remove(gameObject);
			ActorBehaviour.GetActorBehaviour(gameObject).m_WorldspacePanelUI.Destroy();
			UnityEngine.Object.Destroy(gameObject);
			return true;
		}
		catch
		{
			Debug.LogError("Failed to remove Player Character: " + LocalizationManager.GetTranslation(playerToRemove.ActorLocKey()));
			return false;
		}
	}

	public static bool RemoveProp(CObjectProp propToRemove)
	{
		try
		{
			CObjectProp.FindPropTile(propToRemove.InstanceName)?.m_Props.RemoveAll((CObjectProp p) => p.PropGuid == propToRemove.PropGuid);
			ScenarioManager.CurrentScenarioState.Props.RemoveAll((CObjectProp p) => p.PropGuid == propToRemove.PropGuid);
			GameObject propObject = Singleton<ObjectCacheService>.Instance.GetPropObject(propToRemove);
			if (propToRemove.ObjectType == ScenarioManager.ObjectImportType.Tile || propToRemove.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || propToRemove.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble || propToRemove.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns || propToRemove.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || propToRemove.ObjectType == ScenarioManager.ObjectImportType.TerrainVisualEffect)
			{
				UnityEngine.Object.DestroyImmediate(propObject);
			}
			else
			{
				UnityEngine.Object.Destroy(propObject, 0f);
			}
			Debug.Log("Successfully removed prop" + propToRemove.PrefabName);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception trying to remove to remove prop: " + propToRemove.PrefabName + "\nException: " + ex.Message + "\nStack: " + ex.StackTrace);
			return false;
		}
	}

	public static bool RemoveSpawner(CSpawner spawnerToRemove)
	{
		try
		{
			ScenarioManager.CurrentScenarioState.Spawners.Remove(spawnerToRemove);
			if (spawnerToRemove is CInteractableSpawner { Prop: not null } cInteractableSpawner)
			{
				RemoveProp(cInteractableSpawner.Prop);
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception trying to remove to remove spawner: " + spawnerToRemove.SpawnerGuid + "\nException: " + ex.Message + "\nStack: " + ex.StackTrace);
			return false;
		}
	}

	public static bool MoveMonsterToLocation(CClientTile tile, string monsterGuid)
	{
		if (ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex) == null && (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) == null || ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y].IsBridgeOpen))
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.First((CEnemyActor enemyActor) => enemyActor.ActorGuid == monsterGuid);
			if (cEnemyActor != null)
			{
				ScenarioManager.CurrentScenarioState.Monsters.Single((EnemyState p) => p.ActorGuid == monsterGuid).Location = new TileIndex(tile.m_Tile.m_ArrayIndex);
				GameObject obj = Choreographer.s_Choreographer.FindClientActorGameObject(cEnemyActor);
				cEnemyActor.ArrayIndex = tile.m_Tile.m_ArrayIndex;
				ActorBehaviour.GetActorBehaviour(obj).ForceSetLocoIntermediateTarget(tile.m_GameObject.transform.position);
				return true;
			}
		}
		return false;
	}

	public static bool MoveObjectToLocation(CClientTile tile, string monsterGuid)
	{
		if (ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex) == null && (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) == null || ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y].IsBridgeOpen))
		{
			CObjectActor cObjectActor = ScenarioManager.Scenario.Objects.First((CObjectActor objectActor) => objectActor.ActorGuid == monsterGuid);
			if (cObjectActor != null)
			{
				ScenarioManager.CurrentScenarioState.Objects.Single((ObjectState p) => p.ActorGuid == monsterGuid).Location = new TileIndex(tile.m_Tile.m_ArrayIndex);
				GameObject obj = Choreographer.s_Choreographer.FindClientActorGameObject(cObjectActor);
				cObjectActor.ArrayIndex = tile.m_Tile.m_ArrayIndex;
				ActorBehaviour.GetActorBehaviour(obj).ForceSetLocoIntermediateTarget(tile.m_GameObject.transform.position);
				return true;
			}
		}
		return false;
	}

	public static bool MoveCharacterToLocation(CClientTile tile, string characterGuid)
	{
		if (ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex) == null && (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) == null || ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y].IsBridgeOpen))
		{
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.First((CPlayerActor playerActor) => playerActor.ActorGuid == characterGuid);
			if (cPlayerActor != null)
			{
				ScenarioManager.CurrentScenarioState.Players.Single((PlayerState p) => p.ActorGuid == characterGuid).Location = new TileIndex(tile.m_Tile.m_ArrayIndex);
				GameObject obj = Choreographer.s_Choreographer.FindClientActorGameObject(cPlayerActor);
				cPlayerActor.ArrayIndex = tile.m_Tile.m_ArrayIndex;
				ActorBehaviour.GetActorBehaviour(obj).ForceSetLocoIntermediateTarget(tile.m_GameObject.transform.position);
				return true;
			}
		}
		return false;
	}

	public static bool MovePropToLocation(CClientTile tile, string propGuid)
	{
		if (CAbilityFilter.IsValidTile(tile.m_Tile, CAbilityFilter.EFilterTile.EmptyHex))
		{
			CObjectProp cObjectProp = null;
			foreach (CMap map in ScenarioManager.Scenario.Maps)
			{
				if (map.Props.Any((CObjectProp prop) => prop.PropGuid == propGuid))
				{
					cObjectProp = map.Props.First((CObjectProp prop) => prop.PropGuid == propGuid);
					break;
				}
			}
			if (cObjectProp != null)
			{
				if (cObjectProp is CObjectObstacle cObjectObstacle && cObjectObstacle.PathingBlockers.Count > 1)
				{
					return false;
				}
				CTile cTile = CObjectProp.FindPropTile(cObjectProp.InstanceName);
				cTile.m_Props.RemoveAll((CObjectProp prop) => prop.PropGuid == propGuid);
				cTile.m_Hex2Map?.Props.RemoveAll((CObjectProp prop) => prop.PropGuid == propGuid);
				cTile.m_HexMap?.Props.RemoveAll((CObjectProp prop) => prop.PropGuid == propGuid);
				cObjectProp.SetLocation(new TileIndex(tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y), GloomUtility.VToCV(tile.m_GameObject.transform.position), cObjectProp.Rotation);
				tile.m_Tile.m_Props.Add(cObjectProp);
				cObjectProp.SetNewStartingMapGuid(tile.m_Tile.m_HexMap.MapGuid);
				Singleton<ObjectCacheService>.Instance.GetPropObject(cObjectProp).transform.position = tile.m_GameObject.transform.position;
				tile.m_Tile.m_HexMap?.Props.Add(cObjectProp);
				tile.m_Tile.m_Hex2Map?.Props.Add(cObjectProp);
				if (cObjectProp is CObjectObstacle cObjectObstacle2)
				{
					cObjectObstacle2.PathingBlockers.Clear();
					cObjectObstacle2.PathingBlockers.Add(new TileIndex(tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y));
				}
				return true;
			}
		}
		return false;
	}

	public static bool MoveSpawnerToLocation(CClientTile tile, string spawnerGuid)
	{
		if (CAbilityFilter.IsValidTile(tile.m_Tile, CAbilityFilter.EFilterTile.EmptyHex))
		{
			CSpawner cSpawner = null;
			foreach (CMap map in ScenarioManager.Scenario.Maps)
			{
				if (map.Spawners.Any((CSpawner prop) => prop.SpawnerGuid == spawnerGuid))
				{
					cSpawner = map.Spawners.First((CSpawner prop) => prop.SpawnerGuid == spawnerGuid);
					break;
				}
			}
			if (cSpawner != null)
			{
				cSpawner.SetLocation(new TileIndex(tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y));
				if (cSpawner.Prop != null)
				{
					cSpawner.Prop.SetLocation(new TileIndex(tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y), GloomUtility.VToCV(tile.m_GameObject.transform.position), cSpawner.Prop.Rotation);
					cSpawner.Prop.SetNewStartingMapGuid(tile.m_Tile.m_HexMap.MapGuid);
					Singleton<ObjectCacheService>.Instance.GetPropObject(cSpawner.Prop).transform.position = tile.m_GameObject.transform.position;
				}
				return true;
			}
		}
		return false;
	}

	private void SnapGameObjectToHexes(GameObject gameObject)
	{
		gameObject.GetComponent<UnityGameEditorObject>();
		Vector3 position = gameObject.transform.position;
		position.z = MF.SnapFloat(UnityGameEditorRuntime.s_TileSize.z, position.z);
		if ((Mathf.Abs(MF.GetTileIntegerSnapSpace(position).z) & 1) == 1)
		{
			position.x = MF.SnapFloat(UnityGameEditorRuntime.s_TileSize.x, position.x - UnityGameEditorRuntime.s_TileSize.x * 0.5f) + UnityGameEditorRuntime.s_TileSize.x * 0.5f;
		}
		else
		{
			position.x = MF.SnapFloat(UnityGameEditorRuntime.s_TileSize.x, position.x);
		}
		position.y = 0f;
		gameObject.transform.position = position;
	}

	public void ClearDestroyedAllMaps()
	{
		MapSceneRoot = null;
		if (AllMaps == null)
		{
			return;
		}
		for (int num = AllMaps.Count - 1; num >= 0; num--)
		{
			if (AllMaps[num] == null)
			{
				AllMaps.RemoveAt(num);
			}
		}
	}

	public void IterateOverAllAvailableLevels(float perLevelPostLoadDelay, UnityAction<UnityAction<string, bool>, CCustomLevelData> perLevelOperation, UnityAction<string> onCompleteOperation, bool loadIntoLevelEditor = true)
	{
		if (m_BulkLevelProcessingRoutine != null)
		{
			StopCoroutine(m_BulkLevelProcessingRoutine);
			m_BulkLevelProcessingRoutine = null;
		}
		List<CCustomLevelData> levels = SaveData.Instance.LevelEditorDataManager.AvailableFiles.Select((FileInfo f) => SaveData.Instance.LevelEditorDataManager.GetLevelDataForFile(f)).ToList();
		m_BulkLevelProcessingRoutine = LoopThoughLevelsAndProcess(levels, perLevelPostLoadDelay, perLevelOperation, onCompleteOperation, loadIntoLevelEditor);
		StartCoroutine(m_BulkLevelProcessingRoutine);
	}

	private IEnumerator LoopThoughLevelsAndProcess(List<CCustomLevelData> levels, float perLevelPostLoadDelay, UnityAction<UnityAction<string, bool>, CCustomLevelData> perLevelOperation, UnityAction<string> onCompleteOperation, bool loadIntoLevelEditor = true)
	{
		SceneController.Instance.DisableLoadingScreen();
		string reportString = "Bulk Process of [" + levels.Count + "] Levels" + ((levels.Count == 1) ? string.Empty : "s") + " Start " + DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") + "\n==================================================";
		new List<string>();
		int numberOfLevelsProcessed = 0;
		BulkLevelProcessingInProgress = true;
		Application.logMessageReceived += LogCallback;
		while (numberOfLevelsProcessed < levels.Count && !m_EndBulkRun)
		{
			if (loadIntoLevelEditor)
			{
				switch (CurrentState)
				{
				case ELevelEditorState.Idle:
					if (SaveData.Instance.Global.GameMode == EGameMode.MainMenu && !SceneController.Instance.IsLoading)
					{
						BulkProcessingErrorOccurred = false;
						reportString += $"\n\t{levels[numberOfLevelsProcessed].Name.ToUpper()}:\n\t=============";
						if (levels[numberOfLevelsProcessed].ScenarioState != null)
						{
							SaveData.Instance.LoadCustomLevelFromData(levels[numberOfLevelsProcessed], ELevelEditorState.Editing);
							SceneController.Instance.LevelEditorStart();
						}
						else
						{
							numberOfLevelsProcessed++;
						}
					}
					break;
				case ELevelEditorState.Editing:
				{
					while (TransitionManager.s_Instance == null || !TransitionManager.s_Instance.TransitionDone || SceneController.Instance.ScenarioIsLoading || SceneController.Instance.IsLoading)
					{
						yield return null;
					}
					yield return new WaitForSeconds(perLevelPostLoadDelay);
					bool isComplete = false;
					try
					{
						UnityAction<string, bool> arg = delegate(string processResult, bool completionFlag)
						{
							reportString += $"\n\t{processResult}";
							isComplete = completionFlag;
						};
						perLevelOperation(arg, SaveData.Instance.Global.CurrentEditorLevelData);
					}
					catch
					{
						isComplete = true;
					}
					while (!isComplete)
					{
						yield return null;
					}
					if (LoggedErrors.Count > 0)
					{
						reportString = reportString + ": \n\t\tThe following Exceptions occurred during this level process:\n\t\t\t" + LoggedErrors.Select((string s) => SecurityElement.Escape(s));
						LoggedErrors.Clear();
					}
					if (LoggedWarnings.Count > 0)
					{
						reportString = reportString + ": \n\t\tThe following Warnings occurred during this level process:\n\t\t\t" + LoggedWarnings.Select((string s) => SecurityElement.Escape(s));
						LoggedWarnings.Clear();
					}
					numberOfLevelsProcessed++;
					s_Instance?.ExitToMainMenuThroughUI();
					UIManager.LoadMainMenu();
					break;
				}
				}
			}
			else
			{
				BulkProcessingErrorOccurred = false;
				reportString += $"\n\t{levels[numberOfLevelsProcessed].Name.ToUpper()}:\n\t=============";
				if (levels[numberOfLevelsProcessed].ScenarioState != null)
				{
					yield return new WaitForSeconds(perLevelPostLoadDelay);
					bool isComplete2 = false;
					try
					{
						UnityAction<string, bool> arg2 = delegate(string processResult, bool completionFlag)
						{
							reportString += $"\n\t{processResult}";
							isComplete2 = completionFlag;
						};
						perLevelOperation(arg2, levels[numberOfLevelsProcessed].DeepCopySerializableObject<CCustomLevelData>());
					}
					catch
					{
						isComplete2 = true;
					}
					while (!isComplete2)
					{
						yield return null;
					}
					if (LoggedErrors.Count > 0)
					{
						reportString = reportString + ": \n\t\tThe following Exceptions occurred during this level process:\n\t\t\t" + LoggedErrors.Select((string s) => SecurityElement.Escape(s));
						LoggedErrors.Clear();
					}
					if (LoggedWarnings.Count > 0)
					{
						reportString = reportString + ": \n\t\tThe following Warnings occurred during this level process:\n\t\t\t" + LoggedWarnings.Select((string s) => SecurityElement.Escape(s));
						LoggedWarnings.Clear();
					}
					numberOfLevelsProcessed++;
				}
				else
				{
					numberOfLevelsProcessed++;
				}
			}
			yield return null;
		}
		m_EndBulkRun = false;
		Application.logMessageReceived -= LogCallback;
		try
		{
			List<string> list = new List<string>();
			while (reportString.Length > 16000)
			{
				int num = reportString.IndexOf('\n', 16000);
				if (num > 0 && num + 1 < reportString.Length)
				{
					list.Add(reportString.Substring(0, num));
					reportString = reportString.Substring(num + 1);
					continue;
				}
				list.Add(reportString);
				break;
			}
			if (reportString.Length > 0)
			{
				list.Add(reportString);
			}
			foreach (string item in list)
			{
				Debug.Log(item);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while trying to split report for debug log.\n" + ex.Message + "\n" + ex.StackTrace);
		}
		BulkLevelProcessingInProgress = false;
		onCompleteOperation?.Invoke(reportString);
	}

	private void LogCallback(string condition, string stacktrace, LogType type)
	{
		switch (type)
		{
		case LogType.Error:
		case LogType.Assert:
		case LogType.Exception:
			if (type != LogType.Error || !condition.Contains("AmplitudeHttp"))
			{
				LoggedErrors.Add(condition + "\n" + stacktrace);
			}
			break;
		case LogType.Warning:
			if (!condition.Contains("SendMessage"))
			{
				LoggedWarnings.Add(condition);
			}
			break;
		}
	}

	public void August12BulkLevelProcessingLoop()
	{
		UnityAction<UnityAction<string, bool>, CCustomLevelData> perLevelOperation = delegate(UnityAction<string, bool> processString, CCustomLevelData _irrelevantLevelData)
		{
			s_Instance.m_LevelEditorUIInstance.ApparancePanel.EnsureAllStylesHaveAnOverride();
			s_Instance.m_LevelEditorUIInstance.LevelDataPanel.OnSaveDataPressed();
			int num = 0;
			foreach (CObjectProp prop in SaveData.Instance.Global.CurrentEditorLevelData.ScenarioState.Props)
			{
				if (prop.PropHealthDetails != null && prop.PropHealthDetails.HasHealth)
				{
					num++;
				}
			}
			string text = $"{SaveData.Instance.Global.CurrentEditorLevelData.Name} Apparance overrides updated. It has {num} Props with health";
			Debug.Log(text);
			processString(text, arg1: true);
		};
		UnityAction<string> onCompleteOperation = delegate
		{
		};
		IterateOverAllAvailableLevels(10f, perLevelOperation, onCompleteOperation);
	}

	public void September8thRemovePlayerStatesFromCampaignScenariosBulkLevelProcessingLoop()
	{
		UnityAction<UnityAction<string, bool>, CCustomLevelData> perLevelOperation = delegate(UnityAction<string, bool> processString, CCustomLevelData _irrelevantLevelData)
		{
			for (int num = ScenarioManager.Scenario.PlayerActors.Count - 1; num >= 0; num--)
			{
				RemovePlayer(ScenarioManager.Scenario.PlayerActors[num]);
			}
			ScenarioManager.Scenario.ExhaustedPlayers.Clear();
			ScenarioManager.CurrentScenarioState.Players.Clear();
			string text = ((ScenarioManager.CurrentScenarioState.RoundNumber > 1) ? $"{SaveData.Instance.Global.CurrentEditorLevelData.Name} Scenario State round number was INCORRECT" : $"{SaveData.Instance.Global.CurrentEditorLevelData.Name} Scenario State round number was CORRECT");
			ScenarioManager.CurrentScenarioState.RoundNumber = 1;
			foreach (ActorState actorState in ScenarioManager.CurrentScenarioState.ActorStates)
			{
				actorState.ResetCauseOfDeath();
			}
			s_Instance.m_LevelEditorUIInstance.LevelDataPanel.OnSaveDataPressed();
			Debug.Log(text);
			processString(text, arg1: true);
		};
		UnityAction<string> onCompleteOperation = delegate
		{
		};
		IterateOverAllAvailableLevels(10f, perLevelOperation, onCompleteOperation);
	}

	public void Sept15thBulkLevelProcessingLoop()
	{
		UnityAction<UnityAction<string, bool>, CCustomLevelData> perLevelOperation = delegate(UnityAction<string, bool> processString, CCustomLevelData levelData)
		{
			int num = 0;
			foreach (CObjectProp prop in levelData.ScenarioState.Props)
			{
				if (prop.PropHealthDetails != null && prop.PropHealthDetails.HasHealth && prop.PropActorHasBeenAssigned)
				{
					num++;
					prop.ClearAttachedRuntimeActor();
				}
			}
			List<string> objectGuidsToRemove = new List<string>();
			foreach (ObjectState @object in levelData.ScenarioState.Objects)
			{
				if (@object.IsAttachedToProp)
				{
					objectGuidsToRemove.Add(@object.ActorGuid);
				}
			}
			levelData.ScenarioState.Objects.RemoveAll((ObjectState o) => objectGuidsToRemove.Contains(o.ActorGuid));
			string text = "Didn't need saving";
			if (num > 0 || objectGuidsToRemove.Count > 0)
			{
				text = (SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(levelData, levelData.Name, saveReadableJson: false, SaveData.Instance.Global.UseCustomLevelDataFolder) ? "Saved successfully" : "Failed to save");
			}
			string text2 = $"Level {levelData.Name} Processed. It had {num} Props with health needing updates, {objectGuidsToRemove.Count} Objects were removed from the state, {text}";
			Debug.Log(text2);
			processString(text2, arg1: true);
		};
		UnityAction<string> onCompleteOperation = delegate
		{
		};
		IterateOverAllAvailableLevels(0.5f, perLevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
	}

	public void BulkTransplantApparanceOverridesFromVerySpecificFolder()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo("C:\\Users\\seanw\\Desktop\\UnBorkage-Working\\Latest before applying fix");
		List<CCustomLevelData> levelsInFile = new List<CCustomLevelData>();
		if (directoryInfo.Exists)
		{
			FileInfo[] files = directoryInfo.GetFiles("*.lvldat", SearchOption.AllDirectories);
			foreach (FileInfo file in files)
			{
				CCustomLevelData levelDataForFile = SaveData.Instance.LevelEditorDataManager.GetLevelDataForFile(file);
				if (levelDataForFile != null)
				{
					levelsInFile.Add(levelDataForFile);
				}
			}
		}
		UnityAction<UnityAction<string, bool>, CCustomLevelData> perLevelOperation = delegate(UnityAction<string, bool> processString, CCustomLevelData levelData)
		{
			bool flag = false;
			bool flag2 = false;
			string text = "Didn't need saving";
			CCustomLevelData cCustomLevelData = levelsInFile.FirstOrDefault((CCustomLevelData l) => l.Name == levelData.Name);
			if (cCustomLevelData != null)
			{
				List<CApparanceOverrideDetails> list = cCustomLevelData.ApparanceOverrideList.Except(levelData.ApparanceOverrideList).ToList();
				List<CApparanceOverrideDetails> list2 = levelData.ApparanceOverrideList.Except(cCustomLevelData.ApparanceOverrideList).ToList();
				if (list.Count > 0 || list2.Count > 0)
				{
					levelData.ApparanceOverrideList = cCustomLevelData.ApparanceOverrideList.ToList();
					text = (SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(levelData, levelData.Name, saveReadableJson: false, SaveData.Instance.Global.UseCustomLevelDataFolder) ? "Saved successfully" : "Failed to save");
					flag = true;
				}
				flag2 = true;
			}
			string text2 = string.Format("Level {0} Processed. {1}, {2}, {3}", levelData.Name, flag2 ? "Successfully found a level to transplant from" : "Failed to find a level to transplant from", flag ? "Level needed a transplant" : "Level didn't need a transplant", text);
			Debug.Log(text2);
			processString(text2, arg1: true);
		};
		UnityAction<string> onCompleteOperation = delegate
		{
		};
		IterateOverAllAvailableLevels(0.5f, perLevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
	}

	public void BulkCheckForIncorrectPathingBlockersInObstacles()
	{
		UnityAction<UnityAction<string, bool>, CCustomLevelData> perLevelOperation = delegate(UnityAction<string, bool> processString, CCustomLevelData levelData)
		{
			List<CObjectObstacle> list = new List<CObjectObstacle>();
			foreach (CObjectProp prop in levelData.ScenarioState.Props)
			{
				CObjectObstacle obstacleProp = prop as CObjectObstacle;
				if (obstacleProp != null && !obstacleProp.PathingBlockers.Any((TileIndex x) => x.Equals(obstacleProp.ArrayIndex)))
				{
					list.Add(obstacleProp);
				}
			}
			string text = $"Level {levelData.Name} Processed. It had {list.Count} obstacle props needing replacing as they have incorrect PathingBlocker indexes.";
			Debug.Log(text);
			foreach (CObjectObstacle item in list)
			{
				Debug.Log($"Obstacle with GUID: {item.PropGuid} in Level {levelData.Name} has incorrect pathing blockers");
			}
			processString(text, arg1: true);
		};
		UnityAction<string> onCompleteOperation = delegate
		{
		};
		IterateOverAllAvailableLevels(0.5f, perLevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
	}

	public void BulkCheckForScenarioModifierLevelEvents()
	{
		UnityAction<UnityAction<string, bool>, CCustomLevelData> perLevelOperation = delegate(UnityAction<string, bool> processString, CCustomLevelData levelData)
		{
			int num = 0;
			foreach (CLevelEvent levelEvent in levelData.LevelEvents)
			{
				if (levelEvent.EventType == CLevelEvent.ELevelEventType.CloseDoor || levelEvent.EventType == CLevelEvent.ELevelEventType.UnlockDoor)
				{
					num++;
				}
			}
			string text = $"Level {levelData.Name} Processed. It had {num} level events that need replacing as closing/opening/unlocking doors from the main thread is not safe.";
			Debug.Log(text);
			processString(text, arg1: true);
		};
		UnityAction<string> onCompleteOperation = delegate
		{
		};
		IterateOverAllAvailableLevels(0.5f, perLevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
	}

	public void BulkFixTrapPropPrefabName()
	{
		UnityAction<UnityAction<string, bool>, CCustomLevelData> perLevelOperation = delegate(UnityAction<string, bool> processString, CCustomLevelData levelData)
		{
			int num = 0;
			foreach (CObjectProp prop in levelData.ScenarioState.Props)
			{
				if (prop.ObjectType == ScenarioManager.ObjectImportType.Trap && prop.PrefabName == "BearTrap")
				{
					prop.PrefabName = "Trap";
					num++;
				}
			}
			string arg = "Didn't need saving";
			if (num > 0)
			{
				arg = (SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(levelData, levelData.Name, saveReadableJson: false, SaveData.Instance.Global.UseCustomLevelDataFolder) ? "Saved successfully" : "Failed to save");
			}
			string text = $"Level {levelData.Name} Processed. It had {num} traps that needed their prefab name replacing, {arg}.";
			Debug.Log(text);
			processString(text, arg1: true);
		};
		UnityAction<string> onCompleteOperation = delegate
		{
		};
		IterateOverAllAvailableLevels(0.5f, perLevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
	}

	public void BulkSaveOutPropApparanceSettings()
	{
		UnityAction<UnityAction<string, bool>, CCustomLevelData> perLevelOperation = delegate(UnityAction<string, bool> processString, CCustomLevelData levelData)
		{
			int num = 0;
			foreach (CObjectProp prop in levelData.ScenarioState.Props)
			{
				CApparanceOverrideDetails apparanceOverrideDetailsForGUID = levelData.GetApparanceOverrideDetailsForGUID(prop.PropGuid);
				if (apparanceOverrideDetailsForGUID != null && apparanceOverrideDetailsForGUID.OverrideSiblingIndex == -1)
				{
					Singleton<ObjectCacheService>.Instance.GetPropObject(prop).GetComponent<ProceduralStyle>().FetchObjectHierarchyValues(out var objectIndex, out var _);
					apparanceOverrideDetailsForGUID.OverrideSiblingIndex = objectIndex;
					num++;
				}
			}
			string arg = "Didn't need saving";
			if (num > 0)
			{
				arg = (SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(levelData, levelData.Name, saveReadableJson: false, SaveData.Instance.Global.UseCustomLevelDataFolder) ? "Saved successfully" : "Failed to save");
			}
			string text = $"Level {levelData.Name} Processed. It had {num} props that needed their Apparance Override Sibling Index fixing, {arg}.";
			Debug.Log(text);
			processString(text, arg1: true);
		};
		UnityAction<string> onCompleteOperation = delegate
		{
		};
		IterateOverAllAvailableLevels(10f, perLevelOperation, onCompleteOperation);
	}

	public void BulkCheckForSpecificScenarioModifiersAndLog()
	{
		UnityAction<UnityAction<string, bool>, CCustomLevelData> perLevelOperation = delegate(UnityAction<string, bool> processString, CCustomLevelData levelData)
		{
			List<EScenarioModifierType> typesToCheckFor = new List<EScenarioModifierType>
			{
				EScenarioModifierType.ApplyConditionToActor,
				EScenarioModifierType.MoveActorsInDirection,
				EScenarioModifierType.ApplyActiveBonusToActor,
				EScenarioModifierType.MovePropsInSequence,
				EScenarioModifierType.MovePropsToNearestPlayer,
				EScenarioModifierType.TriggerAbility
			};
			string text = string.Empty;
			if (levelData.ScenarioState.ScenarioModifiers.Any((CScenarioModifier x) => typesToCheckFor.Contains(x.ScenarioModifierType)))
			{
				text = $"Level {levelData.Name} Processed. Found one of the scenario modifiers we're searching for in it.";
				Debug.Log(text);
			}
			processString(text, arg1: true);
		};
		UnityAction<string> onCompleteOperation = delegate
		{
		};
		IterateOverAllAvailableLevels(0.5f, perLevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
	}

	public void BulkFixMaxHealthLevelData()
	{
		UnityAction<UnityAction<string, bool>, CCustomLevelData> perLevelOperation = delegate(UnityAction<string, bool> processString, CCustomLevelData levelData)
		{
			string text = string.Empty;
			if (!levelData.EnemyMaxHealthBasedOnPartyLevel)
			{
				levelData.EnemyMaxHealthBasedOnPartyLevel = true;
				text = $"Level {levelData.Name} Processed. Updating EnemyMaxHealthBasedOnPartyLevel value to be true.";
				SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(levelData, levelData.Name, saveReadableJson: false, SaveData.Instance.Global.UseCustomLevelDataFolder);
				Debug.Log(text);
			}
			processString(text, arg1: true);
		};
		UnityAction<string> onCompleteOperation = delegate
		{
		};
		IterateOverAllAvailableLevels(0.5f, perLevelOperation, onCompleteOperation, loadIntoLevelEditor: false);
	}
}
