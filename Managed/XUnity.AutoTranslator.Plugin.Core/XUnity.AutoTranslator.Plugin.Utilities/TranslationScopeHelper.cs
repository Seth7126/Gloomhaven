using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Constants;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Utilities;

internal static class TranslationScopeHelper
{
	public static int GetScope(object ui)
	{
		if (Settings.EnableTranslationScoping)
		{
			try
			{
				int num = -1;
				Component val = (Component)((ui is Component) ? ui : null);
				if (val != null && Object.op_Implicit((Object)(object)val))
				{
					num = GetScopeFromComponent(val);
				}
				if (num != -1)
				{
					return num;
				}
				if (ui is GUIContent)
				{
					return -1;
				}
				return GetActiveSceneId();
			}
			catch (MissingMemberException e)
			{
				XuaLogger.AutoTranslator.Error(e, "A 'missing member' error occurred while retriving translation scope. Disabling translation scopes.");
				Settings.EnableTranslationScoping = false;
			}
		}
		return -1;
	}

	private static int GetScopeFromComponent(Component component)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Scene scene = component.gameObject.scene;
		return ((Scene)(ref scene)).buildIndex;
	}

	public static int GetActiveSceneId()
	{
		if (UnityFeatures.SupportsSceneManager)
		{
			return GetActiveSceneIdBySceneManager();
		}
		return GetActiveSceneIdByApplication();
	}

	private static int GetActiveSceneIdBySceneManager()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Scene activeScene = SceneManager.GetActiveScene();
		return ((Scene)(ref activeScene)).buildIndex;
	}

	public static void RegisterSceneLoadCallback(Action<int> sceneLoaded)
	{
		SceneManagerLoader.EnableSceneLoadScanInternal(sceneLoaded);
	}

	private static int GetActiveSceneIdByApplication()
	{
		return Application.loadedLevel;
	}
}
