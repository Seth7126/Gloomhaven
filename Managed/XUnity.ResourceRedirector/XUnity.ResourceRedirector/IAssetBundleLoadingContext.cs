using UnityEngine;

namespace XUnity.ResourceRedirector;

public interface IAssetBundleLoadingContext
{
	AssetBundleLoadingParameters Parameters { get; }

	AssetBundle Bundle { get; set; }

	string GetNormalizedPath();

	void Complete();

	void Complete(bool skipRemainingPrefixes = true, bool? skipOriginalCall = true);

	void Complete(bool skipRemainingPrefixes = true, bool? skipOriginalCall = true, bool? skipAllPostfixes = true);

	void DisableRecursion();
}
