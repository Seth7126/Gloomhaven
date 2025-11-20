using System;
using System.Linq;
using AsmodeeNet.Foundation;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using SM.Gamepad;
using ScenarioRuleLibrary;
using SpriteMemoryManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPartyItemSlot : MonoBehaviour
{
	private const string CharacterNamePrefix = "AA_";

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private GameObject newTooltip;

	[Header("Item")]
	[SerializeField]
	private Image itemIcon;

	[SerializeField]
	private TextMeshProUGUI itemName;

	[SerializeField]
	private TextMeshProUGUI itemNameShine;

	[Header("Owner")]
	[SerializeField]
	private RawImage highlightBackground;

	[SerializeField]
	private GameObject belongsToOther;

	[SerializeField]
	private Image ownerIcon;

	[SerializeField]
	private Image ownerIconShine;

	[SerializeField]
	private Image validOwnerIcon;

	[SerializeField]
	private Color _equippedHighlightColor;

	[SerializeField]
	private Color _selectedHighlightColor;

	[Header("Price")]
	[SerializeField]
	private GameObject price;

	[SerializeField]
	private TextMeshProUGUI priceText;

	[SerializeField]
	private TextMeshProUGUI warningPriceText;

	[SerializeField]
	private GUIAnimator warningCostAnimation;

	[SerializeField]
	private GameObject priceIcon;

	[Header("Selection")]
	[SerializeField]
	private Image selectedImage;

	[SerializeField]
	private GUIAnimator assignAnimation;

	[SerializeField]
	private Color shineColor;

	[SerializeField]
	private TMP_Text _equippedText;

	[SerializeField]
	private ImageSpriteLoader _imageSpriteLoader;

	[Header("Hover")]
	[SerializeField]
	private Image _hoveredFrameImage;

	[SerializeField]
	private float _highlightMoveDuration;

	[Header("Equipped Settings")]
	[SerializeField]
	private float _equippedMaskPosition;

	private Color _invisibleColor = new Color(0f, 0f, 0f, 0f);

	private ReferenceToSprite _referenceForOwnerIcon;

	private ReferenceToSprite _referenceForOwnerIconShine;

	private IUiNavigationSelectable _navigationSelectable;

	private CItem item;

	private Action<UIPartyItemSlot> onSelect;

	private Action<UIPartyItemSlot, bool> onDeselect;

	private Action<UIPartyItemSlot, bool> onHovered;

	private CMapCharacter character;

	private CMapCharacter owner;

	private const string EMPTY_PRICE = "-";

	private bool isSelected;

	private bool isInOtherSlot;

	private bool selectable;

	private bool isMarked;

	private LTDescr highlightAnimation;

	private SimpleKeyActionHandlerBlocker _simpleKeyActionHandlerBlocker;

	public CMapCharacter Owner => owner;

	public CItem Item => item;

	public bool IsEquippedInOtherSlot
	{
		get
		{
			if (isInOtherSlot)
			{
				return item.IsSlotIndexSet;
			}
			return false;
		}
	}

	public bool IsSelected => isSelected;

	public IUiNavigationSelectable NavigationSelectable => _navigationSelectable ?? (_navigationSelectable = GetComponent<IUiNavigationSelectable>());

	private void Awake()
	{
		if (!InputManager.GamePadInUse)
		{
			button.onClick.AddListener(Toggle);
		}
		button.onMouseEnter.AddListener(OnSelected);
		button.onMouseExit.AddListener(OnDeselected);
		assignAnimation.OnAnimationStarted.AddListener(delegate
		{
			ownerIconShine.color = shineColor;
		});
		warningCostAnimation.OnAnimationStarted.AddListener(delegate
		{
			ownerIconShine.color = Color.red;
		});
		_simpleKeyActionHandlerBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, Toggle).AddBlocker(new ExtendedButtonSelectKeyActionHandlerBlocker(button)).AddBlocker(new ExtendedButtonInteractableKeyActionHandlerBlocker(button)).AddBlocker(_simpleKeyActionHandlerBlocker));
		highlightBackground.material = new Material(highlightBackground.material);
	}

	private void OnDestroy()
	{
		if (!InputManager.GamePadInUse)
		{
			button.onClick.RemoveAllListeners();
		}
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, Toggle);
		}
		CanceAnimation();
	}

	public void SetItem(CMapCharacter character, CItem item, CMapCharacter owner = null, Action<UIPartyItemSlot> onSelected = null, Action<UIPartyItemSlot, bool> onDeselected = null, Action<UIPartyItemSlot, bool> onHovered = null, bool isInOtherSlot = false, bool selectable = true)
	{
		this.character = character;
		onSelect = onSelected;
		onDeselect = onDeselected;
		this.onHovered = onHovered;
		this.item = item;
		this.selectable = selectable;
		itemIcon.sprite = UIInfoTools.Instance.GetItemMiniSprite(item.YMLData.Art);
		TextMeshProUGUI textMeshProUGUI = itemName;
		string text = (itemNameShine.text = LocalizationManager.GetTranslation(item.YMLData.Name));
		textMeshProUGUI.text = text;
		itemName.color = UIInfoTools.Instance.GetItemColor(item.YMLData.Rarity);
		if (_hoveredFrameImage != null)
		{
			_hoveredFrameImage.enabled = false;
		}
		RefreshNewNotification();
		if (item.YMLData.ValidEquipCharacterClassIDs.IsNullOrEmpty())
		{
			TextMeshProUGUI textMeshProUGUI2 = priceText;
			text = (warningPriceText.text = item.SellPrice.ToString());
			textMeshProUGUI2.text = text;
			priceText.color = UIInfoTools.Instance.goldColor;
			validOwnerIcon.gameObject.SetActive(value: false);
			warningCostAnimation.gameObject.SetActive(value: true);
			priceIcon.SetActive(value: true);
		}
		else
		{
			priceText.text = "-";
			priceText.color = UIInfoTools.Instance.greyedOutTextColor;
			priceIcon.SetActive(value: false);
			warningCostAnimation.gameObject.SetActive(value: false);
			validOwnerIcon.sprite = UIInfoTools.Instance.GetCharacterMarker(item.YMLData.ValidEquipCharacterClassIDs[0]);
			validOwnerIcon.gameObject.SetActive(value: true);
		}
		SetSelected(selected: false);
		SetOwner(owner, isInOtherSlot);
		ShowWarningCost(show: false);
	}

	public void SetMarked()
	{
		isMarked = true;
	}

	public void SetSelected(bool selected)
	{
		isSelected = selected;
		if (selectedImage != null)
		{
			selectedImage.enabled = selected;
		}
		if (_equippedText != null)
		{
			_equippedText.gameObject.SetActive(selected);
		}
		highlightBackground.color = (isSelected ? _equippedHighlightColor : _invisibleColor);
		AnimateHighlight(isSelected ? _equippedMaskPosition : 0f);
	}

	public void SetOwner(CMapCharacter ownerCharacter, bool isInOtherSlot = false)
	{
		owner = ownerCharacter;
		this.isInOtherSlot = isInOtherSlot;
		if (ownerCharacter != null)
		{
			ECharacter model = ownerCharacter.CharacterYMLData.Model;
			_referenceForOwnerIcon = UIInfoTools.Instance.GetCharacterSpriteRef(model, highlight: false, ownerCharacter.CharacterYMLData.CustomCharacterConfig);
			_referenceForOwnerIconShine = UIInfoTools.Instance.GetCharacterSpriteRef(model, highlight: true, ownerCharacter.CharacterYMLData.CustomCharacterConfig);
			ownerIcon.color = UIInfoTools.Instance.GetCharacterColor(model, ownerCharacter.CharacterYMLData.CustomCharacterConfig);
			highlightBackground.texture = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(model, highlighted: true, ownerCharacter.CharacterYMLData.CustomCharacterConfig).texture;
			ownerIcon.enabled = true;
			ownerIcon.sprite = UIInfoTools.Instance.GetActiveAbilityIcon("AA_" + ownerCharacter.CharacterYMLData.Model);
			ownerIconShine.sprite = ownerIcon.sprite;
			price.SetActive(owner.CharacterID != character.CharacterID);
			belongsToOther.SetActive(owner.CharacterID != character.CharacterID || isInOtherSlot);
		}
		else
		{
			highlightBackground.texture = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(character.CharacterYMLData.Model, highlighted: true, character.CharacterYMLData.CustomCharacterConfig).texture;
			belongsToOther.SetActive(value: false);
			ownerIcon.enabled = false;
			price.SetActive(value: false);
		}
		RefreshOwnerInteraction();
	}

	private void OnSelected()
	{
		isMarked = false;
		OnHovered(hovered: true);
	}

	private void OnDeselected()
	{
		if (!isMarked)
		{
			OnHovered(hovered: false);
		}
	}

	private void OnHovered(bool hovered)
	{
		if (hovered)
		{
			newTooltip.SetActive(value: false);
		}
		onHovered?.Invoke(this, hovered);
		if (hovered)
		{
			highlightBackground.color = _selectedHighlightColor;
			AnimateHighlight(0f, animate: true);
		}
		else
		{
			highlightBackground.color = (isSelected ? _equippedHighlightColor : _invisibleColor);
			AnimateHighlight(isSelected ? _equippedMaskPosition : 0f, animate: true);
		}
		if (_hoveredFrameImage != null)
		{
			_hoveredFrameImage.enabled = hovered;
		}
	}

	private void AnimateHighlight(float to, bool animate = false)
	{
		CanceAnimation();
		Vector2 tiling = highlightBackground.materialForRendering.GetTextureScale("_Mask");
		if (!animate)
		{
			highlightBackground.materialForRendering.SetTextureScale("_Mask", new Vector2(to, tiling.y));
			return;
		}
		float time = Math.Abs(to - tiling.x) / _equippedMaskPosition * _highlightMoveDuration;
		highlightAnimation = LeanTween.value(highlightBackground.gameObject, delegate(float val)
		{
			highlightBackground.materialForRendering.SetTextureScale("_Mask", new Vector2(val, tiling.y));
		}, tiling.x, to, time).setOnStart(delegate
		{
			highlightBackground.enabled = true;
		}).setOnComplete((Action)delegate
		{
			highlightAnimation = null;
		});
	}

	private void CanceAnimation()
	{
		if (highlightAnimation != null)
		{
			LeanTween.cancel(highlightAnimation.id);
		}
		highlightAnimation = null;
	}

	public void RefreshNewNotification()
	{
		newTooltip.SetActive(item.IsNew);
	}

	public void RefreshOwnerInteraction()
	{
		bool flag = item.YMLData.ValidEquipCharacterClassIDs.IsNullOrEmpty() || item.YMLData.ValidEquipCharacterClassIDs.Contains(character.CharacterID);
		bool flag2 = selectable && flag;
		bool flag3 = flag2;
		if (FFSNetwork.IsOnline)
		{
			int controllableID = (AdventureState.MapState.IsCampaign ? character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(character.CharacterID));
			CMapCharacter cMapCharacter = owner ?? character;
			bool flag4 = cMapCharacter != null && (!AdventureState.MapState.MapParty.SelectedCharacters.Contains(cMapCharacter) || cMapCharacter.IsUnderMyControl) && (owner == null || ControllableRegistry.GetControllable(controllableID)?.NetworkEntity != null);
			flag2 = flag2 && flag4;
			flag3 &= flag4 && character.IsUnderMyControl;
		}
		button.interactable = flag2;
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.Inventory.SetInteractionAvailable(flag3);
		_simpleKeyActionHandlerBlocker.SetBlock(!flag3);
	}

	public void Toggle()
	{
		if (isSelected)
		{
			Deselect();
		}
		else
		{
			onSelect?.Invoke(this);
		}
	}

	private void Deselect()
	{
		onDeselect?.Invoke(this, arg2: true);
	}

	public void AssignOwner(CMapCharacter characterID)
	{
		SetSelected(selected: true);
		SetOwner(characterID);
		assignAnimation.Play();
	}

	public void RemoveEquipped()
	{
		isInOtherSlot = false;
		SetSelected(selected: false);
		assignAnimation.Stop();
	}

	public void ShowWarningCost(bool show)
	{
		if (show && warningCostAnimation.gameObject.activeSelf)
		{
			warningCostAnimation.Play();
		}
		else
		{
			warningCostAnimation.Stop();
		}
	}

	protected void OnEnable()
	{
		if (_referenceForOwnerIconShine != null && _referenceForOwnerIcon != null)
		{
			_imageSpriteLoader.AddReferenceToSpriteForImage(ownerIcon, _referenceForOwnerIcon);
			_imageSpriteLoader.AddReferenceToSpriteForImage(ownerIconShine, _referenceForOwnerIconShine);
		}
	}

	protected void OnDisable()
	{
		_imageSpriteLoader.Release();
		if (!CoreApplication.IsQuitting)
		{
			warningCostAnimation.Stop();
			assignAnimation.Stop();
		}
	}
}
