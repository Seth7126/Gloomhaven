#define ENABLE_LOGS
using System.Collections.Generic;
using System.Threading.Tasks;
using AddressableMisc;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class ImageAddressableLoader : MonoBehaviour
{
	[SerializeField]
	private List<CanvasGroup> _objectsToHideWhileLoad;

	private Dictionary<Image, ImageLoadingContext> _loadingContexts;

	private int _referenceCount;

	public int ReferenceCount
	{
		get
		{
			return _referenceCount;
		}
		private set
		{
			_referenceCount = value;
			Debug.Assert(ReferenceCount >= 0);
			RaiseReferenceCountChanged();
		}
	}

	[UsedImplicitly]
	private void Awake()
	{
		_loadingContexts = new Dictionary<Image, ImageLoadingContext>();
		ReferenceCount = 0;
	}

	public async Task<LoadingAddressableState> LoadAsync(object context, Image image, AssetReferenceSprite referenceSprite)
	{
		if (!referenceSprite.Exists())
		{
			Debug.LogError($"Reference sprite {referenceSprite} is not valid (null or missing)!");
			return LoadingAddressableState.Failed;
		}
		if (!_loadingContexts.TryGetValue(image, out var value))
		{
			value = new ImageLoadingContext();
			_loadingContexts.Add(image, value);
		}
		ChangeReferenceCount(1);
		LoadingAddressableState result = await value.LoadAsync(context, image, referenceSprite);
		ChangeReferenceCount(-1);
		return result;
	}

	private void RaiseReferenceCountChanged()
	{
		HandleObjectsToHide();
	}

	private void HandleObjectsToHide()
	{
		if (_objectsToHideWhileLoad == null)
		{
			return;
		}
		float alpha = ((ReferenceCount == 0) ? 1f : 0f);
		foreach (CanvasGroup item in _objectsToHideWhileLoad)
		{
			item.enabled = true;
			item.alpha = alpha;
		}
	}

	private void ChangeReferenceCount(int offset)
	{
		ReferenceCount += offset;
	}

	public void UnloadAll()
	{
		foreach (KeyValuePair<Image, ImageLoadingContext> loadingContext in _loadingContexts)
		{
			loadingContext.Value.Unload();
		}
		_loadingContexts.Clear();
	}

	public void Unload(Image image)
	{
		if (_loadingContexts != null)
		{
			if (_loadingContexts.TryGetValue(image, out var value))
			{
				value.Unload();
				image.sprite = null;
				_loadingContexts.Remove(image);
			}
			else
			{
				Debug.LogWarning("Can't unload a missing loading context of image " + image.name);
			}
		}
	}

	public void CancelLoadAll()
	{
		foreach (KeyValuePair<Image, ImageLoadingContext> loadingContext in _loadingContexts)
		{
			loadingContext.Value.CancelLoad();
		}
		_loadingContexts.Clear();
	}

	public void CancelLoad(Image image)
	{
		if (_loadingContexts.TryGetValue(image, out var value))
		{
			value.CancelLoad();
			image.sprite = null;
			_loadingContexts.Remove(image);
		}
		else
		{
			Debug.LogWarning("Can't cancel loading of missing loading context of image " + image.name);
		}
	}
}
