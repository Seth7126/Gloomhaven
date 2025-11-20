using System;
using AsmodeeNet.Foundation;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.YML.Locations;
using SM.Gamepad;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using Script.GUI.SMNavigation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITempleShopSlot : MonoBehaviour
{
	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private TMP_Text blessName;

	[SerializeField]
	private TMP_Text blessNameWarning;

	[SerializeField]
	private Image blessIcon;

	[SerializeField]
	private RawImage backgroundImage;

	[SerializeField]
	private GUIAnimator warningAvailableAnimator;

	[SerializeField]
	private Color unavailableTextColor;

	[SerializeField]
	private string clickAudioItem;

	[SerializeField]
	private Hotkey hotkey;

	[SerializeField]
	private UINavigationSelectable _uiNavigationSelectable;

	[SerializeField]
	private UiNavigationRoot _uiNavigationRoot;

	[SerializeField]
	private bool _disableGrayBackground;

	[Header("Price")]
	[SerializeField]
	private TMP_Text priceText;

	[SerializeField]
	private Image goldIcon;

	[SerializeField]
	private TMP_Text priceTextWarning;

	[SerializeField]
	private GUIAnimator warningAffordableAnimator;

	private bool isAffordable;

	private bool isAvailable;

	private Action<bool, UITempleShopSlot> onHovered;

	private Action<UITempleShopSlot> onSelected;

	private Color nameColor;

	private SkipFrameKeyActionHandlerBlocker _skipFrameKeyActionHandlerBlocker;

	private bool _isHovered;

	public TempleYML.TempleBlessingDefinition Blessing { get; private set; }

	public bool IsAvailable => isAvailable;

	public Selectable Selectable => button;

	public bool IsHovered => _isHovered;

	private void Awake()
	{
		_skipFrameKeyActionHandlerBlocker = new SkipFrameKeyActionHandlerBlocker(this);
		nameColor = blessName.color;
		if (hotkey != null)
		{
			hotkey.Initialize(Singleton<UINavigation>.Instance.Input, null, null, activate: false);
		}
	}

	private void OnDestroy()
	{
		if (hotkey != null)
		{
			hotkey.Deinitialize();
		}
	}

	private void OnEnable()
	{
		SubscribeOnEvents();
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			DisableNavigation();
			ShowAffordableWarning(show: false);
			ShowAvailableWarning(show: false);
		}
		UnsubscribeOnEvents();
	}

	public void SetBlessing(TempleYML.TempleBlessingDefinition blessing, Action<UITempleShopSlot> onSelected, Action<bool, UITempleShopSlot> onHovered, ICharacter character = null, bool isAffordable = false, bool isAvailable = true)
	{
		Blessing = blessing;
		this.onHovered = onHovered;
		this.onSelected = onSelected;
		TMP_Text tMP_Text = priceText;
		string text = (priceTextWarning.text = blessing.GoldCost.ToString());
		tMP_Text.text = text;
		UIInfoTools.EffectInfo effectInfo = ((blessing.TempleBlessingCondition.Type != RewardCondition.EConditionType.Negative) ? UIInfoTools.Instance.GetEffectInfoCondition(blessing.TempleBlessingCondition.PositiveCondition) : UIInfoTools.Instance.GetEffectInfoCondition(blessing.TempleBlessingCondition.NegativeCondition));
		blessIcon.sprite = ((effectInfo.TempleIcon == null) ? effectInfo.Icon : effectInfo.TempleIcon);
		ShowAffordableWarning(show: false);
		ShowAvailableWarning(show: false);
		UpdateOwner(character);
		RefreshAffordable(isAffordable);
		RefreshAvailable(isAvailable);
	}

	public void UpdateOwner(ICharacter character)
	{
		string text = null;
		switch (Blessing.TempleBlessingCondition.Type)
		{
		case RewardCondition.EConditionType.Negative:
			text = LocalizationManager.GetTranslation(Blessing.TempleBlessingCondition.NegativeCondition.ToString()).ToTitleCase();
			break;
		case RewardCondition.EConditionType.Positive:
			text = LocalizationManager.GetTranslation(Blessing.TempleBlessingCondition.PositiveCondition.ToString()).ToTitleCase();
			break;
		}
		if (character != null)
		{
			TMP_Text tMP_Text = blessName;
			string text2 = (blessNameWarning.text = text + " " + LocalizationManager.GetTranslation(character.Class.LocKey));
			tMP_Text.text = text2;
			backgroundImage.texture = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(character.Class.Model, highlighted: true, character.Class.CustomCharacterConfig).texture;
			int controllableID = (AdventureState.MapState.IsCampaign ? character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(character.CharacterID));
			button.interactable = !FFSNetwork.IsOnline || ControllableRegistry.GetControllable(controllableID)?.NetworkEntity != null;
		}
		else
		{
			TMP_Text tMP_Text2 = blessName;
			string text2 = (blessNameWarning.text = text);
			tMP_Text2.text = text2;
			button.interactable = true;
		}
	}

	public void RefreshAffordable(bool isAffordable)
	{
		this.isAffordable = isAffordable;
		RefreshPriceState();
	}

	public void RefreshAvailable(bool isAvailable)
	{
		this.isAvailable = isAvailable;
		if (isAvailable)
		{
			Image image = blessIcon;
			Material material = (backgroundImage.material = null);
			image.material = material;
			blessName.color = nameColor;
		}
		else
		{
			Image image2 = blessIcon;
			Material material = (backgroundImage.material = UIInfoTools.Instance.greyedOutMaterial);
			image2.material = material;
			blessName.color = unavailableTextColor;
		}
		if (!isAvailable && _disableGrayBackground)
		{
			backgroundImage.gameObject.SetActive(value: false);
		}
		else
		{
			backgroundImage.gameObject.SetActive(value: true);
		}
		RefreshPriceState();
	}

	private void RefreshPriceState()
	{
		if (!isAvailable)
		{
			Image image = goldIcon;
			Color color = (priceText.color = unavailableTextColor);
			image.color = color;
		}
		else
		{
			TMP_Text tMP_Text = priceText;
			Color color = (goldIcon.color = (isAffordable ? UIInfoTools.Instance.goldColor : UIInfoTools.Instance.warningColor));
			tMP_Text.color = color;
		}
	}

	private void Click()
	{
		if (!LevelMessageUILayoutGroup.IsShown)
		{
			if (!isAvailable)
			{
				ShowAvailableWarning(show: true);
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
			}
			else if (!isAffordable)
			{
				ShowAvailableWarning(show: true);
				ShowAffordableWarning(show: true);
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
			}
			else
			{
				AudioControllerUtils.PlaySound(clickAudioItem);
			}
			onSelected?.Invoke(this);
		}
	}

	private void ShowAffordableWarning(bool show)
	{
		if (show)
		{
			warningAffordableAnimator.Play();
		}
		else
		{
			warningAffordableAnimator.Stop(goToEnd: true);
		}
	}

	private void ShowAvailableWarning(bool show)
	{
		if (show)
		{
			warningAvailableAnimator.Play();
		}
		else
		{
			warningAvailableAnimator.Stop(goToEnd: true);
		}
	}

	public void DisableNavigation()
	{
		button.DisableNavigation();
	}

	private void SubscribeOnEvents()
	{
		if (InputManager.GamePadInUse)
		{
			SubscribeOnEventsGamepad();
		}
		else
		{
			SubscribeOnEventsKeyboardAndMouse();
		}
	}

	private void UnsubscribeOnEvents()
	{
		if (InputManager.GamePadInUse)
		{
			UnsubscribeOnEventsGamepad();
		}
		else
		{
			UnsubscribeOnEventsKeyboardAndMouse();
		}
	}

	private void SubscribeOnEventsKeyboardAndMouse()
	{
		if (!InputManager.GamePadInUse)
		{
			button.onClick.AddListener(Click);
			button.onMouseEnter.AddListener(Select);
			button.onMouseExit.AddListener(Deselect);
		}
	}

	private void UnsubscribeOnEventsKeyboardAndMouse()
	{
		if (!InputManager.GamePadInUse)
		{
			button.onClick.RemoveListener(Click);
			button.onMouseEnter.RemoveListener(Select);
			button.onMouseExit.RemoveListener(Deselect);
		}
	}

	private void SubscribeOnEventsGamepad()
	{
		if (InputManager.GamePadInUse)
		{
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, Click).AddBlocker(new NavigationSelectableSelectKeyActionHandlerBlocker(_uiNavigationSelectable)).AddBlocker(_skipFrameKeyActionHandlerBlocker));
			_uiNavigationRoot.OnRootElementEnabledEvent += OnRootEnabled;
			_uiNavigationSelectable.OnNavigationSelectedEvent += OnGamepadSelected;
			_uiNavigationSelectable.OnNavigationDeselectedEvent += OnGamepadDeselected;
		}
	}

	private void UnsubscribeOnEventsGamepad()
	{
		if (InputManager.GamePadInUse)
		{
			if (Singleton<KeyActionHandlerController>.Instance != null)
			{
				Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, Click);
			}
			_uiNavigationRoot.OnRootElementEnabledEvent -= OnRootEnabled;
			_uiNavigationSelectable.OnNavigationSelectedEvent -= OnGamepadSelected;
			_uiNavigationSelectable.OnNavigationDeselectedEvent -= OnGamepadDeselected;
		}
	}

	private void OnRootEnabled(IUiNavigationRoot obj)
	{
		_skipFrameKeyActionHandlerBlocker.Run();
	}

	private void OnGamepadSelected(IUiNavigationSelectable uiNavigationSelectable)
	{
		Select();
	}

	private void OnGamepadDeselected(IUiNavigationSelectable uiNavigationSelectable)
	{
		Deselect();
	}

	private void Select()
	{
		if (hotkey != null)
		{
			hotkey.DisplayHotkey(isAvailable);
		}
		_isHovered = true;
		onHovered?.Invoke(arg1: true, this);
	}

	private void Deselect()
	{
		if (hotkey != null)
		{
			hotkey.DisplayHotkey(active: false);
		}
		_isHovered = false;
		onHovered?.Invoke(arg1: false, this);
	}
}
