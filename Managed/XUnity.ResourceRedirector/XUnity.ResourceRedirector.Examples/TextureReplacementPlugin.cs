using UnityEngine;

namespace XUnity.ResourceRedirector.Examples;

internal class TextureReplacementPlugin
{
	private void Awake()
	{
		ResourceRedirection.RegisterAssetLoadedHook(HookBehaviour.OneCallbackPerResourceLoaded, 0, AssetLoaded);
	}

	public void AssetLoaded(AssetLoadedContext context)
	{
		Object asset = context.Asset;
		Texture2D val = (Texture2D)(object)((asset is Texture2D) ? asset : null);
		if (val != null)
		{
			context.Asset = (Object)(object)val;
			context.Complete();
		}
	}
}
