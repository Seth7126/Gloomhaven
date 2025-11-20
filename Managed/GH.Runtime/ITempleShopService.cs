using System.Collections.Generic;
using MapRuleLibrary.YML.Locations;

public interface ITempleShopService
{
	int DevotionLevel { get; }

	int DevotionCurrentProgress { get; }

	void Buy(string characterID, TempleYML.TempleBlessingDefinition blessing);

	List<TempleYML.TempleBlessingDefinition> GetAvailableBlessings();

	bool CanBuy(string characterID, TempleYML.TempleBlessingDefinition blessing);

	int CalculateTotalGoldDonated();

	int CalculateDevotionTotalProgress(int level);
}
