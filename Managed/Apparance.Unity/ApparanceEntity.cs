using System;
using System.Collections.Generic;
using System.Linq;
using Apparance.Net;
using Apparance.Unity;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider))]
public class ApparanceEntity : MonoBehaviour, IObjectPlacement, IDebugDrawing, IDebugDisplay, IPlacementParameters, IApparancePlacementProvider, ISerializationCallbackReceiver
{
	private class HierarchyContext
	{
		public HierarchyContext Previous;

		public GameObject Container;

		public int LastChild;

		public int GroupIndex;

		public GameObject RootContainer
		{
			get
			{
				if (Previous != null)
				{
					return Previous.RootContainer;
				}
				return Container;
			}
		}
	}

	private class GameObjectDebugDisplay
	{
		public GameObject Object;

		public Material OriginalMaterial;

		public Material TempMaterial;

		public Color Colour;

		public bool Hidden;

		public void SetColour(Colour c)
		{
			Color color = Conversion.UCfromAC(c);
			if (TempMaterial == null)
			{
				Renderer componentInChildren = Object.GetComponentInChildren<Renderer>();
				if (componentInChildren != null)
				{
					OriginalMaterial = componentInChildren.sharedMaterial;
					TempMaterial = new Material(ApparanceEngine.Instance.DebugTintMaterial);
					TempMaterial.color = color;
					componentInChildren.material = TempMaterial;
					Colour = color;
				}
			}
			if (color != Colour)
			{
				TempMaterial.color = color;
				Colour = color;
			}
		}

		public void ClearColour()
		{
			if (OriginalMaterial != null)
			{
				Renderer componentInChildren = Object.GetComponentInChildren<Renderer>();
				if (componentInChildren != null)
				{
					componentInChildren.material = OriginalMaterial;
				}
				OriginalMaterial = null;
				TempMaterial = null;
			}
		}

		public void ForceHidden()
		{
			if (!Hidden)
			{
				Object.SetActive(value: false);
				Hidden = true;
			}
		}

		public void ShowNormal()
		{
			if (Hidden)
			{
				Object.SetActive(value: true);
				Hidden = false;
			}
		}

		public void Disable()
		{
			ClearColour();
			ShowNormal();
		}
	}

	public class EntityGlobalStats
	{
		public Dictionary<string, int> ObjectTypes = new Dictionary<string, int>();

		public Dictionary<string, int> MaterialTypes = new Dictionary<string, int>();

		public Dictionary<string, int> InstanceTypes = new Dictionary<string, int>();

		private static List<string> keyPurge = new List<string>();

		public void Reset()
		{
			ObjectTypes.Clear();
			MaterialTypes.Clear();
			InstanceTypes.Clear();
		}

		public void Add(EntityGlobalStats gs)
		{
			foreach (KeyValuePair<string, int> objectType in gs.ObjectTypes)
			{
				ObjectTypes.Add(objectType.Key, objectType.Value);
			}
			foreach (KeyValuePair<string, int> materialType in gs.MaterialTypes)
			{
				MaterialTypes.Add(materialType.Key, materialType.Value);
			}
			foreach (KeyValuePair<string, int> instanceType in gs.InstanceTypes)
			{
				InstanceTypes.Add(instanceType.Key, instanceType.Value);
			}
		}

		public void CalculateDelta(EntityGlobalStats new_stats)
		{
			CalculateDelta(ref ObjectTypes, new_stats.ObjectTypes);
			CalculateDelta(ref MaterialTypes, new_stats.MaterialTypes);
			CalculateDelta(ref InstanceTypes, new_stats.InstanceTypes);
		}

		private static void CalculateDelta(ref Dictionary<string, int> from, Dictionary<string, int> to)
		{
			keyPurge.Clear();
			foreach (string key in from.Keys)
			{
				if (!to.ContainsKey(key))
				{
					int num = from[key];
					if (num > 0)
					{
						from[key] = -num;
					}
					else
					{
						keyPurge.Add(key);
					}
				}
			}
			for (int i = 0; i < keyPurge.Count; i++)
			{
				from.Remove(keyPurge[i]);
			}
			keyPurge.Clear();
			foreach (string key2 in to.Keys)
			{
				int value = 0;
				int num2 = to[key2];
				if (from.TryGetValue(key2, out value))
				{
					from[key2] = num2 - value;
				}
				else
				{
					from[key2] = num2;
				}
			}
		}
	}

	public class EntityStats
	{
		public int EntityCount;

		public int BusyCount;

		public int SceneObjects;

		public int SceneComponents;

		public int ObjectCount;

		public int MeshCount;

		public int SubMeshCount;

		public int TriangleCount;

		public int VertexCount;

		public int IndexCount;

		public long MeshBytes;

		public int InstanceCount;

		public int InstanceDrawCount;

		public static EntityGlobalStats Global = new EntityGlobalStats();

		public void Reset(ApparanceEntity entity)
		{
			EntityCount = 1;
			BusyCount = 0;
			SceneObjects = 0;
			SceneComponents = 0;
			ObjectCount = 0;
			MeshCount = 0;
			SubMeshCount = 0;
			TriangleCount = 0;
			VertexCount = 0;
			IndexCount = 0;
			MeshBytes = 0L;
			InstanceCount = 0;
			InstanceDrawCount = 0;
			if (entity != null)
			{
				AddSceneObject(entity.gameObject);
				if (entity.m_GenerationRoot != null)
				{
					AddSceneObject(entity.m_GenerationRoot);
				}
			}
		}

		public void Add(EntityStats s)
		{
			EntityCount += s.EntityCount;
			BusyCount += s.BusyCount;
			SceneObjects += s.SceneObjects;
			SceneComponents += s.SceneComponents;
			ObjectCount += s.ObjectCount;
			MeshCount += s.MeshCount;
			SubMeshCount += s.SubMeshCount;
			TriangleCount += s.TriangleCount;
			VertexCount += s.VertexCount;
			IndexCount += s.IndexCount;
			MeshBytes += s.MeshBytes;
			InstanceCount += s.InstanceCount;
			InstanceDrawCount += s.InstanceDrawCount;
		}

		public void CalculateDelta(EntityStats new_stats)
		{
			EntityCount = new_stats.EntityCount - EntityCount;
			BusyCount = new_stats.BusyCount - BusyCount;
			SceneObjects = new_stats.SceneObjects - SceneObjects;
			SceneComponents = new_stats.SceneComponents - SceneComponents;
			ObjectCount = new_stats.ObjectCount - ObjectCount;
			MeshCount = new_stats.MeshCount - MeshCount;
			SubMeshCount = new_stats.SubMeshCount - SubMeshCount;
			TriangleCount = new_stats.TriangleCount - TriangleCount;
			VertexCount = new_stats.VertexCount - VertexCount;
			IndexCount = new_stats.IndexCount - IndexCount;
			MeshBytes = new_stats.MeshBytes - MeshBytes;
			InstanceCount = new_stats.InstanceCount - InstanceCount;
			InstanceDrawCount = new_stats.InstanceDrawCount - InstanceDrawCount;
		}

		public void AddSceneObject(GameObject o)
		{
			SceneObjects++;
			SceneComponents += o.GetComponents<Component>().Length;
		}
	}

	private struct DebugLine
	{
		public Apparance.Net.Vector3 Start;

		public Apparance.Net.Vector3 End;

		public Colour Colour;
	}

	private struct DebugBox
	{
		public Frame Frame;

		public Colour Colour;
	}

	private struct DebugSphere
	{
		public Apparance.Net.Vector3 Centre;

		public float Radius;

		public Colour Colour;
	}

	[Header("Procedure")]
	[Tooltip("The ID of the procedure to use to generate this entities content")]
	public uint ProcedureID;

	public string ProcedureName = "";

	[Tooltip("Should the entity be populated with its procedurally generated content?")]
	public bool IsPopulated = true;

	[Tooltip("Should the bounding collider be updated from any bounds found in applied parameters?")]
	public bool IsBoundsParameter;

	[Tooltip("Provides a default input parameter (seed value) for generation procedure")]
	public uint Seed = 1u;

	[SerializeField]
	private byte[] ParameterData;

	[NonSerialized]
	public ParameterCollection Parameters;

	[Header("Advanced")]
	[Tooltip("Set to stop further procedural updates and response to changed properties. Use on static game assets once generated (optimisation for large quantity of proc objects)")]
	public bool Frozen;

	[Tooltip("EXPERIMENTAL USE ONLY: By default, transforming an Entity causes a re-build if it's procedural content.")]
	public bool MonitorMovement = true;

	[Tooltip("EXPERIMENTAL USE ONLY: Not supported at the moment.")]
	public bool DynamicDetail;

	[Tooltip("DEMO USE ONLY: Increment the seed and rebuild at regular intervals")]
	public bool AutoSeed;

	[Tooltip("DEMO USE ONLY: How often to increment the seed and rebuild")]
	public float AutoSeedPeriod = 0.25f;

	[NonSerialized]
	private GameObject m_GenerationRoot;

	[NonSerialized]
	private List<HierarchyContext> m_GenerationTiers = new List<HierarchyContext>();

	[NonSerialized]
	private int m_ObjectIndex;

	[NonSerialized]
	private int m_GroupIndex;

	[NonSerialized]
	private int m_NewGroupIndex;

	[NonSerialized]
	private BoxCollider m_BoundsComponent;

	[NonSerialized]
	public ApparanceResources m_Resources;

	[NonSerialized]
	public ParameterCollection FullParameterOverride;

	[NonSerialized]
	private ParameterCollection m_PartialParameterOverride;

	[NonSerialized]
	private uint m_ProcedureID;

	[NonSerialized]
	private Frame m_EntityBounds;

	[NonSerialized]
	private bool m_IsPopulated;

	[NonSerialized]
	private bool m_IsBoundsParameter;

	[NonSerialized]
	private uint m_Seed;

	[NonSerialized]
	private bool m_DynamicDetail;

	[NonSerialized]
	private UnityEngine.Vector3 m_BoundsCentre;

	[NonSerialized]
	private UnityEngine.Vector3 m_BoundsSize;

	[NonSerialized]
	private DateTime g_AutoSeedStart;

	[NonSerialized]
	private bool m_RequestClearAndRefresh;

	[NonSerialized]
	private bool m_RequestRefresh;

	[NonSerialized]
	private bool m_RequestNewGenerationRoot;

	[NonSerialized]
	private bool m_RequestRefreshWhilstFrozen;

	[HideInInspector]
	public int m_EntityHandle;

	[NonSerialized]
	private Entity m_Entity;

	[HideInInspector]
	public long m_SessionTimestamp;

	[NonSerialized]
	private Dictionary<int, GameObject> m_Instances = new Dictionary<int, GameObject>();

	[NonSerialized]
	private IApparancePlacementProvider m_PlacementCustomisation;

	[NonSerialized]
	private Dictionary<int, GameObjectDebugDisplay> m_DebugDisplay = new Dictionary<int, GameObjectDebugDisplay>();

	private EntityStats Stats;

	private static bool statsGatheringEnabled;

	private Utility m_Utility = new Utility();

	private static ReusableBuffer<UnityEngine.Vector3> tempPos = new ReusableBuffer<UnityEngine.Vector3>(65536);

	private static ReusableBuffer<UnityEngine.Vector3> tempNorm = new ReusableBuffer<UnityEngine.Vector3>(65536);

	private static ReusableBuffer<Color32> tempCol = new ReusableBuffer<Color32>(65536);

	private static ReusableBuffers<UnityEngine.Vector2> tempUVs = new ReusableBuffers<UnityEngine.Vector2>(65536);

	private static ReusableBuffers<int> tempIndices = new ReusableBuffers<int>(65536);

	private Dictionary<int, GameObject> m_MeshInstancingObjects = new Dictionary<int, GameObject>();

	[NonSerialized]
	private List<DebugLine> m_DebugLines = new List<DebugLine>();

	[NonSerialized]
	private List<DebugBox> m_DebugBoxes = new List<DebugBox>();

	[NonSerialized]
	private List<DebugSphere> m_DebugSpheres = new List<DebugSphere>();

	public ParameterCollection PartialParameterOverride
	{
		get
		{
			return m_PartialParameterOverride;
		}
		set
		{
			m_PartialParameterOverride = value;
			m_RequestRefresh = true;
			if (Frozen)
			{
				m_RequestRefreshWhilstFrozen = true;
				Frozen = false;
			}
		}
	}

	public bool IsBusy
	{
		get
		{
			if (m_Entity == null)
			{
				return false;
			}
			return m_Entity.IsBusy;
		}
	}

	private string Desc
	{
		get
		{
			string text = base.name;
			if (base.transform.parent != null)
			{
				text = base.transform.parent.name + "." + text;
				if (base.transform.parent.parent != null)
				{
					text = base.transform.parent.parent.name + "." + text;
				}
			}
			if (m_Entity != null)
			{
				text = text + " [" + m_Entity.Handle + "]";
			}
			return "{" + text + "}";
		}
	}

	bool IObjectPlacement.IsValid => this != null;

	private static IEnumerable<ApparanceEntity> AllEntities => Resources.FindObjectsOfTypeAll(typeof(ApparanceEntity)).OfType<ApparanceEntity>();

	public void NotifyPropertyChanged()
	{
		OnValidate();
	}

	private void OnValidate()
	{
		if (EditorServices.IsPrefab(base.gameObject))
		{
			return;
		}
		ApparanceEngine.Register(this);
		if (base.enabled)
		{
			CheckStructure();
			SyncProcID();
			if (Seed != m_Seed)
			{
				m_Seed = Seed;
				RequestEntityRefresh();
			}
			if (IsPopulated != m_IsPopulated)
			{
				m_IsPopulated = IsPopulated;
				RequestEntityRefresh();
			}
			if (IsBoundsParameter != m_IsBoundsParameter)
			{
				m_IsBoundsParameter = IsBoundsParameter;
				RequestEntityRefresh();
			}
			if (DynamicDetail != m_DynamicDetail)
			{
				m_DynamicDetail = DynamicDetail;
				RequestEntityRefresh();
			}
		}
	}

	private void SyncProcID()
	{
		if (ProcedureID != m_ProcedureID)
		{
			m_ProcedureID = ProcedureID;
			if (m_Entity != null)
			{
				m_Entity.Procedure = m_ProcedureID;
				RequestEntityRefresh();
			}
		}
	}

	private void RequestEntityRefresh()
	{
		if (m_Entity != null)
		{
			m_Entity.DynamicDetail = m_DynamicDetail;
			m_RequestRefresh = true;
			if (Frozen)
			{
				m_RequestRefreshWhilstFrozen = true;
				Frozen = false;
			}
		}
	}

	private void Awake()
	{
		if (Application.isPlaying && m_EntityHandle != 0)
		{
			m_EntityHandle = 0;
		}
		integrity_adding(this);
	}

	private void Start()
	{
		if (!EditorServices.IsPrefab(base.gameObject))
		{
			ApparanceEngine.Register(this);
			CheckStructure();
			CheckEntity();
		}
		if (statsGatheringEnabled && Stats == null)
		{
			Stats = new EntityStats();
			Stats.Reset(this);
		}
	}

	private void OnDestroy()
	{
		CheckEntity();
	}

	internal void EditorTick()
	{
		CheckEntity();
		UpdateEntity();
	}

	internal void GameTick()
	{
		CheckEntity();
		UpdateEntity();
	}

	private void UpdateEntity()
	{
		if (base.enabled && AutoSeed && (DateTime.Now - g_AutoSeedStart).TotalSeconds > (double)AutoSeedPeriod)
		{
			g_AutoSeedStart = DateTime.Now;
			Seed++;
			OnValidate();
		}
	}

	internal void RebuildProcedures()
	{
		if (m_Entity != null)
		{
			m_Entity.Refresh();
		}
	}

	private void CheckStructure()
	{
		integrity_check(this);
		Transform transform = base.transform.Find("Generated Content");
		GameObject gameObject = ((transform != null) ? transform.gameObject : null);
		if (!(m_GenerationRoot == null) && !(m_GenerationRoot != gameObject))
		{
			return;
		}
		m_GenerationRoot = null;
		m_GenerationTiers = new List<HierarchyContext>();
		if (gameObject != null)
		{
			m_GenerationRoot = gameObject;
			for (int i = 0; i < m_GenerationRoot.transform.childCount; i++)
			{
				HierarchyContext hierarchyContext = new HierarchyContext();
				hierarchyContext.Container = m_GenerationRoot.transform.GetChild(i).gameObject;
				hierarchyContext.Previous = null;
				hierarchyContext.LastChild = int.MaxValue;
				m_GenerationTiers.Add(hierarchyContext);
			}
			m_RequestClearAndRefresh = true;
		}
		else
		{
			m_RequestNewGenerationRoot = true;
		}
		m_BoundsComponent = GetComponent<BoxCollider>();
		m_Resources = GetComponent<ApparanceResources>();
		IApparanceResourceProvider[] components = GetComponents<IApparanceResourceProvider>();
		for (int j = 0; j < components.Length; j++)
		{
			IApparancePlacementProvider apparancePlacementProvider = (IApparancePlacementProvider)components[j];
			if (apparancePlacementProvider != this)
			{
				m_PlacementCustomisation = apparancePlacementProvider;
				break;
			}
		}
	}

	private HierarchyContext GetTier(int tier_index)
	{
		if (DynamicDetail)
		{
			if (tier_index > 100)
			{
				MonoBehaviour.print("Too many tiers!!!");
				return null;
			}
			while (tier_index >= m_GenerationTiers.Count)
			{
				int count = m_GenerationTiers.Count;
				GameObject gameObject = new GameObject("Tier " + count);
				gameObject.transform.parent = m_GenerationRoot.transform;
				HierarchyContext hierarchyContext = new HierarchyContext();
				hierarchyContext.Container = gameObject;
				hierarchyContext.Previous = null;
				hierarchyContext.LastChild = int.MaxValue;
				m_GenerationTiers.Add(hierarchyContext);
				Stats?.AddSceneObject(gameObject);
			}
			return m_GenerationTiers[tier_index];
		}
		if (m_GenerationTiers.Count != 1)
		{
			ClearObject(m_GenerationRoot);
			m_GenerationTiers.Clear();
			HierarchyContext hierarchyContext2 = new HierarchyContext();
			hierarchyContext2.Container = m_GenerationRoot;
			hierarchyContext2.Previous = null;
			hierarchyContext2.LastChild = int.MaxValue;
			m_GenerationTiers.Add(hierarchyContext2);
		}
		return m_GenerationTiers[0];
	}

	private Transform GetParent(int tier_index)
	{
		HierarchyContext hierarchyContext = GetTier(tier_index);
		while (m_ObjectIndex >= hierarchyContext.LastChild)
		{
			hierarchyContext = hierarchyContext.Previous;
			m_GenerationTiers[tier_index] = hierarchyContext;
			m_GroupIndex = hierarchyContext.GroupIndex;
		}
		return hierarchyContext.Container.transform;
	}

	private void PushGroup(int tier_index, GameObject group_object, int child_count)
	{
		HierarchyContext tier = GetTier(tier_index);
		HierarchyContext hierarchyContext = new HierarchyContext();
		m_GenerationTiers[tier_index] = hierarchyContext;
		hierarchyContext.Previous = tier;
		hierarchyContext.Container = group_object;
		m_GroupIndex = (hierarchyContext.GroupIndex = m_NewGroupIndex++);
		int lastChild = m_ObjectIndex + child_count;
		hierarchyContext.LastChild = lastChild;
	}

	public ApparanceResources GetResources()
	{
		return m_Resources;
	}

	private void Clear(bool immediate = true)
	{
		integrity_check(this);
		IContentUpdateMonitor[] components = GetComponents<IContentUpdateMonitor>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].NotifyContentRemovalStarted();
		}
		for (int j = 0; j < m_GenerationTiers.Count; j++)
		{
			if (!ClearObject(m_GenerationTiers[j].RootContainer, immediate))
			{
				MonoBehaviour.print("Generated content containers broken, refreshing...");
				m_GenerationRoot = null;
				m_GenerationTiers.Clear();
				break;
			}
		}
		Stats?.Reset(this);
		components = GetComponents<IContentUpdateMonitor>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].NotifyContentRemovalComplete();
		}
	}

	private static bool ClearObject(GameObject o)
	{
		return ClearObject(o, !Application.isPlaying);
	}

	private static bool ClearObject(GameObject o, bool immediate = true)
	{
		try
		{
			Transform transform = o.transform;
			int num = transform.childCount;
			while (num > 0)
			{
				num--;
				GameObject obj = transform.GetChild(num).gameObject;
				if (immediate)
				{
					UnityEngine.Object.DestroyImmediate(obj);
				}
				else
				{
					UnityEngine.Object.Destroy(obj);
				}
			}
		}
		catch (MissingReferenceException)
		{
			return false;
		}
		return true;
	}

	private void CheckEntity()
	{
		integrity_check(this);
		if (IsPopulated != m_IsPopulated)
		{
			m_IsPopulated = IsPopulated;
			RequestEntityRefresh();
		}
		if (base.gameObject.activeInHierarchy && base.enabled && IsPopulated)
		{
			if (m_Entity == null)
			{
				CreateEntity();
				CheckStructure();
				MonitorBounds(force_apply: true);
			}
			else
			{
				MonitorBounds(force_apply: false);
			}
			if (!m_RequestClearAndRefresh && !m_RequestRefresh)
			{
				return;
			}
			if (m_RequestClearAndRefresh)
			{
				Clear();
			}
			m_RequestClearAndRefresh = false;
			m_RequestRefresh = false;
			if (m_RequestRefreshWhilstFrozen)
			{
				Frozen = true;
				m_RequestRefreshWhilstFrozen = false;
			}
			if (m_Entity == null)
			{
				return;
			}
			if (FullParameterOverride != null)
			{
				m_Entity.SetParameters(FullParameterOverride);
			}
			else
			{
				m_Entity.Parameters.BeginWrite();
				m_Entity.Parameters.WriteFrame(m_EntityBounds, 1);
				m_Entity.Parameters.WriteInteger((int)Seed, 2);
				m_Entity.Parameters.EndWrite();
				if (PartialParameterOverride != null)
				{
					for (int i = 0; i < PartialParameterOverride.Count; i++)
					{
						Parameter parameterAt = PartialParameterOverride.GetParameterAt(i);
						m_Entity.Parameters.BeginSet();
						m_Entity.Parameters.SetParameter(parameterAt.ID, parameterAt.Value);
						m_Entity.Parameters.EndSet(edited: true);
					}
				}
			}
			m_Entity.Refresh();
		}
		else if (m_EntityHandle != 0)
		{
			DestroyEntity();
		}
	}

	internal static bool IsEntityAlive(ApparanceEntity e)
	{
		if (e != null)
		{
			_ = e.gameObject.scene;
			if (e.gameObject.scene.isLoaded)
			{
				return true;
			}
		}
		return false;
	}

	private static void integrity_adding(ApparanceEntity e)
	{
	}

	private static void integrity_removing(ApparanceEntity e)
	{
	}

	private static void integrity_check(ApparanceEntity e)
	{
	}

	private void CreateEntity()
	{
		if (ApparanceEngine.Instance.CheckFirstForSession(ref m_SessionTimestamp))
		{
			m_EntityHandle = 0;
		}
		if (m_EntityHandle == 0)
		{
			m_Entity = Engine.CreateEntity();
		}
		else
		{
			m_Entity = Engine.CreateEntity(m_EntityHandle);
		}
		SyncProcID();
		m_Entity.Procedure = m_ProcedureID;
		m_Entity.DynamicDetail = m_DynamicDetail;
		m_EntityHandle = m_Entity.Handle;
		integrity_adding(this);
		m_Entity.ObjectPlacementHandler = this;
		m_Entity.DebugDrawingHandler = this;
		m_Entity.DebugDisplayHandler = this;
		m_EntityBounds = new Frame
		{
			AxisX = new Apparance.Net.Vector3(1f, 0f, 0f),
			AxisY = new Apparance.Net.Vector3(0f, 1f, 0f),
			AxisZ = new Apparance.Net.Vector3(0f, 0f, 1f),
			Origin = new Apparance.Net.Vector3(-10f, -10f, 0f),
			Size = new Apparance.Net.Vector3(20f, 20f, 3f)
		};
		ApparanceResources component = GetComponent<ApparanceResources>();
		if (component != null)
		{
			component.RefreshCache(m_EntityHandle);
		}
		m_Entity.Refresh();
	}

	internal void DestroyEntity()
	{
		integrity_removing(this);
		if (m_Entity != null)
		{
			m_Entity.Dispose();
			m_Entity = null;
		}
		else
		{
			Entity.Destroy(m_EntityHandle);
		}
		m_EntityHandle = 0;
		Clear(!Application.isPlaying);
	}

	void IObjectPlacement.CreateObject(int handle, int tier, Apparance.Net.Vector3 offset, int object_type, Frame frame, ParameterCollection parameters, string name, int child_count)
	{
		integrity_check(this);
		object obj;
		if (m_PlacementCustomisation == null)
		{
			obj = this;
		}
		else
		{
			obj = m_PlacementCustomisation;
		}
		IApparancePlacementProvider apparancePlacementProvider = (IApparancePlacementProvider)obj;
		if (base.transform == null || base.gameObject == null)
		{
			return;
		}
		GameObject gameObject = GetParent(tier).gameObject;
		UnityEngine.Vector3 min;
		UnityEngine.Vector3 max;
		List<AssetInstancing> instancingInfo;
		string error_message;
		GameObject prefab = ((m_Resources != null) ? m_Resources : ApparanceEngine.Instance.Resources).GetPrefab(object_type, out min, out max, out instancingInfo, out error_message);
		if (instancingInfo != null && ApparanceEngine.Instance.EnableInstancedRendering)
		{
			Matrix4x4 matrix4x = Conversion.MatrixFromFrame(frame);
			UnityEngine.Vector3 vector = max - min;
			Matrix4x4 matrix4x2 = Matrix4x4.Scale(new UnityEngine.Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z));
			Matrix4x4 matrix4x3 = Matrix4x4.Translate(-(max + min) * 0.5f);
			matrix4x = matrix4x * matrix4x2 * matrix4x3;
			for (int i = 0; i < instancingInfo.Count; i++)
			{
				ApparanceMeshInstances apparanceMeshInstances = FindInstancer(instancingInfo[i], gameObject, m_GroupIndex);
				if (!(apparanceMeshInstances != null))
				{
					continue;
				}
				Matrix4x4 matrix4x4 = instancingInfo[i].transform;
				apparanceMeshInstances.AddInstance(matrix4x * matrix4x4);
				if (Stats != null)
				{
					Stats.InstanceCount++;
					Stats_AddInstance(instancingInfo[i].mesh);
					Material[] materials = instancingInfo[i].materials;
					for (int j = 0; j < materials.Length; j++)
					{
						Stats_AddMaterial(materials[j]);
					}
				}
			}
			m_ObjectIndex++;
			ApparanceEngine.Instance.RequestViewRefresh();
		}
		else
		{
			if (!(prefab != null))
			{
				return;
			}
			if (Stats != null)
			{
				Stats_AddObject(prefab);
			}
			apparancePlacementProvider.BoundsOverride(prefab, object_type, ref min, ref max);
			Matrix4x4 identity = Matrix4x4.identity;
			identity.m00 = frame.AxisX.X;
			identity.m10 = frame.AxisX.Z;
			identity.m20 = frame.AxisX.Y;
			identity.m01 = frame.AxisZ.X;
			identity.m11 = frame.AxisZ.Z;
			identity.m21 = frame.AxisZ.Y;
			identity.m02 = frame.AxisY.X;
			identity.m12 = frame.AxisY.Z;
			identity.m22 = frame.AxisY.Y;
			Quaternion rotation = Conversion.QuaternionFromMatrix(identity);
			UnityEngine.Vector3 localScale = prefab.transform.localScale;
			UnityEngine.Vector3 vector2 = Conversion.UVfromAV(frame.Size);
			UnityEngine.Vector3 vector3 = max - min;
			UnityEngine.Vector3 scale = new UnityEngine.Vector3((vector3.x != 0f) ? (localScale.x * vector2.x / vector3.x) : 1f, (vector3.y != 0f) ? (localScale.y * vector2.y / vector3.y) : 1f, (vector3.z != 0f) ? (localScale.z * vector2.z / vector3.z) : 1f);
			UnityEngine.Vector3 vector4 = new UnityEngine.Vector3((0f - min.x) * scale.x / localScale.x, (0f - min.y) * scale.y / localScale.y, (0f - min.z) * scale.z / localScale.z);
			vector4 = identity.MultiplyVector(vector4);
			UnityEngine.Vector3 position = Conversion.UVfromAV(offset) + Conversion.UVfromAV(frame.Origin) + vector4;
			if (m_GenerationRoot == null)
			{
				CheckStructure();
			}
			if (gameObject != null)
			{
				GameObject gameObject2 = apparancePlacementProvider.CreateInstance(prefab, object_type, position, scale, rotation, gameObject.transform, this);
				if (Stats != null)
				{
					Stats.ObjectCount++;
				}
				HideFlags hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
				if (ApparanceEngine.Instance.ShowGenerated)
				{
					hideFlags = HideFlags.DontSave;
				}
				gameObject2.hideFlags = hideFlags;
				m_Instances[handle] = gameObject2;
				m_ObjectIndex++;
				if (parameters != null)
				{
					IPlacementParameters[] components = gameObject2.GetComponents<IPlacementParameters>();
					IPlacementParameters[] array = components;
					foreach (IPlacementParameters placementParameters in array)
					{
						if (components.Length == 1 || !(placementParameters is ApparanceEntity))
						{
							placementParameters.ApplyParameters(parameters);
						}
					}
				}
				if (child_count > 0)
				{
					PushGroup(tier, gameObject2, child_count);
				}
				if (name != null)
				{
					gameObject2.name = name;
				}
				if (error_message != null)
				{
					gameObject2.name = gameObject2.name + " (" + error_message + ")";
					Debug.LogError(error_message, gameObject2);
				}
			}
			ApparanceEngine.Instance.RequestViewRefresh();
		}
	}

	void IObjectPlacement.CreateGroup(int handle, int tier, string name, int child_count)
	{
		if (m_GenerationRoot == null)
		{
			CheckStructure();
		}
		Transform parent = GetParent(tier);
		if (parent != null)
		{
			GameObject gameObject = new GameObject(name);
			gameObject.transform.SetParent(parent, worldPositionStays: false);
			HideFlags hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
			if (ApparanceEngine.Instance.ShowGenerated)
			{
				hideFlags = HideFlags.DontSave;
			}
			gameObject.hideFlags = hideFlags;
			m_Instances[handle] = gameObject;
			m_ObjectIndex++;
			if (Stats != null)
			{
				Stats.AddSceneObject(gameObject);
			}
			PushGroup(tier, gameObject, child_count);
		}
		ApparanceEngine.Instance.RequestViewRefresh();
	}

	private static T[] GetSubArray<T>(T[] a, int start, int count)
	{
		T[] array = new T[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = a[start + i];
		}
		return array;
	}

	void IObjectPlacement.CreateMesh(int handle, int tier, Apparance.Net.Vector3 offset, int num_verts, Apparance.Net.Vector3[] positions, Apparance.Net.Vector3[] normals, IList<uint[]> colours, IList<Apparance.Net.Vector2[]> uvs, MeshPart[] parts)
	{
		integrity_check(this);
		if (base.transform == null || base.gameObject == null)
		{
			return;
		}
		UnityEngine.Vector3 position = Conversion.UVfromAV(offset);
		if (m_GenerationRoot == null)
		{
			CheckStructure();
		}
		ApparanceResources apparanceResources = ((m_Resources != null) ? m_Resources : ApparanceEngine.Instance.Resources);
		GameObject gameObject = GetParent(tier).gameObject;
		if (!(gameObject != null))
		{
			return;
		}
		GameObject gameObject2 = new GameObject();
		gameObject2.transform.parent = gameObject.transform;
		gameObject2.transform.position = position;
		HideFlags hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
		if (ApparanceEngine.Instance.ShowGenerated)
		{
			hideFlags = HideFlags.DontSave;
		}
		gameObject2.hideFlags = hideFlags;
		m_Instances[handle] = gameObject2;
		m_ObjectIndex++;
		MeshRenderer meshRenderer = gameObject2.AddComponent<MeshRenderer>();
		Mesh mesh = (gameObject2.AddComponent<MeshFilter>().mesh = new Mesh());
		mesh.SetVertices(Conversion.UVsfromAVs(positions, tempPos.Buffer(num_verts)), 0, num_verts);
		mesh.SetNormals(Conversion.UVsfromAVs(normals, tempNorm.Buffer(num_verts)), 0, num_verts);
		if (colours.Count > 0)
		{
			mesh.SetColors(Conversion.UC32sfromUInts(colours[0], tempCol.Buffer(num_verts)), 0, num_verts);
		}
		if (uvs.Count >= 1)
		{
			mesh.SetUVs(0, Conversion.UVsfromAVs(uvs[0], tempUVs.Buffer(0, num_verts)), 0, num_verts);
		}
		if (uvs.Count >= 2)
		{
			mesh.SetUVs(1, Conversion.UVsfromAVs(uvs[1], tempUVs.Buffer(1, num_verts)), 0, num_verts);
		}
		if (uvs.Count >= 3)
		{
			mesh.SetUVs(2, Conversion.UVsfromAVs(uvs[2], tempUVs.Buffer(2, num_verts)), 0, num_verts);
		}
		if (uvs.Count >= 4)
		{
			mesh.SetUVs(3, Conversion.UVsfromAVs(uvs[3], tempUVs.Buffer(3, num_verts)), 0, num_verts);
		}
		if (uvs.Count >= 5)
		{
			mesh.SetUVs(4, Conversion.UVsfromAVs(uvs[4], tempUVs.Buffer(4, num_verts)), 0, num_verts);
		}
		if (uvs.Count >= 6)
		{
			mesh.SetUVs(5, Conversion.UVsfromAVs(uvs[5], tempUVs.Buffer(5, num_verts)), 0, num_verts);
		}
		if (uvs.Count >= 7)
		{
			mesh.SetUVs(6, Conversion.UVsfromAVs(uvs[6], tempUVs.Buffer(6, num_verts)), 0, num_verts);
		}
		if (uvs.Count >= 8)
		{
			mesh.SetUVs(7, Conversion.UVsfromAVs(uvs[7], tempUVs.Buffer(7, num_verts)), 0, num_verts);
		}
		int num = (mesh.subMeshCount = parts.Length);
		Material[] array = new Material[num];
		int num3 = 0;
		Bounds bounds = default(Bounds);
		string error_message = null;
		for (int i = 0; i < num; i++)
		{
			MeshPart meshPart = parts[i];
			array[i] = apparanceResources.GetMaterial(meshPart.Material, out error_message);
			int baseVertex = meshPart.BaseVertex;
			int[] triangles = Conversion.ReverseWinding(meshPart.Indices, meshPart.NumIndices);
			mesh.SetTriangles(triangles, 0, meshPart.NumIndices, i, calculateBounds: false, baseVertex);
			num3 += meshPart.NumIndices / 3;
			UnityEngine.Vector3 vector = Conversion.UVfromAV(meshPart.MinBounds);
			UnityEngine.Vector3 vector2 = Conversion.UVfromAV(meshPart.MaxBounds);
			Bounds bounds2 = new Bounds((vector2 + vector) * 0.5f, vector2 - vector);
			if (i == 0)
			{
				bounds = bounds2;
			}
			else
			{
				bounds.Encapsulate(bounds2);
			}
		}
		mesh.bounds = bounds;
		meshRenderer.sharedMaterials = array;
		mesh.UploadMeshData(markNoLongerReadable: true);
		gameObject2.name = $"Generated Mesh ({num3} tris, {num_verts} verts, {num} parts)";
		if (error_message != null)
		{
			gameObject2.name = gameObject2.name + " (" + error_message + ")";
			Debug.LogError(error_message, gameObject2);
		}
		if (Stats != null)
		{
			Stats.AddSceneObject(gameObject2);
			Stats.MeshCount++;
			Stats.VertexCount += num_verts;
			Stats.TriangleCount += num3;
			Stats.IndexCount += num3 * 3;
			Stats.SubMeshCount += num;
			Material[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				Stats_AddMaterial(array2[j]);
			}
			long num4 = 0L;
			num4 += num_verts * 4 * 3;
			num4 += num_verts * 4 * 3;
			if (colours.Count > 0)
			{
				num4 += num_verts * 4;
			}
			for (int k = 0; k < uvs.Count; k++)
			{
				num4 += num_verts * 4 * 2;
			}
			num4 += num3 * 4 * 3;
			Stats.MeshBytes += num4;
		}
		ApparanceEngine.Instance.RequestViewRefresh();
	}

	void IObjectPlacement.DestroyObject(int handle)
	{
		integrity_check(this);
		if (m_Instances.TryGetValue(handle, out var value))
		{
			m_Instances.Remove(handle);
			m_DebugDisplay.Remove(handle);
			UnityEngine.Object.DestroyImmediate(value);
			ApparanceEngine.Instance.RequestViewRefresh();
		}
	}

	void IObjectPlacement.BeginContentUpdate()
	{
		if (!DynamicDetail)
		{
			m_GenerationTiers.Clear();
			m_ObjectIndex = 0;
			m_GroupIndex = 0;
			m_NewGroupIndex = 0;
		}
		if (m_RequestNewGenerationRoot)
		{
			m_RequestNewGenerationRoot = false;
			m_GenerationRoot = new GameObject("Generated Content");
			m_GenerationRoot.transform.parent = base.transform;
			if (Stats != null)
			{
				Stats.AddSceneObject(m_GenerationRoot);
			}
		}
		IContentUpdateMonitor[] components = GetComponents<IContentUpdateMonitor>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].NotifyContentPlacementStarted();
		}
	}

	void IObjectPlacement.EndContentUpdate()
	{
		EndInstancing();
		IContentUpdateMonitor[] components = GetComponents<IContentUpdateMonitor>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].NotifyContentPlacementComplete();
		}
	}

	public GameObject CreateInstance(GameObject template, int object_type_id, UnityEngine.Vector3 position, UnityEngine.Vector3 scale, Quaternion rotation, Transform parent, IApparancePlacementProvider default_provider)
	{
		GameObject gameObject = null;
		if (ApparanceEngine.Instance.PrefabInstancing && !Application.isPlaying)
		{
			gameObject = EditorServices.InstancePrefab(template);
			gameObject.transform.position = position;
			gameObject.transform.rotation = rotation;
			gameObject.transform.parent = parent;
		}
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(template, position, rotation, parent);
			gameObject.name = template.name;
		}
		gameObject.transform.localScale = scale;
		if (Stats != null)
		{
			Stats.AddSceneObject(gameObject);
		}
		return gameObject;
	}

	public void BoundsOverride(GameObject template, int object_type_id, ref UnityEngine.Vector3 min, ref UnityEngine.Vector3 max)
	{
	}

	private ApparanceMeshInstances FindInstancer(AssetInstancing instancing_info, GameObject tier_container, int group_index)
	{
		int num = instancing_info.handle + group_index * 100000;
		GameObject value = null;
		if (m_MeshInstancingObjects.TryGetValue(num, out value) && value != null)
		{
			return value.GetComponent<ApparanceMeshInstances>();
		}
		value = new GameObject($"{instancing_info.mesh.name} #{num}");
		m_MeshInstancingObjects[num] = value;
		if (m_GenerationRoot == null)
		{
			CheckStructure();
		}
		if (tier_container != null)
		{
			value.transform.parent = tier_container.transform;
		}
		else
		{
			value.transform.parent = m_GenerationRoot.transform;
		}
		value.transform.localPosition = UnityEngine.Vector3.zero;
		value.transform.localRotation = Quaternion.identity;
		value.transform.localScale = new UnityEngine.Vector3(1f, 1f, 1f);
		ApparanceMeshInstances apparanceMeshInstances = value.AddComponent<ApparanceMeshInstances>();
		apparanceMeshInstances.Setup(instancing_info.mesh, instancing_info.materials, instancing_info.handle);
		if (Stats != null)
		{
			Stats.AddSceneObject(value);
			Stats.InstanceDrawCount++;
		}
		return apparanceMeshInstances;
	}

	private void EndInstancing()
	{
		List<int> list = null;
		foreach (int key in m_MeshInstancingObjects.Keys)
		{
			GameObject gameObject = m_MeshInstancingObjects[key];
			if (gameObject != null)
			{
				ApparanceMeshInstances component = gameObject.GetComponent<ApparanceMeshInstances>();
				int num = ((component != null) ? component.ApplyInstances() : 0);
				if (num > 0)
				{
					gameObject.name = $"{component.instanceMesh.name} #{component.instanceTypeHandle} ({num} instances)";
					continue;
				}
			}
			if (list == null)
			{
				list = new List<int>();
			}
			list.Add(key);
		}
		if (list == null)
		{
			return;
		}
		foreach (int item in list)
		{
			GameObject gameObject2 = m_MeshInstancingObjects[item];
			m_MeshInstancingObjects.Remove(item);
			if (gameObject2 != null)
			{
				UnityEngine.Object.DestroyImmediate(gameObject2);
			}
		}
	}

	private void dumpobject(GameObject o, string indent)
	{
		MonoBehaviour.print(indent + o.name + " (" + o.transform.GetInstanceID() + ")");
		for (int i = 0; i < o.transform.childCount; i++)
		{
			dumpobject(o.transform.GetChild(i).gameObject, indent + "  ");
		}
	}

	void IDebugDrawing.ResetDrawing()
	{
		m_DebugLines.Clear();
		m_DebugBoxes.Clear();
		m_DebugSpheres.Clear();
		ApparanceEngine.Instance.RequestViewRefresh();
	}

	void IDebugDrawing.DrawLine(Apparance.Net.Vector3 start, Apparance.Net.Vector3 end, Colour colour)
	{
		m_DebugLines.Add(new DebugLine
		{
			Start = start,
			End = end,
			Colour = colour
		});
	}

	void IDebugDrawing.DrawBox(Frame frame, Colour colour)
	{
		m_DebugBoxes.Add(new DebugBox
		{
			Frame = frame,
			Colour = colour
		});
	}

	void IDebugDrawing.DrawSphere(Apparance.Net.Vector3 centre, float radius, Colour colour)
	{
		m_DebugSpheres.Add(new DebugSphere
		{
			Centre = centre,
			Radius = radius,
			Colour = colour
		});
	}

	void IDebugDisplay.DisplayControl(int geometry_id, bool enable)
	{
		GameObjectDebugDisplay value = null;
		if (m_DebugDisplay.TryGetValue(geometry_id, out value))
		{
			if (!enable)
			{
				value.Disable();
				m_DebugDisplay.Remove(geometry_id);
			}
		}
		else if (enable)
		{
			value = new GameObjectDebugDisplay();
			value.Object = null;
			if (m_Instances.TryGetValue(geometry_id, out value.Object))
			{
				value.Hidden = false;
				value.Colour = new Color(0f, 0f, 0f, 0f);
				value.OriginalMaterial = null;
				value.TempMaterial = null;
				m_DebugDisplay[geometry_id] = value;
			}
		}
	}

	void IDebugDisplay.DisplayHide(int geometry_id, bool hide)
	{
		GameObjectDebugDisplay value = null;
		if (m_DebugDisplay.TryGetValue(geometry_id, out value))
		{
			if (hide)
			{
				value.ForceHidden();
			}
			else
			{
				value.ShowNormal();
			}
		}
	}

	void IDebugDisplay.DisplayColour(int geometry_id, bool enable, Colour c)
	{
		GameObjectDebugDisplay value = null;
		if (m_DebugDisplay.TryGetValue(geometry_id, out value))
		{
			if (enable)
			{
				value.SetColour(c);
			}
			else
			{
				value.ClearColour();
			}
		}
	}

	private void DrawDebugLine(Apparance.Net.Vector3 start, Apparance.Net.Vector3 end, Colour colour)
	{
		Gizmos.color = Conversion.UCfromAC(colour);
		Gizmos.DrawLine(Conversion.UVfromAV(start), Conversion.UVfromAV(end));
	}

	private void DrawDebugBox(Frame frame, Colour colour)
	{
		UnityEngine.Vector3 vector = Conversion.UVfromAV(frame.Origin);
		UnityEngine.Vector3 vector2 = Conversion.UVfromAV(frame.Size);
		UnityEngine.Vector3 vector3 = Conversion.UVfromAV(frame.AxisX) * vector2.x;
		UnityEngine.Vector3 vector4 = Conversion.UVfromAV(frame.AxisZ) * vector2.y;
		UnityEngine.Vector3 vector5 = Conversion.UVfromAV(frame.AxisY) * vector2.z;
		Gizmos.color = Conversion.UCfromAC(colour);
		Gizmos.DrawLine(vector, vector + vector3);
		Gizmos.DrawLine(vector, vector + vector4);
		Gizmos.DrawLine(vector + vector3, vector + vector3 + vector4);
		Gizmos.DrawLine(vector + vector4, vector + vector3 + vector4);
		Gizmos.DrawLine(vector, vector + vector5);
		Gizmos.DrawLine(vector + vector3, vector + vector5 + vector3);
		Gizmos.DrawLine(vector + vector4, vector + vector5 + vector4);
		Gizmos.DrawLine(vector + vector3 + vector4, vector + vector5 + vector3 + vector4);
		Gizmos.DrawLine(vector + vector5, vector + vector5 + vector3);
		Gizmos.DrawLine(vector + vector5, vector + vector5 + vector4);
		Gizmos.DrawLine(vector + vector5 + vector3, vector + vector5 + vector3 + vector4);
		Gizmos.DrawLine(vector + vector5 + vector4, vector + vector5 + vector3 + vector4);
	}

	private void DrawDebugSphere(Apparance.Net.Vector3 centre, float radius, Colour colour)
	{
		UnityEngine.Vector3 center = Conversion.UVfromAV(centre);
		Gizmos.color = Conversion.UCfromAC(colour);
		Gizmos.DrawWireSphere(center, radius);
	}

	private void MonitorBounds(bool force_apply)
	{
		if (!IsBoundsParameter && m_BoundsComponent != null && m_Entity != null && (force_apply || m_BoundsComponent.transform.hasChanged || m_BoundsComponent.center != m_BoundsCentre || m_BoundsComponent.size != m_BoundsSize))
		{
			SetProcedureBoundsFromEntityBounds(force_apply, MonitorMovement);
		}
	}

	private void SetProcedureBoundsFromEntityBounds(bool force_apply, bool allow_refresh)
	{
		bool flag = m_BoundsSize.x == 0f && m_BoundsSize.y == 0f && m_BoundsSize.z == 0f;
		m_BoundsComponent.transform.hasChanged = false;
		m_BoundsCentre = m_BoundsComponent.center;
		m_BoundsSize = m_BoundsComponent.size;
		if (!force_apply && flag)
		{
			return;
		}
		UnityEngine.Vector3 size = m_BoundsComponent.size;
		UnityEngine.Vector3 v = m_BoundsComponent.transform.TransformPoint(m_BoundsComponent.center + size * -0.5f);
		UnityEngine.Vector3 v2 = m_BoundsComponent.transform.TransformDirection(new UnityEngine.Vector3(1f, 0f, 0f));
		UnityEngine.Vector3 v3 = m_BoundsComponent.transform.TransformDirection(new UnityEngine.Vector3(0f, 1f, 0f));
		UnityEngine.Vector3 v4 = m_BoundsComponent.transform.TransformDirection(new UnityEngine.Vector3(0f, 0f, 1f));
		m_EntityBounds = new Frame
		{
			Origin = Conversion.AVfromUV(v),
			AxisX = Conversion.AVfromUV(v2),
			AxisY = Conversion.AVfromUV(v4),
			AxisZ = Conversion.AVfromUV(v3),
			Size = Conversion.AVfromUV(size)
		};
		if (allow_refresh)
		{
			m_RequestRefresh = true;
			if (Frozen)
			{
				m_RequestRefreshWhilstFrozen = true;
				Frozen = false;
			}
		}
	}

	private void SetEntityBoundsFromProcedureBounds()
	{
		if (FullParameterOverride != null)
		{
			Parameter parameterAt = FullParameterOverride.GetParameterAt(0);
			if (parameterAt.Value != null && parameterAt.Value is Frame)
			{
				Frame frame = (Frame)parameterAt.Value;
				Matrix4x4 identity = Matrix4x4.identity;
				identity.m00 = frame.AxisX.X;
				identity.m10 = frame.AxisX.Z;
				identity.m20 = frame.AxisX.Y;
				identity.m01 = frame.AxisZ.X;
				identity.m11 = frame.AxisZ.Z;
				identity.m21 = frame.AxisZ.Y;
				identity.m02 = frame.AxisY.X;
				identity.m12 = frame.AxisY.Z;
				identity.m22 = frame.AxisY.Y;
				Quaternion rotation = Conversion.QuaternionFromMatrix(identity);
				UnityEngine.Vector3 vector = Conversion.UVfromAV(frame.Size);
				UnityEngine.Vector3 position = Conversion.UVfromAV(frame.Origin) + vector * 0.5f;
				m_BoundsComponent = GetComponent<BoxCollider>();
				m_BoundsComponent.size = vector;
				m_BoundsComponent.transform.SetPositionAndRotation(position, rotation);
			}
			RequestEntityRefresh();
		}
	}

	void IPlacementParameters.ApplyParameters(ParameterCollection parameters)
	{
		OnParametersAvailable(parameters);
	}

	public virtual void OnParametersAvailable(ParameterCollection parameters)
	{
		FullParameterOverride = parameters;
		m_RequestRefresh = true;
		if (Frozen)
		{
			m_RequestRefreshWhilstFrozen = true;
			Frozen = false;
		}
		if (IsBoundsParameter)
		{
			SetEntityBoundsFromProcedureBounds();
		}
	}

	private void Dump(ParameterCollection parameters, string indent)
	{
		for (int i = 0; i < parameters.Count; i++)
		{
			Parameter parameterAt = parameters.GetParameterAt(i);
			if (parameterAt.Type == '[')
			{
				ParameterCollection parameterCollection = parameterAt.Value as ParameterCollection;
				Debug.Log(indent + "[" + i + "] = LIST OF " + parameterCollection.Count + " ITEMS:");
				Dump(parameterCollection, indent + "  ");
			}
			else
			{
				Debug.Log(indent + "[" + i + "] " + parameterAt.Type.ToString() + " = " + parameterAt.Value.ToString());
			}
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		ParameterData = null;
		if (Parameters != null)
		{
			Parameters.GetData(out ParameterData);
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (Parameters != null)
		{
			if (ParameterData != null)
			{
				Parameters.SetData(ParameterData);
			}
			else
			{
				Parameters = null;
			}
		}
		else if (ParameterData != null)
		{
			Parameters = ParameterCollection.CreateEmpty();
			Parameters.SetData(ParameterData);
		}
	}

	public static void Stats_Start()
	{
		statsGatheringEnabled = true;
		EntityStats.Global.Reset();
		foreach (ApparanceEntity allEntity in AllEntities)
		{
			allEntity.Stats = new EntityStats();
			allEntity.Stats.Reset(allEntity);
			if (allEntity.gameObject.activeInHierarchy)
			{
				allEntity.RebuildProcedures();
			}
		}
	}

	public static void Stats_Stop()
	{
		statsGatheringEnabled = false;
		foreach (ApparanceEntity allEntity in AllEntities)
		{
			allEntity.Stats = null;
		}
	}

	public static EntityStats Stats_Get()
	{
		EntityStats entityStats = new EntityStats();
		foreach (ApparanceEntity allEntity in AllEntities)
		{
			if (allEntity != null && allEntity.Stats != null)
			{
				entityStats.Add(allEntity.Stats);
				if (allEntity.IsBusy)
				{
					entityStats.BusyCount++;
				}
			}
		}
		return entityStats;
	}

	private static void Stats_AddMaterial(Material m)
	{
		string key = m.name;
		int value = 0;
		value = ((!EntityStats.Global.MaterialTypes.TryGetValue(key, out value)) ? 1 : (value + 1));
		EntityStats.Global.MaterialTypes[key] = value;
	}

	private static void Stats_AddObject(GameObject prefab)
	{
		string key = prefab.name;
		int value = 0;
		value = ((!EntityStats.Global.ObjectTypes.TryGetValue(key, out value)) ? 1 : (value + 1));
		EntityStats.Global.ObjectTypes[key] = value;
	}

	private static void Stats_AddInstance(Mesh mesh)
	{
		string key = mesh.name;
		int value = 0;
		value = ((!EntityStats.Global.InstanceTypes.TryGetValue(key, out value)) ? 1 : (value + 1));
		EntityStats.Global.InstanceTypes[key] = value;
	}
}
