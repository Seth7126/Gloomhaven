using System.Threading;
using System.Threading.Tasks;
using AddressableMisc;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class ImageLoadingContext
{
	private CancellationTokenSource _spriteLoadCancellationTokenSource;

	private object _lastAddressableContext;

	private LoadingAddressableState _lastState;

	private Image _image;

	private Sprite _sprite;

	private AssetReferenceSprite _assetReferenceSprite;

	public async Task<LoadingAddressableState> LoadAsync(object context, Image image, AssetReferenceSprite referenceSprite)
	{
		if (_lastAddressableContext == context && _image == image && _assetReferenceSprite.RuntimeKey == referenceSprite.RuntimeKey && _lastState == LoadingAddressableState.FinishedSuccessfully)
		{
			_image.enabled = true;
			_image.sprite = _sprite;
			return _lastState;
		}
		Unload();
		_lastAddressableContext = context;
		image.enabled = false;
		LoadingAddressableState result = (_lastState = await image.LoadSpriteAsyncAddressable(_lastAddressableContext, referenceSprite, _spriteLoadCancellationTokenSource.Token));
		_image = image;
		_assetReferenceSprite = referenceSprite;
		_sprite = image.sprite;
		image.enabled = true;
		return result;
	}

	public void Unload()
	{
		CancelLoad();
		AddressableLoaderHelper.UnloadAssetsByContext(_lastAddressableContext);
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

	private void EnsureCancellationTokenSourceCreated()
	{
		if (_spriteLoadCancellationTokenSource == null)
		{
			_spriteLoadCancellationTokenSource = new CancellationTokenSource();
		}
	}
}
