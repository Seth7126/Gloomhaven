using TMPro;
using UnityEngine;

public class UIScenarioAccomplishmentRowValue : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI valueText;

	[SerializeField]
	private GameObject maxIndicator;

	[SerializeField]
	private Color maxValueColor;

	[SerializeField]
	private Color defaultColor;

	public void SetValue(string value, bool isMax)
	{
		valueText.text = value;
		maxIndicator.SetActive(isMax);
		valueText.color = (isMax ? maxValueColor : defaultColor);
	}
}
