using System.Text;
using AsmodeeNet.Utils.Extensions;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;

public class EnhancementBuyPriceCalculator : IEnhancementPriceCalculator
{
	private CAbility ability;

	private CAbilityCard card;

	private ICharacterEnhancementService character;

	private const string PRICE_FORMAT = "<voffset=-0.3em><size=180%><sprite name=\"Gold_Icon_White\" color=#{1}></size></voffset><color=#{1}>{0}</color>";

	public EnhancementBuyPriceCalculator(CAbility ability, CAbilityCard card, ICharacterEnhancementService character)
	{
		this.ability = ability;
		this.card = card;
		this.character = character;
	}

	public string BuildPriceDesglose(EnhancementSlot enhancement)
	{
		string arg = UIInfoTools.Instance.goldColor.ToHex();
		StringBuilder stringBuilder = new StringBuilder(LocalizationManager.GetTranslation("GUI_ENHANCEMENT_COST_BASE"));
		stringBuilder.AppendFormat("<voffset=-0.3em><size=180%><sprite name=\"Gold_Icon_White\" color=#{1}></size></voffset><color=#{1}>{0}</color>", CEnhancement.BaseCost(enhancement.enhancement, ability, AdventureState.MapState.EnhancementMode == EEnhancementMode.ClassPersistent), arg);
		if (CEnhancement.MultiTargetCost(enhancement.enhancement, ability, AdventureState.MapState.EnhancementMode == EEnhancementMode.ClassPersistent) > 0)
		{
			stringBuilder.AppendFormat(" x {0}", ScenarioRuleClient.SRLYML.Enhancements.MultiTargetMultiplier(AdventureState.MapState.EnhancementMode == EEnhancementMode.ClassPersistent));
		}
		int num = CEnhancement.PreviousEnhancementCost(card, ability, AdventureState.MapState.EnhancementMode == EEnhancementMode.ClassPersistent);
		if (num > 0)
		{
			stringBuilder.Append(" + ");
			stringBuilder.Append(LocalizationManager.GetTranslation("GUI_ENHANCEMENT_COST_EXISTING"));
			stringBuilder.AppendFormat("<voffset=-0.3em><size=180%><sprite name=\"Gold_Icon_White\" color=#{1}></size></voffset><color=#{1}>{0}</color>", num, arg);
		}
		int num2 = CEnhancement.AbilityCardLevelCost(card, AdventureState.MapState.EnhancementMode == EEnhancementMode.ClassPersistent);
		if (num2 > 0)
		{
			stringBuilder.Append(" + ");
			stringBuilder.Append(LocalizationManager.GetTranslation("GUI_ENHANCEMENT_COST_CARD_LEVEL"));
			stringBuilder.AppendFormat("<voffset=-0.3em><size=180%><sprite name=\"Gold_Icon_White\" color=#{1}></size></voffset><color=#{1}>{0}</color>", num2, arg);
		}
		stringBuilder.AppendFormat(" = {0}", CalculateTotalPrice(enhancement));
		return stringBuilder.ToString();
	}

	public int CalculateTotalPrice(EnhancementSlot enhancement)
	{
		return CEnhancement.TotalCost(enhancement.enhancement, card, ability, AdventureState.MapState.EnhancementMode == EEnhancementMode.ClassPersistent);
	}

	public bool CanAffordPrice(EnhancementSlot enhancement)
	{
		return CalculateTotalPrice(enhancement) <= character.Gold;
	}

	public bool CanAffordPoints(EnhancementSlot enhancement)
	{
		return CanAffordPoints(enhancement, character);
	}

	public static bool CanAffordPoints(EnhancementSlot enhancement, ICharacterEnhancementService character)
	{
		if (!AdventureState.MapState.IsCampaign || !character.IsCardEnhanced(enhancement.button.AbilityCardID))
		{
			return character.HasFreeEnhancementSlots();
		}
		return true;
	}

	public int CalculatePoints(EnhancementSlot enhancement)
	{
		if (!AdventureState.MapState.IsCampaign || !character.IsCardEnhanced(enhancement.button.AbilityCardID))
		{
			return 1;
		}
		return 0;
	}
}
