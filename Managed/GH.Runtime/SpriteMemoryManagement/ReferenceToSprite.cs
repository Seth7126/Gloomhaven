#define ENABLE_LOGS
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SpriteMemoryManagement;

[Serializable]
public class ReferenceToSprite
{
	private class SpriteLoadRequest : IDisposable, IEquatable<ReferenceToSprite>
	{
		private Action<Sprite> _onCompleted;

		private readonly Action _onFailed;

		private AsyncOperationHandle<Sprite> _handler;

		private readonly string _assetGuid;

		public AsyncOperationStatus Status { get; private set; }

		public Sprite Result => _handler.Result;

		public SpriteLoadRequest(AssetReferenceSprite spriteReference, Action<Sprite> onCompleted, Action onFailed = null)
		{
			_onCompleted = onCompleted;
			_onFailed = onFailed;
			_assetGuid = spriteReference.AssetGUID;
			_handler = Addressables.LoadAssetAsync<Sprite>(spriteReference);
			_handler.Completed += OnCompleteOperation;
		}

		private void OnCompleteOperation(AsyncOperationHandle<Sprite> handler)
		{
			if (_onCompleted == null && handler.IsValid())
			{
				Addressables.Release(handler);
			}
			else if (_onCompleted != null)
			{
				if (handler.Status == AsyncOperationStatus.Failed)
				{
					Debug.LogError("[SpriteLoadRequest] Loading failed");
					_onFailed?.Invoke();
					Status = AsyncOperationStatus.Failed;
				}
				else
				{
					_handler = handler;
					Status = AsyncOperationStatus.Succeeded;
					_onCompleted(_handler.Result);
				}
			}
			else
			{
				Debug.LogError("[SpriteLoadRequest] Not correct condition");
			}
		}

		public void Dispose()
		{
			_onCompleted = null;
			if (_handler.IsValid())
			{
				Addressables.Release(_handler);
			}
		}

		public bool Equals(ReferenceToSprite other)
		{
			return _assetGuid.Equals(other?.SpriteReference.AssetGUID);
		}
	}

	[SerializeField]
	private string _spriteName;

	[SerializeField]
	private AssetReferenceSprite _spriteReference;

	private Sprite _specialSprite;

	private SpriteLoadRequest _loadRequest;

	public Sprite SpecialSprite => _specialSprite;

	public bool InitializedWithSpecialSprite => _specialSprite != null;

	public AssetReferenceSprite SpriteReference => _spriteReference;

	public string SpriteName => GetNameSprite();

	public ReferenceToSprite()
	{
	}

	public ReferenceToSprite(Sprite sprite)
	{
		SetSpriteInsteadAddressable(sprite);
	}

	public void SetSpriteInsteadAddressable(Sprite sprite)
	{
		_specialSprite = sprite;
	}

	public bool IsLoaded()
	{
		if (_specialSprite != null || _loadRequest.Status == AsyncOperationStatus.Succeeded)
		{
			return true;
		}
		return false;
	}

	public bool IsHaveSprite()
	{
		if ((bool)_specialSprite || !string.IsNullOrEmpty(_spriteReference.AssetGUID))
		{
			return true;
		}
		return false;
	}

	public Sprite GetSprite()
	{
		if ((bool)_specialSprite)
		{
			return _specialSprite;
		}
		return _loadRequest?.Result;
	}

	public void GetAsyncSprite(Action<Sprite, AsyncLoadingState> action, CancellationToken cancellationToken)
	{
		if ((bool)_specialSprite)
		{
			action(_specialSprite, AsyncLoadingState.Finished);
			return;
		}
		if (_spriteReference == null)
		{
			Debug.LogError("Sprite reference is null");
			return;
		}
		if (string.IsNullOrEmpty(_spriteReference.AssetGUID))
		{
			Debug.LogError("GUID is empty!");
			action(null, AsyncLoadingState.Failed);
			return;
		}
		Release();
		_loadRequest = new SpriteLoadRequest(_spriteReference, delegate(Sprite sprite)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				action(null, AsyncLoadingState.Canceled);
				Release();
			}
			else
			{
				action(sprite, AsyncLoadingState.Finished);
			}
		}, delegate
		{
			Debug.LogWarning("Sprite: " + GetNameSprite() + " | " + _spriteReference?.AssetGUID + " | " + _spriteName + " not loaded.");
			action(null, AsyncLoadingState.Failed);
		});
	}

	public void GetAsyncSprite(Action<Sprite, AsyncLoadingState> action)
	{
		GetAsyncSprite(action, CancellationToken.None);
	}

	public void Release()
	{
		Debug.Log("[ReferenceToSprite][" + GetNameSprite() + "] Release");
		_loadRequest?.Dispose();
	}

	private string GetNameSprite()
	{
		if ((bool)_specialSprite)
		{
			return _specialSprite.name;
		}
		return _spriteName;
	}

	public bool Equals(ReferenceToSprite other)
	{
		if (_spriteReference != null && other._spriteReference != null)
		{
			return _spriteReference.AssetGUID == other._spriteReference.AssetGUID;
		}
		return _specialSprite == other._specialSprite;
	}
}
