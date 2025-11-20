using System.IO;
using UnityEngine;

namespace XUnity.ResourceRedirector.Examples;

internal class AssetBundleRedirectorPlugin
{
	private class AssetBundleRedirectorSyncOverAsyncPlugin
	{
		private void Awake()
		{
			ResourceRedirection.EnableSyncOverAsyncAssetLoads();
			ResourceRedirection.RegisterAsyncAndSyncAssetBundleLoadingHook(1000, AssetBundleLoading);
		}

		public void AssetBundleLoading(IAssetBundleLoadingContext context)
		{
			if (!File.Exists(context.Parameters.Path))
			{
				string normalizedPath = context.GetNormalizedPath();
				string text = Path.Combine("mods", normalizedPath);
				if (File.Exists(text))
				{
					AssetBundle bundle = AssetBundle.LoadFromFile(text);
					context.Bundle = bundle;
					context.Complete(skipRemainingPrefixes: true, true);
				}
			}
		}
	}

	private class SmartAssetBundleRedirectorSyncOverAsyncPlugin
	{
		private void Awake()
		{
			ResourceRedirection.EnableSyncOverAsyncAssetLoads();
			ResourceRedirection.RegisterAsyncAndSyncAssetBundleLoadingHook(1000, AssetBundleLoading);
		}

		public void AssetBundleLoading(IAssetBundleLoadingContext context)
		{
			if (File.Exists(context.Parameters.Path))
			{
				return;
			}
			string normalizedPath = context.GetNormalizedPath();
			string text = Path.Combine("mods", normalizedPath);
			if (File.Exists(text))
			{
				if (context is AsyncAssetBundleLoadingContext asyncAssetBundleLoadingContext)
				{
					AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(text);
					asyncAssetBundleLoadingContext.Request = request;
				}
				else
				{
					AssetBundle bundle = AssetBundle.LoadFromFile(text);
					context.Bundle = bundle;
				}
				context.Complete(skipRemainingPrefixes: true, true);
			}
		}
	}

	private void Awake()
	{
		ResourceRedirection.RegisterAssetBundleLoadingHook(1000, AssetBundleLoading);
		ResourceRedirection.RegisterAsyncAssetBundleLoadingHook(1000, AsyncAssetBundleLoading);
	}

	public void AssetBundleLoading(AssetBundleLoadingContext context)
	{
		if (!File.Exists(context.Parameters.Path))
		{
			string normalizedPath = context.GetNormalizedPath();
			string text = Path.Combine("mods", normalizedPath);
			if (File.Exists(text))
			{
				AssetBundle bundle = AssetBundle.LoadFromFile(text);
				context.Bundle = bundle;
				context.Complete(skipRemainingPrefixes: true, true);
			}
		}
	}

	public void AsyncAssetBundleLoading(AsyncAssetBundleLoadingContext context)
	{
		if (!File.Exists(context.Parameters.Path))
		{
			string normalizedPath = context.GetNormalizedPath();
			string text = Path.Combine("mods", normalizedPath);
			if (File.Exists(text))
			{
				AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(text);
				context.Request = request;
				context.Complete(skipRemainingPrefixes: true, true);
			}
		}
	}
}
