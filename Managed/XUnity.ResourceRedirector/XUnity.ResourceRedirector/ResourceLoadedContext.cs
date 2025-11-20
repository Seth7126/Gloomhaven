using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using XUnity.Common.Extensions;
using XUnity.Common.Utilities;

namespace XUnity.ResourceRedirector;

public class ResourceLoadedContext : IAssetOrResourceLoadedContext
{
	private BackingFieldOrArray _backingField;

	public ResourceLoadedParameters Parameters { get; }

	public Object[] Assets
	{
		get
		{
			return _backingField.Array;
		}
		set
		{
			_backingField.Array = value;
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
			_backingField.Field = value;
		}
	}

	internal bool SkipRemainingPostfixes { get; set; }

	internal ResourceLoadedContext(ResourceLoadedParameters parameters, Object[] assets)
	{
		Parameters = parameters;
		_backingField = new BackingFieldOrArray(assets);
	}

	internal ResourceLoadedContext(ResourceLoadedParameters parameters, Object asset)
	{
		Parameters = parameters;
		_backingField = new BackingFieldOrArray(asset);
	}

	public bool HasReferenceBeenRedirectedBefore(Object asset)
	{
		return asset.GetExtensionData<ResourceExtensionData>()?.HasBeenRedirected ?? false;
	}

	public string GetUniqueFileSystemAssetPath(Object asset)
	{
		ResourceExtensionData orCreateExtensionData = asset.GetOrCreateExtensionData<ResourceExtensionData>();
		if (orCreateExtensionData.FullFileSystemAssetPath == null)
		{
			string text = string.Empty;
			if (!string.IsNullOrEmpty(Parameters.Path))
			{
				text = Parameters.Path.ToLowerInvariant();
			}
			if (Parameters.LoadType == ResourceLoadType.LoadByType)
			{
				string name = asset.name;
				if (!string.IsNullOrEmpty(name))
				{
					text = Path.Combine(text, name.ToLowerInvariant());
				}
				else
				{
					string text2 = null;
					if (Assets.Length > 1)
					{
						int num = Array.IndexOf(Assets, asset);
						text2 = ((num != -1) ? ("_" + num.ToString(CultureInfo.InvariantCulture)) : "_with_unknown_index");
					}
					text = Path.Combine(text, "unnamed_asset" + text2);
				}
			}
			text = text.UseCorrectDirectorySeparators();
			orCreateExtensionData.FullFileSystemAssetPath = text;
		}
		return orCreateExtensionData.FullFileSystemAssetPath;
	}

	public void Complete(bool skipRemainingPostfixes = true)
	{
		SkipRemainingPostfixes = skipRemainingPostfixes;
	}

	public void DisableRecursion()
	{
		ResourceRedirection.RecursionEnabled = false;
	}
}
