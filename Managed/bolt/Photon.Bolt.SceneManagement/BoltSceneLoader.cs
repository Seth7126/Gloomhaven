#define DEBUG
using System.Collections;
using Photon.Bolt.Collections;
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Photon.Bolt.SceneManagement;

internal class BoltSceneLoader : MonoBehaviour
{
	public class LoadSceneOperation : BoltObject, IBoltListNode<LoadSceneOperation>
	{
		public SceneLoadState scene;

		public AsyncOperation async;

		LoadSceneOperation IBoltListNode<LoadSceneOperation>.prev { get; set; }

		LoadSceneOperation IBoltListNode<LoadSceneOperation>.next { get; set; }

		object IBoltListNode<LoadSceneOperation>.list { get; set; }
	}

	private static Coroutine _currentLoad;

	private static readonly int _delay = 60;

	private static readonly BoltSingleList<LoadSceneOperation> _loadOps = new BoltSingleList<LoadSceneOperation>();

	private static BoltSceneLoader _instance;

	internal static AsyncOperation CurrentAsyncOperation
	{
		get
		{
			if (_loadOps != null && _loadOps.count > 0 && _loadOps.first.async != null)
			{
				return _loadOps.first.async;
			}
			return null;
		}
	}

	internal static bool IsLoading => _loadOps.count > 0;

	private void Start()
	{
		_instance = this;
	}

	private void Update()
	{
		if (_loadOps.count > 0 && _loadOps.first.async == null)
		{
			if (_currentLoad != null)
			{
				StopCoroutine(_currentLoad);
			}
			_currentLoad = StartCoroutine(LoadAsync(_loadOps.first));
		}
	}

	private void OnDestroy()
	{
		Clear();
	}

	private IEnumerator LoadAsync(LoadSceneOperation loadOp)
	{
		SceneLoadState scene = loadOp.scene;
		int sceneIndex = scene.Scene.Index;
		string sceneName = BoltNetworkInternal.GetSceneName(sceneIndex);
		BoltLog.Debug("Loading {0} ({1})", sceneName, sceneIndex);
		loadOp.async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
		BoltCore.SceneLoadBegin(scene);
		yield return loadOp.async;
		StartCoroutine(Done(scene, sceneName));
	}

	private IEnumerator Done(SceneLoadState scene, string sceneName)
	{
		for (int i = 0; i < _delay; i++)
		{
			yield return null;
		}
		BoltCore.SceneLoadDone(scene);
		_loadOps.RemoveFirst();
		BoltLog.Debug("Done loading {0}", sceneName);
	}

	internal static void Clear()
	{
		if (_currentLoad != null && _instance != null)
		{
			_instance.StopCoroutine(_currentLoad);
		}
		_loadOps.Clear();
	}

	internal static void Enqueue(SceneLoadState scene)
	{
		BoltLog.Debug("Scene {0} enqueued to load", BoltNetworkInternal.GetSceneName(scene.Scene.Index));
		_loadOps.AddLast(new LoadSceneOperation
		{
			scene = scene
		});
	}
}
