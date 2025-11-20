using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Hooks;
using XUnity.Common.Utilities;
using XUnity.ResourceRedirector;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection;

public abstract class TextAssetLoadedHandlerBase : AssetLoadedHandlerBaseV2<TextAsset>
{
	public TextAssetLoadedHandlerBase()
	{
		HooksSetup.InstallTextAssetHooks();
	}

	public abstract TextAndEncoding TranslateTextAsset(string calculatedModificationPath, TextAsset asset, IAssetOrResourceLoadedContext context);

	protected sealed override bool ReplaceOrUpdateAsset(string calculatedModificationPath, ref TextAsset asset, IAssetOrResourceLoadedContext context)
	{
		TextAndEncoding textAndEncoding = TranslateTextAsset(calculatedModificationPath, asset, context);
		if (textAndEncoding != null)
		{
			TextAssetExtensionData orCreateExtensionData = asset.GetOrCreateExtensionData<TextAssetExtensionData>();
			orCreateExtensionData.Encoding = textAndEncoding.Encoding;
			orCreateExtensionData.Text = textAndEncoding.Text;
			orCreateExtensionData.Data = textAndEncoding.Bytes;
			return true;
		}
		return false;
	}
}
