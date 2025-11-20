using System;
using SpriteMemoryManagement;
using UnityEngine;
using UnityEngine.UI;

public class PassivePerkPreviewUI : MonoBehaviour
{
	[SerializeField]
	private PassivePerkPopup descriptionPopup;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Image durationImage;

	private Action onCancel;

	[SerializeField]
	private ImageSpriteLoader _imageSpriteLoader;

	private Button button;

	private ReferenceToSprite _referenceForIcon;

	public Selectable Selectable => button;

	private void Awake()
	{
		button = GetComponent<Button>();
		button.onClick.AddListener(delegate
		{
			onCancel?.Invoke();
		});
	}

	private void OnDestroy()
	{
		button.onClick.RemoveAllListeners();
		_imageSpriteLoader.Release();
	}

	public void SetPassive(ReferenceToSprite image, string nameKey, string descriptionKey, Transform holder, Action onCancel = null, bool localizeKeys = true, bool showDuration = true)
	{
		this.onCancel = onCancel;
		ToggleDisplayBonus(active: false);
		if (_referenceForIcon == null || !_referenceForIcon.Equals(image))
		{
			_referenceForIcon = image;
			_imageSpriteLoader.AddReferenceToSpriteForImage(iconImage, _referenceForIcon, delegate(Sprite sprite)
			{
				descriptionPopup.Init(sprite, nameKey, descriptionKey, holder, onCancel != null, localizeKeys, showDuration);
			});
		}
		durationImage.enabled = showDuration;
	}

	public void ToggleDisplayBonus(bool active)
	{
		if (active)
		{
			descriptionPopup.Show();
		}
		else
		{
			descriptionPopup.Hide();
		}
	}

	public void SetInteractable(bool interactable)
	{
		button.interactable = interactable && onCancel != null;
	}
}
