using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAchievement : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI achievementName;

	[SerializeField]
	private TextMeshProUGUI achievemenDescription;

	[SerializeField]
	protected Image icon;

	[Header("Progress")]
	[SerializeField]
	private TextMeshProUGUI progressAmount;

	[SerializeField]
	protected UIProgressBar progressBar;

	protected CPartyAchievement partyAchievement;

	protected AchievementYMLData achievementYML;

	public string Name { get; private set; }

	public CPartyAchievement Achievement => partyAchievement;

	public virtual void SetAchievement(CPartyAchievement partyAchievement)
	{
		this.partyAchievement = partyAchievement;
		achievementYML = partyAchievement.Achievement;
		string text = (achievementName.text = CreateLayout.LocaliseText(achievementYML.LocalisedName));
		Name = text;
		achievemenDescription.text = CreateLayout.LocaliseText(achievementYML.LocalisedDescription);
		RefresState();
	}

	public virtual void RefresState()
	{
		icon.sprite = UIInfoTools.Instance.GetAchievementIcon(achievementYML.AchievementType, achievementYML.AchievementLevel, partyAchievement.State == EAchievementState.Completed || partyAchievement.State == EAchievementState.RewardsClaimed);
		if (partyAchievement.State != EAchievementState.RewardsClaimed)
		{
			UpdateProgress();
			progressBar.gameObject.SetActive(value: true);
		}
		else
		{
			progressBar.gameObject.SetActive(value: false);
		}
	}

	protected virtual void UpdateProgress()
	{
		if (Achievement.State != EAchievementState.Locked)
		{
			progressBar.SetAmount(Achievement.AchievementConditionState.TotalConditionsAndTargets, Achievement.AchievementConditionState.CurrentProgress);
		}
	}
}
