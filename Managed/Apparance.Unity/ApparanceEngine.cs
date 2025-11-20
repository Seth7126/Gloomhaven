using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Apparance.Net;
using Apparance.Unity;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(ApparanceResources))]
[RequireComponent(typeof(ApparanceAbout))]
public class ApparanceEngine : MonoBehaviour, ILogging, ILifecycle, ISerializationCallbackReceiver
{
	private struct LoadedResourceList
	{
		public string Path;

		public ApparanceResourceList List;
	}

	[Header("Engine Configuration")]
	[Tooltip("How many background synthesis threads to use in editor (NOTE: Restart of Unity required)")]
	public int EditorSynthesisThreads = 4;

	[Tooltip("Size (MB) of synthesis buffer available to each synthesiser in editor (NOTE: Restart of Unity required)")]
	public int EditorSynthesisBufferSize = 100;

	[Tooltip("Disable to turn off live editing of procedures with Apparance Editor, this allows you to run more than one instance of Unity with an Apparance enabled project (NOTE: Restart of Unity required)")]
	public bool EditorLiveEditing;

	[Tooltip("How many background synthesis threads to use in standalone player")]
	public int PlayerSynthesisThreads = 4;

	[Tooltip("Size (MB) of synthesis buffer available to each synthesiser in standalone player")]
	public int PlayerSynthesisBufferSize = 100;

	[Tooltip("Enable to allow live editing of procedures with Apparance Editor in standalone player")]
	public bool PlayerLiveEditing;

	[HideInInspector]
	public long g_ApplicationStartTime;

	[NonSerialized]
	private static bool g_bViewDirty = false;

	[NonSerialized]
	private static Thread g_MainThread = null;

	[Header("Experimental")]
	[Tooltip("EXPERIMENTAL USE ONLY: Use an specific game object as the centre of detail generation (instead of the main camera)")]
	public bool EnableDetailFocus;

	[Tooltip("EXPERIMENTAL USE ONLY: The game object to focus dynamic detail generation around (only used if Enable Detail Focus is ticked above)")]
	public GameObject DetailFocus;

	[Tooltip("EXPERIMENTAL USE ONLY: Meshes can be rendered using mesh instancing if their materials are set up for it.  This can be faster in many cases.")]
	public bool EnableInstancedRendering;

	[NonSerialized]
	public ApparanceResources Resources;

	[NonSerialized]
	private List<LoadedResourceList> m_IncomingLoadedResourceLists = new List<LoadedResourceList>();

	[Header("Materials")]
	[SerializeField]
	[Tooltip("Default material to use for rendering vertex coloured meshes")]
	public Material DefaultColouredMaterial;

	[NonSerialized]
	[HideInInspector]
	public List<ApparanceEntity> EntityComponents = new List<ApparanceEntity>();

	[HideInInspector]
	[SerializeField]
	private bool m_EntityComponentsClear;

	[SerializeField]
	[Header("Debugging")]
	[Tooltip("Use this material to draw colour tinted version of the meshes in the various diagnostics modes")]
	public Material DebugTintMaterial;

	[SerializeField]
	[Tooltip("Use this prefab as a placeholder for missing or un-assigned prefab resources needed by the procedures")]
	public GameObject DebugMissingObject;

	[SerializeField]
	[Tooltip("Use this material as a placeholder for missing or un-assigned material resources needed by the procedures")]
	public Material DebugMissingMaterial;

	[SerializeField]
	[Tooltip("Enable to see generated content in the scene hierarchy browser")]
	public bool ShowGenerated;

	[SerializeField]
	[Tooltip("Enable to place proper prefab instances (instead of copies)")]
	public bool PrefabInstancing;

	[NonSerialized]
	private static List<ApparanceEntity> m_PendingRegistration = new List<ApparanceEntity>();

	[NonSerialized]
	private bool m_ShowGenerated;

	[NonSerialized]
	private bool m_PrefabInstancing;

	[NonSerialized]
	private Dictionary<string, List<AssetRequest>> m_PendingAssetResolutionMap = new Dictionary<string, List<AssetRequest>>();

	[HideInInspector]
	private static long s_LastRunTime = DateTime.Now.Ticks;

	[HideInInspector]
	[SerializeField]
	private long m_LastRunTime;

	[NonSerialized]
	private bool m_GameJustQuit;

	[NonSerialized]
	private bool m_NextStopIsExit;

	[NonSerialized]
	private static bool s_AlreadyRunning = false;

	[HideInInspector]
	[SerializeField]
	private bool m_AlreadyRunning;

	[HideInInspector]
	[SerializeField]
	private bool m_AlreadyRunningDynamic;

	[NonSerialized]
	private AssetInfo m_MissingObjectInfo;

	[NonSerialized]
	private GameObject m_PrevMissingObject;

	[NonSerialized]
	private AssetInfo m_MissingMaterialInfo;

	[NonSerialized]
	private Material m_PrevMissingMaterial;

	private static ProcedureDefinition testProc;

	private static ProcedureDefinition testStruct;

	public static ApparanceEngine Instance { get; private set; }

	public static string ProceduresDirectory => Path.Combine(Application.streamingAssetsPath, "Procedures");

	private bool IsEngineRunning => Engine.IsRunning;

	private ILifecycle Lifecycle => this;

	void ILifecycle.ApplicationStart()
	{
		g_ApplicationStartTime = DateTime.Now.Ticks;
		StartEngine();
	}

	void ILifecycle.EditorStart()
	{
		RefreshResources();
		HookUpdates();
	}

	void ILifecycle.CodeStart()
	{
		ReconnectEngine();
		HookUpdates();
		RefreshResources();
	}

	void ILifecycle.EditorTick()
	{
		UpdateEngine();
		EntitiesEditorTick();
	}

	void ILifecycle.EditorStop()
	{
		UnhookUpdates();
	}

	void ILifecycle.GameStart()
	{
		HookUpdates();
	}

	void ILifecycle.GameTick()
	{
		UpdateEngine();
		EntitiesGameTick();
	}

	void ILifecycle.GameStop()
	{
		UnhookUpdates();
	}

	void ILifecycle.ApplicationStop()
	{
		StopEngine();
	}

	private void StartEngine()
	{
		MonoBehaviour.print("Starting Apparance Engine");
		int num = 1;
		int num2 = 10;
		bool flag = false;
		if (Application.isEditor)
		{
			num = EditorSynthesisThreads;
			num2 = EditorSynthesisBufferSize;
			flag = EditorLiveEditing;
		}
		else
		{
			num = PlayerSynthesisThreads;
			num2 = PlayerSynthesisBufferSize;
			flag = PlayerLiveEditing;
		}
		Engine.Start(ProceduresDirectory, this, num, num2, flag);
		Instance = this;
		foreach (ApparanceEntity item in m_PendingRegistration)
		{
			Register(item);
		}
		m_PendingRegistration.Clear();
		m_EntityComponentsClear = true;
		RefreshResources();
	}

	public void RefreshResources()
	{
		Resources = GetComponent<ApparanceResources>();
		Engine.AssetCacheClear();
		Resources.RefreshResourceList(clear_unused: true);
		Resources.RefreshCache(0);
	}

	public bool CheckFirstForSession(ref long timestamp)
	{
		bool result = timestamp < g_ApplicationStartTime;
		timestamp = g_ApplicationStartTime;
		return result;
	}

	private void UpdateEngine()
	{
		UnityEngine.Vector3 vector = UnityEngine.Vector3.zero;
		if (EnableDetailFocus && DetailFocus != null)
		{
			vector = DetailFocus.transform.position;
		}
		else if (!Application.isPlaying)
		{
			vector = EditorServices.cameraPosition;
		}
		else if (Camera.main != null && Camera.main.isActiveAndEnabled)
		{
			vector = Camera.main.transform.position;
		}
		else
		{
			Camera[] allCameras = Camera.allCameras;
			foreach (Camera camera in allCameras)
			{
				if (camera.isActiveAndEnabled)
				{
					vector = camera.transform.position;
					break;
				}
			}
		}
		UnityEngine.Vector3 position = base.transform.position;
		Apparance.Net.Vector3 view_position = Conversion.AVfromUV(vector - position);
		UpdateAssets();
		Resources.Update();
		Engine.Update(0.1f, view_position);
		if (g_bViewDirty)
		{
			EditorServices.RepaintAll();
			g_bViewDirty = false;
		}
	}

	private void ReconnectEngine()
	{
		StartEngine();
		g_MainThread = Thread.CurrentThread;
	}

	private void StopEngine()
	{
		MonoBehaviour.print("Stopping Apparance Engine");
		Engine.Stop();
		Instance = null;
	}

	internal void RequestViewRefresh()
	{
		g_bViewDirty = true;
	}

	internal static void Register(ApparanceEntity entity)
	{
		if (Instance != null)
		{
			if (!(entity == null))
			{
				if (Instance.m_EntityComponentsClear)
				{
					Instance.EntityComponents.Clear();
					Instance.m_EntityComponentsClear = false;
				}
				if (!Instance.EntityComponents.Contains(entity))
				{
					Instance.EntityComponents.Add(entity);
				}
			}
		}
		else
		{
			m_PendingRegistration.Add(entity);
		}
	}

	private void EntitiesEditorTick()
	{
		bool flag = false;
		int count = EntityComponents.Count;
		for (int i = 0; i < count; i++)
		{
			ApparanceEntity apparanceEntity = EntityComponents[i];
			if (ApparanceEntity.IsEntityAlive(apparanceEntity))
			{
				if (!apparanceEntity.Frozen)
				{
					apparanceEntity.EditorTick();
				}
			}
			else
			{
				flag = true;
			}
		}
		if (flag)
		{
			PurgeDeadEntities();
		}
		if (ShowGenerated != m_ShowGenerated)
		{
			m_ShowGenerated = ShowGenerated;
			RequestRebuild();
		}
		if (PrefabInstancing != m_PrefabInstancing)
		{
			m_PrefabInstancing = PrefabInstancing;
			RequestRebuild();
		}
	}

	private void EntitiesGameTick()
	{
		bool flag = false;
		int count = EntityComponents.Count;
		for (int i = 0; i < count; i++)
		{
			ApparanceEntity apparanceEntity = EntityComponents[i];
			if (ApparanceEntity.IsEntityAlive(apparanceEntity))
			{
				if (!apparanceEntity.Frozen)
				{
					apparanceEntity.GameTick();
				}
			}
			else
			{
				flag = true;
			}
		}
		if (flag)
		{
			PurgeDeadEntities();
		}
	}

	public void RequestRebuild()
	{
		bool flag = false;
		foreach (ApparanceEntity entityComponent in EntityComponents)
		{
			if (entityComponent == null)
			{
				flag = true;
				continue;
			}
			bool flag2 = false;
			Transform parent = entityComponent.transform.parent;
			while (parent != null)
			{
				if (parent.name == "Generated Content")
				{
					flag2 = true;
					break;
				}
				parent = parent.parent;
			}
			if (!flag2 && entityComponent.IsPopulated)
			{
				entityComponent.RebuildProcedures();
			}
		}
		if (flag)
		{
			PurgeDeadEntities();
		}
	}

	private void PurgeDeadEntities()
	{
		_ = EntityComponents.Count;
		EntityComponents.RemoveAll((ApparanceEntity e) => !ApparanceEntity.IsEntityAlive(e));
		_ = EntityComponents.Count;
	}

	private void Awake()
	{
		Lifecycle.ApplicationStart();
	}

	private void CheckCodeStart()
	{
		if (s_LastRunTime != m_LastRunTime)
		{
			m_LastRunTime = s_LastRunTime;
			Lifecycle.CodeStart();
			m_NextStopIsExit = true;
			s_AlreadyRunning = true;
		}
	}

	private void HookUpdates()
	{
		EditorServices.updateEvent -= Update_Editor_Static;
		EditorServices.updateEvent += Update_Editor_Static;
	}

	private void UnhookUpdates()
	{
		EditorServices.updateEvent -= Update_Editor_Static;
	}

	private void Start()
	{
		CheckCodeStart();
		if (Application.isPlaying)
		{
			Lifecycle.GameStart();
		}
		else
		{
			Lifecycle.EditorStart();
		}
	}

	private void Update()
	{
		CheckCodeStart();
		if (Application.isPlaying)
		{
			Lifecycle.GameTick();
		}
	}

	private static void Update_Editor_Static()
	{
		if (Instance != null)
		{
			Instance.Update_Editor();
		}
	}

	private void Update_Editor()
	{
		if (!Application.isPlaying)
		{
			Lifecycle.EditorTick();
		}
	}

	private void Stop()
	{
		DoStop();
	}

	private void DoStop()
	{
		m_EntityComponentsClear = true;
		if (Application.isPlaying)
		{
			Lifecycle.GameStop();
		}
		else
		{
			Lifecycle.EditorStop();
		}
	}

	private void OnApplicationQuit()
	{
		m_GameJustQuit = true;
	}

	private void OnDestroy()
	{
		if (m_GameJustQuit)
		{
			Lifecycle.GameStop();
		}
		else if (m_NextStopIsExit)
		{
			DoStop();
			Lifecycle.ApplicationStop();
		}
		else
		{
			Lifecycle.EditorStop();
		}
		m_GameJustQuit = false;
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		if (!m_AlreadyRunningDynamic)
		{
			m_AlreadyRunning = s_AlreadyRunning;
			s_AlreadyRunning = false;
			m_AlreadyRunningDynamic = true;
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (m_AlreadyRunningDynamic)
		{
			s_AlreadyRunning = m_AlreadyRunning;
			m_AlreadyRunning = false;
			m_AlreadyRunningDynamic = false;
		}
		m_NextStopIsExit = false;
	}

	public bool PopAssetRequest(ref AssetRequest request)
	{
		int entity_context = 0;
		int asset_id = 0;
		string text = Engine.AssetRequestPoll(out entity_context, out asset_id);
		if (text != null)
		{
			request.Descriptor = text;
			request.EntityContext = entity_context;
			request.ID = asset_id;
			request.PendingAssetPath = null;
			return true;
		}
		return false;
	}

	private void UpdateAssets()
	{
		ProcessWaitingAssetResolutions();
		AssetRequest request = default(AssetRequest);
		while (PopAssetRequest(ref request))
		{
			ProcessAssetRequest(ref request);
		}
	}

	public bool NotifyResourceListDependentRequest(string resource_list_asset_path, ref AssetRequest request)
	{
		bool result = false;
		if (!m_PendingAssetResolutionMap.TryGetValue(resource_list_asset_path, out var value))
		{
			value = new List<AssetRequest>();
			m_PendingAssetResolutionMap[resource_list_asset_path] = value;
			result = true;
		}
		value.Add(request);
		request.StartWaiting(resource_list_asset_path);
		return result;
	}

	public bool CheckAsyncResourceListPending(string resource_list_asset_path)
	{
		return m_PendingAssetResolutionMap.ContainsKey(resource_list_asset_path);
	}

	public void NotifyAsyncResourceListCompleted(string resource_list_asset_path, ApparanceResourceList list)
	{
		Monitor.Enter(m_IncomingLoadedResourceLists);
		m_IncomingLoadedResourceLists.Add(new LoadedResourceList
		{
			Path = resource_list_asset_path,
			List = list
		});
		Monitor.Exit(m_IncomingLoadedResourceLists);
	}

	private void ProcessWaitingAssetResolutions()
	{
		Monitor.Enter(m_IncomingLoadedResourceLists);
		if (m_IncomingLoadedResourceLists.Count > 0)
		{
			foreach (LoadedResourceList incomingLoadedResourceList in m_IncomingLoadedResourceLists)
			{
				bool resource_list_present = incomingLoadedResourceList.List != null;
				try
				{
					if (m_PendingAssetResolutionMap.TryGetValue(incomingLoadedResourceList.Path, out var value))
					{
						for (int i = 0; i < value.Count; i++)
						{
							AssetRequest request = value[i];
							ProcessAssetRequest(ref request, resource_list_present);
						}
						m_PendingAssetResolutionMap.Remove(incomingLoadedResourceList.Path);
					}
				}
				catch (Exception)
				{
				}
			}
			m_IncomingLoadedResourceLists.Clear();
		}
		Monitor.Exit(m_IncomingLoadedResourceLists);
	}

	private void ProcessAssetRequest(ref AssetRequest request, bool resource_list_present = true)
	{
		if (resource_list_present)
		{
			bool flag = false;
			ApparanceResources entityResources = GetEntityResources(request.EntityContext);
			if (entityResources != null)
			{
				AssetInfo assetInfo = entityResources.HandleAssetRequest(ref request);
				if (assetInfo != null)
				{
					assetInfo.Respond(request.EntityContext);
					flag = true;
				}
			}
			if (!flag)
			{
				Resources.HandleAssetRequest(ref request)?.Respond(0);
			}
		}
		else
		{
			AssetInfo assetInfo2 = Resources.GenerateFallbackAssetInfo(ref request);
			Resources.CacheAssetInfo(assetInfo2);
			assetInfo2.Respond(0);
		}
	}

	private ApparanceResources GetEntityResources(int entity_context)
	{
		for (int i = 0; i < EntityComponents.Count; i++)
		{
			if (EntityComponents[i].m_EntityHandle == entity_context)
			{
				return EntityComponents[i].GetResources();
			}
		}
		return null;
	}

	public AssetInfo GetDebugMissingObject()
	{
		if (m_MissingObjectInfo == null || m_PrevMissingObject != DebugMissingObject)
		{
			m_PrevMissingObject = DebugMissingObject;
			m_MissingObjectInfo = new AssetInfo();
			m_MissingObjectInfo.Name = "<Missing Object Placeholder>";
			m_MissingObjectInfo.Object = DebugMissingObject;
			m_MissingObjectInfo.UpdateBounds();
		}
		return m_MissingObjectInfo;
	}

	public AssetInfo GetDebugMissingMaterial()
	{
		if (m_MissingMaterialInfo == null || m_PrevMissingMaterial != DebugMissingMaterial)
		{
			m_PrevMissingMaterial = DebugMissingMaterial;
			m_MissingMaterialInfo = new AssetInfo();
			m_MissingMaterialInfo.Name = "<Missing Material Placeholder>";
			m_MissingMaterialInfo.Object = DebugMissingMaterial;
		}
		return m_MissingMaterialInfo;
	}

	public Material GetMaterialAsset(string name)
	{
		if (Resources != null)
		{
			return Resources.GetMaterialAsset(name);
		}
		return null;
	}

	public GameObject GetPrefabAsset(string name)
	{
		if (Resources != null)
		{
			return Resources.GetPrefabAsset(name);
		}
		return null;
	}

	public void LogMessage(string message)
	{
		Debug.Log(message);
	}

	public void LogWarning(string message)
	{
		Debug.LogWarning(message);
	}

	public void LogError(string message)
	{
		Debug.LogError(message);
	}

	public ProcedureDefinition FindProcedureDefinition(uint proc_id)
	{
		if (proc_id == 42)
		{
			if (testStruct == null)
			{
				testStruct = new ProcedureDefinition();
				testStruct.Name = "A.Test.Procedure";
				testStruct.Description = "Testing the display and editing of procedure input parameters";
				testStruct.Inputs = ParameterCollection.CreateEmpty();
				testStruct.Inputs.BeginWrite();
				testStruct.Inputs.WriteInteger(72, 300, "Font Size (points)");
				testStruct.Inputs.WriteColour(new Colour(0f, 0f, 1f), 301, "Foreground");
				Dictionary<string, object> metadata = new Dictionary<string, object> { ["RadioButtons"] = "Arial=0,Courier=1,Times=2,Comic Sans=99" };
				testStruct.Inputs.WriteInteger(0, 302, "Font", metadata);
				testStruct.Inputs.EndWrite();
			}
			return testStruct;
		}
		if (testProc == null)
		{
			testProc = new ProcedureDefinition();
			testProc.Name = "A.Test.Procedure";
			testProc.Description = "Testing the display and editing of procedure input parameters";
			testProc.Inputs = ParameterCollection.CreateEmpty();
			testProc.Inputs.BeginWrite();
			testProc.Inputs.WriteInteger(123, 100, "Mercury (Nearest Planet)");
			Dictionary<string, object> metadata2 = new Dictionary<string, object>
			{
				["Min"] = 0,
				["Max"] = 100
			};
			testProc.Inputs.WriteInteger(123, 200, "Mercury2", metadata2);
			testProc.Inputs.WriteFloat(3.14159f, 101, "Venus (A hot one)");
			Dictionary<string, object> metadata3 = new Dictionary<string, object>
			{
				["Min"] = 0f,
				["Max"] = 1f
			};
			testProc.Inputs.WriteFloat(3.14159f, 201, "Venus2", metadata3);
			testProc.Inputs.WriteBool(bool_value: true, 102, "Earth (Home Sweet Home)");
			testProc.Inputs.WriteColour(new Colour(1f, 0f, 0f), 103, "Mars (The Red Planet)");
			testProc.Inputs.WriteString("Red Spot", 104, "Jupiter (With Four Moons)");
			testProc.Inputs.WriteVector3(new Apparance.Net.Vector3(2f, 3f, 5f), 105, "Saturn (Prominent Rings)");
			Dictionary<string, object> metadata4 = new Dictionary<string, object> { ["RadioButtons"] = "Units=0,Proportion (0.0 to 1.0)=1,Percentage (0% to 100%)=2" };
			testProc.Inputs.WriteInteger(0, 106, "Neptune (Cold)", metadata4);
			Dictionary<string, object> metadata5 = new Dictionary<string, object> { ["Flags"] = "All=15,Left|L=1,Right|R=2,Top|T=4,Bottom|B=8" };
			testProc.Inputs.WriteInteger(0, 107, "Uranus (Snigger)", metadata5);
			Dictionary<string, object> metadata6 = new Dictionary<string, object> { ["DropList"] = "Black=0,Brown=1,Red=2,Orange=3,Yellow=4,Green=5,Blue=6,Violet=7,Grey=8,White=9" };
			testProc.Inputs.WriteInteger(0, 108, "Pluto (Is it still considered a planet?)", metadata6);
			ParameterCollection parameterCollection = testProc.Inputs.WriteListBegin(109, "Sci-Fi Robots");
			parameterCollection.WriteInteger(99, 200, "Hewey");
			parameterCollection.WriteFloat(99.99f, 201, "Dewey");
			parameterCollection.WriteString("OMG", 202, "Lewey");
			testProc.Inputs.WriteListEnd();
			Dictionary<string, object> metadata7 = new Dictionary<string, object> { ["Structure"] = 42 };
			testProc.Inputs.WriteListBegin(110, "Default Style", metadata7);
			testProc.Inputs.WriteListEnd();
			Dictionary<string, object> metadata8 = new Dictionary<string, object> { ["ElementStruct"] = 42 };
			testProc.Inputs.WriteListBegin(111, "Styles", metadata8);
			testProc.Inputs.WriteListEnd();
			Dictionary<string, object> metadata9 = new Dictionary<string, object> { ["ElementType"] = "" };
			testProc.Inputs.WriteListBegin(112, "Fonts", metadata9);
			testProc.Inputs.WriteListEnd();
			testProc.Inputs.WriteListBegin(113, "Things");
			testProc.Inputs.WriteListEnd();
			testProc.Inputs.EndWrite();
		}
		return testProc;
	}
}
