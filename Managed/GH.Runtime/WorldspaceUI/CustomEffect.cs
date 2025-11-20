using SpriteMemoryManagement;
using UnityEngine;
using UnityEngine.UI;

namespace WorldspaceUI;

public class CustomEffect : MonoBehaviour
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	[SerializeField]
	private IEffectBlink blinkEffect;

	[SerializeField]
	private ImageSpriteLoader _imageSpriteLoader;

	public void ShowEffect(ReferenceToSprite icon, string titleTooltip = null, string descriptionTooltip = null, bool blink = false)
	{
		_imageSpriteLoader.AddReferenceToSpriteForImage(image, icon);
		if (titleTooltip.IsNOTNullOrEmpty() || descriptionTooltip.IsNOTNullOrEmpty())
		{
			tooltip.enabled = true;
			tooltip.SetText(titleTooltip, refreshTooltip: false, descriptionTooltip);
		}
		else
		{
			tooltip.enabled = false;
		}
		EnableBlink(blink);
		base.gameObject.SetActive(value: true);
	}

	public void EnableBlink(bool blink)
	{
		blinkEffect.enabled = blink;
	}

	public void Hide()
	{
		_imageSpriteLoader.Release();
		base.gameObject.SetActive(value: false);
	}
}
