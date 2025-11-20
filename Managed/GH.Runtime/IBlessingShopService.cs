using MapRuleLibrary.YML.Locations;

public interface IBlessingShopService
{
	bool CanAfford(string characterID, TempleYML.TempleBlessingDefinition blessing);

	bool IsAvailable(string characterID, TempleYML.TempleBlessingDefinition blessing);
}
