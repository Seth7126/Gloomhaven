using AsmodeeNet.Utils.Extensions;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public sealed class PriceDiscountPartyInventoryTooltipHandler : PartyInventoryTooltipHandler
{
	[SerializeField]
	private TMP_Text _discountText;

	[SerializeField]
	private GameObject _root;

	public override void OnBuildStarted(CItem item, IShopItemService shopItemService)
	{
		if (item == null || shopItemService == null)
		{
			_root.SetActive(value: false);
			return;
		}
		int num = shopItemService.DiscountedCost(item);
		int buyDiscount = shopItemService.GetBuyDiscount();
		if (buyDiscount == 0)
		{
			_root.SetActive(value: false);
		}
		else if (buyDiscount > 0)
		{
			_discountText.SetText(string.Format(LocalizationManager.GetTranslation("GUI_ITEM_REPUTATION_DISCOUNT_TOOLTIP"), num, num - buyDiscount, $"+{buyDiscount}", UIInfoTools.Instance.warningColor.ToHex(), 180));
			_root.SetActive(value: true);
		}
		else
		{
			_discountText.SetText(string.Format(LocalizationManager.GetTranslation("GUI_ITEM_REPUTATION_DISCOUNT_TOOLTIP"), num, num - buyDiscount, buyDiscount, UIInfoTools.Instance.achievementCompletedColor.ToHex(), 0));
			_root.SetActive(value: true);
		}
	}
}
