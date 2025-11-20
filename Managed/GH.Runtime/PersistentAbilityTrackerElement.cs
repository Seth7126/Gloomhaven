using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PersistentAbilityTrackerElement : MonoBehaviour
{
	[SerializeField]
	private Image circleImage;

	[SerializeField]
	private TextMeshProUGUI experienceText;

	[SerializeField]
	private Sprite xpIcon;

	[SerializeField]
	private Sprite shieldIcon;

	public void SetElementXP(int experience, bool isActive, bool isCompleted = false)
	{
		Color color = (isActive ? UIInfoTools.Instance.currentRoundColor : (isCompleted ? UIInfoTools.Instance.pastRoundColor : UIInfoTools.Instance.leftRoundColor));
		experienceText.text = experience.ToString();
		circleImage.color = color;
		circleImage.sprite = xpIcon;
	}

	public void SetElementShield(bool isActive, bool isCompleted = false)
	{
		Color color = (isActive ? UIInfoTools.Instance.currentRoundColor : (isCompleted ? UIInfoTools.Instance.pastRoundColor : UIInfoTools.Instance.leftRoundColor));
		experienceText.text = string.Empty;
		circleImage.color = color;
		circleImage.sprite = shieldIcon;
	}
}
