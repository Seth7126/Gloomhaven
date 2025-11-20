using System;
using Assets.Script.GUI.MainMenu.Modding;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRulesetModSlot : UIModSlot
{
	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private Image background;

	[SerializeField]
	private UITextTooltipTarget checkboxTooltip;

	[SerializeField]
	private Image checkboxImage;

	[Header("Order")]
	[SerializeField]
	private GameObject orderMask;

	[SerializeField]
	private TextMeshProUGUI orderText;

	[Header("Active")]
	[SerializeField]
	private Sprite activeCheckboxSprite;

	[SerializeField]
	private Sprite activeHoveredCheckboxSprite;

	[SerializeField]
	private Color activeBackgroundHoveredColor;

	[SerializeField]
	private Color activeBackgroundColor;

	[Header("Inactive")]
	[SerializeField]
	private Sprite inactiveCheckboxSprite;

	[SerializeField]
	private Sprite inactiveCheckboxHoveredSprite;

	[SerializeField]
	private Color inactiveBackgroundHighlightedColor;

	[SerializeField]
	private Color inactiveBackgroundColor;

	[Header("Validation")]
	[SerializeField]
	private ExtendedButton validateButton;

	[SerializeField]
	private GameObject invalidMask;

	private Action<IMod> onValidate;

	private Action<IMod, bool> onToggled;

	private bool isHovered;

	private bool isSelected;

	private bool isValid;

	private void Awake()
	{
		button.onClick.AddListener(Toggle);
		button.onMouseEnter.AddListener(delegate
		{
			OnHovered(hovered: true);
		});
		button.onMouseExit.AddListener(delegate
		{
			OnHovered(hovered: false);
		});
		validateButton.onClick.AddListener(Validate);
	}

	private void OnDestroy()
	{
		button.onClick.RemoveAllListeners();
		validateButton.onClick.RemoveAllListeners();
	}

	public void Toggle()
	{
		SetSelected(!isSelected);
		onToggled?.Invoke(modData, isSelected);
	}

	public void SetMod(IMod modData, Action<IMod, bool> onToggled, Action<IMod> onValidate)
	{
		this.onToggled = onToggled;
		this.onValidate = onValidate;
		base.SetMod(modData);
		SetValid(isValid: true);
	}

	public override void SetMod(IMod modData)
	{
		SetMod(modData, null, null);
	}

	public void Show(bool isSelected = true, bool enableSelection = false, int? order = null)
	{
		isHovered = false;
		button.interactable = enableSelection;
		checkboxImage.gameObject.SetActive(enableSelection);
		SetSelected(isSelected);
		SetOrder(order);
		base.gameObject.SetActive(value: true);
	}

	public void SetSelected(bool selected)
	{
		isSelected = selected;
		RefreshHighlight();
	}

	public void SetOrder(int? order)
	{
		if (!order.HasValue)
		{
			orderMask.SetActive(value: false);
			return;
		}
		orderText.text = order.ToString();
		orderMask.SetActive(value: true);
	}

	private void RefreshHighlight()
	{
		thumbnailImage.material = ((isSelected || isHovered) ? null : UIInfoTools.Instance.greyedOutMaterial);
		nameText.color = ((!isValid) ? UIInfoTools.Instance.warningColor : ((isSelected || isHovered) ? UIInfoTools.Instance.basicTextColor : UIInfoTools.Instance.greyedOutTextColor));
		background.color = ((!isSelected) ? (isHovered ? inactiveBackgroundHighlightedColor : inactiveBackgroundColor) : (isHovered ? activeBackgroundHoveredColor : activeBackgroundColor));
		if (checkboxTooltip.gameObject.activeSelf)
		{
			checkboxTooltip.SetText(LocalizationManager.GetTranslation(isSelected ? "GUI_MODDING_REMOVE_MOD_TOOLTIP" : "GUI_MODDING_ADD_MOD_TOOLTIP"), refreshTooltip: true);
		}
		if (checkboxImage.gameObject.activeSelf)
		{
			checkboxImage.sprite = ((!isSelected) ? (isHovered ? inactiveCheckboxHoveredSprite : inactiveCheckboxSprite) : (isHovered ? activeHoveredCheckboxSprite : activeCheckboxSprite));
		}
	}

	private void OnHovered(bool hovered)
	{
		isHovered = hovered;
		RefreshHighlight();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetValid(bool isValid)
	{
		this.isValid = isValid;
		validateButton.gameObject.SetActive(!isValid);
		invalidMask.SetActive(!isValid);
		RefreshHighlight();
	}

	public void Validate()
	{
		onValidate?.Invoke(modData);
	}
}
