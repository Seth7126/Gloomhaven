using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[Serializable]
public class MaterialLoaderData
{
	public Renderer Renderer;

	public bool ForceLoad;

	public bool IsSaveExistedMaterials;

	public List<AssetReferenceT<Material>> MaterialReferences;

	private AsyncOperationHandle<Material>[] _handles;

	private Material[] _loadedMaterials;

	private bool _released;

	public void LoadMaterials()
	{
		_released = false;
		int count = MaterialReferences.Count;
		int num = count;
		if (IsSaveExistedMaterials)
		{
			num += Renderer.sharedMaterials.Count((Material x) => x != null);
		}
		_loadedMaterials = new Material[num];
		_handles = new AsyncOperationHandle<Material>[count];
		Renderer.enabled = false;
		for (int num2 = 0; num2 < count; num2++)
		{
			int temp = num2;
			AssetReferenceT<Material> assetReferenceT = MaterialReferences[num2];
			_handles[num2] = assetReferenceT.LoadAssetAsync();
			_handles[num2].Completed += OnMaterialLoaded;
			void OnMaterialLoaded(AsyncOperationHandle<Material> handle)
			{
				_handles[temp].Completed -= OnMaterialLoaded;
				if (!_released && _handles[temp].IsDone)
				{
					_loadedMaterials[temp] = handle.Result;
					CheckAllMaterialLoaded();
				}
			}
		}
	}

	private void CheckAllMaterialLoaded()
	{
		if (_loadedMaterials.Any((Material m) => m == null))
		{
			return;
		}
		if (IsSaveExistedMaterials)
		{
			for (int num = 0; num < Renderer.sharedMaterials.Length; num++)
			{
				if (Renderer.sharedMaterials[num] != null)
				{
					_loadedMaterials[num] = Renderer.sharedMaterials[num];
				}
			}
		}
		Renderer.sharedMaterials = _loadedMaterials;
		Renderer.enabled = true;
	}

	public void Release()
	{
		if (_handles != null)
		{
			AsyncOperationHandle<Material>[] handles = _handles;
			for (int i = 0; i < handles.Length; i++)
			{
				Addressables.Release(handles[i]);
			}
			_released = true;
		}
	}
}
