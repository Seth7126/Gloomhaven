using System;
using System.IO;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.ResourceRedirector;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection;

public static class AssetLoadedContextExtensions
{
	public static string GetPreferredFilePath(this IAssetOrResourceLoadedContext context, Object asset, string extension)
	{
		string path;
		if (context is AssetLoadedContext)
		{
			path = "assets";
		}
		else
		{
			if (!(context is ResourceLoadedContext))
			{
				throw new ArgumentException("context");
			}
			path = "resources";
		}
		return Path.Combine(Path.Combine(Settings.RedirectedResourcesPath, path), context.GetUniqueFileSystemAssetPath(asset)) + extension;
	}

	public static string GetPreferredFilePath(this IAssetOrResourceLoadedContext context, string parentDirectory, Object asset, string extension)
	{
		return Path.Combine(parentDirectory, context.GetUniqueFileSystemAssetPath(asset)) + extension;
	}

	public static string GetPreferredFilePathWithCustomFileName(this IAssetOrResourceLoadedContext context, Object asset, string fileName)
	{
		string path;
		if (context is AssetLoadedContext)
		{
			path = "assets";
		}
		else
		{
			if (!(context is ResourceLoadedContext))
			{
				throw new ArgumentException("context");
			}
			path = "resources";
		}
		string text = Path.Combine(Path.Combine(Settings.RedirectedResourcesPath, path), context.GetUniqueFileSystemAssetPath(asset));
		if (fileName != null)
		{
			text = Path.Combine(text, fileName);
		}
		return text;
	}

	public static string GetPreferredFilePathWithCustomFileName(this IAssetOrResourceLoadedContext context, string parentDirectory, Object asset, string fileName)
	{
		string text = Path.Combine(parentDirectory, context.GetUniqueFileSystemAssetPath(asset));
		if (fileName != null)
		{
			text = Path.Combine(text, fileName);
		}
		return text;
	}
}
