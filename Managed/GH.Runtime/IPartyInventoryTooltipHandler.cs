using ScenarioRuleLibrary;

public interface IPartyInventoryTooltipHandler
{
	void OnBuildStarted(CItem item, IShopItemService shopItemService);

	void OnHide();
}
