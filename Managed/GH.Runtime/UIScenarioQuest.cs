using GLOOM;
using MapRuleLibrary.MapState;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIScenarioQuest : MonoBehaviour
{
	[SerializeField]
	private GameObject questMarker;

	[SerializeField]
	private Image questIcon;

	[SerializeField]
	private TextMeshProUGUI questText;

	[SerializeField]
	private Image questHighlight;

	public void SetQuest(CQuestState questState)
	{
		questIcon.sprite = UIInfoTools.Instance.GetQuestMarkerSprite(questState);
		questHighlight.sprite = UIInfoTools.Instance.GetQuestMarkerHighlightSprite(questState);
		questText.text = LocalizationManager.GetTranslation(questState.Quest.LocalisedNameKey);
	}
}
