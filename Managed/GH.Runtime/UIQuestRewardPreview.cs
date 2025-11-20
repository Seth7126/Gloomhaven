using System.Linq;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SpriteMemoryManagement;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestRewardPreview : MonoBehaviour
{
	[SerializeField]
	protected Image rewardIcon;

	[SerializeField]
	protected Image rewardSubIcon;

	[SerializeField]
	private ImageSpriteLoader _imageSpriteLoader;

	private ReferenceToSprite _referenceOnIcon;

	private ReferenceToSprite _referenceOnSubIcon;

	public void ShowReward(Reward reward)
	{
		_imageSpriteLoader.CancelLoad();
		CQuest cQuest = ((reward.Type == ETreasureType.UnlockQuest) ? MapRuleLibraryClient.MRLYML.Quests.FirstOrDefault((CQuest s) => s.ID == reward.UnlockName) : null);
		if (cQuest != null)
		{
			QuestTypeConfigUI questTypeConfig = UIInfoTools.Instance.GetQuestTypeConfig(cQuest);
			rewardIcon.color = questTypeConfig.color;
			rewardIcon.sprite = questTypeConfig.GetRewardIcon(!AdventureState.MapState.IsCampaign);
			rewardSubIcon.gameObject.SetActive(value: false);
		}
		else if (reward.Type == ETreasureType.PerkPoint)
		{
			CharacterYMLData characterYMLData = ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData s) => s.ID == reward.GiveToCharacterID);
			if (characterYMLData != null)
			{
				_referenceOnIcon = UIInfoTools.Instance.GetCharacterSpriteRef(characterYMLData.Model, highlight: false, characterYMLData.CustomCharacterConfig);
				rewardIcon.color = UIInfoTools.Instance.GetCharacterColor(characterYMLData.Model, characterYMLData.CustomCharacterConfig);
				_referenceOnSubIcon = UIInfoTools.Instance.GetRewardIcon(reward);
				rewardSubIcon.color = UIInfoTools.Instance.GetRewardColor(reward);
				rewardSubIcon.gameObject.SetActive(value: true);
			}
			else
			{
				Debug.LogError("Unable to find character with ID " + reward.GiveToCharacterID);
			}
		}
		else
		{
			rewardIcon.color = UIInfoTools.Instance.GetRewardColor(reward);
			_referenceOnIcon = UIInfoTools.Instance.GetRewardIcon(reward);
			rewardSubIcon.gameObject.SetActive(value: false);
		}
		LoadImages();
	}

	private void LoadImages()
	{
		if (_referenceOnIcon != null)
		{
			_imageSpriteLoader.AddReferenceToSpriteForImage(rewardIcon, _referenceOnIcon);
		}
		if (_referenceOnSubIcon != null)
		{
			_imageSpriteLoader.AddReferenceToSpriteForImage(rewardSubIcon, _referenceOnSubIcon);
		}
	}

	protected void OnDisable()
	{
		_imageSpriteLoader.Release();
	}
}
