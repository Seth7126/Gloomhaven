using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Hooks;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;
using XUnity.ResourceRedirector;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection;

internal class TextAssetLoadedHandler : AssetLoadedHandlerBaseV2<TextAsset>
{
	public TextAssetLoadedHandler()
	{
		HooksSetup.InstallTextAssetHooks();
	}

	protected override string CalculateModificationFilePath(TextAsset asset, IAssetOrResourceLoadedContext context)
	{
		return context.GetPreferredFilePath((Object)(object)asset, ".txt");
	}

	protected override bool DumpAsset(string calculatedModificationPath, TextAsset asset, IAssetOrResourceLoadedContext context)
	{
		Directory.CreateDirectory(new FileInfo(calculatedModificationPath).Directory.FullName);
		byte[] bytes = asset.bytes;
		if (bytes != null)
		{
			File.WriteAllBytes(calculatedModificationPath, bytes);
			return true;
		}
		string text = asset.text;
		if (text != null)
		{
			File.WriteAllText(calculatedModificationPath, text, Encoding.UTF8);
			return true;
		}
		return false;
	}

	protected override bool ReplaceOrUpdateAsset(string calculatedModificationPath, ref TextAsset asset, IAssetOrResourceLoadedContext context)
	{
		List<RedirectedResource> list = RedirectedDirectory.GetFile(calculatedModificationPath).ToList();
		if (list.Count == 0)
		{
			return false;
		}
		if (list.Count > 1)
		{
			XuaLogger.AutoTranslator.Warn("Found more than one resource file in the same path: " + calculatedModificationPath);
		}
		RedirectedResource redirectedResource = list.FirstOrDefault();
		if (redirectedResource != null)
		{
			using (Stream stream = redirectedResource.OpenStream())
			{
				byte[] data = stream.ReadFully((int)stream.Length);
				TextAssetExtensionData orCreateExtensionData = asset.GetOrCreateExtensionData<TextAssetExtensionData>();
				orCreateExtensionData.Encoding = Encoding.UTF8;
				orCreateExtensionData.Data = data;
				return true;
			}
		}
		return false;
	}

	protected override bool ShouldHandleAsset(TextAsset asset, IAssetOrResourceLoadedContext context)
	{
		return !context.HasReferenceBeenRedirectedBefore((Object)(object)asset);
	}
}
