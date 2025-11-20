#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using MapRuleLibrary.State;
using SM.Utils;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetBundleManager : MonoBehaviour
{
	private const int TotalPercent = 100;

	public static AssetBundleManager Instance;

	public BundleLoadSettings BundleLoadConfigs;

	private Dictionary<string, LoadedHandleModel> _loadedHandles;

	private Dictionary<string, AsyncOperationHandle<IList<UnityEngine.Object>>> _alwaysloadedHandles;

	private Dictionary<string, List<AsyncOperationHandle<GameObject>>> _instantiateHandles;

	[HideInInspector]
	public float InitialLoadProgress;

	[HideInInspector]
	public bool InitialLoadComplete;

	private IEnumerator m_InitialLoadRoutine;

	private IEnumerator m_OnDemandLoadRoutine;

	private bool m_OnDemandRoutineLoading;

	private IEnumerator m_ScenarioLoadRoutine;

	[HideInInspector]
	public bool ScenarioBundlesLoading;

	[SerializeField]
	private List<MonoLabelProvider> _monoLabelProviders;

	[UsedImplicitly]
	private void Awake()
	{
		Instance = this;
		_loadedHandles = new Dictionary<string, LoadedHandleModel>();
		_alwaysloadedHandles = new Dictionary<string, AsyncOperationHandle<IList<UnityEngine.Object>>>();
		_instantiateHandles = new Dictionary<string, List<AsyncOperationHandle<GameObject>>>();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Instance = null;
	}

	public void LoadAllInitiallyRequiredBundles(bool dlcOnly)
	{
		InitialLoadComplete = false;
		InitialLoadProgress = 0f;
		if (m_InitialLoadRoutine != null)
		{
			StopCoroutine(m_InitialLoadRoutine);
			m_InitialLoadRoutine = null;
		}
		m_InitialLoadRoutine = LoadAllInitiallyRequiredBundlesSequentially(dlcOnly);
		StartCoroutine(m_InitialLoadRoutine);
	}

	private IEnumerator LoadAllInitiallyRequiredBundlesSequentially(bool dlcOnly)
	{
		List<string> labels = new List<string>();
		if (!dlcOnly)
		{
			labels.Add("always_loaded_base");
			GatherExtraLabels(labels);
		}
		if (PlatformLayer.DLC.UserInstalledDLC(DLCRegistry.EDLCKey.DLC1))
		{
			labels.Add("always_loaded_dlc_1");
		}
		if (PlatformLayer.DLC.UserInstalledDLC(DLCRegistry.EDLCKey.DLC2))
		{
			labels.Add("always_loaded_dlc_2");
		}
		for (int i = 0; i < labels.Count; i++)
		{
			AsyncOperationHandle? asyncOperationHandle = LoadAlwaysLoadedAddressable(labels[i]);
			if (asyncOperationHandle.HasValue)
			{
				AsyncOperationHandle loading = asyncOperationHandle.Value;
				while (loading.Status == AsyncOperationStatus.None)
				{
					InitialLoadProgress = (loading.GetDownloadStatus().Percent + (float)(i * 100)) / (float)labels.Count;
					yield return null;
				}
				if (loading.Status != AsyncOperationStatus.Succeeded)
				{
					Debug.LogErrorFormat("[ASSET BUNDLE MANAGER] - Loading dependencies is finished with status Failed. Exception: {0}", loading.OperationException);
				}
			}
		}
		InitialLoadProgress = 1f;
		InitialLoadComplete = true;
	}

	private void GatherExtraLabels(List<string> labels)
	{
		foreach (MonoLabelProvider monoLabelProvider in _monoLabelProviders)
		{
			labels.AddRange(monoLabelProvider.GetLabels());
		}
	}

	private AsyncOperationHandle? LoadAlwaysLoadedAddressable(string label)
	{
		if (_alwaysloadedHandles.ContainsKey(label))
		{
			return null;
		}
		AsyncOperationHandle<IList<UnityEngine.Object>> asyncOperationHandle = Addressables.LoadAssetsAsync<UnityEngine.Object>(label, null);
		_alwaysloadedHandles.Add(label, asyncOperationHandle);
		LogUtils.Log("LoadAlwaysLoadedAddressable " + label + " " + asyncOperationHandle.DebugName);
		return asyncOperationHandle;
	}

	private AsyncOperationHandle<T> LoadAddressable<T>(string path, bool alwaysLoaded = false) where T : UnityEngine.Object
	{
		if (_loadedHandles.ContainsKey(path))
		{
			return _loadedHandles[path].AsyncOperationHandle.Convert<T>();
		}
		AsyncOperationHandle<T> asyncOperationHandle = Addressables.LoadAssetAsync<T>(path);
		_loadedHandles.Add(path, new LoadedHandleModel(asyncOperationHandle, alwaysLoaded));
		return asyncOperationHandle;
	}

	private AsyncOperationHandle<GameObject> InstantiateAddressable(string path, Transform parent)
	{
		if (!_instantiateHandles.TryGetValue(path, out var value))
		{
			value = new List<AsyncOperationHandle<GameObject>>();
			_instantiateHandles.Add(path, value);
		}
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(path, parent, instantiateInWorldSpace: false, trackHandle: false);
		value.Add(asyncOperationHandle);
		return asyncOperationHandle;
	}

	private void ReleaseInstantiatedAddressable(string path)
	{
		if (!_instantiateHandles.TryGetValue(path, out var value))
		{
			return;
		}
		foreach (AsyncOperationHandle<GameObject> item in value)
		{
			ReleaseHandle(item, releaseInstance: true);
		}
		_instantiateHandles.Remove(path);
	}

	public IEnumerator LoadAllScenarioAssetsBeforeAction(ScenarioState scenarioStateBeingLoaded, CMapState mapStateBeingLoaded = null, UnityAction postLoadAction = null)
	{
		ScenarioBundlesLoading = true;
		List<ECharacter> charactersInScenarioState = new List<ECharacter>();
		List<CClass.ENPCModel> npcModelsInScenarioState = new List<CClass.ENPCModel>();
		yield return null;
		if (scenarioStateBeingLoaded != null)
		{
			yield return ScrapeScenarioStateForRequiredChars(scenarioStateBeingLoaded, scenarioStateBeingLoaded.Players.Count, charactersInScenarioState, npcModelsInScenarioState);
		}
		List<ECharacter> charactersInMapState = new List<ECharacter>();
		List<CClass.ENPCModel> npcModelsInMapState = new List<CClass.ENPCModel>();
		if (mapStateBeingLoaded != null && mapStateBeingLoaded.CurrentMapScenarioState != null && mapStateBeingLoaded.CurrentMapScenarioState.CurrentState != null)
		{
			yield return ScrapeScenarioStateForRequiredChars(mapStateBeingLoaded.CurrentMapScenarioState.CurrentState, mapStateBeingLoaded.MapParty.CheckCharacters.Count, charactersInMapState, npcModelsInMapState);
		}
		List<ECharacter> list = new List<ECharacter>();
		list.AddRange(charactersInScenarioState);
		list.AddRange(charactersInMapState);
		list = list.Distinct().ToList();
		List<CClass.ENPCModel> list2 = new List<CClass.ENPCModel>();
		list2.AddRange(npcModelsInScenarioState);
		list2.AddRange(npcModelsInMapState);
		list2 = list2.Distinct().ToList();
		if (m_ScenarioLoadRoutine != null)
		{
			StopCoroutine(m_ScenarioLoadRoutine);
			m_ScenarioLoadRoutine = null;
		}
		m_ScenarioLoadRoutine = LoadRequiredBundles(BundleLoadConfigs.GetBundlesNeededForRequirements(list, list2), postLoadAction);
		StartCoroutine(m_ScenarioLoadRoutine);
	}

	private IEnumerator LoadRequiredBundles(List<BundleLoadSettings.BundleLoadConfig> bundlesToLoad, UnityAction postLoadAction)
	{
		IEnumerable<string> enumerable = bundlesToLoad.Select((BundleLoadSettings.BundleLoadConfig item) => item.AssetBundleName);
		foreach (string label in enumerable)
		{
			AsyncOperationHandle<UnityEngine.Object> loading = LoadAddressable<UnityEngine.Object>(label);
			while (loading.Status == AsyncOperationStatus.None)
			{
				yield return null;
			}
			if (loading.Status != AsyncOperationStatus.Succeeded)
			{
				Debug.LogError($"Failed to load {label} {loading.OperationException}");
			}
		}
		ScenarioBundlesLoading = false;
		postLoadAction?.Invoke();
	}

	public void UnloadNotRequiredBundles()
	{
		ScenarioBundlesLoading = true;
		foreach (KeyValuePair<string, LoadedHandleModel> loadedHandleKvp in _loadedHandles)
		{
			BundleLoadSettings.BundleLoadConfig bundleLoadConfig = BundleLoadConfigs.BundleConfigs.FirstOrDefault((BundleLoadSettings.BundleLoadConfig b) => loadedHandleKvp.Key.Contains(b.AssetBundleName, StringComparison.Ordinal)) ?? BundleLoadConfigs.DLCBundleConfigs.FirstOrDefault((BundleLoadSettings.BundleLoadConfig b) => loadedHandleKvp.Key.Contains(b.AssetBundleName, StringComparison.Ordinal));
			if ((bundleLoadConfig == null || !bundleLoadConfig.AlwaysLoaded) && !loadedHandleKvp.Value.AlwaysLoaded)
			{
				ReleaseHandle(loadedHandleKvp.Value.AsyncOperationHandle);
			}
		}
		foreach (List<AsyncOperationHandle<GameObject>> value in _instantiateHandles.Values)
		{
			foreach (AsyncOperationHandle<GameObject> item in value)
			{
				ReleaseHandle(item, releaseInstance: true);
			}
		}
		_loadedHandles.Clear();
		_instantiateHandles.Clear();
		ScenarioBundlesLoading = false;
	}

	public static void ReleaseHandle(AsyncOperationHandle asyncOperationHandle, bool releaseInstance = false)
	{
		if (asyncOperationHandle.IsValid())
		{
			if (releaseInstance)
			{
				Addressables.ReleaseInstance(asyncOperationHandle);
			}
			else
			{
				Addressables.Release(asyncOperationHandle);
			}
		}
	}

	private bool TryGetAssetFromAlwaysLoadedHandles<TObject>(string fileName, out TObject obj) where TObject : UnityEngine.Object
	{
		obj = null;
		foreach (KeyValuePair<string, AsyncOperationHandle<IList<UnityEngine.Object>>> alwaysloadedHandle in _alwaysloadedHandles)
		{
			UnityEngine.Object obj2 = alwaysloadedHandle.Value.Result.FirstOrDefault((UnityEngine.Object x) => x.name.Equals(fileName));
			if (obj2 != null)
			{
				obj = obj2 as TObject;
				return true;
			}
		}
		return false;
	}

	public T1 LoadAssetFromBundle<T1>(string assetBundleName, string fileName, string assetDatabaseFolderName, string fileType = "prefab", string explicitPath = null, bool suppressError = false, bool alwaysLoaded = false) where T1 : UnityEngine.Object
	{
		string text = (string.IsNullOrEmpty(explicitPath) ? Path.Combine("Assets", "_AssetBundles", assetDatabaseFolderName, fileName + "." + fileType).Replace("\\", "/") : explicitPath);
		AsyncOperationHandle<T1> asyncOperationHandle = LoadAddressable<T1>(text, alwaysLoaded);
		T1 result = asyncOperationHandle.WaitForCompletion();
		if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded)
		{
			Debug.LogError($"Failed to load {text} {asyncOperationHandle.OperationException}");
		}
		return result;
	}

	public AsyncOperationHandle<GameObject> GetAsyncCharacterPrefabFromBundle(CActor.EType type, string prefabName, string skinString = null, bool lazyLoadIfMissing = true, Transform parent = null)
	{
		string bundleLoadConfig = GetBundleLoadConfig(type, prefabName, skinString);
		return InstantiateAddressable(bundleLoadConfig, parent);
	}

	private string GetBundleLoadConfig(CActor.EType type, string prefabName, string skinString)
	{
		BundleLoadSettings.BundleLoadConfig bundleLoadConfig = BundleConfigForPrefab(type, prefabName);
		if (bundleLoadConfig.BundleDLC == DLCRegistry.EDLCKey.None)
		{
			return Path.Combine("Assets", "_AssetBundles", (type == CActor.EType.Player) ? "heroes" : "npcs", string.Format("{0}{1}{2}.prefab", prefabName, string.IsNullOrEmpty(skinString) ? string.Empty : "_", skinString)).Replace("\\", "/");
		}
		return Path.Combine("Assets", "_AssetBundles", GloomUtility.GetEnumCategory(bundleLoadConfig.BundleDLC), "Content", "CharacterPrefabs", string.Format("{0}{1}{2}.prefab", prefabName, string.IsNullOrEmpty(skinString) ? string.Empty : "_", skinString)).Replace("\\", "/");
	}

	public void UnloadAsyncCharacterPrefabFromBundle(CActor.EType type, string prefabName, string skinString)
	{
		string bundleLoadConfig = GetBundleLoadConfig(type, prefabName, skinString);
		ReleaseInstantiatedAddressable(bundleLoadConfig);
	}

	public GameObject GetCharacterPrefabFromBundle(CActor.EType type, string prefabName, string skinString = null, bool lazyLoadIfMissing = true, bool alwaysLoaded = false)
	{
		string bundleLoadConfig = GetBundleLoadConfig(type, prefabName, skinString);
		return LoadAddressable<GameObject>(bundleLoadConfig, alwaysLoaded).WaitForCompletion();
	}

	public BundleLoadSettings.BundleLoadConfig BundleConfigForPrefab(CActor.EType type, string prefabName)
	{
		string prefabBundleName = string.Empty;
		BundleLoadSettings.BundleLoadConfig.EBundleConfigType bundleTypeToFind = BundleLoadSettings.BundleLoadConfig.EBundleConfigType.None;
		if (type == CActor.EType.Player)
		{
			prefabBundleName = string.Format("{0}_{1}", "hero", prefabName.ToLowerInvariant().RemoveSpaces());
			bundleTypeToFind = BundleLoadSettings.BundleLoadConfig.EBundleConfigType.Hero;
		}
		else
		{
			prefabBundleName = string.Format("{0}_{1}", "npc", prefabName.ToLowerInvariant().RemoveSpaces());
			bundleTypeToFind = BundleLoadSettings.BundleLoadConfig.EBundleConfigType.NPC;
		}
		BundleLoadSettings.BundleLoadConfig bundleLoadConfig = BundleLoadConfigs.BundleConfigs.FirstOrDefault((BundleLoadSettings.BundleLoadConfig b) => b.BundleConfigType == bundleTypeToFind && b.AssetBundleName == prefabBundleName);
		if (bundleLoadConfig == null)
		{
			foreach (BundleLoadSettings.BundleLoadConfig dLCBundleConfig in BundleLoadConfigs.DLCBundleConfigs)
			{
				string[] array = dLCBundleConfig.AssetBundleName.Split('_');
				if (array.Length == 4)
				{
					string text = $"{array[2]}_{array[3]}";
					if (prefabBundleName == text)
					{
						bundleLoadConfig = dLCBundleConfig;
						break;
					}
				}
				else
				{
					Debug.LogWarningFormat("[ASSET BUNDLE MANAGER] - Unsupported DLC bundle config name {0}", dLCBundleConfig.AssetBundleName);
				}
			}
			if (bundleLoadConfig == null)
			{
				Debug.LogErrorFormat("[ASSET BUNDLE MANAGER] - Failed to find bundle config for character prefab: \"{0}\"", prefabName);
				return null;
			}
			if (!PlatformLayer.DLC.UserInstalledDLC(bundleLoadConfig.BundleDLC))
			{
				bundleLoadConfig = null;
				Debug.LogWarningFormat("[ASSET BUNDLE MANAGER] - Attempted access to unowned DLC character prefab \"{0}\" prevented.", prefabName);
				return null;
			}
		}
		return bundleLoadConfig;
	}

	public static IEnumerator ScrapeScenarioStateForRequiredChars(ScenarioState stateToScrape, int partySize, List<ECharacter> charactersNeeded, List<CClass.ENPCModel> modelsNeeded)
	{
		foreach (PlayerState item in stateToScrape.Players.Distinct(new ActorModelEqualityComparer()))
		{
			ECharacter eCharacterFromPlayerState = GetECharacterFromPlayerState(item);
			if (!charactersNeeded.Contains(eCharacterFromPlayerState))
			{
				charactersNeeded.Add(eCharacterFromPlayerState);
			}
		}
		foreach (EnemyState item2 in stateToScrape.AllEnemyStates.Distinct(new ActorModelEqualityComparer()))
		{
			CClass.ENPCModel nPCModelFromEnemyState = GetNPCModelFromEnemyState(item2, partySize);
			if (nPCModelFromEnemyState != CClass.ENPCModel.None && !modelsNeeded.Contains(nPCModelFromEnemyState))
			{
				modelsNeeded.Add(nPCModelFromEnemyState);
			}
			yield return null;
		}
		foreach (CObjectProp prop in stateToScrape.Props)
		{
			if (prop.PropHealthDetails != null && prop.PropHealthDetails.HasHealth && !modelsNeeded.Contains(CClass.ENPCModel.PropDummyObject))
			{
				modelsNeeded.Add(CClass.ENPCModel.PropDummyObject);
				break;
			}
		}
		foreach (CSpawner item3 in stateToScrape.Spawners.Distinct())
		{
			List<CClass.ENPCModel> nPCModelsFromSpawner = GetNPCModelsFromSpawner(item3, partySize);
			if (nPCModelsFromSpawner != null && nPCModelsFromSpawner.Count > 0)
			{
				foreach (CClass.ENPCModel item4 in nPCModelsFromSpawner)
				{
					if (!modelsNeeded.Contains(item4))
					{
						modelsNeeded.Add(item4);
					}
				}
			}
			yield return null;
		}
	}

	private static ECharacter GetECharacterFromPlayerState(PlayerState playerState)
	{
		return CharacterClassManager.Classes.FirstOrDefault((CCharacterClass c) => c.ID == playerState.ClassID)?.CharacterModel ?? ECharacter.None;
	}

	private static CClass.ENPCModel GetNPCModelFromEnemyState(EnemyState enemyState, int partySize)
	{
		CMonsterClass cMonsterClass = MonsterClassManager.Find(enemyState.ClassID);
		if (cMonsterClass != null)
		{
			switch (enemyState.GetConfigForPartySize(partySize))
			{
			case ScenarioManager.EPerPartySizeConfig.Normal:
				return MonstersYML.GetNPCModelEnumFromSpaceName(cMonsterClass.Models[enemyState.ChosenModelIndex]);
			case ScenarioManager.EPerPartySizeConfig.Hidden:
				return CClass.ENPCModel.None;
			case ScenarioManager.EPerPartySizeConfig.ToElite:
			{
				if (cMonsterClass.NonEliteVariant != null)
				{
					return MonstersYML.GetNPCModelEnumFromSpaceName(cMonsterClass.Models[enemyState.ChosenModelIndex]);
				}
				CMonsterClass cMonsterClass2 = MonsterClassManager.FindEliteVariantOfClass(cMonsterClass.ID);
				if (cMonsterClass2 == null)
				{
					Debug.LogWarningFormat("[ASSET BUNDLE MANAGER] - Unable to find Elite version of monster class \"{0}\"", cMonsterClass.ID);
					return MonstersYML.GetNPCModelEnumFromSpaceName(cMonsterClass.Models[enemyState.ChosenModelIndex]);
				}
				return MonstersYML.GetNPCModelEnumFromSpaceName(cMonsterClass2.Models[enemyState.ChosenModelIndex]);
			}
			}
		}
		return CClass.ENPCModel.None;
	}

	private static List<CClass.ENPCModel> GetNPCModelsFromSpawner(CSpawner spawner, int partySize)
	{
		List<CClass.ENPCModel> list = new List<CClass.ENPCModel>();
		foreach (string item in spawner.SpawnerData.SpawnRoundEntryDictionary.Values.SelectMany((List<SpawnRoundEntry> l) => l.SelectMany((SpawnRoundEntry e) => e.SpawnClass)).Distinct().ToList())
		{
			CMonsterClass cMonsterClass = MonsterClassManager.Find(item);
			if (cMonsterClass != null)
			{
				CClass.ENPCModel nPCModelEnumFromSpaceName = MonstersYML.GetNPCModelEnumFromSpaceName(cMonsterClass.DefaultModel);
				if (nPCModelEnumFromSpaceName != CClass.ENPCModel.None)
				{
					list.Add(nPCModelEnumFromSpaceName);
				}
			}
		}
		return list;
	}
}
