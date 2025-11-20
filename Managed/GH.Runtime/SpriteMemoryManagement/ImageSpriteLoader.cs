#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using SM.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SpriteMemoryManagement;

public class ImageSpriteLoader : MonoBehaviour
{
	private class ImageSpriteLoaderRequest : IDisposable
	{
		private Dictionary<Image, Action<Sprite>> _images;

		public ReferenceToSprite ReferenceToSprite { get; private set; }

		public bool IsLoaded => ReferenceToSprite.IsLoaded();

		public Sprite Result => ReferenceToSprite.GetSprite();

		public bool IsDisposed { get; private set; }

		public ImageSpriteLoaderRequest(ReferenceToSprite referenceToSprite, Image image, Action<Sprite> onFinish, CancellationToken token)
		{
			ReferenceToSprite = referenceToSprite;
			_images = new Dictionary<Image, Action<Sprite>> { { image, onFinish } };
			ReferenceToSprite.GetAsyncSprite(OnCompleted, token);
		}

		private void OnCompleted(Sprite sprite, AsyncLoadingState state)
		{
			if (state != AsyncLoadingState.Finished)
			{
				return;
			}
			foreach (KeyValuePair<Image, Action<Sprite>> image in _images)
			{
				image.Key.sprite = sprite;
				image.Value?.Invoke(sprite);
			}
		}

		public void Dispose()
		{
			IsDisposed = true;
			ReferenceToSprite.Release();
			_images.Clear();
		}

		public bool Contains(Image image)
		{
			return _images.ContainsKey(image);
		}

		public void Add(Image image, Action<Sprite> onFinish)
		{
			_images.Add(image, onFinish);
			if (IsLoaded)
			{
				image.sprite = Result;
				onFinish?.Invoke(Result);
			}
		}

		public void Remove(Image image)
		{
			_images.Remove(image);
			if (_images.Count <= 0)
			{
				Dispose();
			}
		}
	}

	[FormerlySerializedAs("Wait for load")]
	[SerializeField]
	private bool _waitForLoad;

	[FormerlySerializedAs("circularLoader")]
	[SerializeField]
	private GameObject _circularLoader;

	[SerializeField]
	private List<CanvasGroup> _objectsToHideWhileLoad;

	private Dictionary<ReferenceToSprite, ImageSpriteLoaderRequest> _activeRequests;

	private CancellationTokenSource _spriteLoadCancellationTokenSource;

	public bool WaitForLoad => _waitForLoad;

	[UsedImplicitly]
	private void Awake()
	{
		if (_circularLoader != null)
		{
			_circularLoader.SetActive(value: false);
		}
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Release();
	}

	private void OnAllSpritesLoadedSelf()
	{
		if (_circularLoader != null)
		{
			_circularLoader.SetActive(value: false);
		}
		ToggleObjectsWhileRepaint(isActive: true);
	}

	private void ToggleObjectsWhileRepaint(bool isActive)
	{
		if (_objectsToHideWhileLoad == null)
		{
			return;
		}
		float alpha = (isActive ? 1f : 0f);
		foreach (CanvasGroup item in _objectsToHideWhileLoad)
		{
			item.enabled = true;
			item.alpha = alpha;
		}
	}

	private void EnsureCancellationTokenSourceCreated()
	{
		if (_spriteLoadCancellationTokenSource == null)
		{
			_spriteLoadCancellationTokenSource = new CancellationTokenSource();
		}
	}

	public void AddReferenceToSpriteForImage(Image image, ReferenceToSprite reference, Action<Sprite> onFinish = null)
	{
		EnsureCancellationTokenSourceCreated();
		if (reference == null)
		{
			LogUtils.LogError("[ImageSpriteLoader] Sprite reference is not null " + image.gameObject.name);
			return;
		}
		if (_activeRequests == null)
		{
			_activeRequests = new Dictionary<ReferenceToSprite, ImageSpriteLoaderRequest>();
		}
		ImageSpriteLoaderRequest imageSpriteLoaderRequest = _activeRequests.Values.FirstOrDefault((ImageSpriteLoaderRequest req) => req.Contains(image));
		if (imageSpriteLoaderRequest != null)
		{
			if (!imageSpriteLoaderRequest.ReferenceToSprite.Equals(reference))
			{
				imageSpriteLoaderRequest.Remove(image);
				if (imageSpriteLoaderRequest.IsDisposed)
				{
					_activeRequests.Remove(imageSpriteLoaderRequest.ReferenceToSprite);
				}
				CreateRequest(reference, image, delegate(Sprite sprite)
				{
					OnComplete(image, sprite, onFinish);
				});
			}
			else if (!imageSpriteLoaderRequest.IsLoaded)
			{
				Debug.LogWarning("[ImageSpriteLoader] Sprite for this image is already in process");
			}
		}
		else
		{
			CreateRequest(reference, image, delegate(Sprite sprite)
			{
				OnComplete(image, sprite, onFinish);
			});
		}
	}

	private void OnComplete(Image image, Sprite sprite, Action<Sprite> onFinish)
	{
		if (_activeRequests != null)
		{
			onFinish?.Invoke(sprite);
			if (!WaitForLoad)
			{
				image.enabled = true;
			}
			if (_activeRequests.Values.All((ImageSpriteLoaderRequest x) => x.IsLoaded))
			{
				OnAllSpritesLoadedSelf();
			}
		}
	}

	private void CreateRequest(ReferenceToSprite reference, Image image, Action<Sprite> onFinish)
	{
		if (_circularLoader != null)
		{
			_circularLoader.SetActive(value: true);
		}
		ToggleObjectsWhileRepaint(isActive: false);
		if (!WaitForLoad)
		{
			image.enabled = false;
		}
		if (_activeRequests.TryGetValue(reference, out var value))
		{
			value.Add(image, onFinish);
		}
		else
		{
			_activeRequests.Add(reference, new ImageSpriteLoaderRequest(reference, image, onFinish, _spriteLoadCancellationTokenSource.Token));
		}
	}

	public void CancelLoad()
	{
		if (_spriteLoadCancellationTokenSource != null)
		{
			_spriteLoadCancellationTokenSource.Cancel();
			_spriteLoadCancellationTokenSource.Dispose();
		}
		_spriteLoadCancellationTokenSource = new CancellationTokenSource();
	}

	public void Release()
	{
		if (_activeRequests == null)
		{
			return;
		}
		foreach (ImageSpriteLoaderRequest value in _activeRequests.Values)
		{
			value.Dispose();
		}
		_activeRequests.Clear();
		_activeRequests = null;
	}

	public void Release(Image image)
	{
		if (_activeRequests != null)
		{
			ImageSpriteLoaderRequest imageSpriteLoaderRequest = _activeRequests.Values.FirstOrDefault((ImageSpriteLoaderRequest req) => req.Contains(image));
			imageSpriteLoaderRequest?.Remove(image);
			if (imageSpriteLoaderRequest != null && imageSpriteLoaderRequest.IsDisposed)
			{
				_activeRequests.Remove(imageSpriteLoaderRequest.ReferenceToSprite);
			}
		}
	}
}
