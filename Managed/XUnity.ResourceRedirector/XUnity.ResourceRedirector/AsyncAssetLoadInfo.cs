using UnityEngine;

namespace XUnity.ResourceRedirector;

internal class AsyncAssetLoadInfo
{
	public AssetLoadingParameters Parameters { get; }

	public AssetBundle Bundle { get; }

	public bool SkipAllPostfixes { get; }

	public AsyncAssetLoadingResolve ResolveType { get; }

	public Object[] Assets { get; }

	public AsyncAssetLoadInfo(AssetLoadingParameters parameters, AssetBundle bundle, bool skipAllPostfixes, AsyncAssetLoadingResolve resolveType, Object[] assets)
	{
		Parameters = parameters;
		Bundle = bundle;
		SkipAllPostfixes = skipAllPostfixes;
		ResolveType = resolveType;
		Assets = assets;
	}
}
