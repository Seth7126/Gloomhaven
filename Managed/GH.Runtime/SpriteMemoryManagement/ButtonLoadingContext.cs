using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AddressableMisc;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace SpriteMemoryManagement;

public class ButtonLoadingContext
{
	private readonly ImageAddressableLoader _imageAddressableLoader;

	private CancellationTokenSource _spriteLoadCancellationTokenSource;

	private object _lastAddressableContext;

	private Image _lastImage;

	private SpriteState _spriteState;

	private Button _button;

	private ReferenceToSpriteState _stateRef;

	private LoadingAddressableState _lastState;

	private List<Task<LoadingAddressableState>> _tasks = new List<Task<LoadingAddressableState>>();

	private LoadingAddressableState[] _taskResults = new LoadingAddressableState[4];

	public ButtonLoadingContext(ImageAddressableLoader imageAddressableLoader)
	{
		_imageAddressableLoader = imageAddressableLoader;
	}

	public async Task<LoadingAddressableState> LoadAsync(object context, Button button, ReferenceToSprite imageRef, ReferenceToSpriteState stateRef)
	{
		if (_lastAddressableContext == context && _lastState == LoadingAddressableState.FinishedSuccessfully && _button == button && _stateRef.Equals(stateRef))
		{
			_button.spriteState = _spriteState;
			return _lastState;
		}
		Unload();
		_lastImage = button.image;
		_spriteState = button.spriteState;
		_lastAddressableContext = context;
		_tasks.Clear();
		if (!imageRef.InitializedWithSpecialSprite)
		{
			_tasks.Add(LoadBackgroundAsync(context, button.image, imageRef.SpriteReference));
		}
		_tasks.Add(Load(stateRef.DisabledSprite, delegate(Sprite sprite)
		{
			_spriteState.disabledSprite = sprite;
		}));
		_tasks.Add(Load(stateRef.HighlightedSprite, delegate(Sprite sprite)
		{
			_spriteState.highlightedSprite = sprite;
		}));
		_tasks.Add(Load(stateRef.PressedSprite, delegate(Sprite sprite)
		{
			_spriteState.pressedSprite = sprite;
		}));
		_tasks.Add(Load(stateRef.SelectedSprite, delegate(Sprite sprite)
		{
			_spriteState.selectedSprite = sprite;
		}));
		try
		{
			_taskResults = await Task.WhenAll(_tasks);
		}
		catch (AggregateException)
		{
			_lastState = LoadingAddressableState.Failed;
			return _lastState;
		}
		LoadingAddressableState[] taskResults = _taskResults;
		foreach (LoadingAddressableState loadingAddressableState in taskResults)
		{
			if (loadingAddressableState != LoadingAddressableState.FinishedSuccessfully)
			{
				_lastState = loadingAddressableState;
				return _lastState;
			}
		}
		_lastState = LoadingAddressableState.FinishedSuccessfully;
		_button = button;
		_stateRef = stateRef;
		button.spriteState = _spriteState;
		return _lastState;
	}

	private async Task<LoadingAddressableState> LoadBackgroundAsync(object context, Image image, AssetReferenceSprite assetReferenceSprite)
	{
		return await _imageAddressableLoader.LoadAsync(context, image, assetReferenceSprite);
	}

	private async Task<LoadingAddressableState> Load(ReferenceToSprite referenceToSprite, Action<Sprite> setAction)
	{
		var (obj, loadingAddressableState) = await UIExtensions.LoadSpriteAsync(_lastAddressableContext, referenceToSprite.SpriteReference, _spriteLoadCancellationTokenSource.Token);
		if (loadingAddressableState == LoadingAddressableState.FinishedSuccessfully)
		{
			setAction(obj);
		}
		return loadingAddressableState;
	}

	public void Unload()
	{
		CancelLoad();
		if (_imageAddressableLoader != null && _lastImage != null)
		{
			_imageAddressableLoader.Unload(_lastImage);
			_lastImage = null;
		}
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
}
