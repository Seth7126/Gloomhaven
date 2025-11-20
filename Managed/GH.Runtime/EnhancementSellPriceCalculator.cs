using System.Text;
using AsmodeeNet.Utils.Extensions;
using GLOOM;
using MapRuleLibrary.Adventure;

public class EnhancementSellPriceCalculator : IEnhancementPriceCalculator
{
	private EnhancementLine line;

	private ICharacterEnhancementService characterData;

	private const string PRICE_FORMAT = "<voffset=-0.3em><size=180%><sprite name=\"Gold_Icon_White\" color=#{1}></size></voffset><color=#{1}>{0}</color>";

	public EnhancementSellPriceCalculator(EnhancementLine line, ICharacterEnhancementService characterData)
	{
		this.characterData = characterData;
		this.line = line;
	}

	public string BuildPriceDesglose(EnhancementSlot enhancement)
	{
		int paidPriceEnhancement = characterData.GetPaidPriceEnhancement(enhancement.button);
		int num = characterData.CalculateSellPriceEnhancement(enhancement.button);
		string text = (characterData.GetEnhancementSellPricePercentage() * 100f).ToString("0.##\\%");
		StringBuilder stringBuilder = new StringBuilder(LocalizationManager.GetTranslation("GUI_ENHANCEMENT_COST_ORIGINAL"));
		stringBuilder.AppendFormat("<voffset=-0.3em><size=180%><sprite name=\"Gold_Icon_White\" color=#{1}></size></voffset><color=#{1}>{0}</color>", paidPriceEnhancement, UIInfoTools.Instance.goldColor.ToHex());
		stringBuilder.AppendFormat(" -" + text + " = {0}", num);
		return stringBuilder.ToString();
	}

	public int CalculateTotalPrice(EnhancementSlot enhancement)
	{
		return characterData.CalculateSellPriceEnhancement(enhancement.button);
	}

	public bool CanAffordPrice(EnhancementSlot enhancement)
	{
		return true;
	}

	public bool CanAffordPoints(EnhancementSlot enhancement)
	{
		return true;
	}

	public int CalculatePoints(EnhancementSlot enhancement)
	{
		if (!AdventureState.MapState.IsCampaign || characterData.CountEnhancements(enhancement.button.AbilityCardID) != 1)
		{
			return 0;
		}
		return 1;
	}
}
