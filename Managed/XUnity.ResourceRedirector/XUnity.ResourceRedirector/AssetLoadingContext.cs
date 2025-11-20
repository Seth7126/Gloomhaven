using UnityEngine;
using XUnity.Common.Utilities;

namespace XUnity.ResourceRedirector;

public class AssetLoadingContext : IAssetLoadingContext
{
	private AssetBundleExtensionData _ext;

	private bool _lookedForExt;

	private BackingFieldOrArray _backingField;

	public AssetLoadingParameters Parameters { get; }

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

	internal bool SkipRemainingPrefixes { get; private set; }

	internal bool SkipOriginalCall { get; private set; }

	internal bool SkipAllPostfixes { get; private set; }

	internal AssetLoadingContext(AssetLoadingParameters parameters, AssetBundle bundle)
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
