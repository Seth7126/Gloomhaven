using System;
using UnityEngine.SceneManagement;

namespace XUnity.AutoTranslator.Plugin.Utilities;

internal static class SceneManagerLoader
{
	public static void EnableSceneLoadScanInternal(Action<int> sceneLoaded)
	{
		SceneManager.sceneLoaded += delegate(Scene arg1, LoadSceneMode arg2)
		{
			sceneLoaded(((Scene)(ref arg1)).buildIndex);
		};
	}
}
