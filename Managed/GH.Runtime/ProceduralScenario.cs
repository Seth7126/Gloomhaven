#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(ProceduralStyle))]
public class ProceduralScenario : MonoBehaviour
{
	[Header("Visual Debugging : Hide Content")]
	[Tooltip("Temporarily disable all the generated content in this scenario")]
	public bool ShowGeneratedContent = true;

	[Tooltip("Temporarily disable all the base tiles in this scenario")]
	public bool ShowTemplateTiles = true;

	[Tooltip("Temporarily disable any custom tile based objects, e.g. doors, chests, etc manually in test scenes")]
	public bool ShowCustomTiles = true;

	[Header("Visual Debugging : Wall Analysis")]
	[Tooltip("Temporarily show wall bounds (original, and adjusted if available)")]
	public bool ShowWallBounds;

	[Tooltip("Temporarily perform scenario-wide wall calculation and adjustment")]
	public bool PerformWallAnalysis;

	[Tooltip("Draw all lines with endpoint jitter, helps see overlapping segments")]
	public bool JitterLines;

	public int onlyDrawThisWall = -1;

	[Header("Visual Debugging : Style")]
	[Tooltip("Temporarily assign random visual style parameters to all the rooms")]
	public bool RandomiseRooms;

	public int RandomRoomSeed;

	private ProceduralWallAnalyser wallAnalyser;

	private bool m_InitialEditorWallSetup;

	private List<GameObject> toActivate = new List<GameObject>();

	private List<GameObject> toDeactivate = new List<GameObject>();

	private List<GameObject> toRefreshTracking = new List<GameObject>();

	private List<ProceduralWallAnalyser.WallInfo> originalWalls = new List<ProceduralWallAnalyser.WallInfo>();

	private List<ProceduralWallAnalyser.WallInfo> currentWalls = new List<ProceduralWallAnalyser.WallInfo>();

	[NonSerialized]
	private GloomUtility.ValueChangeTracker m_ValueTracking;

	private List<ProceduralMapTile> mapTiles;

	private List<float> mapTilesBlend;

	private StaticAmbience prevStaticAmbience;

	public static float dynamicLightingTransitionTime = 0.5f;

	public static float dynamicFogTransitionTime = 0.5f;

	public int ScenarioSeed
	{
		get
		{
			ProceduralStyle component = GetComponent<ProceduralStyle>();
			if (component != null)
			{
				return component.Seed;
			}
			return 0;
		}
		set
		{
			ProceduralStyle component = GetComponent<ProceduralStyle>();
			if (component != null)
			{
				component.Seed = value;
			}
		}
	}

	private bool IsEditor => !Application.isPlaying;

	private GloomUtility.ValueChangeTracker ValueTracker
	{
		get
		{
			if (m_ValueTracking == null)
			{
				m_ValueTracking = new GloomUtility.ValueChangeTracker();
			}
			return m_ValueTracking;
		}
	}

	public StaticAmbience Ambience { get; set; }

	public DynamicAmbience CurrentDynamicAmbience { get; private set; }

	private IEnumerable<GameObject> PrefabWalls
	{
		get
		{
			foreach (GameObject mapTile in MapTiles)
			{
				_ = mapTile;
				GameObject gameObject = null;
				if (!(gameObject != null))
				{
					continue;
				}
				GameObject gameObject2 = gameObject.FindInImmediateChildren("Walls", includeInactive: true);
				if (!(gameObject2 != null))
				{
					continue;
				}
				foreach (GameObject item in ChildrenOf(gameObject2))
				{
					yield return item;
				}
			}
		}
	}

	private IEnumerable<GameObject> Walls
	{
		get
		{
			foreach (GameObject mapTile in MapTiles)
			{
				GameObject gameObject = mapTile.FindInImmediateChildren("Walls", includeInactive: true);
				if (!(gameObject != null))
				{
					continue;
				}
				foreach (GameObject item in ChildrenOf(gameObject))
				{
					yield return item;
				}
			}
		}
	}

	private IEnumerable<GameObject> CustomTiles => from o in ObjectsOfTypeRecursive(base.gameObject, typeof(ProceduralTile), continue_inside_match: false)
		where o.GetComponent<ApparanceEntity>() == null
		select o;

	private IEnumerable<GameObject> GeneratedContent
	{
		get
		{
			foreach (GameObject entity in Entities)
			{
				GameObject gameObject = entity.FindInImmediateChildren("Generated Content", includeInactive: true);
				if (gameObject != null)
				{
					yield return gameObject;
				}
			}
		}
	}

	private IEnumerable<GameObject> Entities => ObjectsOfTypeRecursive(base.gameObject, typeof(ApparanceEntity));

	private IEnumerable<GameObject> Templates
	{
		get
		{
			foreach (GameObject mapTile in MapTiles)
			{
				IEnumerable<GameObject> enumerable = ChildrenOf(mapTile).Where(delegate(GameObject o)
				{
					if (o.name == "Generated Content" && o.name == "Walls")
					{
						return false;
					}
					UnityGameEditorObject component = o.GetComponent<UnityGameEditorObject>();
					return (component != null && (component.m_ObjectType == ScenarioManager.ObjectImportType.Coverage || component.m_ObjectType == ScenarioManager.ObjectImportType.Tile || component.m_ObjectType == ScenarioManager.ObjectImportType.EdgeTile)) ? true : false;
				});
				foreach (GameObject item in enumerable)
				{
					yield return item;
				}
			}
			foreach (GameObject prop in Props)
			{
				IEnumerable<GameObject> enumerable2 = ChildrenOf(prop).Where(delegate(GameObject o)
				{
					UnityGameEditorObject component = o.GetComponent<UnityGameEditorObject>();
					return component != null && component.m_ObjectType == ScenarioManager.ObjectImportType.Coverage;
				});
				foreach (GameObject item2 in enumerable2)
				{
					yield return item2;
				}
			}
		}
	}

	internal IEnumerable<GameObject> MapTiles => MyChildren.Where((GameObject o) => o.GetComponent<ProceduralMapTile>() != null);

	private GameObject PropsContainer
	{
		get
		{
			if (!IsEditor)
			{
				GameObject[] rootGameObjects = base.gameObject.scene.GetRootGameObjects();
				foreach (GameObject gameObject in rootGameObjects)
				{
					if (gameObject.name == "Props")
					{
						return gameObject;
					}
				}
				return null;
			}
			return base.gameObject;
		}
	}

	private IEnumerable<GameObject> Props
	{
		get
		{
			if (!IsEditor)
			{
				foreach (GameObject item in ChildrenOf(PropsContainer))
				{
					if (item.GetComponent<UnityGameEditorObject>() != null)
					{
						yield return item;
					}
				}
				yield break;
			}
			foreach (GameObject item2 in ChildrenOf(PropsContainer))
			{
				if (item2.GetComponent<ProceduralProp>() != null)
				{
					yield return item2;
				}
			}
		}
	}

	internal IEnumerable<GameObject> Doorways
	{
		get
		{
			if (!IsEditor)
			{
				foreach (GameObject mapTile in MapTiles)
				{
					ProceduralDoorway[] componentsInChildren = mapTile.GetComponentsInChildren<ProceduralDoorway>();
					foreach (ProceduralDoorway proceduralDoorway in componentsInChildren)
					{
						yield return proceduralDoorway.gameObject;
					}
				}
				yield break;
			}
			foreach (GameObject mapTile2 in MapTiles)
			{
				ProceduralDoorway[] componentsInChildren = mapTile2.GetComponentsInChildren<ProceduralDoorway>();
				foreach (ProceduralDoorway proceduralDoorway2 in componentsInChildren)
				{
					yield return proceduralDoorway2.gameObject;
				}
			}
		}
	}

	private IEnumerable<GameObject> MyChildren => ChildrenOf(base.gameObject);

	internal void NotifySeedChange()
	{
		DistributeSeeds();
	}

	private void AnalyseWalls()
	{
		DistributeSeeds();
		CacheWallBounds(IsEditor);
		wallAnalyser.SetSeed(ScenarioSeed);
		wallAnalyser.SetWalls(IsEditor ? originalWalls : currentWalls);
		if (wallAnalyser.Run())
		{
			wallAnalyser.ApplyWalls(currentWalls);
		}
	}

	public void SetupWalls()
	{
		wallAnalyser = new ProceduralWallAnalyser();
		AnalyseWalls();
		wallAnalyser = null;
		foreach (ProceduralWallAnalyser.WallInfo currentWall in currentWalls)
		{
			ApparanceEntity component = currentWall.wallObject.GetComponent<ApparanceEntity>();
			if (component != null)
			{
				component.IsPopulated = true;
			}
		}
	}

	private void DistributeSeeds()
	{
		System.Random random = new System.Random(ScenarioSeed);
		foreach (GameObject mapTile in MapTiles)
		{
			int seed = random.Next();
			ProceduralStyle component = mapTile.GetComponent<ProceduralStyle>();
			if (component != null && !component.HasBeenManuallyInitialised)
			{
				component.Seed = seed;
			}
			System.Random random2 = new System.Random(seed);
			foreach (ProceduralWall item in GloomUtility.FindAllDescendentComponents<ProceduralWall>(mapTile))
			{
				int seed2 = random2.Next();
				ProceduralStyle component2 = item.GetComponent<ProceduralStyle>();
				if (component2 != null && !component2.HasBeenManuallyInitialised)
				{
					component2.Seed = seed2;
				}
			}
		}
	}

	private void Start()
	{
		if (IsEditor)
		{
			m_InitialEditorWallSetup = true;
		}
	}

	private void OnValidate()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		bool flag = false;
		if (ValueTracker.CheckValue("ShowGeneratedContent", ShowGeneratedContent) || ValueTracker.CheckValue("ShowTemplateTiles", ShowTemplateTiles) || ValueTracker.CheckValue("ShowCustomTiles", ShowCustomTiles) || ValueTracker.CheckValue("ShowWallBounds", ShowWallBounds))
		{
			flag = true;
		}
		if (ValueTracker.CheckValue("PerformWallAnalysis", PerformWallAnalysis ? 1 : 0))
		{
			if (PerformWallAnalysis)
			{
				if (wallAnalyser == null)
				{
					CacheWallBounds(include_prefabs: true);
					wallAnalyser = new ProceduralWallAnalyser();
					AnalyseWalls();
				}
			}
			else if (wallAnalyser != null)
			{
				wallAnalyser.ResetWalls(currentWalls);
				wallAnalyser = null;
				CacheWallBounds(include_prefabs: true);
			}
			flag = true;
		}
		if (flag)
		{
			ApplyVisualOptions();
		}
		bool flag2 = ValueTracker.CheckValue("RandomiseRooms", RandomiseRooms ? 1 : 0);
		bool flag3 = ValueTracker.CheckValue("RandomRoomSeed", RandomRoomSeed);
		if (!(flag2 || flag3))
		{
			return;
		}
		if (flag2)
		{
			if (RandomiseRooms)
			{
				SetRandomRoomStyles();
			}
			else
			{
				ClearRandomRoomStyles();
			}
		}
		else if (flag3 && RandomiseRooms)
		{
			SetRandomRoomStyles();
		}
	}

	private void ApplyVisualOptions()
	{
		if (IsEditor)
		{
			ApplyGeneratedContent();
			ApplyTileTemplates();
			ApplyCustomTiles();
		}
	}

	private void ApplyGeneratedContent()
	{
		if (!IsEditor)
		{
			return;
		}
		foreach (GameObject item in GeneratedContent)
		{
			DeferredSetActive(item, ShowGeneratedContent);
		}
	}

	private void ApplyTileTemplates()
	{
		if (!IsEditor)
		{
			return;
		}
		foreach (GameObject template in Templates)
		{
			DeferredSetActive(template, ShowTemplateTiles);
		}
	}

	private void ApplyCustomTiles()
	{
		if (!IsEditor)
		{
			return;
		}
		foreach (GameObject customTile in CustomTiles)
		{
			if (customTile.activeSelf != ShowCustomTiles)
			{
				DeferredSetActive(customTile, ShowCustomTiles);
				toRefreshTracking.Add(customTile);
			}
		}
	}

	private void CacheWallBounds(bool include_prefabs)
	{
		originalWalls.Clear();
		currentWalls.Clear();
		List<GameObject> list = null;
		if (include_prefabs)
		{
			list = new List<GameObject>();
			list.AddRange(PrefabWalls);
		}
		List<GameObject> list2 = new List<GameObject>();
		list2.AddRange(Walls);
		int num = list2.Count;
		if (include_prefabs)
		{
			num = Math.Min(list.Count, list2.Count);
		}
		for (int i = 0; i < num; i++)
		{
			Transform transform = list2[i].transform;
			if (include_prefabs)
			{
				BoxCollider component = list[i].GetComponent<BoxCollider>();
				if (component != null)
				{
					Transform transform2 = component.transform;
					Matrix4x4 localToWorld = transform.transform.parent.localToWorldMatrix * transform2.localToWorldMatrix;
					originalWalls.Add(new ProceduralWallAnalyser.WallInfo
					{
						wallObject = list2[i],
						boxBounds = component,
						boxParent = transform,
						localToWorld = localToWorld
					});
				}
			}
			BoxCollider component2 = list2[i].GetComponent<BoxCollider>();
			if (component2 != null)
			{
				Matrix4x4 localToWorldMatrix = component2.transform.localToWorldMatrix;
				currentWalls.Add(new ProceduralWallAnalyser.WallInfo
				{
					wallObject = list2[i],
					boxBounds = component2,
					boxParent = transform,
					localToWorld = localToWorldMatrix
				});
			}
		}
	}

	private void DeferredSetActive(GameObject o, bool active)
	{
		if (active)
		{
			toActivate.Add(o);
		}
		else
		{
			toDeactivate.Add(o);
		}
	}

	private void SetRandomRoomStyles()
	{
		ProceduralStyle component = GetComponent<ProceduralStyle>();
		if (!(component != null))
		{
			return;
		}
		System.Random random = new System.Random(RandomRoomSeed);
		foreach (GameObject mapTile in MapTiles)
		{
			ProceduralStyle component2 = mapTile.GetComponent<ProceduralStyle>();
			if (component2 != null)
			{
				SetRandomEnum<ScenarioPossibleRoom.EBiome>(out component2.Biome, 2, random);
				SetRandomEnum<ScenarioPossibleRoom.ESubBiome>(out component2.SubBiome, 1, random);
				SetRandomEnum<ScenarioPossibleRoom.ETheme>(out component2.Theme, 1, random);
				SetRandomEnum<ScenarioPossibleRoom.ESubTheme>(out component2.SubTheme, 1, random);
				SetRandomEnum<ScenarioPossibleRoom.ETone>(out component2.Tone, 1, random);
				ScenarioSeed = random.Next();
				component.CheckChanges(invalidate_all: true);
			}
		}
	}

	private void ClearRandomRoomStyles()
	{
		ProceduralStyle component = GetComponent<ProceduralStyle>();
		if (!(component != null))
		{
			return;
		}
		new System.Random(RandomRoomSeed);
		foreach (GameObject mapTile in MapTiles)
		{
			ProceduralStyle component2 = mapTile.GetComponent<ProceduralStyle>();
			if (component2 != null)
			{
				component2.Biome = ScenarioPossibleRoom.EBiome.Inherit;
				component2.SubBiome = ScenarioPossibleRoom.ESubBiome.Inherit;
				component2.Theme = ScenarioPossibleRoom.ETheme.Inherit;
				component2.SubTheme = ScenarioPossibleRoom.ESubTheme.Inherit;
				component2.Tone = ScenarioPossibleRoom.ETone.Inherit;
				ScenarioSeed = 0;
				component.CheckChanges(invalidate_all: true);
			}
		}
	}

	private static void SetRandomEnum<ET>(out ET enum_value, int min_index, System.Random rng)
	{
		List<ET> list = typeof(ET).GetEnumValues().Cast<ET>().ToList();
		int num = list.Count - 1;
		int index = min_index + rng.Next(num - min_index);
		ET val = list[index];
		enum_value = val;
	}

	private void Update()
	{
		UpdateAmbience();
	}

	private void UpdateAmbience()
	{
		if (Ambience != null && Ambience != prevStaticAmbience)
		{
			prevStaticAmbience = Ambience;
			Ambience.Apply();
		}
		if (mapTiles == null || mapTiles.Count == 0)
		{
			List<GameObject> list = MapTiles.ToList();
			if (list.Count > 0)
			{
				mapTiles = new List<ProceduralMapTile>();
				mapTilesBlend = new List<float>();
				for (int i = 0; i < list.Count; i++)
				{
					ProceduralMapTile component = list[i].GetComponent<ProceduralMapTile>();
					if (component != null)
					{
						Debug.Log("Maptile " + component.name + " now a part of scenario " + base.name);
						mapTiles.Add(component);
						mapTilesBlend.Add(0f);
					}
				}
			}
		}
		if (mapTiles == null)
		{
			return;
		}
		Vector3 vector = Vector3.zero;
		if (CameraController.s_CameraController != null)
		{
			vector = CameraController.s_CameraController.m_TargetFocalPoint;
		}
		vector.y = 1f;
		ProceduralMapTile proceduralMapTile = null;
		float num = float.MaxValue;
		for (int j = 0; j < mapTiles.Count; j++)
		{
			if (mapTiles[j].visibility == ProceduralMapTile.Visibility.All && !(mapTiles[j] == null))
			{
				float magnitude = (mapTiles[j].BoxCollider.bounds.ClosestPoint(vector) - vector).magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					proceduralMapTile = mapTiles[j];
				}
			}
		}
		bool flag = proceduralMapTile != null;
		bool flag2 = CurrentDynamicAmbience != null;
		bool flag3 = flag != flag2 || (flag && proceduralMapTile.Ambience != CurrentDynamicAmbience);
		bool flag4 = flag && flag2 && proceduralMapTile.Ambience?.name == CurrentDynamicAmbience.name;
		if (proceduralMapTile != null)
		{
			CurrentDynamicAmbience = proceduralMapTile.Ambience;
			flag2 = CurrentDynamicAmbience != null;
		}
		for (int k = 0; k < mapTiles.Count; k++)
		{
			ProceduralMapTile proceduralMapTile2 = mapTiles[k];
			DynamicAmbience ambience = proceduralMapTile2.Ambience;
			if (ambience == null)
			{
				mapTilesBlend[k] = 0f;
				continue;
			}
			bool flag5 = ambience == CurrentDynamicAmbience;
			bool isReady = ambience.IsReady;
			bool flag6 = flag5;
			bool flag7 = !flag5;
			float num2 = Timekeeper.SafeDeltaTime() / dynamicLightingTransitionTime;
			float num3 = (flag6 ? 1f : (-1f));
			float num4 = mapTilesBlend[k];
			float num5 = Mathf.Clamp01(num4 + num2 * num3);
			if (flag3 && flag4)
			{
				num5 = Mathf.Clamp01(num3);
			}
			if (!(num5 != num4 && isReady))
			{
				continue;
			}
			mapTilesBlend[k] = num5;
			if (flag6)
			{
				if (num4 == 0f)
				{
					ambience.BeginBlendIn();
				}
				if (num4 > 0f && num5 < 1f)
				{
					ambience.ApplyBlendIn(num5);
				}
				if (num5 == 1f)
				{
					ambience.ApplyBlendIn(num5);
					proceduralMapTile2.Ambience.EndBlendIn();
				}
			}
			if (flag7)
			{
				if (num4 == 1f)
				{
					ambience.BeginBlendOut();
				}
				if (num4 < 1f && num5 > 0f)
				{
					ambience.ApplyBlendOut(num5);
				}
				if (num5 == 0f)
				{
					ambience.ApplyBlendOut(num5);
					ambience.EndBlendOut();
				}
			}
		}
	}

	private static IEnumerable<GameObject> ChildrenOf(GameObject o)
	{
		if (o != null)
		{
			for (int i = 0; i < o.transform.childCount; i++)
			{
				yield return o.transform.GetChild(i).gameObject;
			}
		}
	}

	private static IEnumerable<GameObject> ObjectsOfTypeRecursive(GameObject o, Type component_type, bool continue_inside_match = true)
	{
		bool recurse = true;
		if (o.GetComponent(component_type) != null)
		{
			yield return o;
			if (!continue_inside_match)
			{
				recurse = false;
			}
		}
		if (!recurse)
		{
			yield break;
		}
		for (int i = 0; i < o.transform.childCount; i++)
		{
			GameObject o2 = o.transform.GetChild(i).gameObject;
			foreach (GameObject item in ObjectsOfTypeRecursive(o2, component_type, continue_inside_match))
			{
				yield return item;
			}
		}
	}
}
