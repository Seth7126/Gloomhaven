using System;
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using AsmodeeNet.Utils.Extensions;
using Code.State;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.PopupStates;
using SpriteMemoryManagement;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIShopItemSlot : MonoBehaviour, IMoveHandler, IEventSystemHandler
{
	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private TMP_Text itemName;

	[SerializeField]
	private TMP_Text itemNameWarning;

	[SerializeField]
	private Image itemIcon;

	[SerializeField]
	private RawImage backgroundImage;

	[SerializeField]
	private Sprite defaultBackgroundSprite;

	[SerializeField]
	private UINewNotificationTip newItemNotiifcation;

	[SerializeField]
	private GameObject divider;

	[SerializeField]
	private UITextTooltipTarget priceTooltip;

	[SerializeField]
	private string selectedAudioItem;

	[Header("Buy")]
	[SerializeField]
	private TMP_Text itemAmount;

	[SerializeField]
	private UINewNotificationTip newAmountNotiifcation;

	[SerializeField]
	private Image reputation;

	[Header("Sell")]
	[Header("Price")]
	[SerializeField]
	private TMP_Text itemPrice;

	[SerializeField]
	private Image goldIcon;

	[SerializeField]
	private GameObject itemPricePanel;

	[SerializeField]
	private TMP_Text itemPriceWarning;

	[SerializeField]
	private GUIAnimator warningAnimator;

	[Header("Owner")]
	[SerializeField]
	private float equippedMaskPosition;

	[SerializeField]
	private float equippedMaskHighlightDuration;

	[SerializeField]
	private Image validOwnerIcon;

	[SerializeField]
	private GameObject _ownerLabel;

	[SerializeField]
	private ImageSpriteLoader _imageSpriteLoader;

	[SerializeField]
	private GameObject _selectionFrame;

	[SerializeField]
	private TMP_Text _outOfStockLabel;

	private IShopItemService service;

	private int amount;

	private bool canCharacterSell;

	private int total;

	private bool isAffordable;

	private Action<UIShopItemSlot> onSelected;

	private Action<UIShopItemSlot, bool> onHovered;

	private Action<UIShopItemSlot, MoveDirection> onMoved;

	private LTDescr highlightAnimation;

	private bool isEquipped;

	private bool hovered;

	private bool _isForceHovered;

	private bool belongsToParty;

	private CMapCharacter characterControlling;

	private const string EMPTY_PRICE = "-";

	private SimpleKeyActionHandlerBlocker _simpleKeyActionHandlerBlocker;

	private bool _shopItemSlotInteractable;

	public CItem Item { get; private set; }

	public CMapCharacter Owner { get; private set; }

	public string Name { get; private set; }

	public bool IsAvailable
	{
		get
		{
			if (amount <= 0)
			{
				return canCharacterSell;
			}
			return true;
		}
	}

	public bool IsEquipped => isEquipped;

	public Selectable Selectable => button;

	private void Awake()
	{
		if (!InputManager.GamePadInUse)
		{
			button.onClick.AddListener(Select);
		}
		else
		{
			InitGamepadInput();
		}
		button.onMouseEnter.AddListener(delegate
		{
			OnHovered(hovered: true);
		});
		button.onMouseExit.AddListener(delegate
		{
			OnHovered(hovered: false);
		});
		backgroundImage.material = new Material(backgroundImage.material);
	}

	private void OnDestroy()
	{
		DisableGamepadInput();
		if (!InputManager.GamePadInUse)
		{
			button.onClick.RemoveAllListeners();
		}
	}

	private void InitGamepadInput()
	{
		_simpleKeyActionHandlerBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
		IState state = Singleton<UINavigation>.Instance.StateMachine.GetState(PopupStateTag.LevelMessage);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, Select).AddBlocker(_simpleKeyActionHandlerBlocker).AddBlocker(new StateActionHandlerBlocker(new HashSet<IState> { state })));
	}

	private void DisableGamepadInput()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, Select);
		}
	}

	private void Select()
	{
		if (button.interactable && _shopItemSlotInteractable)
		{
			if (!isAffordable)
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
				ShowWarning(show: true);
			}
			else if (IsAvailable)
			{
				AudioControllerUtils.PlaySound(selectedAudioItem);
				onSelected?.Invoke(this);
			}
		}
	}

	private void SetItem(CItem item, int price)
	{
		if (item.Tradeable)
		{
			TMP_Text tMP_Text = itemPrice;
			string text = (itemPriceWarning.text = price.ToString());
			tMP_Text.text = text;
			goldIcon.enabled = true;
			itemPriceWarning.gameObject.SetActive(value: true);
		}
		else
		{
			TMP_Text tMP_Text2 = itemPrice;
			string text = (itemPriceWarning.text = "-");
			tMP_Text2.text = text;
			goldIcon.enabled = false;
			itemPriceWarning.gameObject.SetActive(value: false);
		}
		if ((bool)_outOfStockLabel)
		{
			_outOfStockLabel.gameObject.SetActive(!IsAvailable);
		}
		if (item != Item)
		{
			Item = item;
			TMP_Text tMP_Text3 = itemName;
			TMP_Text tMP_Text4 = itemNameWarning;
			bool isItem;
			string text4 = (Name = LocalizationNameConverter.MultiLookupLocalization(item.Name, out isItem));
			string text = (tMP_Text4.text = text4);
			tMP_Text3.text = text;
			itemName.color = UIInfoTools.Instance.GetItemColor(Item.YMLData.Rarity);
			itemIcon.sprite = UIInfoTools.Instance.GetItemMiniSprite(item.YMLData.Art);
			selectedAudioItem = UIInfoTools.Instance.GetItemConfig(item.YMLData.Art).toggleAudioItem;
			if (item.YMLData.ValidEquipCharacterClassIDs.IsNullOrEmpty())
			{
				validOwnerIcon.gameObject.SetActive(value: false);
				return;
			}
			validOwnerIcon.sprite = UIInfoTools.Instance.GetCharacterMarker(item.YMLData.ValidEquipCharacterClassIDs[0]);
			validOwnerIcon.gameObject.SetActive(value: true);
		}
	}

	private void Init(CItem item, int price, Action<UIShopItemSlot> onSelected, Action<UIShopItemSlot, bool> onHovered, Action<UIShopItemSlot, MoveDirection> onMoved, CMapCharacter owner, bool isEquipped = false, int amount = 1, int total = 1, bool isAffordable = true, float discount = 0f, CMapCharacter characterControlling = null, bool canCharacterSell = false)
	{
		this.onSelected = onSelected;
		this.onHovered = onHovered;
		this.onMoved = onMoved;
		this.total = total;
		this.amount = amount;
		this.canCharacterSell = canCharacterSell;
		this.isAffordable = isAffordable;
		this.characterControlling = characterControlling;
		SetItem(item, price);
		SetDiscount(price, discount);
		UpdateOwner(owner, isEquipped);
		CancelAnimation();
		ShowWarning(show: false);
		EnableDivider(enable: true);
		ToggleSelectionFrame(hovered);
	}

	public void Initialize(CItem item, int price, Action<UIShopItemSlot> onSelected, Action<UIShopItemSlot, bool> onHovered, Action<UIShopItemSlot, MoveDirection> onMoved, CMapCharacter owner, bool isEquipped = false, CMapCharacter characterControlling = null, bool isOwned = false)
	{
		belongsToParty = true;
		Init(item, price, onSelected, onHovered, onMoved, owner, isEquipped, 1, 1, isAffordable: true, 0f, characterControlling);
		itemAmount.gameObject.SetActive(value: false);
		ShowNewItemNotification(item.IsNew);
		ShowNewAmountNotification(isNew: false);
	}

	public void Initialize(CItem item, int price, Action<UIShopItemSlot> onSelected, Action<UIShopItemSlot, bool> onHovered, Action<UIShopItemSlot, MoveDirection> onMoved, CMapCharacter owner, int amount, int total, bool isEquipped = false, CMapCharacter characterControlling = null, bool isOwned = false)
	{
		belongsToParty = true;
		Init(item, price, onSelected, onHovered, onMoved, owner, isEquipped, amount, total, isAffordable: true, 0f, characterControlling, canCharacterSell: true);
		itemAmount.gameObject.SetActive(value: true);
		ShowNewItemNotification(item.IsNew);
		ShowNewAmountNotification(isNew: false);
	}

	public void Initialize(CItem item, int price, Action<UIShopItemSlot> onSelected, Action<UIShopItemSlot, bool> onHovered, Action<UIShopItemSlot, MoveDirection> onMoved, int amount, int total, bool isAffordable, bool isNewItem = false, bool isNewAmount = false, int discount = 0, CMapCharacter characterControlling = null)
	{
		belongsToParty = false;
		Init(item, price, onSelected, onHovered, onMoved, null, isEquipped: false, amount, total, isAffordable, discount, characterControlling);
		itemAmount.gameObject.SetActive(value: true);
		ShowNewItemNotification(isNewItem);
		ShowNewAmountNotification(isNewAmount);
	}

	private void SetDiscount(float price, float discount)
	{
		if (discount == 0f)
		{
			priceTooltip.enabled = false;
			reputation.gameObject.SetActive(value: false);
		}
		else if (discount > 0f)
		{
			priceTooltip.SetText(string.Format(LocalizationManager.GetTranslation("GUI_ITEM_REPUTATION_DISCOUNT_TOOLTIP"), price, price - discount, $"+{discount}", UIInfoTools.Instance.warningColor.ToHex(), 180));
			priceTooltip.enabled = true;
			reputation.color = UIInfoTools.Instance.warningColor;
			reputation.transform.localScale = new Vector3(1f, -1f, 1f);
			reputation.gameObject.SetActive(value: true);
		}
		else
		{
			priceTooltip.SetText(string.Format(LocalizationManager.GetTranslation("GUI_ITEM_REPUTATION_DISCOUNT_TOOLTIP"), price, price - discount, discount, UIInfoTools.Instance.achievementCompletedColor.ToHex(), 0));
			priceTooltip.enabled = true;
			reputation.color = UIInfoTools.Instance.achievementCompletedColor;
			reputation.transform.localScale = new Vector3(1f, 1f, 1f);
			reputation.gameObject.SetActive(value: true);
		}
	}

	public void OnHovered(bool hovered)
	{
		if (!hovered && _isForceHovered)
		{
			return;
		}
		if (InputManager.GamePadInUse)
		{
			if (hovered)
			{
				_simpleKeyActionHandlerBlocker.SetBlock(value: false);
			}
			else
			{
				_simpleKeyActionHandlerBlocker.SetBlock(value: true);
			}
		}
		this.hovered = hovered;
		if (hovered)
		{
			ShowNewItemNotification(isNew: false);
			ShowNewAmountNotification(isNew: false);
		}
		RefreshHighlight(animate: true);
		ToggleSelectionFrame(hovered);
		onHovered?.Invoke(this, hovered);
	}

	private void ToggleSelectionFrame(bool isHovered)
	{
		if (_selectionFrame != null)
		{
			_selectionFrame.SetActive(isHovered);
		}
	}

	private void RefreshHighlight(bool animate = false)
	{
		if (hovered)
		{
			backgroundImage.enabled = true;
			AnimateHighlight(0f, animate);
		}
		else
		{
			backgroundImage.enabled = isEquipped;
			AnimateHighlight((!isEquipped) ? 0f : equippedMaskPosition, animate);
		}
	}

	private void AnimateHighlight(float to, bool animate = false)
	{
		CancelAnimation();
		Vector2 tiling = backgroundImage.materialForRendering.GetTextureScale("_Mask");
		if (!animate)
		{
			backgroundImage.materialForRendering.SetTextureScale("_Mask", new Vector2(to, tiling.y));
			return;
		}
		float time = Math.Abs(to - tiling.x) / equippedMaskPosition * equippedMaskHighlightDuration;
		highlightAnimation = LeanTween.value(backgroundImage.gameObject, delegate(float val)
		{
			backgroundImage.materialForRendering.SetTextureScale("_Mask", new Vector2(val, tiling.y));
		}, tiling.x, to, time).setOnComplete((Action)delegate
		{
			highlightAnimation = null;
		});
	}

	public void EnableDivider(bool enable)
	{
		divider.SetActive(enable);
	}

	private void CancelAnimation()
	{
		if (highlightAnimation != null)
		{
			LeanTween.cancel(highlightAnimation.id);
		}
		highlightAnimation = null;
	}

	public void UpdateAffordability(bool isAffordable)
	{
		this.isAffordable = isAffordable;
		DetermineInteractability();
	}

	public void UpdateAvailability(int amount)
	{
		this.amount = amount;
		DetermineInteractability();
	}

	public void DetermineInteractability()
	{
		_shopItemSlotInteractable = IsAvailable && Item.Tradeable;
		if (FFSNetwork.IsOnline && _shopItemSlotInteractable)
		{
			if (belongsToParty && Owner != null)
			{
				int controllableID = (AdventureState.MapState.IsCampaign ? Owner.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(Owner.CharacterID));
				_shopItemSlotInteractable = PlayerRegistry.MyPlayer.HasControlOver(controllableID);
			}
			else if (characterControlling != null)
			{
				int controllableID2 = (AdventureState.MapState.IsCampaign ? characterControlling.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(characterControlling.CharacterID));
				_shopItemSlotInteractable = PlayerRegistry.MyPlayer.HasControlOver(controllableID2);
			}
		}
		if (!InputManager.GamePadInUse)
		{
			button.interactable = _shopItemSlotInteractable;
		}
		if (!_shopItemSlotInteractable)
		{
			itemIcon.material = UIInfoTools.Instance.greyedOutMaterial;
			if (goldIcon == null)
			{
				Debug.LogError("Null gold icon");
			}
			else
			{
				goldIcon.color = UIInfoTools.Instance.greyedOutTextColor;
			}
			if (itemAmount == null)
			{
				Debug.LogError("Null itemAmount");
			}
			else
			{
				itemAmount.color = UIInfoTools.Instance.greyedOutTextColor;
			}
			if (itemPrice == null)
			{
				Debug.LogError("Null itemPrice");
			}
			else
			{
				itemPrice.color = UIInfoTools.Instance.greyedOutTextColor;
			}
			if (itemName == null)
			{
				Debug.LogError("Null itemName");
			}
			else
			{
				itemName.color = UIInfoTools.Instance.greyedOutTextColor;
			}
			if (backgroundImage == null)
			{
				Debug.LogError("Null backgroundImage");
			}
			else
			{
				backgroundImage.SetAlpha(0f);
			}
			validOwnerIcon.color = UIInfoTools.Instance.greyedOutTextColor;
		}
		else
		{
			itemIcon.material = null;
			if (goldIcon == null)
			{
				Debug.LogError("Null gold icon");
			}
			else
			{
				goldIcon.color = (isAffordable ? UIInfoTools.Instance.goldColor : UIInfoTools.Instance.warningColor);
			}
			if (itemAmount == null)
			{
				Debug.LogError("Null itemAmount");
			}
			else
			{
				itemAmount.color = Color.white;
			}
			if (itemPrice == null)
			{
				Debug.LogError("Null itemPrice");
			}
			else
			{
				itemPrice.color = (isAffordable ? UIInfoTools.Instance.goldColor : UIInfoTools.Instance.warningColor);
			}
			if (itemName == null)
			{
				Debug.LogError("Null itemName");
			}
			else
			{
				itemName.color = UIInfoTools.Instance.GetItemColor(Item.YMLData.Rarity);
			}
			if (backgroundImage == null)
			{
				Debug.LogError("Null backgroundImage");
			}
			else
			{
				backgroundImage.SetAlpha(1f);
			}
			validOwnerIcon.color = UIInfoTools.Instance.White;
		}
		itemAmount.text = amount + "/" + total;
	}

	public void UpdateOwner(CMapCharacter owner, bool isEquipped = false)
	{
		Owner = owner;
		this.isEquipped = isEquipped;
		if (owner == null)
		{
			backgroundImage.texture = ((characterControlling == null) ? defaultBackgroundSprite.texture : UIInfoTools.Instance.GetNewAdventureCharacterPortrait(characterControlling.CharacterYMLData.Model, highlighted: true, characterControlling.CharacterYMLData.CustomCharacterConfig).texture);
		}
		else
		{
			ECharacter model = owner.CharacterYMLData.Model;
			backgroundImage.texture = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(model, highlighted: true, owner.CharacterYMLData.CustomCharacterConfig).texture;
		}
		HandleGamepadOwner();
		DetermineInteractability();
		RefreshHighlight();
	}

	private void HandleGamepadOwner()
	{
		if (InputManager.GamePadInUse)
		{
			_ownerLabel.SetActive(Owner != null);
		}
	}

	protected void OnDisable()
	{
		_imageSpriteLoader.Release();
		if (!CoreApplication.IsQuitting)
		{
			CancelAnimation();
			ShowWarning(show: false);
			if (hovered)
			{
				OnHovered(hovered: false);
			}
		}
	}

	public void ShowNewItemNotification(bool isNew)
	{
		if (isNew)
		{
			newItemNotiifcation.Show();
		}
		else
		{
			newItemNotiifcation.Hide();
		}
	}

	public void ShowNewAmountNotification(bool isNew)
	{
		if (isNew)
		{
			newAmountNotiifcation.Show();
		}
		else
		{
			newAmountNotiifcation.Hide();
		}
	}

	private void ShowWarning(bool show)
	{
		if (show)
		{
			warningAnimator.Play();
		}
		else
		{
			warningAnimator.Stop();
		}
	}

	public void OnMove(AxisEventData eventData)
	{
		onMoved?.Invoke(this, eventData.moveDir);
	}

	public void ForceHovered(bool isForceHovered)
	{
		_isForceHovered = isForceHovered;
	}
}
