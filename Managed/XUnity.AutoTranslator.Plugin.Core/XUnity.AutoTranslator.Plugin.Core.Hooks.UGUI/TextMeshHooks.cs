using System;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI;

internal static class TextMeshHooks
{
	public static readonly Type[] All = new Type[3]
	{
		typeof(TextMesh_text_Hook),
		typeof(GameObject_SetActive_Hook),
		typeof(GameObject_active_Hook)
	};
}
