#define ENABLE_LOGS
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Script.GUI.SMNavigation;
using SharedLibrary.SimpleLog;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

public class Bootstrap : MonoBehaviour
{
	[SerializeField]
	private PlatformLayer _platformLayer;

	private AsyncOperation _loadScene;

	private readonly Stopwatch _stopwatch = new Stopwatch();

	[UsedImplicitly]
	private void Start()
	{
		_platformLayer.InitialisePlatformLayer();
		PlatformLayer.Boost?.EnableCpuBoost();
		SimpleLog.Initialize(PlatformLayer.FileSystem);
		InputSystemUtilities.EnableTouchpadInputSystem();
		InputSystemUtilities.EnableMouseInputSystem();
		Texture.streamingTextureDiscardUnusedMips = true;
		GloomhavenShared.LogBuildInfo();
		Singleton<UINavigation>.Instance.Setup();
		Singleton<KeyActionHandlerController>.Instance.Setup();
		if (Application.platform == RuntimePlatform.OSXPlayer)
		{
			string path = Path.Combine(PathsManager.PersistionDataPath, RootSaveData.SaveRootFolder, "GlobalData.dat");
			if (!PlatformLayer.FileSystem.ExistsFile(path))
			{
				Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
			}
		}
		_stopwatch.Restart();
		Debug.Log($"[Bootstrap] Load scene started. Current:{_stopwatch.ElapsedMilliseconds}ms RealTime:{Time.realtimeSinceStartup}s");
		StartCoroutine(ShowSplash());
	}

	private IEnumerator ShowSplash()
	{
		Debug.Log("[Bootstrap] Showing splash screen");
		string introSceneName = "Intro";
		AsyncOperation introLoadAsyncOperation = SceneManager.LoadSceneAsync(introSceneName, LoadSceneMode.Single);
		bool introCompleted = false;
		introLoadAsyncOperation.completed += OnIntroLoadCompleted;
		while (!introCompleted)
		{
			yield return null;
		}
		Debug.Log("[Bootstrap] Finished showing splash screen.");
		string sceneName = "Gloomhaven_unified";
		_loadScene = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
		_loadScene.allowSceneActivation = true;
		_loadScene.completed += OnLoadCompleted;
		void OnIntroLoadCompleted(AsyncOperation operation)
		{
			introLoadAsyncOperation.completed -= OnIntroLoadCompleted;
			IntroPlayer player = SceneManager.GetSceneByName(introSceneName).GetRootGameObjects().First((GameObject x) => x.GetComponentInChildren<IntroPlayer>() != null)
				.GetComponentInChildren<IntroPlayer>();
			player.EventCompleted += OnIntroCompleted;
			void OnIntroCompleted()
			{
				player.EventCompleted -= OnIntroCompleted;
				SceneManager.UnloadSceneAsync(introSceneName);
				introCompleted = true;
			}
		}
		void OnLoadCompleted(AsyncOperation operation)
		{
			_loadScene.completed -= OnLoadCompleted;
			Debug.Log($"[Bootstrap] Loaded scene in:{_stopwatch.ElapsedMilliseconds}ms RealTime:{Time.realtimeSinceStartup}s");
		}
	}

	private void OnDestroy()
	{
		Debug.Log("[Bootstrap] OnDestroy");
	}

	private static void OnPanelVisibilityChanged(bool visible)
	{
		InputManager.UpdateMouseInputEnabled(visible);
	}
}
