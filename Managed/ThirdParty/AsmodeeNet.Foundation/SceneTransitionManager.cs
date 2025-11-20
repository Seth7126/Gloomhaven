using System;
using System.Collections;
using System.Collections.Generic;
using AsmodeeNet.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AsmodeeNet.Foundation;

public class SceneTransitionManager : MonoBehaviour
{
	public enum TransitionType
	{
		None,
		FadeOut,
		FadeIn,
		FadeOutIn
	}

	private const string _documentation = "<b>SceneTransitionManager</b> loads and displays <b>Scene</b>s with transitions by using <i>Multi Scene Editing</i> system";

	private Dictionary<string, AsyncOperation> _loadingOperations = new Dictionary<string, AsyncOperation>();

	private bool _isTransitioning;

	private TransitionType _transitionType;

	private float _transitionSpeed;

	private string _nextSceneName;

	private const string _debugModuleName = "SceneTransitionManager";

	private Texture2D _fadingTexture;

	private float _fadingAlpha;

	private int _fadingDirection;

	public Color fadingColor = Color.black;

	public bool IsTransitioning
	{
		get
		{
			return _isTransitioning;
		}
		private set
		{
			_isTransitioning = value;
			if (_isTransitioning)
			{
				CoreApplication.Instance.UINavigationManager.BeginIgnoringInteractionEvents("SceneTransitionManager");
			}
			else
			{
				CoreApplication.Instance.UINavigationManager.EndIgnoringInteractionEvents("SceneTransitionManager");
			}
		}
	}

	private bool IsInstantTransition
	{
		get
		{
			if (!IsTransitioning || _transitionType != TransitionType.None)
			{
				return Mathf.Approximately(_transitionSpeed, 0f);
			}
			return true;
		}
	}

	private Texture2D FadingTexture
	{
		get
		{
			if (_fadingTexture == null)
			{
				_fadingTexture = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
				_fadingTexture.SetPixel(0, 0, fadingColor);
				_fadingTexture.Apply();
			}
			return _fadingTexture;
		}
	}

	public event Action<string> SceneWillLoad;

	public event Action<Scene> SceneDidLoad;

	public event Action<Scene> SceneWillUnload;

	public event Action<Scene> SceneDidUnload;

	public bool IsCurrentScene(string sceneName)
	{
		return SceneManager.GetActiveScene().name == sceneName;
	}

	public void PreLoadScene(string sceneName)
	{
		if (IsTransitioning)
		{
			AsmoLogger.Warning("SceneTransitionManager", "Pre loading a scene during a transition is not supported");
		}
		else
		{
			CoreApplication.ExecuteCoroutine(LoadScene(sceneName));
		}
	}

	public void DisplayScene(string sceneName, TransitionType transitionType = TransitionType.FadeOutIn, float transitionDuration = 1f, bool forceReload = false)
	{
		if (IsTransitioning)
		{
			AsmoLogger.Warning("SceneTransitionManager", "Displaying a scene during a transition is not supported");
			return;
		}
		_nextSceneName = sceneName;
		_transitionType = transitionType;
		transitionDuration = Mathf.Max(0f, transitionDuration);
		float num = ((_transitionType == TransitionType.FadeOutIn) ? 2f : 1f);
		_transitionSpeed = num / transitionDuration;
		IsTransitioning = !IsInstantTransition;
		Hashtable extraInfo = new Hashtable
		{
			{ "type", transitionType },
			{ "duration", transitionDuration },
			{ "isInstant", IsInstantTransition }
		};
		Scene sceneByName = SceneManager.GetSceneByName(_nextSceneName);
		if (sceneByName.IsValid() && !forceReload)
		{
			if (sceneByName.isLoaded)
			{
				AsmoLogger.Debug("SceneTransitionManager", () => "DisplayScene: " + _nextSceneName + " [Already Loaded]", extraInfo);
				if (IsInstantTransition)
				{
					SetSceneActive(sceneByName);
				}
			}
			else
			{
				AsmoLogger.Debug("SceneTransitionManager", () => "DisplayScene: " + _nextSceneName + " [Loading]", extraInfo);
				if (IsInstantTransition)
				{
					FinishLoadingScene(_nextSceneName);
				}
			}
		}
		else
		{
			AsmoLogger.Debug("SceneTransitionManager", () => "DisplayScene: " + _nextSceneName + " [Not Loaded]", extraInfo);
			bool isInstantTransition = IsInstantTransition;
			CoreApplication.ExecuteCoroutine(LoadScene(_nextSceneName, isInstantTransition));
		}
		if (!IsInstantTransition)
		{
			StartFadeOut();
		}
	}

	public IEnumerator LoadScene(string sceneName, bool allowSceneActivation = false)
	{
		CallSceneWillLoad(sceneName);
		AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
		if (!allowSceneActivation)
		{
			operation.allowSceneActivation = false;
			_loadingOperations.Add(sceneName, operation);
		}
		float progress = -1f;
		while (!operation.isDone)
		{
			if (operation.progress > progress)
			{
				progress = operation.progress;
				AsmoLogger.Debug("SceneTransitionManager", () => $"Loading scene: {sceneName} [{progress * 100f}%]");
			}
			yield return null;
		}
		Scene? scene = null;
		for (int num = 0; num < SceneManager.sceneCount; num++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(num);
			if (sceneAt.name == sceneName && sceneAt != SceneManager.GetActiveScene())
			{
				scene = sceneAt;
				break;
			}
		}
		if (scene.HasValue)
		{
			CallSceneDidLoad(scene.Value);
			SetSceneActive(scene.Value);
		}
		else
		{
			AsmoLogger.Error("SceneTransitionManager", "Scene not found during loading");
		}
	}

	private void FinishLoadingScene(string sceneName)
	{
		AsmoLogger.Debug("SceneTransitionManager", "Finish loading scene: " + sceneName);
		_loadingOperations[sceneName].allowSceneActivation = true;
		_loadingOperations.Remove(sceneName);
	}

	private void SetSceneActive(Scene scene)
	{
		AsmoLogger.Debug("SceneTransitionManager", "Set scene active: " + scene.name);
		Scene activeScene = SceneManager.GetActiveScene();
		SceneManager.SetActiveScene(scene);
		CallSceneWillUnload(activeScene);
		SceneManager.UnloadSceneAsync(activeScene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
		Resources.UnloadUnusedAssets();
		CallSceneDidUnload(activeScene);
	}

	private void StartFadeOut()
	{
		_fadingDirection = 1;
		if (_transitionType == TransitionType.FadeOut || _transitionType == TransitionType.FadeOutIn)
		{
			_fadingAlpha = 0f;
		}
		else
		{
			_fadingAlpha = 1f;
		}
	}

	private void StartFadeIn()
	{
		_fadingDirection = -1;
		if (_transitionType == TransitionType.FadeIn || _transitionType == TransitionType.FadeOutIn)
		{
			_fadingAlpha = 1f;
		}
		else
		{
			_fadingAlpha = 0f;
		}
	}

	private void PauseFade()
	{
		_fadingDirection = 0;
		_fadingAlpha = 1f;
	}

	private void StopFade()
	{
		_fadingDirection = 0;
		_fadingAlpha = 0f;
	}

	[UsedImplicitly]
	private void OnGUI()
	{
		if (!IsTransitioning)
		{
			return;
		}
		_fadingAlpha += (float)_fadingDirection * _transitionSpeed * Time.deltaTime;
		if (_fadingDirection > 0 && _fadingAlpha > 1f)
		{
			PauseFade();
			FinishLoadingScene(_nextSceneName);
		}
		else if (_fadingDirection == 0)
		{
			if (SceneManager.GetActiveScene().name == _nextSceneName && SceneManager.sceneCount == 1)
			{
				StartFadeIn();
			}
		}
		else if (_fadingDirection < 0 && _fadingAlpha < 0f)
		{
			StopFade();
			IsTransitioning = false;
		}
		GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, Mathf.Clamp01(_fadingAlpha));
		GUI.depth = -1000;
		GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), FadingTexture);
		if (!IsTransitioning)
		{
			_fadingTexture = null;
		}
	}

	private void CallSceneWillLoad(string sceneName)
	{
		AsmoLogger.Debug("SceneTransitionManager", "Will load scene: " + sceneName);
		if (this.SceneWillLoad != null)
		{
			this.SceneWillLoad(sceneName);
		}
	}

	private void CallSceneDidLoad(Scene scene)
	{
		AsmoLogger.Debug("SceneTransitionManager", "Did load scene: " + scene.name);
		if (this.SceneDidLoad != null)
		{
			this.SceneDidLoad(scene);
		}
	}

	private void CallSceneWillUnload(Scene scene)
	{
		AsmoLogger.Debug("SceneTransitionManager", "Will unload scene: " + scene.name);
		if (this.SceneWillUnload != null)
		{
			this.SceneWillUnload(scene);
		}
	}

	private void CallSceneDidUnload(Scene scene)
	{
		AsmoLogger.Debug("SceneTransitionManager", "Did unload scene: " + scene.name);
		if (this.SceneDidUnload != null)
		{
			this.SceneDidUnload(scene);
		}
	}
}
