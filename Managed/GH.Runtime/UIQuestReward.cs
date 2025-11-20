using System;
using AsmodeeNet.Utils.Extensions;
using GLOOM;
using I2.Loc;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestReward : UIReward
{
	[SerializeField]
	private Color lockedItemNameColor;

	[SerializeField]
	private GameObject unavailableMask;

	private Reward reward;

	private Action<Reward, bool> onHovered;

	private void OnEnable()
	{
		I2.Loc.LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
	}

	private new void OnDisable()
	{
		I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
	}

	public override void ShowReward(Reward reward)
	{
		ShowReward(reward, null);
	}

	public void ShowReward(Reward reward, Action<Reward, bool> onHovered, bool isAvailable = true)
	{
		this.onHovered = onHovered;
		if (this.reward != reward)
		{
			this.reward = reward;
			base.ShowReward(reward);
		}
		if (button != null && onHovered != null && reward.Type.In(ETreasureType.ItemStock, ETreasureType.UnlockProsperityItemStock, ETreasureType.Item, ETreasureType.UnlockProsperityItem))
		{
			button.interactable = true;
		}
		UpdateUnlocked(isUnlocked: true, refreshText: false);
		unavailableMask.SetActive(!isAvailable);
	}

	public void ShowReward(Sprite icon, string text, Action<Reward, bool> onHovered, bool isAvailable = true)
	{
		this.onHovered = onHovered;
		reward = null;
		rewardSubIcon.gameObject.SetActive(value: false);
		rewardIcon.sprite = icon;
		rewardIcon.color = UIInfoTools.Instance.White;
		rewardInformation.text = text;
		UpdateUnlocked(isUnlocked: true, refreshText: false);
		unavailableMask.SetActive(!isAvailable);
	}

	public void ShowUnlocked(bool isUnlocked)
	{
		UpdateUnlocked(isUnlocked, refreshText: true);
	}

	private void UpdateUnlocked(bool isUnlocked, bool refreshText)
	{
		Image image = rewardIcon;
		Material material = (rewardSubIcon.material = (isUnlocked ? null : UIInfoTools.Instance.greyedOutMaterial));
		image.material = material;
		if (refreshText && reward != null)
		{
			if (reward.Type == ETreasureType.ItemStock || reward.Type == ETreasureType.UnlockProsperityItemStock)
			{
				rewardInformation.text = string.Format(GLOOM.LocalizationManager.GetTranslation("GUI_QUEST_REWARD_ItemStock"), string.Format("<font=\"MarcellusSC-Regular SDF\"><size=+2><cspace=1>{0}</cspace></size></font>", GLOOM.LocalizationManager.GetTranslation(reward.Item.Name), (isUnlocked ? itemNameColor : lockedItemNameColor).ToHex()));
			}
			else if (reward.Type == ETreasureType.Item || reward.Type == ETreasureType.UnlockProsperityItem)
			{
				rewardInformation.text = string.Format("<font=\"MarcellusSC-Regular SDF\"><size=+2><cspace=1>{0}</cspace></size></font>", GLOOM.LocalizationManager.GetTranslation(reward.Item.Name), isUnlocked ? itemNameColor.ToHex() : lockedItemNameColor.ToHex());
			}
			else if (reward.Type == ETreasureType.UnlockMultiplayer)
			{
				rewardInformation.text = string.Format("<font=\"MarcellusSC-Regular SDF\"><size=+2><cspace=1>{0}</cspace></size></font>", GLOOM.LocalizationManager.GetTranslation("GUI_QUEST_REWARD_Multiplayer"), isUnlocked ? itemNameColor.ToHex() : lockedItemNameColor.ToHex());
			}
		}
	}

	public void OnHovered(bool hovered)
	{
		onHovered?.Invoke(reward, hovered);
	}

	private void OnLanguageChanged()
	{
		if (reward != null)
		{
			base.ShowReward(reward);
		}
	}
}
