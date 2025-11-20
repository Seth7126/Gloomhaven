using System;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class TextAssetHooks
{
	public static readonly Type[] All = new Type[2]
	{
		typeof(TextAsset_bytes_Hook),
		typeof(TextAsset_text_Hook)
	};
}
