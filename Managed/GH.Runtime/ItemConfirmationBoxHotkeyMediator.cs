using System;
using SM.Gamepad;
using UnityEngine;

public class ItemConfirmationBoxHotkeyMediator : MonoBehaviour
{
	[SerializeField]
	private UIItemConfirmationBox _uiItemConfirmationBox;

	[SerializeField]
	private Hotkey _shopBuySellButton;

	private void Awake()
	{
		_uiItemConfirmationBox.ConfirmationBoxRequested += UiItemConfirmationBoxOnConfirmationBoxRequested;
	}

	private void OnDestroy()
	{
		if (_uiItemConfirmationBox != null)
		{
			_uiItemConfirmationBox.ConfirmationBoxRequested -= UiItemConfirmationBoxOnConfirmationBoxRequested;
		}
	}

	private void UiItemConfirmationBoxOnConfirmationBoxRequested(BoxConfirmationType boxConfirmationType)
	{
		switch (boxConfirmationType)
		{
		case BoxConfirmationType.Sell:
			_shopBuySellButton.ExpectedEvent = "ShopSellItem";
			break;
		case BoxConfirmationType.Buy:
			_shopBuySellButton.ExpectedEvent = "ShopBuyItem";
			break;
		case BoxConfirmationType.General:
			_shopBuySellButton.ExpectedEvent = "Select";
			break;
		default:
			throw new ArgumentOutOfRangeException("boxConfirmationType", boxConfirmationType, null);
		}
	}
}
