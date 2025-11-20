#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using SM.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ApparanceResourceListLoader : MonoBehaviour, IApparanceResourceListLoaderAsync
{
	[Serializable]
	[SerializeField]
	public class NameReference
	{
		public AssetReference Reference;

		public string Name;
	}

	[SerializeField]
	private List<NameReference> _references = new List<NameReference>();

	private Dictionary<string, ApparanceResourceList> _loadedPackets = new Dictionary<string, ApparanceResourceList>();

	private HashSet<AssetReference> _nameReferences = new HashSet<AssetReference>();

	private HashSet<string> _loadingPackets = new HashSet<string>();

	private ApparanceEngine _apparanceEngine;

	[UsedImplicitly]
	private void Awake()
	{
		_apparanceEngine = GetComponent<ApparanceEngine>();
		_apparanceEngine.PrefabInstancing = PlatformLayer.Setting.PrefabInstancing;
	}

	public ApparanceResourceList Load(string asset_path)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(asset_path);
		return GetResourceList(fileNameWithoutExtension);
	}

	public void UnloadAll()
	{
		foreach (AssetReference nameReference in _nameReferences)
		{
			nameReference.ReleaseAsset();
		}
		_loadedPackets.Clear();
		_nameReferences.Clear();
		_apparanceEngine.RefreshResources();
	}

	private ApparanceResourceList GetResourceList(string name)
	{
		if (_loadedPackets.TryGetValue(name, out var value))
		{
			return value;
		}
		AssetReference assetReference = null;
		foreach (NameReference reference in _references)
		{
			if (name == reference.Name)
			{
				assetReference = reference.Reference;
				break;
			}
		}
		if (assetReference == null)
		{
			return null;
		}
		AsyncOperationHandle<ApparanceResourceList> asyncOperationHandle = assetReference.LoadAssetAsync<ApparanceResourceList>();
		_nameReferences.Add(assetReference);
		ApparanceResourceList apparanceResourceList = asyncOperationHandle.WaitForCompletion();
		if (apparanceResourceList != null)
		{
			_loadedPackets.Add(name, apparanceResourceList);
		}
		return apparanceResourceList;
	}

	public ApparanceResourceList LoadCheck(string asset_path)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(asset_path);
		if (_loadedPackets.TryGetValue(fileNameWithoutExtension, out var value))
		{
			return value;
		}
		return null;
	}

	public bool LoadAsync(string asset_path, Action<ApparanceResourceList> onCompleted)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(asset_path);
		if (_loadingPackets.Contains(fileNameWithoutExtension))
		{
			return false;
		}
		GetResourceListAsync(fileNameWithoutExtension, onCompleted);
		return true;
	}

	private async void GetResourceListAsync(string name, Action<ApparanceResourceList> onCompleted)
	{
		AssetReference assetReference = null;
		foreach (NameReference reference in _references)
		{
			if (name == reference.Name)
			{
				assetReference = reference.Reference;
				break;
			}
		}
		if (assetReference == null)
		{
			LogUtils.LogWarning("Reference " + name + " is null! You can add an empty asset to prevent this warning!");
			onCompleted(null);
			return;
		}
		AsyncOperationHandle<ApparanceResourceList> handle = assetReference.LoadAssetAsync<ApparanceResourceList>();
		_nameReferences.Add(assetReference);
		_loadingPackets.Add(name);
		await handle.Task;
		ApparanceResourceList result = handle.Result;
		if (result != null)
		{
			_loadedPackets.Add(name, result);
			Debug.Log("Resource lists: we are using " + name);
		}
		_loadingPackets.Remove(name);
		onCompleted(result);
	}
}
