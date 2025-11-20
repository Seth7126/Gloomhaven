using ScenarioRuleLibrary;
using UnityEngine;

public abstract class PartyInventoryTooltipHandler : MonoBehaviour, IPartyInventoryTooltipHandler
{
	public abstract void OnBuildStarted(CItem item, IShopItemService shopItemService);

	public virtual void OnHide()
	{
	}
}
