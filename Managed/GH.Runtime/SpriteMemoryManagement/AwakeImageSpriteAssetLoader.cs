using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace SpriteMemoryManagement;

public class AwakeImageSpriteAssetLoader : MonoBehaviour
{
	[SerializeField]
	private AssetReferenceSprite _spriteAssetRef;

	[SerializeField]
	private Image _image;

	[SerializeField]
	private bool _alphaHideOnLoad = true;

	private async void Awake()
	{
		Color awakeColor = _image.color;
		if (_alphaHideOnLoad && !_spriteAssetRef.IsDone)
		{
			Color color = awakeColor;
			color.a = 0f;
			_image.color = color;
		}
		Image image = _image;
		image.sprite = await _spriteAssetRef.LoadAssetAsync().Task;
		_image.color = awakeColor;
	}

	private void OnDestroy()
	{
		_spriteAssetRef.ReleaseAsset();
	}
}
