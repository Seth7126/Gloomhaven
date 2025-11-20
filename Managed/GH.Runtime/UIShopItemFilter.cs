using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShopItemFilter : MonoBehaviour
{
	public UITab tab;

	public ItemListingType filter;

	[SerializeField]
	private UINewNotificationTip newNotification;

	[SerializeField]
	private TextMeshProUGUI amountText;

	[SerializeField]
	private Material amountTextHighlightMaterial;

	private Material defaultTextMaterial;

	private void Awake()
	{
		defaultTextMaterial = amountText.material;
	}

	public void ShowNewNotification(bool show)
	{
		if (show)
		{
			newNotification.Show();
		}
		else
		{
			newNotification.Hide();
		}
	}

	public void ShowAmount(int amount)
	{
		amountText.text = amount.ToString();
	}

	public void Activate()
	{
		tab.Activate();
	}

	public void OnHovered(bool hovered)
	{
		amountText.material = (hovered ? amountTextHighlightMaterial : defaultTextMaterial);
	}
}
