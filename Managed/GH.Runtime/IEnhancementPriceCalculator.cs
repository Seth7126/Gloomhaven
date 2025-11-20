public interface IEnhancementPriceCalculator
{
	string BuildPriceDesglose(EnhancementSlot enhancement);

	int CalculateTotalPrice(EnhancementSlot enhancement);

	bool CanAffordPrice(EnhancementSlot enhancement);

	bool CanAffordPoints(EnhancementSlot enhancement);

	int CalculatePoints(EnhancementSlot enhancement);
}
