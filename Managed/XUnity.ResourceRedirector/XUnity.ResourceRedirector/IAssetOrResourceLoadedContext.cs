using UnityEngine;

namespace XUnity.ResourceRedirector;

public interface IAssetOrResourceLoadedContext
{
	Object[] Assets { get; set; }

	Object Asset { get; set; }

	bool HasReferenceBeenRedirectedBefore(Object asset);

	string GetUniqueFileSystemAssetPath(Object asset);

	void Complete(bool skipRemainingPostfixes = true);

	void DisableRecursion();
}
