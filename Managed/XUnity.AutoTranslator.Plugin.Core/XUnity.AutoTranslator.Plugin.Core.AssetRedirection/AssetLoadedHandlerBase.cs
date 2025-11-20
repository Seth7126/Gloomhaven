using System;
using System.IO;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;
using XUnity.ResourceRedirector;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection;

public abstract class AssetLoadedHandlerBase<TAsset> where TAsset : Object
{
	protected bool CheckDirectory { get; set; }

	[Obsolete("Use AutoTranslatorSettings.IsDumpingRedirectedResourcesEnabled to obtain whether dumping is enabled instead.")]
	protected bool IsDumpingEnabled => AutoTranslatorSettings.IsDumpingRedirectedResourcesEnabled;

	public AssetLoadedHandlerBase()
	{
		ResourceRedirection.RegisterAssetLoadedHook(HookBehaviour.OneCallbackPerResourceLoaded, 0, HandleAsset);
		ResourceRedirection.RegisterResourceLoadedHook(HookBehaviour.OneCallbackPerResourceLoaded, 0, HandleResource);
	}

	private void HandleAsset(AssetLoadedContext context)
	{
		Handle(context);
	}

	private void HandleResource(ResourceLoadedContext context)
	{
		Handle(context);
	}

	private void Handle(IAssetOrResourceLoadedContext context)
	{
		if (!context.Asset.TryCastTo<TAsset>(out var castedObject) || !ShouldHandleAsset(castedObject, context))
		{
			return;
		}
		string uniqueFileSystemAssetPath = context.GetUniqueFileSystemAssetPath((Object)(object)castedObject);
		string text = CalculateModificationFilePath(castedObject, context);
		if ((CheckDirectory && Directory.Exists(text)) || (!CheckDirectory && File.Exists(text)))
		{
			try
			{
				bool flag = ReplaceOrUpdateAsset(text, ref castedObject, context);
				if (flag)
				{
					if (!Settings.EnableSilentMode)
					{
						XuaLogger.AutoTranslator.Debug("Replaced or updated resource file: '" + uniqueFileSystemAssetPath + "'.");
					}
				}
				else if (!Settings.EnableSilentMode)
				{
					XuaLogger.AutoTranslator.Debug("Did not replace or update resource file: '" + uniqueFileSystemAssetPath + "'.");
				}
				context.Complete(flag);
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Error(e, "An error occurred while replacing or updating resource file: '" + uniqueFileSystemAssetPath + "'.");
			}
		}
		else if (AutoTranslatorSettings.IsDumpingRedirectedResourcesEnabled)
		{
			try
			{
				bool flag2 = DumpAsset(text, castedObject, context);
				if (flag2)
				{
					if (!Settings.EnableSilentMode)
					{
						XuaLogger.AutoTranslator.Debug("Dumped resource file: '" + uniqueFileSystemAssetPath + "'.");
					}
				}
				else if (!Settings.EnableSilentMode)
				{
					XuaLogger.AutoTranslator.Debug("Did not dump resource file: '" + uniqueFileSystemAssetPath + "'.");
				}
				context.Complete(flag2);
			}
			catch (Exception e2)
			{
				XuaLogger.AutoTranslator.Error(e2, "An error occurred while dumping resource file: '" + uniqueFileSystemAssetPath + "'.");
			}
		}
		if (!UnityObjectReferenceComparer.Default.Equals(castedObject, context.Asset))
		{
			context.Asset = (Object)(object)castedObject;
		}
	}

	protected abstract bool ReplaceOrUpdateAsset(string calculatedModificationPath, ref TAsset asset, IAssetOrResourceLoadedContext context);

	protected abstract bool DumpAsset(string calculatedModificationPath, TAsset asset, IAssetOrResourceLoadedContext context);

	protected abstract string CalculateModificationFilePath(TAsset asset, IAssetOrResourceLoadedContext context);

	protected abstract bool ShouldHandleAsset(TAsset asset, IAssetOrResourceLoadedContext context);
}
