using GLOOM;
using MapRuleLibrary.Party;
using UnityEngine;

public class UIGuildmasterPartyStats : UIPartyStats
{
	[Header("Gold")]
	[SerializeField]
	private UITextTooltipTarget _goldTooltip;

	public override void Setup(CMapParty mapParty)
	{
		base.Setup(mapParty);
		RefreshGold();
		RefreshTotalInfo();
	}

	public override void RefreshLevel()
	{
		levelText.text = mapParty.PartyLevel.ToString();
		levelTooltip.SetText(string.Format(LocalizationManager.GetTranslation("GUI_PartyLevel")), refreshTooltip: true);
	}

	public void RefreshGold()
	{
		_goldTooltip.SetText(string.Format(LocalizationManager.GetTranslation("GUI_PartyGold")), refreshTooltip: true);
	}

	public override void RefreshTotalInfo()
	{
		base.TotalTooltip.gameObject.SetActive(value: true);
		string text = levelTooltip.ShownTooltipText;
		if (_goldTooltip.gameObject.activeSelf)
		{
			text = text + "\n\n" + _goldTooltip.ShownTooltipText;
		}
		base.TotalTooltip.SetText(text, refreshTooltip: true);
	}
}
