using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Utils.Extensions;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.State;
using MapRuleLibrary.YML.Achievements;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SpriteMemoryManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIReward : MonoBehaviour
{
	[SerializeField]
	protected Image rewardIcon;

	[SerializeField]
	protected Image rewardSubIcon;

	[SerializeField]
	protected TextMeshProUGUI rewardInformation;

	[SerializeField]
	protected UIItemCardTooltipTarget itemTooltip;

	[SerializeField]
	protected Color itemNameColor;

	[SerializeField]
	protected ImageSpriteLoader _imageSpriteLoader;

	private ReferenceToSprite _referenceOnReward;

	protected const string HIGHLIGHT_FORMAT = "<font=\"MarcellusSC-Regular SDF\"><size=+2><cspace=1>{0}</cspace></size></font>";

	protected Button button;

	public Selectable Selectable => button;

	public bool IsInteractable
	{
		get
		{
			if (button != null)
			{
				return button.interactable;
			}
			return false;
		}
	}

	protected virtual void Awake()
	{
		button = GetComponent<Button>();
		if (_imageSpriteLoader != null)
		{
			_imageSpriteLoader = base.gameObject.AddComponent<ImageSpriteLoader>();
		}
	}

	public virtual void ShowReward(Reward reward)
	{
		if (button != null)
		{
			button.interactable = false;
		}
		if (itemTooltip != null)
		{
			itemTooltip.enabled = false;
		}
		CItem cItem = null;
		CQuest cQuest = null;
		AchievementYMLData achievementYMLData = null;
		rewardIcon.transform.SetAsFirstSibling();
		rewardIcon.transform.localScale = Vector3.one;
		switch (reward.Type)
		{
		case ETreasureType.XP:
		{
			int num2 = ((reward.TreasureDistributionType == ETreasureDistributionType.PerMercenaryInParty) ? (reward.Amount * Mathf.Max(1, AdventureState.MapState.MapParty.SelectedCharacters.Count())) : reward.Amount);
			num2 = reward.Amount;
			rewardInformation.text = $"{num2} {LocalizationManager.GetTranslation(reward.Type.ToString())}";
			break;
		}
		case ETreasureType.Gold:
		{
			int num = ((reward.TreasureDistributionType == ETreasureDistributionType.PerMercenaryInParty && AdventureState.MapState.GoldMode == EGoldMode.PartyGold) ? (reward.Amount * Mathf.Max(1, AdventureState.MapState.MapParty.SelectedCharacters.Count())) : reward.Amount);
			if (reward.TreasureDistributionRestrictionType == ETreasureDistributionRestrictionType.AllAmount && reward.TreasureDistributionType == ETreasureDistributionType.Combined)
			{
				rewardInformation.text = ((num > 0) ? string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_GAIN_GOLDBAG"), num) : string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_LOSE_GOLDBAG"), -num));
			}
			else if (reward.TreasureDistributionRestrictionType == ETreasureDistributionRestrictionType.ExcludePreviousSelectedCharacter && reward.TreasureDistributionType == ETreasureDistributionType.Combined)
			{
				rewardInformation.text = ((num > 0) ? string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_GAIN_EXCLUDEPREVIOUSCHARACTER"), num) : string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_LOSE_EXCLUDEPREVIOUSCHARACTER"), -num));
			}
			else if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold && reward.TreasureDistributionType != ETreasureDistributionType.None && reward.GiveToCharacterID.IsNullOrEmpty())
			{
				rewardInformation.text = $"{num} {LocalizationManager.GetTranslation($"{reward.Type}_{reward.TreasureDistributionType}")}";
			}
			else
			{
				rewardInformation.text = $"{num} {LocalizationManager.GetTranslation(reward.Type.ToString())}";
			}
			break;
		}
		case ETreasureType.ItemStock:
		case ETreasureType.UnlockProsperityItemStock:
			cItem = reward.Item;
			rewardInformation.text = string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_ItemStock"), $"<font=\"MarcellusSC-Regular SDF\"><size=+2><cspace=1>{LocalizationManager.GetTranslation(cItem.Name)}</cspace></size></font>");
			if (itemTooltip != null)
			{
				itemTooltip.Initialize(cItem);
				itemTooltip.enabled = true;
				if (button != null)
				{
					button.interactable = true;
				}
			}
			break;
		case ETreasureType.Item:
		case ETreasureType.UnlockProsperityItem:
			cItem = reward.Item;
			rewardInformation.text = $"<font=\"MarcellusSC-Regular SDF\"><size=+2><cspace=1>{LocalizationManager.GetTranslation(cItem.Name)}</cspace></size></font>";
			if (itemTooltip != null)
			{
				itemTooltip.Initialize(cItem);
				itemTooltip.enabled = true;
				if (button != null)
				{
					button.interactable = true;
				}
			}
			break;
		case ETreasureType.Enhancement:
			rewardInformation.text = LocalizationManager.GetTranslation($"ENHANCEMENT_{reward.Enhancement}");
			break;
		case ETreasureType.Prosperity:
			rewardInformation.text = ((reward.Amount < 0) ? ("<color=#" + UIInfoTools.Instance.warningColor.ToHex() + ">" + string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_Prosperity"), reward.Amount) + "</color>") : string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_Prosperity"), $"+{reward.Amount}"));
			break;
		case ETreasureType.Reputation:
			if (reward.Amount < 0)
			{
				rewardIcon.transform.localScale = new Vector3(1f, -1f, 1f);
				rewardInformation.text = "<color=#" + UIInfoTools.Instance.warningColor.ToHex() + ">" + string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_Reputation"), reward.Amount) + "</color>";
			}
			else
			{
				rewardInformation.text = string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_Reputation"), $"+{reward.Amount}");
			}
			break;
		case ETreasureType.UnlockLocation:
			rewardInformation.text = string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_Unlock"), LocalizationManager.GetTranslation(reward.UnlockName));
			break;
		case ETreasureType.UnlockChapter:
			rewardInformation.text = string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_UnlockChapter"), $"{reward.Amount}.{reward.SubChapter}");
			break;
		case ETreasureType.UnlockCharacter:
			rewardInformation.text = LocalizationManager.GetTranslation("GUI_QUEST_REWARD_UnlockCharacter");
			rewardIcon.transform.SetAsLastSibling();
			break;
		case ETreasureType.UnlockQuest:
			cQuest = MapRuleLibraryClient.MRLYML.Quests.FirstOrDefault((CQuest s) => s.ID == reward.UnlockName);
			rewardInformation.text = ((cQuest != null) ? GenerateUnlockQuestDescription(cQuest) : LocalizationManager.GetTranslation("GUI_QUEST_REWARD_UnlockQuest"));
			break;
		case ETreasureType.UnlockAchievement:
			achievementYMLData = MapRuleLibraryClient.MRLYML.Achievements.FirstOrDefault((AchievementYMLData s) => s.ID == reward.UnlockName);
			if (achievementYMLData != null)
			{
				if (LocalizationManager.TryGetTranslation(achievementYMLData.LocalisedName, out var Translation))
				{
					rewardInformation.text = Translation;
				}
				else
				{
					rewardInformation.text = ((achievementYMLData.AchievementType == EAchievementType.Party) ? LocalizationManager.GetTranslation("GUI_QUEST_REWARD_PartyAchievement") : LocalizationManager.GetTranslation("GUI_QUEST_REWARD_GlobalAchievement"));
				}
			}
			else
			{
				rewardInformation.text = LocalizationManager.GetTranslation("GUI_QUEST_REWARD_PartyAchievement");
			}
			break;
		case ETreasureType.CityEvent:
			rewardInformation.text = LocalizationManager.GetTranslation("GUI_QUEST_REWARD_CityEvent");
			break;
		case ETreasureType.RoadEvent:
			rewardInformation.text = LocalizationManager.GetTranslation("GUI_QUEST_REWARD_RoadEvent");
			break;
		case ETreasureType.UnlockMultiplayer:
			rewardInformation.text = string.Format("<font=\"MarcellusSC-Regular SDF\"><size=+2><cspace=1>{0}</cspace></size></font>", LocalizationManager.GetTranslation("GUI_QUEST_REWARD_Multiplayer"));
			break;
		case ETreasureType.Damage:
			rewardInformation.text = string.Format(LocalizationManager.GetTranslation((SaveData.Instance.Global.CurrentGameState == EGameState.Scenario) ? "GUI_GIFT_DAMAGE_NOW" : "GUI_GIFT_DAMAGE"), reward.Amount);
			break;
		case ETreasureType.Condition:
			rewardInformation.text = GenerateDescriptionCondition(reward.Conditions.SingleOrDefault(), forEnemy: false, reward.TreasureDistributionType == ETreasureDistributionType.Combined);
			break;
		case ETreasureType.EnemyCondition:
			rewardInformation.text = GenerateDescriptionCondition(reward.EnemyConditions.SingleOrDefault(), forEnemy: true, combined: false);
			break;
		case ETreasureType.Infuse:
			rewardInformation.text = string.Format(LocalizationManager.GetTranslation("GUI_GIFT_INFUSION"), reward.Infusions.SingleOrDefault());
			break;
		case ETreasureType.AddModifiers:
			rewardInformation.text = string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_AddModifiers"), string.Join(", ", reward.Modifiers.Select((KeyValuePair<string, int> it) => $"{it.Value} {UIInfoTools.Instance.GetAttackModifierText(ScenarioRuleClient.SRLYML.AttackModifiers.First((AttackModifierYMLData s) => s.Name == it.Key))}")));
			break;
		case ETreasureType.LoseItem:
			cItem = reward.Item;
			rewardInformation.text = string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_LoseItem"), $"<font=\"MarcellusSC-Regular SDF\"><size=+2><cspace=1>{LocalizationManager.GetTranslation(cItem.Name)}</cspace></size></font>");
			if (itemTooltip != null)
			{
				itemTooltip.Initialize(cItem);
				itemTooltip.enabled = true;
			}
			break;
		case ETreasureType.ConsumeItem:
			rewardInformation.text = string.Format(LocalizationManager.GetTranslation((reward.TreasureDistributionType == ETreasureDistributionType.Combined) ? "GUI_QUEST_REWARD_ConsumeItem_Combined" : "GUI_QUEST_REWARD_ConsumeItem"), "<sprite name=\"Inv" + reward.ConsumeSlot + "\" color=#" + UIInfoTools.Instance.warningColor.ToHex() + "> " + LocalizationManager.GetTranslation("GUI_ITEM_SLOT_" + reward.ConsumeSlot));
			break;
		default:
			rewardInformation.text = string.Format(LocalizationManager.GetTranslation($"GUI_QUEST_REWARD_{reward.Type}"), reward.Amount);
			break;
		}
		if (reward.Type == ETreasureType.UnlockQuest && cQuest != null)
		{
			QuestTypeConfigUI questTypeConfig = UIInfoTools.Instance.GetQuestTypeConfig(cQuest);
			rewardIcon.color = (AdventureState.MapState.IsCampaign ? UIInfoTools.Instance.White : questTypeConfig.color);
			_referenceOnReward = new ReferenceToSprite(questTypeConfig.GetRewardIcon(!AdventureState.MapState.IsCampaign));
			rewardSubIcon.gameObject.SetActive(value: false);
		}
		else
		{
			CharacterYMLData owner = GetOwner(reward);
			if (owner != null)
			{
				rewardIcon.color = UIInfoTools.Instance.GetRewardColor(reward);
				_referenceOnReward = UIInfoTools.Instance.GetRewardIcon(reward);
				rewardSubIcon.sprite = UIInfoTools.Instance.GetCharacterMarker(owner.Model, owner.CustomCharacterConfig);
				rewardSubIcon.gameObject.SetActive(value: true);
			}
			else
			{
				rewardIcon.color = UIInfoTools.Instance.GetRewardColor(reward);
				_referenceOnReward = UIInfoTools.Instance.GetRewardIcon(reward);
				rewardSubIcon.gameObject.SetActive(value: false);
			}
		}
		_imageSpriteLoader.AddReferenceToSpriteForImage(rewardIcon, _referenceOnReward);
	}

	private CharacterYMLData GetOwner(Reward reward)
	{
		string characterId = null;
		if (reward.GiveToCharacterID.IsNOTNullOrEmpty() && reward.GiveToCharacterID != "party" && reward.GiveToCharacterID != "NoneID")
		{
			characterId = reward.GiveToCharacterID;
		}
		else if (reward.Item != null && !reward.Item.YMLData.ValidEquipCharacterClassIDs.IsNullOrEmpty())
		{
			characterId = reward.Item.YMLData.ValidEquipCharacterClassIDs.FirstOrDefault();
		}
		if (!characterId.IsNullOrEmpty())
		{
			return ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData s) => s.ID == characterId);
		}
		return null;
	}

	protected virtual string GenerateUnlockQuestDescription(CQuest quest)
	{
		if (LocalizationManager.TryGetTranslation(quest.LocalisedRewardKey, out var Translation))
		{
			return Translation;
		}
		if (quest.Type != EQuestType.Relic)
		{
			return LocalizationManager.GetTranslation("GUI_QUEST_REWARD_UnlockQuest");
		}
		return LocalizationManager.GetTranslation("GUI_QUEST_REWARD_UnlockRelicQuest");
	}

	private string GenerateDescriptionCondition(RewardCondition condition, bool forEnemy, bool combined)
	{
		string text = LocalizationManager.GetTranslation("GUI_GIFT_" + (forEnemy ? "ENEMY_" : string.Empty) + condition.ToString().ToUpper() + (combined ? "_COMBINED" : string.Empty));
		if (condition.RoundDuration > 1)
		{
			text += string.Format(LocalizationManager.GetTranslation("GUI_GIFT_SEVERAL_ROUNDS_DURATION"), condition.RoundDuration);
		}
		else if (condition.RoundDuration == 0)
		{
			text += LocalizationManager.GetTranslation("GUI_GIFT_ROUNDS_DURATION");
		}
		return text;
	}

	protected virtual void OnDisable()
	{
		_imageSpriteLoader.Release();
	}
}
