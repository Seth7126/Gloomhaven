using System;
using UnityEngine;
using XUnity.Common.Utilities;

namespace XUnity.ResourceRedirector;

public class AsyncAssetLoadingContext : IAssetLoadingContext
{
	private AssetBundleExtensionData _ext;

	private bool _lookedForExt;

	private Object[] _assets;

	private AssetBundleRequest _request;

	private BackingFieldOrArray _backingField;

	public AssetLoadingParameters Parameters { get; }

	public AssetBundle Bundle { get; }

	public AssetBundleRequest Request
	{
		get
		{
			return _request;
		}
		set
		{
			_request = value;
			ResolveType = AsyncAssetLoadingResolve.ThroughRequest;
		}
	}

	public Object[] Assets
	{
		get
		{
			return _backingField.Array;
		}
		set
		{
			if (!ResourceRedirection.SyncOverAsyncEnabled)
			{
				throw new InvalidOperationException("Trying to set the Assets/Asset property in async load operation while 'SyncOverAsyncAssetLoads' is disabled is not allowed. Consider settting the Request property instead if possible or enabling 'SyncOverAsyncAssetLoads' through the method 'ResourceRedirection.EnableSyncOverAsyncAssetLoads()'.");
			}
			_backingField.Array = value;
			ResolveType = AsyncAssetLoadingResolve.ThroughAssets;
		}
	}

	public Object Asset
	{
		get
		{
			return _backingField.Field;
		}
		set
		{
			if (!ResourceRedirection.SyncOverAsyncEnabled)
			{
				throw new InvalidOperationException("Trying to set the Assets/Asset property in async load operation while 'SyncOverAsyncAssetLoads' is disabled is not allowed. Consider settting the Request property instead if possible or enabling 'SyncOverAsyncAssetLoads' through the method 'ResourceRedirection.EnableSyncOverAsyncAssetLoads()'.");
			}
			_backingField.Field = value;
			ResolveType = AsyncAssetLoadingResolve.ThroughAssets;
		}
	}

	public AsyncAssetLoadingResolve ResolveType { get; set; }

	internal bool SkipRemainingPrefixes { get; private set; }

	internal bool SkipOriginalCall { get; set; }

	internal bool SkipAllPostfixes { get; private set; }

	internal AsyncAssetLoadingContext(AssetLoadingParameters parameters, AssetBundle bundle)
	{
		Parameters = parameters;
		Bundle = bundle;
	}

	public string GetAssetBundlePath()
	{
		if (!_lookedForExt)
		{
			_lookedForExt = true;
			_ext = Bundle.GetExtensionData<AssetBundleExtensionData>();
		}
		return _ext?.Path;
	}

	public string GetNormalizedAssetBundlePath()
	{
		if (!_lookedForExt)
		{
			_lookedForExt = true;
			_ext = Bundle.GetExtensionData<AssetBundleExtensionData>();
		}
		return _ext?.NormalizedPath;
	}

	public void Complete(bool skipRemainingPrefixes = true, bool? skipOriginalCall = true, bool? skipAllPostfixes = true)
	{
		SkipRemainingPrefixes = skipRemainingPrefixes;
		if (skipOriginalCall.HasValue)
		{
			SkipOriginalCall = skipOriginalCall.Value;
		}
		if (skipAllPostfixes.HasValue)
		{
			SkipAllPostfixes = skipAllPostfixes.Value;
		}
	}

	public void DisableRecursion()
	{
		ResourceRedirection.RecursionEnabled = false;
	}
}
