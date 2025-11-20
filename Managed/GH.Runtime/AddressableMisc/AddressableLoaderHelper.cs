using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressableMisc;

public class AddressableLoaderHelper
{
	private static readonly Dictionary<object, AddressableContext> _addressableContexts;

	static AddressableLoaderHelper()
	{
		_addressableContexts = new Dictionary<object, AddressableContext>();
	}

	public static async Task<TObject> LoadAssetAsync<TObject>(object context, object assetKey)
	{
		AsyncOperationHandle<TObject> asyncOperationHandle = Addressables.LoadAssetAsync<TObject>(assetKey);
		if (_addressableContexts.TryGetValue(context, out var value))
		{
			value.AsyncOperationHandles.Add(asyncOperationHandle);
		}
		else
		{
			AddressableContext addressableContext = new AddressableContext();
			addressableContext.AsyncOperationHandles.Add(asyncOperationHandle);
			_addressableContexts.Add(context, addressableContext);
		}
		return await asyncOperationHandle.Task;
	}

	public static async Task<(TObject, LoadingAddressableState)> LoadAssetAsync<TObject>(object context, object assetKey, CancellationToken ct)
	{
		AsyncOperationHandle<TObject> handler = Addressables.LoadAssetAsync<TObject>(assetKey);
		TObject item = await handler.Task;
		if (ct.IsCancellationRequested)
		{
			if (handler.IsValid())
			{
				Addressables.Release(handler);
			}
			return (default(TObject), LoadingAddressableState.Canceled);
		}
		if (_addressableContexts.TryGetValue(context, out var value))
		{
			value.AsyncOperationHandles.Add(handler);
		}
		else
		{
			AddressableContext addressableContext = new AddressableContext();
			addressableContext.AsyncOperationHandles.Add(handler);
			_addressableContexts.Add(context, addressableContext);
		}
		LoadingAddressableState item2 = LoadingAddressableState.FinishedSuccessfully;
		if (handler.Status == AsyncOperationStatus.Failed)
		{
			item2 = LoadingAddressableState.Failed;
		}
		return (item, item2);
	}

	public static void UnloadAssetsByContext(object context, bool callUnloadUnusedAssets = false)
	{
		if (context != null && _addressableContexts.TryGetValue(context, out var value))
		{
			value.ReleaseAll();
			_addressableContexts.Remove(context);
			if (callUnloadUnusedAssets)
			{
				Resources.UnloadUnusedAssets();
			}
		}
	}
}
