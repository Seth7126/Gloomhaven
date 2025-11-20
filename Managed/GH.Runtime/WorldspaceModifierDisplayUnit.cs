using UnityEngine;
using UnityEngine.UI;

public class WorldspaceModifierDisplayUnit : MonoBehaviour
{
	public enum AnimationType
	{
		FadeOut,
		SlowFadeOut,
		MoveDownFadeOut,
		DiscardMod,
		MoveUpFadeOut,
		DirectDamage
	}

	public bool UnitAvailable = true;

	public Animator Anim;

	public Image ModifierImage;

	public Image BackgroundColourBlueImage;

	public Image BackgroundColourRedImage;

	public Image BackgroundColourAdvantageBlueImage;

	public Image BackgroundColourDisadvantageRedImage;

	public Text ValueTextLeft;

	public Text ValueTextRight;

	private static readonly int _speedMultiplier = Animator.StringToHash("SpeedMultiplier");

	public void DisplayValue(Sprite imageSprite, string valueText, int colour, bool turnOffText = false)
	{
		ResetModifierDisplayUnit();
		ModifierImage.sprite = imageSprite;
		ValueTextRight.text = valueText;
		if (turnOffText)
		{
			ValueTextRight.enabled = false;
		}
		switch (colour)
		{
		case 1:
			BackgroundColourBlueImage.enabled = true;
			break;
		case 2:
			BackgroundColourRedImage.enabled = true;
			break;
		case 3:
			BackgroundColourAdvantageBlueImage.enabled = true;
			break;
		case 4:
			BackgroundColourDisadvantageRedImage.enabled = true;
			break;
		}
	}

	private void ResetModifierDisplayUnit()
	{
		ValueTextRight.text = "";
		ValueTextLeft.text = "";
		BackgroundColourRedImage.enabled = false;
		BackgroundColourBlueImage.enabled = false;
		BackgroundColourAdvantageBlueImage.enabled = false;
		BackgroundColourDisadvantageRedImage.enabled = false;
	}

	public void TurnOffModifierDisplayUnit()
	{
		Object.Destroy(base.gameObject);
	}

	public void AnimateDisplayUnit(AnimationType anim, float animationSpeedMultiplier)
	{
		Anim.SetFloat(_speedMultiplier, animationSpeedMultiplier);
		Anim.SetTrigger(anim.ToString());
	}
}
