using UnityEngine.AddressableAssets;

public static class AssetReferenceExtensions
{
	public static bool Exists(this AssetReference assetReference)
	{
		if (assetReference != null && !string.IsNullOrEmpty(assetReference.AssetGUID))
		{
			return assetReference.RuntimeKeyIsValid();
		}
		return false;
	}
}
