using System;

namespace XUnity.ResourceRedirector.Hooks;

internal static class ResourceAndAssetHooks
{
	public static readonly Type[] GeneralHooks = new Type[21]
	{
		typeof(AssetBundle_LoadFromFileAsync_Hook),
		typeof(AssetBundle_LoadFromFile_Hook),
		typeof(AssetBundle_LoadFromMemoryAsync_Hook),
		typeof(AssetBundle_LoadFromMemory_Hook),
		typeof(AssetBundle_LoadFromStreamAsync_Hook),
		typeof(AssetBundle_LoadFromStream_Hook),
		typeof(AssetBundle_mainAsset_Hook),
		typeof(AssetBundle_returnMainAsset_Hook),
		typeof(AssetBundle_Load_Hook),
		typeof(AssetBundle_LoadAsync_Hook),
		typeof(AssetBundle_LoadAll_Hook),
		typeof(AssetBundle_LoadAsset_Internal_Hook),
		typeof(AssetBundle_LoadAssetAsync_Internal_Hook),
		typeof(AssetBundle_LoadAssetWithSubAssets_Internal_Hook),
		typeof(AssetBundle_LoadAssetWithSubAssetsAsync_Internal_Hook),
		typeof(AssetBundleRequest_asset_Hook),
		typeof(AssetBundleRequest_allAssets_Hook),
		typeof(Resources_Load_Hook),
		typeof(Resources_LoadAll_Hook),
		typeof(Resources_GetBuiltinResource_Old_Hook),
		typeof(Resources_GetBuiltinResource_New_Hook)
	};

	public static readonly Type[] SyncOverAsyncHooks = new Type[10]
	{
		typeof(AssetBundleCreateRequest_assetBundle_Hook),
		typeof(AssetBundleCreateRequest_DisableCompatibilityChecks_Hook),
		typeof(AssetBundleCreateRequest_SetEnableCompatibilityChecks_Hook),
		typeof(AsyncOperation_isDone_Hook),
		typeof(AsyncOperation_progress_Hook),
		typeof(AsyncOperation_priority_Hook),
		typeof(AsyncOperation_set_priority_Hook),
		typeof(AsyncOperation_allowSceneActivation_Hook),
		typeof(AsyncOperation_set_allowSceneActivation_Hook),
		typeof(AsyncOperation_Finalize_Hook)
	};
}
