using UnityEngine;

namespace XUnity.ResourceRedirector;

public interface IAssetLoadingContext
{
	AssetLoadingParameters Parameters { get; }

	AssetBundle Bundle { get; }

	Object[] Assets { get; set; }

	Object Asset { get; set; }

	string GetAssetBundlePath();

	string GetNormalizedAssetBundlePath();

	void Complete(bool skipRemainingPrefixes = true, bool? skipOriginalCall = true, bool? skipAllPostfixes = true);

	void DisableRecursion();
}
