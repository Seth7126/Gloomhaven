using System;
using UnityEngine;
using XUnity.Common.Extensions;
using XUnity.ResourceRedirector.Constants;

namespace XUnity.ResourceRedirector;

public class AsyncAssetBundleLoadingContext : IAssetBundleLoadingContext
{
	private string _normalizedPath;

	private AssetBundle _bundle;

	private AssetBundleCreateRequest _request;

	public AssetBundleLoadingParameters Parameters { get; }

	public AssetBundleCreateRequest Request
	{
		get
		{
			return _request;
		}
		set
		{
			_request = value;
			ResolveType = AsyncAssetBundleLoadingResolve.ThroughRequest;
		}
	}

	public AssetBundle Bundle
	{
		get
		{
			return _bundle;
		}
		set
		{
			if (!ResourceRedirection.SyncOverAsyncEnabled)
			{
				throw new InvalidOperationException("Trying to set the Bundle property in async load operation while 'SyncOverAsyncAssetLoads' is disabled is not allowed. Consider settting the Request property instead if possible or enabling 'SyncOverAsyncAssetLoads' through the method 'ResourceRedirection.EnableSyncOverAsyncAssetLoads()'.");
			}
			_bundle = value;
			ResolveType = AsyncAssetBundleLoadingResolve.ThroughBundle;
		}
	}

	public AsyncAssetBundleLoadingResolve ResolveType { get; set; }

	internal bool SkipRemainingPrefixes { get; private set; }

	internal bool SkipOriginalCall { get; set; }

	internal bool SkipAllPostfixes { get; private set; }

	internal AsyncAssetBundleLoadingContext(AssetBundleLoadingParameters parameters)
	{
		Parameters = parameters;
	}

	public string GetNormalizedPath()
	{
		if (_normalizedPath == null && Parameters.Path != null)
		{
			_normalizedPath = Parameters.Path.ToLowerInvariant().UseCorrectDirectorySeparators().MakeRelativePath(EnvironmentEx.LoweredCurrentDirectory);
		}
		return _normalizedPath;
	}

	public void Complete()
	{
		Complete(skipRemainingPrefixes: true, true, true);
	}

	public void Complete(bool skipRemainingPrefixes = true, bool? skipOriginalCall = true)
	{
		Complete(skipRemainingPrefixes, skipOriginalCall, true);
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
