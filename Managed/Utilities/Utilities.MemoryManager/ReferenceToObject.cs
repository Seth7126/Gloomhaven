#define ENABLE_LOGS
using System;
using System.Threading;
using SM.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Utilities.MemoryManager;

[Serializable]
public class ReferenceToObject<T> where T : UnityEngine.Object
{
	[SerializeField]
	private string _name;

	[SerializeField]
	private AssetReference _reference;

	private T _specialObject;

	private AsyncOperationHandle<T> _handle;

	public T SpecialObject => _specialObject;

	public AssetReference ObjectReference => _reference;

	public string Name => GetName();

	public ReferenceToObject()
	{
	}

	public ReferenceToObject(T obj)
	{
		SetObjectInsteadAddressable(obj);
	}

	public void SetObjectInsteadAddressable(T obj)
	{
		_specialObject = obj;
	}

	public bool IsLoaded()
	{
		if (_specialObject != null || (_handle.IsValid() && _handle.Status == AsyncOperationStatus.Succeeded))
		{
			return true;
		}
		return false;
	}

	public bool IsHaveObject()
	{
		if ((bool)_specialObject || !string.IsNullOrEmpty(_reference.AssetGUID))
		{
			return true;
		}
		return false;
	}

	public T GetObject()
	{
		if ((bool)_specialObject)
		{
			return _specialObject;
		}
		if (_handle.IsValid() && _handle.Result != null)
		{
			return _handle.Result;
		}
		return null;
	}

	public void GetAsyncObject(Action<T, AsyncLoadingState> action, CancellationToken cancellationToken)
	{
		if ((bool)_specialObject)
		{
			action(_specialObject, AsyncLoadingState.Finished);
		}
		else if (_reference == null)
		{
			LogUtils.LogError("Object reference is null");
		}
		else if (string.IsNullOrEmpty(_reference.AssetGUID))
		{
			LogUtils.LogError("GUID is empty!");
			action(null, AsyncLoadingState.Failed);
		}
		else
		{
			_handle = Addressables.LoadAssetAsync<T>(_reference);
			_handle.Completed += Operation;
		}
		void Operation(AsyncOperationHandle<T> x)
		{
			if (!_handle.IsValid() || _handle.Status == AsyncOperationStatus.Failed)
			{
				LogUtils.LogWarning("Sprite: " + GetName() + " | " + _reference?.AssetGUID + " | " + _name + " not loaded.");
			}
			else
			{
				_handle.Completed -= Operation;
				if (cancellationToken.IsCancellationRequested)
				{
					Release();
					action(null, AsyncLoadingState.Canceled);
				}
				else
				{
					action(x.Result, AsyncLoadingState.Finished);
				}
			}
		}
	}

	public T LoadSyncObject()
	{
		if ((bool)_specialObject)
		{
			return _specialObject;
		}
		if (_specialObject == null)
		{
			LogUtils.LogError("Object reference is null");
			return null;
		}
		if (string.IsNullOrEmpty(_reference.AssetGUID))
		{
			LogUtils.LogError("GUID is empty!");
			return null;
		}
		_handle = Addressables.LoadAssetAsync<T>(_reference);
		_handle.Completed += Operation;
		return _handle.WaitForCompletion();
		void Operation(AsyncOperationHandle<T> x)
		{
			_handle.Completed -= Operation;
			if (!_handle.IsValid() || _handle.Status == AsyncOperationStatus.Failed)
			{
				LogUtils.LogError("Sprite: " + GetName() + " | " + _reference?.AssetGUID + " | " + _name + " not loaded.");
			}
		}
	}

	public void GetAsyncObject(Action<T, AsyncLoadingState> action)
	{
		GetAsyncObject(action, CancellationToken.None);
	}

	public void Release()
	{
		if (!_specialObject && _handle.IsValid())
		{
			Addressables.Release(_handle);
			_handle = default(AsyncOperationHandle<T>);
		}
	}

	private string GetName()
	{
		if (_specialObject != null)
		{
			return _specialObject.name;
		}
		return _name;
	}
}
