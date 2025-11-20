using GLOOM;
using SpriteMemoryManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class PassivePerkPopup : SlotPopup
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private GameObject footer;

	[SerializeField]
	private Image background;

	[SerializeField]
	public Sprite defaultBackground;

	[SerializeField]
	public Sprite footerBackground;

	[SerializeField]
	public Image duration;

	[FormerlySerializedAs("name")]
	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private ImageSpriteLoader _imageSpriteLoader;

	private ReferenceToSprite _referenceOnIcon;

	protected void OnDestroy()
	{
		_imageSpriteLoader.Release();
	}

	public void Init(Sprite sprite, string nameKey, string descriptionKey, Transform holder, bool showFooter, bool localizeKeys = true, bool showDuration = true)
	{
		Init(holder);
		footer.SetActive(showFooter);
		background.sprite = (showFooter ? footerBackground : defaultBackground);
		icon.sprite = sprite;
		description.text = (localizeKeys ? LocalizationManager.GetTranslation(descriptionKey) : descriptionKey);
		nameText.text = (localizeKeys ? LocalizationManager.GetTranslation(nameKey) : nameKey);
		duration.enabled = showDuration;
	}
}
