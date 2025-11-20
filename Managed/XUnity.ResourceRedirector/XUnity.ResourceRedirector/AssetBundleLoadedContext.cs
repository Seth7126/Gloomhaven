using UnityEngine;
using XUnity.Common.Extensions;
using XUnity.ResourceRedirector.Constants;

namespace XUnity.ResourceRedirector;

public class AssetBundleLoadedContext
{
	private string _normalizedPath;

	public AssetBundleLoadingParameters Parameters { get; }

	public AssetBundle Bundle { get; set; }

	internal bool SkipRemainingPostfixes { get; private set; }

	internal AssetBundleLoadedContext(AssetBundleLoadingParameters parameters, AssetBundle bundle)
	{
		Parameters = parameters;
		Bundle = bundle;
	}

	public string GetNormalizedPath()
	{
		if (_normalizedPath == null && Parameters.Path != null)
		{
			_normalizedPath = Parameters.Path.ToLowerInvariant().UseCorrectDirectorySeparators().MakeRelativePath(EnvironmentEx.LoweredCurrentDirectory);
		}
		return _normalizedPath;
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
