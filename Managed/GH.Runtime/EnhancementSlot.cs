using System.Linq;
using ScenarioRuleLibrary;

public class EnhancementSlot
{
	public EnhancementButtonBase button;

	public EEnhancement enhancement;

	public IEnhancementPriceCalculator priceCalculator;

	private EnhancementLine _enhancementLine;

	public bool IsCurrentEnhanced
	{
		get
		{
			if (enhancement == button.EnhancedType)
			{
				return button.EnhancementType != EEnhancement.NoEnhancement;
			}
			return false;
		}
	}

	public bool IsEmpty => AnyEmpty != null;

	public EnhancementButtonBase AnyEmpty => _enhancementLine.EnhancementSlots.FirstOrDefault((EnhancementButtonBase x) => x.EnhancedType == EEnhancement.NoEnhancement);

	public CAbility Ability => button.Ability;

	public bool BuyMode { get; }

	public bool Available
	{
		get
		{
			if (!AvailableToBuy)
			{
				return AvailableToSell;
			}
			return true;
		}
	}

	public bool AvailableToBuy
	{
		get
		{
			if (IsEmpty)
			{
				return BuyMode;
			}
			return false;
		}
	}

	public bool AvailableToSell
	{
		get
		{
			if (!BuyMode)
			{
				return IsCurrentEnhanced;
			}
			return false;
		}
	}

	public EnhancementSlot(EnhancementButtonBase button, EEnhancement enhancement, bool buyMode, EnhancementLine enhancementLine, IEnhancementPriceCalculator priceCalculator = null)
	{
		_enhancementLine = enhancementLine;
		BuyMode = buyMode;
		this.button = button;
		this.enhancement = enhancement;
		this.priceCalculator = priceCalculator;
	}
}
