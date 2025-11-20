using AsmodeeNet.Utils.Extensions;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICampaignPartyStats : UIPartyStats
{
	[Header("Reputation")]
	[SerializeField]
	private TextMeshProUGUI reputationText;

	[SerializeField]
	private ImageProgressBar reputationBar;

	[SerializeField]
	private UITextTooltipTarget reputationTooltip;

	[SerializeField]
	private Image reputationFill;

	[SerializeField]
	private Color positiveReputationColor;

	[SerializeField]
	private Color negativeReputationColor;

	[Header("Wealth")]
	[SerializeField]
	private TextMeshProUGUI wealthText;

	[SerializeField]
	private ImageProgressBar wealthBar;

	[SerializeField]
	private UITextTooltipTarget wealthTooltip;

	private int prosperityLevelForInfo;

	public override void Setup(CMapParty mapParty)
	{
		ClearEvents();
		base.Setup(mapParty);
		RefreshReputation(animate: false);
		RefreshWealth(animate: false);
		RefreshTotalInfo();
		Singleton<MapChoreographer>.Instance?.EventBuss?.RegisterToOnProsperityChanged(OnProsperityChanged);
		Singleton<MapChoreographer>.Instance?.EventBuss?.RegisterToOnReputationChanged(OnReputationChanged);
	}

	public void RefreshReputation(bool animate)
	{
		string text = ((mapParty.ShopDiscount >= 0) ? $"+{mapParty.ShopDiscount}" : mapParty.ShopDiscount.ToString());
		string amountText = $"<rotate={((mapParty.Reputation < 0) ? 180 : 0)}><sprite name=\"ReputationLevel_Icon\"></rotate>{text}";
		Color color = ((mapParty.Reputation >= 0) ? positiveReputationColor : negativeReputationColor);
		reputationText.text = string.Format(string.Format("{0} {1}", LocalizationManager.GetTranslation("GUI_PartyReputation"), mapParty.Reputation));
		reputationTooltip.SetText(string.Format(LocalizationManager.GetTranslation("GUI_PartyReputation_Tooltip"), mapParty.Reputation, text, color.ToHex(), (mapParty.Reputation < 0) ? 180 : 0), refreshTooltip: true);
		reputationFill.color = color;
		if (animate)
		{
			reputationBar.PlayProgressTo(Mathf.InverseLerp(-20f, 20f, mapParty.Reputation), amountText);
		}
		else
		{
			reputationBar.SetAmount(Mathf.InverseLerp(-20f, 20f, mapParty.Reputation), amountText);
		}
	}

	public void RefreshWealth(bool animate)
	{
		RefreshWealth(mapParty.ProsperityXP, mapParty.ProsperityLevel, mapParty.ProsperityLevel, animate);
	}

	private void RefreshWealth(int prosperityXP, int prosperityLevel, int oldProsperityLevel, bool animate)
	{
		wealthText.text = string.Format(string.Format("{0} {1}", LocalizationManager.GetTranslation(InputManager.GamePadInUse ? "Consoles/GUI_PartyProsperity" : "GUI_PartyProsperity"), prosperityLevel));
		wealthTooltip.SetText(string.Format(LocalizationManager.GetTranslation("GUI_PartyProsperity_Tooltip"), prosperityLevel), refreshTooltip: true);
		if (prosperityLevel < AdventureState.MapState.HeadquartersState.Headquarters.LevelToProsperity.Count)
		{
			wealthBar.ShowProgressText(show: true);
			int num = AdventureState.MapState.HeadquartersState.Headquarters.LevelToProsperity[prosperityLevel - 1];
			int num2 = AdventureState.MapState.HeadquartersState.Headquarters.LevelToProsperity[prosperityLevel];
			if (animate)
			{
				if (prosperityLevel > oldProsperityLevel)
				{
					wealthBar.SetAmount(0f, 1f);
				}
				else if (prosperityLevel < oldProsperityLevel)
				{
					wealthBar.SetAmount(1f, 1f);
				}
				wealthBar.PlayProgressTo(prosperityXP - num, num2 - num);
			}
			else
			{
				wealthBar.SetAmount(prosperityXP - num, num2 - num);
			}
		}
		else
		{
			wealthBar.SetAmount(1f, 1f);
			wealthBar.ShowProgressText(show: false);
		}
	}

	public override void RefreshTotalInfo()
	{
		if (totalTooltip != null)
		{
			base.TotalTooltip.SetText(levelTooltip.ShownTooltipText + "\n\n" + reputationTooltip.ShownTooltipText + "\n\n" + wealthTooltip.ShownTooltipText, refreshTooltip: true);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ClearEvents();
	}

	private void ClearEvents()
	{
		if (Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance?.EventBuss?.UnregisterToOnReputationChanged(OnReputationChanged);
			Singleton<MapChoreographer>.Instance?.EventBuss?.UnregisterToOnProsperityChange(OnProsperityChanged);
		}
	}

	private void OnProsperityChanged(int newXP, int oldXP, int newLevel, int oldLevel)
	{
		RefreshWealth(newXP, newLevel, oldLevel, animate: true);
		RefreshTotalInfo();
	}

	private void OnReputationChanged(int newvalue)
	{
		RefreshReputation(animate: true);
		RefreshTotalInfo();
	}
}
