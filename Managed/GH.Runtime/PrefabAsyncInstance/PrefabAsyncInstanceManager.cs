using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PrefabAsyncInstance;

public class PrefabAsyncInstanceManager : Singleton<PrefabAsyncInstanceManager>
{
	private class RegisteredMainPrefab
	{
		public IMainPrefab MainPrefab;

		public Transform Parent;

		public AssetReference Asset;

		public bool Loading;

		public bool Disacrd;

		public AsyncOperationHandle<GameObject> HandleInstance;

		public GameObject Instance;

		public Type TypeComponent;

		public Component ComponentInstance;

		public Action<GameObject, Component> CallAfterLoad;
	}

	private Dictionary<IMainPrefab, RegisteredMainPrefab> _registeredMainPrefabs;

	private new void Awake()
	{
		base.Awake();
		SetInstance(this);
		_registeredMainPrefabs = new Dictionary<IMainPrefab, RegisteredMainPrefab>();
	}

	private bool CheckCorrectInput(IMainPrefab prefab, Transform transform, AssetReference asset)
	{
		if (prefab == null)
		{
			Debug.LogError("PrefabAsyncInstanceManager: prefab is null");
			return false;
		}
		if (asset == null || !asset.AssetGUID.IsNOTNullOrEmpty())
		{
			Debug.LogError("PrefabAsyncInstanceManager: asset is null");
			return false;
		}
		if (_registeredMainPrefabs.ContainsKey(prefab))
		{
			Debug.LogError("PrefabAsyncInstanceManager: prefab " + prefab?.ToString() + " already registry.");
			return false;
		}
		return true;
	}

	private RegisteredMainPrefab CreateRegisteredData(IMainPrefab prefab, Transform transform, AssetReference asset)
	{
		RegisteredMainPrefab registeredMainPrefab = new RegisteredMainPrefab();
		registeredMainPrefab.MainPrefab = prefab;
		registeredMainPrefab.Parent = transform;
		registeredMainPrefab.Asset = asset;
		registeredMainPrefab.ComponentInstance = null;
		registeredMainPrefab.Instance = null;
		registeredMainPrefab.TypeComponent = null;
		registeredMainPrefab.HandleInstance = default(AsyncOperationHandle<GameObject>);
		registeredMainPrefab.CallAfterLoad = null;
		prefab.OnCreate += CallOnCreateInternal;
		prefab.OnRemove += CallOnRemoveInternal;
		prefab.OnDead += CallOnDeadInternal;
		return registeredMainPrefab;
		void CallOnCreateInternal()
		{
			CallOnCreate(registeredMainPrefab);
		}
		void CallOnDeadInternal()
		{
			prefab.OnCreate -= CallOnCreateInternal;
			prefab.OnRemove -= CallOnRemoveInternal;
			prefab.OnDead -= CallOnDeadInternal;
			CallOnDead(registeredMainPrefab);
		}
		void CallOnRemoveInternal()
		{
			CallOnRemove(registeredMainPrefab);
		}
	}

	public void ConnectMainPrefab(IMainPrefab prefab, Transform transform, AssetReference asset)
	{
		if (CheckCorrectInput(prefab, transform, asset))
		{
			RegisteredMainPrefab value = CreateRegisteredData(prefab, transform, asset);
			_registeredMainPrefabs.Add(prefab, value);
		}
	}

	public void ConnectMainPrefab<T>(IMainPrefab prefab, Transform transform, AssetReference asset) where T : Component
	{
		if (CheckCorrectInput(prefab, transform, asset))
		{
			RegisteredMainPrefab registeredMainPrefab = CreateRegisteredData(prefab, transform, asset);
			registeredMainPrefab.TypeComponent = typeof(T);
			_registeredMainPrefabs.Add(prefab, registeredMainPrefab);
		}
	}

	public void CallAction(IMainPrefab prefab, Action<GameObject> action)
	{
		if (!_registeredMainPrefabs.ContainsKey(prefab))
		{
			return;
		}
		RegisteredMainPrefab registeredMainPrefab = _registeredMainPrefabs[prefab];
		if (registeredMainPrefab.Instance == null)
		{
			registeredMainPrefab.CallAfterLoad = (Action<GameObject, Component>)Delegate.Combine(registeredMainPrefab.CallAfterLoad, (Action<GameObject, Component>)delegate(GameObject x, Component y)
			{
				action(x);
			});
		}
		else
		{
			action(registeredMainPrefab.Instance);
		}
	}

	public void CallAction<T>(IMainPrefab prefab, Action<T> action) where T : Component
	{
		if (!_registeredMainPrefabs.ContainsKey(prefab))
		{
			return;
		}
		RegisteredMainPrefab registeredMainPrefab = _registeredMainPrefabs[prefab];
		if (registeredMainPrefab.Instance == null)
		{
			registeredMainPrefab.CallAfterLoad = (Action<GameObject, Component>)Delegate.Combine(registeredMainPrefab.CallAfterLoad, (Action<GameObject, Component>)delegate(GameObject x, Component y)
			{
				action((T)y);
			});
		}
		else
		{
			action((T)registeredMainPrefab.ComponentInstance);
		}
	}

	public void ClearGarbage()
	{
		bool flag = false;
		foreach (KeyValuePair<IMainPrefab, RegisteredMainPrefab> registeredMainPrefab in _registeredMainPrefabs)
		{
			if (registeredMainPrefab.Key == null)
			{
				flag = true;
			}
			CallOnRemove(registeredMainPrefab.Value);
		}
		if (flag)
		{
			while (_registeredMainPrefabs.Remove(null))
			{
			}
		}
	}

	private void CallOnCreate(RegisteredMainPrefab registered)
	{
		registered.Disacrd = false;
		if (!registered.HandleInstance.IsValid())
		{
			registered.Loading = true;
			registered.HandleInstance = Addressables.InstantiateAsync(registered.Asset.RuntimeKey, registered.Parent, instantiateInWorldSpace: false, trackHandle: false);
			registered.HandleInstance.Completed += LoadedPrefabInternal;
		}
		void LoadedPrefabInternal(AsyncOperationHandle<GameObject> handle)
		{
			registered.HandleInstance.Completed -= LoadedPrefabInternal;
			LoadedPrefab(handle, registered);
		}
	}

	private void CallOnRemove(RegisteredMainPrefab registered)
	{
		registered.CallAfterLoad = null;
		if (registered.Instance != null && !registered.Loading)
		{
			UnloadPrefab(registered);
		}
		else if (registered.Loading)
		{
			registered.Disacrd = true;
		}
	}

	private void CallOnDead(RegisteredMainPrefab registered)
	{
		registered.CallAfterLoad = null;
		if (registered.Instance != null && !registered.Loading)
		{
			UnloadPrefab(registered);
		}
		else if (registered.Loading)
		{
			registered.Disacrd = true;
		}
		_registeredMainPrefabs.Remove(registered.MainPrefab);
	}

	private void LoadedPrefab(AsyncOperationHandle<GameObject> handle, RegisteredMainPrefab registered)
	{
		registered.Loading = false;
		if (handle.Status == AsyncOperationStatus.Succeeded)
		{
			registered.Instance = handle.Result;
			if (registered.Disacrd)
			{
				UnloadPrefab(registered);
				registered.Disacrd = false;
				return;
			}
			if (registered.TypeComponent != null)
			{
				registered.ComponentInstance = registered.Instance.GetComponent(registered.TypeComponent);
			}
			registered.CallAfterLoad?.Invoke(registered.Instance, registered.ComponentInstance);
			registered.CallAfterLoad = null;
		}
		else
		{
			Debug.LogError("PrefabAsyncInstanceManager: spawn prefab failed.");
		}
	}

	private void UnloadPrefab(RegisteredMainPrefab registered)
	{
		AssetBundleManager.ReleaseHandle(registered.HandleInstance, releaseInstance: true);
		registered.HandleInstance = default(AsyncOperationHandle<GameObject>);
		registered.Instance = null;
		registered.ComponentInstance = null;
	}
}
