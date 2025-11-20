using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPersonalQuestObjectiveTrackerTarget : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI targetText;

	[SerializeField]
	private LayoutElement layoutElement;

	public void SetText(string text, Color color, float size)
	{
		targetText.text = text;
		targetText.color = color;
		layoutElement.preferredWidth = size;
	}
}
