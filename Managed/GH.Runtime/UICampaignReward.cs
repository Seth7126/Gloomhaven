using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Utils.Extensions;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICampaignReward : UIReward
{
	[SerializeField]
	private List<Image> glows;

	[SerializeField]
	private Color positiveGlowColor;

	[SerializeField]
	protected GUIAnimator glowAnimator;

	[SerializeField]
	private string positiveRewardAudioItem = "PlaySound_UIPingRewardPositive";

	[SerializeField]
	private string negativeRewardAudioItem = "PlaySound_UIPingRewardNegative";

	[SerializeField]
	private TextMeshProUGUI extraAmountText;

	[SerializeField]
	private string itemRewardTextLoc;

	private Color? textColor;

	public override void ShowReward(Reward reward)
	{
		base.ShowReward(reward);
		switch (reward.Type)
		{
		case ETreasureType.PerkPoint:
			rewardInformation.text = string.Format("<font=\"MarcellusSC-Regular SDF\"><size=+2><cspace=1>{0}</cspace></size></font>", LocalizationManager.GetTranslation("GUI_QUEST_REWARD_PERKPOINT_Unlocked"), itemNameColor.ToHex());
			break;
		case ETreasureType.EnhancementSlots:
			rewardInformation.text = string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_EnchantmentPoints"), $"+{reward.Amount}");
			break;
		}
		if (reward.GiveToCharacterID.IsNOTNullOrEmpty() && reward.GiveToCharacterID != "party" && reward.GiveToCharacterID != "NoneID")
		{
			CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.First((CMapCharacter it) => it.CharacterID == reward.GiveToCharacterID);
			if (itemRewardTextLoc.IsNullOrEmpty())
			{
				rewardInformation.text = string.Format("<font=\"MarcellusSC-Regular SDF\"><size=+2><cspace=1>{0}</cspace></size></font>", cMapCharacter.CharacterName, itemNameColor.ToHex()) + " " + rewardInformation.text;
			}
			else
			{
				rewardInformation.text = string.Format(LocalizationManager.GetTranslation(itemRewardTextLoc), string.Format("<font=\"MarcellusSC-Regular SDF\"><size=+2><cspace=1>{0}</cspace></size></font>", cMapCharacter.CharacterName, itemNameColor.ToHex()), rewardInformation.text);
			}
		}
		SetupExtraAmount(reward);
		if (!textColor.HasValue)
		{
			textColor = rewardInformation.color;
		}
		if (reward.IsNegative())
		{
			for (int num = 0; num < glows.Count; num++)
			{
				glows[num].color = UIInfoTools.Instance.warningColor;
			}
			rewardInformation.color = UIInfoTools.Instance.warningColor;
			AudioControllerUtils.PlaySound(negativeRewardAudioItem);
		}
		else
		{
			for (int num2 = 0; num2 < glows.Count; num2++)
			{
				glows[num2].color = positiveGlowColor;
			}
			rewardInformation.color = textColor.Value;
			AudioControllerUtils.PlaySound(positiveRewardAudioItem);
		}
		glowAnimator?.Play(fromStart: true);
	}

	protected override string GenerateUnlockQuestDescription(CQuest quest)
	{
		if (LocalizationManager.TryGetTranslation(quest.LocalisedRewardKey, out var Translation))
		{
			return Translation;
		}
		return string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_Unlocked"), string.Format("<font=\"MarcellusSC-Regular SDF\"><size=+2><cspace=1>{0}</cspace></size></font>", LocalizationManager.GetTranslation(quest.LocalisedNameKey), itemNameColor.ToHex()));
	}

	private void SetupExtraAmount(Reward reward)
	{
		ETreasureType type = reward.Type;
		if (type == ETreasureType.Item || type == ETreasureType.UnlockProsperityItem)
		{
			if (reward.Amount > 1)
			{
				extraAmountText.text = $"x{reward.Amount}";
				extraAmountText.gameObject.SetActive(value: true);
			}
			else
			{
				extraAmountText.gameObject.SetActive(value: false);
			}
		}
		else
		{
			extraAmountText.gameObject.SetActive(value: false);
		}
	}

	protected override void OnDisable()
	{
		StopAnimations();
	}

	public virtual void StopAnimations()
	{
		glowAnimator?.Stop();
	}
}
