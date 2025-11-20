using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using XUnity.Common.Extensions;
using XUnity.Common.Utilities;

namespace XUnity.ResourceRedirector;

public class AssetLoadedContext : IAssetOrResourceLoadedContext
{
	private AssetBundleExtensionData _ext;

	private bool _lookedForExt;

	private BackingFieldOrArray _backingField;

	public AssetLoadedParameters Parameters { get; }

	public AssetBundle Bundle { get; }

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

	internal bool SkipRemainingPostfixes { get; private set; }

	internal AssetLoadedContext(AssetLoadedParameters parameters, AssetBundle bundle, Object[] assets)
	{
		Parameters = parameters;
		Bundle = bundle;
		_backingField = new BackingFieldOrArray(assets);
	}

	internal AssetLoadedContext(AssetLoadedParameters parameters, AssetBundle bundle, Object asset)
	{
		Parameters = parameters;
		Bundle = bundle;
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
			string text = Bundle.GetExtensionData<AssetBundleExtensionData>()?.NormalizedPath;
			string path = (string.IsNullOrEmpty(text) ? "unnamed_assetbundle" : text.ToLowerInvariant());
			string name = asset.name;
			if (!string.IsNullOrEmpty(name))
			{
				path = Path.Combine(path, name.ToLowerInvariant());
			}
			else
			{
				string text2 = null;
				if (Assets.Length > 1)
				{
					int num = Array.IndexOf(Assets, asset);
					text2 = ((num != -1) ? ("_" + num.ToString(CultureInfo.InvariantCulture)) : "_with_unknown_index");
				}
				path = Path.Combine(path, (Parameters.LoadType == AssetLoadType.LoadMainAsset) ? "main_asset" : ("unnamed_asset" + text2));
			}
			path = path.UseCorrectDirectorySeparators();
			orCreateExtensionData.FullFileSystemAssetPath = path;
		}
		return orCreateExtensionData.FullFileSystemAssetPath;
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

	public void Complete(bool skipRemainingPostfixes = true)
	{
		SkipRemainingPostfixes = skipRemainingPostfixes;
	}

	public void DisableRecursion()
	{
		ResourceRedirection.RecursionEnabled = false;
	}
}
