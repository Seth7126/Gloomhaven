using System.Collections.Generic;
using System.Threading.Tasks;
using AddressableMisc;
using SM.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SpriteMemoryManagement;

[RequireComponent(typeof(ImageAddressableLoader))]
public class ButtonSpritesAddressableLoader : MonoBehaviour
{
	private readonly Dictionary<Button, ButtonLoadingContext> _loadingContexts = new Dictionary<Button, ButtonLoadingContext>();

	private ImageAddressableLoader _imageLoader;

	private ImageAddressableLoader ImageAddressableLoader => _imageLoader ?? (_imageLoader = GetComponent<ImageAddressableLoader>());

	public async Task<LoadingAddressableState> AddReferenceToSprites(Button button, ReferenceToSprite imageRef, ReferenceToSpriteState stateRef)
	{
		if (!IsValidRefs(imageRef, stateRef))
		{
			LogUtils.LogError("[ButtonSpriteAddressableLoader] Lacks sprites for button: " + button.gameObject.name);
			return LoadingAddressableState.Failed;
		}
		if (!_loadingContexts.TryGetValue(button, out var value))
		{
			value = new ButtonLoadingContext(ImageAddressableLoader);
			_loadingContexts[button] = value;
		}
		return await value.LoadAsync(this, button, imageRef, stateRef);
	}

	public void Unload(Button button)
	{
		_loadingContexts[button].Unload();
	}

	public void UnloadAll()
	{
		foreach (KeyValuePair<Button, ButtonLoadingContext> loadingContext in _loadingContexts)
		{
			Unload(loadingContext.Key);
		}
		_loadingContexts.Clear();
	}

	public void Cancel(Button button)
	{
		_loadingContexts[button].CancelLoad();
	}

	public void CancelAll()
	{
		foreach (KeyValuePair<Button, ButtonLoadingContext> loadingContext in _loadingContexts)
		{
			loadingContext.Value.CancelLoad();
		}
		_loadingContexts.Clear();
	}

	private bool IsValidRefs(ReferenceToSprite imageRef, ReferenceToSpriteState stateRef)
	{
		if (imageRef == null && stateRef.HighlightedSprite == null && stateRef.DisabledSprite == null && stateRef.SelectedSprite == null)
		{
			return stateRef.PressedSprite != null;
		}
		return true;
	}
}
