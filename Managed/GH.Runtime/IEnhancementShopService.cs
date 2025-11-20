using System.Collections.Generic;
using ScenarioRuleLibrary;

public interface IEnhancementShopService
{
	bool IsEnchantressIntroShown { get; }

	bool IsSellAvailable { get; }

	void AddEnhancement(EnhancementButtonBase button, EEnhancement enhancement);

	void RemoveEnhancement(EnhancementButtonBase button);

	void PreviewEnhancement(EnhancementButtonBase button, EEnhancement enhancement);

	void SetEnchantressIntroShown();

	List<EnhancementSlot> GetEnhancementsToBuy(EnhancementLine line);

	void ClearPreviewEnhancement(EnhancementButtonBase previewedEnhancement);
}
