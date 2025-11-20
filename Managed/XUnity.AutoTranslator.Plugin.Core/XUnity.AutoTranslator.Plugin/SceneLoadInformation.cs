using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin;

internal class SceneLoadInformation
{
	public SceneInformation ActiveScene { get; set; }

	public List<SceneInformation> LoadedScenes { get; set; }

	public SceneLoadInformation()
	{
		LoadedScenes = new List<SceneInformation>();
		if (UnityFeatures.SupportsSceneManager)
		{
			LoadBySceneManager();
		}
		else
		{
			LoadByApplication();
		}
	}

	public void LoadBySceneManager()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Scene activeScene = SceneManager.GetActiveScene();
		ActiveScene = new SceneInformation(((Scene)(ref activeScene)).buildIndex, ((Scene)(ref activeScene)).name);
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			LoadedScenes.Add(new SceneInformation(((Scene)(ref sceneAt)).buildIndex, ((Scene)(ref sceneAt)).name));
		}
	}

	public void LoadByApplication()
	{
		ActiveScene = new SceneInformation(Application.loadedLevel, Application.loadedLevelName);
		LoadedScenes.Add(new SceneInformation(Application.loadedLevel, Application.loadedLevelName));
	}
}
