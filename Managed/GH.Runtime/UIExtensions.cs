#define ENABLE_LOGS
using System;
using System.Threading;
using System.Threading.Tasks;
using AddressableMisc;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public static class UIExtensions
{
	public static async Task<LoadingAddressableState> LoadSpriteAsyncAddressable(this Image image, object context, AssetReference assetReference, CancellationToken ct)
	{
		if (!assetReference.Exists())
		{
			throw new ArgumentNullException();
		}
		var (sprite, loadingAddressableState) = await AddressableLoaderHelper.LoadAssetAsync<Sprite>(context, assetReference, ct);
		if (sprite == null)
		{
			Debug.LogWarning($"Load of asset is {loadingAddressableState}!");
			return loadingAddressableState;
		}
		image.sprite = sprite;
		return loadingAddressableState;
	}

	public static async Task<LoadingAddressableState> LoadSpriteAsyncAddressable(this Image image, object context, AssetReference assetReference)
	{
		return await image.LoadSpriteAsyncAddressable(context, assetReference, CancellationToken.None);
	}

	public static async Task<(Sprite, LoadingAddressableState)> LoadSpriteAsync(object context, AssetReference assetReference, CancellationToken ct)
	{
		if (!assetReference.Exists())
		{
			throw new ArgumentNullException();
		}
		return await AddressableLoaderHelper.LoadAssetAsync<Sprite>(context, assetReference, ct);
	}
}
