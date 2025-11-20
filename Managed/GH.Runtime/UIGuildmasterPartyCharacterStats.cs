#define ENABLE_LOGS
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIGuildmasterPartyCharacterStats : UICustomPartyCharacterStats
{
	public Button levelUpButton;

	[SerializeField]
	protected GameObject cardsButton;

	public UnityEvent OnClickLevelUp => levelUpButton.onClick;

	public override void EnableUIForLevelingUp()
	{
		if (character != null)
		{
			base.EnableUIForLevelingUp();
			levelUpButton.gameObject.SetActive(value: true);
			cardsButton.SetActive(value: false);
		}
		else
		{
			Debug.Log("Trying to activate Level up button but characterData is null.");
		}
	}

	public override void EnableNavigation()
	{
		base.EnableNavigation();
		levelUpButton.SetNavigation(Navigation.Mode.Vertical, wrapAround: true);
	}

	public override void DisableNavigation()
	{
		base.DisableNavigation();
		levelUpButton.DisableNavigation();
	}

	public override void DisableUIForLevelingUp()
	{
		base.DisableUIForLevelingUp();
		levelUpButton.gameObject.SetActive(value: false);
		cardsButton.SetActive(value: true);
	}
}
