using System;
using System.Linq;
using System.Text;
using AsmodeeNet.Foundation;
using FFSNet;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINewEnhancementShopSlot : MonoBehaviour
{
	[SerializeField]
	private TMP_Text itemName;

	[SerializeField]
	private TMP_Text enhancedText;

	[SerializeField]
	private Image itemIcon;

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private RectTransform tooltipHolder;

	[SerializeField]
	private RawImage background;

	[SerializeField]
	private GameObject _hoverImage;

	[Header("Price")]
	[SerializeField]
	protected TMP_Text itemPrice;

	[SerializeField]
	private GUIAnimator priceWarning;

	[SerializeField]
	private Image goldIcon;

	[SerializeField]
	private TextMeshProUGUI priceTextWarning;

	[Header("Points")]
	[SerializeField]
	private TextMeshProUGUI enhancementPoints;

	[SerializeField]
	private Image enhancementPointsIcon;

	[SerializeField]
	private GUIAnimator pointsWarning;

	[SerializeField]
	private Material _enhancedMaterial;

	private bool enoughPoints;

	private Action<bool, EnhancementSlot> onHovered;

	protected Action<EnhancementSlot> onSelect;

	protected EnhancementSlot enhancement;

	private Action<bool, RectTransform> onShowPrice;

	protected bool isAffordable;

	private ICharacter character;

	private bool _showEnhanced;

	private void Awake()
	{
		button.onClick.AddListener(Select);
		button.onMouseEnter.AddListener(OnHover);
		button.onMouseExit.AddListener(OnUnhover);
	}

	private void OnDestroy()
	{
		button.onClick.RemoveAllListeners();
	}

	public void Initialize(EnhancementSlot enhancement, ICharacter character, int price, string suffix, Action<bool, EnhancementSlot> onHovered, Action<EnhancementSlot> onSelect, Action<bool, RectTransform> onShowPrice, bool isAffordable, int points, bool enoughPoints)
	{
		priceWarning.Stop(goToEnd: true);
		pointsWarning.Stop(goToEnd: true);
		this.enhancement = enhancement;
		this.onHovered = onHovered;
		this.onSelect = onSelect;
		this.onShowPrice = onShowPrice;
		this.character = character;
		UpdateAffordable(isAffordable);
		TMP_Text tMP_Text = itemPrice;
		string text = (priceTextWarning.text = price.ToString());
		tMP_Text.text = text;
		background.texture = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(character.Class.Model, highlighted: true, character.Class.CustomCharacterConfig).texture;
		_hoverImage.gameObject.SetActive(value: false);
		_showEnhanced = enhancement.IsCurrentEnhanced && InputManager.GamePadInUse;
		UpdateBackground(hover: false);
		StringBuilder stringBuilder = new StringBuilder(LocalizationManager.GetTranslation($"ENHANCEMENT_{enhancement.enhancement}"));
		int num = enhancement.button.EnhancementSlot + 1;
		if (suffix.IsNOTNullOrEmpty() && !_showEnhanced)
		{
			stringBuilder.AppendFormat(" ({0})", num);
		}
		itemName.text = stringBuilder.ToString();
		itemIcon.sprite = UIInfoTools.Instance.GetEnhancementIcon(enhancement.enhancement);
		UpdatePoints(points, enoughPoints);
		DetermineInteractability();
		if (!InputManager.GamePadInUse)
		{
			return;
		}
		if (_showEnhanced)
		{
			background.texture = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(character.Class.Model, highlighted: true, character.Class.CustomCharacterConfig).texture;
			stringBuilder = new StringBuilder(LocalizationManager.GetTranslation("CONSOLES/GUI_ENHANCED"));
			if (suffix.IsNOTNullOrEmpty())
			{
				stringBuilder.AppendFormat(" ({0})", num);
			}
			enhancedText.text = stringBuilder.ToString();
		}
		enhancedText.gameObject.SetActive(_showEnhanced);
	}

	private void UpdateBackground(bool hover)
	{
		Material material = ((_showEnhanced && !hover) ? _enhancedMaterial : null);
		background.material = material;
		background.enabled = (hover && (enhancement.IsEmpty || enhancement.IsCurrentEnhanced)) || _showEnhanced;
	}

	public void UpdateAffordable(bool isAffordable)
	{
		this.isAffordable = isAffordable;
		TMP_Text tMP_Text = itemPrice;
		Color color = (goldIcon.color = (isAffordable ? UIInfoTools.Instance.mainColor : UIInfoTools.Instance.redDifficultyColor));
		tMP_Text.color = color;
	}

	public void OnHover()
	{
		if (button.IsSelected)
		{
			onHovered?.Invoke(arg1: true, enhancement);
			_hoverImage.gameObject.SetActive(value: true);
			UpdateBackground(hover: true);
		}
	}

	private void OnUnhover()
	{
		onHovered?.Invoke(arg1: false, enhancement);
		_hoverImage.gameObject.SetActive(value: false);
		UpdateBackground(hover: false);
	}

	public void ShowTooltipPrice(bool show)
	{
		onShowPrice(show, tooltipHolder);
	}

	public void Select()
	{
		priceWarning.Stop(goToEnd: true);
		pointsWarning.Stop(goToEnd: true);
		if (!isAffordable)
		{
			priceWarning.Play(fromStart: true);
		}
		if (!enoughPoints)
		{
			pointsWarning.Play(fromStart: true);
		}
		AudioControllerUtils.PlaySound((!isAffordable || !enoughPoints) ? UIInfoTools.Instance.InvalidOptionAudioItem : UIInfoTools.Instance.generalAudioButtonProfile.mouseDownAudioItem);
		onSelect?.Invoke(enhancement);
		_hoverImage.gameObject.SetActive(value: true);
	}

	public void UpdatePoints(int points, bool canAffordPoints)
	{
		enoughPoints = canAffordPoints;
		if (points == 0)
		{
			enhancementPointsIcon.gameObject.SetActive(value: false);
			enhancementPoints.gameObject.SetActive(value: false);
			return;
		}
		enhancementPoints.text = points.ToString();
		Image image = enhancementPointsIcon;
		Color color = (enhancementPoints.color = (canAffordPoints ? UIInfoTools.Instance.buyEnhancementColor : UIInfoTools.Instance.warningColor));
		image.color = color;
		enhancementPointsIcon.gameObject.SetActive(value: true);
		enhancementPoints.gameObject.SetActive(value: true);
	}

	public void DetermineInteractability()
	{
		CCharacterClass cCharacterClass = CharacterClassManager.Classes.FirstOrDefault((CCharacterClass x) => x.FindCardWithID(enhancement.button.AbilityCardID) != null);
		int controllableID = ((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? character.CharacterName.GetHashCode() : cCharacterClass.ModelInstanceID);
		button.interactable = !FFSNetwork.IsOnline || (cCharacterClass != null && PlayerRegistry.MyPlayer.HasControlOver(controllableID) && ControllableRegistry.GetControllable(controllableID)?.NetworkEntity != null);
	}

	public void EnableNavigation()
	{
		button.SetNavigation(Navigation.Mode.Vertical);
	}

	public void DisableNavigation()
	{
		button.DisableNavigation();
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			DisableNavigation();
		}
	}
}
