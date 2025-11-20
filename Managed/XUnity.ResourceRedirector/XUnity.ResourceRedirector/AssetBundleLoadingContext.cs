using UnityEngine;
using XUnity.Common.Extensions;
using XUnity.ResourceRedirector.Constants;

namespace XUnity.ResourceRedirector;

public class AssetBundleLoadingContext : IAssetBundleLoadingContext
{
	private string _normalizedPath;

	public AssetBundleLoadingParameters Parameters { get; }

	public AssetBundle Bundle { get; set; }

	internal bool SkipRemainingPrefixes { get; private set; }

	internal bool SkipOriginalCall { get; private set; }

	internal bool SkipAllPostfixes { get; private set; }

	internal AssetBundleLoadingContext(AssetBundleLoadingParameters parameters)
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
