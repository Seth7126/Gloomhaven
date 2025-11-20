using MapRuleLibrary.MapState;
using UnityEngine;
using UnityEngine.UI;

public class UILocationPopup : MonoBehaviour
{
	[SerializeField]
	private TextLocalizedListener locationName;

	[SerializeField]
	private Image locationIcon;

	[SerializeField]
	private GUIAnimator showAnimator;

	public void Show(CQuestState quest)
	{
		locationName.SetTextKey(quest.Quest.LocalisedNameKey);
		locationIcon.sprite = UIInfoTools.Instance.GetQuestMarkerSprite(quest);
		showAnimator.Play(fromStart: true);
	}

	public void Hide()
	{
		showAnimator.Stop();
		showAnimator.GoInitState();
	}

	private void OnDisable()
	{
		showAnimator.Stop();
	}
}
