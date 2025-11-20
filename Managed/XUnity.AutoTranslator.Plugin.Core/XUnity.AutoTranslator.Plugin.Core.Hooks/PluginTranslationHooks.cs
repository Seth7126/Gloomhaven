using System;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class PluginTranslationHooks
{
	public static readonly Type[] All = new Type[6]
	{
		typeof(Transform_SetParent_Hook),
		typeof(GameObject_AddComponent_Hook),
		typeof(Object_InstantiateSingle_Hook),
		typeof(Object_InstantiateSingleWithParent_Hook),
		typeof(Object_CloneSingle_Hook),
		typeof(Object_CloneSingleWithParent_Hook)
	};
}
