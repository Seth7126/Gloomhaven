#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class StoryImageViewer : MonoBehaviour
{
	private class LoadImageToken
	{
		private readonly List<AsyncOperationHandle<Sprite>> _handles;

		private readonly Dictionary<string, Sprite> loadedImages = new Dictionary<string, Sprite>();

		private readonly List<string> loadingImages = new List<string>();

		public LoadImageToken(List<AsyncOperationHandle<Sprite>> handles)
		{
			_handles = handles;
		}

		public void Load(List<string> imagesIds)
		{
			foreach (string imagesId in imagesIds)
			{
				Load(imagesId);
			}
		}

		private void Load(string imageId)
		{
			if (imageId.IsNullOrEmpty())
			{
				return;
			}
			if (loadedImages.ContainsKey(imageId))
			{
				Debug.LogGUI("Already loaded " + imageId);
				return;
			}
			NarrativeConfigUI narrativeImageConfig = UIInfoTools.Instance.GetNarrativeImageConfig(imageId);
			if (narrativeImageConfig != null && !(narrativeImageConfig.picture != null))
			{
				if (loadingImages.Contains(imageId))
				{
					Debug.LogGUI("Already loading " + imageId);
					return;
				}
				Debug.LogGUI("Start loading " + narrativeImageConfig.pathPicture);
				loadingImages.Add(imageId);
				CoroutineHelper.RunCoroutine(LoadImageAsync(narrativeImageConfig.pathPicture, imageId, _handles));
			}
		}

		private IEnumerator LoadImageAsync(string path, string imageID, List<AsyncOperationHandle<Sprite>> handles)
		{
			path = path.Replace("\\", "/") + ".png";
			AsyncOperationHandle<Sprite> loading = Addressables.LoadAssetAsync<Sprite>(path);
			if (loading.OperationException != null)
			{
				loadingImages.Remove(imageID);
				Debug.LogError("Unable to load image from path " + path);
				yield break;
			}
			while (loading.Status == AsyncOperationStatus.None)
			{
				yield return null;
			}
			handles.Add(loading);
			loadingImages.Remove(imageID);
			loadedImages[imageID] = loading.Result;
		}

		private IEnumerator WaitForImageLoad(string imageID, Action<Sprite> onLoaded)
		{
			while (loadingImages.Contains(imageID))
			{
				yield return null;
			}
			if (loadedImages.ContainsKey(imageID))
			{
				onLoaded(loadedImages[imageID]);
			}
			else
			{
				onLoaded(null);
			}
		}

		public void Get(string imageId, Action<Sprite> onLoaded)
		{
			if (imageId.IsNullOrEmpty())
			{
				onLoaded(null);
			}
			else if (loadedImages.ContainsKey(imageId))
			{
				Debug.LogGUI("Get loaded image " + imageId);
				onLoaded(loadedImages[imageId]);
			}
			else if (loadingImages.Contains(imageId))
			{
				Debug.LogGUI("Get loading image " + imageId);
				CoroutineHelper.RunCoroutine(WaitForImageLoad(imageId, onLoaded));
			}
			else
			{
				NarrativeConfigUI narrativeImageConfig = UIInfoTools.Instance.GetNarrativeImageConfig(imageId);
				onLoaded((narrativeImageConfig?.picture != null) ? narrativeImageConfig.picture : null);
			}
		}

		public void Free()
		{
			loadedImages.Clear();
			loadingImages.Clear();
		}

		public void Cancel(string imageId)
		{
			if (imageId.IsNOTNullOrEmpty() && loadingImages.Contains(imageId))
			{
				Debug.LogGUI("Cancel loading image " + imageId);
				loadingImages.Remove(imageId);
			}
		}

		public bool AreAllImagesLoaded()
		{
			return loadingImages.Count == 0;
		}
	}

	[SerializeField]
	private GameObject container;

	[SerializeField]
	private Image imageHolder;

	[SerializeField]
	private GUIAnimator loadImagesAnimator;

	[SerializeField]
	private GUIAnimator showImageAnimator;

	private Action onShownImage;

	private LoadImageToken token;

	private Action onLoaded;

	private string currentImage;

	private List<AsyncOperationHandle<Sprite>> _handles = new List<AsyncOperationHandle<Sprite>>();

	[UsedImplicitly]
	private void Awake()
	{
		loadImagesAnimator.OnAnimationFinished.AddListener(WaitLoadImage);
		loadImagesAnimator.OnAnimationStarted.AddListener(delegate
		{
			Debug.LogGUI("Start load images anim");
		});
		loadImagesAnimator.OnAnimationStopped.AddListener(delegate
		{
			Debug.LogGUI("Load images anim stopped");
		});
		if (showImageAnimator != null)
		{
			showImageAnimator.OnAnimationFinished.AddListener(delegate
			{
				onShownImage?.Invoke();
			});
		}
	}

	[UsedImplicitly]
	protected void OnDestroy()
	{
		foreach (AsyncOperationHandle<Sprite> handle in _handles)
		{
			AssetBundleManager.ReleaseHandle(handle);
		}
	}

	public void LoadImages(List<string> images, Action onLoaded)
	{
		this.onLoaded = onLoaded;
		if (images.Count > 0)
		{
			Debug.LogGUI("LoadImages");
			if (loadImagesAnimator.IsPlaying)
			{
				Debug.LogErrorGUI("Stop LoadImages Animator");
				loadImagesAnimator.Stop();
			}
			imageHolder.gameObject.SetActive(value: false);
			container.SetActive(value: true);
			token = new LoadImageToken(_handles);
			token.Load(images);
			loadImagesAnimator.Play();
		}
		else
		{
			Hide();
		}
	}

	public void Show(string image, Action onShown = null)
	{
		if (currentImage == image || image.IsNullOrEmpty())
		{
			onShown?.Invoke();
			return;
		}
		token.Cancel(currentImage);
		currentImage = image;
		onShownImage = onShown;
		token.Get(image, delegate(Sprite sprite)
		{
			if (!(currentImage != image))
			{
				if (sprite == null)
				{
					onShown?.Invoke();
				}
				else
				{
					imageHolder.sprite = sprite;
					imageHolder.gameObject.SetActive(value: true);
					container.SetActive(value: true);
					if (showImageAnimator != null)
					{
						Debug.LogGUI("Show image " + currentImage);
						showImageAnimator.Play();
					}
					else
					{
						onShown?.Invoke();
					}
				}
			}
		});
	}

	public void Hide()
	{
		container?.SetActive(value: false);
		OnHidden();
	}

	private void OnDisable()
	{
		OnHidden();
	}

	private void OnHidden()
	{
		StopAllCoroutines();
		currentImage = null;
		loadImagesAnimator.Stop();
		showImageAnimator?.Stop();
		token?.Free();
	}

	private void WaitLoadImage()
	{
		Debug.LogGUI("WaitLoadImage");
		StartCoroutine(CheckLoaded());
	}

	private IEnumerator CheckLoaded()
	{
		yield return new WaitUntil(token.AreAllImagesLoaded);
		onLoaded?.Invoke();
	}
}
